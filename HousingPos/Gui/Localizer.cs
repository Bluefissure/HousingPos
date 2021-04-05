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
        private Dictionary<string, string> en = new Dictionary<string, string> { };
        public Localizer(string language="en")
        {
            Language = language;
            LoadZh();
        }
        public string Localize(string message)
        {
            if (message == null) return message;
            if (Language == "zh") return zh.ContainsKey(message) ? zh[message] : message;
            if (Language == "en") return en.ContainsKey(message) ? en[message] : message;
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
            zh.Add("BDTH integrate: leave the position set to BDTH.",
                    "BDTH 集成: 将摆放过程交给 BDTH。");
            zh.Add("Automatically place item at specified position.", "自动将家具摆放在相应位置。");
            zh.Add("Export", "导出");
            zh.Add("Import", "导入");
            zh.Add("Upload", "上传");
            zh.Add("Copied {0} items to your clipboard.", "将{0}个物品复制到了剪贴板。");
            zh.Add("Exported {0} items to your clipboard.", "将{0}个物品导出到了剪贴板。");
            zh.Add("Imported {0} items from your clipboard.", "从剪贴板导入了{0}个物品。");
            zh.Add("Imported {0} chocobo items from your clipboard, {1} failed.", "从剪贴板导入了{0}个陆行鸟装修导出的物品, {1}个失败了。");
            zh.Add("Rotate:", "旋转:");
            zh.Add("Name", "名称");
            zh.Add("Rotate", "旋转");
            zh.Add("Set", "设置");
            zh.Add("BDTH Set", "BDTH 设置");
            zh.Add("Copy", "复制");
            zh.Add("Single Export", "单独导出");
            zh.Add("Single Upload", "单独上传");
            zh.Add("Add Export button to the single furnitures.", "对单独的家具添加导出按钮。");
            zh.Add("Group", "分组");
            zh.Add("Add", "添加");
            zh.Add("Lavender Beds", "薰衣草苗圃");
            zh.Add("The Goblet", "高脚孤丘");
            zh.Add("Mist", "海雾村");
            zh.Add("Shirogane", "白银乡");
            zh.Add("Combination", "组合"); 
            zh.Add("WholeHouse","全屋");
            zh.Add("Far Eastern-style", "东方风格");
            zh.Add("Modern-style", "现代风格");
            zh.Add("Loft-style", "LOFT风格");
            zh.Add("Plants", "植物");
            zh.Add("Cartoon-style", "卡通风格");
            zh.Add("Anonymous", "匿名");
            zh.Add("Clear", "清空");
            zh.Add("Close", "关闭");
            zh.Add("Cancel", "取消");
            zh.Add("Location", "位置");
            zh.Add("Send Data", "发送数据");
            zh.Add("Upload Name:", "命名:");
            zh.Add("Selected Tags:", "已选标签:");
            zh.Add("Available Tags:", "可选标签:");
            zh.Add("Add Custom Tags:", "自订标签");
            zh.Add("Use Default Cloud Service", "使用默认云服务");
            zh.Add("Here are {0} items that will be sent.", "{0}个家具的数据将被发送。");
            zh.Add("Imported {0} items from Cloud.", "从服务器导入了{0}个家具");
            zh.Add("Uploader:", "署名:");
            zh.Add("Server Address:", "服务器地址:");
            zh.Add("Del", "删除");
            zh.Add("Disband", "解散");
            zh.Add("Grouping", "分组中");
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
            zh.Add("Draw on screen", "屏幕绘制");
            zh.Add("Draw items on screen.", "在屏幕上绘制家具。");
            zh.Add("Undo", "撤销");
            zh.Add("Unknown", "未知");
            zh.Add("Undo the on-screen setting.", "撤销屏幕摆放。");
            zh.Add("Drawing Distance:", "绘制距离:");
            zh.Add("Cloud Upload", "云上传");
            zh.Add("Cloud Download", "云下载");
            zh.Add("Name Your Export","命名你的导出内容。");
            zh.Add("Tag Your Export", "为导出内容添加标签。");
            zh.Add("Uploading {0} items to Cloud.", "正在将{0}个物品上传到服务器。");
            zh.Add("Only draw items within this distance to your character. (0 for unlimited)", "只绘制距离以内的家具。（0为无限）");
            zh.Add("Send Data To Leancloud", "发送到LeanCloud");
            zh.Add("LeanCloud Import","LeanCloud导入");
            zh.Add("Imported {0} items from LeanCloud.", "从LeanCloud导入了{0}个物品");
            zh.Add("Salt used to encrypt your user id.", "用于加密用户ID的MD5盐值。");
            zh.Add("Md5 Salt:", "MD5盐值：");
            zh.Add("Open server which stores all uploaded data.", "存储上传数据的公开服务器。");
            zh.Add("Preview", "预览");
            zh.Add("Exit your house to disable preview.", "离开房屋才能取消预览。");
            zh.Add("Preview the current decoration plan when entering house.", "在进入房屋时预览当前装修方案。");
            zh.Add("Decorate in preview mode is dangerous!", "预览模式下装修可能会造成危险！");
            zh.Add("Please exit the house and disable preview!", "请退出房屋并关闭预览选项！");
            zh.Add("Previewing {0} furnitures.", "预览了{0}个家具。");
        }
    }
}
