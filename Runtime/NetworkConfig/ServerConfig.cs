namespace NetworkConfig
{
    using Models;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ServerConfig", menuName = "GDK/Server Config data")]
    public class ServerConfig : ScriptableObject, IGameConfig
    {
        public string version = "0.0.0";
        public string host = "https://dev.host.com/v1";
    }
}