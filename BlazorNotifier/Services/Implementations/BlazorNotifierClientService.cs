using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using BlazorNotifier.Classes;
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
        /// <summary> Событие при изменениях в сервисе </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();
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
        public Dictionary<DateTime?, string> Events = new Dictionary<DateTime?, string>();
        /// <summary> адрес сервиса api </summary>
        string Url = "https://localhost:44303/notificationhub";
        /// <summary> Коллекция уведомительных сообщений </summary>
        public ObservableCollection<BlazorNotifierMessage> Notifications = new ObservableCollection<BlazorNotifierMessage>();
        /// <summary> хаб для подключения к api </summary>
        HubConnection _Connection = null;
        /// <summary> Id клиента для отправки сообщений </summary>
        public string UserId => _Connection.ConnectionId;
        /// <summary> стиль статуса подключения к сервису </summary>
        public string StatusColor { get; set; } = "background-color: red; width: 20px; height: 20px; border-radius: 50%";

        #region Состояния подключения к сервису

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
                NotifyStateChanged();
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
                NotifyStateChanged();
            }
        }

        #endregion

        /// <summary> Запись событий </summary>
        /// <param name="message">сообщение</param>
        void ShowNotification(BlazorNotifierMessage message)
        {
            Events.Add(message.Time, $"{message.Type}: {message.Title}");
            NotifyStateChanged();
        }

        /// <summary> Подключение к серверу </summary>
        private async void ConnectToServerAsync()
        {
            //Добавляя новое сообщение в коллекцию - записываем сообщение в события
            Notifications.CollectionChanged += OnNotificationsCollectionChanged;

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
        }
        /// <summary>
        /// Выполняем при получении сообщений клиентом
        /// </summary>
        /// <param name="message">сообщение</param>
        private void DoWhenClientGetNewMessage(BlazorNotifierMessage message)
        {
            Notifications.Add(message);
            NotifyStateChanged();
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
        /// <summary>
        /// Добавляя новое сообщение в коллекцию - записываем сообщение в события
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNotificationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(sender is ObservableCollection<BlazorNotifierMessage>))
            {
                return;
            }

            foreach (BlazorNotifierMessage message in e.NewItems)
            {
                ShowNotification(message);
            }
        }

        #endregion

        #region Уведомления на клиенте

        public void SendNotification(BlazorNotifierMessage message)
        {
            Notifications.Add(message);
        }
        public void SendNotification(string text, BlazorNotifierType type = BlazorNotifierType.Info)
        {
            if (type == BlazorNotifierType.Debug)
            {
                Console.WriteLine($"Debug: {text}");
                return;
            }

            Notifications.Add(new BlazorNotifierMessage{Title = text, Type = type});
        }

        #endregion
    }
}
