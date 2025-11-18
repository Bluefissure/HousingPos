using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Dalamud.Game.Text;
using Dalamud.Bindings.ImGui;
using HousingPos.Objects;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Reflection;
using Lumina.Excel.Sheets;
using Dalamud.Interface;
using System.Diagnostics;
using System.Globalization;
using Dalamud.Utility;
using Dalamud.Logging;
using Dalamud.Interface.Textures;

namespace HousingPos.Gui
{
    public class ConfigurationWindow : Window<HousingPos>
    {

        public Configuration Config => Plugin.Config;
        private readonly string[] _languageList;
        private int _selectedLanguage;
        private Localizer _localizer;
        private string CustomTag = string.Empty;
        private readonly Dictionary<uint, uint> iconToFurniture = new() { };

        public ConfigurationWindow(HousingPos plugin) : base(plugin)
        {
            _localizer = new Localizer(Config.UILanguage);
            _languageList = new string[] { "en", "zh" };
        }

        protected override void DrawUi()
        {
            ImGui.SetNextWindowSize(new Vector2(530, 450), ImGuiCond.FirstUseEver);
            if (!ImGui.Begin($"{Plugin.Name}", ref WindowVisible, ImGuiWindowFlags.NoScrollWithMouse))
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
            ImGui.End();
        }

