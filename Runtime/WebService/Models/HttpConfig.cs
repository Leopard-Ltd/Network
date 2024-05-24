namespace GameFoundation.Scripts.Network.WebService.Models
{
    using GameFoundation.Scripts.Network.WebService.Interface;

    public class HttpConfig : IHttpConfig
    {
        public int    RequestTimeout         { get; set; }
        public int    DownloadTimeout        { get; set; }
        public string AuthorizationHeaderKey { get; set; }
    }
}