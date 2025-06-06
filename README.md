# unity-neuroguide-manager
Unity3D package used to read data and respond to events from the NeuroGuide hardware.

------------------------------

**Assembly:**\
com.asgs.neuroguide

**Namespace:**\
asgs.neuroguide

**ASMDEF File:**\
asgs.neuroguide

**Scripting Define Symbol:**\
ASGS_NEUROGUIDE

------------------------------
INSTALLATION INSTRUCTIONS
------------------------------
- Open your unity package manager manifest (YourProject/Packages/manifest.json)

- Add a new entry...\
  "com.gambit.webui": "https://github.com/GambitGamesLLC/unity-web-ui.git?path=Assets/Plugins/Package",

- If you want to keep up to date with this repo, then you're done.
- If you want a specific version, add #v1.0.0 to the end of the URL (replace with the released version you want)

- Check the [Unity manual](https://docs.unity3d.com/Manual/upm-git.html#subfolder) on installing plugins from the subfolder of a Git repo for more info.

NOTES</br>
- You will need to manually add the ```GAMBIT_VUPLEX``` scripting define symbol to your project to make proper use of the Vuplex plugin.

------------------------------
DEPENDENCIES
------------------------------
unity-web-messenger [[Repo](https://github.com/GambitGamesLLC/unity-web-messenger)]</br>
Provides communication logic on the JS side to receive and send commands between Unity's Vuplex package & Javascript

unity-static-coroutine [[Repo](https://github.com/GambitGamesLLC/unity-static-coroutine)]</br>
Adds functionality to use coroutines from non-monobehaviour derived classes

Vuplex [[Repo](https://github.com/GambitGamesLLC/unity-vuplex)][[Documentation](https://developer.vuplex.com/webview/overview)]</br>
Requires the Vuplex package as a dependency and the 'GAMBIT_VUPLEX' scripting define symbol in your project to operate.

------------------------------
USAGE EXAMPLE
------------------------------
For a full example of loading a GUI button in HTML and send & receiving commands is available within the ```Assets/Demos/Scripts/WebUIDemo.cs``` script</br>

For a quick example, check out the following pseudo code
```
using static gambit.webui.WebUIManager;

//Spawn a new web browser and load up a url, we recommend you host your user interface html on an Amazon AWS S3 bucket and link to the resource.
private string url = "https://www.youtube.com";

Create
(
    new Options(),
    (WebUISystem webUI)=>
    { 
        LoadURL(url, webUI );
    },
    (string error)=>{ Debug.Log( error ); }
);
```

------------------------------
CREATION OPTIONS
------------------------------
### `clickWithoutStealingFocus` : boolean
(defaults to true)

Should clicks take in focus away from other input?

### `disableVideo` : boolean
(defaults to false)

Should we disable video support to increase performance?

### `clickingEnabled` : boolean
(defaults to true)

Should click input be enabled for this UI?

### `dragMode` : DragMode
(defaults to DragMode.DragWithinPage)

How should we handle drag events? By default Desktop uses 'DragWithinPage' and mobile should use 'DragToScroll'

### `remoteDebuggingEnabled` : boolean
(defaults to true)

Should we allow connecting to the Vuplex web view remotely for debugging? https://support.vuplex.com/articles/how-to-debug-web-content

### `nativeOnScreenKeyboardEnabled` : boolean
(defaults to true)

Should we enable the native device keyboard on Android & iOS?

### `native2DModeEnabled` : boolean
(defaults to true)

Should we use the native web browser instead of the Vuplex Web View system on Android & iOS?

### `scrollSensitivity` : int
(defaults to 45)

Sets your scroll speed. Defaults to 45 for desktop.


-------------------------------
PUBLIC FUNCTIONS
-------------------------------
```
/// <summary>
/// Creates a UI by loading an HTML file with the platform's native web browser solution in fullscreen
/// </summary>
/// <param name="options">Customization options for your web browser based UI system</param>
/// <param name="OnSuccess">Success callback function that returns the WebUISystem object</param>
/// <param name="OnFailed">Failed callback function that returns a string error</param>
//------------------------------------------//
public static async void Create(
    Options options = null, 
    Action<WebUISystem> OnSuccess = null,
    Action<string> OnFailed = null )
//------------------------------------------//
```

```
/// <summary>
/// Clears the browser's cache
/// </summary>
//------------------------------------------------//
public static void ClearCache()
//------------------------------------------------//
```

```
/// <summary>
/// Takes in an already created WebUISystem and changes the loaded URL
/// </summary>
/// <param name="url">The URL you want to load</param>
/// <param name="webUISystem">The WebUISystem that was previously created</param>
/// <param name="OnStarted">Called when loading starts. Not available on WebGL</param>
/// <param name="OnProgress">Called when progress value updates. Not available on WebGL</param>
/// <param name="OnFinished">Called when loading finishes successfully</param>
/// <param name="OnFailed">Called when loading fails</param>
//-----------------------------------------------------------------//
public static void LoadURL( string url, 
    WebUISystem webUISystem, 
    Action OnStarted = null, 
    Action<float> OnProgress = null, 
    Action OnFinished = null,
    Action<string> OnFailed = null )
//-----------------------------------------------------------------//
```

```
/// <summary>
/// Sends a message to the WebUI system's Window object
/// </summary>
/// <param name="webUI">The WebUI system you want to send a message to</param>
/// <param name="key">The verification key sent alongside the message. If this unique identifier key doesn't match what we're expecting in the iframe's Javascript, then this message will not be called</param>
/// <param name="nameSpace">The window namespace that actually contains the method you want to call. EX: window.namespace.method(parameters)</param>
/// <param name="method">The method within the window.namespace you want to call. EX: window.namespace.method(parameters)</param>
/// <param name="parameters">The parameters string passed within the window.namespace.method(parameters)</param>
//------------------------------------------------------//
public static void PostMessage( WebUISystem webUI, string key, string nameSpace, string method, string parameters = "" )
//------------------------------------------------------//
```

```
/// <summary>
/// Starts recieving messages that are passed in with a matching unique identifier key. Messages that are successfully recieved will in turn call a method and pass in parameters for a gameobject using the SendMessage() functionality of Unity3D
/// </summary>
/// <param name="webUI">The WebUI system you wish to recieve messages from</param>
/// <param name="key">The unique identifier key you want to use to verify that a message should be recieved from the WebUI system</param>
//--------------------------------------------//
public static void RecieveMessages( WebUISystem webUI, string key )
//--------------------------------------------//
```

```
/// <summary>
/// Destroys the passed in WebUISystem
/// </summary>
/// <param name="webUISystem"></param>
//-----------------------------------------------------//
public static void Destroy( WebUISystem webUISystem )
//-----------------------------------------------------//
```
