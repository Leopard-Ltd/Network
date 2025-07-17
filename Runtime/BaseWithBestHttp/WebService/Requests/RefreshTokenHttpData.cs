namespace BaseWithBestHttp.WebService.Requests
{
    using GameFoundation.Scripts.Utilities.Utils;

    [HttpRequestDefinition("login/refresh")]
    public class RefreshTokenRequestData
    {
        public string RefreshToken { get; set; }
    }

    public class RefreshTokenResponseData : LoginResponseData
    {
    }
}