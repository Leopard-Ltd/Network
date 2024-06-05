namespace GameFoundation.Scripts.Network
{
    using GameFoundation.Scripts.Network.Signal;
    using GameFoundation.Scripts.Network.WebService;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.UserData;
    using global::Models;
    using Zenject;

    /// <summary>Is used in zenject, install all stuffs relate to network into global context.</summary>
    public class NetworkServicesInstaller : Installer<NetworkServicesInstaller>
    {
        public override void InstallBindings()
        {
            // //TODO move this into WrappedBestHttpService instead of separate them
            this.Container.Bind<NetworkConfig>().AsSingle().NonLazy();
            this.BindNetworkSetting();

            // Pooling for http request object, transfer data object
            this.Container.BindIFactoryForAllDriveTypeFromPool<BaseHttpRequest>();
            this.Container.BindIFactory<ClientWrappedHttpRequestData>().FromPoolableMemoryPool();
            this.Container.DeclareSignal<MissStatusCodeSignal>();
            var noWrapHttpService = this.Container.Instantiate<NoWrappedService>();
            this.Container.Bind<IHttpService>().FromInstance(noWrapHttpService).AsCached();
        }

        private async void BindNetworkSetting()
        {
            var localDataServices = this.Container.Resolve<IHandleUserDataServices>();
            var soundData         = await localDataServices.Load<NetworkLocalData>();
            this.Container.Bind<NetworkLocalData>().FromInstance(soundData).AsCached().NonLazy();
        }
    }
}