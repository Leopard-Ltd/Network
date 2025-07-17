namespace BaseWithBestHttp.WebService
{
    using BaseWithBestHttp.Interfaces;
    using BaseWithBestHttp.Models;
    using GameFoundation.Scripts.Utilities.LogService;
    using Zenject;

    public class WrappedResponseNoRequestHttpServices : BestBaseHttpProcess, IWrapResponse
    {
        public WrappedResponseNoRequestHttpServices(ILogService logger, NetworkLocalData LocalData, NetworkConfig networkConfig, DiContainer container) : base(logger, LocalData, networkConfig,
            container)
        {
        }

        public string RootResponse => "data";
    }
}