using System;
using System.IO;
using System.Threading.Tasks;
using BlazorNotifier.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace BlazorNotifier.Controllers
{
    public class NotifierController: ControllerBase
    {
        private readonly IHubContext<NotificationHub> _HubContext;

        public NotifierController(IHubContext<NotificationHub> hubContext)
        {
            _HubContext = hubContext;
        }

        [HttpGet]
        public string Get()
        {
            return "It work";
        }

        /// <summary>
        /// Пересылает информационное сообщение отправленное в теле запроса
        /// </summary>
        /// <returns></returns>
        [HttpPost("SendTitle")]
        public async Task<IActionResult> SendTitle()
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();
            var inbound = JsonConvert.DeserializeObject<string>(json);
            await _HubContext.Clients.All.SendAsync("notification", new BlazorNotifierMessage { Title = inbound });
            return Ok("Notification has been sent succesfully!");
        }
        /// <summary>
        /// Пересылает сообщение о завершении процесса
        /// </summary>
        /// <returns></returns>
        [HttpPost("progress/finish")]
        public async Task<IActionResult> ProgressFinish(BlazorNotifierProgressMessage Message)
        {
            if (string.IsNullOrWhiteSpace(Message.ToUserId))
                return NotFound();
            await _HubContext.Clients.Client(Message.ToUserId).SendAsync("ProgressFinish", Message);
            return Ok("Progress Finished");
        }
        /// <summary>
        /// Пересылает сообщение об изменении процесса
        /// </summary>
        /// <returns></returns>
        [HttpPost("progress/update")]
        public async Task<IActionResult> ProgressUpdate(BlazorNotifierProgressMessage Message)
        {
            if (string.IsNullOrWhiteSpace(Message.ToUserId))
                return NotFound();
            await _HubContext.Clients.Client(Message.ToUserId).SendAsync("ProgressUpdate", Message);
            return Ok("Progress Updated");
        }
        /// <summary>
        /// Пересылает сообщение об изменении процесса
        /// </summary>
        /// <returns></returns>
        [HttpPost("progress/Add")]
        public async Task<IActionResult> ProgressAddNew(BlazorNotifierProgressMessage Message)
        {
            if (string.IsNullOrWhiteSpace(Message.ToUserId))
                return NotFound();
            await _HubContext.Clients.Client(Message.ToUserId).SendAsync("ProgressStart", Message);
            return Ok("Progress Added");
        }

        /// <summary>
        /// Получает информационное сообщение отправленное в строке
        /// </summary>
        /// <param name="Title">сообщение</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromQuery] string Title)
        {
            await _HubContext.Clients.All.SendAsync("notification", new BlazorNotifierMessage { Title = Title });
            return Ok("Notification has been sent succesfully!");
        }

        /// <summary>
        /// Получает информационное сообщение и рассылает его пользователям
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <returns></returns>
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage(BlazorNotifierMessage message)
        {
            try
            {
                var id = message.FromUserId;
                message.FromUserId = string.Empty;

                if (message.IsPrivate)
                    await _HubContext.Clients.Client(id).SendAsync("notification", message);
                else
                    await _HubContext.Clients.All.SendAsync("notification", message);

                return Ok("Notification has been sent succesfully!");
            }
            catch (Exception e)
            {
                return BadRequest($"Error to send notification: {e.Message}");
            }

        }
    }
}
