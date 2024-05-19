using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using BisleriumBloggers.Controllers.Base;
using BisleriumBloggers.DTOs;
using BisleriumBloggers.Hubs;
using BisleriumBloggers.Models;
using BisleriumBloggers.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BisleriumBloggers.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class NotificationController(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor) : BaseController<FeedController>
    {
        [HttpPost]
        public IActionResult PostNotifications(NotificationPostDto notification)
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);

            if (userId != notification.ReceiverId)
            {
                var notificationModel = new Notification()
                {
                    Title = notification.Header,
                    Content = notification.Message,
                    SenderId = userId,
                    ReceiverId = notification.ReceiverId,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now
                };

                dbContext.Notifications.Add(notificationModel);
                dbContext.SaveChanges();

                var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<NotificationHub>>();

                hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveAlertNotification", userId.ToString());
                hubContext.Clients.User(notification.ReceiverId.ToString()).SendAsync("ReceiveAlertNotification", notification.ReceiverId.ToString());
            }

            return Ok();
        }
        
        [HttpGet]
        public IActionResult GetNotifications()
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var notifications = dbContext.Notifications.Where(x => x.ReceiverId == userId && x.IsActive);

            var result = notifications
                .OrderByDescending(x => x.CreatedOn)
                .GroupBy(x => x.CreatedOn.Date)
                .Select(x => new NotificationDetailsDto()
                {
                    TimePeriod = x.Key.ToString("dd-MM-yyyy"),
                    Notifications = x.Select(n => new Notifications()
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Content = n.Content,
                        IsSeen = n.IsSeen,
                        SentByUser = dbContext.Users.Find(n.SenderId)!.FullName,
                        SentImageUrl = dbContext.Users.Find(n.SenderId)!.ImageURL ?? "dummy.svg",
                    }).ToList(),
                }).ToList();

            return Ok(result);
        }
        
        [HttpGet]
        public IActionResult GetUnreadNotificationsCount()
        {
            var userIdClaimValue = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(userIdClaimValue, out var userId);
            
            var notifications = dbContext.Notifications.Where(x => x.ReceiverId == userId && x.IsActive && !x.IsSeen);

            return Ok(notifications.Count());
        }
    }
}

