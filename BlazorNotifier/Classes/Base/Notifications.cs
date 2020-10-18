using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BlazorNotifier.Classes.Base
{
    public class Notifications
    {
        public ObservableCollection<BlazorNotifierMessage> Messages { get; }= new ObservableCollection<BlazorNotifierMessage>();
        public ObservableCollection<BlazorNotifierProgressMessage> Progress { get; }= new ObservableCollection<BlazorNotifierProgressMessage>();

        /// <summary> Число сообщений </summary>
        public int Count => Messages.Count + Progress.Count;
        /// <summary> Событие при изменениях в сервисе </summary>
        public event Action OnChange;

        public event Action<BlazorNotifierMessage> OnAddNewMessage;
        public event Action<BlazorNotifierMessage> OnRemoveMessage;
        public event Action<BlazorNotifierProgressMessage> OnAddNewProgress;
        public event Action<BlazorNotifierProgressMessage> OnRemoveProgress;
        public event Action<BlazorNotifierProgressMessage> OnUpdateProgress;

        private void Change() => OnChange?.Invoke();


        #region Сообщения

        public void AddMessage(BlazorNotifierMessage message)
        {
            Messages.Add(message);
            Change();
            OnAddNewMessage?.Invoke(message);
        }

        public void RemoveMessage(BlazorNotifierMessage message)
        {
            Messages.Remove(message);
            Change();  
            OnRemoveMessage?.Invoke(message);
        }

        public void RemoveMessage(Guid id)
        {
            var message = Messages.FirstOrDefault(m => m.Id == id);
            if (message is null)
            {
                Change();
                return;
            }
            RemoveMessage(message);
        }

        public void ClearMessages()
        { 
            Messages.Clear();
            Change();
        }

        #endregion

        #region Прогресс бар
        public void AddProgress(BlazorNotifierProgressMessage progress)
        {
            Progress.Add(progress);
            Change();
            OnAddNewProgress?.Invoke(progress);

        }

        public void RemoveProgress(BlazorNotifierProgressMessage progress)
        {
            Progress.Remove(progress);
            Change();
            OnRemoveProgress?.Invoke(progress);
        } 
        public void RemoveProgress(Guid id)
        {
            var progress = Progress.FirstOrDefault(m => m.Id == id);
            if (progress is null)
            {
                Console.WriteLine($"NOT FIND {id}");
                Change();
                return;
            }
            RemoveProgress(progress);
        }

        public bool ContainsProgress(Guid id)
        {
            var progress = Progress.FirstOrDefault(m => m.Id == id);
            return !(progress is null);
        }
        public void UpdateProgress(BlazorNotifierProgressMessage progress)
        {
            var current = Progress.FirstOrDefault(m => m.Id == progress.Id);
            current?.Update(progress);
            Change();
            OnUpdateProgress?.Invoke(progress);
        }

        public void ClearProgress()
        {
            Progress.Clear();
            OnChange?.Invoke();
        }

        #endregion
    }
}