using BisleriumBloggers.Hubs;
using BisleriumBloggers.Persistence;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using BisleriumBloggers.Configurations;
using BisleriumBloggers.Abstractions.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BisleriumBloggers.Implementations.Services;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("DefaultConnection");

services.AddControllers();

services.AddEndpointsApiExplorer();

services.AddCustomSwaggerGen();

services.AddCors();

services.AddSignalR();

services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        b => b.MigrationsAssembly("BisleriumBloggers")));

services.AddTransient<IEmailService, EmailService>();

services.AddHttpContextAccessor();

services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddCustomJWTBearer(configuration);

services.AddAuthorization();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var app = builder.Build();

app.UseSwagger();

app.UseStaticFiles();

app.UseSwaggerUI(c =>
{
    c.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
});

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

app.UseCors(policyBuilder =>
{
    policyBuilder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.UseHttpsRedirection();

app.Run();
