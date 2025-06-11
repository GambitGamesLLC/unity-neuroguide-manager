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

#if EXT_DOTWEEN
using DG.Tweening;
#endif

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

        #region PRIVATE - VARIABLES

        /// <summary>
        /// Flag for if we've finished a debug keyboard tween, so the timer to change to the 'No Data' incoming state should be activated
        /// </summary>
        private static bool debugNoDataTimerActive = false;

        /// <summary>
        /// How long we want to wait after a debug keyboard tween completes before we switch states to simulate a lack of data being sent by the NeuroGuide hardware
        /// </summary>
        private static float debugNoDataTimerLength = 4f;

        /// <summary>
        /// How much time remains in the No Data Timer?
        /// </summary>
        private static float debugNoDataTimerCurrentValue = 0f;


        #endregion

        #region PUBLIC - START

        /// <summary>
        /// Unity lifecycle method
        /// </summary>
        //----------------------------------------//
        public void Start()
        //----------------------------------------//
        {
#if EXT_DOTWEEN
            DOTween.Init( false, false, LogBehaviour.Verbose );
#endif
        } //END Start

        #endregion

        #region PUBLIC - UPDATE

        /// <summary>
        /// The Unity update method
        /// </summary>
        //---------------------------//
        public void Update()
        //---------------------------//
        {

            if(system == null)
            {
                return;
            }

            if(!system.options.enableDebugData)
            {
                return;
            }

            if(system.state == State.NotInitialized)
            {
                return;
            }

            if(system.data == null || (system.data != null && system.data.Count == 0))
            {
                return;
            }

#if EXT_DOTWEEN
#if UNITY_INPUT
            HandleDebugInput();
#else
            HandleLegacyDebugInput();
#endif

#endif

            HandleDebugNoDataTimer();

        } //END Update Method

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
            /// Should we enable debug data on initialization, and then allow for keyboard input to debug input values? Press 'Up' on the keyboard to raise the values "Read" by the debug NeuroGuide Device
            /// </summary>
            public bool enableDebugData = false;

            /// <summary>
            /// If 'enableDebugData' is true, we will generate a number of randomized data nodes based on this value
            /// </summary>
            public int debugNumberOfEntries = 100;

            /// <summary>
            /// If 'enabledDebugData' is true, we check this flag to see if we should randomize the starting values of the NeuroGuideData, if disabled the NeuroGuideData values are zero at start
            /// </summary>
            public bool debugRandomizeStartingValues = true;

            /// <summary>
            /// If 'enableDebugData' is true, we will use this 'minimum' value as the lowest value we tween to when holding the 'Down' arrow key
            /// </summary>
            public float debugMinCurrentValue = 0.0f;

            /// <summary>
            /// If 'enableDebugData' is true, we will use this 'max' value as the highest value we tween to when holding the 'Up' arrow key
            /// </summary>
            public float debugMaxCurrentValue = 50.0f;

            /// <summary>
            /// How long should it take for our debug keyboard input tweens to reach the max or min value?
            /// </summary>
            public float debugTweenDuration = 5f;

