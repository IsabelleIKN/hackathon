using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/*
 * Determines wiich of two 'layers' to enable, and which one to destroy, based on whether or not a user is a local client or not.
 * Optimized for Didimo Avatar Template
 * 
 * author
 * max@senseglove.com
 */

namespace Presence
{

    public class PHap_DidimoColliderSelection : MonoBehaviour
    {
        public bool isLocalPlayer = true;

        public bool destroyUnusedAssets = true;

        public PHap_DidmoAvatarHaptics emitterLayer;
        public PHap_DidmoAvatarHaptics receiverLayer;

        public Component[] localPlayerAssets = new Component[0];
        public Component[] remoteAvatarAssets = new Component[0];

        // Start is called before the first frame update
        void Start()
        {
            //Enable / disable the two layers
            if (destroyUnusedAssets)
            {
                if (isLocalPlayer && emitterLayer != null)
                {
                    emitterLayer.RemoveAssetsAtRuntime(); //removed the assets that might have been children, so they no longer take up any space.
                    GameObject.Destroy(emitterLayer.gameObject);
                }
                if (!isLocalPlayer && receiverLayer != null)
                {
                    receiverLayer.RemoveAssetsAtRuntime();
                    GameObject.Destroy(receiverLayer.gameObject);
                }
            }
            emitterLayer?.gameObject.SetActive(!isLocalPlayer);
            receiverLayer?.gameObject.SetActive(isLocalPlayer);


            //and clean up other assets, ensure others are enabled?
            if (isLocalPlayer)
            {
                foreach (Component component in localPlayerAssets)
                    EnableComponent(component);
                foreach (Component component in remoteAvatarAssets)
                    DisableComponent(component);
            }
            else
            {
                foreach (Component component in localPlayerAssets)
                    DisableComponent(component);
                foreach (Component component in remoteAvatarAssets)
                    EnableComponent(component);
            }

        }

        private void DisableComponent(Component component)
        {
            if (component == null)
                return;

            if (component is Transform)
            {
                if (destroyUnusedAssets)
                    GameObject.Destroy(component.gameObject);
                else
                    component.gameObject.SetActive(false);
                return;
            }

            if (destroyUnusedAssets)
                GameObject.Destroy(component);
            else //disables any component that can be enabled / disabled.
            {
                //slow, but we're only using it for setup, so  we might as well.
                var type = component.GetType();
                var enabledProp = type.GetProperty("enabled");
                if (enabledProp != null && enabledProp.CanWrite)
                {
                    enabledProp.SetValue(component, false);
                }
            }
        }

        private void EnableComponent(Component component)
        {
            if (component == null)
                return;

            if (component is Transform)
            {
                component.gameObject.SetActive(true);
                return;
            }

            //slow, but we're only using it for setup, so  we might as well.
            var type = component.GetType();
            var enabledProp = type.GetProperty("enabled");
            if (enabledProp != null && enabledProp.CanWrite)
            {
                enabledProp.SetValue(component, true); // or false
            }
        }

        public void CalculateOffsets()
        {
            if (this.emitterLayer != null)
                this.emitterLayer.CalculateOffsets();
            if (this.receiverLayer != null)
                this.receiverLayer.CalculateOffsets();
        }

    }





#if UNITY_EDITOR

    [CustomEditor(typeof(PHap_DidimoColliderSelection))] // This binds the custom inspector to the Effect class
    public class PHap_DidimoColliderSelectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            PHap_DidimoColliderSelection script = (PHap_DidimoColliderSelection)target;

            EditorGUI.BeginChangeCheck();

            GUILayout.Label("Auto-Detection", UnityEditor.EditorStyles.boldLabel);

            if (GUILayout.Button("Calculate Offsets"))
                script.CalculateOffsets();

            UnityEditor.EditorGUILayout.Space();


            DrawDefaultInspector();
            if (GUI.changed)
                EditorUtility.SetDirty(target);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(script);
        }


    }

#endif
}