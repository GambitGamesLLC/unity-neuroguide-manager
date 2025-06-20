#region IMPORTS

#if GAMBIT_NEUROGUIDE
using DG.Tweening;
using gambit.neuroguide;
#endif

using UnityEngine;

#endregion

/// <summary>
/// Simple test to show how to respond to NeuroGuide experience value changes
/// </summary>
public class NeuroGuideInteractableDemo : MonoBehaviour, INeuroGuideInteractable
{

    #region PUBLIC - VARIABLES

    /// <summary>
    /// Cube used to demo the NeuroGuide Interactable interface functionality
    /// </summary>
    public GameObject cube;

    #endregion

    #region PUBLIC - ON DATA UPDATE

    /// <summary>
    /// Called when the NeuroGuideExperience updates the users progress in the experience
    /// </summary>
    /// <param name="normalizedValue">Progress, normalized 0-1 value</param>
    //------------------------------------------------------------------------------------------//
    public void OnDataUpdate( float normalizedValue )
    //------------------------------------------------------------------------------------------//
    {
        if(cube == null)
            return;

        //Debug.Log( normalizedValue );

#if EXT_DOTWEEN
        Vector3 scale = new Vector3( normalizedValue, normalizedValue, normalizedValue );
        cube.transform.DOScale( scale, .25f );
#endif

    } //END OnDataUpdate Method

    #endregion

} //END NeuroGuideInteractableDemo Class