namespace GameFoundation.Scripts.Network.WebService.Requests
{
    using GameFoundation.Scripts.Network.WebService.Interface;
    using GameFoundation.Scripts.Utilities.Utils;

    [HttpRequestDefinition("blueprint/get")]
    public class GetBlueprintRequestData
    {
        [Required] public string Version { set; get; }
    }

    public class GetBlueprintResponseData
    {
        public string Url  { set; get; }
        public string Hash { set; get; }
    }
}