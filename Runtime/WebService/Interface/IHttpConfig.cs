namespace GameFoundation.Scripts.Network.WebService.Interface
{
    public interface IHttpConfig
    {
        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        int RequestTimeout { get; }

        /// <summary>
        /// Download timeout in seconds
        /// </summary>
        int DownloadTimeout { get; }

        string AuthorizationHeaderKey { get; }
    }
}