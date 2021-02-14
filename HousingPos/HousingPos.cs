﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud;
using Dalamud.Plugin;
using Dalamud.Game.Command;
using Dalamud.Game.Internal.Network;
using Lumina;
using Lumina.Excel.GeneratedSheets;
using HousingPos.Objects;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Reflection;
using System.IO;
using System.Globalization;
using Dalamud.Game;
using Dalamud.Hooking;
using HousingPos.Gui;

namespace HousingPos
{
    public class HousingPos : IDalamudPlugin
    {
        public string Name => "HousingPos";
        public PluginUi Gui { get; private set; }
        public DalamudPluginInterface Interface { get; private set; }
        public Configuration Config { get; private set; }
        public OpcodeDefinition Opcode { get; private set; }
        public CommandManager CommandManager = null;
        public SigScanner Scanner { get; private set; }

        private Localizer _localizer;

        public List<HousingItem> HousingItemList = new List<HousingItem>();

        public IntPtr LoadHousingFunc;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void LoadHousingFuncDelegate(Int64 a1, Int64 a2);
        private Hook<LoadHousingFuncDelegate> LoadHousingFuncHook;
        public IntPtr UIFunc;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void UIFuncDelegate(Int64 a1, UInt32 a2, char a3);
        private Hook<UIFuncDelegate> UIFuncHook;
        public void Dispose()
        {
            LoadHousingFuncHook.Disable();
            UIFuncHook.Disable();
            Config.PlaceAnywhere = false;
            Interface.ClientState.TerritoryChanged -= TerritoryChanged;
            Interface.Framework.Network.OnNetworkMessage -= OnNetwork;
            Interface.CommandManager.RemoveHandler("/xhouse");
            Gui?.Dispose();
            Interface?.Dispose();
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            Interface = pluginInterface;
            CommandManager = pluginInterface.CommandManager;
            Scanner = Interface.TargetModuleScanner;
            Config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Config.Initialize(pluginInterface);
            _localizer = new Localizer(Config.UILanguage);
            // LoadOffset();
            Initialize();
            Interface.CommandManager.AddHandler("/xhouse", new CommandInfo(CommandHandler)
            {
                HelpMessage = "/xhouse - load housing item list."
            });
            Gui = new PluginUi(this);
            Interface.Framework.Network.OnNetworkMessage += OnNetwork;
            Interface.ClientState.TerritoryChanged += TerritoryChanged;
        }
        public void Initialize()
        {
            UIFunc = Interface.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 89 77 04") + 5;
            LoadHousingFunc = Interface.TargetModuleScanner.ScanText("48 8B 41 08 48 85 C0 74 09 48 8D 48 10");

            UIFuncHook = new Hook<UIFuncDelegate>(
                UIFunc,
                new UIFuncDelegate(UIFuncDetour)
            );
            LoadHousingFuncHook = new Hook<LoadHousingFuncDelegate>(
                LoadHousingFunc,
                new LoadHousingFuncDelegate(LoadHousingFuncDetour)
            );
            UIFuncHook.Enable();
            LoadHousingFuncHook.Enable();
        }
        private void UIFuncDetour(Int64 a1, UInt32 a2, char a3)
        {
            //Log($"TestFuncHook: {a1}, {a2}, {(int)a3}");
            if (a2 == 67 && a3 == 1)
            {
                if (HousingItemList.Count > 0 && Config.HousingItemList.Count == 0)
                {
                    Log(String.Format(_localizer.Localize("Load {0} furnitures."), HousingItemList.Count));
                    Config.HousingItemList = HousingItemList.ToList();
                    Config.HiddenScreenItemHistory = new List<int>();
                    Config.Save();
                }
                else
                {
                    Log(_localizer.Localize("Please clear the furniture list and re-enter house to load current furniture list."));
                }
            }
            if (a2 == 48 && a3 == 1 && Config.Recording)
            {
                if (HousingItemList.Count > 0 && Config.HousingItemList.Count == 0)
                {
                    Log(String.Format(_localizer.Localize("Load {0} furnitures."), HousingItemList.Count));
                    Log($"偷家有风险，使用需谨慎.");
                    Config.HousingItemList = HousingItemList.ToList();
                    Config.HiddenScreenItemHistory = new List<int>();
                    Config.Save();
                }
                else
                {
                    Log(_localizer.Localize("Please clear the furniture list and re-enter house to load current furniture list."));
                }
            }
            this.UIFuncHook.Original(a1, a2, a3);
        }


