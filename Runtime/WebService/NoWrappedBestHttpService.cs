namespace GameFoundation.Scripts.Network.WebService
{
    using global::Models;
    using Zenject;

    public class NoWrappedRequestAndResponseService : BestBaseHttpProcess
    {
        public NoWrappedRequestAndResponseService(WrapLogger wrapLogger, NetworkLocalData LocalData, NetworkConfig networkConfig, DiContainer container) : base(wrapLogger, LocalData, networkConfig,
            container)
        {
        }
    }
}