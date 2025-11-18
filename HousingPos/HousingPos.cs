using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin;
using Dalamud.Game.Command;
using Dalamud.Game.Network;
using Dalamud.Game.Gui;
using Lumina.Excel.Sheets;
using HousingPos.Objects;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Hooking;
using HousingPos.Gui;
using Dalamud.IoC;
using Dalamud.Game.ClientState;
using Dalamud.Data;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Internal;
using System.Net;
using Dalamud.Game.ClientState.Conditions;

namespace HousingPos
{

    public class HousingPos : IDalamudPlugin
    {
        public string Name => "HousingPos";
        public PluginUi Gui { get; private set; }
        public Configuration Config { get; private set; }
        public OpcodeDefinition Opcode { get; private set; }


        [PluginService]
        public static ICommandManager CommandManager { get; private set; }
        [PluginService]
        public static IFramework Framework { get; private set; }
        [PluginService]
        public static ISigScanner SigScanner { get; private set; }
        [PluginService]
        public static IDalamudPluginInterface Interface { get; private set; }
        [PluginService]
        public static IGameGui GameGui { get; private set; }
        [PluginService]
        public static IChatGui ChatGui { get; private set; }
        [PluginService]
        public static IClientState ClientState { get; private set; }
        [PluginService]
        public static IDataManager Data { get; private set; }
        [PluginService]
        public static ISigScanner Scanner { get; private set; }
        [PluginService]
        public static IGameInteropProvider Hook { get; private set; }
        [PluginService]
        public static IPluginLog PluginLog { get; private set; }
        [PluginService]
        public static ITextureProvider Tex { get; private set; }
        [PluginService]
        public static ICondition Condition { get; private set; }

        private Localizer _localizer;

        // Texture dictionary for the housing item icons.
        // public readonly Dictionary<ushort, TextureWrap> TextureDictionary = new Dictionary<ushort, TextureWrap>();

        private bool threadRunning = false;

        public List<HousingItem> HousingItemList = new List<HousingItem>();
        public List<int> PreviewPages = new List<int>();
        public int PreviewTerritory = 0;

        public IntPtr LoadHousingFunc;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate Int64 LoadHousingFuncDelegate(Int64 a1, Int64 a2);
        private Hook<LoadHousingFuncDelegate> LoadHousingFuncHook;
        public IntPtr LoadHouFurFunc;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void LoadHouFurFuncDelegate(Int64 a1, IntPtr a2);
        private Hook<LoadHouFurFuncDelegate> LoadHouFurFuncHook;
        public IntPtr UIFunc;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void UIFuncDelegate(Int64 a1, UInt32 a2, char a3);
        private Hook<UIFuncDelegate> UIFuncHook;
        public void Dispose()
        {
            //Network.NetworkMessage -= OnNetwork;
            Condition.ConditionChange -= OnConditionChange;
            LoadHousingFuncHook.Disable();
            LoadHousingFuncHook.Dispose();
            //UIFuncHook.Disable();
            Config.PlaceAnywhere = false;
            ClientState.TerritoryChanged -= TerritoryChanged;
            CommandManager.RemoveHandler("/xhouse");
            Gui?.Dispose();
        }

        public HousingPos()
        {
            Config = Interface.GetPluginConfig() as Configuration ?? new Configuration();
            // Config.Initialize(Interface);
            RefreshFurnitureList(ref Config.HousingItemList);
            Config.Grouping = false;
            Config.Save();
            _localizer = new Localizer(Config.UILanguage);
            // LoadOffset();
            Initialize();

            CommandManager.AddHandler("/xhouse", new CommandInfo(CommandHandler)
            {
                HelpMessage = "/xhouse - load housing item list."
            });
            Gui = new PluginUi(this);
            ClientState.TerritoryChanged += TerritoryChanged;

        }
        public void Initialize()
        {
            //Network.NetworkMessage += OnNetwork;
            //UIFunc = Scanner.ScanText("E8 ?? ?? ?? ?? 89 77 04") + 5;
            //LoadHousingFunc = Scanner.ScanText("48 8B 41 08 48 85 C0 74 09 48 8D 48 10");
            LoadHousingFunc = Scanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 20 48 8B 71 08 48 8B FA");
            //LoadHouFurFunc = Interface.TargetModuleScanner.ScanText("48 8B FA 0F 97 C3") - 0xE;

            //UIFuncHook = Hook.HookFromAddress<UIFuncDelegate>(
            //    UIFunc,
            //    new UIFuncDelegate(UIFuncDetour)
            //);
            Condition.ConditionChange += OnConditionChange;
            LoadHousingFuncHook = Hook.HookFromAddress<LoadHousingFuncDelegate>(
                LoadHousingFunc,
                new LoadHousingFuncDelegate(LoadHousingFuncDetour)
            );
            //UIFuncHook.Enable();
            LoadHousingFuncHook.Enable();
            //LoadHouFurFuncHook.Enable();
        }


