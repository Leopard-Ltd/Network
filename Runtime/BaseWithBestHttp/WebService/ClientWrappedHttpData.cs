namespace BaseWithBestHttp.WebService
{
    using System;
    using BaseWithBestHttp.WebService.Requests;
    using Newtonsoft.Json;
    using Zenject;
    using Zenject.Internal;

    public class ClientWrappedHttpRequestData : WrappedHttpRequestData, IPoolable<IMemoryPool>, IDisposable
    {
        [JsonIgnore] public IMemoryPool Pool;

        public void Dispose() { this.Pool.Despawn(this); }

        public void OnDespawned()             { this.Pool = null; }
        public void OnSpawned(IMemoryPool p1) { this.Pool = p1; }
    }
}