namespace GameFoundation.Scripts.Network.WebService.Requests
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class WrappedHttpRequestData<T> : WrappedHttpRequestData
    {
        [JsonProperty("data")] public new T Data { get; set; }
    }

    public class WrappedHttpResponseData<T> : WrappedHttpResponseData
    {
        [JsonProperty("data")] public new T Data { get; set; }
    }

    public class WrappedHttpRequestData
    {
        [JsonProperty("data")] public object Data { get; set; }
    }

    /// <summary>
    ///     Wrapped data of http response. It will contain some additional data (request ID, events, RPC call...) beside
    ///     request data
    /// </summary>
    public class WrappedHttpResponseData
    {
        [JsonProperty("data")] public object                   Data     { get; set; }
        public                        Dictionary<string, long> Currency { get; set; } = new();
    }

    public static class CommonErrorCode
    {
        public const int Unknown             = 500;
        public const int NotFound            = 101;
        public const int InvalidData         = 102;
        public const int InvalidBlueprint    = 1001;
        public const int BadRequest          = 400;
        public const int Unauthorized        = 401;
        public const int RequestForbidden    = 403;
        public const int InternalServerError = 500;
    }

    public class ErrorResponse
    {
        public const int AUTH_ERROR_CODE_USER_ID_INVALID        = 1;
        public const int AUTH_ERROR_CODE_TOKEN_INVALID          = 2;
        public const int AUTH_ERROR_CODE_REFRESH_TOKEN_INVALID  = 3;
        public const int AUTH_ERROR_CODE_REFRESH_TOKEN_NOTFOUND = 4;
        public const int AUTH_ERROR_CODE_OTP_INVALID            = 5;
        public const int AUTH_ERROR_CODE_EMAIL_INVALID          = 6;

        public const int AUTH_ERROR_LINK_WALLET              = 7;
        public const int AUTH_ERROR_LINK_WALLET_NOT_FOUND    = 8;
        public const int AUTH_ERROR_LINK_USER_NOT_FOUND      = 9;
        public const int AUTH_ERROR_LINK_SIGNATURE           = 10;
        public const int AUTH_ERROR_EMAIL_IS_NOT_WHITELISTED = 11;

        public ErrorResponse()
        {
            this.Code    = CommonErrorCode.Unknown;
            this.Message = string.Empty;
        }

        public ErrorResponse(int code, string message)
        {
            this.Code    = code;
            this.Message = message;
        }

        [JsonProperty("statusCode")] public int    Code    { get; set; }
        [JsonProperty("message")]    public string Message { get; set; }
    }
}