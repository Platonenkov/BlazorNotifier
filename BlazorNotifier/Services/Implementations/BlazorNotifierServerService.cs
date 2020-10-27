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
        /// <summary>
        /// Отправить на клиент сообщение
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <returns></returns>
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

        /// <summary>
        /// Отправить на клиент сообщение
        /// </summary>
        /// <param name="text">сообщение</param>
        /// <param name="UserId">кому</param>
        /// <param name="type">тип уведомления</param>
        /// <returns></returns>
        public async Task<bool> SendNotificationAsync(string text, string UserId, BlazorNotifierType type = BlazorNotifierType.Info)
        {
            var message = new BlazorNotifierMessage{Title = text, ToUserId = UserId, Type = type};
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

        /// <summary>
        /// Отправить на клиент запись для лога
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <returns></returns>
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

        /// <summary>
        /// Отправить на клиент запись для лога
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SendLogAsync(string text, string UserId, BlazorNotifierType type = BlazorNotifierType.Info)
            => await SendLogAsync(new BlazorNotifierMessage { ToUserId = UserId, Title = text, Type = type});
       
        /// <summary>
        /// отправить на клиент сообщение
        /// </summary>
        /// <param name="Title"></param>
        /// <returns></returns>
        public async Task<bool> SendAll(string Title)
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

        /// <summary>
        /// отправить на клиент сообщение
        /// </summary>
        /// <param name="Title">текст</param>
        /// <param name="type">тип</param>
        /// <returns></returns>
        public async Task<bool> SendAll(string Title, BlazorNotifierType type)
        {
            var message = new BlazorNotifierMessage{Title = Title, Type = type, IsPrivate = false};
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
        /// <summary>
        /// завершить на клиенте прогресс
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Обновить на клиенте состояние прогресса
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Информировать клиент о начале процесса
        /// </summary>
        /// <param name="Message">сообщение</param>
        /// <returns></returns>
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
