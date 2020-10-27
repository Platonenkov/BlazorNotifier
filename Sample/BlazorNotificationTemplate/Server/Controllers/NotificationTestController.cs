using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorNotifier.Classes;
using BlazorNotifier.Services.Implementations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlazorNotificationTemplate.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationTestController : ControllerBase
    {

        private readonly ILogger<NotificationTestController> logger;
        private readonly BlazorNotifierServerService _Notification;

        public NotificationTestController(ILogger<NotificationTestController> logger, BlazorNotifierServerService notification)
        {
            this.logger = logger;
            _Notification = notification;
        }

        [HttpGet("GetSomeData/{UserId}")]
        public async Task<IActionResult> GetSomeData(string UserId)
        {
            var random = new Random();
            using var progress = new BlazorNotifierProgress(UserId, _Notification);

            var count = 10;
            for (var i = 1; i <= count; i++)
            {
                var type = random.Next(2, 6);
                await _Notification.SendNotificationAsync(new BlazorNotifierMessage { Title = $"Step {i}", FromUserId = UserId, Type = (BlazorNotifierType)type });
                var val = i == 4||i==5||i==6 ? (int?)null : i * 100 / count;
                progress.Report((val, i>2? null : $"Step {i}", i>5?null: $"Test message {i}"));

                //long operation
                await Task.Delay(1000);
            }

            return Ok();
        }
    }

}
