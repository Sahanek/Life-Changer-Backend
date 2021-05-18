//using API.Dtos;
//using API.Errors;
//using API.Helpers;
//using Core.Entities;
//using Core.Interfaces;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace API.Controllers
//{
//    public class EmailController : BaseApiController
//    {
//        private readonly UserManager<AppUser> _userManager;

//        public EmailController(UserManager<AppUser> userManager)
//        {
//            _userManager = userManager;
//        }

//        [HttpGet]
//        public async Task<ActionResult> ConfirmEmail([FromQuery] string token, [FromQuery] string email)
//        {
//            var user = await _userManager.FindByEmailAsync(email);

//            if (user is null) return NotFound(new ErrorDetails(404, "This user does not exist."));

//            var result = await _userManager.ConfirmEmailAsync(user, token);

//            if (!result.Succeeded) return BadRequest(new ErrorDetails(400, "The email could not be confirmed."));

//            return Ok(); 
//        }

//        [HttpGet("changeemail")]
//        public async Task<ActionResult> ChangeEmail([FromQuery] string token, [FromQuery] string email, [FromQuery] string newEmail )
//        {
//            var user = await _userManager.FindByEmailAsync(email);

//            if (user is null) return NotFound(new ErrorDetails(404, "This user does not exist."));

//            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

//            if (!result.Succeeded) return BadRequest(new ErrorDetails(400, "The email could not be confirmed."));

//            return Ok();
//        }

//        [HttpPost("confirm")]
//        public async Task<ActionResult> GenerateConfirmEmail([FromQuery] string email)
//        {
//            var user = await _userManager.FindByEmailAsync(email);

//            if (user is null) return NotFound(new ErrorDetails(404, "This user does not exist."));

//            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

//            if (emailConfirmed) return BadRequest(new ErrorDetails(400, "This email is confirmed." ));

//            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
//            var confirmationLink = Url.Action("ConfirmEmail", "Email", new { confirmationToken, user.Email }, Request.Scheme, "localhost:4200");
//            try
//            {
//                await EmailHelper.SendConfirmationMail(user.Email, confirmationLink);
//            }
//            catch
//            {
//                return BadRequest(new ErrorDetails(400, "Problem with sending Email. Please try it via login screen"));
//            }

//            //await _userManager.ConfirmEmailAsync(user, confirmationToken); //using for manually confirmation
//            return Ok();
//        }
//    }
//}
