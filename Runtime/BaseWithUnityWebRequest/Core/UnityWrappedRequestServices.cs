namespace BaseWithUnityWebRequest.Core
{
    using BaseWithBestHttp;
    using BaseWithBestHttp.Interfaces;
    using BaseWithBestHttp.Models;
    using GameFoundation.Scripts.Utilities.LogService;
    using Zenject;

    public class UnityWrappedRequestServices : UnityWebRequestBaseProcess, IWrapRequest
    {
        public UnityWrappedRequestServices(ILogService logger, NetworkLocalData localData, NetworkConfig networkConfig, DiContainer container) : base(logger, localData, networkConfig, container) { }

        public string RootRequest => "data";
    }
}