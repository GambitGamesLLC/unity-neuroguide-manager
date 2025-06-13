/**************************************************
 * INeuroGuideInteractable
 * 
 * Interface class that contains methods to respond to NeuroGuide hardware events called by the NeuroGuideManager
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
        /// Called when the NeuroGuide hardware updates with new data.
        /// </summary>
        /// <param name="system">The current NeuroGuideSystem object that stores up to date info about the NeuroGuide hardware</param>
        void OnDataUpdate( NeuroGuideManager.NeuroGuideSystem system );

    } //END INeuroGuideInteractable Interface Class

}