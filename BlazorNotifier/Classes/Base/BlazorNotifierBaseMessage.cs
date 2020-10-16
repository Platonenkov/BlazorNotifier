using System;

namespace BlazorNotifier.Classes.Base
{
    public abstract class BlazorNotifierBaseMessage
    {
        public string Title { get; set; }
        public string ToUserId { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}