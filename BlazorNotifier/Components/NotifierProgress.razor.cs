using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using BlazorNotifier.Classes;
using Microsoft.AspNetCore.Components;

namespace BlazorNotifier.Components
{
    public partial class NotifierProgress
    {
        [Parameter]
        public BlazorNotifierProgressMessage Progress { get; set; }
        [Parameter]
        public EventCallback<BlazorNotifierProgressMessage> OnClick { get; set; }
        [Parameter]
        public EventCallback<BlazorNotifierProgressMessage> OnClose { get; set; }

        void Click() => OnClick.InvokeAsync(Progress);

        void Close()
        {
            Task.Delay(0).ContinueWith(r =>
            {
                IsVisible = false;
                OnClose.InvokeAsync(Progress);
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
        protected override void OnInitialized() => Task.Delay(10).ContinueWith(r => IsVisible = true);
    }
}
