using System;

namespace BlazorNotifier.Classes
{
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
}
