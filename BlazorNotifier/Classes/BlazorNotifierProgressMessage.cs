using System;

namespace BlazorNotifier.Classes
{
    public class BlazorNotifierProgressMessage
    {
        public string Title { get;  set; }
        public string Message { get;  set;}
        public int? Percent { get;  set;}
        public string ToUserId { get;  set; }
        public Guid Id { get;  set; }

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