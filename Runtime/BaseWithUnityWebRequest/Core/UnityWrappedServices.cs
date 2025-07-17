namespace BaseWithUnityWebRequest.Core
{
    using BaseWithBestHttp;
    using BaseWithBestHttp.Interfaces;
    using BaseWithBestHttp.Models;
    using GameFoundation.Scripts.Utilities.LogService;
    using Zenject;

    public class UnityWrappedServices : UnityWebRequestBaseProcess, IWrapRequest, IWrapResponse
    {
        public UnityWrappedServices(ILogService logger, NetworkLocalData LocalData, NetworkConfig networkConfig, DiContainer container) : base(logger, LocalData, networkConfig, container) { }
        public string RootRequest  => "data";
        public string RootResponse => "data";
    }
}