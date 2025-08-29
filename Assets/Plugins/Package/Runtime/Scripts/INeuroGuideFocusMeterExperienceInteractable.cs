using UnityEngine;

namespace gambit.neuroguide
{
    /// <summary>
    /// Interface class, contains methods to respond to NeuroGuide hardware relating to user's focus
    /// </summary>
    public interface INeuroGuideFocusMeterExperienceInteractable
    {
        
        /// <summary>
        /// Called when the user gets their score above the threshold value in the experience.
        /// When this happens, this callback will be prevented until the user falls back below the threshold
        /// and a set amount of time has passed, configurable in the NeuroGuideExperience Options object
        /// </summary>
        void OnAboveFocusThreshold();

        /// <summary>
        /// Called when the user gets their score above the threshold value in the experience, then
        /// the score falls below the threshold
        /// </summary>
        void OnBelowFocusThreshold();

        /// <summary>
        /// Called when the user starts or stops recieving a reward from the NeuroGuide hardware
        /// </summary>
        /// <param name="isRecievingReward">Is the user currently recieving a reward?</param>
        void OnRecievingFocusRewardChanged(bool isRecievingReward);

        /// <summary>
        /// Called 60 times a second by the NeuroGuideExperience with the latest normalized value of how far the user is from reaching the end goal of the experience
        /// </summary>
        /// <param name="system">The current normalized value (0-1) of how far we are in the NeuroGuide experience</param>
        void OnFocusDataUpdate(float normalizedValue);

    } //END INeuroGuideFocusExperienceInteractable Interface Class
}