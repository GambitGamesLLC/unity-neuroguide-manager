
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

    #region PRIVATE - VARIABLES

    private GameObject cubeParent;

    public bool logs = true;
    public bool debug = true;
    public int entries = 1;
    public int min = -5;
    public int max = 5;
#if EXT_DOTWEEN
    public Ease ease = Ease.OutBounce;
#endif
    public int duration = 2;

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
                debugTweenDuration = duration
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