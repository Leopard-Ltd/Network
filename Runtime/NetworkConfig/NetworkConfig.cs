namespace NetworkConfig
{
    using Models;
    using UnityEngine;

    /// <summary>Our global network config for HttpRequest and SignalR.</summary>
    public class NetworkConfig : ScriptableObject, IGameConfig
    {
        public double httpRequestTimeout     = 10; // Default timeout for all http request
        public double downloadRequestTimeout = 600; // Default timeout for download
    }
}