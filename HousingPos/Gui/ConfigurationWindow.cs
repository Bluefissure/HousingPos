using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Chat;
using ImGuiNET;
using HousingPos.Objects;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HousingPos.Gui
{
    public class ConfigurationWindow : Window<HousingPos>
    {

        public Configuration Config => Plugin.Config;
        private readonly string[] _languageList;
        private int _selectedLanguage;
        private Localizer _localizer;

        public ConfigurationWindow(HousingPos plugin) : base(plugin)
        {
            _languageList = new string[] { "en", "zh" };
            _localizer = new Localizer(Config.UILanguage);
        }

        protected override void DrawUi()
        {
            ImGui.SetNextWindowSize(new Vector2(530, 450), ImGuiCond.FirstUseEver);
            if (!ImGui.Begin($"{Plugin.Name} {_localizer.Localize("Panel")}", ref WindowVisible, ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.End();
                return;
            }
            if (ImGui.BeginChild("##SettingsRegion"))
            {
                DrawGeneralSettings();
                if (ImGui.BeginChild("##ItemListRegion"))
                {
                    DrawItemList();
                    ImGui.EndChild();
                }
                ImGui.EndChild();
            }
            
        }

        

        private void DrawGeneralSettings()
        {
            
            ImGui.TextUnformatted(_localizer.Localize("Language:"));
            if (Plugin.Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(_localizer.Localize("Change the UI Language."));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            if (ImGui.Combo("##hideLangSetting", ref _selectedLanguage, _languageList, _languageList.Length))
            {
                Config.UILanguage = _languageList[_selectedLanguage];
                _localizer.Language = Config.UILanguage;
                Config.Save();
            }
            ImGui.SameLine(ImGui.GetColumnWidth() - 80);
            ImGui.TextUnformatted(_localizer.Localize("Tooltips"));
            ImGui.AlignTextToFramePadding();
            ImGui.SameLine();
            if (ImGui.Checkbox("##hideTooltipsOnOff", ref Config.ShowTooltips)) Config.Save();

            if (ImGui.Checkbox(_localizer.Localize("BDTH"), ref Config.BDTH)) Config.Save();
            if (Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(_localizer.Localize("BDTH integrate: leave the position set to BDTH. \n" +
                    "(Note that BDTH cannot set rotation.)"));
            /*
            if (ImGui.Checkbox(_localizer.Localize("Force Move"), ref Config.ForceMove)) Config.Save();
            if (Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(_localizer.Localize("Force the position when moving items (cannot be seen until re-enter)."));
            */
            ImGui.Text("X:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputFloat("##placeX", ref Config.PlaceX, 0.01f, 0.1f))
            {
                Config.Save();
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.Text("Y:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputFloat("##placeY", ref Config.PlaceY, 0.01f, 0.1f))
            {
                Config.Save();
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.Text("Z:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputFloat("##placeZ", ref Config.PlaceZ, 0.01f, 0.1f))
            {
                Config.Save();
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.Text(_localizer.Localize("Rotate:"));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputFloat("##placeRotate", ref Config.PlaceRotate, 0.01f, 0.1f))
            {
                Config.Save();
            }
        }

        private void DrawItemList()
        {
            ImGui.Columns(1);
            if (ImGui.Button(_localizer.Localize("Clear")))
            {
                Config.HousingItemList.Clear();
                Config.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button(_localizer.Localize("Sort")))
            {
                Config.SelectedItemIndex = -1;
                Config.HousingItemList.Sort((x, y) => {
                    if (x.ItemKey.CompareTo(y.ItemKey) != 0)
                        return x.ItemKey.CompareTo(y.ItemKey);
                    if (x.X.CompareTo(y.X) != 0)
                        return x.X.CompareTo(y.X);
                    if (x.Y.CompareTo(y.Y) != 0)
                        return x.Y.CompareTo(y.Y);
                    if (x.Z.CompareTo(y.Z) != 0)
                        return x.Z.CompareTo(y.Z);
                    if (x.Rotate.CompareTo(y.Rotate) != 0)
                        return x.Rotate.CompareTo(y.Rotate);
                    return 0;
                });
                Config.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button(_localizer.Localize("Copy")))
            {
                try
                {
                    string str = _localizer.Localize("Only for purchasing, please use Export/Import for the whole preset.\n");
                    for (int i = 0; i < Config.HousingItemList.Count(); i++)
                    {
                        var housingItem = Config.HousingItemList[i];
                        str += $"item#{housingItem.ItemKey} {housingItem.Name}\n";
                    }
                    Win32Clipboard.CopyTextToClipboard(str);
                    Plugin.Log(String.Format(_localizer.Localize("Copied {0} items to your clipboard."), Config.HousingItemList.Count));
                }
                catch (Exception e)
                {
                    Plugin.LogError($"Error while exporting all items: {e.Message}");
                }
            }
            ImGui.SameLine();
            if (ImGui.Button(_localizer.Localize("Export")))
            {
                try
                {
                    string str = JsonConvert.SerializeObject(Config.HousingItemList);
                    Win32Clipboard.CopyTextToClipboard(str);
                    Plugin.Log(String.Format(_localizer.Localize("Exported {0} items to your clipboard."), Config.HousingItemList.Count));
                }
                catch (Exception e)
                {
                    Plugin.LogError($"Error while exporting items: {e.Message}");
                }
            }
            ImGui.SameLine();
            if (ImGui.Button(_localizer.Localize("Import")))
            {
                try
                {
                    string str = ImGui.GetClipboardText();
                    Config.HousingItemList = JsonConvert.DeserializeObject<List<HousingItem>>(str);
                    Config.Save();
                    Plugin.Log(String.Format(_localizer.Localize("Imported {0} items from your clipboard."), Config.HousingItemList.Count));
                }
                catch (Exception e)
                {
                    Plugin.LogError($"Error while importing items: {e.Message}");
                }
            }
            // name, x, t, z, r, set
            int columns = Config.BDTH ? 6 : 5;
            ImGui.Columns(columns, "ItemList", true);
            ImGui.Separator();
            ImGui.Text(_localizer.Localize("Name")); ImGui.NextColumn();
            ImGui.Text(_localizer.Localize("X")); ImGui.NextColumn();
            ImGui.Text(_localizer.Localize("Y")); ImGui.NextColumn();
            ImGui.Text(_localizer.Localize("Z")); ImGui.NextColumn();
            ImGui.Text(_localizer.Localize("Rotate")); ImGui.NextColumn();
            if (Config.BDTH)
            {
                ImGui.Text(_localizer.Localize("BDTH Set")); ImGui.NextColumn();
            }
            ImGui.Separator();
            for (int i = 0; i < Config.HousingItemList.Count(); i++)
            {
                var housingItem = Config.HousingItemList[i];
                var displayName = housingItem.Name;
                if (i == Config.SelectedItemIndex)
                {
                    displayName = '\ue06f' + displayName;
                }
                ImGui.Text(displayName); ImGui.NextColumn();
                ImGui.Text($"{housingItem.X:N3}"); ImGui.NextColumn();
                ImGui.Text($"{housingItem.Y:N3}"); ImGui.NextColumn();
                ImGui.Text($"{housingItem.Z:N3}"); ImGui.NextColumn();
                ImGui.Text($"{housingItem.Rotate:N3}"); ImGui.NextColumn();
                if (Config.BDTH)
                {
                    if (ImGui.Button(_localizer.Localize("Set") + "##" + i.ToString()))
                    {
                        Config.SelectedItemIndex = i;

                        Config.PlaceX = housingItem.X;
                        Config.PlaceY = housingItem.Y;
                        Config.PlaceZ = housingItem.Z;
                        Config.PlaceRotate = housingItem.Rotate;
                        Plugin.CommandManager.ProcessCommand($"/bdth {housingItem.X} {housingItem.Y} {housingItem.Z} {housingItem.Rotate}");
                        // Config.ForceMove = true;
                        Config.Save();
                    }
                    ImGui.NextColumn();
                }
                ImGui.Separator();
            }
            
        }

    }
}