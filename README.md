# HousingPos
[![CN doc](https://img.shields.io/badge/doc-%E4%B8%AD%E6%96%87-brightgreen)](https://github.com/Bluefissure/HousingPos/blob/master/README_CN.md)

HousingPos is a Dalamud plugin that reads and saves FF14 housing furniture position presets. It can save the current housing furniture placement presets and apply them later.

![quicker_27a98420-c717-4735-b3d6-5c331655c59d.png](https://i.loli.net/2021/01/18/GS6HkexFmKjJn5v.png)

## Installation

Please refer to [this page](https://github.com/Bluefissure/DalamudPlugins/tree/Bluefissure).

Use `/xhouse` in the game to open the config panel.

## Instructions

Please clear the furniture list before entering the room. The furniture list will be automatically read after opening the `Housing-Indoor Furnishings` page.

BDTH: After enabling it, it will integrate with [BDTHPlugin](https://github.com/LeonBlade/BDTHPlugin). Clicking on the set button will automatically invoke the BDTH placement command.

### Import/Export Of The Whole House

Copy：Copy the furniture list to the clipboard. At the same time, you will be prompted: Copied ? items to your clipboard. For purchasing furniture only, please use `Import` / `Export` to save the preset.

Export：Export the furniture position preset to the clipboard. Hint：Exported ? items to your clipboard.

Import：Import the furniture position preset from the clipboard. Hint：Imported ? items to your clipboard.  

### Create/Export Group Of Furnitures

Group：
- Click the `group` button to enter the group mode. Furniture can be added to the group. The first furniture will be used as a benchmark, and the rest will be calculated by relative coordinates.    
- After selection, click the `grouping` button to exit the group mode. The grouped furniture will be added to the data of the first furniture. At this time, the furniture combination can be exported by using `Single Export`.
- Check `Single Export` to export a furniture or group separately.  
- For application grouping, you need to manually change the coordinates of the benchmark furniture at **XYZR** *(consistent with BDTH coordinates)*, and then the rest of the furniture in the group will automatically calculate the updated coordinates.  
### Draw on screen

Draw on screen：Draw items on screen.  

### Export and Import with Cloud

Cloud Export / Cloud Import：
- Just like The Whole House's Import / Export,You can change the Server Address in the interface that appears after clicking `Cloud Export`.If you want, you can set up [the Back End](https://github.com/lclichen/BackendForHousingPos) yourself.

- [IMEPlugin](https://github.com/Bluefissure/IMEPlugin) is recommended for Chinese input.

- Optional input contents: Location (Which may be useful for the selection of house style), size (Can be used to distinguish the house size), Tags (You can `Add Custom Tags`), Uper (`Anonymous` optional).

## FAQs

- Can I use it to steal others' housing presets?

  No, you can only read the furniture list of the house **which you can decorate**.
