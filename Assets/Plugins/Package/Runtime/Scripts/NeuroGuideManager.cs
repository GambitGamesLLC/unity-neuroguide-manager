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
using System.Net;
using System.Net.Sockets;
using System.Threading;
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

        /// <summary>
        /// Thread that keeps a lookout for new UDP data incoming to our selected port
        /// </summary>
        private static Thread udpReceiveThread;

        /// <summary>
        /// The UDP client used to listen for UDP message on a seperate thread
        /// </summary>
        private static UdpClient udpClient;

        /// <summary>
        /// Is the UDP thread running?
        /// </summary>
        private static bool isThreadRunning = false;

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

            /// <summary>
            /// The address we want to listen to for UDP messages from the NeuroGuide hardware
            /// We utilize the home address, so we're looking for local messages from this PC
            /// </summary>
            public string udpAddress = "127.0.0.1";

            /// <summary>
            /// UDP Port to listen for NeuroGuide updates on
            /// </summary>
            public int udpPort = 50000;

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
        public static void Create(
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

            //Start listening to the UDP port for data from the NeuroGuide hardware
            StartUDPListener();

            //Mark the system as initialized, so Update methods know we're ready
            system.state = State.Initialized;
            SendStateUpdatedMessage();

            //If we were unable to make a connection to the NeuroGuide hardware, we cannot continue
            if(system.state == State.NotInitialized)
            {
                OnFailed?.Invoke( "NeuroGuideManager.cs Create() Unable to connect to NeuroGuide hardware. Unable to continue." );
                return;
            }

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

            StopUDPListener();

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

        #region PRIVATE - START UDP LISTENER

        /// <summary>
        /// Starts a UDP listener at a specific port
        /// </summary>
        //----------------------------------------------//
        private static void StartUDPListener()
        //----------------------------------------------//
        {
            //Start a new thread outside of our main thread
            udpReceiveThread = new Thread( new ThreadStart( ReceiveUDPData ) );
            udpReceiveThread.IsBackground = true;
            udpReceiveThread.Start();
            isThreadRunning = true;

        } //END StartUDPListener Method

        #endregion

        #region PRIVATE - STOP UDP LISTENER

        /// <summary>
        /// Stops the UDP thread and client
        /// </summary>
        //----------------------------------------------//
        private static void StopUDPListener()
        //----------------------------------------------//
        {
            isThreadRunning = false;

            if(udpReceiveThread != null && udpReceiveThread.IsAlive)
            {
                udpReceiveThread.Abort();
            }
            if(udpClient != null)
            {
                udpClient.Close();
            }
        } //END StopUDPListener Method

        #endregion

        #region PRIVATE - RECIEVE UDP DATA
        
        /// <summary>
        /// Whenever the UDP thread recieves data, check what the reward state of the user is and let our listeners know
        /// </summary>
        //-------------------------------//
        private static void ReceiveUDPData()
        //-------------------------------//
        {
            if(system == null)
                return;

            udpClient = new UdpClient( system.options.udpPort );

            while(isThreadRunning)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint( IPAddress.Parse( system.options.udpAddress ), system.options.udpPort );
                    byte[ ] data = udpClient.Receive( ref anyIP );

                    if(data.Length > 0)
                    {
                        bool rewardState = false;

                        //1 = true, 0 = false
                        if(data[ 0 ] == 1)
                        {
                            rewardState = true;
                        }

                        NeuroGuideData neuroGuideData = new NeuroGuideData
                        {
                            isRecievingReward = rewardState,
                            timestamp = System.DateTime.Now
                        };

                        SendDataUpdatedMessage( neuroGuideData );
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError( "Error receiving UDP data: " + e.Message );
                }
            }

        } //END ReceiveUDPData Method

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
            NeuroGuideData data = ScriptableObject.CreateInstance<NeuroGuideData>();

            // --- UP ARROW PRESSED ---
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

            // --- UP ARROW RELEASED ---
            else if(Input.GetKeyUp( KeyCode.UpArrow ))
            {
                if(system.state != State.NoData)
                {
                    system.state = State.NoData;
                    SendStateUpdatedMessage();
                }
            }

            // --- DOWN ARROW PRESSED ---
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

            // --- DOWN ARROW RELEASED ---
            else if(Input.GetKeyUp( KeyCode.DownArrow ))
            {
                if(system.state != State.NoData)
                {
                    system.state = State.NoData;
                    SendStateUpdatedMessage();
                }
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