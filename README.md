# UnityPlayModeSwitcher
Unity editor window to quickly toggle domain and scene reloading for faster Play Mode entry.

## Before
![before](https://github.com/user-attachments/assets/83646def-8579-45b9-badf-a8da031cee0f)
## Before
![after](https://github.com/user-attachments/assets/16b38eda-bebf-4dbb-ab2d-4b05e251f77b)


## Features

*   Provides a simple window with buttons to switch between the four "Enter Play Mode" options.
*   Easy to install and use.
*   Helps improve iteration time by making it easy to disable domain and scene reloads.

## Installation

1.  Clone this repository or download the source code.
2.  Copy the `Editor` folder into your Unity project's `Assets` directory.
3.  Unity will automatically compile the script.

## How to Use

1.  After the script has compiled, you will find a new menu item at the top of the Unity Editor: **Window > Play Mode Switch**.
2.  Click on **Play Mode Switch** to open the utility window.
3.  You can now use the buttons in this window to easily switch between the different "Enter Play Mode" options.
4.  You can dock the window anywhere in the editor for easy access.

## "Enter Play Mode" Options

*   **Reload Domain & Scene:** The default Unity behavior. Both the scripting domain and the current scene are reloaded when you enter Play Mode.
*   **Reload Scene Only:** The scripting domain is not reloaded, but the current scene is.
*   **Reload Domain Only:** The scene is not reloaded, but the scripting domain is.
*   **Do Not Reload:** Neither the domain nor the scene are reloaded. This provides the fastest entry into Play Mode.
