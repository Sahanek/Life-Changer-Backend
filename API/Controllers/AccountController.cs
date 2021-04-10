using API.Dtos;
using API.Errors;
using API.Helpers;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);
            return new UserDto
            {
                Email = user.Email,
                Token = _tokenService.CreateToken(user),
                UserName = user.UserName
            };

        }

        [Authorize]
        [HttpGet("userinfo")]
        public async Task<ActionResult<UserInformationDto>> GetCurrentUserInformation()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);
            return new UserInformationDto
            {
                Email = user.Email,
                UserName = user.UserName,
                Gender = user.Gender.ToString(),
                BirthDate = user.BirthDate.ToShortDateString(),
                PhoneNumber = user.PhoneNumber,
            };

        }

        [Authorize]
        [HttpPut("changepassword")]
        public async Task<ActionResult<UserDto>> ChangePassword([FromBody] PasswordDto passwords)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var checkPasswordResult = await _signInManager.CheckPasswordSignInAsync(user, passwords.OldPassword, false);

            if (!checkPasswordResult.Succeeded) return BadRequest(new ErrorDetails(400, "Wrong Password!"));

            var deletePasswordResult = await _userManager.RemovePasswordAsync(user);

            if (!deletePasswordResult.Succeeded) return BadRequest();

            var addPasswordResult = await _userManager.AddPasswordAsync(user, passwords.NewPassword);

            if (!addPasswordResult.Succeeded) return BadRequest();

            await EmailHelper.SendPasswordChangedMail(email);

            return new UserDto
            {
                Email = user.Email,
                Token = _tokenService.CreateToken(user),
                UserName = user.UserName
            };
        }

        [HttpGet("emailexists")]
        [SwaggerOperation(Summary = "e.g. localhost:5001/api/account/emailexists?email=greg@test.com")]
        public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }

        [HttpGet("userexists")]
        [SwaggerOperation(Summary = "e.g. localhost:5001/api/account/userexists?username=Nika")]
        public async Task<ActionResult<bool>> CheckUserNameExistsAsync([FromQuery] string userName)
        {
            return await _userManager.FindByNameAsync(userName) is not null;
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user is null) return NotFound(new ErrorDetails(404, "Nie istnieje taki użytkownik."));

            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            if (!emailConfirmed) return Unauthorized(new ErrorDetails(401, "Potwierdź swój email."));

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized(new ErrorDetails(401, "Wprowadzono złe hasło."));

            return new UserDto
            {
                Email = user.Email,
                Token = _tokenService.CreateToken(user),
                UserName = user.UserName
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult<bool>> Register(RegisterDto registerDto)
        {
            if (CheckEmailExistsAsync(registerDto.Email).Result.Value)
            {
                return BadRequest(new ErrorDetails(400, "Email jest zajęty."));
            }

            var user = new AppUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Gender = (Gender)Enum.Parse(typeof(Gender), registerDto.Gender, true), 
                PhoneNumber = registerDto.PhoneNumber,
                BirthDate = DateTime.Parse(registerDto.BirthDate),
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(new ErrorDetails(400, "Nie udało się zarejestrować. Spróbuj jeszcze raz"));

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Email", new { confirmationToken, email = user.Email }, Request.Scheme);
            try
            {
                await EmailHelper.SendConfirmationMail(user.Email, confirmationLink);
            }
            catch
            {
                return BadRequest("Problem with sending Email. Please try it via login screen");
            }

            return Ok();
        }

        [HttpGet("testauth")]
        [Authorize]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }

    }
}
