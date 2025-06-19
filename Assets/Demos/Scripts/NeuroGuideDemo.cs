
namespace gambit.neuroguide
{

    #region IMPORTS

#if UNITY_INPUT
    using UnityEngine.InputSystem;
#endif

#if GAMBIT_NEUROGUIDE
    using gambit.neuroguide;
#endif


#if EXT_DOTWEEN
    using DG.Tweening;
#endif

    using UnityEngine;

    #endregion

    /// <summary>
    /// Spawns cubes to test the NeuroGuide hardware package. Press Enter to spawn, Delete to destroy, and Up and Down keys to test
    /// </summary>
    public class NeuroGuideDemo : MonoBehaviour
{

    #region PUBLIC - VARIABLES

    /// <summary>
    /// Should we enable the NeuroGuideManager debug logs?
    /// </summary>
    public bool logs = true;

    /// <summary>
    /// Should we enable the debug system for the NeuroGear hardware? This will enable keyboard events to control simulated NeuroGear hardware data spawned during the Create() method of NeuroGuideManager.cs
    /// </summary>
    public bool debug = true;

    /// <summary>
    /// How long should this experience last if the user were to be in a reward state (doesn't have to be consecutively), but their score to get towards the goal lowers when they are not in the reward state
    /// </summary>
    public float totalDurationInSeconds = 5;

    #endregion

    #region PUBLIC - START

    /// <summary>
    /// Unity lifecycle method
    /// </summary>
    //---------------------------------//
    public void Start()
    //---------------------------------//
    {

        CreateNeuroGuideManager();

    } //END Start Method

    #endregion

    #region PUBLIC - UPDATE

    /// <summary>
    /// Unity lifecycle method
    /// </summary>
    //----------------------------------//
    public void Update()
    //----------------------------------//
    {

        CreateOnEnterKey();
        DestroyOnDeleteKey();

    } //END Update Method

    #endregion

    #region PRIVATE - CREATE ON ENTER KEY PRESSED

    /// <summary>
    /// Creates the NeuroGuideManager instance when the enter key is pressed
    /// </summary>
    //-----------------------------------------------//
    private void CreateOnEnterKey()
    //-----------------------------------------------//
    {
#if UNITY_INPUT
        if(Keyboard.current.enterKey.wasPressedThisFrame)
        {
            CreateNeuroGuideManager();
        }
#else
        if( Input.GetKeyUp( KeyCode.Enter ) )
        {
            CreateNeuroGuideManager();
        }
#endif

    } //END CreateOnEnterKey Method

    /// <summary>
    /// Creates the NeuroGuideManager
    /// </summary>
    //---------------------------------------------//
    private void CreateNeuroGuideManager()
    //---------------------------------------------//
    {

        NeuroGuideManager.Create
        (
            //Create and pass in Options object
            new NeuroGuideManager.Options()
            {
                showDebugLogs = logs,
                enableDebugData = debug
            },

            //OnSuccess
            ( NeuroGuideManager.NeuroGuideSystem system ) => {
                if( logs ) Debug.Log( "NeuroGuideDemo.cs CreateNeuroGuideManager() Successfully created NeuroGuideManager and recieved system object" );

                CreateNeuroGuideExperience();
            },

            //OnFailed
            ( string error ) => {
                if( logs ) Debug.LogWarning( error );
            },

            //OnDataUpdate
            (NeuroGuideData) =>
            {
                //if( logs ) Debug.Log( "NeuroGuideDemo CreateNeuroGuideManager() Hardware Data updated ... data.isRecievingReward = " + data.isRecievingReward );
            },

            //OnStateUpdate
            ( NeuroGuideManager.State state ) =>
            {
                if( logs ) Debug.Log( "NeuroGuideDemo.cs CreateNeuroGuideManager() State changed to " + state.ToString() );
            } );

    } //END CreateNeuroGuideManager Method

    //----------------------------------------------//
    private void CreateNeuroGuideExperience()
    //----------------------------------------------//
    {
        NeuroGuideExperience.Create
        (
            //Create and Pass in Options object
            new NeuroGuideExperience.Options()
            {
                showDebugLogs = logs,
                totalDurationInSeconds = totalDurationInSeconds
            }, 

            //OnSuccess
            (NeuroGuideExperience.NeuroGuideExperienceSystem system)=> 
            {
                if( logs ) Debug.Log( "CreateNeuroGuideExperience() OnSuccess" );
            },
            
            //OnError
            (string error ) =>
            {
                if( logs ) Debug.Log( error );
            },

            //OnDataUpdated
            (float data ) =>
            {
                if(logs) Debug.Log( data );
            }
        ); 
    
    } //END CreateNeuroGuideExperience Method

#endregion

    #region PRIVATE - DESTROY ON DELETE KEY PRESSED

    /// <summary>
    /// Destroy the NeuroGuideManager instance
    /// </summary>
    //--------------------------------------------//
    private void DestroyOnDeleteKey()
    //--------------------------------------------//
    {

#if UNITY_INPUT
        if(Keyboard.current.deleteKey.wasPressedThisFrame)
        {
            NeuroGuideManager.Destroy();
            NeuroGuideExperience.Destroy();
        }
#else
        if( Input.GetKeyUp( KeyCode.Delete ) )
        {
            NeuroGuideManager.Destroy();
            NeuroGuideExperience.Destroy();
        }
#endif

    } //END DestroyOnDelete

    #endregion

} //END NeuroGuideDemo Class

} //END gambit.neuroguide Namespace