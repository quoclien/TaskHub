using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevelopmentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DevelopmentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generates a JWT for a test user. FOR DEVELOPMENT USE ONLY.
        /// </summary>
        /// <param name="userId">Optional user ID to include in the token. If not provided, a default GUID will be used.</param>
        /// <returns>A JWT string.</returns>
        [HttpGet("generate-token")]
        [ApiExplorerSettings(IgnoreApi = false)] // Ensure this is visible in Swagger
        public IActionResult GenerateToken([FromQuery] Guid? userId)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured")));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, (userId ?? Guid.NewGuid()).ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, (userId ?? Guid.NewGuid()).ToString()) // Set the user's ID in the Name claim
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(Convert.ToDouble(jwtSettings["ExpireDays"])),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return Ok(new { token = tokenHandler.WriteToken(token) });
        }
    }
}
