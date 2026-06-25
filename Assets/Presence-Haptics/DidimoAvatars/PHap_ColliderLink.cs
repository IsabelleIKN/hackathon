using Presence;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/*
 * Contains offset information between this collider and the one it's linked to, so it may be parented at runtime.
 * TODO: have this done during the Editor as well?
 * 
 * author
 * max@senseglove.com
 */

namespace Presence
{

    public class PHap_ColliderLink : MonoBehaviour
    {
        public Transform linkedTo;

        public Vector3 positionOffset = Vector3.zero;
        public Quaternion rotationOffset = Quaternion.identity;

        public PHap_BodyPart linkToBodyPart = PHap_BodyPart.Unknown;

        public bool activateOnAwake = false;

        private MeshRenderer visualization = null;

        public void LinkTo(Transform obj, bool recalculateOffsets = true)
        {
            linkedTo = obj;
            if (recalculateOffsets)
                CalculateOffsets();
        }

        /// <summary> Sets this Transform as a child of linkedTo, at the precviously determined offsets </summary>
        public void SetAsChild()
        {
            if (linkedTo == null)
                return;

            this.transform.parent = linkedTo;
            Vector3 myPos; Quaternion myRot;
            SG.Util.SG_Util.CalculateTargetLocation(linkedTo, positionOffset, rotationOffset, out myPos, out myRot);
            this.transform.rotation = myRot;
            this.transform.position = myPos;
        }


        public void CalculateOffsets()
        {
            if (linkedTo != null)
                SG.Util.SG_Util.CalculateOffsets(this.transform, linkedTo, out positionOffset, out rotationOffset);
        }


        public void UpdateLocation()
        {
            if (linkedTo == null)
                return;
            Vector3 myPos; Quaternion myRot;
            SG.Util.SG_Util.CalculateTargetLocation(linkedTo, positionOffset, rotationOffset, out myPos, out myRot);
            this.transform.rotation = myRot;
            this.transform.position = myPos;
        }

        public void SetShowCollider(bool value)
        {
            if (this.visualization == null)
                this.visualization = this.gameObject.GetComponent<MeshRenderer>();

#if UNITY_EDITOR
            // Defer to the next editor update
            if (Application.isPlaying)
            {
                if (visualization != null)
                    this.visualization.enabled = value;
            }
            else
            {
                EditorApplication.delayCall += () =>
                {
                    if (visualization != null)
                        this.visualization.enabled = value;
                };
            }

#else
        if (visualization != null)
            this.visualization.enabled = value;
#endif

        }

        private void Awake()
        {
            if (activateOnAwake)
                SetAsChild();
        }

    }



#if UNITY_EDITOR

    [CustomEditor(typeof(PHap_ColliderLink))] // This binds the custom inspector to the Effect class
    public class PHap_ColliderLinkEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            PHap_ColliderLink script = (PHap_ColliderLink)target;

            DrawDefaultInspector();
            UnityEditor.EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Calculate Offsets"))
                script.CalculateOffsets();

            if (GUI.changed)
                EditorUtility.SetDirty(target);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(script);
        }


    }

#endif
}