/********************************************************
 * NeuroGuideManager.cs
 * 
 * Listens to and responds to events from the NeuroGuide hardware
 * 
 ********************************************************/


#region IMPORTS

#if GAMBIT_SINGLETON
using gambit.singleton;
#else
/// <summary>
/// Fallback Singleton base class if GAMBIT_SINGLETON is not defined.
/// </summary>
/// <typeparam name="T">Type of the MonoBehaviour singleton.</typeparam>
public class Singleton<T>: MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    /// <summary>
    /// Gets the singleton instance, creating it if necessary.
    /// </summary>
    //---------------------------------------------//
    public static T Instance
    //---------------------------------------------//
    {
        get
        {
            if(instance == null)
            {
                instance = new GameObject( typeof( T ).Name ).AddComponent<T>();
                GameObject.DontDestroyOnLoad( instance.gameObject );
            }
            return instance;
        }
    }

} //END Singleton<T> class
#endif

#if GAMBIT_STATICCOROUTINE
using gambit.staticcoroutine;
#endif

#if GAMBIT_MATHHELPER
using gambit.mathhelper;
#endif

#if EXT_DOTWEEN
using DG.Tweening;
#endif


using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

#if UNITY_INPUT
using UnityEngine.InputSystem;
#endif

#endregion

namespace gambit.neuroguide
{

    /// <summary>
    /// Singleton Manager for interacting with the NeuroGuide hardware
    /// </summary>
    public class NeuroGuideManager : Singleton<NeuroGuideManager>
    {
        #region PUBLIC - VARIABLES

        /// <summary>
        /// Current instance of the NeuroGuideSystem instantiated during Create()
        /// </summary>
        public static NeuroGuideSystem system;

        #endregion

        #region PUBLIC - CREATION OPTIONS

        /// <summary>
        /// Options object you can pass in to customize the spawned NeuroGuide system
        /// </summary>
        //---------------------------------------------//
        public class Options
        //---------------------------------------------//
        {
            /// <summary>
            /// Should debug logs be printed to the console log?
            /// </summary>
            public bool showDebugLogs = true;

            /// <summary>
            /// Should we enable debug data on initialization, and then allow for keyboard input to debug input values? Press 'Up' on the keyboard to raise the values "Read" by the debug NeuroGear Device
            /// </summary>
            public bool enableDebugData = false;

            /// <summary>
            /// If 'enableDebugData' is true, we will generate a number of randomized data nodes based on this value
            /// </summary>
            public int debugNumberOfEntries = 100;

            /// <summary>
            /// If 'enableDebugData' is true, we will generate a number of randomized data nodes based on this value
            /// </summary>
            public int debugMinRawDataPoints = 5;

            /// <summary>
            /// If 'enableDebugData' is true, we will generate a number of randomized data nodes based on this value
            /// </summary>
            public int debugMaxRawDataPoints = 15;

            /// <summary>
            /// If 'enableDebugData' is true, we will generate a number of randomized data nodes based on this value
            /// </summary>
            public float debugMinRawDataValue = -100.0f;

            /// <summary>
            /// If 'enableDebugData' is true, we will generate a number of randomized data nodes based on this value
            /// </summary>
            public float debugMaxRawDataValue = 100.0f;

            /// <summary>
            /// If 'enableDebugData' is true, we will generate a number of randomized data nodes based on this value
            /// </summary>
            public float debugMinCurrentValue = 0.0f;

            /// <summary>
            /// If 'enableDebugData' is true, we will generate a number of randomized data nodes based on this value
            /// </summary>
            public float debugMaxCurrentValue = 50.0f;

            /// <summary>
            /// How long should it take for our debug keyboard input tweens to reach the max or min value?
            /// </summary>
            public float debugTweenDuration = 20f;

#if EXT_DOTWEEN
            /// <summary>
            /// What is the tweening type our debug functionality should use to change values over time?
            /// </summary>
            public DG.Tweening.Ease debugEaseType = Ease.InOutExpo;
#endif

        } //END Options

        #endregion

        #region PUBLIC - ENUM - STATE

        /// <summary>
        /// The state enum of the NeuroGuide hardware
        /// </summary>
        public enum State
        {
            NotInitialized,
            Initialized,
            NoData,
            ReceivingData
        }

        #endregion

        #region PUBLIC - RETURN CLASS : NEUROGUIDE SYSTEM

