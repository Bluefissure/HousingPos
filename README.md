# HousingPos

HousingPos is a Dalamud plugin that reads and saves FF14 housing furniture position presets. It can save the current housing furniture placement presets and apply them later.

![quicker_27a98420-c717-4735-b3d6-5c331655c59d.png](https://i.loli.net/2021/01/18/GS6HkexFmKjJn5v.png)

## Installation

[Download](https://github.com/Bluefissure/HousingPos/releases/latest), unzip it and throw it to `XIVLauncher\devPlugins`.

Use `/xhouse` in the game to open the config panel.

## Instructions

Please clear the furniture list before entering the room. The furniture list will be automatically read after opening the `Housing-Indoor Furnishings` page.

BDTH: After enabling it, it will integrate with [BDTHPlugin](https://github.com/LeonBlade/BDTHPlugin). Clicking on the setting will automatically invoke the BDTH placement command.

Force move: The target position is forced to change when the furniture is moved, and the effect needs to be re-entered to be seen. Used to place furniture in batches to restore the furniture presets. **Use at your own risk.**

## Update

Since the opcode is updated with the version, the furniture list and forc move function will become invalid after each patch update. Please submit an issue to request an update to the definitions.
