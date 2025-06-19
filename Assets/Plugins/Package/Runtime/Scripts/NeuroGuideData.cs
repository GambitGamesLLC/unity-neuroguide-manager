
using UnityEngine;

namespace gambit.neuroguide
{

    /// <summary>
    /// Stores data controlled by NeuroGuide hardware
    /// </summary>
    public class NeuroGuideData : ScriptableObject
    {
        /// <summary>
        /// Is the user in the process of recieving a reward from the NeuroGear software?
        /// </summary>
        public bool isRecievingReward;

    } //END NeuroGuideData Class

} //END gambit.neuroguide Namespace