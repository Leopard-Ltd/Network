namespace GameFoundation.Scripts.Network.WebService
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using BestHTTP;
    using GameFoundation.Scripts.Utilities.LogService;
    using global::Models;
    using ModestTree;
    using Newtonsoft.Json;
    using Zenject;

    public class NoWrappedService : WrappedBestHttpService
    {
        public NoWrappedService(ILogService logger, DiContainer container, NetworkConfig networkConfig, NetworkLocalData localData) : base(logger, container, networkConfig, localData) { }

        protected override void InitBaseRequest(HTTPRequest request, object httpRequestData, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", "Bearer " + token);
            }
            if (!string.IsNullOrEmpty(GameVersion.Version))
            {
                request.AddHeader("game-version", GameVersion.Version);
            }
            request.AddHeader("Content-Type", "application/json");
            request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(httpRequestData));
        }

        protected override StringBuilder SetParam<T, TK>(object httpRequestData)
        {
            return base.SetParam<T, TK>(httpRequestData);
            // var parameters    = new StringBuilder();
            // var propertyInfos = httpRequestData.GetType().GetProperties();
            //
            // if (propertyInfos.Length <= 0) return parameters;
            //
            // var parametersStr =
            //     $"?{propertyInfos.Select(propertyInfo => typeof(IEnumerable<string>).IsAssignableFrom(propertyInfo.PropertyType) ? (propertyInfo.GetValue(httpRequestData) as IEnumerable<string>)?.Select(value => $"{propertyInfo.Name}={value}").Join("&") : $"{propertyInfo.Name}={propertyInfo.GetValue(httpRequestData)}").Join("&")}";
            //
            // parameters.Append(parametersStr);
            //
            // return parameters;
        }
    }
}