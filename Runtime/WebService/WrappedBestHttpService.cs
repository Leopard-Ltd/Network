namespace GameFoundation.Scripts.Network.WebService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using BestHTTP;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.Utils;
    using global::Models;
    using ModestTree;
    using Newtonsoft.Json;
    using UniRx;
    using UnityEngine;
    using Zenject;

    public class WrappedBestHttpService : BestBaseHttpProcess, IHttpService, IInitializable, IDisposable
    {
        protected readonly ILogService                  Logger;
        protected readonly NetworkConfig                NetworkConfig;
        protected readonly NetworkLocalData             LocalData;
        protected          Dictionary<HTTPMethods, int> RetryCount = new();

        public WrappedBestHttpService(ILogService logger, DiContainer container, NetworkConfig networkConfig, NetworkLocalData localData) : base(logger, container)
        {
            this.Logger        = logger;
            this.NetworkConfig = networkConfig;
            this.LocalData     = localData;
        }

        public virtual void Initialize()
        {
            this.RetryCount[HTTPMethods.Post]   = 0;
            this.RetryCount[HTTPMethods.Get]    = 0;
            this.RetryCount[HTTPMethods.Put]    = 0;
            this.RetryCount[HTTPMethods.Patch]  = 0;
            this.RetryCount[HTTPMethods.Delete] = 0;
        }

        public virtual void Dispose() { }

        protected virtual void InitBaseRequest(HTTPRequest request, object httpRequestData, string token)
        {
            using (var wrappedData = this.Container.Resolve<IFactory<ClientWrappedHttpRequestData>>().Create())
            {
                wrappedData.Data = httpRequestData;
                request.AddHeader("Content-Type", "application/json");

                var jwtToken = token;

                if (!string.IsNullOrEmpty(jwtToken))
                {
                    request.AddHeader("Authorization", "Bearer " + jwtToken);
                }

                if (!string.IsNullOrEmpty(GameVersion.Version))
                {
                    request.AddHeader("game-version", GameVersion.Version);
                }

                request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(wrappedData));
            }
        }

        #region Post

        public virtual void InitPostRequest(HTTPRequest request, object httpRequestData, string token) { this.InitBaseRequest(request, httpRequestData, token); }

        public virtual async UniTask<TK> SendPostAsync<T, TK>(object httpRequestData = null, string jwtToken = "") where T : BasePostRequest<TK>
        {
            if (Attribute.GetCustomAttribute(typeof(T), typeof(HttpRequestDefinitionAttribute)) is not HttpRequestDefinitionAttribute httpRequestDefinition)
            {
                throw new Exception($"request {typeof(T)} wasn't defined yet!!! Please add HttpRequestDefinitionAttribute for it!!!!");
            }

#if (DEVELOPMENT_BUILD || UNITY_EDITOR) && FAKE_DATA
            if (typeof(IFakeResponseAble<TK>).IsAssignableFrom(typeof(T)))
            {
                var baseHttpRequest = this.Container.Resolve<IFactory<T>>().Create();
                var responseData = ((IFakeResponseAble<TK>)baseHttpRequest).FakeResponse();
                baseHttpRequest.Process(responseData);

                return responseData;
            }
#endif

            var response = await this.TryGetResponse<T, TK>(httpRequestData, httpRequestDefinition.Route, null, jwtToken, true, HTTPMethods.Post);

            return response;
        }

        #endregion

        #region Get

        public virtual void InitGetRequest(HTTPRequest request, object httpRequestData, string token) { this.InitBaseRequest(request, httpRequestData, token); }

        public virtual async UniTask<TK> SendGetAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = true) where T : BaseGetRequest<TK>
        {
            if (Attribute.GetCustomAttribute(typeof(T), typeof(HttpRequestDefinitionAttribute)) is not HttpRequestDefinitionAttribute httpRequestDefinition)
            {
                throw new Exception($"request {typeof(T)} wasn't defined yet!!! Please add HttpRequestDefinitionAttribute for it!!!!");
            }

#if (DEVELOPMENT_BUILD || UNITY_EDITOR) &&FAKE_DATA
            if (typeof(IFakeResponseAble<TK>).IsAssignableFrom(typeof(T)))
            {
                var baseHttpRequest = this.Container.Resolve<IFactory<T>>().Create();
                var responseData = ((IFakeResponseAble<TK>)baseHttpRequest).FakeResponse();
                baseHttpRequest.Process(responseData);

                return responseData;
            }
#endif

            var parameters = this.SetParam<T, TK>(httpRequestData);

            var response = await this.TryGetResponse<T, TK>(httpRequestData, httpRequestDefinition.Route, parameters, jwtToken, includeBody, HTTPMethods.Get);

            return response;
        }

        #endregion

        #region Put

        public virtual void InitRequestPut(HTTPRequest request, object httpRequestData, string token) { this.InitBaseRequest(request, httpRequestData, token); }

        public async UniTask<TK> SendPutAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = false) where T : BasePutRequest<TK>
        {
            if (Attribute.GetCustomAttribute(typeof(T), typeof(HttpRequestDefinitionAttribute)) is not HttpRequestDefinitionAttribute httpRequestDefinition)
            {
                throw new Exception($"request {typeof(T)} wasn't defined yet!!! Please add HttpRequestDefinitionAttribute for it!!!!");
            }

#if (DEVELOPMENT_BUILD || UNITY_EDITOR) &&FAKE_DATA
            if (typeof(IFakeResponseAble<TK>).IsAssignableFrom(typeof(T)))
            {
                var baseHttpRequest = this.Container.Resolve<IFactory<T>>().Create();
                var responseData = ((IFakeResponseAble<TK>)baseHttpRequest).FakeResponse();
                baseHttpRequest.Process(responseData);

                return responseData;
            }
#endif

            var parameters = this.SetParam<T, TK>(httpRequestData);

            var response = await this.TryGetResponse<T, TK>(httpRequestData, httpRequestDefinition.Route, parameters, jwtToken, includeBody, HTTPMethods.Put);

            return response;
        }

        #endregion

        #region Pacth

        public virtual void InitRequestPatch(HTTPRequest request, object httpRequestData, string token) { this.InitBaseRequest(request, httpRequestData, token); }

        public async UniTask<TK> SendPatchAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = true) where T : BasePatchRequest<TK>
        {
            if (Attribute.GetCustomAttribute(typeof(T), typeof(HttpRequestDefinitionAttribute)) is not HttpRequestDefinitionAttribute httpRequestDefinition)
            {
                throw new Exception($"request {typeof(T)} wasn't defined yet!!! Please add HttpRequestDefinitionAttribute for it!!!!");
            }

#if (DEVELOPMENT_BUILD || UNITY_EDITOR) &&FAKE_DATA
            if (typeof(IFakeResponseAble<TK>).IsAssignableFrom(typeof(T)))
            {
                var baseHttpRequest = this.Container.Resolve<IFactory<T>>().Create();
                var responseData = ((IFakeResponseAble<TK>)baseHttpRequest).FakeResponse();
                baseHttpRequest.Process(responseData);

                return responseData;
            }
#endif

            var parameters = this.SetParam<T, TK>(httpRequestData);

            var response = await this.TryGetResponse<T, TK>(httpRequestData, httpRequestDefinition.Route, parameters, jwtToken, includeBody, HTTPMethods.Patch);

            return response;
        }

        #endregion

        #region Delete

        public virtual void InitDeleteRequest(HTTPRequest request, object httpRequestData, string token) { this.InitBaseRequest(request, httpRequestData, token); }

        public virtual async UniTask<TK> SendDeleteAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = true) where T : BaseDeleteRequest<TK>
        {
            if (Attribute.GetCustomAttribute(typeof(T), typeof(HttpRequestDefinitionAttribute)) is not HttpRequestDefinitionAttribute httpRequestDefinition)
            {
                throw new Exception($"request {typeof(T)} wasn't defined yet!!! Please add HttpRequestDefinitionAttribute for it!!!!");
            }

#if (DEVELOPMENT_BUILD || UNITY_EDITOR) &&FAKE_DATA
            if (typeof(IFakeResponseAble<TK>).IsAssignableFrom(typeof(T)))
            {
                var baseHttpRequest = this.Container.Resolve<IFactory<T>>().Create();
                var responseData = ((IFakeResponseAble<TK>)baseHttpRequest).FakeResponse();
                baseHttpRequest.Process(responseData);

                return responseData;
            }
#endif

            var parameters = this.SetParam<T, TK>(httpRequestData);

            var response = await this.TryGetResponse<T, TK>(httpRequestData, httpRequestDefinition.Route, parameters, jwtToken, includeBody, HTTPMethods.Delete);

            return response;
        }

        #endregion

        /// <summary>
        /// Temporary logic for download, streaming data
        /// </summary>
        /// <param name="address">Download uri</param>
        /// <param name="filePath">output file path</param>
        /// <param name="onDownloadProgress">% of download will be presented through this</param>
        public async UniTask Download(string address, string filePath, OnDownloadProgressDelegate onDownloadProgress)
        {
            filePath = this.GetDownloadPath(filePath);

            var request = new HTTPRequest(new Uri(address));
            request.Timeout            =  TimeSpan.FromSeconds(this.GetDownloadTimeout());
            request.OnDownloadProgress =  (httpRequest, downloaded, length) => onDownloadProgress(downloaded, length);
            request.OnStreamingData    += OnData;
            request.DisableCache       =  true;

            var response = await request.GetHTTPResponseAsync();

            if (request.Tag is FileStream fs)
                fs.Dispose();

            switch (request.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (response.IsSuccess)
                    {
                        base.Logger.Log($"Download {filePath} Done!");
                    }
                    else
                    {
                        base.Logger.Warning(string.Format(
                            "Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                            response.StatusCode, response.Message,
                            response.DataAsText));
                    }

                    break;

                default:
                    // There were an error while downloading the content.
                    // The incomplete file should be deleted.
                    File.Delete(filePath);

                    break;
            }

            bool OnData(HTTPRequest req, HTTPResponse resp, byte[] dataFragment, int dataFragmentLength)
            {
                if (resp.IsSuccess)
                {
                    if (!(req.Tag is FileStream fileStream))
                        req.Tag = fileStream = new FileStream(filePath, FileMode.Create);

                    fileStream.Write(dataFragment, 0, dataFragmentLength);
                }

                // Return true if dataFragment is processed so the plugin can recycle it
                return true;
            }
        }

        public async UniTask<byte[]> DownloadAndReadStreaming(string address, OnDownloadProgressDelegate onDownloadProgress)
        {
            var response = new byte[] { };
            var request  = new HTTPRequest(new Uri(address));
            request.Timeout            =  TimeSpan.FromSeconds(this.GetDownloadTimeout());
            request.OnDownloadProgress =  (httpRequest, downloaded, length) => onDownloadProgress(downloaded, length);
            request.OnStreamingData    += OnData;
            request.DisableCache       =  true;
            await request.GetHTTPResponseAsync();

            bool OnData(HTTPRequest req, HTTPResponse resp, byte[] dataFragment, int dataFragmentLength)
            {
                if (resp.IsSuccess)
                {
                    response = dataFragment;
                }

                return true;
            }

            return response;
        }

        public string GetDownloadPath(string path) => $"{Application.persistentDataPath}/{path}";

        protected double GetHttpTimeout() => this.NetworkConfig.HttpRequestTimeout;

        protected double GetDownloadTimeout() => this.NetworkConfig.DownloadRequestTimeout;

        public BoolReactiveProperty HasInternetConnection { get; set; } = new(true);
        public string               Host                  { get; set; }

        protected virtual StringBuilder SetParam<T, TK>(object httpRequestData) where T : BaseHttpRequest<TK>
        {
            var parameters    = new StringBuilder();
            var propertyInfos = httpRequestData.GetType().GetProperties();

            if (propertyInfos.Length > 0)
            {
                var parametersStr =
                    $"{this.NetworkConfig.ParamDelimiter}{propertyInfos.Select(propertyInfo => typeof(IEnumerable<string>).IsAssignableFrom(propertyInfo.PropertyType) ? (propertyInfo.GetValue(httpRequestData) as IEnumerable<string>)?.Select(value => $"{propertyInfo.Name}={value}").Join(this.NetworkConfig.ParamLink) : $"{propertyInfo.Name}={propertyInfo.GetValue(httpRequestData)}").Join(this.NetworkConfig.ParamLink)}";

                parameters.Append(parametersStr);
            }

            return parameters;
        }

        private async UniTask<TK> TryGetResponse<T, TK>(object httpRequestData, string route, StringBuilder parameters, string jwtToken, bool includeBody, HTTPMethods methods)
            where T : BaseHttpRequest<TK>
        {
            this.RetryCount[methods] = 0;
            var response     = default(TK);
            var request      = default(HTTPRequest);
            var canRetry     = true;
            var maximumRetry = this.NetworkConfig.MaximumRetryStatusCode0;

            if (Attribute.GetCustomAttribute(typeof(T), typeof(RetryAttribute)) is RetryAttribute retryAttribute)
            {
                maximumRetry = retryAttribute.RetryCount;
            }

            while (canRetry && response == null && this.RetryCount[methods] < maximumRetry)
            {
                try
                {
                    request         = new HTTPRequest(this.ReplaceUri($"{route}{parameters}"), methods);
                    request.Timeout = TimeSpan.FromSeconds(this.GetHttpTimeout());

                    if (includeBody)
                    {
                        switch (methods)
                        {
                            case HTTPMethods.Get:
                                this.InitGetRequest(request, httpRequestData, jwtToken);

                                break;
                            case HTTPMethods.Post:
                                this.InitPostRequest(request, httpRequestData, jwtToken);

                                break;
                            case HTTPMethods.Put:
                                this.InitRequestPut(request, httpRequestData, jwtToken);

                                break;
                            case HTTPMethods.Patch:
                                this.InitRequestPatch(request, httpRequestData, jwtToken);

                                break;
                            case HTTPMethods.Head:
                                break;
                            case HTTPMethods.Delete:
                                this.InitDeleteRequest(request, httpRequestData, jwtToken);

                                break;
                            case HTTPMethods.Merge:
                                break;
                            case HTTPMethods.Options:
                                break;
                            case HTTPMethods.Connect:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(methods), methods, null);
                        }
                    }

                    this.HasInternetConnection.Value = true;

                    response = await this.MainProcess<T, TK>(request, httpRequestData);
                    canRetry = false;
                }
                catch (AsyncHTTPException ex)
                {
                    if (ex.StatusCode == 0)
                    {
                        if (!this.NetworkConfig.AllowRetry)
                        {
                            this.RetryCount[methods] = maximumRetry;
                        }
                        else
                        {
                            this.RetryCount[methods]++;
                            this.HasInternetConnection.Value = true;
                            this.Logger.LogWithColor($"Retry {this.RetryCount[methods]} for request {request.Uri} Error detail:{ex.Message}, {ex.StatusCode}, {ex.Content}", Color.cyan);
                        }

                        if (this.RetryCount[methods] >= maximumRetry)
                        {
                            base.Logger.Log($"Request {request.Uri} Error");
                            this.HasInternetConnection.Value = false;
                            this.HandleAsyncHttpException(ex);
                        }
                    }
                    else
                    {
                        canRetry = false;
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(this.NetworkConfig.RetryDelay));
                }
            }

            return response;
        }

        protected Uri ReplaceUri(string route)
        {
            foreach (var keyValuePair in this.LocalData.ServerToken.ParameterNameToValue)
            {
                var parameterName  = keyValuePair.Key;
                var parameterValue = keyValuePair.Value;
                route = route.Replace($"{{{parameterName}}}", parameterValue);
            }

            var host = this.Host;

            return new Uri($"{host}{route}");
        }
    }
}