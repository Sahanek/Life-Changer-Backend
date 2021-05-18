using API.Dtos;
using API.Errors;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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
using System.Text.Json;
using System.Text.Json.Serialization;
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

        /// <summary>
        /// Login user via external service like google. Returns UserDto with Token needed for Authorization.
        /// </summary>
        /// <param name="externalAuth"> Dto contains provider e.g. Google and tokenId from returned that service </param>
        /// <returns></returns>
        [HttpPost("ExternalLogin")]
        public async Task<ActionResult<UserDto>> ExternalLogin([FromBody] ExternalAuthDto externalAuth)
        {
            var payload = await _googleVerification.VerifyGoogleToken(externalAuth.Token);
            if (payload == null)
                return BadRequest("Invalid External Authentication.");

            var info = new UserLoginInfo("Google", payload.Subject, externalAuth.Provider);

            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new AppUser { Email = payload.Email, UserName = payload.GivenName };
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

            return new UserDto
            {
                Email = user.Email,
                Token = _tokenService.CreateToken(user),
                UserName = user.UserName
            };
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("{email}")]
        public async Task<ActionResult<UserDto>> UserForTesting(string email)
        {
            //var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);
            return new UserDto
            {
                Email = user.Email,
                Token = _tokenService.CreateToken(user),
                UserName = user.UserName
            };
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

        /// <summary>
        /// Return CalendarId for current logged in user.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("calendar")]
        public async Task<ActionResult<CalendarDto>> GetCalendarId()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            return new CalendarDto { Token = user.CalendarId };
        }


        /// <summary>
        /// Update CalendarId selected for saving events.
        /// </summary>
        /// <param name = "calendar"> Method replace CalendarId for current logged in user with CalendarId passed in body. </param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("updatecalendar")]
        public async Task<ActionResult<UserDto>> UpdateCalendarId(CalendarDto calendar)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            user.CalendarId = calendar.Token;

            await _userManager.UpdateAsync(user);

            return new UserDto
            {
                Email = user.Email,
                Token = _tokenService.CreateToken(user),
                UserName = user.UserName
            };
        }

        //[ApiExplorerSettings(IgnoreApi = true)]
        //[Route("gooogle-repose")]
        //public async Task<IActionResult> GoogleResponse()
        //{
        //    var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        //    var claims = result.Principal.Identities.FirstOrDefault()
        //        .Claims.Select(claim => new
        //        {
        //            claim.Issuer,
        //            claim.OriginalIssuer,
        //            claim.Type,
        //            claim.Value
        //        });


        //    return Content(JsonSerializer.Serialize(claims));
        //}

        //[ApiExplorerSettings(IgnoreApi = true)]
        //[Route("google-login")]
        //public IActionResult SignInWithGoogle()
        //{
        //    var authenticationProperties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, Url.Action(nameof(HandleExternalLogin)));
        //    authenticationProperties.AllowRefresh = true;
        //    return Challenge(authenticationProperties, "Google");
        //}

        //[ApiExplorerSettings(IgnoreApi = true)]
        //[Route("google-callback")]
        //public async Task<IActionResult> HandleExternalLogin()
        //{
        //    var info = await _signInManager.GetExternalLoginInfoAsync();
        //    var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

        //    if (!result.Succeeded) //user does not exist yet
        //    {
        //        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        //        var newUser = new AppUser
        //        {
        //            UserName = email,
        //            Email = email,
        //            EmailConfirmed = true
        //        };
        //        var createResult = await _userManager.CreateAsync(newUser);
        //        if (!createResult.Succeeded)
        //            throw new Exception(createResult.Errors.Select(e => e.Description).Aggregate((errors, error) => $"{errors}, {error}"));

        //        await _userManager.AddLoginAsync(newUser, info);
        //        var newUserClaims = info.Principal.Claims.Append(new Claim("userId", newUser.Id));
        //        await _userManager.AddClaimsAsync(newUser, newUserClaims);
        //        await _signInManager.SignInAsync(newUser, isPersistent: false);
        //        //await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        //    }

        //    return Content("Great");
        //}

        //[ApiExplorerSettings(IgnoreApi = true)]
        //public async Task<IActionResult> Logout()
        //{
        //    await _signInManager.SignOutAsync();
        //    return Redirect("http://localhost:4200");
        //}



        //[Authorize]
        //[HttpGet]
        //public async Task<ActionResult<UserDto>> GetCurrentUser()
        //{
        //    var email = User.FindFirstValue(ClaimTypes.Email);

        //    var user = await _userManager.FindByEmailAsync(email);
        //    return new UserDto
        //    {
        //        Email = user.Email,
        //        Token = _tokenService.CreateToken(user),
        //        UserName = user.UserName
        //    };

        //}

        //[Authorize]
        //[HttpGet("userinfo")]
        //public async Task<ActionResult<UserInformationDto>> GetCurrentUserInformation()
        //{
        //    var email = User.FindFirstValue(ClaimTypes.Email);

        //    var user = await _userManager.FindByEmailAsync(email);
        //    return new UserInformationDto
        //    {
        //        Email = user.Email,
        //        UserName = user.UserName,
        //        Gender = user.Gender.ToString(),
        //        BirthDate = user.BirthDate.ToShortDateString(),
        //        PhoneNumber = user.PhoneNumber,
        //    };

        //}

        //[HttpGet("emailexists")]
        //[SwaggerOperation(Summary = "e.g. localhost:5001/api/account/emailexists?email=greg@test.com")]
        //public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
        //{
        //    return await _userManager.FindByEmailAsync(email) is not null;
        //}

        //[HttpGet("userexists")]
        //[SwaggerOperation(Summary = "e.g. localhost:5001/api/account/userexists?username=Nika")]
        //public async Task<ActionResult<bool>> CheckUserNameExistsAsync([FromQuery] string userName)
        //{
        //    return await _userManager.FindByNameAsync(userName) is not null;
        //}


        //[HttpPost("login")]
        //public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        //{
        //    var user = await _userManager.FindByEmailAsync(loginDto.Email);

        //    if (user is null) return NotFound(new ErrorDetails(404, "This user doesn't exist."));

        //    var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

        //    if (!emailConfirmed) return Unauthorized(new ErrorDetails(401, "Confirm your email please."));

        //    var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        //    if (!result.Succeeded) return Unauthorized(new ErrorDetails(401, "Wrong password."));

        //    return new UserDto
        //    {
        //        Email = user.Email,
        //        Token = _tokenService.CreateToken(user),
        //        UserName = user.UserName
        //    };
        //}

        //[HttpPost("register")]
        //public async Task<ActionResult<bool>> Register(RegisterDto registerDto)
        //{
        //    if (CheckEmailExistsAsync(registerDto.Email).Result.Value)
        //    {
        //        return BadRequest(new ErrorDetails(400, "Email is in use."));
        //    }

        //    var user = new AppUser
        //    {
        //        UserName = registerDto.UserName,
        //        Email = registerDto.Email,
        //        Gender = (Gender)Enum.Parse(typeof(Gender), registerDto.Gender, true), 
        //        PhoneNumber = registerDto.PhoneNumber,
        //        BirthDate = DateTime.Parse(registerDto.BirthDate),
        //    };

        //    var result = await _userManager.CreateAsync(user, registerDto.Password);

        //    if (!result.Succeeded) return BadRequest(new ErrorDetails(400, "Nie udało się zarejestrować."));

        //    var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //    var confirmationLink = Url.Action("ConfirmEmail", "Email", new { confirmationToken, email = user.Email }, Request.Scheme);
        //    try
        //    {
        //        await EmailHelper.SendConfirmationMail(user.Email, confirmationLink);
        //    }
        //    catch
        //    {
        //        return BadRequest("Problem with sending Email.");
        //    }

        //    return Ok();
        //}

        //[Authorize]
        //[HttpPut("changeemail")]
        //public async Task<ActionResult> ChangeEmail([FromBody] EmailDto newEmail)
        //{
        //    if (CheckEmailExistsAsync(newEmail.NewEmail).Result.Value)
        //    {
        //        return BadRequest(new ErrorDetails(400, "Email is in use."));
        //    }

        //    var email = User.FindFirstValue(ClaimTypes.Email);

        //    var user = await _userManager.FindByEmailAsync(email);

        //    var changeToken = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail.NewEmail);

        //    var confirmationLink = Url.Action("ChangeEmail", "Email", new { changeToken, email = user.Email, newEmail = newEmail.NewEmail }, Request.Scheme);
        //    try
        //    {
        //        await EmailHelper.SendNewEmailConfirmationMail(user.Email, confirmationLink);
        //    }
        //    catch
        //    {
        //        return BadRequest("Problem with sending Email.");
        //    }

        //    return NoContent();
        //}

        //[Authorize]
        //[HttpPut("changepassword")]
        //public async Task<ActionResult<UserDto>> ChangePassword([FromBody] PasswordDto passwords)
        //{
        //    var email = User.FindFirstValue(ClaimTypes.Email);

        //    var user = await _userManager.FindByEmailAsync(email);

        //    var checkPasswordResult = await _signInManager.CheckPasswordSignInAsync(user, passwords.OldPassword, false);

        //    if (!checkPasswordResult.Succeeded) return BadRequest(new ErrorDetails(400, "Wrong Password!"));

        //    var deletePasswordResult = await _userManager.RemovePasswordAsync(user);

        //    if (!deletePasswordResult.Succeeded) return BadRequest();

        //    var addPasswordResult = await _userManager.AddPasswordAsync(user, passwords.NewPassword);

        //    if (!addPasswordResult.Succeeded) return BadRequest();

        //    await EmailHelper.SendPasswordChangedMail(email);

        //    return new UserDto
        //    {
        //        Email = user.Email,
        //        Token = _tokenService.CreateToken(user),
        //        UserName = user.UserName
        //    };
        //}

        //[Authorize]
        //[HttpPatch("updateuser")]
        //public async Task<ActionResult> UpdatePartOfUser([FromBody] JsonPatchDocument<UserPatchDto> patchDoc)
        //{
        //    if (patchDoc is null) return BadRequest();

        //    var email = User.FindFirstValue(ClaimTypes.Email);

        //    var user = await _userManager.FindByEmailAsync(email);

        //    _mapper.Map<JsonPatchDocument<UserPatchDto>, JsonPatchDocument<AppUser>>(patchDoc).ApplyTo(user, ModelState);

        //    var isValid = TryValidateModel(user);
        //    //Console.WriteLine(user.Gender);
        //    if (!isValid) return BadRequest(ModelState);

        //    var result = await _userManager.UpdateAsync(user);

        //    if (!result.Succeeded) return BadRequest(result.Errors);

        //    return NoContent();
        //}

        //[Authorize]
        //[HttpDelete]
        //public async Task<ActionResult> DeleteUser()
        //{
        //    var email = User.FindFirstValue(ClaimTypes.Email);
        //    var user = await _userManager.FindByEmailAsync(email);
        //    var result = await _userManager.DeleteAsync(user);
        //    if (!result.Succeeded) return BadRequest(result.Errors);
        //    return NoContent();
        //}

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("testauth")]
        [Authorize]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }

    }
}
