namespace BaseWithUnityWebRequest.Core
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using BaseWithBestHttp;
    using BaseWithBestHttp.Interfaces;
    using BaseWithBestHttp.Models;
    using BaseWithBestHttp.WebService;
    using BaseWithBestHttp.WebService.Requests;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.Utils;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using R3;
    using UnityEngine;
    using UnityEngine.Networking;
    using Zenject;
    using Color = UnityEngine.Color;

    public abstract class UnityWebRequestBaseProcess : IHttpService, IInitializable, IDisposable
    {
        protected readonly ILogService      Logger;
        protected readonly NetworkLocalData LocalData;
        protected readonly NetworkConfig    NetworkConfig;
        protected readonly DiContainer      Container;

        public UnityWebRequestBaseProcess(ILogService logger, NetworkLocalData localData, NetworkConfig networkConfig, DiContainer container)
        {
            this.Logger        = logger;
            this.LocalData     = localData;
            this.NetworkConfig = networkConfig;
            this.Container     = container;
        }

        public virtual void Initialize() { }
        public virtual void Dispose()    { }

        public string GetDownloadPath(string path) => $"{Application.persistentDataPath}/{path}";

        public ReactiveProperty<bool> HasInternetConnection { get; set; } = new(true);
        public ReactiveProperty<bool> IsProcessApi          { get; set; } = new(false);
        public string                 Host                  { get; set; }

        public UniTask<TK> SendPostAsync<T, TK>(object httpRequestData = null, string jwtToken = "") where T : BasePostRequest<TK>
            => this.SendRequestInternal<T, TK>(httpRequestData, jwtToken, HttpMethod.Post);

        public UniTask<TK> SendGetAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = true) where T : BaseGetRequest<TK>
            => this.SendRequestInternal<T, TK>(httpRequestData, jwtToken, HttpMethod.Get, includeBody);

        public UniTask<TK> SendPutAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = false) where T : BasePutRequest<TK>
            => this.SendRequestInternal<T, TK>(httpRequestData, jwtToken, HttpMethod.Put, includeBody);

        public UniTask<TK> SendDeleteAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = true) where T : BaseDeleteRequest<TK>
            => this.SendRequestInternal<T, TK>(httpRequestData, jwtToken, HttpMethod.Delete, includeBody);

        public UniTask<TK> SendPatchAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = true)
            where T : BasePatchRequest<TK>
            => this.SendRequestInternal<T, TK>(httpRequestData, jwtToken, HttpMethod.Patch, includeBody);

        private async UniTask<TK> SendRequestInternal<T, TK>(object data, string jwtToken, string method, bool includeBody = true) where T : BaseHttpRequest<TK>
        {
            if (Attribute.GetCustomAttribute(typeof(T), typeof(HttpRequestDefinitionAttribute)) is not HttpRequestDefinitionAttribute attr)
                throw new Exception($"[Http] `{typeof(T).Name}` missing HttpRequestDefinitionAttribute");

#if (DEVELOPMENT_BUILD || UNITY_EDITOR) && FAKE_DATA
        if (typeof(IFakeResponseAble<TK>).IsAssignableFrom(typeof(T)))
        {
            var baseHttpRequest = this.Container.Resolve<IFactory<T>>().Create();
            var responseData = ((IFakeResponseAble<TK>)baseHttpRequest).FakeResponse();
            baseHttpRequest.Process(responseData);
            return responseData;
        }
#endif

            return await this.SendRequestAsync<T, TK>(attr.Route, data, jwtToken, method, includeBody);
        }

        public async UniTask<byte[]> DownloadAndReadStreaming(string address, OnDownloadProgressDelegate onDownloadProgress, int timeoutSeconds = 30,
            int retryCount = 3, CancellationToken cancellationToken = default)
        {
            int attempt = 0;

            while (attempt <= retryCount)
            {
                attempt++;
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfterSlim(TimeSpan.FromSeconds(timeoutSeconds));

                using var request = UnityWebRequest.Get(address);
                request.downloadHandler = new DownloadHandlerBuffer();

                try
                {
                    var operation = request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        onDownloadProgress?.Invoke((long)(request.downloadProgress * 100), 100);
                        await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        var data = request.downloadHandler.data;
                        this.Logger.Log($"✅ Streamed {data.Length} bytes from {address}");

                        return data;
                    }

                    this.Logger.Warning($"❌ Stream failed {request.responseCode}: {request.error}");
                }
                catch (Exception e)
                {
                    this.Logger.Error($"🔥 Streaming Exception: {e.Message}");
                }

                if (attempt <= retryCount)
                {
                    this.Logger.Log($"🔁 Retry {attempt}/{retryCount}");
                    await UniTask.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return Array.Empty<byte>();
        }

        public async UniTask Download(string address, string filePath, OnDownloadProgressDelegate onProgress, int timeout = 30, int retry = 3, CancellationToken token = default)
        {
            filePath = this.GetDownloadPath(filePath);
            int attempt = 0;

            while (attempt <= retry)
            {
                attempt++;
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfterSlim(TimeSpan.FromSeconds(timeout));

                using var request = UnityWebRequest.Get(address);
                request.downloadHandler = new DownloadHandlerFile(filePath) { removeFileOnAbort = true };

                try
                {
                    var op = request.SendWebRequest();

                    while (!op.isDone)
                    {
                        token.ThrowIfCancellationRequested();
                        onProgress?.Invoke((long)(request.downloadProgress * 100), 100);
                        await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        this.Logger.Log($"✅ File downloaded: {filePath}");

                        return;
                    }

                    this.Logger.Warning($"❌ Download failed {request.responseCode}: {request.error}");
                }
                catch (Exception e)
                {
                    this.Logger.Error($"🔥 Download Exception: {e.Message}");
                }

                if (File.Exists(filePath)) File.Delete(filePath);

                if (attempt <= retry)
                {
                    this.Logger.Log($"🔁 Retry {attempt}/{retry}");
                    await UniTask.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }

        protected virtual async UniTask<TResponse> SendRequestAsync<TRequest, TResponse>(string route, object requestData, string jwtToken, string method, bool includeBody = true)
            where TRequest : BaseHttpRequest<TResponse>
        {
            this.IsProcessApi.Value = true;
            TResponse result = default;

            var       url     = this.ReplaceUri(route);
            using var request = new UnityWebRequest(url, method);
            request.timeout = (int)this.NetworkConfig.HttpRequestTimeout;

            if (includeBody && requestData != null)
            {
                string jsonBody;

                if (this is IWrapRequest wrapRequest)
                {
                    var wrapper = new JObject
                    {
                        [wrapRequest.RootRequest]   = JObject.FromObject(requestData)
                    };
                    jsonBody = wrapper.ToString(Formatting.None);
                }
                else
                {
                    jsonBody = JsonConvert.SerializeObject(requestData);
                }

                var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(jwtToken))
                request.SetRequestHeader("Authorization", $"Bearer {jwtToken}");

            if (!string.IsNullOrEmpty(GameVersion.Version))
                request.SetRequestHeader("game-version", GameVersion.Version);

            try
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    this.Logger.LogWithColor($"[✅ {typeof(TRequest).Name},{this.GetType().Name}] {url}\n{request.downloadHandler.text}", Color.cyan);
                    var json = JObject.Parse(request.downloadHandler.text);
                    result = this.RequestSuccessProcess<TRequest, TResponse>(json, requestData);
                }
                else
                {
                    this.Logger.Error($"[❌ {typeof(TRequest).Name},{this.GetType().Name}] {url} | {request.responseCode}: {request.downloadHandler.text}");
                    this.HandleRequestError<TRequest>(request);
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error($"[🔥 {typeof(TRequest).Name},{this.GetType().Name}] {url} | {ex.Message}");
                this.HandleErrorException<TRequest>(ex);
            }

            this.IsProcessApi.Value = false;

            return result;
        }

        protected virtual TResponse RequestSuccessProcess<TRequest, TResponse>(JObject responseData, object requestData)
            where TRequest : BaseHttpRequest, IDisposable
        {
            var request = this.Container.Resolve<TRequest>();

            var parsed = this is IWrapResponse wrap && responseData.TryGetValue(wrap.RootResponse, out var d)
                ? d.ToObject<TResponse>()
                : responseData.ToObject<TResponse>();

            request.Process(parsed);
            request.PredictProcess(requestData);

            return parsed;
        }

        protected void HandleErrorException<TRequest>(Exception ex) where TRequest : BaseHttpRequest, IDisposable
            => this.Container.Resolve<TRequest>().ExceptionProcess(ex);

        protected void HandleRequestError<TRequest>(UnityWebRequest request) where TRequest : BaseHttpRequest, IDisposable
        {
            try
            {
                var errorData = JsonConvert.DeserializeObject<ErrorData>(request.downloadHandler.text);
                this.Container.Resolve<TRequest>().ErrorProcess(errorData);
            }
            catch (Exception e)
            {
                this.Logger.LogWithColor(e.Message, Color.red);
                this.Container.Resolve<TRequest>().ErrorProcess(request.downloadHandler.text);
            }
        }

        protected Uri ReplaceUri(string route)
        {
            var builder = new StringBuilder(route);

            foreach (var pair in this.LocalData.ServerToken.ParameterNameToValue)
                builder.Replace($"{{{pair.Key}}}", pair.Value);

            return new Uri($"{this.Host}{builder}");
        }
    }
}