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
            this.Container.Bind(typeof(IDisposable), typeof(IInitializable), typeof(IHttpService)).WithId("wrap").To<WrappedBestHttpService>().FromInstance(wrapData).AsCached();
            var noWrapHttpService = this.Container.Instantiate<NoWrappedService>();

            this.Container.Bind(typeof(IDisposable), typeof(IInitializable), typeof(IHttpService)).To<NoWrappedService>().FromInstance(noWrapHttpService).AsCached();
        }

        private async void BindNetworkSetting()
        {
            var localDataServices = this.Container.Resolve<IHandleUserDataServices>();
            var soundData         = await localDataServices.Load<NetworkLocalData>();
            this.Container.Bind<NetworkLocalData>().FromInstance(soundData).AsCached().NonLazy();
        }
    }
}