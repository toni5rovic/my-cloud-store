using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyCloudStore.Service.DataLayer;
using MyCloudStore.Service.DataLayer.Models;
using MyCloudStore.SharedModels;
using MyCloudStore.SharedModels.Results;

namespace MyCloudStore.Service.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly IConfiguration configuration;
		private readonly UserManager<AppUser> userManager;
		private readonly SignInManager<AppUser> signInManager;
		private readonly AppDbContext context;

		public UsersController(AppDbContext context,
			UserManager<AppUser> userManager,
			SignInManager<AppUser> signInManager,
			IConfiguration configuration)
		{
			this.context = context;
			this.userManager = userManager;
			this.signInManager = signInManager;
			this.configuration = configuration;
		}

		[HttpPost]
		[Route("register")]
		public async Task<IActionResult> Register([FromBody]RegisterModel model)
		{
			var newUser = new AppUser { UserName = model.Email, Email = model.Email };

			var result = await this.userManager.CreateAsync(newUser, model.Password);

			if (!result.Succeeded)
			{
				var errors = result.Errors.Select(x => x.Description);

				return BadRequest(new { Successful = false, Errors = errors });

			}

			return Ok(new { Successful = true });
		}

		[HttpPost]
		[Route("login")]
		public async Task<IActionResult> LogIn([FromBody]LogInModel model)
		{
			var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

			if (!result.Succeeded) return BadRequest(new LogInResult { Successful = false, Error = "Username and password are invalid." });

			var claims = new[]
			{
				new Claim(ClaimTypes.Name, model.Email)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSecurityKey"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expiry = DateTime.Now.AddDays(Convert.ToInt32(configuration["JwtExpiryInDays"]));

			var token = new JwtSecurityToken(
				configuration["JwtIssuer"],
				configuration["JwtAudience"],
				claims,
				expires: expiry,
				signingCredentials: creds
			);

			return Ok(new LogInResult { Successful = true, Token = new JwtSecurityTokenHandler().WriteToken(token) });
		}

		[HttpGet]
		[Route("logout")]
		public IActionResult LogOut()
		{
			var result = HttpContext.User.Claims == null ? true : false;
			if (result)
			{
				return Ok();
			}
			else
			{
				return BadRequest();
			}
		}
	}
}
