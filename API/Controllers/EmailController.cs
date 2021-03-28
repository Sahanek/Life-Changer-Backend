﻿using API.Dtos;
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

        public EmailController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult> ConfirmEmail([FromQuery] string token, [FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null) return NotFound(new ErrorDetails(404, "Nie ma takiego użytkownika."));

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded) return BadRequest(new ErrorDetails(400, "Nie udało się potwierdzić maila"));

            return Ok(); 
        }

        [HttpPost("confirm")]
        public async Task<ActionResult> GenerateConfirmEmail(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user is null) return BadRequest(new ErrorDetails(401, "Nie ma takiego użytkownika."));

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result) return Unauthorized(new ErrorDetails(401, "Wprowadzono złe hasło!"));

            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            if (emailConfirmed) return BadRequest();

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Email", new { confirmationToken, email = user.Email }, Request.Scheme, "localhost:4200");
            bool emailResponse = await EmailHelper.SendConfirmationMail(user.Email, confirmationLink);

            if (!emailResponse) return BadRequest();

            //await _userManager.ConfirmEmailAsync(user, confirmationToken); //using for manually confirmation
            return Ok();
        }
    }
}