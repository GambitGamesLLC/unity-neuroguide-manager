
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

        private GameObject cubeParent;

        /// <summary>
        /// Should we enable the NeuroGuideManager debug logs?
        /// </summary>
        public bool logs = true;

        /// <summary>
        /// Should we enable the debug system for the NeuroGear hardware? This will enable keyboard events to control simulated NeuroGear hardware data spawned during the Create() method of NeuroGuideManager.cs
        /// </summary>
        public bool debug = true;

        /// <summary>
        /// How many cubes should we spawn? Each cube will be tied to a NeuroGuideData object
        /// </summary>
        public int entries = 1;

        /// <summary>
        /// What is the min value we should use for the local position possible to reach by the cube movement?
        /// </summary>
        public int min = -5;

        /// <summary>
        /// What is the max value we should use for the local position possible to reach by the cube movement?
        /// </summary>
        public int max = 5;

#if EXT_DOTWEEN
        /// <summary>
        /// What tween easing should we use?
        /// </summary>
        public Ease ease = Ease.OutBounce;
#endif

        /// <summary>
        /// How long should our tweens take?
        /// </summary>
        public int duration = 2;

        /// <summary>
        /// Should the starting value of our NeuroGuideData be randomized?
        /// </summary>
        public bool randomizeStartValue = true;

    #endregion

    #region PUBLIC - START

    /// <summary>
    /// Unity lifecycle method
    /// </summary>
    //----------------------------------//
    public void Start()
    //----------------------------------//
    {
        cubeParent = gameObject;

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
                enableDebugData = debug,
                debugNumberOfEntries = entries,
                debugMinCurrentValue = min,
                debugMaxCurrentValue = max,
#if EXT_DOTWEEN
                debugEaseType = ease,
#endif
                debugTweenDuration = duration,
                debugRandomizeStartingValues = randomizeStartValue
            },
            ( NeuroGuideManager.NeuroGuideSystem system ) => {
                Debug.Log( "NeuroGuideDemo.cs CreateNeuroGuideManager() Successfully created NeuroGuideManager and recieved system object... system.data.count = " + system.data.Count );

                //Spawn cubes to match the system data
                for(int i = 0; i < system.data.Count; i++)
                {
                    GameObject go = GameObject.CreatePrimitive( PrimitiveType.Cube );
                    go.name = "Cube: " + system.data[i].name;
                    go.transform.parent = cubeParent.transform;
                    go.transform.localPosition = new Vector3( system.data[i].currentValue, 0, 0 );
                }
                
            },
            ( string error ) => {
                Debug.LogWarning( error );
            },
            (NeuroGuideManager.NeuroGuideSystem system) =>
            {
                //Debug.Log( "NeuroGuideDemo CreateNeuroGuideManager() Data Updated" );

                if(system != null && system.data != null && system.data.Count > 0)
                {
                    for(int i = 0; i < system.data.Count; i++)
                    {
                        GameObject go = GameObject.Find( "Cube: " + system.data[ i ].name );

                        if(go != null)
                        {
                            go.transform.localPosition = new Vector3( system.data[ i ].currentValue, 0, 0 );
                        }
                    }
                }
                
            },
        ( NeuroGuideManager.NeuroGuideSystem system, NeuroGuideManager.State state ) =>
            {
                Debug.Log( "NeuroGuideDemo.cs CreateNeuroGuideManager() State changed to " + state.ToString() );
            } );

    } //END CreateNeuroGuideManager Method

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
        }
#else
        if( Input.GetKeyUp( KeyCode.Delete ) )
        {
            DestroyCubes();
            NeuroGuideManager.Destroy();
        }
#endif

    } //END DestroyOnDelete

    //-------------------------------//
    private void DestroyCubes()
    //-------------------------------//
    {

        if(NeuroGuideManager.system != null && NeuroGuideManager.system.data.Count > 0)
        {
            for(int i = 0; i < NeuroGuideManager.system.data.Count; i++)
            {
                GameObject go = GameObject.Find( "Cube: " + NeuroGuideManager.system.data[ i ].name );
                Destroy( go );
            }
        }

    } //END DestroyCubes

    #endregion

} //END NeuroGuideDemo Class

} //END gambit.neuroguide Namespace