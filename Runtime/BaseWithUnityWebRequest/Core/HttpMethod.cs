using UnityEngine.Networking;

namespace BaseWithUnityWebRequest.Core
{
    public static class HttpMethod
    {
        public const string Get    = UnityWebRequest.kHttpVerbGET;
        public const string Post   = UnityWebRequest.kHttpVerbPOST;
        public const string Put    = UnityWebRequest.kHttpVerbPUT;
        public const string Delete = UnityWebRequest.kHttpVerbDELETE;
        public const string Patch  = "PATCH";
    }
}