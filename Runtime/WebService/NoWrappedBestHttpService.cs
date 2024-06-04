namespace GameFoundation.Scripts.Network.WebService
{
    using System.Text;
    using BestHTTP;
    using GameFoundation.Scripts.Utilities.LogService;
    using global::Models;
    using Newtonsoft.Json;
    using Zenject;

    public class NoWrappedBestHttpService : WrappedBestHttpService
    {
        public NoWrappedBestHttpService(ILogService logger, DiContainer container, NetworkConfig networkConfig, NetworkLocalData localData) : base(logger, container, networkConfig, localData) { }

        public override void InitPostRequest(HTTPRequest request, object httpRequestData, string token)
        {
            request.AddHeader("Content-Type", "application/json");
            request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(httpRequestData));
        }
    }
}