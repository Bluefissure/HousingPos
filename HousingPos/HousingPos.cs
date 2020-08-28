using System;
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

namespace HousingPos
{
    public class HousingPos : IDalamudPlugin
    {
        public string Name => "HousingPos";
        public PluginUi Gui { get; private set; }
        public DalamudPluginInterface Interface { get; private set; }
        public Configuration Config { get; private set; }
        public OffsetDefinition Offset { get; private set; }
        public SigScanner Scanner { get; private set; }
        public void Dispose()
        {
            Config.PlaceAnywhere = false;
            PlaceAnywhere();
            Interface.Framework.Network.OnNetworkMessage -= OnNetwork;
            Interface.CommandManager.RemoveHandler("/xhouse");
            Gui?.Dispose();
            Interface?.Dispose();
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            Interface = pluginInterface;
            Scanner = Interface.TargetModuleScanner;
            Config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Config.Initialize(pluginInterface);
            LoadOffset();
            Initialize();
            Interface.CommandManager.AddHandler("/xhouse", new CommandInfo(CommandHandler)
            {
                HelpMessage = "/xhouse - load housing item list."
            });
            Gui = new PluginUi(this);
            Interface.Framework.Network.OnNetworkMessage += OnNetwork;
        }
        public void Initialize()
        {
            PlaceAnywhere();
        }
        public void LoadOffset()
        {
            string OffsetFilePath = Path.Combine(Assembly.GetExecutingAssembly().Location, "../offset.json");
            Offset = JsonConvert.DeserializeObject<OffsetDefinition>(File.ReadAllText(OffsetFilePath));
            var verFilePath = Path.Combine(Scanner.Module.FileName, "../ffxivgame.ver");
            var gameVer = File.ReadAllText(verFilePath);
            if (gameVer.Trim() != Offset.ExeVersion.Trim())
            {
                string message = $"Unsupported game version: {gameVer}";
                LogError(message);
                throw new Exception($"[{Name}] {message}");
            }
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

        public bool PlaceAnywhere()
        {
            IntPtr baseAddr = Scanner.Module.BaseAddress;
            int placeAnywhereOff = Int32.Parse(Offset.PlaceAnywhere, NumberStyles.HexNumber);
            IntPtr placeAnywhereAddr = baseAddr + placeAnywhereOff;
            int wallPartitionOff = Int32.Parse(Offset.WallPartition, NumberStyles.HexNumber);
            IntPtr wallPartitionAddr = baseAddr + wallPartitionOff;
            if (Config.PlaceAnywhere)
            {
                Memory.Write(placeAnywhereAddr, new byte[] { 1 });
                Memory.Write(wallPartitionAddr, new byte[] { 1 });
            }
            else
            {
                Memory.Write(placeAnywhereAddr, new byte[] { 0 });
                Memory.Write(wallPartitionAddr, new byte[] { 0 });
            }
            return Config.PlaceAnywhere;
        }

        public void ReadPrePosition()
        {
            IntPtr baseAddr = Scanner.Module.BaseAddress;
            int prePositionOff = Int32.Parse(Offset.PrePositionBase, NumberStyles.HexNumber);
            IntPtr prePositionAddr = baseAddr + prePositionOff;
            int[] offList = Offset.PrePositionOffset.Split(',').Select( off => Int32.Parse(off, NumberStyles.HexNumber)).ToArray();
            foreach (var off in offList)
            {
                prePositionAddr = Marshal.ReadIntPtr(prePositionAddr);
                prePositionAddr += off;
            }
            byte[] posArr = new byte[12];
            Marshal.Copy(prePositionAddr, posArr, 0, 12);
            var x = BitConverter.ToSingle(posArr, 0);
            var y = BitConverter.ToSingle(posArr, 4);
            var z = BitConverter.ToSingle(posArr, 8);
            Config.PlaceX = x;
            Config.PlaceY = y;
            Config.PlaceZ = z;
            Config.Save();
        }

        public void WritePrePosition()
        {
            IntPtr baseAddr = Scanner.Module.BaseAddress;
            int prePositionOff = Int32.Parse(Offset.PrePositionBase, NumberStyles.HexNumber);
            IntPtr prePositionAddr = baseAddr + prePositionOff;
            int[] offList = Offset.PrePositionOffset.Split(',').Select(off => Int32.Parse(off, NumberStyles.HexNumber)).ToArray();
            foreach (var off in offList)
            {
                prePositionAddr = Marshal.ReadIntPtr(prePositionAddr);
                prePositionAddr += off;
            }
            byte[] posArr = new byte[12];
            byte[] bx = BitConverter.GetBytes(Config.PlaceX);
            bx.CopyTo(posArr, 0);
            byte[] by = BitConverter.GetBytes(Config.PlaceY);
            by.CopyTo(posArr, 4);
            byte[] bz = BitConverter.GetBytes(Config.PlaceZ);
            bz.CopyTo(posArr, 8);
            Memory.Write(prePositionAddr, posArr);
        }

        public void OnNetwork(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            var OpcodeLoadHousing = Int32.Parse(Offset.LoadHousing, NumberStyles.HexNumber);
            var OpcodeMoveItem = Int32.Parse(Offset.MoveItem, NumberStyles.HexNumber);
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
                    Config.SelectedItemIndex = -1;
                    Config.HousingItemList.Clear();
                    Config.Save();
                    return;
                }
                if (DateTime.Now > Config.lastPosPackageTime.AddSeconds(5))
                {
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
                    Config.HousingItemList.Add(new HousingItem(
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
                if (opCode != OpcodeMoveItem || !Config.ForceMove)
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
                Log($"Moving to ({x:N3}, {y:N3}, {z:N3}, {rotate:N3})");
                x = Config.PlaceX;
                y = Config.PlaceY;
                z = Config.PlaceZ;
                rotate = Config.PlaceRotate;
                Log($"Forced to ({x:N3}, {y:N3}, {z:N3}, {rotate:N3})");
                byte[] bx = BitConverter.GetBytes(x);
                bx.CopyTo(posArr, 12);
                byte[] by = BitConverter.GetBytes(y);
                by.CopyTo(posArr, 16);
                byte[] bz = BitConverter.GetBytes(z);
                bz.CopyTo(posArr, 20);
                byte[] br = BitConverter.GetBytes(rotate);
                br.CopyTo(posArr, 24);
                Marshal.Copy(posArr, 0, dataPtr, 28);
                Config.ForceMove = false;
                Config.Save();
                WritePrePosition();
            }
        }
    }

}
