namespace GameFoundation.Scripts.Network.WebService
{
    using System;
    using BestHTTP;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Network.Signal;
    using GameFoundation.Scripts.Network.WebService.Requests;
    using GameFoundation.Scripts.Utilities.LogService;
    using UniRx;
    using Zenject;

    /// <summary>Provide a way to send http request, download content.</summary>
    public interface IHttpService
    {
        string Host { get; set; }

        /// <summary>Send http request async with a IHttpRequestData</summary>
        UniTask<TK> SendPostAsync<T, TK>(object httpRequestData = null) where T : BasePostRequest<TK>;

        UniTask<TK> SendGetAsync<T, TK>(object httpRequestData = null, bool includeBody = true) where T : BaseGetRequest<TK>;

        UniTask<TK> SetPutAsync<T, TK>(object httpRequestData = null) where T : BasePutRequest<TK>;

        UniTask<TK> SendDeleteAsync<T, TK>(object httpRequestData = null,string jwtToken="", bool includeBody = true) where T : BaseDeleteRequest<TK>;

        /// <summary>Download from <paramref name="address"/> to <paramref name="filePath"/>, download progress will be updated into <paramref name="onDownloadProgress"/>.</summary>
        UniTask Download(string address, string filePath, OnDownloadProgressDelegate onDownloadProgress);

        UniTask<byte[]> DownloadAndReadStreaming(string address, OnDownloadProgressDelegate onDownloadProgress);

        /// <summary>Return the real download path.</summary>
        string GetDownloadPath(string path);

        BoolReactiveProperty HasInternetConnection { get; set; }
        void                 InitRequest(HTTPRequest request, object httpRequestData);
    }

    /// <summary>Download progress delegate.</summary>
    public delegate void OnDownloadProgressDelegate(long downloaded, long downloadLength);

    /// <summary>All http request object will implement this class.</summary>
    public abstract class BaseHttpRequest
    {
        [Inject] private SignalBus signalBus;

        public abstract void Process(object responseData);

        public virtual void ErrorProcess(int statusCode)
        {
            this.signalBus.Fire(new MissStatusCodeSignal());
            //throw new MissStatusCodeException();
        }

        public virtual void ErrorProcess(ErrorData errorData)
        {
            // process common code
            switch (errorData.Code)
            {
            }
        }

        public virtual void PredictProcess(object requestData) { }

        public class MissStatusCodeException : Exception
        {
        }
    }

    public abstract class BasePostRequest<T> : BaseHttpRequest<T>
    {
        protected BasePostRequest(ILogService logger) : base(logger) { }
    }

    public abstract class BaseGetRequest<T> : BaseHttpRequest<T>
    {
        protected BaseGetRequest(ILogService logger) : base(logger) { }
    }

    public abstract class BasePutRequest<T> : BaseHttpRequest<T>
    {
        protected BasePutRequest(ILogService logger) : base(logger) { }
    }

    public abstract class BaseDeleteRequest<T> : BaseHttpRequest<T>
    {
        protected BaseDeleteRequest(ILogService logger) : base(logger) { }
    }

    /// <summary>
    /// All http request class will extend this. It will manage main flow to process response data, pool circle,..
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    public abstract class BaseHttpRequest<T> : BaseHttpRequest, IDisposable, IPoolable<IMemoryPool>
    {
        protected readonly ILogService Logger;

        private IMemoryPool pool;

        protected BaseHttpRequest(ILogService logger) { this.Logger = logger; }

        public void Dispose()                   { this.pool.Despawn(this); }
        public void OnDespawned()               { this.Logger.Log($"spawned {this}"); }
        public void OnSpawned(IMemoryPool pool) { this.pool = pool; }

        public override void Process(object responseData)
        {
            this.PreProcess((T)responseData);
            this.Process((T)responseData);
            this.PostProcess((T)responseData);
        }

        public abstract void Process(T responseData);
        public virtual  void PostProcess(T responseData) { }
        public virtual  void PreProcess(T responseData)  { }
    }

    public interface IFakeResponseAble<out T>
    {
        T FakeResponse();
    }
}