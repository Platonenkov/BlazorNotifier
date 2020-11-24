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
        private readonly string _ServerAddress;

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
        public BlazorNotifierClientService(NotifierServiceOptions options)
        {
            _ServerAddress = options.ServiceAddress+$"/{options.HubName}";
            if(string.IsNullOrWhiteSpace(options.ServiceAddress))
                SetNotifications();
            else
                ConnectToServerAsync();
        }
        #region Notification
        /// <summary> Словарь событий время/сообщение </summary>
        public Dictionary<DateTime, BlazorNotifierMessage> Events { get; }= new Dictionary<DateTime, BlazorNotifierMessage>();
        /// <summary> адрес сервиса api </summary>
        /// <summary> Коллекция уведомительных сообщений </summary>
        public Notifications Notification { get; }= new Notifications();

        public int CountOfNotifications => Notification.Count;
        /// <summary> хаб для подключения к api </summary>
        HubConnection _Connection = null;
        /// <summary> Id клиента для отправки сообщений </summary>
        public string UserId => _Connection.ConnectionId;


        #region Состояния подключения к сервису

        /// <summary> стиль статуса при подключенном состоянии </summary>
        public static string StatusStyleOnOpen { get; set; } = "background-color: green; width: 20px; height: 20px; border-radius: 50%";
        /// <summary> стиль статуса при отсутствии соединения </summary>
        public static string StatusStyleOnClose { get; set; } = "background-color: red; width: 20px; height: 20px; border-radius: 50%";
        /// <summary> стиль статуса подключения к сервису </summary>
        public string StatusStyle { get; set; } = StatusStyleOnClose;

        private bool _IsConnected;
        /// <summary> состояния подключения к сервису </summary>
        public bool IsConnected
        {
            get => _IsConnected;
            set
            {
                _IsConnected = value;
                StatusStyle = value ? StatusStyleOnOpen : StatusStyleOnClose;
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

        private void SetNotifications()
        {
            //Добавляя новое сообщение в коллекцию - записываем сообщение в события
            Notification.OnChange += NotifyChanged;
            Notification.OnAddNewMessage += async m => await StartNotifierTimer(m);

        }

        /// <summary> Подключение к серверу </summary>
        private async void ConnectToServerAsync()
        {
            SetNotifications();

            _Connection = new HubConnectionBuilder()
                .WithUrl(_ServerAddress)
                .Build();

            await _Connection.StartAsync();
            IsConnected = true;

            ConnectionStatus = "Connected";

            //автоматически присоединяемся к серверу при разъединении
            _Connection.Closed += async m => await OnConnectionClosed(m);
            //выполняем при получении сообщений клиентом
            _Connection.On<BlazorNotifierMessage>("notification", async m=>await DoWhenClientGetNewMessage(m));
            _Connection.On<BlazorNotifierMessage>("Log", DoWhenGetLogMessage);
            _Connection.On<BlazorNotifierProgressMessage>("ProgressFinish", DoWhenProgressFinish);
            _Connection.On<BlazorNotifierProgressMessage>("ProgressUpdate", DoWhenProgressUpdate);
            _Connection.On<BlazorNotifierProgressMessage>("ProgressStart", async m => await DoWhenProgressStart(m));
        }

        /// <summary>
        /// Автоматически присоединяемся к серверу при разъединении
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private async Task OnConnectionClosed(Exception s)
        {
            await LogNotification($"Disconnect from server with message: {s.Message}");
            IsConnected = false;
            ConnectionStatus = "Disconnected";
            Notification.ClearProgress();
            await _Connection.StartAsync();
            IsConnected = true;
            ConnectionStatus = "Connected";
        }

        #endregion

        #region PROGRESS

        private void DoWhenProgressUpdate(BlazorNotifierProgressMessage Message) => Notification.UpdateProgress(Message);

        private async Task DoWhenProgressStart(BlazorNotifierProgressMessage Progress)
        {
            Notification.AddProgress(Progress);
            await LogNotification(new BlazorNotifierMessage
            {
                Title = $"{Progress.Title}: {Progress.Message}",
                Time = Progress.Time,
                Id = Progress.Id,
                Type = BlazorNotifierType.Progress
            });
        }

        private void DoWhenProgressFinish(BlazorNotifierProgressMessage Progress) => Notification.RemoveProgress(Progress.Id);
        /// <summary>
        /// Закрыть прогресс бар
        /// </summary>
        /// <param name="id">id окна</param>
        public void CloseProgress(Guid id) => Notification.RemoveProgress(id);
        /// <summary>
        /// показать прогресс или обновить данные
        /// </summary>
        /// <param name="progress">прогресс</param>
        public async Task SendOrUpdateProgress(BlazorNotifierProgressMessage progress)
        {
            if (Notification.ContainsProgress(progress.Id))
            {
                Notification.UpdateProgress(progress);
                NotifyChanged();
            }
            else
            {
                Notification.AddProgress(progress);
                await LogNotification(new BlazorNotifierMessage
                {
                    Title = $"{progress.Title}: {progress.Message}",
                    Time = progress.Time,
                    Id = progress.Id,
                    Type = BlazorNotifierType.Progress
                });
                NotifyChanged();
            }
        }

        #endregion

        #region Messages

        /// <summary>
        /// Выполняем при получении сообщений клиентом
        /// </summary>
        /// <param name="message">сообщение</param>
        private async Task DoWhenClientGetNewMessage(BlazorNotifierMessage message) => await SendNotification(message);
        private async Task DoWhenGetLogMessage(BlazorNotifierMessage message) => await LogNotification(message);
        /// <summary>
        /// закрыть сообщение
        /// </summary>
        /// <param name="id">id сообщения</param>
        public void CloseMessage(Guid id) => Notification.RemoveMessage(id);
        /// <summary>
        /// показать сообщение
        /// </summary>
        /// <param name="message">сообщение</param>
        public async Task SendNotification(BlazorNotifierMessage message)
        {
            Notification.AddMessage(message);
            await LogNotification(message);
        }
        /// <summary>
        /// показать сообщение
        /// </summary>
        /// <param name="text">сообщение</param>
        /// <param name="type">тип</param>
        public async Task SendNotification(string text, BlazorNotifierType type = BlazorNotifierType.Info)
        {
            if (type == BlazorNotifierType.Debug)
            {
                Console.WriteLine($"Debug: {text}");
                return;
            }

            var message = new BlazorNotifierMessage { Title = text, Type = type };
            await SendNotification(message);
        }

        #endregion

        #region Log

        /// <summary> Запись событий </summary>
        /// <param name="message">сообщение</param>
        public async Task LogNotification(BlazorNotifierMessage message)
        {
            try
            {
                Events.Add(DateTime.Now, message);
                NotifyChanged();
            }
            catch (ArgumentException)
            {
                await Task.Delay(1000);
                await LogNotification(message);
            }
        }
        /// <summary> Запись событий </summary>
        /// <param name="text">сообщение</param>
        /// <param name="type">тип события</param>
        public async Task LogNotification(string text, BlazorNotifierType type = BlazorNotifierType.Info)
        {
            if (type == BlazorNotifierType.Debug)
            {
                Console.WriteLine($"Debug: {text}");
                return;
            }

            var message = new BlazorNotifierMessage { Title = text, Type = type };
            await LogNotification(message);
        }


        #endregion


        /// <summary>
        /// Удаляем сообщение из сервиса по окончанию таймера
        /// </summary>
        /// <param name="message">сообщения</param>
        private async Task StartNotifierTimer(BlazorNotifierMessage message)
        {
            await Task.Run(async () =>
            {
                await Task.Delay(message.TimeOut * 1000-1000 );
                Notification.RemoveMessage(message);
                NotifyChanged();
            });
        }

    }
}
