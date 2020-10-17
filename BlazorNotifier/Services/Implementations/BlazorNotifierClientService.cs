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
    public class BlazorNotifierClientService
    {
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
        public Dictionary<DateTime, string> Events = new Dictionary<DateTime, string>();
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
        void LogNotification(BlazorNotifierMessage message)
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

        public void CloseMessage(Guid id)
        {
            var message = Notifications.FirstOrDefault(m => m.Id == id);
            if (message is null)
            {
                Console.WriteLine($"NOT FIND {id}");
                OnChange?.Invoke();
                return;
            }
            Notifications.Remove(message);
            OnChange?.Invoke();
        }
        public void CloseProgress(Guid id)
        {
            var message = ProgressMessages.FirstOrDefault(m => m.Id == id);
            if (message is null)
            {
                OnChange?.Invoke();
                return;
            }
            ProgressMessages.Remove(message);
            OnChange?.Invoke();
        }

        //private async Task StartNotifierTimer(BlazorNotifierMessage message)
        //{
        //    await Task.Run(
        //        () =>
        //        {
        //            try
        //            {
        //                var timer = new Timer(message.TimeOut * 1000);
        //                timer.Elapsed += (Sender, Args) =>
        //                {
        //                    Notifications.Remove(message);
        //                    NotifyStateChanged();
        //                };
        //                timer.AutoReset = false;
        //                timer.Start();

        //            }
        //            catch (Exception e)
        //            {
        //                Console.WriteLine($"Ошибка таймера {e.Message}");
        //            }
        //        });
        //}
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
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        foreach (BlazorNotifierMessage message in e.NewItems)
                        {
                            Console.WriteLine($"сообщение добавлено {message.Id} | {message.Title}");
                            LogNotification(message);
                        }
                        break;
                    }
                    case NotifyCollectionChangedAction.Move:
                    {
                        foreach (BlazorNotifierMessage message in e.OldItems)
                        {
                            Console.WriteLine($"Move сообщение {message.Id} | {message.Title}");
                        }
                        break;
                    }

                    case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (BlazorNotifierMessage message in e.OldItems)
                        {
                            Console.WriteLine($"Remove сообщение {message.Id} | {message.Title}");
                        }
                        break;
                    }
                    case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (BlazorNotifierMessage message in e.OldItems)
                        {
                            Console.WriteLine($"Replace сообщение {message.Id} | {message.Title}");
                        }
                        break;
                    }

                    case NotifyCollectionChangedAction.Reset:
                    {
                        foreach (BlazorNotifierMessage message in e.OldItems)
                        {
                            Console.WriteLine($"Reset сообщение {message.Id} | {message.Title}");
                        }
                        break;
                    }

                    default: throw new ArgumentOutOfRangeException();
                }
            }
            else if (sender is ObservableCollection<BlazorNotifierProgressMessage>)
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        foreach (BlazorNotifierProgressMessage message in e.NewItems)
                        {
                                Console.WriteLine($"Прогресс добавлен{message.Id} | {message.Title}");
                                LogNotification(new BlazorNotifierMessage { Title = $"{message.Title}: {message.Message}: {message.Percent}" });
                        }
                        break;
                    }
                    case NotifyCollectionChangedAction.Move: break;
                    case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (BlazorNotifierProgressMessage message in e.NewItems)
                        {
                                Console.WriteLine($"Удален Прогресс {message.Id} | {message.Title}");
                        }
                        break;
                    }
                    case NotifyCollectionChangedAction.Replace: break;
                    case NotifyCollectionChangedAction.Reset: break;
                    default: throw new ArgumentOutOfRangeException();
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