        #region Helper Functions
        public void DrawIcon(ushort icon, Vector2 size)
        {
            if (icon < 65000)
            {
                var tex = HousingPos.Tex.GetFromGameIcon(new GameIconLookup(icon));
                if (tex == null || tex.GetWrapOrEmpty().Handle == IntPtr.Zero)
                {
                    ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1));
                    ImGui.BeginChild("FailedTexture", size);
                    ImGui.Text(icon.ToString());
                    ImGui.EndChild();
                    ImGui.PopStyleColor();
                }
                else
                    ImGui.Image(tex.GetWrapOrEmpty().Handle, size);
            }
        }
        #endregion


        #region Basic UI
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
                Config.Tags.Clear();
                Config.Save();
            }
            ImGui.SameLine(ImGui.GetColumnWidth() - 80);
            ImGui.TextUnformatted(_localizer.Localize("Tooltips"));
            ImGui.AlignTextToFramePadding();
            ImGui.SameLine();
            if (ImGui.Checkbox("##hideTooltipsOnOff", ref Config.ShowTooltips)) Config.Save();

            bool preview = Config.Previewing;
            if (ImGui.Checkbox(_localizer.Localize("Preview"), ref preview))
            {
                var currentTerritory = HousingPos.ClientState.TerritoryType;
                if (preview) Config.Previewing = preview;
                if (!preview)
                {
                    if (currentTerritory != Plugin.PreviewTerritory)
                        Config.Previewing = preview;
                    else
                        Plugin.Log(_localizer.Localize("Exit your house to disable preview."));
                }
                Config.Save();
            }
            if (Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(_localizer.Localize("Preview the current decoration plan when entering house."));

            if (ImGui.Checkbox(_localizer.Localize("BDTH"), ref Config.BDTH)) Config.Save();
            if (Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(_localizer.Localize("BDTH integrate: leave the position set to BDTH."));
            ImGui.SameLine();
            if (ImGui.Checkbox(_localizer.Localize("Single Export"), ref Config.SingleExport)) Config.Save();
            if (Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(_localizer.Localize("Add Export button to the single furnitures."));
            if (ImGui.Checkbox(_localizer.Localize("Draw on screen"), ref Config.DrawScreen)) Config.Save();
            if (Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(_localizer.Localize("Draw items on screen."));
            
            if (Config.DrawScreen)
            {
                ImGui.SameLine();
                if (ImGui.Button(_localizer.Localize("Undo") + "##Undo"))
                {
                    if (Config.HiddenScreenItemHistory != null && Config.HiddenScreenItemHistory.Count > 0)
                    {
                        var lastIndex = Config.HiddenScreenItemHistory.Last();
                        if (lastIndex < Config.HousingItemList.Count && lastIndex >= 0)
                        {
                            Config.HiddenScreenItemHistory.RemoveAt(Config.HiddenScreenItemHistory.Count - 1);
                            Config.Save();
                        }
                    }
                }
                if (Config.ShowTooltips && ImGui.IsItemHovered())
                    ImGui.SetTooltip(_localizer.Localize("Undo the on-screen setting."));
                ImGui.TextUnformatted(_localizer.Localize("Drawing Distance:"));
                if (Config.ShowTooltips && ImGui.IsItemHovered())
                    ImGui.SetTooltip(_localizer.Localize("Only draw items within this distance to your character. (0 for unlimited)"));
                if (ImGui.DragFloat("##DrawDistance", ref Config.DrawDistance, 0.1f, 0, 52)) { 
                    Config.DrawDistance = Math.Max(0, Config.DrawDistance);
                    Config.Save();
                }
            }
            ImGui.Text("X:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputFloat("##placeX", ref Config.PlaceX, 0.01f, 0.1f))
            {
                if (Config.SelectedItemIndex >= 0 && Config.SelectedItemIndex < Config.HousingItemList.Count) 
                { 
                    Config.HousingItemList[Config.SelectedItemIndex].X = Config.PlaceX;
                    if (Config.HousingItemList[Config.SelectedItemIndex].children.Count > 0)
                            Config.HousingItemList[Config.SelectedItemIndex].ReCalcChildrenPos();
                }
                Config.Save();
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.Text("Y:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputFloat("##placeY", ref Config.PlaceY, 0.01f, 0.1f))
            {
                if (Config.SelectedItemIndex >= 0 && Config.SelectedItemIndex < Config.HousingItemList.Count)
                {
                    Config.HousingItemList[Config.SelectedItemIndex].Y = Config.PlaceY;
                    if (Config.HousingItemList[Config.SelectedItemIndex].children.Count > 0)
                        Config.HousingItemList[Config.SelectedItemIndex].ReCalcChildrenPos();
                }
                Config.Save();
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.Text("Z:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputFloat("##placeZ", ref Config.PlaceZ, 0.01f, 0.1f))
            {
                if (Config.SelectedItemIndex >= 0 && Config.SelectedItemIndex < Config.HousingItemList.Count)
                {
                    Config.HousingItemList[Config.SelectedItemIndex].Z = Config.PlaceZ;
                    if (Config.HousingItemList[Config.SelectedItemIndex].children.Count > 0)
                        Config.HousingItemList[Config.SelectedItemIndex].ReCalcChildrenPos();
                }
                    Config.HousingItemList[Config.SelectedItemIndex].Z = Config.PlaceZ;
                Config.Save();
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.Text(_localizer.Localize("Rotate:"));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            float rotateDegree = Config.PlaceRotate / (float)Math.PI * 180;
            if (ImGui.InputFloat("##placeRotate", ref rotateDegree, 1f, 5f))
            {
                rotateDegree = (rotateDegree + 180 + 360) % 360 - 180;
                Config.PlaceRotate = rotateDegree / 180 * (float)Math.PI;
                if (Config.SelectedItemIndex >= 0 && Config.SelectedItemIndex < Config.HousingItemList.Count)
                {
                    Config.HousingItemList[Config.SelectedItemIndex].Rotate = Config.PlaceRotate;
                    if (Config.HousingItemList[Config.SelectedItemIndex].children.Count > 0)
                        Config.HousingItemList[Config.SelectedItemIndex].ReCalcChildrenPos();
                }
                Config.Save();
            }
            if (ImGui.Button(_localizer.Localize("Cloud Upload")))
            {
                Config.UploadItems = Config.HousingItemList;
                CanUpload = true;
                //Config.Tags.Clear();
                Config.UploadName = "";
                Config.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button(_localizer.Localize("Cloud Download")))
            {
                Task<string> strTask = HttpPost.GetMap(Config.DefaultCloudUri);
                strTask.ContinueWith((t) =>
                {
                    try
                    {
                        Config.CloudMap = JsonConvert.DeserializeObject<List<CloudMap>>(t.Result);
                        Config.Save();
                        CanImport = true;
                    }
                    catch (Exception e)
                    {
                        Plugin.LogError($"Error Importing from cloud: {e.Message}", e.ToString());
                    }
                });
            }
            /*
            ImGui.SameLine();
            if (ImGui.Button(_localizer.Localize("LeanCloud Import")))
            {
                Task<string> strTask = HttpPost.GetMapWithLeanCloud(Config.API_BASE_URL + Config.CLASS_NAME);
                strTask.ContinueWith((t) =>
                {
                    try
                    {
                        Config.CloudMap = JsonConvert.DeserializeObject<List<CloudMap>>(JObject.Parse(t.Result)["results"].ToString());
                        Config.Save();
                        CanImport = true;
                    }
                    catch (Exception e)
                    {
                        Plugin.LogError($"Error Importing from cloud: {e.Message}");
                    }
                });
            }
            */

            if (ImGui.Button(_localizer.Localize("Clear")))
            {
                Config.HousingItemList.Clear();
                Config.Save();
            }
            ImGui.SameLine();
            if (!Config.Grouping && ImGui.Button(_localizer.Localize("Sort")))
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
                    var itemList = new List<string>();
                    foreach (var housingItem in Config.HousingItemList)
                    {
                        itemList.Add($"item#{housingItem.ItemKey}\t{housingItem.Name}");
                        if (housingItem.children.Count > 0)
                        {
                            foreach(var childItem in housingItem.children)
                            {
                                itemList.Add($"item#{childItem.ItemKey}\t{childItem.Name}");
                            }
                        }
                    }
                    var itemSet = new HashSet<string>(itemList);
                    foreach (string itemName in itemSet)
                    {
                        str += $"{itemName}\t{itemList.Count(x => x == itemName)}\n";
                    }
                    Win32Clipboard.CopyTextToClipboard(str);
                    Plugin.Log(String.Format(_localizer.Localize("Copied {0} items to your clipboard."), itemSet.Count));
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
                string str = ImGui.GetClipboardText();
                try
                {
                    Config.HousingItemList = JsonConvert.DeserializeObject<List<HousingItem>>(str);
                    Plugin.TranslateFurnitureList(ref Config.HousingItemList);
                    Config.ResetRecord();
                    Plugin.Log(String.Format(_localizer.Localize("Imported {0} items from your clipboard."), Config.HousingItemList.Count));
                }
                catch (Exception e)
                {
                    Plugin.LogError($"Error while importing items: {e.Message}");
                    LoadChocoboSave(str);
                }
            }
            ImGui.SameLine(ImGui.GetColumnWidth() - 80);
            if (ImGui.Button(_localizer.Localize(Config.Grouping ? "Grouping" : "Group"))) 
            {
                if (Config.Grouping)
                {
                    if (Config.GroupingList.Count > 1)
                    {
                        var baseItem = Config.HousingItemList[Config.GroupingList[0]];
                        var childrenList = Config.GroupingList.GetRange(1, Config.GroupingList.Count - 1);
                        childrenList.Sort();
                        for (int i = childrenList.Count - 1; i >= 0; i--)
                        {
                            var index = childrenList[i];
                            var housingItem = Config.HousingItemList[index];
                            housingItem.CalcRelativeTo(baseItem);
                            baseItem.children.Add(housingItem);
                            Config.HousingItemList.RemoveAt(index);
                        }
                    }
                    Config.GroupingList.Clear();
                    Config.Grouping = false;
                }
                else
                {
                    Config.GroupingList.Clear();
                    Config.Grouping = true;
                }
                Config.Save();
            }
        }
        private void BDTHSet(int i, HousingItem housingItem)
        {
            Config.SelectedItemIndex = i;
            Config.PlaceX = housingItem.X;
            Config.PlaceY = housingItem.Y;
            Config.PlaceZ = housingItem.Z;
            Config.PlaceRotate = housingItem.Rotate;
            string bdthCommand = "/bdth";
            bdthCommand += $" {housingItem.X.ToString(CultureInfo.InvariantCulture)}";
            bdthCommand += $" {housingItem.Y.ToString(CultureInfo.InvariantCulture)}";
            bdthCommand += $" {housingItem.Z.ToString(CultureInfo.InvariantCulture)}";
            bdthCommand += $" {housingItem.Rotate.ToString(CultureInfo.InvariantCulture)}";
            HousingPos.CommandManager.ProcessCommand(bdthCommand);
            if (housingItem.children.Count > 0)
                housingItem.ReCalcChildrenPos();
            Config.Save();
        }
        private void DrawRow(int i, HousingItem housingItem, int childIndex = -1)
        {
            ImGui.Text($"{housingItem.X:N3}"); ImGui.NextColumn();
            ImGui.Text($"{housingItem.Y:N3}"); ImGui.NextColumn();
            ImGui.Text($"{housingItem.Z:N3}"); ImGui.NextColumn();
            ImGui.Text($"{housingItem.Rotate:N3}"); ImGui.NextColumn();
            var colorName = HousingPos.Data.GetExcelSheet<Stain>().GetRow(housingItem.Stain).Name;
            ImGui.Text($"{colorName}"); ImGui.NextColumn();
            string uniqueID = childIndex == -1 ? i.ToString() : i.ToString() + "_" + childIndex.ToString();
            if (Config.BDTH)
            {
                if (ImGui.Button(_localizer.Localize("Set") + "##" + uniqueID))
                {
                    BDTHSet(i, housingItem);
                }
                ImGui.NextColumn();
            }
            if (Config.Grouping)
            {
                var index = Config.GroupingList.IndexOf(i);
                var buttonText = housingItem.children.Count == 0 ? (index == -1 ? "Add" : "Del") : "Disband";
                if (childIndex == -1 && ImGui.Button(_localizer.Localize(buttonText) + "##Group_" + uniqueID))
                {
                    if (buttonText == "Add")
                        Config.GroupingList.Add(i);
                    else if (buttonText == "Del")
                        Config.GroupingList.RemoveAt(index);
                    else if (buttonText == "Disband")
                    {
                        for (int j = 0; j < housingItem.children.Count; j++)
                        {
                            Config.HousingItemList.Add(housingItem.children[j]);
                        }
                        housingItem.children.Clear();
                        Config.Save();
                    }
                }
                ImGui.NextColumn();
            }

            if (Config.SingleExport)
            {
                if (ImGui.Button(_localizer.Localize("Export") + "##Single_" + uniqueID))
                {
                    List<HousingItem> tempList = new List<HousingItem>();
                    tempList.Add(housingItem);
                    string str = JsonConvert.SerializeObject(tempList);
                    Win32Clipboard.CopyTextToClipboard(str);
                    Plugin.Log(String.Format(_localizer.Localize("Exported {0} items to your clipboard."), tempList.Count));
                }
                if(housingItem.children.Count > 0)
                {
                    ImGui.SameLine();
                    if (ImGui.Button(_localizer.Localize("Cloud Upload") + "##Single_" + uniqueID))
                    {
                        List<HousingItem> tempList = new List<HousingItem>();
                        tempList.Add(housingItem);
                        Config.UploadItems = tempList;
                        CanUpload = true;
                        Config.UploadName = "";
                        Config.Save();
                    }
                }
                ImGui.NextColumn();
            }
        }
        private void DrawItemList()
        {
            // name, x, t, z, r, color, set
            int columns = 6;
            if (Config.BDTH) columns += 1;
            if (Config.SingleExport) columns += 1;
            if (Config.Grouping) columns += 1;
            ImGui.Columns(columns, "ItemList", true);
            ImGui.Separator();
            ImGui.Text(_localizer.Localize("Name")); ImGui.NextColumn();
            ImGui.Text(_localizer.Localize("X")); ImGui.NextColumn();
            ImGui.Text(_localizer.Localize("Y")); ImGui.NextColumn();
            ImGui.Text(_localizer.Localize("Z")); ImGui.NextColumn();
            ImGui.Text(_localizer.Localize("Rotate")); ImGui.NextColumn();
            ImGui.Text(_localizer.Localize("Color")); ImGui.NextColumn();
            if (Config.BDTH)
            {
                ImGui.Text(_localizer.Localize("BDTH Set")); ImGui.NextColumn();
            }
            if (Config.Grouping)
            {
                ImGui.Text(_localizer.Localize("Grouping")); ImGui.NextColumn();
            }
            if (Config.SingleExport)
            {
                ImGui.Text(_localizer.Localize("Single Export")); ImGui.NextColumn();
            }
            ImGui.Separator();
            for (int i = 0; i < Config.HousingItemList.Count(); i++)
            {
                var housingItem = Config.HousingItemList[i];
                var displayName = housingItem.Name;
                if (i == Config.SelectedItemIndex)
                    displayName = '\ue06f' + displayName;
                if (housingItem.children.Count == 0)
                {
                    Nullable<Item> item = HousingPos.Data.GetExcelSheet<Item>().GetRow(housingItem.ItemKey);
                    if (item != null)
                    {
                        DrawIcon(item.Value.Icon, new Vector2(20, 20));
                        ImGui.SameLine();
                    }
                    if (Config.Grouping && Config.GroupingList.IndexOf(i) != -1)
                    {
                        if (Config.GroupingList.IndexOf(i) == 0)
                            ImGui.TextColored(new Vector4(1.0f, 0.0f, 1.0f, 1.0f), displayName);
                        else
                            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), displayName);
                    }
                    else
                    {
                        ImGui.Text(displayName);
                    }
                    ImGui.NextColumn();
                    DrawRow(i, housingItem);
                }
                else
                {
                    Nullable<Item> item = HousingPos.Data.GetExcelSheet<Item>().GetRow(housingItem.ItemKey);
                    if (item != null)
                    {
                        DrawIcon(item.Value.Icon, new Vector2(20, 20));
                        ImGui.SameLine();
                    }
                    bool open1 = ImGui.TreeNode(displayName);
                    ImGui.NextColumn();
                    DrawRow(i, housingItem);
                    if (open1)
                    {
                        for (int j = 0; j < housingItem.children.Count; j++)
                        {
                            var childItem = housingItem.children[j];
                            displayName = childItem.Name;
                            item = HousingPos.Data.GetExcelSheet<Item>().GetRow(childItem.ItemKey);
                            if (item != null)
                            {
                                DrawIcon(item.Value.Icon, new Vector2(20, 20));
                                ImGui.SameLine();
                            }
                            ImGui.Text(displayName);
                            ImGui.NextColumn();
                            DrawRow(i, childItem, j);
                        }
                        ImGui.TreePop();
                    }
                }

                ImGui.Separator();
            }

        }

        #endregion


        #region Draw Screen
        protected override void DrawScreen()
        {
            if (Config.DrawScreen)
            {
                DrawItemOnScreen();
            }
        }

        private void DrawItemOnScreen()
        {
            for (int i = 0; i < Config.HousingItemList.Count(); i++)
            {
                var playerPos = HousingPos.ClientState.LocalPlayer.Position;
                var housingItem = Config.HousingItemList[i];
                var itemPos = new Vector3(housingItem.X, housingItem.Y, housingItem.Z);
                if (Config.HiddenScreenItemHistory.IndexOf(i) >= 0) continue;
                if (Config.DrawDistance > 0 && (playerPos - itemPos).Length() > Config.DrawDistance)
                    continue;
                var displayName = housingItem.Name;
                if (HousingPos.GameGui.WorldToScreen(itemPos, out var screenCoords))
                {
                    ImGui.PushID("HousingItemWindow" + i);
                    ImGui.SetNextWindowPos(new Vector2(screenCoords.X, screenCoords.Y));
                    ImGui.SetNextWindowBgAlpha(0.8f);
                    if (ImGui.Begin("HousingItem" + i,
                        ImGuiWindowFlags.NoDecoration |
                        ImGuiWindowFlags.AlwaysAutoResize |
                        ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoMove |
                        ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav))
                    {
                        if (Config.Grouping && Config.GroupingList.IndexOf(i) != -1)
                        {
                            if (Config.GroupingList.IndexOf(i) == 0)
                                ImGui.TextColored(new Vector4(1.0f, 0.0f, 1.0f, 1.0f), displayName);
                            else
                                ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), displayName);
                        }
                        else
                        {
                            ImGui.Text(displayName);
                        }
                        ImGui.SameLine();
                        if (Config.BDTH)
                        {
                            if (ImGui.Button(_localizer.Localize("Set") + "##ScreenItem" + i.ToString()))
                            {
                                BDTHSet(i, housingItem);
                                Config.HiddenScreenItemHistory.Add(i);
                                Config.Save();
                            }
                        }
                        ImGui.SameLine();
                        if (Config.Grouping)
                        {
                            var index = Config.GroupingList.IndexOf(i);
                            if (ImGui.Button(_localizer.Localize(index == -1 ? "Add" : "Del") + "##Group_" + i.ToString()))
                            {
                                if (index == -1)
                                    Config.GroupingList.Add(i);
                                else
                                    Config.GroupingList.RemoveAt(index);
                            }
                            ImGui.NextColumn();
                        }
                        ImGui.End();
                    }

                    ImGui.PopID();
                }
            }
        }
        #endregion


        #region Chocobo Shit
        private bool LoadChocoboSave(string str)
        {
            try
            {
                Config.HousingItemList.Clear();
                var chocoboInput = JsonConvert.DeserializeObject<ChocoboInput>(str);
                int failed = 0;
                int successed = 0;
                if (iconToFurniture.Count == 0)
                {
                    var housingFurnitures = HousingPos.Data.GetExcelSheet<HousingFurniture>();
                    foreach (var furniture in housingFurnitures)
                    {
                        var item = furniture.Item.Value;
                        ushort iconId = item.Icon;
                        if (!iconToFurniture.ContainsKey(iconId))
                            iconToFurniture.Add(item.Icon, furniture.RowId);
                    }
                }
                bool oldSave = false;
                foreach (var chocoboItem in chocoboInput.list)
                {
                    var iconIdOrCategoryId = chocoboItem.categoryId;
                    Nullable<HousingFurniture> furniture = HousingPos.Data.GetExcelSheet<HousingFurniture>().GetRow(iconIdOrCategoryId + 0x30000);
                    if (furniture == null)
                    {
                        oldSave = true;
                        var furnitureId = iconToFurniture.ContainsKey(iconIdOrCategoryId) ? (int)iconToFurniture[iconIdOrCategoryId] : -1;
                        if(furnitureId == -1)
                        {
                            Plugin.Log($"Cannot find iconIdOrCategoryId:{iconIdOrCategoryId}");
                            failed += chocoboItem.count;
                            continue;
                        }
                        furniture = HousingPos.Data.GetExcelSheet<HousingFurniture>().GetRow((uint)furnitureId);
                    }
                    var item = furniture.Value.Item.Value;
                    int len = chocoboItem.count;
                    for (int i = 0; i < len; i++)
                    {
                        var x = chocoboItem.posX[i];
                        var y = chocoboItem.posY[i];
                        var z = chocoboItem.posZ[i];
                        var rotation = oldSave ? (float)(Math.Asin(chocoboItem.Rotation[i]) * 2) : chocoboItem.Rotation[i];
                        if (float.IsNaN(rotation))
                            rotation = 0;
                        Config.HousingItemList.Add(new HousingItem(
                            furniture.Value.RowId, furniture.Value.ModelKey, item.RowId, 0, x, y, z, rotation, item.Name.ToString()));
                        successed++;
                    }
                    Config.ResetRecord();
                }
                Plugin.Log(String.Format(_localizer.Localize("Imported {0} chocobo items from your clipboard, {1} failed."), successed, failed));
                return true;
            }
            catch (Exception e)
            {
                Plugin.LogError($"Error while importing chocobo save: {e.Message}", e.ToString());
            }
            return false;
        }

        #endregion
        
        
        #region Cloud Upload & Import

        private void DrawImportRow(int i, CloudMap cloudMap)
        {
            string uniqueId = i.ToString();
            string territoryName = "";
            try
            {
                territoryName = HousingPos.Data.GetExcelSheet<TerritoryType>().GetRow((uint)cloudMap.LocationId).PlaceName.Value.Name.ToString();
            } catch (ArgumentOutOfRangeException)
            {
                territoryName = _localizer.Localize("Unknown");
            }
            ImGui.Text($"{cloudMap.Name}"); ImGui.NextColumn();
            ImGui.Text(territoryName); ImGui.NextColumn();
            ImGui.Text($"{cloudMap.Tags}"); ImGui.NextColumn();
            if (ImGui.Button(_localizer.Localize("Import") + "##" + uniqueId))
            {
                Config.LocationId = cloudMap.LocationId;
                HousingPos.PluginLog.Info("CloudMap Hash: {0}", cloudMap.Hash);
                if (cloudMap.Hash != "")
                {
                    Task<string> cloudItems = HttpPost.GetItems(Config.DefaultCloudUri, cloudMap.Hash);
                    cloudItems.ContinueWith((t) => {
                        string str = t.Result;
                        //Plugin.Log(str);
                        try
                        {
                            Config.HousingItemList = JsonConvert.DeserializeObject<List<HousingItem>>(str);
                            Plugin.TranslateFurnitureList(ref Config.HousingItemList);
                            Config.ResetRecord();
                            Plugin.Log(String.Format(_localizer.Localize("Imported {0} items from Cloud."), Config.HousingItemList.Count));
                        }
                        catch (Exception e)
                        {
                            Plugin.LogError($"Error while importing items: {e.Message}", e.ToString());
                            LoadChocoboSave(str);
                        }
                    });
                }
                /*
                if (cloudMap.ObjectId != "")
                {
                    Task<string> cloudItems = HttpPost.GetItemsWithLeanCloud(Config.API_BASE_URL + Config.CLASS_NAME, cloudMap.ObjectId);
                    cloudItems.ContinueWith((t) => {
                        string str = JObject.Parse(t.Result)["Items"].ToString();
                        //Plugin.Log(str);
                        try
                        {
                            Config.HousingItemList = JsonConvert.DeserializeObject<List<HousingItem>>(str);
                            foreach (var item in Config.HousingItemList)
                            {
                                try
                                {
                                    item.Name = Plugin.Interface.Data.GetExcelSheet<Item>().GetRow(item.ItemKey).Name;
                                }
                                catch (Exception e)
                                {
                                    Plugin.LogError($"Error while translating item#{item.ItemKey}: {e.Message}");
                                }
                            }
                            Config.ResetRecord();
                            Plugin.Log(String.Format(_localizer.Localize("Imported {0} items from LeanCloud."), Config.HousingItemList.Count));
                        }
                        catch (Exception e)
                        {
                            Plugin.LogError($"Error while importing items: {e.Message}");
                            LoadChocoboSave(str);
                        }
                    });
                }
                */

            }
            ImGui.NextColumn();
        }
        private void DrawImportList()
        {
            int columns = 4;
            ImGui.Columns(columns, "CloudItemsList", true);
            ImGui.Separator();
            ImGui.Text(_localizer.Localize("Name")); ImGui.NextColumn();
            ImGui.Text(_localizer.Localize("Location")); ImGui.NextColumn();
            ImGui.Text(_localizer.Localize("Tags")); ImGui.NextColumn();
            ImGui.Text(_localizer.Localize("Import")); ImGui.NextColumn();
            ImGui.Separator();
            for (int i = 0; i < Config.CloudMap.Count(); i++)
            {
                if (Config.CloudMap[i] != null)
                {
                    DrawImportRow(i, Config.CloudMap[i]);
                    ImGui.Separator();
                }
            }
            ImGui.Columns(1);
        }
        protected override void DrawUploadUi()
        {
            uint buf_size = 255;
            ImGui.SetNextWindowSize(new Vector2(400, 300), ImGuiCond.FirstUseEver);
            if (ImGui.Begin($"{Plugin.Name} - Upload", ref WindowCanUpload, ImGuiWindowFlags.NoScrollWithMouse))
            {

                if (ImGui.BeginChild("##SettingUpload"))
                {
                    ImGui.TextUnformatted(_localizer.Localize("Server Address:"));
                    if (Config.ShowTooltips && ImGui.IsItemHovered())
                        ImGui.SetTooltip(_localizer.Localize("Open server which stores all uploaded data."));
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.CalcTextSize(_localizer.Localize("Server Address:")).X - (16 * ImGui.GetIO().FontGlobalScale));
                    if (ImGui.InputText("##ServerAddr", ref Config.DefaultCloudUri, 255))
                    {
                        Config.Save();
                    }
                    ImGui.TextUnformatted(_localizer.Localize("Md5 Salt:"));
                    if (Config.ShowTooltips && ImGui.IsItemHovered())
                        ImGui.SetTooltip(_localizer.Localize("Salt used to encrypt your user id."));
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.CalcTextSize(_localizer.Localize("Md5 Salt:")).X - (16 * ImGui.GetIO().FontGlobalScale));
                    if (ImGui.InputText("##Md5Salt", ref Config.Md5Salt, 255))
                    {
                        Config.Save();
                    }
                    /*
                    ImGui.TextUnformatted(_localizer.Localize("Serverless Api:"));
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.CalcTextSize(_localizer.Localize("Serverless Api:")).X - (16 * ImGui.GetIO().FontGlobalScale));
                    if (ImGui.InputText("##ServerlessApi", ref Config.API_BASE_URL, 255))
                    {
                        Config.Save();
                    }
                    */
                    ImGui.Checkbox(_localizer.Localize("Anonymous"), ref Config.Anonymous);

                    ImGui.Text(_localizer.Localize("Upload Name:"));
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.CalcTextSize(_localizer.Localize("Upload Name:")).X - (16 * ImGui.GetIO().FontGlobalScale));
                    ImGui.InputText("##UploadName", ref Config.UploadName, (int)buf_size);
                    if (Config.Tags.Count() == 0)
                    {
                        Config.Tags = new List<string>() { _localizer.Localize("Combination"),
                        _localizer.Localize("WholeHouse"),
                        _localizer.Localize("Far Eastern-style"),
                        _localizer.Localize("Modern-style"),
                        _localizer.Localize("Plants"),
                        _localizer.Localize("Loft-style"),
                        _localizer.Localize("Cartoon-style")};
                        Config.TagsSelectList = new List<bool>() { false, false, false, false, false, false, false };
                        Config.Save();
                    }

                    ImGui.Text(_localizer.Localize("Selected Tags:"));
                    for (int i = 0; i < Config.Tags.Count(); i++)
                    {
                        var showText = Config.Tags[i];
                        Vector4 Selected = new Vector4(255, 71, 71, 255);
                        if (Config.TagsSelectList[i])
                        {
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(ImGui.CalcTextSize(Config.Tags[i]).X);
                            ImGui.TextColored(Selected, showText);
                        }
                    }

                    ImGui.Text(_localizer.Localize("Available Tags:"));
                    ImGui.SameLine();
                    for (int i = 0; i < Config.Tags.Count(); i++)
                    {
                        if (i % 8 != 0)
                            ImGui.SameLine();
                        ImGui.SetNextItemWidth(ImGui.CalcTextSize(Config.Tags[i]).X);
                        var buttonText = Config.Tags[i];
                        //Vector4 Unselect = new Vector4(0, 255, 0, 255);
                        //Vector4 Selected = new Vector4(255, 0, 0, 255);
                        if (ImGui.RadioButton(_localizer.Localize(buttonText), Config.TagsSelectList[i]))
                        {
                            Config.TagsSelectList[i] = !Config.TagsSelectList[i];
                            Config.Save();
                        }
                    }
                    ImGui.Text(_localizer.Localize("Add Custom Tags:"));
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(50);
                    ImGui.InputText("##CustomTags", ref this.CustomTag, 20);
                    if (!string.IsNullOrEmpty(this.CustomTag))
                    {
                        ImGui.SameLine();
                        if (ImGui.Button(_localizer.Localize("Add")))
                        {
                            Config.Tags.Add(this.CustomTag);
                            Config.TagsSelectList.Add(true);
                            this.CustomTag = string.Empty;
                            Config.Save();
                        }
                    }

                    if (!Config.Anonymous)
                    {
                        ImGui.Text(_localizer.Localize("Uploader:"));
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.CalcTextSize(_localizer.Localize("Uploader:")).X - (16 * ImGui.GetIO().FontGlobalScale));
                        ImGui.InputText("##Uploader", ref Config.Uploader, (int)buf_size);
                    }
                    else
                        Config.Uploader = "Anonymous";
                    ImGui.Text(String.Format(_localizer.Localize("Here are {0} items that will be sent."), Config.UploadItems.Count));
                    if (ImGui.Button(_localizer.Localize("Send Data")))
                    {
                        Config.Save();
                        if (Config.UploadName.Trim() == "")
                        {
                            Plugin.LogError($"Error while Postdata: Empty name cannot be uploaded.");
                            CanUpload = false;
                        }
                        else if (Config.UploadItems.Count() == 0)
                        {
                            Plugin.LogError($"Error while Postdata: No items will be uploaded.");
                            CanUpload = false;
                        }
                        else
                        {
                            string str = JsonConvert.SerializeObject(Config.UploadItems);
                            //Plugin.Log(str);
                            string tags = "";
                            for (int i = 0; i < Config.Tags.Count(); i++)
                            {
                                if (Config.TagsSelectList[i])
                                    tags += Config.Tags[i] + ",";
                            }
                            if (tags.Length > 0)
                                tags = tags.Remove(tags.Length - 1);
                            //Plugin.Log(tags);
                            var cid = HousingPos.ClientState.LocalContentId.ToString();

                            Task<string> posttask = HttpPost.Post(Config.DefaultCloudUri, Config.LocationId, Config.UploadName, str, tags, Config.Uploader, cid, Config.Md5Salt);
                            CanUpload = false;
                            Plugin.Log(String.Format(_localizer.Localize("Uploading {0} items to Cloud."), Config.UploadItems.Count));
                            posttask.ContinueWith((t) => {
                                try
                                {
                                    string res = posttask.Result;
                                    Plugin.Log(res);
                                }
                                catch (AggregateException e)
                                {
                                    foreach (var errInner in e.InnerExceptions)
                                    {
                                        Plugin.LogError($"Error while Postdata: {errInner.Message}", e.ToString()); //this will call ToString() on the inner execption and get you message, stacktrace and you could perhaps drill down further into the inner exception of it if necessary 
                                    }
                                    CanUpload = true;
                                }
                            });
                        }

                    }
                    /*
                    ImGui.SameLine();
                    if (ImGui.Button(_localizer.Localize("Send Data To Leancloud")))
                    {
                        Config.Save();
                        if (Config.UploadItems.Count() == 0)
                        {
                            Plugin.LogError($"Error while Postdata: No Items Can Be Upload");
                            CanUpload = false;
                        }
                        else
                        {
                            string str = JsonConvert.SerializeObject(Config.UploadItems);
                            //Plugin.Log(str);
                            string tags = "";
                            for (int i = 0; i < Config.Tags.Count(); i++)
                            {
                                if (Config.TagsSelectList[i])
                                    tags += Config.Tags[i] + ",";
                            }
                            if (tags.Length > 0)
                                tags = tags.Remove(tags.Length - 1);
                            //Plugin.Log(tags);
                            var cid = Plugin.Interface.ClientState.LocalContentId.ToString();
                            CanUpload = false;
                            Task<string> sessionToken = HttpPost.Login(Config.API_BASE_URL,cid);
                            sessionToken.ContinueWith((t) => {
                                JObject res = JObject.Parse(t.Result);
                                Config.SessionToken = res["sessionToken"].ToString();
                                Config.Save();
                                Task<string> posttask = HttpPost.PostWithLeanCloud(Config.API_BASE_URL, Config.CLASS_NAME, Config.Location, Config.UploadName, str, tags, Config.Uploader, Config.SessionToken);
                                    posttask.ContinueWith((tt) => {
                                    try
                                    {
                                        string res2 = tt.Result;
                                        Plugin.Log(String.Format(_localizer.Localize("Exported {0} items to Cloud."), Config.UploadItems.Count));

                                    }
                                    catch (Exception e)
                                    {
                                        Plugin.LogError($"Error while Postdata: {e.Message}");
                                        CanUpload = true;
                                    }
                                });
                            });
                        }
                    }
                    */
                    ImGui.SameLine();
                    if (ImGui.Button(_localizer.Localize("Cancel")))
                    {
                        Config.Save();
                        CanUpload = false;
                    }
                    Config.Save();
                    ImGui.EndChild();
                }

                ImGui.End();
            }
        }
        protected override void DrawImportUi()
        {
            ImGui.SetNextWindowSize(new Vector2(520, 430), ImGuiCond.FirstUseEver);
            if (ImGui.Begin($"{Plugin.Name} - Import", ref WindowCanImport, ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (CanImport && ImGui.BeginChild("##ImportCloud"))
                {
                    DrawImportList();
                    if (ImGui.Button(_localizer.Localize("Cancel")))
                    {
                        CanImport = false;
                    }
                    ImGui.EndChild();
                }
                ImGui.End();
            }
            

        }

        #endregion

    }
}