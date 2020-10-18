using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using BlazorNotifier.Classes;
using BlazorNotifier.Services.Implementations;
using Microsoft.AspNetCore.Components;
using Timer = System.Timers.Timer;

namespace BlazorNotifier.Components
{
    public partial class NotifierMessage
    {

        [Parameter]
        public BlazorNotifierMessage Message { get; set; }
        [Parameter]
        public EventCallback<Guid> OnClick { get; set; }
        [Parameter]
        public EventCallback<Guid> OnClose { get; set; }
        void Click() => OnClick.InvokeAsync(Message.Id);

        void Close()
        {
            Task.Delay(0).ContinueWith(r =>
            {
                IsVisible = false;
                OnClose.InvokeAsync(Message.Id);
            });
        }

        private bool _IsVisible;
        bool IsVisible
        {
            get => _IsVisible;
            set
            {
                _IsVisible = value;
                StateHasChanged();
            }
        }

        protected override void OnInitialized()
        {
            Task.Delay(10).ContinueWith(r => IsVisible = true);
            Task.Delay(Message.TimeOut * 1000).ContinueWith(r => InvokeAsync(Close));
        }

    }
}
