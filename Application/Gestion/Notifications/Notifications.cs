using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Text;
using static Application.Gestion.Notifications.NotificationsHub;

namespace Application.Gestion.Notifications;
public static class Notifications
{
    public static void NotificationsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/usuario/notifications", async (ISender mediator, HttpContext context) =>
        {
            string requestBody;

            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
            {
                requestBody = await reader.ReadToEndAsync();
            }
            return await mediator.Send(new PostNotificationsCommand(requestBody));
        })
        .Produces(StatusCodes.Status200OK);
    }

    public record PostNotificationsCommand(string RequestBody) : IRequest<IResult>;

    public sealed class PostNotificationsCommandHandler : IRequestHandler<PostNotificationsCommand, IResult>
    {
        private readonly IHubContext<MyHub> _hubContext;

        public PostNotificationsCommandHandler(IHubContext<MyHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task<IResult> Handle(PostNotificationsCommand request, CancellationToken cancellationToken)
        {
            var notification = await Task.Run(() => JsonConvert.DeserializeObject<NotificationMeli>(request.RequestBody));

            var notificationTiendaNube = await Task.Run(() => JsonConvert.DeserializeObject<NotificationTiendanube>(request.RequestBody));

            if (notification!.Topic == "questions"
                || notification.Topic == "messages"
                || notification.Topic == "orders_v2")
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);

                Console.WriteLine(notification!.Resource + " Topic: " + notification.Topic + " User_ID: " + notification.User_id);
            }

            if (notificationTiendaNube!.Event == "order/created"
                || notificationTiendaNube!.Event == "order/paid")
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notificationTiendaNube);

                Console.WriteLine(notificationTiendaNube!.Event);
            }
            return Results.Ok();
        }
    }

}

public class NotificationMeli
{
    public string? _id { get; set; }
    public string? Resource { get; set; }
    public long? User_id { get; set; }
    public string? Topic { get; set; }
    public long? Application_id { get; set; }
    public int? Attempts { get; set; }
    public DateTime Sent { get; set; }
    public DateTime Received { get; set; }
    public string Platform { get; } = "Mercado Libre";
}

public class NotificationTiendanube
{
    public string? Store_id { get; set; }
    public string? Event { get; set; }
    public int Id { get; set; }
    public DateTime Sent { get; set; } = DateTime.Now;
    public string Platform { get; } = "TiendaNube";
}
