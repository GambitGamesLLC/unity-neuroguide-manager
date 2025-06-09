# unity-neuroguide-manager  
Handles connectivity and data interaction with the NeuroGear neuromodulation headset for use in Unity3D desktop applications.

**Assembly:**\
com.gambit.neuroguide

**Namespace:**\
gambit.neuroguide

**ASMDEF File:**\
gambit.neuroguide

**Scripting Define Symbol:**\
GAMBIT_NEUROGUIDE

------------------------------
USAGE INSTRUCTIONS
------------------------------

**NeuroGuideManager.cs**\
Singleton component for accessing and controlling the NeuroGear headset from Unity applications. This class will listen for data streams and provide normalized brain-state metrics for integration into cognitive training, wellness apps, and game-based experiences.

### ▶ Initialization:
```csharp
NeuroGuideManager.Instance.Create(new NeuroGuideManager.Options
{
    showDebugLogs = true
});
```

### 🔧 Public Options:
- `showDebugLogs`: Enable or disable Unity console logs for internal state
- Future options may include mock mode, simulated metrics, and data stream filters

------------------------------
INSTALLATION INSTRUCTIONS
------------------------------

- Open your Unity package manager manifest file (`YourProject/Packages/manifest.json`)

- Add a new entry like so:
```
"com.gambit.neuroguide": "https://github.com/GambitGamesLLC/unity-neuroguide-manager.git?path=Assets/Plugins/Package",
```

- For a specific version tag:
```
"com.gambit.neuroguide": "https://github.com/GambitGamesLLC/unity-neuroguide-manager.git?path=Assets/Plugins/Package#v1.0.0"
```

- Reference: [Unity Docs – Git Subfolder UPM](https://docs.unity3d.com/Manual/upm-git.html#subfolder)

------------------------------
RECOMMENDED PACKAGES
------------------------------

**Gambit Singleton** [[Repo]](https://github.com/GambitGamesLLC/unity-singleton)\
Used as the base pattern for `NeuroGuideManager.Instance`

**Static Coroutine Utility** [[Repo]](https://github.com/GambitGamesLLC/unity-static-coroutine)\
Allows coroutines to run from static contexts like the Singleton interface

**DOTween Plugin** [[Asset Store]](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676) [[Gambit Repo]](https://github.com/GambitGamesLLC/unity-plugin-dotween)\
Used for animating debug/test data streams when simulating headset input

**Gambit Math Helper** [[Repo]](https://github.com/GambitGamesLLC/unity-math-helper)\
Supports mapping EEG values from device range to normalized in-app range

------------------------------
SUPPORT
------------------------------
Created and maintained by **Gambit Games LLC**\
For support or feature requests, contact: **gambitgamesllc@gmail.com**
