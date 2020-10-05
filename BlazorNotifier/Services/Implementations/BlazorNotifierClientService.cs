using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlazorNotifier.Classes;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorNotifier.Services.Implementations
{
    public class BlazorNotifierClientService
    {
        // Lets components receive change notifications
        // Could have whatever granularity you want (more events, hierarchy...)
        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();

        public BlazorNotifierClientService()
        {
            ConnectToServerAsync();
        }
        #region Notification
        public Dictionary<DateTime?, string> events = new Dictionary<DateTime?, string>();

        string url = "https://localhost:44303/notificationhub";

        public ObservableCollection<BlazorNotifierMessage> notifications = new ObservableCollection<BlazorNotifierMessage>();
        HubConnection _Connection = null;

        public string UserId => _Connection.ConnectionId;

        public string StatusColor { get; set; } = "background-color: red; width: 20px; height: 20px; border-radius: 50%";
        private bool _IsConnected;
        public bool IsConnected
        {
            get => _IsConnected;
            set
            {
                _IsConnected = value;
                StatusColor = value ? "background-color: green; width: 20px; height: 20px; border-radius: 50%" : "background-color: red; width: 20px; height: 20px; border-radius: 50%";
                NotifyStateChanged();
            }
        }

        private string _ConnectionStatus = "Closed";
        public string connectionStatus
        {
            get => _ConnectionStatus;
            set
            {
                _ConnectionStatus = value;
                NotifyStateChanged();
            }
        }


        void ShowNotification(BlazorNotifierMessage message)
        {
            events.Add(message.Time, $"{message.Type}: {message.Title}");
            NotifyStateChanged();
        }
        private async void ConnectToServerAsync()
        {
            notifications.CollectionChanged += async (sender, e) =>
            {
                if (!(sender is ObservableCollection<BlazorNotifierMessage>))
                {
                    return;
                }

                foreach (BlazorNotifierMessage message in e.NewItems)
                {
                    ShowNotification(message);
                }
            };
            _Connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            await _Connection.StartAsync();
            IsConnected = true;

            connectionStatus = "Connected";

            _Connection.Closed += async (s) =>
            {
                IsConnected = false;
                connectionStatus = "Disconnected";
                await _Connection.StartAsync();
                IsConnected = true;
                connectionStatus = "Connected";
            };

            _Connection.On<BlazorNotifierMessage>("notification", m =>
            {
                notifications.Add(m);
                NotifyStateChanged();
            });
        }

        #endregion
    }
}
