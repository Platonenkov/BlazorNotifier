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
                await Task.Delay(100);
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

    }
}
