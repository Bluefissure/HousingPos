using Lumina.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingPos.Gui
{
    class Localizer
    {
        public string Language = "en";
        private Dictionary<string, string> zh = new Dictionary<string, string> { };
        public Localizer(string language="en")
        {
            Language = language;
            LoadZh();
        }
        public string Localize(string message)
        {
            if (message == null) return message;
            if (Language == "zh") return zh.ContainsKey(message) ? zh[message] : message;
            return message;
        }
        private void LoadZh()
        {
            zh.Add("Panel", "面板");
            zh.Add("Recording", "记录");
            zh.Add("Automatically record housing item list.", "自动记录家具列表。");
            zh.Add("Tooltips", "提示");
            zh.Add("Language:", "语言:");
            zh.Add("Change the UI Language.", "改变界面语言。");
            zh.Add("Force Move", "强制移动");
            zh.Add("Force the position when moving items (cannot be seen until re-enter).",
                    "移动家具时强行设定位置，重新进入才能看到变化。");
            zh.Add("BDTH integrate: leave the position set to BDTH. \n" + "(Note that BDTH cannot set rotation.)",
                    "BDTH 集成: 将摆放过程交给 BDTH。 \n" + "(注意 BDTH 无法设定旋转。)");
            zh.Add("Automatically place item at specified position.", "自动将家具摆放在相应位置。");
            zh.Add("Export", "导出");
            zh.Add("Import", "导入");
            zh.Add("Copied {0} items to your clipboard.", "将{0}个物品复制到了剪贴板。");
            zh.Add("Exported {0} items to your clipboard.", "将{0}个物品导出到了剪贴板。");
            zh.Add("Imported {0} items from your clipboard.", "从剪贴板导入了{0}个物品。");
            zh.Add("Rotate:", "旋转:");
            zh.Add("Name", "名称");
            zh.Add("Rotate", "旋转");
            zh.Add("Set", "设置");
            zh.Add("BDTH Set", "BDTH 设置");
            zh.Add("Clear", "清空");
            zh.Add("Copy", "复制");
            zh.Add("Place Anywhere", "任意摆放");
            zh.Add("Load {0} furnitures.", "读取了{0}个家具。");
            zh.Add("Place item at anywhere.", "将物品移动到任何地方。");
            zh.Add("Please clear the furniture list and re-enter house to load current furniture list.", "请清空家具列表并重新进入房屋以读取当前家具列表。");
            zh.Add("Read Position", "读取位置");
            zh.Add("Sync Position", "同步位置");
            zh.Add("Read the starting position of currently selected item to XYZ. Invalid if syncing position.", "读取当前选中物品的初始位置到XYZ。同步位置时无效。");
            zh.Add("Sync the starting position of currently selected item with XYZ.", "同步XYZ与当前选中物品的初始位置。");
            zh.Add("Sort", "排序");
            zh.Add("Only for purchasing, please use Export/Import for the whole preset.\n", "仅用于采购家具，保存预设请使用导入/导出。\n");
        }
    }
}
