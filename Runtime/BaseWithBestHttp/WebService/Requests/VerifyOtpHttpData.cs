namespace BaseWithBestHttp.WebService.Requests
{
    using GameFoundation.Scripts.Utilities.Utils;

    /// <summary>Will response LoginResponseData</summary>
    [HttpRequestDefinition("otp/verify")]
    public class VerifyOtpRequestData
    {
        public string Email { get; set; }
        public string Code  { get; set; }
    }
}