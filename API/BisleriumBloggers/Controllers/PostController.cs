using System.Net;
using System.Security.Claims;
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
    public class PostController(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor) : BaseController<PostController>
    {
        [HttpPost]
        public IActionResult CreatePost(PostUploadDto post)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);

            var user = dbContext.Users.Find(userId);

            var blogModel = new Blog()
            {
                Title = post.Title,
                Body = post.Body,
                Location = post.Location,
                Reaction = post.Reaction,
                BlogImages = post.Images?.Select(x => new BlogImage()
                {
                    ImageURL = x,
                    IsActive = true,
                    CreatedOn = DateTime.Now,
                    CreatedBy = user.Id
                }).ToList(),
                CreatedOn = DateTime.Now,
                CreatedBy = user.Id,
            };

            dbContext.Blogs.Add(blogModel);
            dbContext.SaveChanges();

            return Ok();
        }

        [HttpGet]
        public IActionResult GetPostById(int postId)
        {
            var blogDetails = dbContext.Blogs.Find(postId);

            var posts = new PostReadDto()
            {
                Id = blogDetails!.Id,
                Body = blogDetails.Body,
                Location = blogDetails.Location,
                Reaction = blogDetails.Reaction,
                Title = blogDetails.Title,
                Images = dbContext.BlogImages.Where(x => x.BlogId == blogDetails.Id).Select(x => x.ImageURL).ToList()
            };
            
            return Ok(posts);
        }

        [HttpGet]
        public IActionResult GetPostLogs(int postId)
        {
            var blogDetails = dbContext.BlogLogs.Where(x => x.BlogId == postId);
            
            var postLogDetails = blogDetails.Select(x => new BlogLogsDto()
            {
                Id = x!.Id,
                Body = x.Body,
                Location = x.Location,
                Reaction = x.Reaction,
                Title = x.Title,
            }).ToList();
            
            return Ok(postLogDetails);
        }
        
        [HttpPatch]
        public IActionResult UpdatePost(PostEditDto post)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);

            var user = dbContext.Users.Find(userId);
            
            var blogModel = dbContext.Blogs.Find(post.Id);

            var blogLog = new BlogLog()
            {
                BlogId = blogModel!.Id,
                Title = blogModel.Title,
                Location = blogModel.Location,
                Reaction = blogModel.Reaction,
                CreatedOn = DateTime.Now,
                CreatedBy = user!.Id,
                Body = blogModel.Body,
                IsActive = false
            };

            dbContext.BlogLogs.Add(blogLog);
            
            blogModel.Title = post.Title;
            blogModel.Body = post.Body;
            blogModel.Location = post.Location;
            blogModel.Reaction = post.Reaction;
            
            blogModel.LastModifiedOn = DateTime.Now;
            blogModel.LastModifiedBy = user.Id;
            
            dbContext.Blogs.Update(blogModel);
            dbContext.SaveChanges();
            
            return Ok();
        }

        [HttpDelete]
        public IActionResult DeletePost(int postId)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);

            var user = dbContext.Users.Find(userId);
            
            var blogModel = dbContext.Blogs.Find(postId);

            blogModel!.IsActive = false;
            blogModel.DeletedOn = DateTime.Now;
            blogModel.DeletedBy = user!.Id;
            
            dbContext.Blogs.Update(blogModel);
            dbContext.SaveChanges();
            
            return Ok();
        }
    }
}

