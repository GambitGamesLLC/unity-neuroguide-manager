using UnityEngine;

#if GAMBIT_NEUROGUIDE
using gambit.neuroguide;
#endif

public class NeuroGuideDemo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NeuroGuideManager.Create( 
            new NeuroGuideManager.Options()
            {
                showDebugLogs = true,
                enableDebugKeyboardInput = true
            }, 
            (NeuroGuideManager.NeuroGuideSystem system) => {
                Debug.Log("NeuroGuideDemo Start() Successfully created NeuroGuideManager and recieved system object"); 
            }, 
            (string error) =>{
                Debug.LogError( "NeuroGuideDemo Start() Failed to create NeuroGuideManager ... error = " + error);            
            });
    }


} //END Class
