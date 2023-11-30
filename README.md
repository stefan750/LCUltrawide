# LCUltrawide

Lethal Company is locked to 16:9 aspect ratio by default and will add black bars on the sides of ultrawide monitors.  
This mod makes some changes to the games rendering and UI to enable support for any custom resolution and aspect ratio.

## Features

- Automatically detects monitor aspect ratio and scales the game to fill the entire screen (works even in window mode!)
- Increase the games resolution for better visibility (at the cost of performance)
- Allows changing of HUD scale and aspect ratio
- Fixes the inventory slots being slightly misaligned on some monitors
- Fixes the UI being slightly too large on wider monitors
- More robust code for the Scanner HUD to ensure correct position of markers

## Installation

1. Make sure you have [BepInEx](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/) installed for the game
2. Download the latest version of the mod from [Thunderstore](https://thunderstore.io/c/lethal-company/p/stefan750/LCUltrawide/) or [GitHub releases](https://github.com/stefan750/LCUltrawide/releases/latest)
3. Navigate to the games install folder (you can right click the game in your Steam library, select "Manage" and then "Browse Local Files" to easily find it)
4. Copy the BepInEx folder from the downloaded .zip into your game folder making sure the contents end up in the already existing folders

## Usage

By default the mod will take the original game resolution and automatically scale it to fit your monitor.
Optionally you can change the resolution and UI scale and aspect ratio in the mods config file **BepInEx/config/LCUltrawide.cfg**.