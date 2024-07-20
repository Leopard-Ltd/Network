﻿namespace GameFoundation.Scripts.Network.WebService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using BestHTTP;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Network.WebService.Requests;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.Utils;
    using global::Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using R3;
    using UnityEngine;
    using Zenject;
    using Zenject.Internal;

    public abstract class BestBaseHttpProcess : IHttpService, IInitializable, IDisposable
    {
        protected readonly ILogService      Logger; // wrapped log 
        protected readonly NetworkLocalData LocalData;
        protected readonly NetworkConfig    NetworkConfig;
        protected readonly DiContainer      Container; // zenject container of this

        protected BestBaseHttpProcess(ILogService logger, NetworkLocalData LocalData, NetworkConfig networkConfig, DiContainer container)
        {
            this.Logger        = logger;
            this.LocalData     = LocalData;
            this.NetworkConfig = networkConfig;
            this.Container     = container;
        }

        protected Dictionary<HTTPMethods, int> RetryCount = new();

        public virtual void Initialize()
        {
            this.RetryCount[HTTPMethods.Post]   = 0;
            this.RetryCount[HTTPMethods.Get]    = 0;
            this.RetryCount[HTTPMethods.Put]    = 0;
            this.RetryCount[HTTPMethods.Patch]  = 0;
            this.RetryCount[HTTPMethods.Delete] = 0;
        }

        public virtual void Dispose() { }

        #region MainProcess

        protected virtual void InitBaseRequest(HTTPRequest request, object httpRequestData, string token)
        {
            if (this is IWrapRequest wrapRequest)
            {
                this.InitWrapData(request, httpRequestData, token);
            }
            else
            {
                this.InitNoWrapData(request, httpRequestData, token);
            }
        }

        protected virtual void InitWrapData(HTTPRequest request, object httpRequestData, string token)
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

        protected virtual void InitNoWrapData(HTTPRequest request, object httpRequestData, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }

            if (!string.IsNullOrEmpty(GameVersion.Version))
            {
                request.AddHeader("game-version", GameVersion.Version);
            }

            request.AddHeader("Content-Type", "application/json");
            request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(httpRequestData));
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

        protected double GetHttpTimeout() => this.NetworkConfig.HttpRequestTimeout;

        protected double GetDownloadTimeout() => this.NetworkConfig.DownloadRequestTimeout;

        public ReactiveProperty<bool> HasInternetConnection { get; set; } = new(true);
        public ReactiveProperty<bool> IsProcessApi          { get; set; } = new(false);
        public string                 Host                  { get; set; }

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
            this.IsProcessApi.Value  = true;
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
                            this.Logger.Log($"Request {request.Uri} Error");
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

            this.IsProcessApi.Value = false;

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

        #endregion

        protected async UniTask<TK> MainProcess<T, TK>(HTTPRequest request, object requestData)
            where T : BaseHttpRequest, IDisposable
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD||SHOW_API_LOG
            try
            {
                this.Logger.Log(
                    $"{request.Uri} - [REQUEST] - Header: {request.DumpHeaders()} - \n Body:{Encoding.UTF8.GetString(request.GetEntityBody())}");
            }
            catch (Exception e)
            {
                this.Logger.Error($"{e.Message} - {e.StackTrace}");
            }

#endif
            var response = await request.GetHTTPResponseAsync();
#if UNITY_EDITOR || DEVELOPMENT_BUILD||SHOW_API_LOG
            this.Logger.Log($"{request.Uri} - [RESPONSE] - raw data: {response.DataAsText}");
