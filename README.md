# BlazorNotifier
Blazor notifier from server to client by SignalR

## Install-Package BlazorNotifier -Version 5.0.0.0
Net5 support
![Demo](https://github.com/Platonenkov/BlazorNotifier/blob/main/Resources/Notifier.gif)

## Methods
### OnClient
| Properties | Description | Описание |
|:----------------|:---------|:----------------|
| Events | Time/Message Event Dictionary | Словарь событий время/сообщение |
| Notification | Notification Message Collection | Коллекция уведомительных сообщений |
| CountOfNotifications | Count of visible messages | Количество видимых сообщений |
| UserId | Client ID for sending messages | Id клиента для отправки сообщений |
| StatusStyleOnOpen | Status style when connected | Стиль статуса при подключенном состоянии |
| StatusStyleOnClose |Status style when disconnected | Стиль статуса при отсутствии соединения |
| StatusStyle | Current Status Style | Текущий стиль статуса по состоянию |
| IsConnected | Service connection status | Состояние подключения к сервису |
| ConnectionStatus | Data Link States | Состояния канала передачи данных |

| Method | Description | Описание |
|:----------------|:---------|:----------------|
| SendNotification | Show message (2 overloads) | Показать сообщение (2 перегрузки) |
| CloseMessage | Close the message (forced) | Закрыть сообщение (принудительно) |
| LogNotification | Write message to log (2 overloads) | Записать сообщение в лог (2 перегрузки) |
| SendOrUpdateProgress | Show progress bar or update status if already shown | Показать прогресс бар или обновить состояние если уже показан |
| CloseProgress | Close progress bar (forced) | Закрыть прогресс бар (принудительно) |

### OnServer
| Method | Description | Описание |
|:----------------|:---------|:----------------|
| SendNotificationAsync | Send message to client (2 overloads) | Отправить сообщение клиенту (2 перегрузки) |
| SendLogAsync | Send log message to client (2 overloads) | Отправить клиенту сообщение в лог (2 перегрузки) |
| AddNewProgress | Inform the customer of the start of the process | Информировать клиент о начале процесса |
| UpdateProgress | Update progress status on client | Обновить на клиенте состояние прогресса |
| FinishProgress | Сomplete progress on the client | Завершить на клиенте прогресс |

For progress bar you can use IDisposable Progress<(int? percent, string Title,string message)>

### OnCLient
```C#
using 
@inject BlazorNotifierClientService NotifiService

    async Task TestClientProgress()
    {

        using var progress = new BlazorNotifierProgress(NotifiService);

        await SomeMethod(progress);

        progress.Report((null,"Finish","Last method"));
        await Task.Delay(3000);

    }

    async Task SomeMethod(IProgress<(int?,string, string)> progress)
    {
        for (var i = 0; i < 101; i++)
        {
            if(i>30 && i <50)
                progress.Report((null,$"Title {i}", $"intermediate"));
            else
                progress.Report((i,$"Title {i}", $"Message {i}"));
            await Task.Delay(300);
        }
            

    }
```

#### OnServer
```C#
    using var progress = new BlazorNotifierProgress(UserId, _Notification);

    var count = 10;
    for (var i = 1; i <= count; i++)
    {
        progress.Report((val,  $"Step {i}", $"Test message {i}"));

        //long operation
        await Task.Delay(1000);
    }

```

to set progress bar as intermediate - set percent as null;

## How Use

### 1 Create Api project .NET Core 3.1
### 2 Install package Microsoft.AspNetCore.SignalR to API
### 3 Install package BlazoreNotifier to API
### 4 Api Properties - > LainchSettings -> sslPort ->change in to 44303
### 5 Edit Api Startup.cs
  5.1 add service
  ```C#
              services.AddCors(
                o =>
                {
                    o.AddPolicy("CorsPolicy", policy =>
                    {
                        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    });
                });
            services.AddSignalR();
  ```
  5.2 edit Configure
  ```C#
  app.UseCors("CorsPolicy");
  app.UseEndpoints(endpoints =>
  {
    endpoints.MapHub<NotificationHub>("/notificationhub");
    endpoints.MapControllers();
  });
  ```
  5.3 add conroller to api with name NotificationsController
  
  sample
  ```C#
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _HubContext;

        public NotificationsController(IHubContext<NotificationHub> hubContext)
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
        /// <summary>
        /// Получает информационное сообщение и рассылает его пользователям
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <returns></returns>
        [HttpPost("Log")]
        public async Task<IActionResult> SendLog(BlazorNotifierMessage message)
        {
            try
            {
                var id = message.FromUserId;
                message.FromUserId = string.Empty;

                if (message.IsPrivate)
                    await _HubContext.Clients.Client(id).SendAsync("Log", message);
                else
                    await _HubContext.Clients.All.SendAsync("Log", message);

                return Ok("Notification has been sent succesfully!");
            }
            catch (Exception e)
            {
                return BadRequest($"Error to send notification: {e.Message}");
            }

        }
    }

  ```
### 6 Install package BlazorNotifier to CLient 
### 7 Edit Client Programm.cs
 add service
 ```C#
             builder.Services.AddScoped<BlazorNotifierClientService>();
 ```
### 8 Edit MainLayout and Index.html on client
add Notifier section in MainLayout
```C#
        <BlazorNotifier.Components.NotifierArea />
```
add if you want <BlazorNotifier.Components.NotifierRouter/>
use it to see api-connection status

in Index.html add
```html
 <link rel="stylesheet" href="_content/BlazorNotifier/css/NotifierStyles.css">
```

### 9 Add LogSection if you want or create new
sample
```C#
@page "/LogPage"

<h3>Logs</h3>
<BlazorNotifier.Components.Log/>
```

### 10 Use in page like this
```C#
@page "/"
@using BlazorNotifier.Services.Implementations
@inject BlazorNotifierClientService NotifiService

<div style="padding: 10px 10px 10px 10px;line-height: 0.5">
@foreach (var e in Notifi.Events.OrderByDescending(i => i.Key))
    {
        <p >@e.Key : @e.Value.Type - @e.Value.Title</p>
    }

</div>
@code
{
    protected override void OnInitialized()
    {
        NotifiService.OnChange += StateHasChanged;
    }

}
```

you can take
Notifi.events to show Time -> key, and message

### 11 Add Service to Server
```C#
      services.AddSingleton<BlazorNotifierServerService>();
```

### Send message

From Client you mast send ConnectionId to controller
```C#
@inject BlazorNotifierClientService NotifiService
@inject HttpClient client


var result = await client.GetAsync($"NotificationTest/GetSomeData/{NotifiService.UserId}");
```
in Server conroller add to constructor BlazorNotifierServerService notification

just call to api service
```C#
        await notification.SendNotificationAsync(new BlazorNotifierMessage {Title = $"Step {i}", FromUserId = UserId, Type = BlazorNotifierType.Info});
```

### Send Debug Message On CLient
```C#
@inject BlazorNotifierClientService Notifier

Notifier.SendNotification("Console Debug Test", BlazorNotifierType.Debug);
```
Сообщение отобразится в консоли


