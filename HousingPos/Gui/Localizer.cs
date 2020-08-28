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
            zh.Add("First: select an item and place it to an arbitrary position, this checkbox will be turned off.\n" +
                    "Second: with sync position enabled, select it again and cancel placing to view its actually position.",
                    "首先：选择一个家具并将其放置在任意位置，此选框将被自动关闭。\n" +
                    "之后：在同步位置被勾选的情况下，再次选择它并取消放置以查看其实际位置。");
            zh.Add("Automatically place item at specified position.", "自动将家具摆放在相应位置。");
            zh.Add("Rotate:", "旋转:");
            zh.Add("Name", "名称");
            zh.Add("Rotate", "旋转");
            zh.Add("Set", "读取");
            zh.Add("Clear", "清空");
            zh.Add("Copy", "复制");
            zh.Add("Place Anywhere", "任意摆放");
            zh.Add("Place item at anywhere.", "将物品移动到任何地方。");
            zh.Add("Read Position", "读取位置");
            zh.Add("Sync Position", "同步位置");
            zh.Add("Read the starting position of currently selected item to XYZ. Invalid if syncing position.", "读取当前选中物品的初始位置到XYZ。同步位置时无效。");
            zh.Add("Sync the starting position of currently selected item with XYZ.", "同步XYZ与当前选中物品的初始位置。");
            zh.Add("Sort", "排序");
            zh.Add("Only for purchasing, please copy config file for the whole preset.\n", "仅用于采购家具，保存预设请复制配置文件。\n");
        }
    }
}
