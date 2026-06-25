using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Presence
{


    [CreateAssetMenu(fileName = "StiffnessEffect", menuName = "SenseGlove/Stiffness", order = 1)]
    public class SG_PremadeStiffness : SG_ForceResponse
    {
        /// <summary> Evaluates the force level at a particular flexion </summary>
        /// <param name="flexion01"></param>
        /// <returns></returns>
        public float GetCurrentForceLevel(float flexion01, float scaleFactor)
        {
            if (flexion01 < 0.0001f) //less than 0 flexion == no ffb
                return 0.0f;
            return this.forceResponse.Evaluate(flexion01) * scaleFactor;
        }


    }
}