# unity-neuroguide-manager

Handles connectivity and data interaction with the NeuroGear neuromodulation headset for use in Unity3D desktop applications.

**Package Name:** com.gambit.neuroguide  
**GameObject Display Name:** gambit.neuroguide.NeuroGuideManager (Singleton)
**Namespace:** gambit.neuroguide  
**Assembly Definition:** gambit.neuroguide  
**Scripting Define Symbol:** GAMBIT_NEUROGUIDE

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

The primary class for this package is **`NeuroGuideManager.cs`**. It's a singleton component used for accessing and controlling the NeuroGear headset. This class listens for data streams and provides normalized brain-state metrics.

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
  * `debugMinCurrentValue`: (float) The minimum value for debug data tweens.
  * `debugMaxCurrentValue`: (float) The maximum value for debug data tweens.
  * `debugTweenDuration`: (float) The duration (in seconds) for the debug value tweens.
  * `debugEaseType`: (DG.Tweening.Ease) The DOTween ease type to use for debug animations (requires DOTween).

-----

## DEPENDENCIES

This package relies on other open-source packages to function correctly.

  * **Gambit Singleton** [[Repo]](https://github.com/GambitGamesLLC/unity-singleton)  
    Used as the base pattern for `NeuroGuideManager.Instance`.

  * **Unity Input** [[Docs]](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/index.html)  
    Utilizes the new and legacy input systems of Unity to simulate input using the arrow keys.

  * **DOTween Plugin** (Optional) [[Asset Store]](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676) [[Gambit Repo]](https://github.com/GambitGamesLLC/unity-plugin-dotween)  
    Used for animating debug data streams when simulating headset input.

-----

## SUPPORT

Created and maintained by **Gambit Games LLC**  
For support or feature requests, contact: **gambitgamesllc@gmail.com**