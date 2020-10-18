# BlazorNotifier
Blazor notifier from server to client by SignalR

Install-Package BlazorNotifier -Version 2.0.0

![Demo](https://github.com/Platonenkov/BlazorNotifier/blob/main/Resources/Notifier.gif)

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
    public class NotificationsController : NotifierController
    {

        public NotificationsController(IHubContext<NotificationHub> hubContext) : base(hubContext) { }

    }

  ```
### 6 Install package BlazorNotifier to CLient 
### 7 Edit Client Programm.cs
 add service
 ```C#
             builder.Services.AddScoped<BlazorNotifierClientService>();
 ```
### 8 Edit MainLayout
add Notifier section
```C#
        <BlazorNotifier.Components.NotifierArea />
```
use it to see api-connection status

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
    @foreach (var (_, (date, message)) in Notifi.Events.OrderByDescending(i => i.Value.date))
    {
        <p >@date : @message</p>
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
Notifi.events to show id -> key, and (date, message)

ore 
notification.Messages - > collection of BlazorNotifierMessage

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

### Send Progress
On Server 
```C#
         [HttpGet("GetSomeData/{UserId}")]
        public async Task<IActionResult> GetSomeData(string UserId)
        {
            using var progress = new BlazorNotifierProgress(UserId, _Notification);

            var count = 10;
            for (var i = 1; i <= count; i++)
            {
                var val = i % 4 == 0 ? (int?)null : i * 100 / count;
                progress.Report((val, $"Step {i}", $"Test message {i}"));

                //long operation
                await Task.Delay(1000);
            }

            return Ok();
        }
```
it use IProgress<(int? Percent, string Title, string Message)>

