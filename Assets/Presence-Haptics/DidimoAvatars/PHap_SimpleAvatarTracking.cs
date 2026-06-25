#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/*
 * A script to move an avatar's head and torso according to Main Camera movement.
 * Places the head at the desired camera position & rotation. Then places the torso underneath the neck in a somewhat believable rotation.
 * 
 * author
 * max@senseglove.com
 */

namespace Presence
{

    public class PHap_SimpleAvatarTracking : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        // Member Variables.

        public Transform mainCamera;

        public Transform neckTransform;

        public Transform torsoTransform;


        //neck <> camera
        public Vector3 camNeckOffset_p = Vector3.zero;
        public Quaternion camNeckOffset_r = Quaternion.identity;


        //neck <> head
        private Vector3 neckTorsoOffset_p = Vector3.zero;
        private Quaternion neckTorsoOffset_r = Quaternion.identity;


        //-----------------------------------------------------------------------------------------------------------------------------
        // Tracking Scripts

        private void CalculateOffsets()
        {
            SG.Util.SG_Util.CalculateOffsets(neckTransform, torsoTransform, out neckTorsoOffset_p, out neckTorsoOffset_r);
        }

        private void UpdateHeadAndTorso(float dT)
        {
            //moves the head transform such that its global position & rotation euals that of the cam + offset.
            Vector3 newNeckPos; Quaternion newNeckRot;
            SG.Util.SG_Util.CalculateTargetLocation(mainCamera, camNeckOffset_p, camNeckOffset_r, out newNeckPos, out newNeckRot);

            //base the 'forward rotation' on the angle of the head's 'right' position...?
            Vector3 camRight = mainCamera.rotation * Vector3.right; //using right so you can look up/down without it affecting the tracking
            float relativeAngle = Mathf.Atan2(camRight.z, camRight.x) * Mathf.Rad2Deg;

            //now that we know where the neck is, let's try to point the torso forward. the result is a quaternion rotation
            //Quaternion torsoTargetRotation = torsoTransform.rotation;
            Quaternion torsoTargetRotation = Quaternion.Euler(0, -relativeAngle, 0); ;

            //the torso's position must be placed in such a way that, in it's current rotation, it ends up the same dxyz from the neck as in the begining.
            Vector3 torsoPositon = newNeckPos - (torsoTargetRotation * neckTorsoOffset_p);

            //Apply torso first (it affects the neck), then apply the neck. TODO: Make this configurable?
            torsoTransform.rotation = torsoTargetRotation; //in 3D
            torsoTransform.position = torsoPositon;

            neckTransform.rotation = newNeckRot;
            neckTransform.position = newNeckPos;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Monobehaviour

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main != null ? Camera.main.transform : null;
            CalculateOffsets();
        }



        // Update is called once per frame
        void Update()
        {
            UpdateHeadAndTorso(Time.deltaTime);
        }
    }


#if UNITY_EDITOR

    [CustomEditor(typeof(PHap_SimpleAvatarTracking))] // This binds the custom inspector to the Effect class
    public class SG_SimpleAvatarTrackingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            PHap_SimpleAvatarTracking script = (PHap_SimpleAvatarTracking)target;

            DrawDefaultInspector();
            UnityEditor.EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Calculate Camera Offset"))
            {
                SG.Util.SG_Util.CalculateOffsets(script.neckTransform, script.mainCamera, out script.camNeckOffset_p, out script.camNeckOffset_r);
            }

            if (GUI.changed)
                EditorUtility.SetDirty(target);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(script);
        }


    }

#endif
}