        public void RefreshFurnitureList(ref List<HousingItem> FurnitureList)
        {
            for(var i = 0; i < FurnitureList.Count; i++)
            {
                if(FurnitureList[i].ModelKey > 0 && FurnitureList[i].FurnitureKey == 0)
                {
                    FurnitureList[i].FurnitureKey = (uint)(FurnitureList[i].ModelKey + 0x30000);
                    Nullable<HousingFurniture> furniture = Data.GetExcelSheet<HousingFurniture>().GetRow(FurnitureList[i].FurnitureKey);
                    if (furniture == null) continue;
                    FurnitureList[i].ModelKey = furniture.Value.ModelKey;
                }
            }
        }

        public void TranslateFurnitureList(ref List<HousingItem> FurnitureList)
        {
            for (var i = 0; i < FurnitureList.Count; i++)
            {
                if (FurnitureList[i].FurnitureKey == 0)
                {
                    RefreshFurnitureList(ref FurnitureList);
                    break;
                }
            }
            for (var i = 0; i < FurnitureList.Count; i++)
            {
                Nullable<HousingFurniture> furniture = Data.GetExcelSheet<HousingFurniture>().GetRow(FurnitureList[i].FurnitureKey);
                FurnitureList[i].Name = furniture == null ? "" : furniture.Value.Item.Value.Name.ToString();
            }
            FurnitureList = FurnitureList.Where(e => e.Name != "").ToList();
        }


        private void OnConditionChange(ConditionFlag flag, bool value)
        {
            if (flag != ConditionFlag.UsingHousingFunctions)
            {
                return;
            }

            if (Config.Previewing)  // disable decorate UI
            {
                LogError(_localizer.Localize("Decorate in preview mode is dangerous!"));
                LogError(_localizer.Localize("Please exit the house and disable preview!"));
                return;
            }

            if (HousingItemList.Count > 0 && Config.HousingItemList.Count == 0)
            {
                Log(String.Format(_localizer.Localize("Load {0} furnitures."), HousingItemList.Count));
                RefreshFurnitureList(ref HousingItemList);
                RefreshFurnitureList(ref Config.HousingItemList);
                Config.HousingItemList = HousingItemList.ToList();
                Config.HiddenScreenItemHistory = new List<int>();
                var territoryTypeId = ClientState.TerritoryType;
                Config.LocationId = territoryTypeId;
                Config.Save();
            }
            else
            {
                Log(_localizer.Localize("Please clear the furniture list and re-enter house to load current furniture list."));
            }
        }

        private void UIFuncDetour(Int64 a1, UInt32 a2, char a3)
        {
            //Log($"TestFuncHook: {a1}, {a2}, {(int)a3}");
            if(a2 == 67 && a3 == 1 )
            {
                if (Config.Previewing)  // disable decorate UI
                {
                    LogError(_localizer.Localize("Decorate in preview mode is dangerous!"));
                    LogError(_localizer.Localize("Please exit the house and disable preview!"));
                    this.UIFuncHook.Original(a1, a2, (char)0);
                    return;
                }
                if(HousingItemList.Count > 0 && Config.HousingItemList.Count == 0)
                {
                    Log(String.Format(_localizer.Localize("Load {0} furnitures."), HousingItemList.Count));
                    RefreshFurnitureList(ref HousingItemList);
                    RefreshFurnitureList(ref Config.HousingItemList);
                    Config.HousingItemList = HousingItemList.ToList();
                    Config.HiddenScreenItemHistory = new List<int>();
                    var territoryTypeId = ClientState.TerritoryType;
                    Config.LocationId = territoryTypeId;
                    Config.Save();
                }
                else
                {
                    Log(_localizer.Localize("Please clear the furniture list and re-enter house to load current furniture list."));
                }
            }
            this.UIFuncHook.Original(a1, a2, a3);
        }

        /*
        public void LoadHouFurFuncDetour(Int64 a1, IntPtr a2)
        {
            PluginLog.Info($"a1:{a1:X} a2:{a2:X}");
            this.LoadHouFurFuncHook.Original(a1, a2);
        }
        */

