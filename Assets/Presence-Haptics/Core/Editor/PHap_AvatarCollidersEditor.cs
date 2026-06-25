using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Presence
{
    [CustomEditor(typeof(PHap_AvatarColliders))] // This binds the custom inspector to the Effect class
    public class PHap_AvatarCollidersEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            UnityEditor.EditorGUILayout.Space();
            GUILayout.Label("Help functions:", UnityEditor.EditorStyles.boldLabel);

            PHap_AvatarColliders colliderScript = (PHap_AvatarColliders)target;
            if (GUILayout.Button("Detect Emitters"))
            {
                PHap_HapticEmitter[] emitters = colliderScript.GetComponentsInChildren<PHap_HapticEmitter>(true);
                foreach (PHap_HapticEmitter emit in emitters)
                {
                    colliderScript.TryAddEmitter(emit);
                }
            }
            if (GUILayout.Button("Detect Receivers"))
            {
                PHap_HapticReceiver[] emitters = colliderScript.GetComponentsInChildren<PHap_HapticReceiver>(true);
                foreach (PHap_HapticReceiver emit in emitters)
                {
                    colliderScript.TryAddReceiver(emit);
                }
            }

        }
    }
}