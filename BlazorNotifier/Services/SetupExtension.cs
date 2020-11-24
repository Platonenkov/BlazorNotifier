using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorNotifier.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorNotifier.Services
{
    public static class SetupExtension
    {
        //public static IServiceCollection AddNotifierService(this IServiceCollection Services, string serviceAddress)
        //{
        //    if (string.IsNullOrWhiteSpace(serviceAddress))
        //        throw new ArgumentNullException(nameof(serviceAddress), "Service address can't be null or white space");
        //    Services.AddNotifierService(o => o.ServiceAddress = serviceAddress);
        //    //Services.AddSingleton(si =>
        //    //{
        //    //    NotifierServiceOptions notifierServiceOptions = new NotifierServiceOptions(serviceAddress);
        //    //    return notifierServiceOptions;
        //    //}); 
        //    ////Services.AddSingleton<NotifierServiceOptions>((Func<IServiceProvider, NotifierServiceOptions>)(si =>
        //    ////{
        //    ////    NotifierServiceOptions notifierServiceOptions = new NotifierServiceOptions(serviceAddress);
        //    ////    return notifierServiceOptions;
        //    ////}));
        //    //Services.AddScoped<BlazorNotifierClientService>();
        //    //Services.AddScoped<BlazorNotifierServerService>();
        //    return Services;
        //}
        public static IServiceCollection AddNotifierService(
            this IServiceCollection services,
            Action<INotifierServiceOptions> setOptions)
        {
            if (setOptions == null)
                throw new ArgumentNullException(nameof(setOptions));
            services.AddSingleton<INotifierServiceOptions, NotifierServiceOptions>((Func<IServiceProvider, NotifierServiceOptions>)(si =>
            {
                NotifierServiceOptions serviceOptions = new NotifierServiceOptions();
                setOptions((INotifierServiceOptions)serviceOptions);
                return !string.IsNullOrWhiteSpace(serviceOptions.ServiceAddress)
                       && !string.IsNullOrWhiteSpace(serviceOptions.HubName)
                       && !string.IsNullOrWhiteSpace(serviceOptions.ControllerApiPath)
                    ? serviceOptions
                    : throw new ArgumentNullException(nameof(serviceOptions.ServiceAddress), "Service address, hub name or controller path can't be null or white space");
            }));
            services.AddScoped<BlazorNotifierClientService>();
            services.AddScoped<BlazorNotifierServerService>();
            return services;
        }

    }
    public interface INotifierServiceOptions
    {
        /// <summary>
        /// Service https address.
        /// </summary>
        /// <remarks>
        /// Sample address:
        ///
        ///    https://sample.ru
        ///    https://localhost:5333
        ///
        /// </remarks>
        string ServiceAddress { get; set; }
        /// <summary>
        /// Service hub name.
        /// </summary>
        /// <remarks>
        /// Sample name:
        ///
        ///    notificationhub
        ///
        /// </remarks>
        string HubName { get; set; }
        /// <summary>
        /// Service controller path.
        /// </summary>
        /// <remarks>
        /// Sample name:
        ///
        ///    api/notifications
        ///
        /// </remarks>
        string ControllerApiPath { get; set; }
    }

}
