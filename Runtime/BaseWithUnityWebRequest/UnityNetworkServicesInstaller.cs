namespace BaseWithUnityWebRequest
{
    using System;
    using BaseWithBestHttp;
    using BaseWithBestHttp.Interfaces;
    using BaseWithBestHttp.Models;
    using BaseWithBestHttp.Signal;
    using BaseWithBestHttp.WebService;
    using BaseWithUnityWebRequest.Core;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.UserData;
    using UnityEngine.Scripting;
    using Zenject;

    public class UnityNetworkServicesInstaller : Installer<NetworkConfig, UnityNetworkServicesInstaller>
    {
        private readonly NetworkConfig networkConfig;

        [Preserve]
        public UnityNetworkServicesInstaller(NetworkConfig networkConfig) { this.networkConfig = networkConfig; }

        public override void InstallBindings()
        {
            this.Container.Bind<NetworkConfig>().FromInstance(this.networkConfig).AsCached().NonLazy();
            this.BindNetworkSetting();

            // Pooling for http request object, transfer data object
            this.Container.BindInterfacesAndSelfToAllTypeDriveFrom<BaseHttpRequest>();
            this.Container.BindIFactory<ClientWrappedHttpRequestData>();
            this.Container.DeclareSignal<MissStatusCodeSignal>();

            var wrapData = this.Container.Instantiate<UnityWrappedServices>();
            wrapData.Host = this.networkConfig.Host;
            this.Container.Bind(typeof(IDisposable), typeof(IInitializable), typeof(IHttpService)).WithId(NetworkConfig.WrapFull).To<UnityWrappedServices>().FromInstance(wrapData).AsCached();
           
            var noWrapHttpService = this.Container.Instantiate<UnityWebNoWrapServices>();
            noWrapHttpService.Host = this.networkConfig.Host;
            this.Container.Bind(typeof(IDisposable), typeof(IInitializable), typeof(IHttpService)).To<UnityWebNoWrapServices>().FromInstance(noWrapHttpService).AsCached();

            var wrapRequestNoResponse = this.Container.Instantiate<UnityWrappedRequestServices>();
            wrapRequestNoResponse.Host = this.networkConfig.Host;
            this.Container.Bind(typeof(IDisposable), typeof(IInitializable), typeof(IHttpService)).WithId(NetworkConfig.WrapRequest).To<UnityWrappedRequestServices>()
                .FromInstance(wrapRequestNoResponse)
                .AsCached();

            var wrapResponseNoRequest = this.Container.Instantiate<UnityWrappedResponseServices>();
            wrapResponseNoRequest.Host = this.networkConfig.Host;
            this.Container.Bind(typeof(IDisposable), typeof(IInitializable), typeof(IHttpService)).WithId(NetworkConfig.WrapResponse).To<UnityWrappedResponseServices>()
                .FromInstance(wrapResponseNoRequest).AsCached();
        }

        private async void BindNetworkSetting()
        {
            var localDataServices = this.Container.Resolve<IHandleUserDataServices>();
            var networkLocalData  = await localDataServices.Load<NetworkLocalData>();
            this.Container.Bind<NetworkLocalData>().FromInstance(networkLocalData).AsCached().NonLazy();
        }
    }
}