        /// <summary>
        /// NeuroGuide System generated when Create() is successfully called. Contains values important for future modification and communication with the NeuroGuide Manager
        /// </summary>
        //-----------------------------------------//
        public class NeuroGuideSystem
        //-----------------------------------------//
        {
            /// <summary>
            /// The options passed in during Create()
            /// </summary>
            public Options options = new Options();

            /// <summary>
            /// The current state of the NeuroGuide hardware
            /// </summary>
            public State state = State.NotInitialized;

            /// <summary>
            /// A collection of data read from the hardware, updated each cycle
            /// </summary>
            public List<NeuroGuideData> data = new List<NeuroGuideData>();

            // A single instance of System.Random for consistent random number generation.
            // This addresses the issue where rapid instantiation of System.Random
            // can lead to identical sequences if seeded by the system clock too quickly.
            public System.Random random = new System.Random();

        } //END NeuroGuideSystem
        #endregion

        #region PUBLIC - CREATE

        /// <summary>
        /// Starts listening to the NearoGuide hardware for device state and sends out updates as data changes
        /// </summary>
        /// <param name="options">Options object that determines how the NeuroGuide manager is initialized</param>
        /// <param name="OnSuccess">Callback action when the NeuroGuide system successfully initializes</param>
        /// <param name="OnFailed">Callback action that returns a string with an error message when initialization fails</param>
        //-------------------------------------//
        public static async void Create(
            Options options = null,
            Action<NeuroGuideSystem> OnSuccess = null,
            Action<string> OnFailed = null )
        //-------------------------------------//
        {
            if( system != null )
            {
                OnFailed?.Invoke( "NeuroGuideManager.cs Create() NeuroGuideSystem object already exists. Unable to continue." );
                return;
            }

#if !GAMBIT_STATIC_COROUTINE
            OnFailed?.Invoke("NeuroGuideManager.cs Create() missing 'GAMBIT_STATIC_COROUTINE' scripting define symbol or package. This is used to asynchronously connect to the NeuroGear hardware. Unable to continue.");
            return;
#endif

            //If the user didn't pass in any options, use the defaults
            if( options == null ) options = new Options();

            //If we are set to debug the NeuroGear hardware, make sure we have access to the tween system, as we use it to debug the values changing over time
#if !EXT_DOTWEEN
            if( options.enableDebugKeyboardInput )
            {
                OnFailed?.Invoke("NeuroGuideManager.cs Create() missing 'EXT_DOTWEEN' scripting define symbol or package. But we need to use tweens to debug our NeuroGear values using the keyboard input. Unable to continue.");
                return;
            }
#endif

            //Generate a NeuroGuideSystem object
            system = new NeuroGuideSystem();
            system.options = options;

            //Connect to the NeuroGuide hardware to make sure we can see it
#if GAMBIT_STATIC_COROUTINE
            await CheckConnection();
#endif

            //If we were unable to make a connection to the NeuroGear hardware, we cannot continue
            if(system.state == State.NotInitialized)
            {
                OnFailed?.Invoke( "NeuroGuideManager.cs Create() Unable to connect to NeuroGear hardware. Unable to continue." );
                return;
            }

            InitializeData();

            //We're done, call the OnSuccess callback
            OnSuccess?.Invoke(system);

        } //END Create Method

        #endregion

        #region PRIVATE - CHECK CONNECTION

        /// <summary>
        /// Performs a check of the NeuroGear hardware, if connected updates our State
        /// </summary>
        /// <returns></returns>
        //----------------------------------------------------//
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private static async Task CheckConnection()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        //----------------------------------------------------//
        {
            if( system == null )
            {
                Debug.LogError( "NeuroGuideManager.cs CheckConnection() passed in system object is null, unable to continue" );
                
            }

            //Since we don't know anything about the NeuroGear, let's just say we're connected
            system.state = State.Initialized;

        } //END CheckConnection Method

        #endregion

        #region PRIVATE - INITIALIZE DATA

        /// <summary>
        /// Initializes the NeuroGear hardware data nodes, creating a number of nodes based on how many nodes our hardware is reporting
        /// </summary>
        //---------------------------------------------------------------------//
        private static void InitializeData()
        //---------------------------------------------------------------------//
        {

            if( system == null )
            {
                Debug.LogError( "NeuroGuideManager.cs InitializeData() passed in system object is null, unable to continue" );
                return;
            }

            //If debug data is enabled, initialize a number of debug data nodes
            if(system.options.enableDebugData)
            {
                system.data = new List<NeuroGuideData>(system.options.debugNumberOfEntries);
                
                for( int i = 0; i < system.options.debugNumberOfEntries; i++ )
                {
                    GameObject go = new GameObject();
                    go.name = GenerateRandomSensorID( 8, system.random ); // Generate an 8-character ID

                    go.transform.parent = NeuroGuideManager.Instance.transform;

                    NeuroGuideData data = go.AddComponent<NeuroGuideData>();
                    data.sensorID = go.name;
                    data.rawData = GenerateRandomFloatList( system.random, system.options.debugMinRawDataPoints, system.options.debugMaxRawDataPoints, system.options.debugMinRawDataValue, system.options.debugMaxRawDataValue );
                    data.currentValue = GenerateRandomFloat( system.random, system.options.debugMinCurrentValue, system.options.debugMaxCurrentValue );
                    data.currentNormalizedValue = GenerateRandomFloat( system.random, 0.0f, 1.0f ); // Normalized value typically 0-1

                    system.data.Add( data );
                }
            }

        } //END InitializeData

