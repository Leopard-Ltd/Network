namespace GameFoundation.Scripts.Network.Websocket
{
    using System.Threading.Tasks;
    using R3;

    public enum ServiceStatus
    {
        NotInitialize,
        Initialized,
        Connected,
        Closed
    }

    public interface IWebSocketService
    {
        public ReactiveProperty<ServiceStatus> State { get; }

        Task OpenConnection();

        Task CloseConnection();
    }
}