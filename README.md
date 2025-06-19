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
- `Enter Key` - Creates a NeuroGuideSystem and spawns cubes to match the data. When you hit 'Play' we automatically create one for you
- `Delete Key` - Destroys the NeuroGuideSystem and NeuroGuideExperiencesystem, afterwards the `Enter` key can be pressed again to spawn a new NeuroGuideSystem and NeuroGuideExperienceSystem
- `Up Key` - Simulates the user entering a "reward" state. The cube in the scene will begin to scale up.
- `Down Key` - Simulates the user leaving the "reward" state. The cube in the scene will begin to scale down.

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

USAGE INSTRUCTIONS
The package is built around a two-layer system:

1.  NeuroGuideManager: A low-level singleton that connects directly to the NeuroGuide hardware and reports its state and whether the user is receiving a reward.  
2.  NeuroGuideExperience: A higher-level singleton that uses the data from the NeuroGuideManager to track progress over a defined period. It accumulates progress when the user is in a "reward" state and decreases it when they are not. It then provides a normalized value (0-1) of this progress to any interactable objects.

▶ Initialization & Usage
Step 1: Initialize the Hardware Manager (NeuroGuideManager)
First, initialize the NeuroGuideManager to connect to the hardware.

```
#if GAMBIT_NEUROGUIDE
using gambit.neuroguide;
#endif

using UnityEngine;

public class MyNeuroGuideController : MonoBehaviour
{
    void Start()
    {
        // 1. Initialize the NeuroGuideManager, to start recieving hardware events
        NeuroGuideManager.Create(
            new NeuroGuideManager.Options()
            {
                showDebugLogs = true,
                enableDebugData = true // Use keyboard simulation if no hardware is present
            },
            // OnSuccess
            (system) => {
                Debug.Log("NeuroGuideManager created successfully!");
                // 2. Once the manager is ready, create the experience
                CreateExperience();
            },
            // OnFailed
            (error) => {
                Debug.LogWarning("NeuroGuideManager failed to create: " + error);
            },
            // OnDataUpdate: Called when new data is received from the hardware
            (data) => {
                // This callback receives NeuroGuideData
                Debug.Log("Is user in reward state? " + data.isRecievingReward);
            },
            // OnStateUpdate: Called when the connection state changes
            (state) => {
                Debug.Log("NeuroGuideManager state changed to: " + state);
            }
        );
    }
    
    // ... See next steps
```

Step 2: Create the Experience Tracker (NeuroGuideExperience)
After the NeuroGuideManager is successfully created, you can create a NeuroGuideExperience to track progress.

```
// ... Continued from previous example

    private void CreateExperience()
    {
        NeuroGuideExperience.Create(
            new NeuroGuideExperience.Options()
            {
                showDebugLogs = true,
                totalDurationInSeconds = 120f // The time in seconds to reach 100% progress
            },
            // OnSuccess
            (system) => {
                Debug.Log("NeuroGuideExperience created successfully!");
            },
            // OnError
            (error) => {
                Debug.LogWarning("NeuroGuideExperience failed to create: " + error);
            }
        );
    }
```

Step 3: Make Your GameObjects Interactable
Implement the INeuroGuideInteractable interface on any MonoBehaviour to make it react to experience updates. The OnDataUpdate method will be called automatically with the normalized (0-1) progress value from the NeuroGuideExperience.

```
#if GAMBIT_NEUROGUIDE
using gambit.neuroguide;
#endif

using UnityEngine;

public class ResponsiveCube : MonoBehaviour, INeuroGuideInteractable
{
    // This method is called by NeuroGuideExperience whenever the progress value changes.
    public void OnDataUpdate(float normalizedValue)
    {
        // Scale the cube based on the user's progress
        Debug.Log("Experience progress: " + normalizedValue);
        transform.localScale = Vector3.one * normalizedValue;
    }
}
```

Step 4: Cleanup
Always destroy the managers when you are done to clean up listeners and resources.

```
// ... Continued from previous example

    void OnDestroy()
    {
        // Destroy in reverse order of creation
        NeuroGuideExperience.Destroy();
        NeuroGuideManager.Destroy();
    }
}
```

🔧 Public Options
NeuroGuideManager.Options
- showDebugLogs (bool): Enables or disables internal state logs in the Unity console.
- enableDebugData (bool): Enables simulated data for testing without a headset. Allows keyboard input (Up/Down arrows) to simulate the reward state.
  
NeuroGuideExperience.Options
- showDebugLogs (bool): Enables or disables experience-related logs.
- totalDurationInSeconds (float): The total amount of time the user must be in the reward state to reach 100% progress (a normalized value of 1.0). Progress decreases when not in the reward state.

-----

## DEPENDENCIES

This package relies on other open-source packages to function correctly.

  * **Gambit Singleton** (Optional) [[Gambit Repo]](https://github.com/GambitGamesLLC/unity-singleton)  
    If 'GAMBIT_SINGLETON' scripting define symbol is present. Used as the base pattern for `NeuroGuideManager.Instance`. If not included we utilize a built in alternative, but we recommend this package in any project using Singletons to keep this common code to one location.

  * **Gambit Math Helper** (Optional) [[Gambit Repo]](https://github.com/GambitGamesLLC/unity-math-helper)  
    If 'GAMBIT_MATHHELPER' scripting define symbol is present. Used to perform Map() operations to convert a value into a normalized 0-1 value within a specified range.

  * **Unity Input** (Optional) [[Docs]](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/index.html)  
    If 'UNITY_INPUT' scripting define symbol is present. Uses the new input system for simulating hardware via keyboard input when the `enableDebugData` option is enabled during Create(). If the 'UNITY_INPUT' scripting define symbol is not set, we will use the Legacy Input System.

  * **DOTween Plugin** (Optional) [[Asset Store]](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676) [[Gambit Repo]](https://github.com/GambitGamesLLC/unity-plugin-dotween)  
    If 'EXT_DOTWEEN' scripting define symbol is present. We use DOTween for animating debug data streams when simulating headset input. Set the `enableDebugData` Option variable to true and use the keyboard to set the simulated hardware values.

-----

## SUPPORT

Created and maintained by **Gambit Games LLC**  
For support or feature requests, contact: **gambitgamesllc@gmail.com**
