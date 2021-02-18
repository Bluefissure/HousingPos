using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Dalamud.Game.Chat;
using ImGuiNET;
using HousingPos.Objects;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Numerics;
using System.Reflection;
using Lumina.Excel.GeneratedSheets;

namespace HousingPos.Gui
{
    public class ConfigurationWindow : Window<HousingPos>
    {

        public Configuration Config => Plugin.Config;
        private readonly string[] _languageList;
        private int _selectedLanguage;
        private readonly string[] _locationList;
        private int _selectedLocation;
        private readonly string[] _sizeList;
        private int _selectedSize;
        private Localizer _localizer;
        private string CustomTag = string.Empty;
        private Dictionary<uint, uint> iconToFurniture = new Dictionary<uint, uint> { };

        public ConfigurationWindow(HousingPos plugin) : base(plugin)
        {
            _languageList = new string[] { "en", "zh" };
            _locationList = new string[] { "薰衣草苗圃","高脚孤丘","海雾村","白银乡"};
            _sizeList = new string[] { "S","M","L"};
            _localizer = new Localizer(Config.UILanguage);
        }

        protected override void DrawUi()
        {
            ImGui.SetNextWindowSize(new Vector2(530, 450), ImGuiCond.FirstUseEver);
            if (!ImGui.Begin($"{Plugin.Name} v{Assembly.GetExecutingAssembly().GetName().Version}", ref WindowVisible, ImGuiWindowFlags.NoScrollWithMouse))
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
        protected override void DrawUploadUi()
        {
            uint buf_size = 255;
            ImGui.SetNextWindowSize(new Vector2(400, 300), ImGuiCond.FirstUseEver);
            if (!ImGui.Begin($"{Plugin.Name} v{Assembly.GetExecutingAssembly().GetName().Version}-Upload", ref WindowCanUpload, ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.End();
                return;
            }
            if (ImGui.BeginChild("##SettingUpload"))
            {
                string str = JsonConvert.SerializeObject(Config.UploadItems);
                ImGui.TextUnformatted(_localizer.Localize("Location"));
                ImGui.SameLine();
                ImGui.SetNextItemWidth(200);
                if (ImGui.Combo("##SetLocation", ref _selectedLocation, _locationList, _locationList.Length))
                {
                    Config.Location = _locationList[_selectedLocation];
                    Config.Save();
                }
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                if (ImGui.Combo("##SetSize", ref _selectedSize, _sizeList, _sizeList.Length))
                {
                    Config.Size = _sizeList[_selectedSize];
                    Config.Save();
                }
                ImGui.Checkbox("Anonymous", ref Config.Anonymous);

                ImGui.Text(_localizer.Localize("Nameit:"));
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.CalcTextSize(_localizer.Localize("Nameit:")).X - (16 * ImGui.GetIO().FontGlobalScale));
                ImGui.InputText("##Nameit", ref Config.Nameit, buf_size);
                if(Config.Tags.Count()==0)
                {
                    Config.Tags = new List<string>() { "组合", "全屋", "和风", "现代", "古风", "卡通" };
                    Config.TagsSelectList = new List<bool>() { false, false, false, false, false, false };
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

                ImGui.Text(_localizer.Localize("Can Select Tags:"));
                ImGui.SameLine();
                for (int i = 0; i < Config.Tags.Count(); i++)
                {
                    if (i % 6 != 0)
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
                ImGui.Text("Add Custom Tags:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(50);
                ImGui.InputText("##CustomTags",ref this.CustomTag, 20);
                if (!string.IsNullOrEmpty(this.CustomTag) && ImGui.Button(_localizer.Localize("Add")))
                {
                    ImGui.SameLine();
                    Config.Tags.Add(this.CustomTag);
                    Config.TagsSelectList.Add(false);
                    this.CustomTag = string.Empty;
                }

                if (!Config.Anonymous)
                {
                    ImGui.Text(_localizer.Localize("Uper:"));
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.CalcTextSize(_localizer.Localize("Uper:")).X - (16 * ImGui.GetIO().FontGlobalScale));
                    ImGui.InputText("##Uper", ref Config.Uper, buf_size);
                }
                else
                    Config.Uper = "Anonymous";

                if (ImGui.Button(_localizer.Localize("Send Data")))
                {
                    Config.Save();
                    try
                    {
                        //Plugin.Log(str);
                        List<string> tempTags = new List<string>();
                        for(int i=0;i< Config.Tags.Count();i++){
                            if(Config.TagsSelectList[i])
                                tempTags.Add( Config.Tags[i]);
                        }
                        string tags = JsonConvert.SerializeObject(tempTags);
                        Task<string> posttask = HttpPost.Post(Config.Location + '-' + Config.Size, Config.Nameit, str, tags, Config.Uper);
                        string res = posttask.Result;
                        Plugin.Log(res);
                        Plugin.Log(String.Format(_localizer.Localize("Exported {0} items to Cloud."), Config.UploadItems.Count));
                    }
                    catch (Exception e)
                    {
                        Plugin.LogError($"Error while Postdata: {e.Message}");
                    }
                    CanUpload = false;
                }
                ImGui.SameLine();
                if (ImGui.Button(_localizer.Localize("Cancel")))
                {
                    Config.Save();
                    CanUpload = false;
                }
                Config.Save();
                ImGui.EndChild();
            }
            
        }

        protected override void DrawScreen()
        {
            if (Config.DrawScreen)
            {
                DrawItemOnScreen();
            }
        }

        private void DrawGeneralSettings()
        {
            if (ImGui.Checkbox(_localizer.Localize("Recording"), ref Config.Recording)) Config.Save();
            if (Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(_localizer.Localize("Automatically record housing item list."));

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
                        itemList.Add($"item#{housingItem.ItemKey}\t{housingItem.Name}");
                    var itemSet = new HashSet<string>(itemList);
                    foreach (string itemName in itemSet)
                    {
                        str += $"{itemName}\t{itemList.Count(x => x == itemName)}\n";
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
                string str = ImGui.GetClipboardText();
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
                    Plugin.Log(String.Format(_localizer.Localize("Imported {0} items from your clipboard."), Config.HousingItemList.Count));
                }
                catch (Exception e)
                {
                    Plugin.LogError($"Error while importing items: {e.Message}");
                    LoadChocoboSave(str);
                }
            }
            ImGui.SameLine();
            if (ImGui.Button(_localizer.Localize("Export To Cloud")))
            {
                Config.UploadItems = Config.HousingItemList;
                CanUpload = true;
                Config.Nameit = "";
                Config.Save();
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
        private void DrawItemOnScreen()
        {
            for (int i = 0; i < Config.HousingItemList.Count(); i++)
            {
                SharpDX.Vector3 playerPos = Plugin.Interface.ClientState.LocalPlayer.Position;
                var housingItem = Config.HousingItemList[i];
                var itemPos = new SharpDX.Vector3(housingItem.X, housingItem.Y, housingItem.Z);
                if (Config.HiddenScreenItemHistory.IndexOf(i) >= 0) continue;
                if (Config.DrawDistance > 0 && (playerPos - itemPos).Length() > Config.DrawDistance)
                    continue;
                var displayName = housingItem.Name;
                if (Plugin.Interface.Framework.Gui.WorldToScreen(itemPos, out var screenCoords))
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

        private bool LoadChocoboSave(string str)
        {
            try
            {
                var chocoboInput = JsonConvert.DeserializeObject<ChocoboInput>(str);
                int failed = 0;
                int successed = 0;
                if (iconToFurniture.Count == 0)
                {
                    var housingFurnitures = Plugin.Interface.Data.GetExcelSheet<HousingFurniture>();
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
                    var iconIdOrFurnitureKey = chocoboItem.categoryId;
                    var furniture = Plugin.Interface.Data.GetExcelSheet<HousingFurniture>().GetRow(iconIdOrFurnitureKey + 196608);
                    if (furniture == null)
                    {
                        oldSave = true;
                        var furnitureId = iconToFurniture.ContainsKey(iconIdOrFurnitureKey) ? (int)iconToFurniture[iconIdOrFurnitureKey] : -1;
                        if(furnitureId == -1)
                        {
                            failed += chocoboItem.count;
                            continue;
                        }
                        furniture = Plugin.Interface.Data.GetExcelSheet<HousingFurniture>().GetRow((uint)furnitureId);
                    }
                    var item = furniture.Item.Value;
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
                            furniture.ModelKey, item.RowId, x, y, z, rotation, item.Name));
                        successed++;
                    }
                    Config.ResetRecord();
                }
                Plugin.Log(String.Format(_localizer.Localize("Imported {0} chocobo items from your clipboard, {1} failed."), successed, failed));
                return true;
            }
            catch (Exception e)
            {
                Plugin.LogError($"Error while importing chocobo save: {e.Message}");
            }
            return false;
        }

        private void BDTHSet(int i, HousingItem housingItem)
        {
            Config.SelectedItemIndex = i;
            Config.PlaceX = housingItem.X;
            Config.PlaceY = housingItem.Y;
            Config.PlaceZ = housingItem.Z;
            Config.PlaceRotate = housingItem.Rotate;
            Plugin.CommandManager.ProcessCommand($"/bdth {housingItem.X} {housingItem.Y} {housingItem.Z} {housingItem.Rotate}");
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
            string uniqueID = childIndex == -1 ? i.ToString() : i.ToString() + "_" + childIndex.ToString();
            if (Config.BDTH)
            {
                if (ImGui.Button(_localizer.Localize("Set") + "##" + uniqueID))
                {
                    BDTHSet(i, housingItem);
                }
                ImGui.NextColumn();
            }
            if (Config.Grouping )
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
                ImGui.SameLine();
                if (ImGui.Button(_localizer.Localize("Upload") + "##Single_" + uniqueID))
                {
                    List<HousingItem> tempList = new List<HousingItem>();
                    tempList.Add(housingItem);
                    Config.UploadItems = tempList;
                    CanUpload = true;
                    Config.Nameit = "";
                    Config.Save();
                }
                ImGui.NextColumn();
            }
        }
        private void DrawItemList()
        {
            // name, x, t, z, r, set
            int columns = 5;
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
                if(housingItem.children.Count == 0)
                {
                    if(Config.Grouping && Config.GroupingList.IndexOf(i) != -1)
                    {
                        if(Config.GroupingList.IndexOf(i) == 0)
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
                    bool open1 = ImGui.TreeNode(displayName);
                    ImGui.NextColumn();
                    DrawRow(i, housingItem);
                    if (open1)
                    {
                        for(int j=0; j < housingItem.children.Count; j++)
                        {
                            var childItem = housingItem.children[j];
                            displayName = childItem.Name;
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

    }
}