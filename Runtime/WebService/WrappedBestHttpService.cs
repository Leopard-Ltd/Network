namespace GameFoundation.Scripts.Network.WebService
{
    using global::Models;
    using Zenject;

    public class WrappedBestHttpService : BestBaseHttpProcess, IWrapRequest, IWrapResponse
    {
        public WrappedBestHttpService(WrapLogger wrapLogger, NetworkLocalData LocalData, NetworkConfig networkConfig, DiContainer container) : base(wrapLogger, LocalData, networkConfig, container) { }
    }
}