        private void LoadHousingFuncDetour(Int64 a1, Int64 a2)
        {
            IntPtr dataPtr = (IntPtr)a2;

            byte[] posArr = new byte[2416];
            Marshal.Copy(dataPtr, posArr, 0, 2416);
            if (BitConverter.ToString(posArr).Replace("-", " ").StartsWith("FF FF FF FF FF FF FF FF"))
            {
                Config.SelectedItemIndex = -1;
                HousingItemList.Clear();
                Config.HousingItemList.Clear();
                Config.HiddenScreenItemHistory = new List<int>();
                Config.Save();
                this.LoadHousingFuncHook.Original(a1, a2);
                return;
            }
            if (DateTime.Now > Config.lastPosPackageTime.AddSeconds(5))
            {
                HousingItemList.Clear();
                Config.HousingItemList.Clear();
                Config.lastPosPackageTime = DateTime.Now;
                Config.Save();
            }
            for (int i = 12; i < posArr.Length && i + 24 < posArr.Length; i += 24)
            {
                var modelKey = BitConverter.ToUInt16(posArr, i);
                var item = Interface.Data.GetExcelSheet<HousingFurniture>().GetRow((uint)(modelKey + 196608)).Item.Value;
                if (item.RowId == 0) continue;
                var rotate = BitConverter.ToSingle(posArr, i + 8);
                var x = BitConverter.ToSingle(posArr, i + 12);
                var y = BitConverter.ToSingle(posArr, i + 16);
                var z = BitConverter.ToSingle(posArr, i + 20);
                HousingItemList.Add(new HousingItem(
                        modelKey,
                        item.RowId,
                        x,
                        y,
                        z,
                        rotate,
                        item.Name
                    ));
            }
            this.LoadHousingFuncHook.Original(a1, a2);
        }
        private void TerritoryChanged(object sender, ushort e)
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

        public void Log(string message)
        {
            //if (!Config.PrintMessage) return;
            var msg = $"[{Name}] {message}";
            PluginLog.Log(msg);
            Interface.Framework.Gui.Chat.Print(msg);
        }
        public void LogError(string message)
        {
            //if (!Config.PrintError) return;
            var msg = $"[{Name}] {message}";
            PluginLog.LogError(msg);
            Interface.Framework.Gui.Chat.PrintError(msg);
        }

        public void OnNetwork(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            return;
            var OpcodeLoadHousing = Int32.Parse(Opcode.LoadHousing, NumberStyles.HexNumber);
            var OpcodeMoveItem = Int32.Parse(Opcode.MoveItem, NumberStyles.HexNumber);
            if (direction == NetworkMessageDirection.ZoneDown)
            {
                if (opCode != OpcodeLoadHousing || !Config.Recording)
                {
                    return;
                }
                byte[] posArr = new byte[2416];
                Marshal.Copy(dataPtr, posArr, 0, 2416);
                if (BitConverter.ToString(posArr).Replace("-", " ").StartsWith("FF FF FF FF FF FF FF FF"))
                {
                    HousingItemList.Clear();
                    return;
                }
                if (DateTime.Now > Config.lastPosPackageTime.AddSeconds(5))
                {
                    HousingItemList.Clear();
                    Config.HousingItemList.Clear();
                    Config.lastPosPackageTime = DateTime.Now;
                    Config.Save();
                }
                for (int i = 12; i < posArr.Length && i + 24 < posArr.Length; i += 24)
                {
                    var modelKey = BitConverter.ToUInt16(posArr, i);
                    var item = Interface.Data.GetExcelSheet<HousingFurniture>().GetRow((uint)(modelKey + 196608)).Item.Value;
                    if (item.RowId == 0) continue;
                    var rotate = BitConverter.ToSingle(posArr, i + 8);
                    var x = BitConverter.ToSingle(posArr, i + 12);
                    var y = BitConverter.ToSingle(posArr, i + 16);
                    var z = BitConverter.ToSingle(posArr, i + 20);
                    HousingItemList.Add(new HousingItem(
                            modelKey,
                            item.RowId,
                            x,
                            y,
                            z,
                            rotate,
                            item.Name
                        ));
                }
                Log($"Load {Config.HousingItemList.Count} items.");
                Config.Save();
            }
            else
            {
                //if (opCode != OpcodeMoveItem || !Config.ForceMove)
                if (opCode != OpcodeMoveItem)
                {
                    return;
                }
                byte[] posArr = new byte[28];
                Marshal.Copy(dataPtr, posArr, 0, 28);
                //Log(BitConverter.ToString(posArr).Replace("-", " "));
                float x = BitConverter.ToSingle(posArr, 12);
                float y = BitConverter.ToSingle(posArr, 16);
                float z = BitConverter.ToSingle(posArr, 20);
                float rotate = BitConverter.ToSingle(posArr, 24);
                Log($"({x:N3}, {y:N3}, {z:N3}, {rotate:N3}) ==>");
                x = Config.PlaceX;
                y = Config.PlaceY;
                z = Config.PlaceZ;
                rotate = Config.PlaceRotate;
                Log($"({x:N3}, {y:N3}, {z:N3}, {rotate:N3})");
                byte[] bx = BitConverter.GetBytes(x);
                bx.CopyTo(posArr, 12);
                byte[] by = BitConverter.GetBytes(y);
                by.CopyTo(posArr, 16);
                byte[] bz = BitConverter.GetBytes(z);
                bz.CopyTo(posArr, 20);
                byte[] br = BitConverter.GetBytes(rotate);
                br.CopyTo(posArr, 24);
                Marshal.Copy(posArr, 0, dataPtr, 28);
                // Config.ForceMove = false;
                Config.Save();
            }
        }
    }

}
