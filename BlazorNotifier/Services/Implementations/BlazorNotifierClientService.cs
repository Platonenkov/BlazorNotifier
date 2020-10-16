using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using BlazorNotifier.Classes;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorNotifier.Services.Implementations
{
    public class BlazorNotifierClientService:IDisposable
    {
        private Timer Countdown;
        public event Action<object> OnShow;
        public event Action OnHide;

        #region События

        /// <summary> Событие при изменениях в сервисе </summary>
        public event Action OnChange;
        /// <summary> Событие при изменении статуса подключения </summary>
        public event Action OnConnectionStatusChange; 
        /// <summary> Событие при изменении статуса подключения </summary>
        public event Action OnProgressChange;
        /// <summary> Событие при изменениях в сервисе </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();
        /// <summary> Событие при изменении статуса подключения </summary>
        private void NotifyConnectionChanged() => OnConnectionStatusChange?.Invoke();
        /// <summary> Событие при изменении статуса подключения </summary>
        private void NotifyProgressChanged() => OnProgressChange?.Invoke();

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
            ProgressMessages.CollectionChanged += OnNotificationsCollectionChanged;

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


        #region PROGRESS

        public ObservableCollection<BlazorNotifierProgressMessage> ProgressMessages = new ObservableCollection<BlazorNotifierProgressMessage>();
        private void DoWhenProgressUpdate(BlazorNotifierProgressMessage Message)
        {
            var progress = ProgressMessages.FirstOrDefault(m => m.Id == Message.Id);
            progress?.Update(Message);
            NotifyProgressChanged();
        }

        private void DoWhenProgressStart(BlazorNotifierProgressMessage Message)
        {
            ProgressMessages.Add(Message);
            NotifyProgressChanged();
        }

        private void DoWhenProgressFinish(BlazorNotifierProgressMessage Message)
        {
            var progress = ProgressMessages.FirstOrDefault(m => m.Id == Message.Id);
            if (progress is null)
                return;
            ProgressMessages.Remove(progress);
            NotifyProgressChanged();
        }

        #endregion

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
            if (sender is ObservableCollection<BlazorNotifierMessage>)
            {
                foreach (BlazorNotifierMessage message in e.NewItems)
                {
                    ShowNotification(message);
                }
            }
            //else if( sender is ObservableCollection<BlazorNotifierProgressMessage>)
            //    foreach (BlazorNotifierProgressMessage message in e.NewItems)
            //    {
            //        ShowNotification(new BlazorNotifierMessage{Title = $"{message.Title}: {message.Message}: {message.Percent}"});
            //    }
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

        #region IDisposable

        public void Dispose()
        {
            Countdown?.Dispose();
        }

        #endregion
        public void ShowMessage(BlazorNotifierMessage message)
        {
            OnShow?.Invoke(message);
            StartCountdown();
        }
        public void ShowProgress(BlazorNotifierProgressMessage progress)
        {
            OnShow?.Invoke(progress);
            StartCountdown();
        }

        private void StartCountdown()
        {
            SetCountdown();

            if (Countdown.Enabled)
            {
                Countdown.Stop();
                Countdown.Start();
            }
            else
            {
                Countdown.Start();
            }
        }

        private void SetCountdown()
        {
            if (Countdown == null)
            {
                Countdown = new Timer(5000);
                Countdown.Elapsed += HideToast;
                Countdown.AutoReset = false;
            }
        }

        private void HideToast(object source, ElapsedEventArgs args)
        {
            OnHide?.Invoke();
        }

    }
}
