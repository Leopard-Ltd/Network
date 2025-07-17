namespace BaseWithBestHttp.WebService.Models
{
    using BaseWithBestHttp.WebService.Interface;

    public class HttpConfig : IHttpConfig
    {
        public int    RequestTimeout         { get; set; }
        public int    DownloadTimeout        { get; set; }
        public string AuthorizationHeaderKey { get; set; }
    }
}