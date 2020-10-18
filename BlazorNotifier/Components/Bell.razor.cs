using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorNotifier.Services.Implementations;
using Microsoft.AspNetCore.Components;

namespace BlazorNotifier.Components
{
    public partial class Bell
    {
        [Inject]
        private BlazorNotifierClientService Service { get; set; }

        protected override void OnInitialized() => Service.Notification.OnChange += Update;

        void Update() => InvokeAsync(StateHasChanged);

    }
}
