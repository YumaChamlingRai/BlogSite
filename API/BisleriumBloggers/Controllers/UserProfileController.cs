using System.Security.Claims;
using BisleriumBloggers.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using BisleriumBloggers.Controllers.Base;
using BisleriumBloggers.DTOs;
using BisleriumBloggers.Helper;
using BisleriumBloggers.Persistence;
using Microsoft.AspNetCore.Authorization;

namespace BisleriumBloggers.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserProfileController(ApplicationDbContext dbContext, IEmailService emailService, IHttpContextAccessor contextAccessor) : BaseController<UserProfileController>
    {
        [HttpGet]
        public IActionResult ResetPassword(string emailAddress)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.EmailAddress == emailAddress);

            if (user != null)
            {
                const string message = $"Your password has been updated to Admin@123.<br><br>" +
                                       $"Regards,<br>" +
                                       $"Bislerium.";
                
                var email = new EmailDto()
                {
                    Email = user.EmailAddress,
                    Message = message,
                    Subject = "Bislerium"
                };
        
                emailService.SendEmail(email);

                return Ok(message);
            }

            return BadRequest();
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetUserDetails()
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var user = dbContext.Users.Find(userId);

            var role = dbContext.Roles.Find(user!.RoleId);

            var userProfile = new UserProfileDetailsDto()
            {
                UserId = user.Id,
                FullName = user.FullName,
                Username = user.UserName,
                EmailAddress = user.EmailAddress,
                RoleId = role!.Id,
                RoleName = role.Name,
                ImageURL = user.ImageURL ?? "dummy.svg",
                MobileNumber = user.MobileNo ?? ""
            };

            return Ok(userProfile);
        }
        
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordDto changePassword)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var user = dbContext.Users.Find(userId);

            var isPasswordValid = PasswordManager.VerifyHash(changePassword.CurrentPassword, user.Password);

            if (isPasswordValid)
            {
                user.Password = PasswordManager.HashSecret(changePassword.NewPassword);

                dbContext.Users.Update(user);
                dbContext.SaveChanges();

                return Ok();
            }
            
            return BadRequest();
        }
        
        [HttpPatch]
        public IActionResult UpdateProfileDetails(UserProfileDetailsDto profileDetails)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var user = dbContext.Users.Find(userId);

            user!.FullName = profileDetails.FullName;
            user.MobileNo = profileDetails.MobileNumber;
            user.EmailAddress = profileDetails.EmailAddress;
        
            dbContext.Users.Update(user);
            dbContext.SaveChanges();
            
            return Ok();
        }

        [HttpDelete]
        public IActionResult DeleteProfile()
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var user = dbContext.Users.Find(userId);
            
            var blogs = dbContext.Blogs.Where(x => x.CreatedBy == user!.Id);

            var blogLogs = dbContext.BlogLogs.Where(x => blogs.Select(z => z.Id).Contains(x.BlogId));
        
            var blogImages = dbContext.BlogImages.Where(x => blogs.Select(z => z.Id).Contains(x.BlogId));

            var comments = dbContext.Comments.Where(x => x.CreatedBy == user!.Id);

            var commentLogs = dbContext.CommentLogs.Where(x => comments.Select(z => z.Id).Contains(x.CommentId));

            var reactions = dbContext.Reactions.Where(x => x.CreatedBy == user!.Id);

            dbContext.RemoveRange(reactions);
            
            dbContext.RemoveRange(commentLogs);
            
            dbContext.RemoveRange(comments);
            
            dbContext.RemoveRange(blogImages);
            
            dbContext.RemoveRange(blogLogs);
            
            dbContext.RemoveRange(blogs);

            if (user != null)
            {
                dbContext.Remove(user);
            }
            
            return Ok();
        }
    }
}

