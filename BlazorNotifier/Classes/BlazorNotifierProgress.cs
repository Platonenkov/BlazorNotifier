using System;
using BlazorNotifier.Services.Implementations;

namespace BlazorNotifier.Classes
{
    public class BlazorNotifierProgress : Progress<(int? percent, string Title, string Message)>, IDisposable
    {
        private readonly BlazorNotifierServerService _Service;
        private Guid ProgressID { get; }
        private string UserId { get; }
        private bool IsFirstMessage { get; set; } = true;
        public BlazorNotifierProgress(Action<(int? percent, string Title, string Message)> handler, string userId, BlazorNotifierServerService service) : base(handler) 
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
                await _Service.AddNewProgress(new BlazorNotifierProgressMessage(ProgressID, value.Title, value.Message, value.percent, UserId));
                IsFirstMessage = false;
            }
            else
                await _Service.UpdateProgress(new BlazorNotifierProgressMessage(ProgressID, value.Title, value.Message , value.percent, UserId));

            base.OnReport(value);
        }

        #endregion

        public async void Report((int? percent, string Title, string Message) value)
        {
            if (IsFirstMessage)
            {
                IsFirstMessage = false;
                await _Service.AddNewProgress(new BlazorNotifierProgressMessage(ProgressID, value.Title, value.Message, value.percent, UserId));
            }
            await _Service.UpdateProgress(new BlazorNotifierProgressMessage(ProgressID, value.Title, value.Message, value.percent, UserId));
            base.OnReport(value);
        }

        public async void Dispose()
        {
            await _Service.FinishProgress(new BlazorNotifierProgressMessage{ Id = ProgressID, ToUserId = UserId });
        }
        public BlazorNotifierProgress(string userId, BlazorNotifierServerService service)
        {
            UserId = userId;
            ProgressID = Guid.NewGuid();
            _Service = service;
        }

    }
}