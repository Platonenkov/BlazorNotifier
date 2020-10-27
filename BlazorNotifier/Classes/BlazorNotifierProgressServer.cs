using System;
using BlazorNotifier.Services.Implementations;

namespace BlazorNotifier.Classes
{
    public class BlazorNotifierProgressServer : Progress<(int? percent, string Title, string Message)>, IDisposable
    {
        private readonly BlazorNotifierServerService _Service;
        private Guid ProgressID { get; }
        private string UserId { get; }
        private bool IsFirstMessage { get; set; } = true;
        public BlazorNotifierProgressServer(Action<(int? percent, string Title, string Message)> handler, string userId, BlazorNotifierServerService service) : base(handler) 
        {
            _Service = service;
            UserId = userId;
            ProgressID = Guid.NewGuid();
        }

        #region Overrides of Progress<(int?,string,string)>

        protected override async void OnReport((int? percent, string Title, string Message) value)
        {
            if (IsFirstMessage)
            {
                IsFirstMessage = false;
                await _Service.AddNewProgress(new BlazorNotifierProgressMessage(ProgressID, value.Title, value.Message, value.percent, UserId));
            }
            else
                await _Service.UpdateProgress(new BlazorNotifierProgressMessage(ProgressID, value.Title, value.Message , value.percent, UserId));

            base.OnReport(value);
        }

        #endregion

        public void Report((int? percent, string Title, string Message) value)=> OnReport(value);

        public async void Dispose()
        {
            await _Service.FinishProgress(new BlazorNotifierProgressMessage{ Id = ProgressID, ToUserId = UserId });
        }
        public BlazorNotifierProgressServer(string userId, BlazorNotifierServerService service)
        {
            UserId = userId;
            ProgressID = Guid.NewGuid();
            _Service = service;
        }

    }
    public class BlazorNotifierProgressClient : Progress<(int? percent, string Title, string Message)>, IDisposable
    {
        private readonly BlazorNotifierClientService _Service;
        private Guid ProgressID { get; }
        public BlazorNotifierProgressClient(Action<(int? percent, string Title, string Message)> handler, BlazorNotifierClientService service) : base(handler) 
        {
            _Service = service;
            ProgressID = Guid.NewGuid();
        }
        public BlazorNotifierProgressClient(BlazorNotifierClientService service)
        {
            ProgressID = Guid.NewGuid();
            _Service = service;
        }

        #region Overrides of Progress<(int?,string,string)>

        protected override void OnReport((int? percent, string Title, string Message) value)
        {
            _Service.SendOrUpdateProgress(new BlazorNotifierProgressMessage(ProgressID, value.Title, value.Message, value.percent, _Service.UserId));
            base.OnReport(value);
        }

        #endregion

        public void Report((int? percent, string Title, string Message) value) => OnReport(value);

        public void Dispose()
        {
            _Service.CloseProgress(ProgressID);
        }

    }
}