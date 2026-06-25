using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Presence
{
    [CreateAssetMenu(fileName = "ForceEffect", menuName = "SenseGlove/ForceEffect", order = 1)]
    public class SG_PremadeForce : SG_ForceResponse
    {
        [Header("Effect Duraction (seconds)")]
        public float effectDuration = 0.1f; //100ms?

        /// <summary> Evaluates the force level at a particular time... </summary>
        /// <param name="timeInSeconds"></param>
        /// <returns></returns>
        public float GetCurrentForceLevel(float timeInSeconds, float scaleFactor)
        {
            if (effectDuration < 0.0001f)
                return 0.0f;
            if (timeInSeconds > effectDuration)
                return 0.0f; //ended...?

            float normalizedTime = timeInSeconds / effectDuration; //place this between 0.0f and 1.0f to evaluate on the 0.0f, 1.0f scale.
            return this.forceResponse.Evaluate(normalizedTime) * scaleFactor;
        }

    }
}