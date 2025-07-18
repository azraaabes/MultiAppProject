using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using Webapiproje.Authentication;
using Webapiproje.DataDbContext;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.OpenApi.Models;

var logger = LogManager.Setup()
                       .LoadConfigurationFromFile("nlog.config")
                       .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // --- LOGGING ---
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();   // AddNLog yerine modern yol

    // --- SERVICES ---
 // Redis 
    builder.Services.AddStackExchangeRedisCache(option =>
    {
        option.Configuration = builder.Configuration.GetConnectionString("Redis");
        option.InstanceName = "WebApiCache_";
    });
    builder.Services.AddControllers();
  
    //builder.Services.AddMemoryCache();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                         Type = ReferenceType.SecurityScheme,
                         Id = "Bearer"
                    }
                 },
            new string []{}
            }
        });
    });


    //builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuthentication", null);
    // --- JWT AYARLARI ------------------------------------------------------
    // appsettings.json ? Jwt section'ýný POCO'ya baðla
    builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

    // Token üretim servisini DI container'a ekle
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

    // Sadece JWT Bearer Authentication
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(jwt.Key))
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowMyMvcApp", policy =>
            policy.WithOrigins("https://localhost:44348")
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    var app = builder.Build();

    // --- MIDDLEWARE ---
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseCors("AllowMyMvcApp");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Program baþlatýlýrken kritik hata");
    throw;
}
finally
{
    LogManager.Shutdown();
}
