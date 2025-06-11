# unity-neuroguide-manager

Handles connectivity and data interaction with the NeuroGuide neuromodulation headset for use in Unity3D desktop applications.

**Package Name:** com.gambit.neuroguide  
**GameObject Display Name:** gambit.neuroguide.NeuroGuideManager (Singleton)  
**Namespace:** gambit.neuroguide  
**Assembly Definition:** gambit.neuroguide  
**Scripting Define Symbol:** GAMBIT_NEUROGUIDE

-----

## DEMO INSTRUCTIONS
A quick demo scene to see how the NeuroGuide hardware and our manager works.  
Spawns cubes based on debug values set in the demo scene that follow the NeuroGear hardware values as they are read.  
If no NeuroGuide is present, you can control the cubes using the Keyboard arrow keys.  

- Open the Unity Project in the editor on a PC with a keyboard attached.
- Open Assets/Demos/Demo.unity scene
- Look at the Demo GameObject and its values in the scene inspector, make sure the `debug` variable is enabled if you want to test without the NeuroGuide hardware
- Play the scene

## DEMO CONTROLS
- `Enter Key` - Creates a NeuroGuideSystem and spawns cubes to match the data
- `Delete Key` - Destroys the NeuroGuideSystem and any spawned cubes, afterwards the `Enter` key can be pressed again to spawn a new NeuroGuideSystem
- `Up Key` - Tweens the debug value of each NeuroGuideData node to the max value, set by the public value in the NeuroGuideDemo component on the Demo GameObject
- `Down Key` - Tweens the debug value of each NeuroGuideData node to the minimum value, set by the public value in the NeuroGuideDemo component on the Demo GameObject

-----

## INSTALLATION INSTRUCTIONS

### Method 1: Unity Package Manager (via Git URL)

This is the recommended installation method.

1.  In your Unity project, open the **Package Manager** (`Window > Package Manager`).
2.  Click the **'+'** button in the top-left corner and select **"Add package from git URL..."**
3.  Enter the following URL:
    ```
    https://github.com/GambitGamesLLC/unity-neuroguide-manager.git?path=Assets/Plugins/Package
    ```
4.  To install a specific version, append the version tag to the URL:
    ```
    https://github.com/GambitGamesLLC/unity-neuroguide-manager.git?path=Assets/Plugins/Package#v1.0.0
    ```

**Alternatively, you can manually edit your project's `Packages/manifest.json` file:**

```json
{
  "dependencies": {
    "com.gambit.neuroguide": "https://github.com/GambitGamesLLC/unity-neuroguide-manager.git?path=Assets/Plugins/Package",
    ...
  }
}
```

### Method 2: Local Installation

1.  Download or clone this repository to your computer.
2.  In your Unity project, open the **Package Manager** (`Window > Package Manager`).
3.  Click the **'+'** button in the top-left corner and select **"Add package from disk..."**
4.  Navigate to the cloned repository folder and select the `package.json` file inside `Assets/Plugins/Package`.

-----

## USAGE INSTRUCTIONS

The primary class for this package is **`NeuroGuideManager.cs`**. It's a singleton component used for accessing and controlling the NeuroGuide headset. This class listens for data streams and provides normalized brain-state metrics.

### ▶ Initialization & Usage

To begin interacting with the headset, you must first initialize the manager.

```csharp
using gambit.neuroguide;
using UnityEngine;

public class NeuroGuideExample : MonoBehaviour
{
    void Start()
    {
        // Options for customizing the manager's behavior
        var options = new NeuroGuideManager.Options
        {
            showDebugLogs = true,
            enableDebugData = true,
            debugNumberOfEntries = 10
        };

        // Create and initialize the NeuroGuide manager
        NeuroGuideManager.Create(
            options,
            // OnSuccess: Called when the manager initializes successfully
            (system) => {
                Debug.Log("NeuroGuideManager created successfully! System data count: " + system.data.Count);
            },
            // OnFailed: Called if initialization fails
            (error) => {
                Debug.LogWarning("NeuroGuideManager failed to create: " + error);
            },
            // OnDataUpdated: Called every time new data is received from the headset
            (system) => {
                // Access real-time data from the headset
                foreach (var sensorData in system.data)
                {
                    Debug.Log($"Sensor {sensorData.sensorID}: Value = {sensorData.currentValue}");
                }
            },
            // OnStateUpdated: Called when the headset's connection state changes
            (system, state) => {
                Debug.Log("NeuroGuideManager state changed to: " + state);
            }
        );
    }

    void OnDestroy()
    {
        // Clean up the manager when you're done
        NeuroGuideManager.Destroy();
    }
}
```

### 🔧 Public Options

You can customize the manager's behavior by passing an `Options` object during creation.

  * `showDebugLogs`: (bool) Enables or disables internal state logs in the Unity console.
  * `enableDebugData`: (bool) Enables simulated data for testing without a headset. Allows keyboard input (Up/Down arrows) to control debug values.
  * `debugNumberOfEntries`: (int) If debug data is enabled, this is the number of randomized data nodes to generate.
  * `debugRandomizeStartingValues`: (bool) If enabled, randomizes the start value between the 'min' and 'max' value, if disabled the starting value is 0.
  * `debugMinCurrentValue`: (float) The minimum value for debug data tweens.
  * `debugMaxCurrentValue`: (float) The maximum value for debug data tweens.
  * `debugTweenDuration`: (float) The duration (in seconds) for the debug value tweens.
  * `debugEaseType`: (DG.Tweening.Ease) The DOTween ease type to use for debug animations (requires DOTween).
    
-----

## DEPENDENCIES

This package relies on other open-source packages to function correctly.

  * **Gambit Singleton** (Optional) [[Gambit Repo]](https://github.com/GambitGamesLLC/unity-singleton)  
    If 'GAMBIT_SINGLETON' scripting define symbol is present. Used as the base pattern for `NeuroGuideManager.Instance`. If not included we utilize a built in alternative, but we recommend this package in any project using Singletons to keep this common code to one location.

  * **Unity Input** (Optional) [[Docs]](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/index.html)  
    If 'UNITY_INPUT' scripting define symbol is present. Uses the new input system for simulating hardware via keyboard input when the appropriate debug option is enabled during Create(). If the 'UNITY_INPUT' scripting define symbol is not set, we will use the Legacy Input System.

  * **DOTween Plugin** (Optional) [[Asset Store]](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676) [[Gambit Repo]](https://github.com/GambitGamesLLC/unity-plugin-dotween)  
    If 'EXT_DOTWEEN' scripting define symbol is present. We use DOTween for animating debug data streams when simulating headset input. Set the `enableDebugData` Option variable to true and use the keyboard to set the simulated hardware values.

-----

## SUPPORT

Created and maintained by **Gambit Games LLC**  
For support or feature requests, contact: **gambitgamesllc@gmail.com**
