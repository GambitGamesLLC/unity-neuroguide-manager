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

#if UNITY_INPUT
using UnityEngine.InputSystem;
#endif

using System;
using UnityEngine;
using System.Threading.Tasks;

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

#if EXT_DOTWEEN
#if UNITY_INPUT
            HandleDebugInput();
#else
            HandleLegacyDebugInput();
#endif

#endif

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
            /// Should we enable debug data on initialization, and then allow for keyboard input to debug input values? Press 'Up' on the keyboard to send a stream of reward=true values
            /// </summary>
            public bool enableDebugData = false;

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
            /// Unity action to call when the NeuroGuide data has been updated
            /// </summary>
            public Action<NeuroGuideData> OnDataUpdate;

            /// <summary>
            /// Unity action to call when the hardware state has changed
            /// </summary>
            public Action<State> OnStateUpdate;

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
            Action<NeuroGuideData> OnDataUpdated = null,
            Action<State> OnStateUpdated = null)
        //-------------------------------------//
        {
            if( system != null )
            {
                OnFailed?.Invoke( "NeuroGuideManager.cs Create() NeuroGuideSystem object already exists. Unable to continue." );
                return;
            }

            //If the user didn't pass in any options, use the defaults
            if( options == null ) options = new Options();

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

            //Access a variable of the singleton instance, this will ensure it is initialized in the hierarchy with a GameObject representation
            //Doing this makes sure that Unity Lifecycle methods like Update() will run
            Instance.enabled = true;

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
        /// Starts a UDP connection to connect to a port
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

        } //END InitializeData

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

            NeuroGuideData data = ScriptableObject.CreateInstance<NeuroGuideData>();

            // --- UP ARROW PRESSED ---
            if(Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                if(system.state != State.ReceivingData)
                {
                    system.state = State.ReceivingData;
                    SendStateUpdatedMessage();
                }

                Debug.Log( "Up pressed this frame" );
                data.isRecievingReward = true;
                SendDataUpdatedMessage( data );
            }

            // --- UP ARROW RELEASED ---
            else if(Keyboard.current.upArrowKey.wasReleasedThisFrame)
            {
                if(system.state != State.NoData)
                {
                    system.state = State.NoData;
                    SendStateUpdatedMessage();
                }
            }

            // --- DOWN ARROW PRESSED ---
            else if(Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                if(system.state != State.ReceivingData)
                {
                    system.state = State.ReceivingData;
                    SendStateUpdatedMessage();
                }

                data.isRecievingReward = false;
                SendDataUpdatedMessage( data );
            }

            // --- DOWN ARROW RELEASED ---
            else if(Keyboard.current.downArrowKey.wasReleasedThisFrame)
            {
                if(system.state != State.NoData)
                {
                    system.state = State.NoData;
                    SendStateUpdatedMessage();
                }
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

                data.isRecievingReward = true;
                SendDataUpdatedMessage( data );
            }
            else if( Input.GetKeyDown( KeyCode.DownArrow ) )
            {
                if(system.state != State.ReceivingData)
                {
                    system.state = State.ReceivingData;
                    SendStateUpdatedMessage();
                }

                data.isRecievingReward = false;
                SendDataUpdatedMessage( data );
            }
#endif

        } //END HandleLegacyDebugInput Method

        #endregion

        #region PRIVATE - SEND DATA UPDATED MESSAGE

        /// <summary>
        /// Sends a message out to any listeners via the Unity Action<> system
        /// </summary>
        //---------------------------------------------------//
        private static void SendDataUpdatedMessage( NeuroGuideData data )
        //---------------------------------------------------//
        {
            if(system == null )
            {
                return;
            }

            system.OnDataUpdate?.Invoke( data );

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

            //Debug.Log( "NeuroGuideManager.cs SendStateUpdatedMessage() state = " + system.state.ToString() );
            system.OnStateUpdate?.Invoke(system.state);

        } //END SendStateUpdatedMessage Method

        #endregion

    } //END NeuroGuideManager Class

} //END gambit.neuroguide Namespace