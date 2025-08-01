
using System;
using UnityEngine;

namespace gambit.neuroguide
{

    /// <summary>
    /// Struct that stores data controlled by NeuroGuide hardware.
    /// Nullable struct, use HasValue() to determine if the struct is null or not before referencing its .Value
    /// </summary>
    public struct NeuroGuideData 
    {
        /// <summary>
        /// Is the user in the process of recieving a reward from the NeuroGear software?
        /// </summary>
        public bool isRecievingReward;

        /// <summary>
        /// Timestamp of when this data object was recieved/created
        /// </summary>
        public DateTime timestamp;

        /// <summary>
        /// Constructor for the NeuroGuideData struct
        /// </summary>
        /// <param name="_isRecievingReward"></param>
        /// <param name="_timestamp"></param>
        //----------------------------------------------------------------------------//
        public NeuroGuideData( bool _isRecievingReward, DateTime _timestamp )
        //----------------------------------------------------------------------------//
        {
            isRecievingReward = _isRecievingReward;
            timestamp = _timestamp;

        } //END NeuroGuideData Constructor Method

    } //END NeuroGuideData Class

} //END gambit.neuroguide Namespace