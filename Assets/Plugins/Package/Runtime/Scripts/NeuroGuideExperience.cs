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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace gambit.neuroguide
{

    /// <summary>
    /// When the NeuroGuideManager tracks the incoming reward state, this class tracks the overall length of how long we have been in a reward state and calls INeuroGuideInteractable callbacks
    /// </summary>
    public class NeuroGuideExperience: Singleton<NeuroGuideExperience>
    {

        #region PUBLIC - VARIABLES

        /// <summary>
        /// Current instance of the NeuroGuideExperience system, initialized during Create()
        /// </summary>
        public static NeuroGuideExperienceSystem system;

        #endregion

        #region PRIVATE - VARIABLES

        #endregion

        #region PUBLIC - UPDATE

        /// <summary>
        /// Unity lifecycle method, used to update the experience progress every time the hardware sends us an update
        /// </summary>
        //--------------------------------//
        public void Update()
        //--------------------------------//
        {
            if(system == null)
            {
                return;
            }

            if(NeuroGuideManager.system.state != NeuroGuideManager.State.ReceivingData)
            {
                return;
            }

            // If the total duration isn't set, do nothing to prevent division by zero.
            if(system.options.totalDurationInSeconds <= 0)
            {
                return;
            }

            //If we do not have data to parse from the hardware, return early
            if(system.currentData == null)
            {
                return;
            }

            // Store the score before any changes to see if an update is needed.
            float previousScore = system.currentScore;

            // If in the reward state, add time. If not, subtract time.
            // Time.deltaTime ensures the change is frame-rate independent.
            if(system.currentData.isRecievingReward)
            {
                system.currentProgressInSeconds += Time.deltaTime;
            }
            else
            {
                system.currentProgressInSeconds -= Time.deltaTime;
            }

            // Clamp the progress to ensure it doesn't go below 0 or above the total duration.
            system.currentProgressInSeconds = Mathf.Clamp( system.currentProgressInSeconds, 0f, system.options.totalDurationInSeconds );

            // Calculate the new normalized score.
            system.currentScore = system.currentProgressInSeconds / system.options.totalDurationInSeconds;

            // If the score has changed, invoke the event to notify other parts of the experience.
            // This check prevents the event from firing unnecessarily every single frame.
            if(system.currentScore != previousScore)
            {
                //Let any interactables know about the new score
                if(system.interactables != null)
                {
                    for(int i = 0; i < system.interactables.Count; i++)
                    {
                        system.interactables[ i ].OnDataUpdate( system.currentScore );
                    }
                }

            }

            //Now that we've processed the data from the hardware, set our data object to null so we dont' reprocess it next Update!
            system.currentData = null;

        } //END Update Method

        #endregion

        #region PUBLIC - CREATION OPTIONS

        /// <summary>
        /// Creation options when creating a NeuroGuideExperience
        /// </summary>
        public class Options
        {
            /// <summary>
            /// Should debug logs be printed to the console log?
            /// </summary>
            public bool showDebugLogs = true;

            /// <summary>
            /// The NeuroGuideManager that deals with connecting and responding to harware event
            /// </summary>
            public NeuroGuideManager.NeuroGuideSystem hardwareSystem;

            /// <summary>
            /// How long the experience should take to reach the final state. Whenever the user is in the 'reward' state we count towards this, and when they leave the success state we move away from this.
            /// </summary>
            public float totalDurationInSeconds = 120f; 

        } //END Options Class

        #endregion

        #region PUBLIC - RETURN CLASS : NEUROGUIDE EXPERIENCE SYSTEM

        public class NeuroGuideExperienceSystem
        {
            /// <summary>
            /// The options passed in during Create()
            /// </summary>
            public Options options = new Options();

            /// <summary>
            /// List of the interactables we located when initializing
            /// </summary>
            public List<INeuroGuideInteractable> interactables = new List<INeuroGuideInteractable>();

            /// <summary>
            /// Unity action to call when the NeuroGuide data has been updated
            /// </summary>
            public Action<float> OnDataUpdate;

            /// <summary>
            /// How far the player is, normalizes 0-1, where '1' is reaching the end of the experience
            /// </summary>
            public float currentScore = 0f;

            /// <summary>
            /// The current progress in seconds. Can be useful for debugging or other UI.
            /// </summary>
            public float currentProgressInSeconds = 0f;

            /// <summary>
            /// The most up to date data that was sent in. If this is not null, we will process it in the Update() then set it to null
            /// </summary>
            public NeuroGuideData currentData;

        } //END NeuroGuideExperienceSystem Class

        #endregion

        #region PUBLIC - CREATE

        /// <summary>
        /// Tracks the NeuroGuideManager for updates, and changes the NeuroGuide experience according to the reward state
        /// </summary>
        /// <param name="options">Options object that determines how the NeuroGuide experience is initialized</param>
        /// <param name="OnSuccess">Callback action when the NeuroGuideExperience system successfully initializes</param>
        /// <param name="OnFailed">Callback action that returns a string with an error message when initialization fails</param>
        //-------------------------------------//
        public static void Create(
            Options options = null,
            Action<NeuroGuideExperienceSystem> OnSuccess = null,
            Action<string> OnFailed = null,
            Action<float> OnDataUpdated = null )
        //-------------------------------------//
        {
            if(system != null)
            {
                OnFailed?.Invoke( "NeuroGuideExperience.cs Create() NeuroGuideExperienceSystem object already exists. Unable to continue." );
                return;
            }

            //If the user didn't pass in any options, use the defaults
            if( options == null ) options = new Options();

            //If our NeuroGuideManager has not been instantiated, that needs to be done first
            if(options.hardwareSystem == null && NeuroGuideManager.system == null)
            {
                OnFailed.Invoke( "NeuroGuideExperience Create() unable to create experience, the 'NeuroGuideManager' that connects to the hardware was not passed in and has not been initialized yet. Please do that first before creating an experience using this singleton" );
            }
            else if(options.hardwareSystem == null && NeuroGuideManager.system != null)
            {
                options.hardwareSystem = NeuroGuideManager.system;
            }

            //Generate a NeuroGuideSystem object
            system = new NeuroGuideExperienceSystem();
            system.options = options;
            system.OnDataUpdate = OnDataUpdated;
            system.options.hardwareSystem.OnDataUpdate += OnHardwareUpdate;

            StoreAllComponentsWithInterfaceIncludingInactive( system );

            //Access a variable of the singleton instance, this will ensure it is initialized in the hierarchy with a GameObject representation
            //Doing this makes sure that Unity Lifecycle methods like Update() will run
            Instance.enabled = true;

            //We're done, call the OnSuccess callback
            OnSuccess?.Invoke( system );

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

            //Remove our listener to hardware events from the manager
            if(system.options.hardwareSystem != null)
            {
                system.options.hardwareSystem.OnDataUpdate -= OnHardwareUpdate;
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
                Debug.Log( "NeuroGuideExperience.cs FinishDestroy() cleaned up objects and data, ready to Create()" );
            }

            system = null;

        } //END FinishDestroy

        #endregion

        #region PRIVATE - ON HARDWARE DATA UPDATE

        /// <summary>
        /// Called whenever the hardware has new data for us to parse
        /// </summary>
        /// <param name="data"></param>
        //---------------------------------------------------------------//
        private static void OnHardwareUpdate( NeuroGuideData data )
        //---------------------------------------------------------------//
        {

            if(data == null)
            {
                return;
            }

            if(system == null)
            {
                return;
            }

            if( system.options.showDebugLogs ) Debug.Log( "NeuroGuideExperience.cs OnHardwareUpdate() isRecievingReward = " + data.isRecievingReward );
            system.currentData = data;

        } //END OnHardwareUpdate Method

        #endregion

        #region PRIVATE - STORE ALL COMPONENTS WITH INTERFACE

        /// <summary>
        /// Finds all components (both active and inactive) in the scene that implement the INeuroGuideInteractable interface.
        /// </summary>
        /// <returns>A List of components that implement the INeuroGuideInteractable.</returns>
        //----------------------------------------------------------------------------------------------//
        private static void StoreAllComponentsWithInterfaceIncludingInactive( NeuroGuideExperienceSystem system )
        //----------------------------------------------------------------------------------------------//
        {
            // To include inactive GameObjects in the search, use the FindObjectsInactive parameter.
            INeuroGuideInteractable[ ] interfaces = UnityEngine.Object.FindObjectsByType<UnityEngine.MonoBehaviour>( FindObjectsInactive.Include, FindObjectsSortMode.None )
                                                             .OfType<INeuroGuideInteractable>()
                                                             .ToArray();

            if(interfaces == null || (interfaces != null && interfaces.Length == 0))
            {
                return;
            }
            else
            {
                system.interactables = new List<INeuroGuideInteractable>( interfaces );
            }

        } //END StoreAllComponentsWithInterfaceIncludingInactive Method

        #endregion

    } //END NeuroGuideExperience Class

} //END gambit.neuroguide Namespace