namespace BaseWithBestHttp.WebService
{
    using BaseWithBestHttp.Interfaces;
    using BaseWithBestHttp.Models;
    using GameFoundation.Scripts.Utilities.LogService;
    using Zenject;

    public class WrappedBestHttpService : BestBaseHttpProcess, IWrapRequest, IWrapResponse
    {
        public WrappedBestHttpService(ILogService logger, NetworkLocalData LocalData, NetworkConfig networkConfig, DiContainer container) : base(logger, LocalData, networkConfig, container) { }
        public string RootResponse => "data";
        public string RootRequest  => "data";
    }
}