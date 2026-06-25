/*
 * Tests a PHap_HapticEffect with a button and some simulated location parameters.
 * 
 * author:
 * max@senseglove.com
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Presence
{

    public class PHap_EffectTester : MonoBehaviour
    {
        public PHap_HapticEffect effectToTest;

        [Header("Location Parameters")]
        public PHap_BodyPart bodyPart = PHap_BodyPart.Unknown;

        public Vector3 effectPosition = Vector3.zero;

        public float effectScale = 1.0f;

        public void PlayEffect()
        {
            if (effectToTest == null )
                return;
            //TODO: validate other variables?
            effectToTest.PlayEffect(new PHap_EffectLocation(bodyPart, effectPosition, effectScale));
        }

        public void StopEffect()
        {
            if (effectToTest == null || !Application.isPlaying)
                return;
            effectToTest.StopEffect(new PHap_EffectLocation(bodyPart, effectPosition, effectScale));
        }

        public override string ToString()
        {
            return bodyPart.ToString() + " - " + effectPosition.ToString() + ", " + effectScale.ToString("0.00");
        }

        private void Reset()
        {
            if (this.effectToTest == null)
                this.effectToTest = this.GetComponent<PHap_HapticEffect>();
        }

    }


#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(PHap_EffectTester))]
    [UnityEditor.CanEditMultipleObjects]
    public class PHap_EffectTesterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            PHap_EffectTester tester = (PHap_EffectTester)target;

            DrawDefaultInspector();
            UnityEditor.EditorGUILayout.Space();
            GUILayout.Label("Controls (Play Mode Only)", UnityEditor.EditorStyles.boldLabel);
            if (Application.isPlaying) //limiting this to play mode only for now so that looping etc is still possible.
            {
                if (GUILayout.Button("Play Effect"))
                {
                    tester.PlayEffect();
                }
                if (GUILayout.Button("Stop Effect"))
                {
                    tester.StopEffect();
                }
            }
        }
    }
#endif

}
