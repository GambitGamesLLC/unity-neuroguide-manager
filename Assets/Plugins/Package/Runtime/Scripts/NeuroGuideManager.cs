/********************************************************
 * NeuroGuideManager.cs
 * 
 * Listens to and responds to events from the NeuroGuide hardware
 * 
 ********************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace gambit.neuroguide
{

    /// <summary>
    /// Singleton Manager for interacting with the NeuroGuide hardware
    /// </summary>
    public class NeuroGuideManager : Singleton<NeuroGuideManager>
    {

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
            /// Should we enable keyboard input to debug input values? Press 'Up' on the keyboard to raise the values "Read" by the debug NeuroGear Device
            /// </summary>
            public bool enableDebugKeyboardInput = false;

            

        } //END Options

        #endregion

        #region PUBLIC - RETURN CLASS : NEUROGUIDE SYSTEM

        /// <summary>
        /// NeuroGuide System generated when Create() is successfully called. Contains values important for future modification and communication with the NeuroGuide Manager
        /// </summary>
        //-----------------------------------------//
        public class NeuroGuideSystem
        //-----------------------------------------//
        {

            

        } //END NeuroGuideSystem
        #endregion

        #region CREATE

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
            Action<string> OnFailed = null )
        //-------------------------------------//
        {
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
            NeuroGuideSystem system = new NeuroGuideSystem();

            //We're done, call the OnSuccess callback
            OnSuccess?.Invoke(system);

        } //END CREATE

        #endregion

    } //END NeuroGuideManager Class

} //END gambit.neuroguide Namespace