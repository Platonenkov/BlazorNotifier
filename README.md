# BlazorNotifier
Blazor notifier from server to client by SignalR

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
  
### 6 Install package BlazorNotifier to CLient
### 7 Edit Client Programm.cs
 add service
 ```C#
             builder.Services.AddScoped<BlazorNotifierClientService>();
 ```
### 8 Edit MainLayout (Not requred)
add Notifier section
```C#
        <BlazorNotifier.Components.NotifierRouter />
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
    @foreach (var (date, message) in Notifi.events.OrderByDescending(i => i.Key))
    {
        <p >@date: @message</p>
    }
</div>
@code
{
    protected override void OnInitialized()
    {
        Notifi.OnChange += StateHasChanged;
    }

}
```

you can take
Notifi.events to show Datetime -> key, and type:message -> value

ore 
notifications - > collection of BlazorNotifierMessage

```C#
    public class BlazorNotifierMessage
    {
        public string Title { get; set; }
        public DateTime? Time { get; set; } = DateTime.Now;
        public BlazorNotifierType Type { get; set; } = BlazorNotifierType.none;
        public bool IsPrivate { get; set; } = true;
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
    }

    public enum BlazorNotifierType
    {
        none,
        Debug,
        Success,
        Info,
        Warning,
        Error
    }
```

### 11 Add Service to Server
```C#
      services.AddSingleton<BlazorNotifierServerService>();
```

### Send message

From Client you mast send ConnectionId to controller
```C#
        var result = await client.GetAsync($"NotificationTest/GetSomeData/{NotifiService.UserId}");
```
and in conroller just call to api service
add to constructor BlazorNotifierServerService notification
```C#
        await _Notification.SendNotificationAsync(new BlazorNotifierMessage {Title = $"Step {i}", FromUserId = UserId, Type = BlazorNotifierType.Info});
```
 
