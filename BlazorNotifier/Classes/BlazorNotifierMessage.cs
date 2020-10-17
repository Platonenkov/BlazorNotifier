using System;
using System.Threading.Tasks;
using BlazorNotifier.Classes.Base;

namespace BlazorNotifier.Classes
{
    public class BlazorNotifierMessage : BlazorNotifierBaseMessage
    {
        /// <summary> время создания сообщения </summary>
        public DateTime Time { get; set; } = DateTime.Now;
        /// <summary> Тип сообщения </summary>
        public BlazorNotifierType Type { get; set; } = BlazorNotifierType.none;
        /// <summary> Приватное или всем пользователям </summary>
        public bool IsPrivate { get; set; } = true;
        /// <summary> от кого пришло сообщение </summary>
        public string FromUserId { get; set; }

        /// <summary> Время показа пользователю в секундах </summary>
        public int TimeOut { get; set; } = 5;

    }
}
