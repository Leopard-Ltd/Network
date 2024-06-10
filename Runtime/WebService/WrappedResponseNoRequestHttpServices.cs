namespace GameFoundation.Scripts.Network.WebService
{
    using GameFoundation.Scripts.Utilities.LogService;
    using global::Models;
    using Zenject;

    public class WrappedResponseNoRequestHttpServices : BestBaseHttpProcess, IWrapResponse
    {
        public WrappedResponseNoRequestHttpServices(ILogService logger, NetworkLocalData LocalData, NetworkConfig networkConfig, DiContainer container) : base(logger, LocalData, networkConfig,
            container)
        {
        }
    }
}