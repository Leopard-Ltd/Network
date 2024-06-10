namespace GameFoundation.Scripts.Network.WebService
{
    using System.Text;
    using GameFoundation.Scripts.Utilities.LogService;
    using global::Models;
    using Zenject;

    public class NoWrappedRequestAndResponseService : BestBaseHttpProcess
    {
        public NoWrappedRequestAndResponseService(ILogService logger, NetworkLocalData LocalData, NetworkConfig networkConfig, DiContainer container) : base(logger, LocalData, networkConfig,
            container)
        {
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