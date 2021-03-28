# HousingPos
[![EN doc](https://img.shields.io/badge/doc-EN-brightgreen)](https://github.com/Bluefissure/HousingPos/blob/master/README.md)

HousingPos 是一个可读取并保存FF14房屋家具位置预设的Dalamud插件。 
它可以保存当前的房屋家具放置预设并在之后应用他们。

![quicker_27a98420-c717-4735-b3d6-5c331655c59d.png](https://i.loli.net/2021/01/18/GS6HkexFmKjJn5v.png)

## 安装

请转到[这个页面](https://github.com/Bluefissure/DalamudPlugins/tree/Bluefissure).

在游戏中使用 `/xhouse` 命令来打开配置面板。

## 指示

请在进入房间前清空家具列表。在打开 `布置家具` 页面后，家具列表将会自动完成读取。

BDTH: 启用BDTH后，本插件将与 [BDTHPlugin](https://github.com/LeonBlade/BDTHPlugin) 整合。点击设置按钮将会自动调用BDTH的放置命令。 

### 全屋导入/导出

复制：将家具列表复制到剪贴板。同时会获得提示：将？个物品复制到了剪贴板。仅用于采购家具，保存预设请使用导入/导出。  

导出：将家具位置预设导出到剪贴板。提示：将？个物品导出到了剪贴板。  

导入：将家具位置预设从剪贴板导入到插件。提示：从剪贴板导入了？个物品。   

### 创建/导出家具组合

分组：
- 点击 `分组` 按钮后进入分组模式，可添加家具到分组内，其中第一个家具会被当作基准，其余的通过相对坐标计算。
- 选择完成后再点击一下`分组中`按钮即可退出分组模式，刚刚分组的家具会被附加进第一个家具的数据中，此时使用单独导出即可将该家具组合导出。
- 勾选 `单独导出` 可以单独导出某个家具或分组。
- 应用分组是需要在XYZR处手动更改基准家具的坐标（和BDTH坐标保持一致），然后组内其余家具便会自动计算更新后的坐标。

### 屏幕绘制

屏幕绘制：在屏幕上绘制家具。 

### 通过云服务导出与导入

云导出/云导入：
- 类似于全屋导出/导入,可在点击后出现的界面中更改上传地址（需建立 [后端](https://github.com/lclichen/BackendForHousingPos)）。
- 推荐使用 [IMEPlugin](https://github.com/Bluefissure/IMEPlugin) 进行中文输入。
- 可选输入内容：位置（对全屋风格选择或许有用）、房屋类型（可用于区分房屋大小）、标签（可 `自订标签` ）、署名（可选 `匿名`）。


## 常见问题

- 我可以使用本工具获取别人的家具预设吗?

  不可以，你只能读取你 **具有装修权限** 的房屋中的家具。
