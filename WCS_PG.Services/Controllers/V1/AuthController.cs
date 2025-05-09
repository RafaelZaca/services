using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WCS_PG.Data;
using WCS_PG.Data.Models;
using WCS_PG.Services.Models.request.V1;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly WCSContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ILogger<AuthController> logger, WCSContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto LoginModel)
        {
            try
            {
                var databaseName = _context.Database.GetDbConnection().Database;
                User? _user = await _context.Users.Where(srch => srch.Username == LoginModel.UserName).FirstOrDefaultAsync();
                if (_user == null)
                {
                    return BadRequest("Usuário inválido");
                }
                if (_user.PasswordHash != LoginModel.Password)
                {
                    return BadRequest("Senha inválida");
                }

                var key = Encoding.ASCII.GetBytes(_configuration.GetSection("Encryption:Secret").Value!);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Name),
                new Claim(JwtRegisteredClaimNames.Name, _user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim("ID", _user.Id.ToString()),
                new Claim("IP", HttpContext.Connection.RemoteIpAddress.ToString())
             }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials
                    (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var stringToken = tokenHandler.WriteToken(token);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };
                return Ok(new { token = stringToken });

            }
            catch (Exception _ex)
            {
                return StatusCode(500, _ex.Message);
            }
        }


        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [HttpPut]
        public async Task<IActionResult> Logout()
        {
            try
            {
                Int32 _idusuario = Convert.ToInt32((from c in HttpContext.User.Claims where c.Type == "ID" select c.Value).FirstOrDefault());
                User _user = (await _context.Users.Where(_ => _.Id == _idusuario).FirstOrDefaultAsync())!;
                if(_user != null)
                {
                _user.LastLogin = DateTime.Now;
                await _context.SaveChangesAsync();
                }
                return Ok();
            }
            catch (Exception _ex)
            {
                return StatusCode(500, _ex.Message);
            }
        }
    }
}
