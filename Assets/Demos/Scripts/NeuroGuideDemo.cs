
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
        public float totalDurationInSeconds = 120;

    #endregion

        #region PUBLIC - START

        /// <summary>
        /// Unity lifecycle method
        /// </summary>
        //----------------------------------//
    public void Start()
    //----------------------------------//
    {
        //Spawn cube to show progress for debugging
        GameObject go = GameObject.CreatePrimitive( PrimitiveType.Cube );
        go.name = "Cube";
        go.transform.parent = gameObject.transform;
        go.transform.localPosition = new Vector3( 0, 0, 0 );

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

        NeuroGuideManager.Create(
            new NeuroGuideManager.Options()
            {
                showDebugLogs = logs,
                enableDebugData = debug
            },
            ( NeuroGuideManager.NeuroGuideSystem system ) => {
                if( logs ) Debug.Log( "NeuroGuideDemo.cs CreateNeuroGuideManager() Successfully created NeuroGuideManager and recieved system object" );

                CreateNeuroGuideExperience();
            },
            ( string error ) => {
                if( logs ) Debug.LogWarning( error );
            },
            (NeuroGuideData) =>
            {
                //if( logs ) Debug.Log( "NeuroGuideDemo CreateNeuroGuideManager() Hardware Data updated ... data.isRecievingReward = " + data.isRecievingReward );
            },
            ( NeuroGuideManager.State state ) =>
            {
                if( logs ) Debug.Log( "NeuroGuideDemo.cs CreateNeuroGuideManager() State changed to " + state.ToString() );
            } );

    } //END CreateNeuroGuideManager Method

    //----------------------------------------------//
    private void CreateNeuroGuideExperience()
    //----------------------------------------------//
    {
        NeuroGuideExperience.Options options = new NeuroGuideExperience.Options();
        options.showDebugLogs = logs;
        options.totalDurationInSeconds = totalDurationInSeconds;

        NeuroGuideExperience.Create(); 
    
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
            DestroyCubes();
            NeuroGuideManager.Destroy();
            NeuroGuideExperience.Destroy();
        }
#else
        if( Input.GetKeyUp( KeyCode.Delete ) )
        {
            DestroyCubes();
            NeuroGuideManager.Destroy();
            NeuroGuideExperience.Destroy();
        }
#endif

    } //END DestroyOnDelete

    //-------------------------------//
    private void DestroyCubes()
    //-------------------------------//
    {

        GameObject go = GameObject.Find( "Cube" );
        Destroy( go );

     } //END DestroyCubes

    #endregion

} //END NeuroGuideDemo Class

} //END gambit.neuroguide Namespace