using API.Dtos;
using API.Errors;
using API.Helpers;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class EmailController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;

        public EmailController(UserManager<AppUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult> ConfirmEmail([FromQuery] string token, [FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null) return BadRequest(new ErrorDetails(400, "Nie ma takiego użytkownika."));

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded) return BadRequest(new ErrorDetails(400, "Nie udało się potwierdzić maila"));

            return null; //Or maybe user with token?
        }

        [HttpPost("confirm")]
        public async Task<ActionResult> GenerateConfirmEmail(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user is null) return Unauthorized(new ErrorDetails(401, "Wprowadzono zły email!"));

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (result) return Unauthorized(new ErrorDetails(401, "Wprowadzono złe hasło!"));

            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            if (emailConfirmed) return Unauthorized(new ErrorDetails(401, "Email jest już potwierdzony, zaloguj się."));

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Email", new { confirmationToken, email = user.Email }, Request.Scheme);
            EmailHelper emailHelper = new();
            bool emailResponse = await emailHelper.SendConfirmationMail(user.Email, confirmationLink);

            if (emailResponse) return BadRequest();

            return Ok();
        }
    }
}
