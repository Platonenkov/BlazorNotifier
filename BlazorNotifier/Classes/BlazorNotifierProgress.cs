using System;
using BlazorNotifier.Services.Implementations;

namespace BlazorNotifier.Classes
{
    public class BlazorNotifierProgress : Progress<(int? percent, string Title, string Message)>, IDisposable
    {
        private readonly object _Service;
        private Guid ProgressID { get; }
        private string UserId { get; }
        private bool IsFirstMessage { get; set; } = true;
        public BlazorNotifierProgress(Action<(int? percent, string Title, string Message)> handler, string userId, BlazorNotifierServerService service) : base(handler) 
        {
            _Service = service;
            UserId = userId;
            ProgressID = Guid.NewGuid();
        }
        public BlazorNotifierProgress(string userId, BlazorNotifierServerService service)
        {
            UserId = userId;
            ProgressID = Guid.NewGuid();
            _Service = service;
        }
        public BlazorNotifierProgress(Action<(int? percent, string Title, string Message)> handler, BlazorNotifierClientService service) : base(handler)
        {
            _Service = service;
            ProgressID = Guid.NewGuid();
        }
        public BlazorNotifierProgress(BlazorNotifierClientService service)
        {
            ProgressID = Guid.NewGuid();
            _Service = service;
        }

        #region Overrides of Progress<(int?,string,string)>

        protected override async void OnReport((int? percent, string Title, string Message) value)
        {
            switch (_Service)
            {
                case BlazorNotifierServerService server when IsFirstMessage:
                    IsFirstMessage = false;
                    await server.AddNewProgress(new BlazorNotifierProgressMessage(ProgressID, value.Title, value.Message, value.percent, UserId));
                    break;
                case BlazorNotifierServerService server:
                    await server.UpdateProgress(new BlazorNotifierProgressMessage(ProgressID, value.Title, value.Message, value.percent, UserId));
                    break;
                case BlazorNotifierClientService client: 
                    await client.SendOrUpdateProgress(new BlazorNotifierProgressMessage(ProgressID, value.Title, value.Message, value.percent, client.UserId));
                    break;
            }

            base.OnReport(value);
        }

        #endregion

        public void Report((int? percent, string Title, string Message) value)=> OnReport(value);

        public async void Dispose()
        {
            switch (_Service)
            {
                case BlazorNotifierServerService server: await server.FinishProgress(new BlazorNotifierProgressMessage{ Id = ProgressID, ToUserId = UserId });
                    break;
                case BlazorNotifierClientService client: client.CloseProgress(ProgressID);
                    break;
            }
        }

    }
}