using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace BisleriumBloggers.Configurations
{
    public static class JWTBearerConfiguration
    {
        public static AuthenticationBuilder AddCustomJWTBearer(this AuthenticationBuilder builder, IConfiguration configuration)
        {
            return builder.AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.Audience = "https://localhost:44306";
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = "Bislerium",
                    ValidAudience = "https://localhost:44306",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("JWTAuthenticationHIGHsecuredPasswordVVVp1OH7Xzyr")),
                    ClockSkew = TimeSpan.FromSeconds(1000),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });
        }
    }
}