        private Int64 LoadHousingFuncDetour(Int64 a1, Int64 a2)
        {
            IntPtr dataPtr = (IntPtr)a2;

            byte[] posArr = new byte[2416];
            Marshal.Copy(dataPtr, posArr, 0, 12);
            if (BitConverter.ToString(posArr).Replace("-", " ").StartsWith("FF FF FF FF FF FF FF FF"))
            {
                HousingItemList.Clear();
                Config.DrawScreen = false;
                Config.Save();
                return this.LoadHousingFuncHook.Original(a1, a2);
            }
            if (Config.Previewing)
            {
                RefreshFurnitureList(ref Config.HousingItemList);
                if (DateTime.Now > Config.lastPosPackageTime.AddSeconds(5))
                {
                    PreviewPages.Clear();
                    Config.lastPosPackageTime = DateTime.Now;
                    Config.Save();
                }
                int curPage = 0;
                int count = 0;
                while (PreviewPages.IndexOf(curPage) != -1)
                    curPage++;
                /*
                string s = string.Join(",", PreviewPages);
                // Log($"PreviewPages:{s}");
                // Log($"curPage:{curPage}");
                */
                PreviewPages.Add(curPage);
                List<string> compatibleTalks = new()
                {
                    "CmnDefHousingObject",
                    "CmnDefRetainerBell",
                    "ComDefCompanyChest",
                    "CmnDefBeautySalon",
                    "CmnDefCutSceneReplay",
                    "CmnDefMiniGame",
                    "CmnDefCabinet",
                    "HouFurVisitNote"
                };
                for (int i = 12; i < posArr.Length && i + 24 < posArr.Length; i += 24)
                {
                    var hashIndex = ((i - 12) / 24) + curPage * 100;
                    if(hashIndex < Config.HousingItemList.Count)
                    {
                        count++;
                        var item = Config.HousingItemList[hashIndex];
                        var furniture = Data.GetExcelSheet<HousingFurniture>().GetRow(item.FurnitureKey);
                        ushort furnitureNetId = (ushort)(item.FurnitureKey - 0x30000);
                        byte[] itemBytes = new byte[24];
                        itemBytes[2] = 1;
                        if (furniture.CustomTalk.RowId > 0)
                        {
                            string talk = furniture.CustomTalk.Value.Name.ToString().Split('_')[0];
                            if (compatibleTalks.Contains(talk))
                            {
                                itemBytes[2] = 1;
                            }
                            else
                            {
                                switch (talk)
                                {
                                    case "CmnDefHousingDish":
                                        itemBytes[2] = 0;
                                        break;
                                    case "HouFurOrchestrion":
                                        itemBytes[2] = 2;
                                        break;
                                    case "HouFurAquarium":
                                        furnitureNetId = 0x1EF;
                                        itemBytes[2] = 1;
                                        break;
                                    case "HouFurVase":
                                        ushort oldId = furnitureNetId;
                                        furnitureNetId = oldId switch
                                        {
                                            196828 - 0x30000 => 196751 - 0x30000,
                                            196829 - 0x30000 => 196752 - 0x30000,
                                            _ => 196753 - 0x30000,
                                        };
                                        itemBytes[2] = 1;
                                        break;
                                    case "HouFurPlantPot":
                                        furnitureNetId = 0x160;
                                        itemBytes[2] = 1;
                                        break;
                                    case "HouFurPicture":
                                    case "HouFurFishprint":
                                        furnitureNetId = (ushort)(furnitureNetId == 0x222 ? 0x2F0 : 0x1E);
                                        itemBytes[2] = 1;
                                        break;
                                    case "HouFurWallpaperPartition":
                                        furnitureNetId = 0x20C;
                                        itemBytes[2] = 1;
                                        break;
                                    default:
                                        PluginLog.Info($"ignore {furniture.Item.Value.Name}:{furniture.CustomTalk.Value.Name}");
                                        Array.Copy(itemBytes, 0, posArr, i, 24);
                                        count--;
                                        continue;
                                }
                            }
                        }

                        BitConverter.GetBytes(furnitureNetId).CopyTo(itemBytes, 0);
                        itemBytes[4] = item.Stain;
                        BitConverter.GetBytes(item.Rotate).CopyTo(itemBytes, 8);
                        BitConverter.GetBytes(item.X).CopyTo(itemBytes, 12);
                        BitConverter.GetBytes(item.Y).CopyTo(itemBytes, 16);
                        BitConverter.GetBytes(item.Z).CopyTo(itemBytes, 20);
                        /*
                        byte[] tmpArr = new byte[24];
                        Array.Copy(itemBytes, 0, tmpArr, 0, 24);
                        PluginLog.Log($"Replace {item.Name}:" + (BitConverter.ToString(tmpArr).Replace("-", " ")));
                        */
                        Array.Copy(itemBytes, 0, posArr, i, 24);
                    }
                }
                Log(String.Format(_localizer.Localize("Previewing {0} furnitures."), count));
                PreviewTerritory = ClientState.TerritoryType;
                Marshal.Copy(posArr, 0, dataPtr, 2416);
                return this.LoadHousingFuncHook.Original(a1, a2);
            }


            Marshal.Copy(dataPtr, posArr, 0, 2416);
            if (DateTime.Now > Config.lastPosPackageTime.AddSeconds(5))
            {
                HousingItemList.Clear();
                // Config.HousingItemList.Clear();
                Config.lastPosPackageTime = DateTime.Now;
                Config.Save();
            }
            /*
            byte[] headArr = new byte[12];
            Array.Copy(posArr, 0, headArr, 0, 12);
            Log($"Head: {BitConverter.ToString(headArr).Replace('-', ' ')}");
            */
            int cnt = 0;
            for (int i = 12; i < posArr.Length && i + 24 < posArr.Length; i += 24)
            {
                uint furnitureKey = (uint)(BitConverter.ToUInt16(posArr, i) + 0x30000);
                var furniture = Data.GetExcelSheet<HousingFurniture>().GetRow(furnitureKey);
                var item = furniture.Item.Value;
                if (item.RowId == 0) continue;
#if DEBUG
                byte[] tmpArr = new byte[24];
                Array.Copy(posArr, i, tmpArr, 0, 24);
                PluginLog.Info($"{item.Name}:" + (BitConverter.ToString(tmpArr).Replace("-", " ")));
                if (furniture.CustomTalk.RowId > 0 || furniture.Item.Value.Name.ToString().EndsWith("空白隔离墙"))
                {
                    string talk = furniture.CustomTalk.Value.Name.ToString();
                    PluginLog.Info($"FurnitureTalk {furniture.Item.Value.Name}: {talk}");
                    PluginLog.Info(BitConverter.ToString(tmpArr).Replace("-", " "));
                }
#endif

                byte stain = posArr[i + 4];
                var rotate = BitConverter.ToSingle(posArr, i + 8);
                var x = BitConverter.ToSingle(posArr, i + 12);
                var y = BitConverter.ToSingle(posArr, i + 16);
                var z = BitConverter.ToSingle(posArr, i + 20);
                cnt++;
                HousingItemList.Add(new HousingItem(
                        furnitureKey,
                        furniture.ModelKey,
                        item.RowId,
                        stain,
                        x,
                        y,
                        z,
                        rotate,
                        item.Name.ToString()
                    ));
            }
            return this.LoadHousingFuncHook.Original(a1, a2);
        }
        private void TerritoryChanged(ushort e)
        {
            Config.DrawScreen = false;
            Config.Save();
        }
        public void CommandHandler(string command, string arguments)
        {
            var args = arguments.Trim().Replace("\"", string.Empty);

            if (string.IsNullOrEmpty(args) || args.Equals("config", StringComparison.OrdinalIgnoreCase))
            {
                Gui.ConfigWindow.Visible = !Gui.ConfigWindow.Visible;
                return;
            }
        }

