using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/*
 * Contains info about a (VR) Avatar to be used by other scripts.
 * Can be assigned manually, or populated automatically - provided it has the same Skeleton names as the Didimo Avatars
 *
 * author:
 * max@senseglove.com
 */

namespace Presence
{
    public class PHap_DidimoAvatarInfo : MonoBehaviour
    {

        /// <summary> Optional component to quickly add Avatar transforms to the list. </summary>
        [Header("Optional for auto-population")]
        public Transform avatarBaseTransform;

        [Header("Avatar Transforms")]
        public Transform neckTransform;
        public Transform torsoTransform;

        [Header("Left Side")]
        public Transform leftUpperArm;
        public Transform leftLowerArm;
        public Transform leftPalm;
        public Transform leftThumb;
        public Transform leftIndexFinger;
        public Transform leftMiddleFinger;
        public Transform leftRingFinger;
        public Transform leftPinkyFinger;

        public Transform leftUpperLeg;
        public Transform leftLowerLeg;


        [Header("Right Side")]
        public Transform rightUpperArm;
        public Transform rightLowerArm;

        public Transform rightPalm;
        public Transform rightThumb;
        public Transform rightIndexFinger;
        public Transform rightMiddleFinger;
        public Transform rightRingFinger;
        public Transform rightPinkyFinger;

        public Transform rightUpperLeg;
        public Transform rightLowerLeg;



        public Transform GetRelevantTransform(PHap_ColliderLink collider)
        {
            return GetRelevantTransform(collider.linkToBodyPart);
        }

        public Transform GetRelevantTransform(Presence.PHap_BodyPart bodyPart)
        {
            switch (bodyPart)
            {
                case Presence.PHap_BodyPart.LeftHead:
                case Presence.PHap_BodyPart.RightHead:
                    return neckTransform;

                case Presence.PHap_BodyPart.Torso:
                case PHap_BodyPart.LeftChest:
                case PHap_BodyPart.RightChest:
                    return torsoTransform;

                case Presence.PHap_BodyPart.LeftUpperArm:
                    return leftUpperArm;
                case Presence.PHap_BodyPart.LeftLowerArm:
                    return leftLowerArm;
                case Presence.PHap_BodyPart.LeftHandPalm:
                    return leftPalm;
                case Presence.PHap_BodyPart.LeftThumb:
                    return leftThumb;
                case Presence.PHap_BodyPart.LeftIndexFinger:
                    return leftIndexFinger;
                case Presence.PHap_BodyPart.LeftMiddleFinger:
                    return leftMiddleFinger;
                case Presence.PHap_BodyPart.LeftRingFinger:
                    return leftRingFinger;
                case Presence.PHap_BodyPart.LeftPinky:
                    return leftPinkyFinger;
                case Presence.PHap_BodyPart.LeftUpperLeg:
                    return leftUpperLeg;
                case Presence.PHap_BodyPart.LeftLowerLeg:
                    return leftLowerLeg;

                case Presence.PHap_BodyPart.RightUpperArm:
                    return rightUpperArm;
                case Presence.PHap_BodyPart.RightLowerArm:
                    return rightLowerArm;
                case Presence.PHap_BodyPart.RightHandPalm:
                    return rightPalm;
                case Presence.PHap_BodyPart.RightThumb:
                    return rightThumb;
                case Presence.PHap_BodyPart.RightIndexFinger:
                    return rightIndexFinger;
                case Presence.PHap_BodyPart.RightMiddleFinger:
                    return rightMiddleFinger;
                case Presence.PHap_BodyPart.RightRingFinger:
                    return rightRingFinger;
                case Presence.PHap_BodyPart.RightPinky:
                    return rightPinkyFinger;
                case Presence.PHap_BodyPart.RightUpperLeg:
                    return rightUpperLeg;
                case Presence.PHap_BodyPart.RightLowerLeg:
                    return rightLowerLeg;

                default:
                    return null;
            }
        }


