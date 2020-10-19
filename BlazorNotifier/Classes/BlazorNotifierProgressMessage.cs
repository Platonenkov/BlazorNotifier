using System;
using BlazorNotifier.Classes.Base;

namespace BlazorNotifier.Classes
{
    public class BlazorNotifierProgressMessage : BlazorNotifierBaseMessage
    {
        public string Message { get;  set;}
        public int? Percent { get;  set;}

        public BlazorNotifierProgressMessage(Guid id, string title, string message, int? percent, string toUser)
        {
            Id = id;
            Title = title;
            Message = message;
            Percent = percent;
            ToUserId = toUser;
        }

        public BlazorNotifierProgressMessage()
        {
            
        }
        public void Update(BlazorNotifierProgressMessage message)
        {
            Id = message.Id;
            if(!string.IsNullOrWhiteSpace(message.Title))
                Title = message.Title;
            if (!string.IsNullOrWhiteSpace(message.Message))
                Message = message.Message;
            Percent = message.Percent;
        }

    }
}