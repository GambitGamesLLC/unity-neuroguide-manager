using System.Collections.Generic;

#if EXT_DOTWEEN
using DG.Tweening;
using UnityEngine;
#endif

namespace gambit.neuroguide
{

    /// <summary>
    /// Stores data controlled by NeuroGuide hardware
    /// </summary>
    public class NeuroGuideData : MonoBehaviour
    {
        /// <summary>
        /// Unique identifier of the sensor
        /// </summary>
        public string sensorID;

        /// <summary>
        /// List of raw data from the sensor
        /// </summary>
        public List<float> rawData;

        /// <summary>
        /// The current value from the sensor, not normalized
        /// </summary>
        public float currentValue = 0f;

        /// <summary>
        /// The current value from the sensor, normalized between 0-1
        /// </summary>
        public float currentNormalizedValue = 0f;

#if EXT_DOTWEEN
        /// <summary>
        /// Used to tween the data
        /// </summary>
        public Tween activeTween;
#endif

    } //END NeuroGuideData Class

} //END gambit.neuroguide Namespace