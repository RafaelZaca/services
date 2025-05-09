using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;
using WCS_PG.Data;
using WCS_PG.Services;
using WCS_PG.Services.Middleware;
using WCS_PG.Services.Services;

var builder = WebApplication.CreateBuilder(args);
// JSON configuration files
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // Default settings
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true); // Environment-specific settings

builder.Configuration.AddEnvironmentVariables();
// Add services to the container.

builder.Services.AddControllers();

// API versioning
var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true; // Adiciona as versões ao header
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// API explorer for Swagger
apiVersioningBuilder.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // formatação como "v1"
    options.SubstituteApiVersionInUrl = true;
});

//Adiciona Conexão Entity

builder.Services.AddDbContext<WCSContext>
    (options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSingleton<ClpCommunicationService>();

//HttpClient nomeado
builder.Services.AddHttpClient("PrimeApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PrimeApi:BaseUrl"]);
    // Outras configurações como headers, timeouts, etc.
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = true,
    CookieContainer = new CookieContainer()
}); 

//Adiciona Authentication

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration["Encryption:Secret"]))
    };
});

//Adiciona Authorization

builder.Services.AddAuthorization(auth =>
{
    auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
        .RequireAuthenticatedUser().Build());
});


var app = builder.Build();
WCS_PG.Services.Helpers.ServiceProviderHelper.ServiceProvider = app.Services;

// Middleware pipeline para Swagger
if (builder.Configuration.GetSection("SwaggerConfig")?.GetValue<string>("habilitado")?.Equals("true", StringComparison.CurrentCultureIgnoreCase) == true)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        bool apenasIntegracao = builder.Configuration.GetSection("SwaggerConfig")?.GetValue<string>("apenasIntegracao")?.Equals("true", StringComparison.CurrentCultureIgnoreCase) == true;

        if (apenasIntegracao)
        {
            // Apenas endpoints de integração
            options.SwaggerEndpoint("/swagger/v1.1/swagger.json", "Integracao_Prime");
        }
        else
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
            //options.SwaggerEndpoint("/swagger/v2/swagger.json", "V2");
            options.SwaggerEndpoint("/swagger/v1.1/swagger.json", "Integracao_Prime");
        }
    });
}

app.UseCors(options =>
    options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseMiddleware<SetUserMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