#if EXT_DOTWEEN
            /// <summary>
            /// What is the tweening type our debug functionality should use to change values over time?
            /// </summary>
            public Ease debugEaseType = Ease.InOutExpo;
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

            /// <summary>
            /// Unity action to call when data has been updated
            /// </summary>
            public Action<NeuroGuideSystem> OnDataUpdate;

            /// <summary>
            /// Unity action to call when the hardware state has changed
            /// </summary>
            public Action<NeuroGuideSystem, State> OnStateUpdate;

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
            Action< string> OnFailed = null,
            Action<NeuroGuideSystem> OnDataUpdated = null,
            Action<NeuroGuideSystem, State> OnStateUpdated = null)
        //-------------------------------------//
        {
            if( system != null )
            {
                OnFailed?.Invoke( "NeuroGuideManager.cs Create() NeuroGuideSystem object already exists. Unable to continue." );
                return;
            }


            //If the user didn't pass in any options, use the defaults
            if( options == null ) options = new Options();

            //If we are set to debug the NeuroGuide hardware, make sure we have access to the tween system, as we use it to debug the values changing over time
#if !EXT_DOTWEEN
            if( options.enableDebugKeyboardInput )
            {
                OnFailed?.Invoke( "NeuroGuideManager.cs Create() missing 'EXT_DOTWEEN' scripting define symbol or package. But we need to use tweens to debug our NeuroGuide values using the keyboard input. Unable to continue.");
                return;
            }
#endif

            //Generate a NeuroGuideSystem object
            system = new NeuroGuideSystem();
            system.options = options;
            system.OnDataUpdate = OnDataUpdated;
            system.OnStateUpdate = OnStateUpdated;

            //Connect to the NeuroGuide hardware to make sure we can see it
            await CheckConnection();

            //If we were unable to make a connection to the NeuroGuide hardware, we cannot continue
            if(system.state == State.NotInitialized)
            {
                OnFailed?.Invoke( "NeuroGuideManager.cs Create() Unable to connect to NeuroGuide hardware. Unable to continue." );
                return;
            }

            InitializeData();

            //We're done, call the OnSuccess callback
            OnSuccess?.Invoke(system);

        } //END Create Method

        #endregion

        #region PUBLIC - DESTROY

        /// <summary>
        /// Stops listening to input and prepare the manager to have Create() called again
        /// </summary>
        //-------------------------------//
        public static void Destroy()
        //-------------------------------//
        {

            if(system == null)
            {
                return;
            }

            if(system.data == null || (system.data != null && system.data.Count == 0))
            {
                return;
            }

            for(int i = 0; i < system.data.Count; i++)
            {
#if EXT_DOTWEEN
                DOTween.Kill( system.data[ i ].gameObject );
#endif
            }

            KillNoDataTimer();

            Instance.Invoke( "FinishDestroy", .1f );

        } //END Destroy Method

        /// <summary>
        /// Invoked by Destroy(), after allowing for tweens to be cleaned up, destroys the gameobjects
        /// </summary>
        //------------------------------------//
        private void FinishDestroy()
        //------------------------------------//
        {
            if(system.options.showDebugLogs)
            {
                Debug.Log( "NeuroGuideManager.cs FinishDestroy() cleaned up objects and data, ready to Create()" );
            }

            if(system != null && system.data.Count > 0)
            {
                for(int i = 0; i < system.data.Count; i++)
                {
                    if(system.data[ i ].gameObject != null)
                    {
                        Destroy( system.data[ i ].gameObject );
                    }
                }
            }

            system = null;

        } //END FinishDestroy

        #endregion

        #region PRIVATE - CHECK CONNECTION

        /// <summary>
        /// Performs a check of the NeuroGuide hardware, if connected updates our State
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

            //Since we don't know anything about the NeuroGuide, let's just say we're connected
            system.state = State.Initialized;

            SendStateUpdatedMessage();

        } //END CheckConnection Method

        #endregion

        #region PRIVATE - INITIALIZE DATA

        /// <summary>
        /// Initializes the NeuroGuide hardware data nodes, creating a number of nodes based on how many nodes our hardware is reporting
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

                    if(system.options.debugRandomizeStartingValues)
                    {
                        data.currentValue = GenerateRandomFloat( system.random, system.options.debugMinCurrentValue, system.options.debugMaxCurrentValue );
                        data.currentNormalizedValue = GenerateRandomFloat( system.random, 0.0f, 1.0f ); // Normalized value typically 0-1
                    }
                    else
                    {
                        data.currentValue = 0f;
                        data.currentNormalizedValue = 0f;
                    }

#if EXT_DOTWEEN
                    // Store the original values for tweening back
                    data.originalValue = data.currentValue;
                    data.originalNormalizedValue = data.currentNormalizedValue;
