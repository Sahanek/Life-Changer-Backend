﻿using API.Dtos;
using API.Errors;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
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
        private readonly IMapper _mapper;
        private readonly GoogleVerification _googleVerification;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            ITokenService tokenService, IMapper mapper, GoogleVerification googleVerification)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _googleVerification = googleVerification;
        }


        [HttpPost("ExternalLogin")]
        public async Task<IActionResult> ExternalLogin([FromBody]ExternalAuthDto externalAuth)
        {
            var payload = await _googleVerification.VerifyGoogleToken(externalAuth.Token);
            if (payload == null)
                return BadRequest("Invalid External Authentication.");

            var info = new UserLoginInfo("Google", payload.Subject, "GOOGLE" );

            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new AppUser { Email = payload.Email, UserName = payload.Email };
                    await _userManager.CreateAsync(user);
                    await _userManager.AddLoginAsync(user, info);
                }
                else
                {
                    await _userManager.AddLoginAsync(user, info);
                }
            }

            if (user == null)
                return BadRequest("Invalid External Authentication.");

            //check for the Locked out account

            var token =_tokenService.CreateToken(user);
            return Ok(token);
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

            if (user is null) return NotFound(new ErrorDetails(404, "This user doesn't exist."));

            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            if (!emailConfirmed) return Unauthorized(new ErrorDetails(401, "Confirm your email please."));

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized(new ErrorDetails(401, "Wrong password."));

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
                return BadRequest(new ErrorDetails(400, "Email is in use."));
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

            if (!result.Succeeded) return BadRequest(new ErrorDetails(400, "Nie udało się zarejestrować."));

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Email", new { confirmationToken, email = user.Email }, Request.Scheme);
            try
            {
                await EmailHelper.SendConfirmationMail(user.Email, confirmationLink);
            }
            catch
            {
                return BadRequest("Problem with sending Email.");
            }

            return Ok();
        }

        [Authorize]
        [HttpPut("changeemail")]
        public async Task<ActionResult> ChangeEmail([FromBody] EmailDto newEmail)
        {
            if (CheckEmailExistsAsync(newEmail.NewEmail).Result.Value)
            {
                return BadRequest(new ErrorDetails(400, "Email is in use."));
            }

            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            var changeToken = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail.NewEmail);

            var confirmationLink = Url.Action("ChangeEmail", "Email", new { changeToken, email = user.Email, newEmail = newEmail.NewEmail }, Request.Scheme);
            try
            {
                await EmailHelper.SendNewEmailConfirmationMail(user.Email, confirmationLink);
            }
            catch
            {
                return BadRequest("Problem with sending Email.");
            }

            return NoContent();
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

        [Authorize]
        [HttpPatch("updateuser")]
        public async Task<ActionResult> UpdatePartOfUser([FromBody] JsonPatchDocument<UserPatchDto> patchDoc)
        {
            if (patchDoc is null) return BadRequest();

            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            _mapper.Map<JsonPatchDocument<UserPatchDto>, JsonPatchDocument<AppUser>>(patchDoc).ApplyTo(user, ModelState);

            var isValid = TryValidateModel(user);
            //Console.WriteLine(user.Gender);
            if (!isValid) return BadRequest(ModelState);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> DeleteUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return NoContent();
        }


        [HttpGet("testauth")]
        [Authorize]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }

    }
}
