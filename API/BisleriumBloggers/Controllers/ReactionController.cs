using System.Security.Claims;
using BisleriumBloggers.DTOs;
using BisleriumBloggers.Models;
using Microsoft.AspNetCore.Mvc;
using BisleriumBloggers.Persistence;
using Microsoft.AspNetCore.Authorization;

namespace BisleriumBloggers.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ReactionController(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor) : Controller
    {
        [HttpGet]
        public IActionResult GetBlogDetails(int blogId)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var user = dbContext.Users.Find(userId);
            
            var blog = dbContext.Blogs.Find(blogId);
            
            var reactions = dbContext.Reactions.Where(x => x.BlogId == blog!.Id && x.IsReactedForBlog && x.IsActive);

            var comments = dbContext.Comments.Where(x => x.IsActive);

            var upVotes = reactions.Where(x => x.ReactionId == 1 && x.BlogId == blog!.Id && x.IsReactedForBlog);
         
            var downVotes = reactions.Where(x => x.ReactionId == 2 && x.BlogId == blog!.Id && x.IsReactedForBlog);
         
            var popularity = upVotes.Count() * 2 - 
                             downVotes.Count() * 1 +
                             comments.Count();

            var blogDetails = new PostDetailsDto()
            {
                BlogId = blog!.Id,
                Title = blog.Title,
                Body = blog.Body,
                BloggerName = dbContext.Users.Find(blog.CreatedBy)!.FullName,
                BloggerImage = dbContext.Users.Find(blog.CreatedBy)!.ImageURL ?? "dummy.svg",
                Location = blog.Location,
                Reaction = blog.Reaction,
                UpVotes = reactions.Count(x => x.ReactionId == 1),
                DownVotes = reactions.Count(x => x.ReactionId == 2),
                IsUpVotedByUser = user != null && reactions.Any(x => x.ReactionId == 1 && x.CreatedBy == user.Id),
                IsDownVotedByUser = user != null && reactions.Any(x => x.ReactionId == 2 && x.CreatedBy == user.Id),
                IsEdited = blog.LastModifiedOn != null,
                CreatedAt = blog.CreatedOn,
                PopularityPoints = popularity,
                Images = dbContext.BlogImages.Where(x => x.BlogId == blog.Id && x.IsActive).Select(x => x.ImageURL).ToList(),
                CommentCount = comments.Count(),
                UploadedTimePeriod = (DateTime.Now - blog.CreatedOn).TotalMinutes < 1 ? $"{(int)((DateTime.Now - blog.CreatedOn).TotalSeconds)} seconds ago" :
                    (DateTime.Now - blog.CreatedOn).TotalHours < 1 ? $"{(int)((DateTime.Now - blog.CreatedOn).TotalMinutes)} minutes ago" :
                    (DateTime.Now - blog.CreatedOn).TotalHours < 24 ? $"{(int)((DateTime.Now - blog.CreatedOn).TotalHours)} hours ago" :
                    blog.CreatedOn.ToString("dd-MM-yyyy HH:mm"),
                Comments = GetCommentsRecursive( false, true, blog.Id)
            };
            
           return Ok(blogDetails);
        }
        
        [Authorize]
        [HttpPost]
        public IActionResult UpVoteDownVoteBlog(UpVoteDownVoteActionDto reactionModel)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var user = dbContext.Users.Find(userId);
            
            var blog = dbContext.Blogs.Find(reactionModel.BlogId ?? 0);

            var existingReaction = 
                dbContext.Reactions.Where(x => x.CreatedBy == user!.Id && x.ReactionId != 3 && x.IsReactedForBlog);

            if (existingReaction.Any())
            {
                dbContext.Reactions.RemoveRange(existingReaction);
                dbContext.SaveChanges();
            }

            var reaction = new Reaction
            {
                ReactionId = reactionModel.ReactionId ?? 0,
                BlogId = blog!.Id,
                CommentId = null,
                IsReactedForBlog = true,
                IsReactedForComment = false,
                CreatedOn = DateTime.Now,
                CreatedBy = user!.Id,
                IsActive = true,
            };
            
            dbContext.Reactions.Add(reaction);
            dbContext.SaveChanges();

            if (user.Id != blog.CreatedBy)
            {
                var action = reactionModel.ReactionId == 1 ? "up voted" : "down voted";
                
                var notificationModel = new Notification()
                {
                    Title = "You have received a new action",
                    Content = $"Your blog on {blog.Title} has been {action} by {user.Id}",
                    SenderId = userId,
                    ReceiverId = blog.CreatedBy,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now
                };

                dbContext.Notifications.Add(notificationModel);
                dbContext.SaveChanges();
            }
            
            return Ok();
        }
        
        [Authorize]
        [HttpPost]
        public IActionResult UpVoteDownVoteComment(UpVoteDownVoteActionDto reactionModel)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var user = dbContext.Users.Find(userId);

            var comment = dbContext.Comments.Find(reactionModel.CommentId ?? 0);

            var existingReaction = 
                dbContext.Reactions.Where(x => x.CreatedBy == user.Id && x.CommentId == comment.Id && x.ReactionId != 3 && x.IsReactedForComment);

            if (existingReaction.Any())
            {
                dbContext.Reactions.RemoveRange(existingReaction);
                dbContext.SaveChanges();
            }

            var reaction = new Reaction()
            {
                ReactionId = reactionModel.ReactionId ?? 0,
                BlogId = null,
                CommentId = comment!.Id,
                IsReactedForBlog = false,
                IsReactedForComment = true,
                CreatedOn = DateTime.Now,
                CreatedBy = user!.Id,
                IsActive = true,
            };

            dbContext.Reactions.Add(reaction);
            dbContext.SaveChanges();

            if (user.Id != comment.CreatedBy)
            {
                var action = reactionModel.ReactionId == 1 ? "up voted" : "down voted";
                
                var notificationModel = new Notification()
                {
                    Title = "You have received a new action",
                    Content = $"Your comment on {comment.Text} has been {action} by {user.Id}",
                    SenderId = userId,
                    ReceiverId = comment.CreatedBy,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now
                };

                dbContext.Notifications.Add(notificationModel);
                dbContext.SaveChanges();
            }
            
            return Ok();
        }

        [Authorize]
        [HttpPost]
        public IActionResult CommentForBlog(CommentActionDto reactionModel)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var user = dbContext.Users.Find(userId);
            
            var blog = dbContext.Blogs.Find(reactionModel.BlogId ?? 0);

            var comment = new Comment()
            {
                BlogId = blog!.Id,
                CommentId = null,
                Text = reactionModel.Comment ?? "",
                IsCommentForBlog = true,
                IsCommentForComment = false,
                IsActive = true,
                CreatedOn = DateTime.Now,
                CreatedBy = user!.Id,
            };
            
            dbContext.Comments.Add(comment);
            dbContext.SaveChanges();

            if (user.Id != blog.CreatedBy)
            {
                var notificationModel = new Notification()
                {
                    Title = "You have received a new action",
                    Content = $"Your blog on {blog.Title} has been commented by {user.Id}",
                    SenderId = userId,
                    ReceiverId = comment.CreatedBy,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now
                };

                dbContext.Notifications.Add(notificationModel);
                dbContext.SaveChanges();
            }
            
            return Ok();
        }
        
        [Authorize]
        [HttpPost]
        public IActionResult CommentForComment(CommentActionDto reactionModel)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var user = dbContext.Users.Find(userId);
            
            var commentModel = dbContext.Comments.Find(reactionModel.CommentId ?? 0);

            var comment = new Comment()
            {
                BlogId = commentModel!.BlogId,
                CommentId = commentModel.Id,
                Text = reactionModel.Comment ?? "",
                IsCommentForBlog = false,
                IsCommentForComment = true,
                IsActive = true,
                CreatedOn = DateTime.Now,
                CreatedBy = user!.Id,
            };
            
            dbContext.Comments.Add(comment);
            dbContext.SaveChanges();
            
            if (user.Id != comment.CreatedBy)
            {
                var notificationModel = new Notification()
                {
                    Title = "You have received a new action",
                    Content = $"Your comment on {comment.Text} has been looped on a commented by {user.Id}",
                    SenderId = userId,
                    ReceiverId = comment.CreatedBy,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now
                };

                dbContext.Notifications.Add(notificationModel);
                dbContext.SaveChanges();
            }
            
            return Ok();
        }

        [Authorize]
        [HttpDelete]
        public IActionResult DeleteComment(int commentId)
        {
            var comment = dbContext.Comments.Find(commentId);

            comment!.IsActive = false;
            
            dbContext.Comments.Update(comment);
            dbContext.SaveChanges();
            
            return Ok();
        }
        
        [NonAction]
        private List<PostComments> GetCommentsRecursive(bool isForComment, bool isForBlog, int? blogId = null,  int? parentId = null)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var user = dbContext.Users.Find(userId);

            var comments = dbContext.Comments
                .Where(x => x.BlogId == blogId && x.IsActive &&
                            x.IsCommentForBlog == isForBlog && x.IsCommentForComment == isForComment && x.CommentId == parentId);
                
            var result = new List<PostComments>();

            foreach (var comment in comments)
            {
                result.Add(new PostComments()
                {
                    Comment = comment.Text,
                    UpVotes = GetCommentReactionsCount(dbContext, comment.Id, 1),
                    DownVotes = GetCommentReactionsCount(dbContext, comment.Id, 2),
                    IsUpVotedByUser = IsCommentReactedByUser(dbContext, user!, comment.Id, 1),
                    IsDownVotedByUser = IsCommentReactedByUser(dbContext, user!, comment.Id, 2),
                    CommentId = comment.Id,
                    CommentedBy = GetUserName(dbContext, comment.CreatedBy),
                    ImageUrl = GetUserImageUrl(dbContext, comment.CreatedBy),
                    IsUpdated = comment.LastModifiedOn != null,
                    IsDeletable = comment.CreatedBy == userId,
                    CommentedTimePeriod = GetCommentTimePeriod(comment.CreatedOn),
                    Comments = GetCommentsRecursive(true, false, comment.BlogId, comment.Id)
                });
            }
            
            return result;
        }

        [NonAction]
        private static int GetCommentReactionsCount(ApplicationDbContext databaseContext, int commentId, int reactionId)
        {
            return databaseContext.Reactions
                .Count(z => z.IsReactedForComment && z.ReactionId == reactionId && z.CommentId == commentId);
        }

        [NonAction]
        private static bool IsCommentReactedByUser(ApplicationDbContext databaseContext, User user, int commentId, int reactionId)
        {
            return user != null && databaseContext.Reactions
                .Any(z => z.IsReactedForComment && z.ReactionId == reactionId && z.CreatedBy == user.Id && z.CommentId == commentId);
        }

        [NonAction]
        private static string GetUserName(ApplicationDbContext databaseContext, int userId)
        {
            var user = databaseContext.Users.Find(userId);

            return user != null ? user.FullName : "Unknown";
        }

        [NonAction]
        private static string GetUserImageUrl(ApplicationDbContext databaseContext, int userId)
        {
            var user = databaseContext.Users.Find(userId);
            
            return user != null ? user.ImageURL ?? "dummy.svg" : "dummy.svg";
        }

        [NonAction]
        private static string GetCommentTimePeriod(DateTime createdOn)
        {
            var timeDifference = DateTime.Now - createdOn;
            
            if (timeDifference.TotalMinutes < 1) return $"{(int)timeDifference.TotalSeconds} seconds ago";
            
            return timeDifference.TotalHours switch
            {
                < 1 => $"{(int)timeDifference.TotalMinutes} minutes ago",
                < 24 => $"{(int)timeDifference.TotalHours} hours ago",
                _ => createdOn.ToString("dd-MM-yyyy HH:mm")
            };
        }
    }
}

