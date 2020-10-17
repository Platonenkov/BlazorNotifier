using BlazorNotifier;
using BlazorNotifier.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BlazorNotificationTemplate.RealTimeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : NotifierController
    {

        public NotificationsController(IHubContext<NotificationHub> hubContext):base(hubContext)
        {
        }

    }
}
