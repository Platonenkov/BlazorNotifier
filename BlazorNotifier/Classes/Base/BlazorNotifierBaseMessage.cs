using System;

namespace BlazorNotifier.Classes.Base
{
    public abstract class BlazorNotifierBaseMessage
    {
        /// <summary> Сообщение </summary>
        public string Title { get; set; }
        /// <summary> кому будет направлено </summary>
        public string ToUserId { get; set; }
        /// <summary> Время формирования сообщения </summary>
        public DateTime Time { get; set; } = DateTime.Now;
        /// <summary> Id сообщения </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}