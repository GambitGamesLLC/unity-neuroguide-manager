#region IMPORTS

#if EXT_DOTWEEN
using DG.Tweening;
#endif

#if GAMBIT_NEUROGUIDE
using gambit.neuroguide;
using Unity.VisualScripting;

#endif

using UnityEngine;
using UnityEngine.UI;

#endregion

/// <summary>
/// Simple test to show how to respond to NeuroGuide experience value changes
/// </summary>
public class NeuroGuideInteractableDemo : MonoBehaviour, INeuroGuideAnimationExperienceInteractable, INeuroGuideFocusMeterExperienceInteractable
{

    #region PUBLIC - VARIABLES

    /// <summary>
    /// Cube used to demo the NeuroGuide Interactable interface functionality
    /// </summary>
    public GameObject cube;

    /// <summary>
    /// Slider used to convey how much they are to the next level
    /// </summary>
    public Slider slider;

    #endregion

    #region PRIVATE - VARIABLES

    /// <summary>
    /// Renderer used to render the object
    /// </summary>
    private Renderer objectRenderer;

    private bool hasReachedThreshold = false;

    [Header("Colors used to represent and convey each level to the user")]
    [SerializeField] private Color level0Color;
    [SerializeField] private Color level1Color;
    [SerializeField] private Color level2Color;
    [SerializeField] private Color level3Color;
    [SerializeField] private Color level4Color;
    [SerializeField] private Color level5Color;

    #endregion

    #region PRIVATE - AWAKE

    //------------------//
    private void Awake()
    //------------------//
    {
        objectRenderer = cube.GetComponent<Renderer>();

        level0Color = Color.grey;
        level1Color = Color.green;
        level2Color = Color.blue;
        level3Color = Color.yellow;
        level4Color = Color.orange;
        level5Color = Color.red;

    } //END Start

    #endregion

    #region PUBLIC - ON ABOVE THRESHOLD

    /// <summary>
    /// Called when the score goes above the threshold.
    /// Once the score falls below the threshold, 
    /// we wait for a timer to complete before we can call this again when we go above the threshold
    /// </summary>
    //----------------------------------//
    public void OnAboveThreshold()
    //----------------------------------//
    {
        Debug.Log("NeuroGuideInteractableDemo.cs // OnAboveThreshold() //");

    } //END OnAboveThreshold

    #endregion

    #region PUBLIC - ON BELOW THRESHOLD

    /// <summary>
    /// Called when the score goes above the threshold, then falls back below it
    /// </summary>
    //--------------------------------//
    public void OnBelowThreshold()
    //--------------------------------//
    {
        Debug.Log("NeuroGuideInteractableDemo // OnBelowThreshold() //");
    
    } //END OnBelowThreshold Method

    #endregion

    #region PUBLIC - ON RECIEVING REWARD UPDATE

    /// <summary>
    /// Called when the user start or stops recieving a reward
    /// </summary>
    /// <param name="isRecievingReward">Is the user recieving a reward?</param>
    //------------------------------------------------------------------------------------------//
    public void OnRecievingRewardChanged(bool isRecievingReward)
    //------------------------------------------------------------------------------------------//
    {
        Debug.Log("NeuroGuideInteractableDemo.cs OnRecievingRewardChanged() state = " + isRecievingReward.ToString());

    } //END OnRecievingRewardChanged Method

    #endregion

    #region PUBLIC - ON ABOVE FOCUS THRESHOLD

