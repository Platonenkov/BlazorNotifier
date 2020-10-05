using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace BlazorNotifier
{
    public class NotificationHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
    }
}
