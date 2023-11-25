# LCUltrawide

Lethal Company is locked to 16:9 aspect ratio by default and will add black bars on the sides of ultrawide monitors.  
This mod makes some changes to the games rendering and UI to enable better support for any custom resolution and aspect ratio.

## Features

- Modify the games rendering resolution with full support for aspect ratios other than 16:9
- Improve visibility by increasing the resolution (at the cost of performance)
- Allows changing of HUD scale and aspect ratio
- Fixes the inventory slots being slightly misaligned on some monitors
- More robust code for the Scanner HUD to ensure correct position of markers


## Installation

1. Make sure you have **BepInEx** installed for the game (see https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/)
2. Download the [latest release](https://github.com/stefan750/LCUltrawide/releases/latest) of the mod from the releases page
3. Navigate to the games install folder (you can right click the game in your Steam library, select "Manage" and then "Browse Local Files" to easily find it)
4. Copy the downloaded **LCUltrawide.dll** to the **BepInEx/plugins/** folder
5. Launch the game once with the mod installed to generate the config file

## Usage

1. Open the config file located under **BepInEx/config/LCUltrawide.cfg** inside the games install directory.  
2. Change the **Width** and **Height** values under the **\[Resolution\]** section to your preferred resolution and aspect ratio.  
    NOTE: the games default render resolution is 860x520, increasing it too far beyond these values can cause performance issues and will remove the "pixelated" style of the game. It is recommended to leave the Height at its default and only adjust the Width according to your monitors aspect ratio for best results.
3. (Optional) Change the values under the **\[UI\]** section to adjust the scale and aspect ratio of the ingame HUD.
