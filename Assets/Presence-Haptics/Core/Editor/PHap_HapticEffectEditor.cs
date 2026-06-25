using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Presence
{
    [CustomEditor(typeof(PHap_HapticEffect))] // This binds the custom inspector to the Effect class
    public class PHap_HapticEffectEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            UnityEditor.EditorGUILayout.Space();

            PHap_HapticEffect effectScript = (PHap_HapticEffect)target;

            if (effectScript.BaseEffect == null)
            {
                EditorGUILayout.HelpBox("No BaseEffect Assigned. This effect will not play!", MessageType.Error);
            }
            else
            {
                PHap_HapticModality effectType = effectScript.BaseEffect.GetEffectType();
                EditorGUILayout.LabelField("Effect Type", effectType.ToString());
                if (effectType == PHap_HapticModality.Unknown || PHap_Core.IsTimedEffect(effectType))
                {
                    EditorGUILayout.LabelField("Base Duration", effectScript.BaseDuration.ToString("0.00"));
                    //this is a timed effect, so it's relevant for us to show these parameters
                    string durationString = effectScript.IsLooping ? "Plays until stopped" : effectScript.TotalDuration.ToString("0.00");
                    EditorGUILayout.LabelField("Total Duration", durationString);
                }
                else
                {
                    //Message about non-timed effect.
                    EditorGUILayout.HelpBox(effectType.ToString() + " are not timed effects. RepeatAmount and Looping will not affect it. Play / Stop should be used to enable / disable it.", MessageType.Info);
                }
            }
        }
    }
}