namespace GameFoundation.Scripts.Network
{
    /// <summary>Our global network config for HttpRequest and SignalR.</summary>
    public class NetworkConfig
    {
        public string Host                    { get; set; } // our web service server URI
        public double HttpRequestTimeout      { get; set; } = 30; // Default timeout for all http request
        public double DownloadRequestTimeout  { get; set; } = 600; // Default timeout for download
        public string BattleWebsocketUri      { get; set; } //our websocket service server URI
        public int    MaximumRetryStatusCode0 { get; set; } = 5; // Maximum retry for status code 0
        public float  RetryDelay              { get; set; } = 0.1f;
        public bool   AllowRetry              { get; set; } = true;
        public string ParamLink      = "&";
        public string ParamDelimiter = "?";

        public static string WrapFull => "wrap";

        public static string WrapRequest  => "wrap_request";
        public static string WrapResponse => "wrap_response";
    }
}