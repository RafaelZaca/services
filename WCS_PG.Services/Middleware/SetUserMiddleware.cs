using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WCS_PG.Services.Middleware
{
    public class SetUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SetUserMiddleware> _logger;

        public SetUserMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<SetUserMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Verificar primeiro se há um cookie de autenticação do PrIME
            bool isPrimeRequest = context.Request.Path.ToString().Contains("prime", StringComparison.OrdinalIgnoreCase);
            if (isPrimeRequest && context.Request.Cookies.TryGetValue("PrIME_Auth", out string primeAuthCookie))
            {
                try
                {
                    // Validar o cookie usando a mesma lógica de validação do token JWT
                    var tokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection("Encryption:Secret").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };

                    var handler = new JwtSecurityTokenHandler();
                    var userClaims = handler.ValidateToken(primeAuthCookie, tokenValidationParameters, out var validatedToken);

                    var identity = new ClaimsIdentity(userClaims.Claims, "Cookie");
                    context.User = new ClaimsPrincipal(identity);
                    _logger.LogInformation("Usuário autenticado via cookie do PrIME");

                    // Continuar para o próximo middleware
                    await _next(context);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao validar cookie do PrIME");
                    // Falha na validação do cookie, continuar para validação de token
                }
            }

            // Verificar autenticação por token Bearer (código existente)
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection("Encryption:Secret").Value)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
                try
                {
                    var userClaims = handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                    var identity = new ClaimsIdentity(userClaims.Claims, "Jwt");
                    context.User = new ClaimsPrincipal(identity);
                    _logger.LogInformation("Usuário autenticado via token Bearer");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao validar token Bearer");
                }
            }
            await _next(context);
        }
    }
}