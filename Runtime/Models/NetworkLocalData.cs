namespace Models
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Interfaces;

    public class NetworkLocalData : ILocalData, IIgnoreCreateFromReflection
    {
        public ServerToken ServerToken = new ServerToken();

        public void Init() { }
    }

    public class ServerToken
    {
        public Dictionary<string, string> ParameterNameToValue { get; set; } = new();
    }
}