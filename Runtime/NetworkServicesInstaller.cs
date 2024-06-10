namespace GameFoundation.Scripts.Network
{
    using System;
    using GameFoundation.Scripts.Network.Signal;
    using GameFoundation.Scripts.Network.WebService;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.UserData;
    using global::Models;
    using Zenject;

    /// <summary>Is used in zenject, install all stuffs relate to network into global context.</summary>
    public class NetworkServicesInstaller : Installer<NetworkConfig, NetworkServicesInstaller>
    {
        private readonly NetworkConfig networkConfig;

        public NetworkServicesInstaller(NetworkConfig networkConfig) { this.networkConfig = networkConfig; }

        public override void InstallBindings()
        {
            this.Container.Bind<NetworkConfig>().FromInstance(this.networkConfig).AsCached().NonLazy();
            this.BindNetworkSetting();

            // Pooling for http request object, transfer data object
            this.Container.BindIFactoryForAllDriveTypeFromPool<BaseHttpRequest>();
            this.Container.BindIFactory<ClientWrappedHttpRequestData>().FromPoolableMemoryPool();
            this.Container.DeclareSignal<MissStatusCodeSignal>();

            var wrapData = this.Container.Instantiate<WrappedBestHttpService>();
            wrapData.Host = this.networkConfig.Host;
            this.Container.Bind(typeof(IDisposable), typeof(IInitializable), typeof(IHttpService)).WithId(NetworkConfig.WrapFull).To<WrappedBestHttpService>().FromInstance(wrapData).AsCached();
            var noWrapHttpService = this.Container.Instantiate<NoWrappedRequestAndResponseService>();
            noWrapHttpService.Host = this.networkConfig.Host;
            this.Container.Bind(typeof(IDisposable), typeof(IInitializable), typeof(IHttpService)).To<NoWrappedRequestAndResponseService>().FromInstance(noWrapHttpService).AsCached();
            var wrapRequestNoResponse = this.Container.Instantiate<WrappedRequestNoResponseHttpServices>();
            wrapRequestNoResponse.Host = this.networkConfig.Host;

            this.Container.Bind(typeof(IDisposable), typeof(IInitializable), typeof(IHttpService)).WithId(NetworkConfig.WrapRequest).To<WrappedRequestNoResponseHttpServices>().FromInstance(wrapRequestNoResponse)
                .AsCached();

            var wrapResponseNoRequest = this.Container.Instantiate<WrappedResponseNoRequestHttpServices>();
            wrapResponseNoRequest.Host = this.networkConfig.Host;

            this.Container.Bind(typeof(IDisposable), typeof(IInitializable), typeof(IHttpService)).WithId(NetworkConfig.WrapResponse).To<WrappedResponseNoRequestHttpServices>()
                .FromInstance(wrapResponseNoRequest).AsCached();
        }

        private async void BindNetworkSetting()
        {
            var localDataServices = this.Container.Resolve<IHandleUserDataServices>();
            var networkLocalData         = await localDataServices.Load<NetworkLocalData>();
            this.Container.Bind<NetworkLocalData>().FromInstance(networkLocalData).AsCached().NonLazy();
        }
    }
}