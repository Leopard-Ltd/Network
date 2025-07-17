namespace BaseWithUnityWebRequest.Core
{
    using BaseWithBestHttp;
    using BaseWithBestHttp.Interfaces;
    using BaseWithBestHttp.Models;
    using GameFoundation.Scripts.Utilities.LogService;
    using Zenject;

    public class UnityWrappedResponseServices:UnityWebRequestBaseProcess,IWrapResponse
    {
        public UnityWrappedResponseServices(ILogService logger, NetworkLocalData localData, NetworkConfig networkConfig, DiContainer container) : base(logger, localData, networkConfig, container)
        {
        }

        public string RootResponse => "data";
    }
}