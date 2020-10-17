using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using BlazorNotifier.Classes;
using Microsoft.AspNetCore.Components;
using Timer = System.Timers.Timer;

namespace BlazorNotifier.Components
{
    public partial class NotifierMessage : ComponentBase
    {
        private string MStyle { get; set; }

        [Parameter]
        public Guid Id { get; set; }
        [Parameter]
        public int TimeOut { get; set; }
        [Parameter]
        public string Title { get; set; }
        [Parameter]
        public BlazorNotifierType Type { get; set; }
        [Parameter]
        public EventCallback<Guid> OnClick { get; set; }
        [Parameter]
        public EventCallback<Guid> OnClose { get; set; }
        void Click() => OnClick.InvokeAsync(Id);

        void Close()
        {
            IsVisible = false;
            Task.Run(async
                () =>
                {
                    //await Task.Delay(100);
                    await OnClose.InvokeAsync(Id);
                    StateHasChanged();

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
            Task.Run(async
                () =>
            {
                await Task.Delay(10);
                IsVisible = true;

                await Task.Delay(TimeSpan.FromSeconds(TimeOut-1));
                Close();
            });

        }
    }
}
