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
        public void Update(BlazorNotifierProgressMessage Message)
        {
            Id = Message.Id;
            Title = Message.Title;
            this.Message = Message.Message;
            Percent = Message.Percent;
        }

    }
}