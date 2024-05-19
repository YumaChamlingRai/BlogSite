using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using BisleriumBloggers.Persistence;
using BisleriumBloggers.Controllers.Base;
using BisleriumBloggers.DTOs;
using BisleriumBloggers.Helper;
using BisleriumBloggers.Models;
using Microsoft.IdentityModel.Tokens;

namespace BisleriumBloggers.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthenticationController(ApplicationDbContext dbContext) : BaseController<AuthenticationController>
    { 
        [HttpPost]
        public IActionResult Login(LoginDto login)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.EmailAddress == login.EmailAddress);

            if (user != null)
            {
                var isPasswordValid = PasswordManager.VerifyHash(login.Password, user.Password);

                if (isPasswordValid)
                {
                    var role = dbContext.Roles.Find(user.RoleId);
                   
                    var authClaims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, (user.Id.ToString() ?? null) ?? string.Empty),
                        new(ClaimTypes.Name, user.FullName),
                        new(ClaimTypes.Email, user.EmailAddress),
                        new(ClaimTypes.Role, role!.Name ?? ""),
                        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };
      
                    var symmetricSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("JWTAuthenticationHIGHsecuredPasswordVVVp1OH7Xzyr"));
                
                    var signingCredentials = new SigningCredentials(symmetricSigningKey, SecurityAlgorithms.HmacSha256);
                
                    var expirationTime = DateTime.UtcNow.AddMinutes(Convert.ToInt32(1000));
                
                    var accessToken = new JwtSecurityToken(
                       "Bislerium",
                       "https://localhost:44306",
                       claims: authClaims,
                       signingCredentials: signingCredentials,
                       expires: expirationTime
                    );

                    var userDetails = new AppUserDto()
                    {
                       Id = user.Id,
                       Name = user.FullName,
                       Username = user.UserName,
                       EmailAddress = user.EmailAddress,
                       RoleId = role.Id,
                       Role = role.Name ?? "",
                       ImageUrl = user.ImageURL ?? "dummy.svg",
                       Token = new JwtSecurityTokenHandler().WriteToken(accessToken)
                    };

                    return Ok(userDetails);
                }
            }

            return BadRequest();
        }

        [HttpPost]
        public IActionResult Register(RegisterDto register)
        {
            var existingUser = dbContext.Users.FirstOrDefault(x =>
                x.EmailAddress == register.EmailAddress || 
                x.UserName == register.Username);

            if (existingUser == null)
            {
                var role = dbContext.Roles.FirstOrDefault(x => x.Name == "Blogger");
                
                var appUser = new User()
                {
                    FullName = register.FullName,
                    EmailAddress = register.EmailAddress,
                    RoleId = role!.Id,
                    Password = PasswordManager.HashSecret(register.Password),
                    UserName = register.Username,
                    MobileNo = register.MobileNumber,
                    ImageURL = register.ImageURL
                };

                dbContext.Users.Add(appUser);
                dbContext.SaveChanges();

                return Ok();
            }

            return BadRequest();
        }
    }
}

