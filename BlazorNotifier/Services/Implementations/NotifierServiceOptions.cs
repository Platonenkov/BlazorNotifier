namespace BlazorNotifier.Services.Implementations
{
    class NotifierServiceOptions : INotifierServiceOptions
    {
        #region Implementation of INotifierServiceOptions
        public string ServiceAddress { get; set; }
        public string HubName { get; set; }

        public string ControllerApiPath { get; set; }

        #endregion
    }
}