namespace BaseWithUnityWebRequest.Core
{
    using BaseWithBestHttp;
    using BaseWithBestHttp.Models;
    using GameFoundation.Scripts.Utilities.LogService;
    using Zenject;

    public class UnityWebNoWrapServices:UnityWebRequestBaseProcess
    {
        public UnityWebNoWrapServices(ILogService logger, NetworkLocalData localData, NetworkConfig networkConfig, DiContainer container) : base(logger, localData, networkConfig, container)
        {
        }
    }
}