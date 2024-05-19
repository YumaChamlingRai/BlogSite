using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using BisleriumBloggers.Controllers.Base;
using BisleriumBloggers.DTOs;
using BisleriumBloggers.Persistence;
using Microsoft.AspNetCore.Authorization;

namespace BisleriumBloggers.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FeedController(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor) : BaseController<FeedController>
    {
        [HttpGet]
        public IActionResult GetHomePagePosts(string? sortBy = null)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var user = dbContext.Users.Find(userId);

            var blogs = dbContext.Blogs.Where(x => x.IsActive).ToList();

            var blogPostDetails = (from blog in blogs
                let reactions = dbContext.Reactions.Where(x => x.BlogId == blog.Id && x.IsReactedForBlog && x.IsActive)
                let comments = dbContext.Comments.Where(x => x.IsActive)
                let commentForBlogs = comments.Where(x => x.BlogId == blog.Id)
                let upVotes = reactions.Where(x => x.ReactionId == 1 && x.BlogId == blog.Id && x.IsReactedForBlog)
                let downVotes = reactions.Where(x => x.ReactionId == 2 && x.BlogId == blog.Id && x.IsReactedForBlog)
                let upVoteCount = upVotes.Count() * 2
                let downVoteCount = downVotes.Count() * 1
                let commentCount = commentForBlogs.Count()
                let popularity = upVoteCount - downVoteCount + commentCount
                select new PostDetailsDto()
                {
                    BlogId = blog.Id,
                    Title = blog.Title,
                    BloggerName = dbContext.Users.FirstOrDefault(x => x.Id == blog.CreatedBy)!.FullName,
                    BloggerImage = dbContext.Users.FirstOrDefault(x => x.Id == blog.CreatedBy)!.ImageURL ?? "dummy.svg",
                    Location = blog.Location,
                    Reaction = blog.Reaction,
                    Body = blog.Body,
                    UpVotes = reactions.Count(x => x.ReactionId == 1),
                    DownVotes = reactions.Count(x => x.ReactionId == 2),
                    IsUpVotedByUser = user != null && reactions.Any(x => x.ReactionId == 1 && x.CreatedBy == user.Id),
                    IsDownVotedByUser = user != null && reactions.Any(x => x.ReactionId == 2 && x.CreatedBy == user.Id),
                    IsEdited = blog.LastModifiedOn != null,
                    CreatedAt = blog.CreatedOn,
                    PopularityPoints = popularity,
                    Images = dbContext.BlogImages.Where(x => x.BlogId == blog.Id && x.IsActive).Select(x => x.ImageURL).ToList(),
                    UploadedTimePeriod = (DateTime.Now - blog.CreatedOn).TotalMinutes < 1 ? $"{(int)((DateTime.Now - blog.CreatedOn).TotalSeconds)} seconds ago" :
                        (DateTime.Now - blog.CreatedOn).TotalHours < 1 ? $"{(int)((DateTime.Now - blog.CreatedOn).TotalMinutes)} minutes ago" :
                        (DateTime.Now - blog.CreatedOn).TotalHours < 24 ? $"{(int)((DateTime.Now - blog.CreatedOn).TotalHours)} hours ago" : blog.CreatedOn.ToString("dd-MM-yyyy HH:mm"),
                    CommentCount = commentForBlogs.Count(),
                    Comments = dbContext.Comments.Where(x => x.BlogId == blog.Id && x.IsActive && x.IsCommentForBlog)
                        .Select(x => new PostComments()
                        {
                            Comment = x.Text,
                            UpVotes = dbContext.Reactions.Where(z => z.IsReactedForComment && z.IsActive && z.CommentId == x.Id).Count(z => z.ReactionId == 1),
                            DownVotes = dbContext.Reactions.Where(z => z.IsReactedForComment && z.IsActive && z.CommentId == x.Id).Count(z => z.ReactionId == 2),
                            IsUpVotedByUser = user != null && dbContext.Reactions.Where(z => z.IsReactedForComment && x.IsActive && z.CommentId == x.Id).Any(z => z.ReactionId == 1 && z.CreatedBy == user.Id),
                            IsDownVotedByUser = user != null && dbContext.Reactions.Where(z => z.IsReactedForComment && x.IsActive && z.CommentId == x.Id).Any(z => z.ReactionId == 2 && z.CreatedBy == user.Id),
                            CommentId = x.Id,
                            CommentedBy = dbContext.Users.FirstOrDefault(z => z.Id == x.CreatedBy)!.FullName,
                            ImageUrl = dbContext.Users.FirstOrDefault(z => z.Id == x.CreatedBy)!.ImageURL ?? "dummy.svg",
                            IsUpdated = x.LastModifiedOn != null,
                            CommentedTimePeriod = (DateTime.Now - x.CreatedOn).TotalMinutes < 1 ? $"{(int)((DateTime.Now - x.CreatedOn).TotalSeconds)} seconds ago" :
                                (DateTime.Now - x.CreatedOn).TotalHours < 1 ? $"{(int)((DateTime.Now - x.CreatedOn).TotalMinutes)} minutes ago" :
                                (DateTime.Now - x.CreatedOn).TotalHours < 24 ? $"{(int)((DateTime.Now - x.CreatedOn).TotalHours)} hours ago" : x.CreatedOn.ToString("dd-MM-yyyy HH:mm")
                        })
                        .Take(1)
                        .ToList()
                }).ToList();

            switch (sortBy)
            {
                case null:
                    blogPostDetails = blogPostDetails.OrderByDescending(x => x.CreatedAt).ToList();
                    break;
                case "Popularity":
                    blogPostDetails = blogPostDetails.OrderByDescending(x => x.PopularityPoints).ToList();
                    break;
                case "Recency":
                    blogPostDetails = blogPostDetails.OrderByDescending(x => x.CreatedAt).ToList();
                    break;
                default:
                    var random = new Random();
                    var n = blogPostDetails.Count;
                    while (n > 1)
                    {
                        n--;
                        var k = random.Next(n + 1);
                        (blogPostDetails[k], blogPostDetails[n]) = (blogPostDetails[n], blogPostDetails[k]);
                    }
                    
                    break;
            }
            
            return Ok(blogPostDetails);
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetMyBlogsList(string? sortBy = null)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var user = dbContext.Users.Find(userId);

            var blogs = dbContext.Blogs.Where(x => x.CreatedBy == user.Id && x.IsActive).ToList();

            var blogPostDetails = (from blog in blogs
                let reactions = dbContext.Reactions.Where(x => x.BlogId == blog.Id && x.IsReactedForBlog && x.IsActive)
                let comments = dbContext.Comments.Where(x => x.IsActive)
                let commentForBlogs = comments.Where(x => x.BlogId == blog.Id)
                let upVotes = reactions.Where(x => x.ReactionId == 1 && x.BlogId == blog.Id && x.IsReactedForBlog)
                let downVotes = reactions.Where(x => x.ReactionId == 2 && x.BlogId == blog.Id && x.IsReactedForBlog)
                let upVoteCount = upVotes.Count() * 2
                let downVoteCount = downVotes.Count() * 1
                let commentCount = commentForBlogs.Count()
                let popularity = upVoteCount - downVoteCount + commentCount
                select new PostDetailsDto()
                {
                    BlogId = blog.Id,
                    Title = blog.Title,
                    BloggerName = dbContext.Users.FirstOrDefault(x => x.Id == blog.CreatedBy)!.FullName,
                    BloggerImage = dbContext.Users.FirstOrDefault(x => x.Id == blog.CreatedBy)!.ImageURL ?? "dummy.svg",
                    Location = blog.Location,
                    Reaction = blog.Reaction,
                    Body = blog.Body,
                    UpVotes = reactions.Count(x => x.ReactionId == 1),
                    DownVotes = reactions.Count(x => x.ReactionId == 2),
                    IsUpVotedByUser = user != null && reactions.Any(x => x.ReactionId == 1 && x.CreatedBy == user.Id),
                    IsDownVotedByUser = user != null && reactions.Any(x => x.ReactionId == 2 && x.CreatedBy == user.Id),
                    IsEdited = blog.LastModifiedOn != null,
                    CreatedAt = blog.CreatedOn,
                    PopularityPoints = popularity,
                    Images = dbContext.BlogImages.Where(x => x.BlogId == blog.Id && x.IsActive).Select(x => x.ImageURL).ToList(),
                    UploadedTimePeriod = (DateTime.Now - blog.CreatedOn).TotalMinutes < 1 ? $"{(int)((DateTime.Now - blog.CreatedOn).TotalSeconds)} seconds ago" :
                        (DateTime.Now - blog.CreatedOn).TotalHours < 1 ? $"{(int)((DateTime.Now - blog.CreatedOn).TotalMinutes)} minutes ago" :
                        (DateTime.Now - blog.CreatedOn).TotalHours < 24 ? $"{(int)((DateTime.Now - blog.CreatedOn).TotalHours)} hours ago" : blog.CreatedOn.ToString("dd-MM-yyyy HH:mm"),
                    CommentCount = commentForBlogs.Count(),
                    Comments = dbContext.Comments.Where(x => x.BlogId == blog.Id && x.IsActive && x.IsCommentForBlog)
                        .Select(x => new PostComments()
                        {
                            Comment = x.Text,
                            UpVotes = dbContext.Reactions.Where(z => z.IsReactedForComment && z.IsActive && z.CommentId == x.Id).Count(z => z.ReactionId == 1),
                            DownVotes = dbContext.Reactions.Where(z => z.IsReactedForComment && z.IsActive && z.CommentId == x.Id).Count(z => z.ReactionId == 2),
                            IsUpVotedByUser = user != null && dbContext.Reactions.Where(z => z.IsReactedForComment && x.IsActive && z.CommentId == x.Id).Any(z => z.ReactionId == 1 && z.CreatedBy == user.Id),
                            IsDownVotedByUser = user != null && dbContext.Reactions.Where(z => z.IsReactedForComment && x.IsActive && z.CommentId == x.Id).Any(z => z.ReactionId == 2 && z.CreatedBy == user.Id),
                            CommentId = x.Id,
                            CommentedBy = dbContext.Users.FirstOrDefault(z => z.Id == x.CreatedBy)!.FullName,
                            ImageUrl = dbContext.Users.FirstOrDefault(z => z.Id == x.CreatedBy)!.ImageURL ?? "dummy.svg",
                            IsUpdated = x.LastModifiedOn != null,
                            CommentedTimePeriod = (DateTime.Now - x.CreatedOn).TotalMinutes < 1 ? $"{(int)((DateTime.Now - x.CreatedOn).TotalSeconds)} seconds ago" :
                                (DateTime.Now - x.CreatedOn).TotalHours < 1 ? $"{(int)((DateTime.Now - x.CreatedOn).TotalMinutes)} minutes ago" :
                                (DateTime.Now - x.CreatedOn).TotalHours < 24 ? $"{(int)((DateTime.Now - x.CreatedOn).TotalHours)} hours ago" : x.CreatedOn.ToString("dd-MM-yyyy HH:mm")
                        })
                        .Take(1)
                        .ToList()
                }).ToList();

            switch (sortBy)
            {
                case null:
                    blogPostDetails = blogPostDetails.OrderByDescending(x => x.CreatedAt).ToList();
                    break;
                case "Popularity":
                    blogPostDetails = blogPostDetails.OrderByDescending(x => x.PopularityPoints).ToList();
                    break;
                case "Recency":
                    blogPostDetails = blogPostDetails.OrderByDescending(x => x.CreatedAt).ToList();
                    break;
                default:
                    var random = new Random();
                    var n = blogPostDetails.Count;
                    while (n > 1)
                    {
                        n--;
                        var k = random.Next(n + 1);
                        (blogPostDetails[k], blogPostDetails[n]) = (blogPostDetails[n], blogPostDetails[k]);
                    }
                    
                    break;
            }
            
            return Ok(blogPostDetails);
        }
    }
}

