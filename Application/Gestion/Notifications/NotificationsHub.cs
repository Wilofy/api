using Microsoft.AspNetCore.SignalR;

namespace Application.Gestion.Notifications;
public static class NotificationsHub
{
    public class HubNotifications : Hub
    {
        public async Task SendNotification(object notification)
        {
            await Clients.All.SendAsync("ReceiveNotification", notification);
        }
    }
}

