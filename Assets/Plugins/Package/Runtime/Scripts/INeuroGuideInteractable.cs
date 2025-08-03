/**************************************************
 * INeuroGuideInteractable
 * 
 * Interface class that contains methods to respond to NeuroGuide experience events called by the NeuroGuideExperience singleton
 ***************************************************/

using UnityEngine;

namespace gambit.neuroguide
{

    /// <summary>
    /// Interface class, contains methods to respond to NeuroGuide hardware events
    /// </summary>
    public interface INeuroGuideInteractable
    {
        /// <summary>
        /// Called when the user gets their score above the threshold value in the experience.
        /// When this happens, this callback will be prevented until the user falls back below the threshold
        /// and a set amount of time has passed, configurable in the NeuroGuideExperience Options object
        /// </summary>
        void OnAboveThreshold();

        /// <summary>
        /// Called when the user gets their score above the threshold value in the experience, then
        /// the score falls below the threshold
        /// </summary>
        void OnBelowThreshold();

        /// <summary>
        /// Called when the user starts or stops recieving a reward from the NeuroGuide hardware
        /// </summary>
        /// <param name="isRecievingReward">Is the user currently recieving a reward?</param>
        void OnRecievingRewardChanged( bool isRecievingReward );

        /// <summary>
        /// Called 60 times a second by the NeuroGuideExperience with the latest normalized value of how far the user is from reaching the end goal of the experience
        /// </summary>
        /// <param name="system">The current normalized value (0-1) of how far we are in the NeuroGuide experience</param>
        void OnDataUpdate( float normalizedValue );

    } //END INeuroGuideInteractable Interface Class

}