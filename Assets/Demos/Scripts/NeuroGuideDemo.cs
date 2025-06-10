using UnityEngine;

#if UNITY_INPUT
using UnityEngine.InputSystem;
#endif

#if GAMBIT_NEUROGUIDE
using gambit.neuroguide;
#endif

public class NeuroGuideDemo : MonoBehaviour
{
    
    #region PUBLIC - START

    /// <summary>
    /// Unity lifecycle method
    /// </summary>
    //----------------------------------//
    public void Start()
    //----------------------------------//
    {
        

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

    #region CREATE ON ENTER KEY PRESSED

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
            CreateNeuroGearManager();
        }
#else
        if( Input.GetKeyUp( KeyCode.Enter ) )
        {
            CreateNeuroGearManager();
        }
#endif

    } //END CreateOnEnterKey Method

    /// <summary>
    /// Creates the NeuroGearManager
    /// </summary>
    //---------------------------------------------//
    private void CreateNeuroGearManager()
    //---------------------------------------------//
    {

        NeuroGuideManager.Create(
            new NeuroGuideManager.Options()
            {
                showDebugLogs = true,
                enableDebugData = true
            },
            ( NeuroGuideManager.NeuroGuideSystem system ) => {
                Debug.Log( "NeuroGuideDemo CreateNeuroGearManager() Successfully created NeuroGuideManager and recieved system object" );

                /*
                Debug.Log( "sensorID: " + system.data[ 0 ].sensorID + "\n" +
                            "currentValue : " + system.data[ 0 ].currentValue + "\n" +
                            "currentNormalizedValue : " + system.data[ 0 ].currentNormalizedValue );
                */
            },
            ( string error ) => {
                Debug.LogWarning( error );
            },
            () =>
            {
                Debug.Log( "NeuroGuideDemo CreateNeuroGearManager() Data Updated" );
            },
            ( NeuroGuideManager.State state ) =>
            {
                Debug.Log( "NeuroGuideDemo CreateNeuroGearManager() State changed to " + state.ToString() );
            } );

    } //END CreateNeuroGearManager Method

    #endregion

    #region DESTROY ON DELETE KEY PRESSED

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
        }
#else
        if( Input.GetKeyUp( KeyCode.Delete ) )
        {
            NeuroGuideManager.Destroy();
        }
#endif

    } //END DestroyOnDelete

    #endregion


} //END Class
