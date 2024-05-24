namespace GameFoundation.Scripts.Network.WebService
{
    using System;
    using System.Text;
    using BestHTTP;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Network.WebService.Requests;
    using GameFoundation.Scripts.Utilities.LogService;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Zenject;

    public abstract class BestHttpBaseProcess
    {
        protected BestHttpBaseProcess(ILogService logger, DiContainer container)
        {
            this.Logger    = logger;
            this.Container = container;
        }

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
                //
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
                        case CommonErrorCode.Unknown:
                            this.Logger.Error($"Code {statusCode}: Unknown error");

                            break;
                        case CommonErrorCode.NotFound:
                            this.Logger.Error($"Code {statusCode}: NotFound error");

                            break;
                        case CommonErrorCode.InvalidData:
                            this.Logger.Error($"Code {statusCode}: InvalidData error");

                            break;
                        case CommonErrorCode.InvalidBlueprint:
                            this.Logger.Error($"Code {statusCode}: InvalidBlueprint error");

                            break;
                        default:
                            //In the case server return a logic error but client didn't implement that logic yet 
                            try
                            {
                                this.Container.Resolve<IFactory<T>>().Create().ErrorProcess(statusCode);
                            }
                            catch (BaseHttpRequest.MissStatusCodeException e)
                            {
                                this.Logger.Error($"Didn't implement status Code: {statusCode} for {typeof(T)}");
                                this.Logger.Exception(e);
                            }

                            break;
                    }
                });

            return returnResponse;
        }

        //Deserialize then process response data when request success
        protected virtual TK RequestSuccessProcess<T, TK>(JObject responseData, object requestData)
            where T : BaseHttpRequest, IDisposable
        {
            var baseHttpRequest = this.Container.Resolve<IFactory<T>>().Create();
            var data            = responseData.ToObject<TK>();

            if (this.GetType().IsAssignableFrom(typeof(WrappedBestHttpService)))
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
                                this.Container.Resolve<IFactory<T>>().Create().ErrorProcess(new ErrorData()
                                {
                                    Code    = errorMessage.Code,
                                    Message = errorMessage.Message
                                });

                                this.Logger.Error(
                                    $"{req.Uri} request receive error code: {errorMessage.Code}-{errorMessage.Message}");

                                onRequestError(errorMessage.Code);
                            }
                        }
                        else
                        {
                            this.Logger.Error(
                                $"{req.Uri}- Request finished Successfully, but the server sent an error. Status Code: {resp.StatusCode}-{resp.Message} Message: {resp.DataAsText}");
                        }
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

        protected readonly ILogService Logger; // wrapped log 
        protected readonly DiContainer Container; // zenject container of this

        protected string uri { get; set; } // uri of service 

        #endregion
    }
}