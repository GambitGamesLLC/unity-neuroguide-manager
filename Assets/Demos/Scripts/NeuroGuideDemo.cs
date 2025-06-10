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
                enableDebugData = true
            }, 
            (NeuroGuideManager.NeuroGuideSystem system) => {
                Debug.Log("NeuroGuideDemo Start() Successfully created NeuroGuideManager and recieved system object");

                Debug.Log( "sensorID: " + system.data[ 0 ].sensorID + "\n" +
                            "currentValue : " + system.data[ 0 ].currentValue + "\n" +
                            "currentNormalizedValue : " + system.data[ 0 ].currentNormalizedValue + "\n" +
                            "rawData[0] : " + system.data[ 0 ].rawData[0]
                    );
            }, 
            (string error) =>{
                Debug.LogError( error );            
            });
    }


} //END Class