        public void Log(string message, string detail_message = "")
        {
            //if (!Config.PrintMessage) return;
            var msg = $"[{Name}] {message}";
            PluginLog.Info(detail_message == "" ? msg : detail_message);
            ChatGui.Print(msg);
        }
        public void LogError(string message, string detail_message = "")
        {
            //if (!Config.PrintError) return;
            var msg = $"[{Name}] {message}";
            PluginLog.Error(detail_message == "" ? msg : detail_message);
            ChatGui.PrintError(msg);
        }
        public void OnNetwork(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (direction == NetworkMessageDirection.ZoneDown)
            {
                ushort[] filteredOutOpcodes = new ushort[] { 0x18B, 0x1C5, 0x2B5, 0x36B, 0x206, 0x3A2, 0x145, 0x1E9, 0x36B };
                byte[] posArr = new byte[2416];
                Marshal.Copy(dataPtr, posArr, 0, 12);
                if (BitConverter.ToString(posArr).Replace("-", " ").StartsWith("FF FF FF FF FF FF FF FF"))
                {
                    if (filteredOutOpcodes.ToList().IndexOf(opCode) == -1)
                        Log($"Receive likely yard opcode:0x{opCode:X}");
                }
                //if (filteredOutOpcodes.ToList().IndexOf(opCode) == -1)
                //    Log($"Receive opcode:0x{opCode:X}");
            }
        }
    }

}
