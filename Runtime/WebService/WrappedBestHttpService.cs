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

    public class WrappedBestHttpService : BestHttpBaseProcess, IHttpService
    {
        protected readonly ILogService      logger;
        protected readonly NetworkConfig    networkConfig;
        protected readonly NetworkLocalData localData;

        public WrappedBestHttpService(ILogService logger, DiContainer container, NetworkConfig networkConfig, NetworkLocalData localData) : base(logger, container)
        {
            this.logger        = logger;
            this.networkConfig = networkConfig;
            this.localData     = localData;
        }

        public string Host { get; set; }

        #region Post

        public virtual void InitPostRequest(HTTPRequest request, object httpRequestData, string token)
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

            //Init request
            var request = new HTTPRequest(this.ReplaceUri(httpRequestDefinition.Route), HTTPMethods.Post);
            request.Timeout = TimeSpan.FromSeconds(this.GetHttpTimeout());

            this.InitPostRequest(request, httpRequestData, jwtToken);

            try
            {
                this.HasInternetConnection.Value = true;

                return await this.MainProcess<T, TK>(request, httpRequestData);
            }
            catch (AsyncHTTPException ex)
            {
                this.Logger.Log($"Request {request.Uri} Error");
                this.HasInternetConnection.Value = false;
                this.HandleAsyncHttpException(ex);

                return default;
            }
        }

        #endregion

        #region Get

        public virtual void InitGetRequest(HTTPRequest request, object httpRequestData, string token)
        {
            
            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            request.AddHeader("Content-Type", "application/json");
            request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(httpRequestData));
        }

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

            var parameters    = new StringBuilder();
            var propertyInfos = httpRequestData.GetType().GetProperties();

            if (propertyInfos.Length > 0)
            {
                var parametersStr =
                    $"?{propertyInfos.Select(propertyInfo => typeof(IEnumerable<string>).IsAssignableFrom(propertyInfo.PropertyType) ? (propertyInfo.GetValue(httpRequestData) as IEnumerable<string>)?.Select(value => $"{propertyInfo.Name}={value}").Join("&") : $"{propertyInfo.Name}={propertyInfo.GetValue(httpRequestData)}").Join("&")}";

                parameters.Append(parametersStr);
            }

            var httpRequest = new HTTPRequest(this.ReplaceUri($"{httpRequestDefinition.Route}{parameters}"), HTTPMethods.Get);
            httpRequest.Timeout = TimeSpan.FromSeconds(this.GetHttpTimeout());

            if (includeBody)
            {
                this.InitGetRequest(httpRequest, httpRequestData, jwtToken);
            }

            try
            {
                this.HasInternetConnection.Value = true;

                return await this.MainProcess<T, TK>(httpRequest, httpRequestData);
            }
            catch (AsyncHTTPException ex)
            {
                this.Logger.Log($"Request {httpRequest.Uri} Error");
                this.HasInternetConnection.Value = false;
                this.HandleAsyncHttpException(ex);

                return default;
            }
        }

        #endregion

        #region Put

        public virtual void InitRequestPut(HTTPRequest request, object httpRequestData, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            request.AddHeader("Content-Type", "application/json");
            request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(httpRequestData));
        }

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

            var parameters    = new StringBuilder();
            var propertyInfos = httpRequestData.GetType().GetProperties();

            if (propertyInfos.Length > 0)
            {
                var parametersStr =
                    $"?{propertyInfos.Select(propertyInfo => typeof(IEnumerable<string>).IsAssignableFrom(propertyInfo.PropertyType) ? (propertyInfo.GetValue(httpRequestData) as IEnumerable<string>)?.Select(value => $"{propertyInfo.Name}={value}").Join("&") : $"{propertyInfo.Name}={propertyInfo.GetValue(httpRequestData)}").Join("&")}";

                parameters.Append(parametersStr);
            }

            var httpRequest = new HTTPRequest(this.ReplaceUri($"{httpRequestDefinition.Route}{parameters}"), HTTPMethods.Put);
            httpRequest.Timeout = TimeSpan.FromSeconds(this.GetHttpTimeout());

            if (includeBody)
            {
                this.InitRequestPut(httpRequest, httpRequestData, jwtToken);
            }

            try
            {
                this.HasInternetConnection.Value = true;

                return await this.MainProcess<T, TK>(httpRequest, httpRequestData);
            }
            catch (AsyncHTTPException ex)
            {
                this.Logger.Log($"Request {httpRequest.Uri} Error");
                this.HasInternetConnection.Value = false;
                this.HandleAsyncHttpException(ex);

                return default;
            }
        }

        #endregion

        #region Pacth

        public virtual void InitRequestPatch(HTTPRequest request, object httpRequestData, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            request.AddHeader("Content-Type", "application/json");
            request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(httpRequestData));
        }

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

            var parameters    = new StringBuilder();
            var propertyInfos = httpRequestData.GetType().GetProperties();

            if (propertyInfos.Length > 0)
            {
                var parametersStr =
                    $"?{propertyInfos.Select(propertyInfo => typeof(IEnumerable<string>).IsAssignableFrom(propertyInfo.PropertyType) ? (propertyInfo.GetValue(httpRequestData) as IEnumerable<string>)?.Select(value => $"{propertyInfo.Name}={value}").Join("&") : $"{propertyInfo.Name}={propertyInfo.GetValue(httpRequestData)}").Join("&")}";

                parameters.Append(parametersStr);
            }

            var httpRequest = new HTTPRequest(this.ReplaceUri($"{httpRequestDefinition.Route}{parameters}"), HTTPMethods.Patch);
            httpRequest.Timeout = TimeSpan.FromSeconds(this.GetHttpTimeout());

            if (includeBody)
            {
                this.InitRequestPatch(httpRequest, httpRequestData, jwtToken);
            }

            try
            {
                this.HasInternetConnection.Value = true;

                return await this.MainProcess<T, TK>(httpRequest, httpRequestData);
            }
            catch (AsyncHTTPException ex)
            {
                this.Logger.Log($"Request {httpRequest.Uri} Error");
                this.HasInternetConnection.Value = false;
                this.HandleAsyncHttpException(ex);

                return default;
            }
        }

        #endregion

        #region Delete

        public virtual void InitDeleteRequest(HTTPRequest request, object httpRequestData, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            request.AddHeader("Content-Type", "application/json");
            request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(httpRequestData));
        }

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

            var parameters    = new StringBuilder();
            var propertyInfos = httpRequestData.GetType().GetProperties();

            if (propertyInfos.Length > 0)
            {
                var parametersStr =
                    $"?{propertyInfos.Select(propertyInfo => typeof(IEnumerable<string>).IsAssignableFrom(propertyInfo.PropertyType) ? (propertyInfo.GetValue(httpRequestData) as IEnumerable<string>)?.Select(value => $"{propertyInfo.Name}={value}").Join("&") : $"{propertyInfo.Name}={propertyInfo.GetValue(httpRequestData)}").Join("&")}";

                parameters.Append(parametersStr);
            }

            var httpRequest = new HTTPRequest(this.ReplaceUri($"{httpRequestDefinition.Route}{parameters}"), HTTPMethods.Delete);
            httpRequest.Timeout = TimeSpan.FromSeconds(this.GetHttpTimeout());

            if (includeBody)
            {
                this.InitDeleteRequest(httpRequest, httpRequestData, jwtToken);
            }

            try
            {
                this.HasInternetConnection.Value = true;

                return await this.MainProcess<T, TK>(httpRequest, httpRequestData);
            }
            catch (AsyncHTTPException ex)
            {
                this.Logger.Log($"Request {httpRequest.Uri} Error");
                this.HasInternetConnection.Value = false;
                this.HandleAsyncHttpException(ex);

                return default;
            }
        }

        #endregion

        //TODO need to test and improve code here, this is just a temporary logic
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
                        this.Logger.Log($"Download {filePath} Done!");
                    }
                    else
                    {
                        this.Logger.Warning(string.Format(
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

        protected double GetHttpTimeout() => this.networkConfig.HttpRequestTimeout;

        protected double GetDownloadTimeout() => this.networkConfig.DownloadRequestTimeout;

        public BoolReactiveProperty HasInternetConnection                                        { get; set; } = new(true);
        public void                 InitPostRequest(HTTPRequest request, object httpRequestData) { throw new NotImplementedException(); }

        protected Uri ReplaceUri(string route)
        {
            foreach (var keyValuePair in this.localData.ServerToken.ParameterNameToValue)
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