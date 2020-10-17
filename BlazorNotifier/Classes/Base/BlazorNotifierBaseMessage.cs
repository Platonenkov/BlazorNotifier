using System;

namespace BlazorNotifier.Classes.Base
{
    public abstract class BlazorNotifierBaseMessage
    {
        /// <summary> Сообщение </summary>
        public string Title { get; set; }
        /// <summary> кому будет направлено </summary>
        public string ToUserId { get; set; }

        private Guid _Id = Guid.NewGuid();

        /// <summary> Id сообщения </summary>
        public Guid Id
        {
            get => _Id;
            set
            {
                Console.WriteLine($"old {_Id} for {Title}");
                _Id = value;
                Console.WriteLine($"new {_Id} for {Title}");
            }
        }
    }
}