using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using BlazorNotifier.Annotations;
using BlazorNotifier.Classes;
using BlazorNotifier.Classes.Base;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorNotifier.Services.Implementations
{
    public class BlazorNotifierClientService
    {
        #region События

        /// <summary> Событие при изменениях в сервисе </summary>
        public event Action OnChange;
        /// <summary> Событие при изменении статуса подключения </summary>
        public event Action OnConnectionStatusChange; 
        /// <summary> Событие при изменении статуса подключения </summary>
        private void NotifyChanged() => OnChange?.Invoke();
        /// <summary> Событие при изменении статуса подключения </summary>
        private void NotifyConnectionChanged() => OnConnectionStatusChange?.Invoke();

        #endregion

        /// <summary> Конструктор с инициализатором сервиса </summary>
        public BlazorNotifierClientService()
        {
            ConnectToServerAsync();
        }
        #region Notification
        /// <summary> Словарь событий время/сообщение </summary>
        public Dictionary<Guid, (DateTime date,string message)> Events = new Dictionary<Guid, (DateTime date, string message)>();
        /// <summary> адрес сервиса api </summary>
        string Url = "https://localhost:44303/notificationhub";
        /// <summary> Коллекция уведомительных сообщений </summary>
        public Notifications Notification { get; }= new Notifications();

        public int CountOfNotifications => Notification.Count;
        /// <summary> хаб для подключения к api </summary>
        HubConnection _Connection = null;
        /// <summary> Id клиента для отправки сообщений </summary>
        public string UserId => _Connection.ConnectionId;
        /// <summary> стиль статуса подключения к сервису </summary>

        #region Состояния подключения к сервису
        public string StatusColor { get; set; } = "background-color: red; width: 20px; height: 20px; border-radius: 50%";

        private bool _IsConnected;
        /// <summary> состояния подключения к сервису </summary>
        public bool IsConnected
        {
            get => _IsConnected;
            set
            {
                _IsConnected = value;
                StatusColor = value ? "background-color: green; width: 20px; height: 20px; border-radius: 50%" : "background-color: red; width: 20px; height: 20px; border-radius: 50%";
                NotifyConnectionChanged();
                NotifyChanged();
            }
        }

        #endregion

        #region Состояние канала передачи данных

        private string _ConnectionStatus = "Closed";
        /// <summary> Состояния канала передачи данных в тестовом виде </summary>
        public string ConnectionStatus
        {
            get => _ConnectionStatus;
            set
            {
                _ConnectionStatus = value;
                NotifyChanged();
            }
        }

        #endregion

        /// <summary> Подключение к серверу </summary>
        private async void ConnectToServerAsync()
        {
            //Добавляя новое сообщение в коллекцию - записываем сообщение в события
            Notification.OnChange += NotifyChanged;
            Notification.OnAddNewMessage += StartNotifierTimer;

            _Connection = new HubConnectionBuilder()
                .WithUrl(Url)
                .Build();

            await _Connection.StartAsync();
            IsConnected = true;

            ConnectionStatus = "Connected";

            //автоматически присоединяемся к серверу при разъединении
            _Connection.Closed += OnConnectionClosed;
            //выполняем при получении сообщений клиентом
            _Connection.On<BlazorNotifierMessage>("notification", DoWhenClientGetNewMessage);
            _Connection.On<BlazorNotifierProgressMessage>("ProgressFinish", DoWhenProgressFinish);
            _Connection.On<BlazorNotifierProgressMessage>("ProgressUpdate", DoWhenProgressUpdate);
            _Connection.On<BlazorNotifierProgressMessage>("ProgressStart", DoWhenProgressStart);
        }

        /// <summary>
        /// Автоматически присоединяемся к серверу при разъединении
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private async Task OnConnectionClosed(Exception s)
        {
            IsConnected = false;
            ConnectionStatus = "Disconnected";
            await _Connection.StartAsync();
            IsConnected = true;
            ConnectionStatus = "Connected";
        }

        /// <summary> Запись событий </summary>
        /// <param name="message">сообщение</param>
        void LogNotification(BlazorNotifierMessage message)
        {
            Events.Add(message.Id,(message.Time, $"{message.Type} - {message.Title}"));
            NotifyChanged();
        }

        #region PROGRESS

        private void DoWhenProgressUpdate(BlazorNotifierProgressMessage Message) => Notification.UpdateProgress(Message);

        private void DoWhenProgressStart(BlazorNotifierProgressMessage Progress)
        {
            Notification.AddProgress(Progress);
            LogNotification(new BlazorNotifierMessage { Title = $"{Progress.Title}: {Progress.Message}", Time = Progress.Time, 
                Id = Progress.Id, Type = BlazorNotifierType.Progress });
        }

        private void DoWhenProgressFinish(BlazorNotifierProgressMessage Progress) => Notification.RemoveProgress(Progress.Id);

        #endregion

        #region Messages

        /// <summary>
        /// Выполняем при получении сообщений клиентом
        /// </summary>
        /// <param name="message">сообщение</param>
        private void DoWhenClientGetNewMessage(BlazorNotifierMessage message) => SendNotification(message);

        public void CloseMessage(Guid id) => Notification.RemoveMessage(id);
        public void CloseProgress(Guid id) => Notification.RemoveProgress(id);

        #endregion

        #endregion

        #region Уведомления на клиенте

        public void SendNotification(BlazorNotifierMessage message)
        {
            Notification.AddMessage(message);
            LogNotification(message);
        }
        public void SendNotification(string text, BlazorNotifierType type = BlazorNotifierType.Info)
        {
            if (type == BlazorNotifierType.Debug)
            {
                Console.WriteLine($"Debug: {text}");
                return;
            }

            var message = new BlazorNotifierMessage {Title = text, Type = type};
            SendNotification(message);
        }

        #endregion
        private void StartNotifierTimer(BlazorNotifierMessage message)
        {
            Task.Run(async () =>
            {
                await Task.Delay(message.TimeOut * 1000+ 1000);
                Notification.RemoveMessage(message);
                NotifyChanged();
            });
        }

    }
}
