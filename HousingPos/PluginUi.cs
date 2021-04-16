using System;
using HousingPos.Gui;

namespace HousingPos
{
    public class PluginUi : IDisposable
    {
        private readonly HousingPos _plugin;
        public ConfigurationWindow ConfigWindow { get; }

        public PluginUi(HousingPos plugin)
        {
            ConfigWindow = new ConfigurationWindow(plugin);

            _plugin = plugin;
            _plugin.Interface.UiBuilder.OnBuildUi += Draw;
            _plugin.Interface.UiBuilder.OnOpenConfigUi += OnOpenConfigUi;
        }

        private void Draw()
        {
            ConfigWindow.Draw();
        }
        private void OnOpenConfigUi(object sender, EventArgs args)
        {
            ConfigWindow.Visible = true;
            ConfigWindow.CanUpload = false;
            ConfigWindow.CanImport = false;
        }

        public void Dispose()
        {
            _plugin.Interface.UiBuilder.OnBuildUi -= Draw;
            _plugin.Interface.UiBuilder.OnOpenConfigUi -= OnOpenConfigUi;
        }
    }
}