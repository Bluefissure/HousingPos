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
            HousingPos.Interface.UiBuilder.Draw += Draw;
            HousingPos.Interface.UiBuilder.OpenConfigUi += OnOpenConfigUi;
        }

        private void Draw()
        {
            ConfigWindow.Draw();
        }
        private void OnOpenConfigUi()
        {
            ConfigWindow.Visible = true;
            ConfigWindow.CanUpload = false;
            ConfigWindow.CanImport = false;
        }

        public void Dispose()
        {
            HousingPos.Interface.UiBuilder.Draw -= Draw;
            HousingPos.Interface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
        }
    }
}