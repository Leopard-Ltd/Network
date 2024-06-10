namespace GameFoundation.Scripts.Network.WebService
{
    using GameFoundation.Scripts.Utilities.LogService;
    using global::Models;
    using Zenject;

    public class WrappedRequestNoResponseHttpServices : BestBaseHttpProcess, IWrapRequest
    {
        public WrappedRequestNoResponseHttpServices(ILogService logger, NetworkLocalData LocalData, NetworkConfig networkConfig, DiContainer container) : base(logger, LocalData, networkConfig,
            container)
        {
        }
    }
}