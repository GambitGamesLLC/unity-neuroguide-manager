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
    /// When the NeuroGuideManager tracks the incoming reward state,
    /// we transform the focus into a 0-1 normalize score.
    /// When the score goes above or below a threshold, we send callbacks
    /// this class tracks the overall length of how long we have been 
    /// in a reward state and calls INeuroGuideInteractable callbacks
    /// </summary>
    public class NeuroGuideFocusMeterExperience : Singleton<NeuroGuideAnimationExperience>
    {

        #region PUBLIC - VARIABLES

        /// <summary>
        /// Current instance of the NeuroGuideExperience system, initialized during Create()
        /// </summary>
        public static NeuroGuideFocusMeterExperienceSystem system;

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
        /// Has the score passed the threshold value?
        /// </summary>
        private static bool scoreIsAboveThreshold;

        /// <summary>
        /// Are we calling OnAboveCallback this Update()? If so, we also need to let all our interactables know
        /// </summary>
        private static bool sendOnAboveThresholdCallback = false;

        /// <summary>
        /// Are we calling OnBelowCallback this Update()? If so, we also need to let all our interactables know
        /// </summary>
        private static bool sendOnBelowThresholdCallback = false;

        #endregion

        #region PUBLIC - UPDATE

        /// <summary>
        /// Unity lifecycle method, used to update the experience progress every time the hardware sends us an update
        /// </summary>
        //--------------------------------//
        public void Update()
        //--------------------------------//
        {
            //Regardless of anything else, we always update our score
            DetermineCurrentScore();


            //Next, Figure out if the score is above or below the threshold
            CheckIfScoreIsAboveThreshold();

            CheckIfScoreIsBelowThreshold();

            SendThresholdMessageToCallbacks();

            SendThresholdMessageToInteractables();

            ResetThresholdVariablesAndFlags();


            //Next, figure out if the DataUpdate callback should be messaged
            CheckIfOnRecievingRewardHasChanged();

            SendDataUpdateMessageToCallbacks();

            SendDataUpdateMessageToInteractables();

            SetupDataForNextUpdate();

        } //END Update Method

        #endregion

        #region PRIVATE - UPDATE - DETERMINE CURRENT SCORE

        /// <summary>
        /// Calculates the current score for the experience
        /// </summary>
        //------------------------------------------------------//
        private static void DetermineCurrentScore()
        //------------------------------------------------------//
        {
            if (system == null)
            {
                return;
            }

            if (system.currentData == null)
            {
                return;
            }

            if (system.currentData.HasValue == false)
            {
                return;
            }

            if (system.options.totalDurationInSeconds <= 0)
            {
                return;
            }

            // If in the reward state, add time. If not, subtract time.
            // Time.deltaTime ensures the change is frame-rate independent.
            if (system.currentData.Value.isRecievingReward)
            {
                system.currentProgressInSeconds += Time.deltaTime;
            }
            else
            {
                system.currentProgressInSeconds -= Time.deltaTime;
            }

            // Clamp the progress to ensure it doesn't go below 0 or above the total duration.
            system.currentProgressInSeconds = Mathf.Clamp(system.currentProgressInSeconds, 0f, system.options.totalDurationInSeconds);

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
            if (system == null)
            {
                return;
            }

            if (system.options == null)
            {
                return;
            }

            if (system.currentScore > system.options.threshold)
            {

                if (scoreIsAboveThreshold == false)
                {
                    scoreIsAboveThreshold = true;

                    sendOnAboveThresholdCallback = true;
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
            if (system == null)
            {
                return;
            }

            if (system.options == null)
            {
                return;
            }

            //If below the threshold
            if (system.currentScore < system.options.threshold)
            {
                //We only need to call our Callback and set our flags if this is the first time we've fallen below the threshold.
                //When we get back above the threshold, these flags are okay to be set again
                if (scoreIsAboveThreshold == true)
                {
                    scoreIsAboveThreshold = false;

                    sendOnBelowThresholdCallback = true;
                }

            }

        } //END CheckIfScoreIsBelowThreshold Method

        #endregion

        #region PRIVATE - UPDATE - SEND THRESHOLD UPDATE MESSAGES TO CALLBACKS

        /// <summary>
        /// If we've gone above or below the threshold this Update, inform the listeners
        /// </summary>
        //-----------------------------------------------------------------//
        private static void SendThresholdMessageToCallbacks()
        //-----------------------------------------------------------------//
        {
            if (system == null)
            {
                return;
            }

            if (system.options == null)
            {
                return;
            }

            if (sendOnAboveThresholdCallback)
            {
                system.options.OnAboveThreshold?.Invoke();
            }

            if (sendOnBelowThresholdCallback)
            {
                system.options.OnBelowThreshold?.Invoke();
            }

        } //END SendThresholdMessageToCallbacks Method

        #endregion

        #region PRIVATE - UPDATE - SEND THRESHOLD UPDATE MESSAGES TO INTERACTABLES

        /// <summary>
        /// Messages interactables based on if we are above or below the threshold
        /// </summary>
        //-------------------------------------------//
        private static void SendThresholdMessageToInteractables()
        //-------------------------------------------//
        {
            if (system.interactables == null)
            {
                return;
            }

            //Loop through all interactables, and let them know if the OnAboveThreshold callback should be called
            //We do this regardless of whether or not the score has changed from the last Update()
            for (int i = 0; i < system.interactables.Count; i++)
            {
                if (sendOnAboveThresholdCallback)
                {
                    system.interactables[i].OnAboveThreshold();
                }

                if (sendOnBelowThresholdCallback)
                {
                    system.interactables[i].OnBelowThreshold();
                }
            }

        } //END SendThresholdMessageToInteractables Method

        #endregion

        #region PRIVATE - UPDATE - RESET THRESHOLD VARIABLES AND FLAGS

        /// <summary>
        /// Resets variables and flags used to track when we go above and below the score threshold.
        /// Occurs every Update() cycle
        /// </summary>
        //------------------------------------------------------------//
        private static void ResetThresholdVariablesAndFlags()
        //------------------------------------------------------------//
        {
            sendOnAboveThresholdCallback = false;
            sendOnBelowThresholdCallback = false;

        } //END ResetThresholdVariablesAndFlags Method

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
            if (system == null)
            {
                return;
            }

            if (system.currentData == null)
            {
                return;
            }

            //Reset the flag for this update
            isRecievingRewardChanged = false;

            //If the previousData is null (so this is our first piece of data),
            //then the state has changed
            if (system.previousData.HasValue == false)
            {
                //Debug.Log( "previousData has no value, so reward has changed" );
                isRecievingRewardChanged = true;
            }
            //Of if the previous 'isRecievingReward' is different than the current data
            //then the state has changed
            else if (system.previousData.Value.isRecievingReward != system.currentData.Value.isRecievingReward)
            {
                //Debug.Log( "previous.isRecievingReward is different from current.isRecievingReward" );
                isRecievingRewardChanged = true;
            }

        } //END CheckIfOnRecievingRewardHasChanged Method

        #endregion

        #region PRIVATE - UPDATE - SEND DATA UPDATE MESSAGE TO CALLBACKS

        /// <summary>
        /// Messages callbacks based on the current score
        /// </summary>
        //---------------------------------------------------//
        private static void SendDataUpdateMessageToCallbacks()
        //---------------------------------------------------//
        {

            if (system == null)
            {
                return;
            }

            if (system.options == null)
            {
                return;
            }

            // If the score has changed, invoke the event to notify other parts of the experience.
            // This check prevents the event from firing unnecessarily every single frame.
            if (system.currentScore != previousScore)
            {
                //Debug.Log( "current = " + system.currentScore + ", previous = " + previousScore );

                //If the isRecievingReward changed from true to false, or false to true, then let the callbacks know
                if (isRecievingRewardChanged)
                {
                    system.options.OnRecievingRewardChanged?.Invoke(system.currentData.Value.isRecievingReward);
                }

                //Let any callbacks know about the new score
                system.options.OnDataUpdate?.Invoke(system.currentScore);
            }

        } //END SendDataUpdateMessageToCallbacks Method

        #endregion

        #region PRIVATE - UPDATE - SEND DATA UPDATE MESSAGES TO INTERACTABLES

        /// <summary>
        /// Messages interactables based on the current score
        /// </summary>
        //-------------------------------------------//
        private static void SendDataUpdateMessageToInteractables()
        //-------------------------------------------//
        {
            if (system.interactables == null)
            {
                return;
            }

            if (NeuroGuideManager.system.state != NeuroGuideManager.State.ReceivingData)
            {
                return;
            }

            // If the score has changed, invoke the event to notify other parts of the experience.
            // This check prevents the event from firing unnecessarily every single frame.
            if (system.currentScore != previousScore)
            {
                for (int i = 0; i < system.interactables.Count; i++)
                {

                    //If the isRecievingReward changed from true to false, or false to true, then let the interactables know
                    if (isRecievingRewardChanged)
                    {
                        system.interactables[i].OnRecievingRewardChanged(system.currentData.Value.isRecievingReward);
                    }

                    //Let any interactables know about the new score
                    system.interactables[i].OnDataUpdate(system.currentScore);
                }
            }

        } //END SendDataUpdateMessageToInteractables Method

        #endregion

        #region PRIVATE - UPDATE - SETUP DATA FOR NEXT UPDATE

        /// <summary>
        /// Reset variables and flags used to update the score data every Update()
        /// </summary>
        //-------------------------------------------------------//
        private static void SetupDataForNextUpdate()
        //-------------------------------------------------------//
        {

            // Store the current score as the previous
            //Debug.Log( "Setting previousScore to " + system.currentScore );
            previousScore = system.currentScore;

            //If our currentData is not null, copy it to our previousData
            if (system.currentData.HasValue)
            {
                //Store our currentData as our previousData
                system.previousData = new NeuroGuideData()
                {
                    isRecievingReward = system.currentData.Value.isRecievingReward,
                    timestamp = system.currentData.Value.timestamp
                };

                //Debug.Log( "Set previous to current, previous.HasValue = " + system.previousData.HasValue + ", current.HasValue = " + system.currentData.HasValue );

                //Now that we've processed the data from the hardware, set our data object to null so we dont' reprocess it next Update!
                system.currentData = null;

            }

        } //END SetupDataForNextUpdate Method

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
            /// Callback delegate called when our score goes above the threshold and enough time has passed
            /// </summary>
            public Action OnAboveThreshold;

            /// <summary>
            /// Callback delegate called when our score goes below the threshold
            /// </summary>
            public Action OnBelowThreshold;

            /// <summary>
            /// Callback delegate called when our reward boolean has flipped from true to false or false to true
            /// </summary>
            public Action<bool> OnRecievingRewardChanged;

            /// <summary>
            /// Callback delegate called when our score changes
            /// </summary>
            public Action<float> OnDataUpdate;

        } //END Options Class

        #endregion

        #region PUBLIC - RETURN CLASS : NEUROGUIDE EXPERIENCE SYSTEM

        public class NeuroGuideFocusMeterExperienceSystem
        {
            /// <summary>
            /// The options passed in during Create()
            /// </summary>
            public Options options = new Options();

            /// <summary>
            /// List of the interactables we located when initializing
            /// </summary>
            public List<INeuroGuideFocusMeterExperienceInteractable> interactables = new List<INeuroGuideFocusMeterExperienceInteractable>();

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

        } //END NeuroGuideFocusExperienceSystem Class

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
            Action<NeuroGuideFocusMeterExperienceSystem> OnSuccess = null,
            Action<string> OnFailed = null)
        //-------------------------------------//
        {
            if (system != null)
            {
                OnFailed?.Invoke("NeuroGuideFocusExperience.cs Create() NeuroGuideExperienceSystem object already exists. Unable to continue.");
                return;
            }

            //If the user didn't pass in any options, use the defaults
            if (options == null) options = new Options();

            //If our NeuroGuideManager has not been instantiated, that needs to be done first
            if (options.hardwareSystem == null && NeuroGuideManager.system == null)
            {
                OnFailed.Invoke("NeuroGuideExperience Create() unable to create experience, the 'NeuroGuideManager' that connects to the hardware was not passed in and has not been initialized yet. Please do that first before creating an experience using this singleton");
            }
            else if (options.hardwareSystem == null && NeuroGuideManager.system != null)
            {
                options.hardwareSystem = NeuroGuideManager.system;
            }

            //Generate a NeuroGuideSystem object
            system = new NeuroGuideFocusMeterExperienceSystem();
            system.options = options;
            system.options.hardwareSystem.OnDataUpdate += OnHardwareUpdate;

            StoreAllComponentsWithInterfaceIncludingInactive(system);

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

            if (system == null)
            {
                return;
            }

            //Remove our listener to hardware events from the manager
            if (system.options.hardwareSystem != null)
            {
                system.options.hardwareSystem.OnDataUpdate -= OnHardwareUpdate;
            }

            Instance.Invoke("FinishDestroy", .1f);

        } //END Destroy Method

        /// <summary>
        /// Invoked by Destroy(), after allowing for tweens to be cleaned up, destroys the gameobjects
        /// </summary>
        //------------------------------------//
        private void FinishDestroy()
        //------------------------------------//
        {
            if (system.options.showDebugLogs)
            {
                Debug.Log("NeuroGuideExperience.cs FinishDestroy() cleaned up objects and data, ready to Create()");
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
        private static void OnHardwareUpdate(NeuroGuideData? data)
        //---------------------------------------------------------------//
        {

            if (data == null)
            {
                return;
            }

            if (data.HasValue == false)
            {
                return;
            }

            if (system == null)
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
        private static void StoreAllComponentsWithInterfaceIncludingInactive(NeuroGuideFocusMeterExperienceSystem system)
        //----------------------------------------------------------------------------------------------//
        {
            // To include inactive GameObjects in the search, use the FindObjectsInactive parameter.
            INeuroGuideFocusMeterExperienceInteractable[] interfaces = UnityEngine.Object.FindObjectsByType<UnityEngine.MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                                                             .OfType<INeuroGuideFocusMeterExperienceInteractable>()
                                                             .ToArray();

            if (interfaces == null || (interfaces != null && interfaces.Length == 0))
            {
                return;
            }
            else
            {
                system.interactables = new List<INeuroGuideFocusMeterExperienceInteractable>(interfaces);
            }

        } //END StoreAllComponentsWithInterfaceIncludingInactive Method

        #endregion

    } //END NeuroGuideExperience Class

} //END gambit.neuroguide Namespace