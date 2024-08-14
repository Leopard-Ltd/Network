namespace GameFoundation.Scripts.Network.WebService
{
    using global::Models;
    using Zenject;

    public class WrappedRequestNoResponseHttpServices : BestBaseHttpProcess, IWrapRequest
    {
        public WrappedRequestNoResponseHttpServices(WrapLogger wrapLogger, NetworkLocalData LocalData, NetworkConfig networkConfig, DiContainer container) : base(wrapLogger, LocalData, networkConfig,
            container)
        {
        }
    }
}