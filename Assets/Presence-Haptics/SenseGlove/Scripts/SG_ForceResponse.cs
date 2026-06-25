using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Represents a force (Newton over time) to be used inside the Presence Haptics API
 * 
 * TODO: Support Stiffness (possibly as a different script, or integrated into this one)
 * TODO: Add a way to add something other than a constant force usign AnimationCurves, for example.
 * 
 * author:
 * max@senseglove.com
 */

/// <summary> Force == N / s, Striffness if N / m (distance / flexion based) </summary>
//[CreateAssetMenu(fileName = "ForceEffect", menuName = "Presence/SenseGlove/ForceEffect", order = 1)] //this will eventually be replaced by OnImport functions
public abstract class SG_ForceResponse : ScriptableObject
{
    //For now, I'll work with ... 2 keypoints: Start and end. They'll have an X and Y component to control, which is visualized in an AnimationCurve.

    [Range(0.0f, 1.0f)] public float start = 0.0f;
    [Range(0.0f, 1.0f)] public float forceAtStart = 1.0f;

    [Range(0.0f, 1.0f)] public float end = 1.0f;
    [Range(0.0f, 1.0f)] public float forceAtEnd = 1.0f;

    public AnimationCurve forceResponse = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);


#if UNITY_EDITOR

    private void OnValidate()
    {
        float inTan = 0.0f, outTan = 0.0f;
        float inW = 0.0f, outW = 0.0f;
        List<Keyframe> frames = new List<Keyframe>();
        if (start > 0.0f)
            frames.Add( new Keyframe(0.0f, forceAtStart, inTan, outTan, inW, outW) );

        frames.Add( new Keyframe(start, forceAtStart, inTan, outTan, inW, outW) );
        frames.Add( new Keyframe(end, forceAtEnd, inTan, outTan, inW, outW) );

        if (end < 1.0f)
            frames.Add( new Keyframe(1.0f, forceAtEnd, inTan, outTan, inW, outW) );

        forceResponse.keys = frames.ToArray();
    }

#endif
}
