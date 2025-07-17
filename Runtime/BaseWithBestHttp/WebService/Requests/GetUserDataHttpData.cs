namespace BaseWithBestHttp.WebService.Requests
{
    using BaseWithBestHttp.WebService.Models.UserData;
    using GameFoundation.Scripts.Utilities.Utils;

    [HttpRequestDefinition("user/data/get")]
    public class GetUserDataRequestData
    {
    }

    public class GetUserDataResponseData
    {
        public UserData UserData { get; set; } = new();
    }
}