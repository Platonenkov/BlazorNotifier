using System;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorNotifier.Services;
using BlazorNotifier.Services.Implementations;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorNotificationTemplate.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddNotifierService(
                o =>
                {
                    o.ServiceAddress = "https://localhost:44303";
                    o.HubName = "notificationhub";
                    o.ControllerApiPath = "api/notifications";
                });
            await builder.Build().RunAsync();
        }
    }
}
