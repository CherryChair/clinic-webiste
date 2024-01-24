using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MvcClinic.Data;
using MvcClinic.Models;

namespace MvcClinic.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly MvcClinicContext _context;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IConfiguration _configuration;

        public LoginController(MvcClinicContext context, UserManager<UserAccount> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("/login")]

        public async Task<ActionResult<Patient>> Login([FromBody] LoginModel model)
        {
            if (model.Email == null || model.Password == null)
            {
                return Unauthorized();
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound();
            }
            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userClaims = await _userManager.GetClaimsAsync(user);
                var tokenClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, model.Email),
                };
                tokenClaims.AddRange(userClaims);
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: tokenClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }

            return Unauthorized();
        }

    }
}
