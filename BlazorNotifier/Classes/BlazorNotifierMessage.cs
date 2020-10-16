using System;
using System.Threading.Tasks;
using BlazorNotifier.Classes.Base;

namespace BlazorNotifier.Classes
{
    public class BlazorNotifierMessage : BlazorNotifierBaseMessage
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public BlazorNotifierType Type { get; set; } = BlazorNotifierType.none;
        public bool IsPrivate { get; set; } = true;
        public string FromUserId { get; set; }
    }
}