#endif
            TK returnResponse = default;

            this.PreProcess<T>(request, response, (statusCode) =>
                {
                    var responseData = JObject.Parse(Encoding.UTF8.GetString(response.Data));
                    returnResponse = this.RequestSuccessProcess<T, TK>(responseData, requestData);
                },
                (statusCode) =>
                {
                    switch (statusCode)
                    {
                        // case CommonErrorCode.Unknown:
                        //     this.Logger.Error($"Code {statusCode}: Unknown error");
                        //
                        //     break;
                        // case CommonErrorCode.NotFound:
                        //     this.Logger.Error($"Code {statusCode}: NotFound error");
                        //
                        //     break;
                        // case CommonErrorCode.InvalidData:
                        //     this.Logger.Error($"Code {statusCode}: InvalidData error");
                        //
                        //     break;
                        // case CommonErrorCode.InvalidBlueprint:
                        //     this.Logger.Error($"Code {statusCode}: InvalidBlueprint error");
                        //
                        //     break;
                        default:

                            var model  = JsonConvert.DeserializeObject<RootErrorData>(response.DataAsText);
                            var model1 = JsonConvert.DeserializeObject<ErrorData>(response.DataAsText);

                            if (!string.IsNullOrEmpty(model1.Message))
                            {
                                this.Container.Resolve<IFactory<T>>().Create().ErrorProcess(new ErrorData()
                                {
                                    Code                    = model1.Code,
                                    Message                 = model1.Message,
                                    Name                    = response.Message,
                                    HttpErrorCode           = model1.HttpErrorCode,
                                    HttpErrorCodeFromHeader = statusCode
                                });

                                this.Logger.Error(
                                    $"{request.Uri} request receive error code: {statusCode}-{model1.Code}-{model1.Message}");
                            }
                            else
                            {
                                if (model != null)
                                {
                                    this.Container.Resolve<IFactory<T>>().Create().ErrorProcess(new ErrorData()
                                    {
                                        Code                    = model.ErrorData.Code,
                                        Message                 = model.ErrorData.Message,
                                        Name                    = response.Message,
                                        HttpErrorCode           = model.ErrorData.HttpErrorCode,
                                        HttpErrorCodeFromHeader = statusCode
                                    });

                                    this.Logger.Error(
                                        $"{request.Uri} request receive error code: {statusCode}-{model.ErrorData.Code}-{model.ErrorData.Message}");
                                }
                            }

                            // try
                            // {
                            //     this.Container.Resolve<IFactory<T>>().Create().ErrorProcess(statusCode);
                            // }
                            // catch (BaseHttpRequest.MissStatusCodeException e)
                            // {
                            //     this.Logger.Error($"Didn't implement status Code: {statusCode} for {typeof(T)}");
                            //     this.Logger.Exception(e);
                            // }

                            break;
                    }
                });

            return returnResponse;
        }

        //Deserialize then process response data when request success
        protected virtual TK RequestSuccessProcess<T, TK>(JObject responseData, object requestData)
            where T : BaseHttpRequest, IDisposable
        {
            var baseHttpRequest = this.Container.Resolve<T>();
            var data            = responseData.ToObject<TK>();

            if (this is IWrapResponse wrapResponse)
            {
                if (responseData.TryGetValue("data", out var requestProcessData))
                {
                    data = requestProcessData.ToObject<TK>();
                    baseHttpRequest.Process(data);
                }
            }
            else
            {
                baseHttpRequest.Process(data);
            }

            baseHttpRequest.PredictProcess(requestData);
            this.PostProcess();

            return data;
        }

        /// <summary>Handle errors that are defined by Best Http/2, return false of there is any error, otherwise return true</summary>
        protected void PreProcess<T>(HTTPRequest req, HTTPResponse resp, RequestSuccess onRequestSuccess,
            RequestError onRequestError) where T : BaseHttpRequest, IDisposable
        {
            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        onRequestSuccess(resp.StatusCode);
                    }
                    else
                    {
                        //Specific error for each requests
                        if (resp.StatusCode == 400)
                        {
                            var errorMessage = JsonConvert.DeserializeObject<ErrorResponse>(resp.DataAsText);

                            if (errorMessage != null)
                            {
                                // this.Container.Resolve<IFactory<T>>().Create().ErrorProcess(new ErrorData()
                                // {
                                //     Code    = errorMessage.Code,
                                //     Message = errorMessage.Message
                                // });

                                this.Logger.Error(
                                    $"{req.Uri} request receive error code: {errorMessage.Code}-{errorMessage.Message}");
                            }
                        }
                        else
                        {
                            this.Logger.Error(
                                $"{req.Uri}- Request finished Successfully, but the server sent an error. Status Code: {resp.StatusCode}-{resp.Message} Message: {resp.DataAsText}");
                        }

                        onRequestError(resp.StatusCode);
                    }

                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    this.Logger.Error("Request Finished with Error! " + (req.Exception != null
                        ? (req.Exception.Message + "\n" + req.Exception.StackTrace)
                        : "No Exception"));

                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    this.Logger.Warning("Request Aborted!");

                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    this.Logger.Error("Connection Timed Out!");

                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    this.Logger.Error("Processing the request Timed Out!");

                    break;
                case HTTPRequestStates.Initial:
                    break;
                case HTTPRequestStates.Queued:
                    break;
                case HTTPRequestStates.Processing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>Handler unexpected exceptions of http requests.</summary>
        protected void HandleAsyncHttpException(AsyncHTTPException ex)
        {
            this.Logger.Log("Status Code: " + ex.StatusCode);
            this.Logger.Log("Message: " + ex.Message);
            this.Logger.Log("Content: " + ex.Content);
        }

        /// <summary>Run common logic like common error, analysis, events...</summary>
        private void PostProcess()
        {
            // Do something here
        }

        protected Uri GetUri(string route) => new Uri($"{this.uri}{route}");

        protected delegate void RequestSuccess(int statusCode);

        protected delegate void RequestError(int statusCode);

        #region Injection

        protected string uri { get; set; } // uri of service 

        #endregion
    }
}