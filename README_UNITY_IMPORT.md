# Project Frontier - Unity Import Instructions

## Prerequisites

You need to install **Unity Hub** and **Unity Editor** version **2022.3 LTS** or later.

### Required Unity Version
- **Recommended**: Unity 2022.3.x LTS
- **Minimum**: Unity 2022.2.x
- The project uses Unity Entities 1.0.16 and URP 14.0.11 which require Unity 2022.2+

## Installation Steps

### Step 1: Install Unity Hub
1. Download Unity Hub from: https://unity.com/download
2. Install Unity Hub on your system

### Step 2: Install Unity Editor
1. Open Unity Hub
2. Go to **Installs** tab
3. Click **Install Editor**
4. Select **Unity 2022.3.x LTS** (or latest 2022.3 version)
5. In the **Add Modules** section, ensure you select:
   - **Universal Windows Platform Build Support** (if on Windows)
   - **Linux Build Support** (if on Linux)
   - **Mac Build Support** (if on macOS)
6. Click **Install**

### Step 3: Import the Project
1. Open Unity Hub
2. Go to **Projects** tab
3. Click **Add** button
4. Navigate to and select the `/workspace` folder (the root folder containing Assets, ProjectSettings, Packages, Scenes)
5. The project will appear in your projects list
6. Click on the project to open it in Unity Editor

### Step 4: Let Unity Import Packages
When you first open the project:
1. Unity will automatically download and install all required packages from the Packages/manifest.json
2. Wait for the package manager to complete (check the bottom right corner for progress)
3. This may take several minutes depending on your internet connection

Required packages include:
- com.unity.entities (1.0.16)
- com.unity.entities.graphics (1.0.16)
- com.unity.physics (1.0.16)
- com.unity.rendering.universal (14.0.11)
- com.unity.inputsystem (1.7.0)
- com.unity.cinemachine (2.10.1)
- And more...

### Step 5: Wait for Asset Database Refresh
1. Unity will compile all C# scripts
2. Wait for the console to show "Compilation completed successfully"
3. The asset database will refresh

### Step 6: Open the Main Scene
1. In the Project window, navigate to `Assets/Scenes/`
2. Double-click `MainGame.unity` to open it
3. Alternatively, go to **File → Open Scene** and select MainGame.unity

### Step 7: Test the Game
1. Press the **Play** button (▶) in the Unity Editor
2. The game should start running
3. Check the Console window for any errors

## Troubleshooting

### If you see compilation errors:
1. Make sure all packages are installed correctly
2. Go to **Window → Package Manager**
3. Check if any packages show errors
4. Try clicking **Resolve** if available

### If scenes are missing:
1. The main scene is located at `Assets/Scenes/MainGame.unity`
2. Check **File → Build Settings** to ensure the scene is added to the build

### If you get shader compilation errors:
1. Wait a few moments - shaders compile on first import
2. Go to **Edit → Preferences → Graphics** and check shader settings

### For ECS/DOTS related issues:
1. Ensure Unity Entities package version 1.0.16 is installed
2. The project uses Unity DOTS 1.0 architecture

## Build Instructions

To create a standalone build:
1. Go to **File → Build Settings**
2. Select your target platform (Windows, Mac, Linux)
3. Click **Switch Platform** if needed
4. Ensure `MainGame` scene is in the "Scenes In Build" list
5. Click **Build** or **Build And Run**
6. Choose an output folder

## System Requirements

- **OS**: Windows 10/11, macOS 10.15+, or Ubuntu 18.04+
- **RAM**: 16 GB minimum (32 GB recommended)
- **Storage**: 10 GB free space
- **GPU**: DirectX 11/12 compatible graphics card

## Support

If you encounter issues:
1. Check Unity Console for error messages
2. Verify all packages are correctly installed
3. Ensure you're using Unity 2022.3.x LTS
4. Try deleting the `Library` folder and reopening the project

---

**Project Version**: 1.0
**Unity Version**: 2022.3.x LTS
**Render Pipeline**: Universal Render Pipeline (URP) 14.0.11