        public void PopulateAvatar()
        {
            if (avatarBaseTransform == null)
            {
                //Debug.LogError("An AvatarBaseTransform is required to auto-assign body parts.", this);
                return;
            }

            System.StringComparison comp = System.StringComparison.OrdinalIgnoreCase;
            neckTransform = FindChildRecursive(avatarBaseTransform, "Neck", comp);
            torsoTransform = FindChildRecursive(avatarBaseTransform, "Spine", comp);

            leftUpperArm = FindChildRecursive(avatarBaseTransform, "Left_UpperArm", comp);
            leftLowerArm = FindChildRecursive(avatarBaseTransform, "Left_LowerArm", comp);
            leftPalm = FindChildRecursive(avatarBaseTransform, "Left_Hand", comp);
            leftThumb = FindChildRecursive(avatarBaseTransform, "Left_ThumbDistal", comp);
            leftIndexFinger = FindChildRecursive(avatarBaseTransform, "Left_IndexDistal", comp);
            leftMiddleFinger = FindChildRecursive(avatarBaseTransform, "Left_MiddleDistal", comp);
            leftRingFinger = FindChildRecursive(avatarBaseTransform, "Left_RingDistal", comp);
            leftPinkyFinger = FindChildRecursive(avatarBaseTransform, "Left_PinkyDistal", comp);
            leftUpperLeg = FindChildRecursive(avatarBaseTransform, "Left_UpperLeg", comp);
            leftLowerLeg = FindChildRecursive(avatarBaseTransform, "Left_LowerLeg", comp);

            rightUpperArm = FindChildRecursive(avatarBaseTransform, "Right_UpperArm", comp);
            rightLowerArm = FindChildRecursive(avatarBaseTransform, "Right_LowerArm", comp);
            rightPalm = FindChildRecursive(avatarBaseTransform, "Right_Hand", comp);
            rightThumb = FindChildRecursive(avatarBaseTransform, "Right_ThumbDistal", comp);
            rightIndexFinger = FindChildRecursive(avatarBaseTransform, "Right_IndexDistal", comp);
            rightMiddleFinger = FindChildRecursive(avatarBaseTransform, "Right_MiddleDistal", comp);
            rightRingFinger = FindChildRecursive(avatarBaseTransform, "Right_RingDistal", comp);
            rightPinkyFinger = FindChildRecursive(avatarBaseTransform, "Right_PinkyDistal", comp);
            rightUpperLeg = FindChildRecursive(avatarBaseTransform, "Right_UpperLeg", comp);
            rightLowerLeg = FindChildRecursive(avatarBaseTransform, "Right_LowerLeg", comp);

#if UNITY_EDITOR
            EditorUtility.SetDirty(this); //ensures that the new variable are stored!
#endif
        }


        Transform FindChildRecursive(Transform parent, string name, System.StringComparison comparison)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Equals(name, comparison))
                    return child;

                Transform result = FindChildRecursive(child, name, comparison);
                if (result != null)
                    return result;
            }
            return null;
        }


#if UNITY_EDITOR
        private Transform previousAvatarRoot = null;
        private void OnValidate()
        {
            if (previousAvatarRoot != this.avatarBaseTransform)
            {
                previousAvatarRoot = this.avatarBaseTransform;
                PopulateAvatar();
            }

        }
#endif


    }





#if UNITY_EDITOR

    [CustomEditor(typeof(PHap_DidimoAvatarInfo))] // This binds the custom inspector to the Effect class
    public class PHap_DidimoAvatarInfoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            PHap_DidimoAvatarInfo script = (PHap_DidimoAvatarInfo)target;

            EditorGUI.BeginChangeCheck();

            GUILayout.Label("Auto-Detection", UnityEditor.EditorStyles.boldLabel);

            if (GUILayout.Button("Find Body Parts"))
            {
                if (script.avatarBaseTransform != null)
                    script.PopulateAvatar();
                else
                    Debug.LogError("An AvatarBaseTransform is required to auto-assign body parts.", script);
            }

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