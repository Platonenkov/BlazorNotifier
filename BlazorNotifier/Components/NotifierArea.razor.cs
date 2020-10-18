using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorNotifier.Services.Implementations;
using Microsoft.AspNetCore.Components;

namespace BlazorNotifier.Components
{
    public partial class NotifierArea : IDisposable
    {
        [Inject]
        private BlazorNotifierClientService Service { get; set; }

        private void Update() => InvokeAsync(StateHasChanged);

        public void Dispose() => Service.Notification.OnChange -= Update;

        protected override void OnInitialized() => Service.Notification.OnChange += Update;

    }
}
