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
using UnityEngine.Rendering.VirtualTexturing;

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

        /// <summary>
        /// The score variable from the previous update
        /// </summary>
        private static float previousScore;

        //Store a flag, if set to true then later during Update() we'll send a message to all INeuroGuideInteractable
        //classes to let them know the isRecievingReward state has changed
        private static bool isRecievingRewardChanged = false;

        /// <summary>
        /// Flag set after we fall below the threshold value, 
        /// used to prevent the OnAboveThreshold callback for the length of time set via the preventThresholdPassedLength value.
        /// </summary>
        private static bool preventThresholdPassed;

        /// <summary>
        /// Used to prevent how long we should wait before we call OnAboveThreshold,
        /// after we get our score above the threshold value, then fall back below.
        /// This counter float has the DeltaTime variable added to it each Update()
        /// </summary>
        private static float preventThresholdPassedTimer = 0f;

        /// <summary>
        /// Has the score passed the threshold value?
        /// </summary>
        private static bool thresholdPassed;

        /// <summary>
        /// Callback delegate called when our score goes above the threshold and enough time has passed
        /// </summary>
        private static Action OnAboveThreshold;

        /// <summary>
        /// Callback delegate called when our score goes below the threshold
        /// </summary>
        private static Action OnBelowThreshold;

        #endregion

        #region PUBLIC - UPDATE

        /// <summary>
        /// Unity lifecycle method, used to update the experience progress every time the hardware sends us an update
        /// </summary>
        //--------------------------------//
        public void Update()
        //--------------------------------//
        {
            if( ShouldUpdateBePrevented())
            {
                return;
            }

            DetermineCurrentScore();

            CheckIfScoreIsAboveThreshold();

            CheckIfScoreIsBelowThreshold();

            CheckIfOnRecievingRewardHasChanged();

            SendMessagesToInteractables();

            StorePreviousData();

        } //END Update Method

        #endregion

        #region PRIVATE - UPDATE - CHECK IF UPDATE SHOULD BE PREVENTED

        /// <summary>
        /// Checks for null references and missing dependencies
        /// </summary>
        /// <returns></returns>
        //------------------------------------------------------------//
        private static bool ShouldUpdateBePrevented()
        //------------------------------------------------------------//
        {
            if(system == null)
            {
                return true;
            }

            if(NeuroGuideManager.system.state != NeuroGuideManager.State.ReceivingData)
            {
                return true;
            }

            // If the total duration isn't set, do nothing to prevent division by zero.
            if(system.options.totalDurationInSeconds <= 0)
            {
                return true;
            }

            //If we do not have data to parse from the hardware, return early
            if(system.currentData.HasValue == false)
            {
                return true;
            }

            return false;

        } //END ShouldUpdateBePrevented Method

        #endregion

        #region PRIVATE - UPDATE - DETERMINE CURRENT SCORE

        /// <summary>
        /// Calculates the current score for the experience
        /// </summary>
        //------------------------------------------------------//
        private static void DetermineCurrentScore()
        //------------------------------------------------------//
        {

            // Store the score before any changes to see if an update is needed.
            previousScore = system.currentScore;

            // If in the reward state, add time. If not, subtract time.
            // Time.deltaTime ensures the change is frame-rate independent.
            if(system.currentData.Value.isRecievingReward)
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


        } //END DetermineCurrentScore Method

        #endregion

        #region PRIVATE - UPDATE - CHECK IF SCORE IS ABOVE THRESHOLD

        /// <summary>
        /// If the score is above the threshold, send a OnAboveThreshold callback
        /// </summary>
        //------------------------------------------------------------//
        private static void CheckIfScoreIsAboveThreshold()
        //------------------------------------------------------------//
        {

            if(system.currentScore > system.options.threshold)
            {
                if(preventThresholdPassed)
                {

                }
            }

        } //END CheckIfScoreIsAboveThreshold Method

        #endregion

        #region PRIVATE - UPDATE - CHECK IF SCORE IS BELOW THRESHOLD

        /// <summary>
        /// If the score is below the threshold after being above the threshold, 
        /// run a timer that prevent the OnAboveThreshold callback from being called until the timer is complete
        /// </summary>
        //------------------------------------------------------------//
        private static void CheckIfScoreIsBelowThreshold()
        //------------------------------------------------------------//
        {

            //If below the threshold
            if(system.currentScore < system.options.threshold)
            {

            }

        } //END CheckIfScoreIsBelowThreshold Method

        #endregion

        #region PRIVATE - UPDATE - CHECK IF 'ON RECIEVING REWARD' HAS CHANGED

        /// <summary>
        /// During Update, we check if we've changed states 
        /// between recieving or not recieving a reward, 
        /// let our callbacks and INeuroGuideInteractables know
        /// </summary>
        //-----------------------------------------------------------//
        private static void CheckIfOnRecievingRewardHasChanged()
        //-----------------------------------------------------------//
        {

            //Reset the flag for this update
            isRecievingRewardChanged = false;

            //If the previousData is null (so this is our first piece of data),
            //then the state has changed
            if(system.previousData.HasValue == false)
            {
                isRecievingRewardChanged = true;
            }
            //Of if the previous 'isRecievingReward' is different than the current data
            //then the state has changed
            else if(system.previousData.Value.isRecievingReward != system.currentData.Value.isRecievingReward)
            {
                isRecievingRewardChanged = true;
            }

        } //END CheckIfOnRecievingRewardHasChanged Method

        #endregion

        #region PRIVATE - UPDATE - SEND MESSAGES TO INTERACTABLES

        /// <summary>
        /// Messages interactables based on the current score
        /// </summary>
        //-------------------------------------------//
        private static void SendMessagesToInteractables()
        //-------------------------------------------//
        {

            // If the score has changed, invoke the event to notify other parts of the experience.
            // This check prevents the event from firing unnecessarily every single frame.
            if(system.currentScore != previousScore)
            {

                if(system.interactables != null)
                {
                    for(int i = 0; i < system.interactables.Count; i++)
                    {
                        //If the isRecievingReward changed from true to false, or false to true, then let the interactables know
                        if(isRecievingRewardChanged)
                        {
                            system.interactables[ i ].OnRecievingRewardChanged( system.currentData.Value.isRecievingReward );
                        }

                        //Let any interactables know about the new score
                        system.interactables[ i ].OnDataUpdate( system.currentScore );
                    }
                }

            }

        } //END UpdateInteractables Method

        #endregion

        #region PRIVATE - UPDATE - STORE PREVIOUS DATA

        /// <summary>
        /// Stores the current data as the previous, then sets our current data as empty, setting us up for the next Update()
        /// </summary>
        //-------------------------------------------//
        private static void StorePreviousData()
        //-------------------------------------------//
        {

            //Store our currentData as our previousData
            system.previousData = system.currentData;

            //Now that we've processed the data from the hardware, set our data object to null so we dont' reprocess it next Update!
            system.currentData = null;

        } //END StorePreviousData Method

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

            /// <summary>
            /// How far into the experience should the score be before it passes the threshold, causing the OnAboveThreshold to be called?
            /// If the score goes back below this value, we call OnBelowThreshold
            /// </summary>
            public float threshold = .9f;

            /// <summary>
            /// How long we want to prevent the OnAboveThreshold Action callback from being called
            /// after first being above the threshold, then falling below the threshold value?
            /// This prevents OnAboveThreshold callback from being called until the timer runs out
            /// </summary>
            private static float preventThresholdPassedLength = 2f;

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
            public NeuroGuideData? currentData;

            /// <summary>
            /// The previous data update, If this is not null, we will check to see if the currentData has a different isRecievingReward state and let our INeuroGuideInteractables know about the change
            /// </summary>
            public NeuroGuideData? previousData;

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
        private static void OnHardwareUpdate( NeuroGuideData? data )
        //---------------------------------------------------------------//
        {

            if(data == null)
            {
                return;
            }

            if(data.HasValue == false )
            {
                return;
            }

            if(system == null)
            {
                return;
            }

            //if( system.options.showDebugLogs ) Debug.Log( "NeuroGuideExperience.cs OnHardwareUpdate() isRecievingReward = " + data.Value.isRecievingReward );
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