#endif

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

        #region PRIVATE - HANDLE DEBUG INPUT

        /// <summary>
        /// Handles keyboard input for debug tweening when debug mode is active.
        /// </summary>
        //---------------------------------------//
        public void HandleDebugInput()
        //---------------------------------------//
        {

#if UNITY_INPUT
            
            // --- UP ARROW ---
            if(Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                if(system.state != State.ReceivingData)
                {
                    system.state = State.ReceivingData;
                    SendStateUpdatedMessage();
                }

                KillNoDataTimer();
                TweenAllValues( system.options.debugMaxCurrentValue, 1.0f );
            }

            if(Keyboard.current.upArrowKey.wasReleasedThisFrame)
            {
                if(system.state != State.ReceivingData)
                {
                    system.state = State.ReceivingData;
                    SendStateUpdatedMessage();
                }

                KillNoDataTimer();
                TweenAllValuesToOriginal();
            }

            // --- DOWN ARROW ---
            if(Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                if(system.state != State.ReceivingData)
                {
                    system.state = State.ReceivingData;
                    SendStateUpdatedMessage();
                }

                KillNoDataTimer();
                TweenAllValues( system.options.debugMinCurrentValue, 0.0f );
            }

            if(Keyboard.current.downArrowKey.wasReleasedThisFrame)
            {
                if(system.state != State.ReceivingData)
                {
                    system.state = State.ReceivingData;
                    SendStateUpdatedMessage();
                }

                KillNoDataTimer();
                TweenAllValuesToOriginal();
            }
#endif

        } //END HandleDebugInput Method

        #endregion

        #region PRIVATE - HANDLE DEBUG INPUT - LEGACY

        /// <summary>
        /// Handles keyboard input for debug tweening when debug mode is active.
        /// </summary>
        //---------------------------------------//
        public void HandleLegacyDebugInput()
        //---------------------------------------//
        {

#if !UNITY_INPUT
            // --- UP ARROW ---
            if(Input.GetKeyDown( KeyCode.UpArrow ))
            {
                if(system.state != State.ReceivingData)
                {
                    system.state = State.ReceivingData;
                    SendStateUpdatedMessage();
                }

                KillNoDataTimer();
                TweenAllValues( system.options.debugMaxCurrentValue, 1.0f );
            }

            if(Input.GetKeyUp( KeyCode.UpArrow ))
            {
                if(system.state != State.ReceivingData)
                {
                    system.state = State.ReceivingData;
                    SendStateUpdatedMessage();
                }

                KillNoDataTimer();
                TweenAllValuesToOriginal();
            }

            // --- DOWN ARROW ---
            if(Input.GetKeyDown( KeyCode.DownArrow ))
            {
                if(system.state != State.ReceivingData)
                {
                    system.state = State.ReceivingData;
                    SendStateUpdatedMessage();
                }

                KillNoDataTimer();
                TweenAllValues( system.options.debugMinCurrentValue, 0.0f );
            }

            if(Input.GetKeyUp( KeyCode.DownArrow ))
            {
                if(system.state != State.ReceivingData)
                {
                    system.state = State.ReceivingData;
                    SendStateUpdatedMessage();
                }

                KillNoDataTimer();
                TweenAllValuesToOriginal();
            }
#endif

        } //END HandleLegacyDebugInput Method

        #endregion

        #region PRIVATE - TWEEN VALUES

        /// <summary>
        /// Tweens all data values to a target value.
        /// </summary>
        //------------------------------------------------------------------------------------//
        private void TweenAllValues( float targetValue, float targetNormalizedValue )
        //------------------------------------------------------------------------------------//
        {
            for(int i = 0; i < system.data.Count; i++)
            {
                // Capture the index in a local variable to avoid closure issues.
                int index = i;

                // Kill any existing tween on this data item.
                if(system.data[ index ].activeTween != null && system.data[ index ].activeTween.IsActive())
                {
                    system.data[ index ].activeTween.Kill( false );
                }

                // Tween currentValue
                system.data[ index ].activeTween = DOTween.To(
                    () => system.data[ index ].currentValue,
                    x => { if(system != null && system.data != null && system.data.Count > 0) system.data[ index ].currentValue = x;  },
                    targetValue,
                    system.options.debugTweenDuration
                ).SetEase( system.options.debugEaseType )
                .OnUpdate( () => { if( index == 0 && system != null ) system.OnDataUpdate?.Invoke(system); } )
                .OnComplete( () => { if(system != null && system.data != null && system.data.Count > 0) system.data[ index ].activeTween = null; StartNoDataTimer(); } );

                // Tween currentNormalizedValue
                DOTween.To(
                    () => system.data[ index ].currentNormalizedValue,
                    x => { if(system != null && system.data != null && system.data.Count > 0) system.data[ index ].currentNormalizedValue = x; },
                    targetNormalizedValue,
                    system.options.debugTweenDuration
                ).SetEase( system.options.debugEaseType );
            }

        } //END TweenAllValues

        /// <summary>
        /// Tweens all data values back to their original state.
        /// </summary>
        //----------------------------------------------------------//
        private void TweenAllValuesToOriginal()
        //----------------------------------------------------------//
        {
            for(int i = 0; i < system.data.Count; i++)
            {
                int index = i;

                if(system.data[ index ].activeTween != null && system.data[ index ].activeTween.IsActive())
                {
                    system.data[ index ].activeTween.Kill( false );
                }

                // Tween back to originalValue
                system.data[ index ].activeTween = DOTween.To(
                    () => system.data[ index ].currentValue,
                    x => { if(system != null && system.data != null && system.data.Count > 0) system.data[ index ].currentValue = x; },
                    system.data[ index ].originalValue,
                    system.options.debugTweenDuration
                ).SetEase( system.options.debugEaseType )
                .OnUpdate( () => { if( index == 0 && system != null ) system.OnDataUpdate?.Invoke(system); } )
                .OnComplete( () => { if(system != null && system.data != null && system.data.Count > 0) system.data[ index ].activeTween = null; StartNoDataTimer(); } );

                // Tween back to originalNormalizedValue
                DOTween.To(
                    () => system.data[ index ].currentNormalizedValue,
                    x => { if(system != null && system.data != null && system.data.Count > 0) system.data[ index ].currentNormalizedValue = x; },
                    system.data[ index ].originalNormalizedValue,
                    system.options.debugTweenDuration
                ).SetEase( system.options.debugEaseType );
            }

        } //END TweenAllValuesToOriginal

        #endregion

        #region PRIVATE - SEND DATA UPDATED MESSAGE

        /// <summary>
        /// Sends a message out to any listeners via the Unity Action<> system
        /// </summary>
        //---------------------------------------------------//
        private static void SendDataUpdatedMessage()
        //---------------------------------------------------//
        {
            if(system == null)
            {
                return;
            }

            system.OnDataUpdate?.Invoke(system);

        } //END SendDataUpdatedMessage Method

        #endregion

        #region PRIVATE - SEND STATE CHANGED MESSAGE

        /// <summary>
        /// Sends a message out to any listeners via the Unity Action<> system
        /// </summary>
        //---------------------------------------------------//
        private static void SendStateUpdatedMessage()
        //---------------------------------------------------//
        {
            if(system == null)
            {
                return;
            }

            system.OnStateUpdate?.Invoke(system, system.state);

        } //END SendDataUpdatedMessage Method

        #endregion

        #region PRIVATE - NO DATA TIMER

        /// <summary>
        /// If a debug keyboard tween completes, set a timer to go off, and when it does change the state to simulate no data being recieved
        /// </summary>
        //-----------------------------------------------------//
        private static void HandleDebugNoDataTimer()
        //-----------------------------------------------------//
        {
            
            if(debugNoDataTimerActive)
            {
                debugNoDataTimerCurrentValue -= Time.deltaTime;
                //Debug.Log( "HandleDebugNoDataTimer() value = " + debugNoDataTimerCurrentValue );

                if(debugNoDataTimerCurrentValue <= 0f)
                {
                    FinishNoDataTimer();
                }
            }

        } //END HandleDebugNoDataTimer

        /// <summary>
        /// Creates a timer, that will go off in a few moments, when it does we will change the state
        /// </summary>
        //----------------------------------------//
        private static void StartNoDataTimer()
        //----------------------------------------//
        {
            debugNoDataTimerActive = true;
            debugNoDataTimerCurrentValue = debugNoDataTimerLength;

        } //END StartNoDataTimer Method

        /// <summary>
        /// Called when the timer hits 0
        /// </summary>
        //--------------------------------------------//
        private static void FinishNoDataTimer()
        //--------------------------------------------//
        {
            debugNoDataTimerActive = false;

            if(system.state != State.NoData)
            {
                system.state = State.NoData;
                SendStateUpdatedMessage();
            }

        } //END FinishNoDataTimer Method

        /// <summary>
        /// Kills any active "No Data' timer that's active
        /// </summary>
        //-------------------------------------------------//
        private static void KillNoDataTimer()
        //-------------------------------------------------//
        {
            debugNoDataTimerActive = false;

        } //END KillNoDataTimer

        #endregion

    } //END NeuroGuideManager Class

} //END gambit.neuroguide Namespace