        #endregion

        #region PRIVATE - GENERATE DEBUG DATA

        /// <summary>
        /// Helper function to generate a random string for sensorID
        /// </summary>
        /// <param name="length">How long the ID should be</param>
        /// <param name="random">Instance of System.Random</param>
        /// <returns></returns>
        //-----------------------------------------------------------------------------//
        private static string GenerateRandomSensorID( int length, System.Random random )
        //-----------------------------------------------------------------------------//
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder result = new StringBuilder( length );
            for(int i = 0; i < length; i++)
            {
                result.Append( chars[ random.Next( chars.Length ) ] );
            }
            return result.ToString();

        } //END GenerateRandomSensorID Method

        /// <summary>
        /// Helper function to generate a list of random floats for rawData
        /// </summary>
        /// <param name="random">Instance of System.Random</param>
        /// <param name="minCount"></param>
        /// <param name="maxCount"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        //-------------------------------------------------------------------//
        private static List<float> GenerateRandomFloatList( System.Random random, int minCount, int maxCount, float minValue, float maxValue )
        //-------------------------------------------------------------------//
        {
            int count = random.Next( minCount, maxCount + 1 ); // Next is exclusive for max
            List<float> floatList = new List<float>( count );
            for(int i = 0; i < count; i++)
            {
                floatList.Add( (float)(random.NextDouble() * (maxValue - minValue) + minValue) );
            }
            return floatList;

        } //END GenerateRandomFloatList Method

        /// <summary>
        /// Helper function to generate a single random float
        /// </summary>
        /// <param name="random">Instance of System.Random</param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        //---------------------------------------------------------------------------------------------//
        private static float GenerateRandomFloat( System.Random random, float minValue, float maxValue )
        //---------------------------------------------------------------------------------------------//
        {
            return (float)(random.NextDouble() * (maxValue - minValue) + minValue);

        } //END GenerateRandomFloat Method

        #endregion

        #region PUBLIC - AWAKE

        /// <summary>
        /// Unity lifecycle method
        /// </summary>
        //----------------------------------------//
        public void Start()
        //----------------------------------------//
        {
#if EXT_DOTWEEN
            DOTween.Init( true, false, LogBehaviour.Verbose );
#endif
        } //END Awake

#endregion

        #region PUBLIC - UPDATE

        /// <summary>
        /// The Unity update method
        /// </summary>
        //---------------------------//
        public void Update()
        //---------------------------//
        {
            
            if( system == null )
            {
                return;
            }

            if( !system.options.enableDebugData )
            {
                return;
            }

            if(system.state == State.NotInitialized)
            {
                return;
            }

            if(system.data == null || ( system.data != null && system.data.Count == 0 ) )
            {
                return;
            }

            HandleDebugInput();

        } //END Update Method

        #endregion

        #region PRIVATE - HANDLE DEBUG INPUT

        /// <summary>
        /// Handles keyboard input for debug tweening when debug mode is active.
        /// </summary>
        //---------------------------------------//
        public void HandleDebugInput()
        //---------------------------------------//
        {
            bool up_keyUp = false;
#if UNITY_INPUT
            up_keyUp = true;
#else
            up_keyUp = Input.GetKeyUp( KeyCode.UpArrow );
#endif
            if( up_keyUp )
            {
                for( int i = 0; i < system.data.Count; i++ )
                {
                    // Kill any existing tween on this specific data value to ensure smooth transition.
                    if(system.data[ i ].activeTween != null && system.data[ i ].activeTween.IsActive())
                    {
                        system.data[ i ].activeTween.Kill(false); // false: don't complete the tween, just stop it.
                    }

                    //Create the tween
                    //Gemini - Error occurs on the next line!
                    system.data[ i ].activeTween = DOTween.To(
                        getter: () => system.data[ i ].currentValue,
                        setter: ( x ) => system.data[ i ].currentValue = x,
                        endValue: system.options.debugMaxCurrentValue,
                        duration: system.options.debugTweenDuration )
                    .SetEase( system.options.debugEaseType )
                    .OnComplete( () => { system.data[ i ].activeTween = null; } );
                }
            }

        } //END HandleDebugInput Method

#endregion

    } //END NeuroGuideManager Class

} //END gambit.neuroguide Namespace