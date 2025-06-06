# unity-neuroguide-manager
Unity3D package used to read data and respond to events from the NeuroGuide hardware.

------------------------------

**Assembly:**\
com.gambit.neuroguide

**Namespace:**\
gambit.neuroguide

**ASMDEF File:**\
gambit.neuroguide

**Scripting Define Symbol:**\
GAMBIT_NEUROGUIDE

------------------------------
INSTALLATION INSTRUCTIONS
------------------------------
- Open your unity package manager manifest (YourProject/Packages/manifest.json)

- Add a new entry...\
  "com.gambit.neuroguide": "https://github.com/GambitGamesLLC/unity-neuroguide-manager.git?path=Assets/Plugins/Package",

- If you want to keep up to date with this repo, then you're done.
- If you want a specific version, add #v1.0.0 to the end of the URL (replace with the released version you want)

- Check the [Unity manual](https://docs.unity3d.com/Manual/upm-git.html#subfolder) on installing plugins from the subfolder of a Git repo for more info.

NOTES</br>
- You will need to manually add the ```GAMBIT_TWEEN``` scripting define symbol to your project to utilize the debug keyboard input feature, which tweens our debug values when the 'Up' arrow key is held while enableDebugKeyboardValues is set to 'true'.

------------------------------
DEPENDENCIES
------------------------------
unity-tween-manager [[Repo](https://github.com/GambitGamesLLC/unity-tween-manager)]</br>
Copy of the DoTween plugin wrapped into a package.

unity-static-coroutine [[Repo](https://github.com/GambitGamesLLC/unity-static-coroutine)]</br>
Adds functionality to use coroutines from non-monobehaviour derived classes


/// </summary>
/// <param name="webUISystem"></param>
//-----------------------------------------------------//
public static void Destroy( WebUISystem webUISystem )
//-----------------------------------------------------//
```
