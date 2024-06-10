namespace GameFoundation.Scripts.Network.WebService
{
    using GameFoundation.Scripts.Utilities.LogService;
    using global::Models;
    using Zenject;

    public class WrappedBestHttpService : BestBaseHttpProcess, IWrapRequest, IWrapResponse
    {
        public WrappedBestHttpService(ILogService logger, NetworkLocalData LocalData, NetworkConfig networkConfig, DiContainer container) : base(logger, LocalData, networkConfig, container) { }
    }
}