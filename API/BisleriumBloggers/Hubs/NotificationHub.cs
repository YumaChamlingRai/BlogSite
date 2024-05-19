using Microsoft.AspNetCore.SignalR;

namespace BisleriumBloggers.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(int userId)
    {
        await Clients.All.SendAsync("ReceiveAlertNotification", userId);
    }
}