using API.Dtos;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user is null) return Unauthorized(); // Give Error response class to Unauthorized when created with code 401

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized(); //again..

            return new UserDto
            {
                Email = user.Email,
                Token = "This will be a token ;)",
                FirstName = user.FirstName
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            var user = new AppUser
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                Sex = registerDto.Sex, //fix enums later
                PhoneNumber = registerDto.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest();// Someday sth better info

            return new UserDto
            {
                FirstName = user.FirstName,
                Token = "This will be a token",
                Email = user.Email
            };
        }
    }
}
