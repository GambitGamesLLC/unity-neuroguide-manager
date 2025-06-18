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
        /// Called 60 times a second by the NeuroGuideExperience with the latest normalized value of how far the user is from reaching the end goal of the experience
        /// </summary>
        /// <param name="system">The current normalized value (0-1) of how far we are in the NeuroGuide experience</param>
        void OnDataUpdate( float normalizedValue );

    } //END INeuroGuideInteractable Interface Class

}