using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorNotifier.Classes;
using Microsoft.Extensions.Logging;

namespace BlazorNotifier.Services.Implementations
{
    public class BlazorNotifierServerService
    {
        private readonly ILogger<BlazorNotifierServerService> _Logger;

        public BlazorNotifierServerService(ILogger<BlazorNotifierServerService> Logger)
        {
            _Logger = Logger;
        }

        #region Implementation of IBlazorNotifierServerService

        public async Task<bool> SendNotificationAsync(BlazorNotifierMessage message)
        {
            try
            {
                using var response = await new HttpClient().PostAsJsonAsync("https://localhost:44303/api/notifications/SendMessage", message);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _Logger.LogError("Ошибка отправки сообщения на сервер: {error}", e.Message);
                return false;
            }
        }
        public async Task<bool> SendLogAsync(BlazorNotifierMessage message)
        {
            try
            {
                using var response = await new HttpClient().PostAsJsonAsync("https://localhost:44303/api/notifications/Log", message);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _Logger.LogError("Ошибка отправки сообщения на сервер: {error}", e.Message);
                return false;
            }
        }
        public async Task<bool> SendNotificationAsync(string Title)
        {
            try
            {
                using var response = await new HttpClient().PostAsJsonAsync("https://localhost:44303/api/notifications/SendTitle", Title);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _Logger.LogError("Ошибка отправки сообщения на сервер: {error}", e.Message);
                return false;
            }
        }

        public async Task<bool> FinishProgress(BlazorNotifierProgressMessage Message)
        {
            try
            {
                using var response = await new HttpClient().PostAsJsonAsync("https://localhost:44303/api/notifications/progress/finish", Message);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _Logger.LogError("Ошибка отправки сообщения на сервер: {error}", e.Message);
                return false;
            }

        }
        #endregion

        public async Task<bool> UpdateProgress(BlazorNotifierProgressMessage Message)
        {
            try
            {
                using var response = await new HttpClient().PostAsJsonAsync("https://localhost:44303/api/notifications/progress/update", Message);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _Logger.LogError("Ошибка отправки сообщения на сервер: {error}", e.Message);
                return false;
            }

        }
        public async Task<bool> AddNewProgress(BlazorNotifierProgressMessage Message)
        {
            try
            {
                using var response = await new HttpClient().PostAsJsonAsync("https://localhost:44303/api/notifications/progress/Add", Message);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _Logger.LogError("Ошибка отправки сообщения на сервер: {error}", e.Message);
                return false;
            }

        }
    }
}
