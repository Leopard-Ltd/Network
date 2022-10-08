namespace Editor
{
    using System.Linq;
    using Editor.GDKManager;
    using Models;
    using NetworkConfig;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class NetworkConfigEditor : BaseGameConfigEditor
    {
        private string[]     defaultServerConfigs = new string[] { "Local", "Develop", "Staging", "Release" };
        private ServerConfig currentServerConfig;

        public NetworkConfigEditor(GDKConfig gdkConfig) : base(gdkConfig)
        {
        }
        public override void PreSetup()
        {
            this.GdkConfig.AddGameConfig(this.CreateInstanceInResource<NetworkConfig>($"NetworkConfig", "GameConfigs"));
            var listServerConfigs =
                this.defaultServerConfigs.ToDictionary(serverConfig => serverConfig, serverConfig => this.CreateInstanceInResource<ServerConfig>($"{serverConfig}ServerConfig", "GameConfigs"));

            this.currentServerConfig = listServerConfigs["Develop"];
            this.GdkConfig.AddGameConfig(this.currentServerConfig);
        }
        public override VisualElement LoadView()
        {
            var template = EditorGUIUtility.Load("Packages/com.gdk.network/Editor/NetworkConfigEditor.uxml") as VisualTreeAsset;
            if (template != null)
            {
                this.Add(template.CloneTree());

                // display network config
                var networkConfigElement = this.Q<VisualElement>("NetworkConfig");
                networkConfigElement.Add(this.GdkConfig.GetGameConfig<NetworkConfig>().CreateUIElementInspector());
                
                // display server config
                var serverConfigElement = this.Q<VisualElement>("ServerConfig");

                var serverConfigPicker = serverConfigElement.Q<ObjectField>("ServerConfigPicker");
                this.currentServerConfig = this.GdkConfig.GetGameConfig<ServerConfig>();
                serverConfigPicker.SetValueWithoutNotify(this.currentServerConfig);
                serverConfigPicker.RegisterValueChangedCallback(this.OnChangeServerConfig);

                this.UpdateServerConfigInspector(this.currentServerConfig);

            }

            return this;
        }
        
        private void OnChangeServerConfig(ChangeEvent<Object> evt)
        {
            var newServerConfig = evt.newValue as ServerConfig;
            this.UpdateServerConfigInspector(newServerConfig);
        }

        private VisualElement currentVisualConfigInspector;
        private void UpdateServerConfigInspector(ServerConfig newServerConfig)
        {
            this.GdkConfig.RemoveGameConfig(this.currentServerConfig);
            this.GdkConfig.AddGameConfig(newServerConfig);
            
            var serverConfigElement = this.Q<VisualElement>("ServerConfig");
            if (this.currentVisualConfigInspector != null)
            {
                serverConfigElement.Remove(this.currentVisualConfigInspector);
            }

            this.currentVisualConfigInspector = newServerConfig.CreateUIElementInspector();
            serverConfigElement.Add(this.currentVisualConfigInspector);
        }
    }
}