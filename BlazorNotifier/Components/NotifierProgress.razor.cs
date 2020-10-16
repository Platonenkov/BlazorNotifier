using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Components;

namespace BlazorNotifier.Components
{
    public partial class NotifierProgress:ComponentBase
    {
        private string MStyle { get; set; }
        [Parameter]
        public Guid Id { get; set; }
        [Parameter]
        public string Title { get; set; }
        [Parameter]
        public string Message { get; set; }
        [Parameter]
        public int? Percent { get; set; }
        [Parameter]
        public EventCallback<Guid> OnClickCallback { get; set; }
        [Parameter]
        public EventCallback<Guid> OnCloseClick { get; set; }

        void OnClick() => OnClickCallback.InvokeAsync(Id);

        private bool IsClosed = false;
        void OnClose()
        {
            IsClosed = true;
            HideToast();
            Task.Delay(TimeSpan.FromMilliseconds(1));
            OnCloseClick.InvokeAsync(Id);
        }

        private Timer Countdown;
        bool IsVisible { get; set; }

        protected override void OnInitialized()
        {
            ShowMessage();
        }

        private void ShowMessage()
        {
            IsVisible = true;
            this.StateHasChanged();
            StartCountdown();
        }

        private void StartCountdown()
        {
            if (Countdown == null)
            {
                Countdown = new Timer(5000);
                Countdown.Elapsed += HideToast;
                Countdown.AutoReset = false;
            }

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
        private void HideToast()
        {
            IsVisible = false;
            StateHasChanged();
        }

        private void HideToast(object source, ElapsedEventArgs args)
        {
            IsVisible = false;
            if (!IsClosed)
                OnClose();
            StateHasChanged();
        }

    }
}
