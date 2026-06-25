#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/*
 * Links multiple colliders to an avatar via the AvatarInfo. 
 * Can be used in two ways; 
 * SetAsChildren - On start, the colliders are set as Children to their relevant transforms
 * UpdateManually - Updates all collider positions duing Update. Use this when you have one single RigidBody to control collisions between multiple colliders.
 * 
 * author 
 * max@senseglove.com
 */

namespace Presence
{


    public class PHap_DidmoAvatarHaptics : MonoBehaviour
    {
        public enum ColliderSetup
        {
            UpdateManually,
            SetAsChildren,
        }

        [SerializeField] private bool showColliders = false;

        public ColliderSetup colliderMode = ColliderSetup.UpdateManually;

        public PHap_DidimoAvatarInfo avatarInfo;

        /// <summary> Each collider to link to the  </summary>
        public PHap_ColliderLink[] collidersToLink = new PHap_ColliderLink[0];
        private bool setChildrenRuntime = true;

        private void EstablishLinks(bool recalculateOffsets)
        {
            if (avatarInfo == null)
            {
                avatarInfo = gameObject.GetComponent<PHap_DidimoAvatarInfo>();
                if (avatarInfo == null) //still NULL :(
                {
                    Debug.LogError("This PHap_DidmoAvatarHaptics script needs to be linked to a PHap_DidimoAvatarInfo script!", this);
                    return;
                }
            }
            foreach (PHap_ColliderLink collider in collidersToLink)
            {
                Transform linkedBodyPart = avatarInfo.GetRelevantTransform(collider);
                if (linkedBodyPart != null)
                {
                    collider.LinkTo(linkedBodyPart, recalculateOffsets); //TODO: Re-calculate offsets
#if UNITY_EDITOR
                    EditorUtility.SetDirty(collider); //ensures it is updated from the editor.
#endif
                }
            }
        }


        public void CalculateOffsets()
        {
            EstablishLinks(true);
        }

        public void SetAsChildren()
        {
            EstablishLinks(true);
            foreach (PHap_ColliderLink collider in collidersToLink)
            {
                collider.SetAsChild();
            }
        }

        public void ReturnColliders()
        {
            Transform mine = this.transform;
            foreach (PHap_ColliderLink collider in collidersToLink)
            {
                collider.transform.parent = mine;
            }
        }


        public void PopulareColliders()
        {
            collidersToLink = this.GetComponentsInChildren<PHap_ColliderLink>();
        }


        public void SetShowColliders(bool value)
        {
            foreach (PHap_ColliderLink collider in collidersToLink)
            {
                collider.SetShowCollider(value);
            }
        }


        public void RemoveAssetsAtRuntime()
        {
            if (Application.isPlaying)
            {
                for (int i = 0; i < this.collidersToLink.Length; i++)
                {
                    if (collidersToLink[i] != null)
                        GameObject.Destroy(collidersToLink[i].gameObject);
                }
                collidersToLink = new PHap_ColliderLink[0];
            }
        }

        private void Update()
        {
            if (colliderMode == ColliderSetup.UpdateManually)
            {
                foreach (PHap_ColliderLink collider in collidersToLink)
                {
                    collider.UpdateLocation();
                }
            }
            else if (colliderMode == ColliderSetup.SetAsChildren && setChildrenRuntime)
            {
                setChildrenRuntime = false;
                SetAsChildren();
            }
        }

#if UNITY_EDITOR
        private bool previousShowColliders = false;
        private void OnValidate()
        {
            if (previousShowColliders != this.showColliders)
            {
                previousShowColliders = this.showColliders;
                SetShowColliders(showColliders);

            }
        }
#endif

    }



#if UNITY_EDITOR

    [CustomEditor(typeof(PHap_DidmoAvatarHaptics))] // This binds the custom inspector to the Effect class
    public class PHap_DidmoAvatarHapticsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            PHap_DidmoAvatarHaptics script = (PHap_DidmoAvatarHaptics)target;

            EditorGUI.BeginChangeCheck();

            GUILayout.Label("Auto-Detection", UnityEditor.EditorStyles.boldLabel);

            if (GUILayout.Button("Find Colliders"))
                script.PopulareColliders();

            GUILayout.Label("Collider Setup", UnityEditor.EditorStyles.boldLabel);

            if (GUILayout.Button("Calculate Offsets"))
                script.CalculateOffsets();

            if (GUILayout.Button("Set Colliders as Children"))
                script.SetAsChildren();

            if (GUILayout.Button("Return Colliders"))
                script.ReturnColliders();

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