    /// <summary>
    /// Called when the score goes above the threshold.
    /// Once the score falls below the threshold, 
    /// we wait for a timer to complete before we can call this again when we go above the threshold
    /// </summary>
    //----------------------------------//
    public void OnAboveFocusThreshold()
    //----------------------------------//
    {
        //Debug.Log("NeuroGuideInteractableDemo.cs // OnAboveFocusThreshold() // ");

        if(NeuroGuideFocusMeterExperience.system.currentLevel > 5)
        {
            NeuroGuideFocusMeterExperience.system.currentLevel = 5;
        }

        switch (NeuroGuideFocusMeterExperience.system.currentLevel)
        {
            case 0:
                objectRenderer.material.color = level1Color;
                SetSliderToStartValue();

                NeuroGuideFocusMeterExperience.system.currentLevel++;

                break;

            case 1:
                objectRenderer.material.color = level2Color;
                SetSliderToStartValue();

                NeuroGuideFocusMeterExperience.system.currentLevel++;

                break;

            case 2:
                objectRenderer.material.color = level3Color;
                SetSliderToStartValue();

                NeuroGuideFocusMeterExperience.system.currentLevel++;

                break;

            case 3:
                objectRenderer.material.color = level4Color;
                SetSliderToStartValue();

                NeuroGuideFocusMeterExperience.system.currentLevel++;

                break;

            case 4:
                objectRenderer.material.color = level5Color;
                SetSliderToStartValue();

                NeuroGuideFocusMeterExperience.system.currentLevel++;

                break;

            case 5:
                objectRenderer.material.color = level5Color;
                SetSliderToStartValue();

                break;
        }
    } //END OnAboveThreshold

    #endregion

    #region PUBLIC - ON BELOW FOCUS THRESHOLD

    /// <summary>
    /// Called when the score goes above the threshold, then falls back below it
    /// </summary>
    //--------------------------------//
    public void OnBelowFocusThreshold()
    //--------------------------------//
    {
        Debug.Log("NeuroGuideInteractableDemo // OnBelowFocusThreshold() //");

        NeuroGuideFocusMeterExperience.system.currentLevel--;

        switch (NeuroGuideFocusMeterExperience.system.currentLevel)
        {
            case 0:
                objectRenderer.material.color = level0Color;
                break;

            case 1:
                objectRenderer.material.color = level1Color;
                break;

            case 2:
                objectRenderer.material.color = level2Color;
                break;

            case 3:
                objectRenderer.material.color = level3Color;
                break;

            case 4:
                objectRenderer.material.color = level4Color;
                break;

            case 5:
                objectRenderer.material.color = level5Color;
                break;
        }
    } //END OnBelowThreshold Method

    #endregion

    #region PUBLIC - ON RECIEVING FOCUS REWARD UPDATE

    /// <summary>
    /// Called when the user start or stops recieving a reward
    /// </summary>
    /// <param name="isRecievingReward">Is the user recieving a reward?</param>
    //------------------------------------------------------------------------------------------//
    public void OnRecievingFocusRewardChanged(bool isRecievingReward)
    //------------------------------------------------------------------------------------------//
    {
        Debug.Log("NeuroGuideInteractableDemo.cs // OnRecievingFocusRewardChanged() // state = " + isRecievingReward.ToString());

    } //END OnRecievingRewardChanged Method

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

    #region PUBLIC - ON DATA FOCUS UPDATE

    /// <summary>
    /// Called when the NeuroGuideExperience updates the users progress in the experience
    /// </summary>
    /// <param name="normalizedValue">Progress, normalized 0-1 value</param>
    //------------------------------------------------------------------------------------------//
    public void OnFocusDataUpdate(float normalizedValue)
    //------------------------------------------------------------------------------------------//
    {
        if (cube == null)
            return;

        //Debug.Log( normalizedValue );
        
        if(slider != null)
        {
            slider.value = normalizedValue;
        }

#if EXT_DOTWEEN
        Vector3 scale = new Vector3(normalizedValue, normalizedValue, normalizedValue);
        cube.transform.DOScale(scale, .25f);
#endif

    } //END OnDataUpdate Method

    #endregion

    #region PRIVATE - SET SLIDER VALUE

    //--------------------------//
    private void SetSliderToStartValue()
    //-------------------------//
    {
        if (slider != null)
        {
            NeuroGuideFocusMeterExperience.system.hasReachedThreshold = true;
            NeuroGuideFocusMeterExperience.system.currentScore = NeuroGuideFocusMeterExperience.system.startMeterValue;
            //NeuroGuideFocusMeterExperience.system.hasReachedThreshold = false;
        }
    } // END SetSliderValue

    #endregion


} //END NeuroGuideInteractableDemo Class