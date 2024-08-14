namespace GameFoundation.Scripts.Network.WebService
{
    using global::Models;
    using Zenject;

    public class WrappedResponseNoRequestHttpServices : BestBaseHttpProcess, IWrapResponse
    {
        public WrappedResponseNoRequestHttpServices(WrapLogger wrapLogger, NetworkLocalData LocalData, NetworkConfig networkConfig, DiContainer container) : base(wrapLogger, LocalData, networkConfig,
            container)
        {
        }
    }
}