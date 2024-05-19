using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using BisleriumBloggers.Controllers.Base;
using BisleriumBloggers.DTOs;
using BisleriumBloggers.Models;
using BisleriumBloggers.Persistence;
using Microsoft.AspNetCore.Authorization;

namespace BisleriumBloggers.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AdministratorController(ApplicationDbContext dbContext) : BaseController<AdministratorController>
    {
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = dbContext.Users.ToList();

            var userList = users.Select(x => new AppUserDto()
            {
                Id = x.Id,
                RoleId = x.RoleId,
                EmailAddress = x.EmailAddress,
                ImageUrl = x.ImageURL ?? "dummy.svg",
                Username = x.UserName,
                Name = x.FullName,
                Role = dbContext.Roles.Find(x.RoleId)!.Name
            }).ToList();
        
            return Ok(userList);
        }
        
        [HttpPost]
        public IActionResult RegisterAdministrator(RegisterDto register)
        {
            var existingUser = dbContext.Users.FirstOrDefault(x =>
                x.EmailAddress == register.EmailAddress || x.UserName == register.Username);

            if (existingUser == null)
            {
                var role = dbContext.Roles.FirstOrDefault(x => x.Name == "Admin");

                var appUser = new User
                {
                    FullName = register.FullName,
                    EmailAddress = register.EmailAddress,
                    RoleId = role!.Id,
                    Password = register.Password,
                    UserName = register.Username,
                    MobileNo = register.MobileNumber,
                    ImageURL = register.ImageURL
                };

                dbContext.Add(appUser);
                dbContext.SaveChanges();
                
                return Ok();
            }
      
            return BadRequest();
        }
        
        [HttpGet]
        public IActionResult GetDashboardDetails(string? selectedMonth)
        {
            var blogs = dbContext.Blogs.Where(x => x.IsActive);
            var reactions = dbContext.Reactions.Where(x => x.IsActive);
            var comments = dbContext.Comments.Where(x => x.IsActive);

            if (selectedMonth != null)
            {
                var targetMonth = DateTime.ParseExact(selectedMonth, "MMMM", CultureInfo.InvariantCulture).Month;
                blogs = blogs.Where(blog => blog.CreatedOn.Month == targetMonth);
                reactions = reactions.Where(reaction => reaction.CreatedOn.Month == targetMonth);
                comments = comments.Where(comment => comment.CreatedOn.Month == targetMonth);
            }

            var blogsWithCounts = blogs.Select(blog => new
            {
                Blog = blog,
                UpVotesCount = reactions.Count(x => x.ReactionId == 1 && x.BlogId == blog.Id && x.IsReactedForBlog),
                DownVotesCount = reactions.Count(x => x.ReactionId == 2 && x.BlogId == blog.Id && x.IsReactedForBlog),
                CommentsCount = comments.Count(x => x.BlogId == blog.Id)
            }).ToList();

            var dashboardDetails = new List<DashboardDetails>()
            {
                new()
                {
                    Title = "Posts",
                    Count = blogsWithCounts.Count()
                },
                new()
                {
                    Title = "Up Votes",
                    Count = blogsWithCounts.Sum(x => x.UpVotesCount)
                },
                new()
                {
                    Title = "Down Votes",
                    Count = blogsWithCounts.Sum(x => x.DownVotesCount)
                },
                new()
                {
                    Title = "Comments",
                    Count = blogsWithCounts.Sum(x => x.CommentsCount)
                }
            };

            var bloggerPopularity = blogsWithCounts
                .GroupBy(x => x.Blog.CreatedBy)
                .Select(group => new
                {
                    BloggerId = group.Key,
                    BloggerName = dbContext.Users.FirstOrDefault(u => u.Id == group.Key)!.FullName ?? "Unknown",
                    Popularity = group.Sum(x => x.UpVotesCount * 2 - x.DownVotesCount + x.CommentsCount)
                })
                .OrderByDescending(x => x.Popularity)
                .Take(10)
                .Select(z => new DashboardDetails
                {
                    Title = z.BloggerName,
                    Count = z.Popularity
                })
                .ToList();

            var popularBlogs = blogsWithCounts
                .OrderByDescending(x => x.UpVotesCount * 2 - x.DownVotesCount + x.CommentsCount)
                .Take(10)
                .Select(z => new DashboardDetails
                {
                    Title = z.Blog.Title,
                    Count = z.UpVotesCount * 2 - z.DownVotesCount + z.CommentsCount
                })
                .ToList();

            var dashboardAnalytics = new DashboardAnalyticsDto()
            {
                ReactionCount = dashboardDetails,
                PopularBloggersCount = bloggerPopularity,
                PopularBlogsCount = popularBlogs
            };

            return Ok(dashboardAnalytics);
       }
    }
}

