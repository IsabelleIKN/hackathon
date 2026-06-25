using UnityEngine;

/*
 * A simple class to collect or assign Didimo Hand Bone Transforms. Re-used between the Animator and Mapper.
 * 
 * author:
 * max@senseglove.com
 */

public class PHap_DidimoHandInfo : MonoBehaviour
{
    /// <summary> Tells me which names for the bones will be used for auto detection. </summary>
    public enum BoneLayout
    {
        Default, //the ones orignally submitted by didimo
        OpenXR,  //the ones where the hand model has been updated for OpenXR 
    }

    public BoneLayout layout = BoneLayout.Default;
    public bool rightHand = true; //mostly useful for auto-detection

    public Transform avatarWrist;

    public Transform avatarThumbCMC;
    public Transform avatarThumbMCP;
    public Transform avatarThumbIP;
    public Transform avatarThumbTip;

    public Transform avatarIndexMCP;
    public Transform avatarIndexPIP;
    public Transform avatarIndexDIP;
    public Transform avatarIndexTIP;

    public Transform avatarMiddleMCP;
    public Transform avatarMiddlePIP;
    public Transform avatarMiddleDIP;
    public Transform avatarMiddleTIP;

    public Transform avatarRingMCP;
    public Transform avatarRingPIP;
    public Transform avatarRingDIP;
    public Transform avatarRingTIP;

    public Transform avatarPinkyMCP;
    public Transform avatarPinkyPIP;
    public Transform avatarPinkyDIP;
    public Transform avatarPinkyTIP;

    private Transform[] fingerTransforms = null;


    public Transform[] GetFingerTransforms()
    {
        if (fingerTransforms == null || fingerTransforms.Length == 0)
        {
            fingerTransforms = new Transform[]
            {
                avatarWrist,

                avatarThumbCMC,
                avatarThumbMCP,
                avatarThumbIP,
                avatarThumbTip,

                avatarIndexMCP,
                avatarIndexPIP,
                avatarIndexDIP,
                avatarIndexTIP,

                avatarMiddleMCP,
                avatarMiddlePIP,
                avatarMiddleDIP,
                avatarMiddleTIP,

                avatarRingMCP,
                avatarRingPIP,
                avatarRingDIP,
                avatarRingTIP,

                avatarPinkyMCP,
                avatarPinkyPIP,
                avatarPinkyDIP,
                avatarPinkyTIP
            };
        }
        return fingerTransforms;
    }



    //TODO: Auto Detect the joint(s).


    private static string[] defaultBoneNames_Left = new string[21]
    {
        "Left_Hand",

        "Left_ThumbProximal",
        "Left_ThumbIntermediate",
        "Left_ThumbDistal",
        "Left_ThumbDistalEnd",

        "Left_IndexProximal",
        "Left_IndexIntermediate",
        "Left_IndexDistal",
        "Left_IndexDistalEnd",

        "Left_MiddleProximal",
        "Left_MiddleIntermediate",
        "Left_MiddleDistal",
        "Left_MiddleDistalEnd",

        "Left_RingProximal",
        "Left_RingIntermediate",
        "Left_RingDistal",
        "Left_RingDistalEnd",

        "Left_PinkyProximal",
        "Left_PinkyIntermediate",
        "Left_PinkyDistal",
        "Left_PinkyDistalEnd"
    };

    private static string[] defaultBoneNames_Right = new string[]
    {
        "Right_Hand",

        "Right_ThumbProximal",
        "Right_ThumbIntermediate",
        "Right_ThumbDistal",
        "Right_ThumbDistalEnd",

        "Right_IndexProximal",
        "Right_IndexIntermediate",
        "Right_IndexDistal",
        "Right_IndexDistalEnd",

        "Right_MiddleProximal",
        "Right_MiddleIntermediate",
        "Right_MiddleDistal",
        "Right_MiddleDistalEnd",

        "Right_RingProximal",
        "Right_RingIntermediate",
        "Right_RingDistal",
        "Right_RingDistalEnd",

        "Right_PinkyProximal",
        "Right_PinkyIntermediate",
        "Right_PinkyDistal",
        "Right_PinkyDistalEnd"
    };

    private static string[] openXRBoneNames_Left = new string[21]
    {
        "L_Wrist",

        "L_RingProximal",
        "L_RingProximal",
        "L_RingProximal",
        "L_RingProximal",

        "L_IndexProximal",
        "L_IndexIntermediate",
        "L_IndexDistal",
        "L_IndexTip",

        "L_MiddleProximal",
        "L_MiddleIntermediate",
        "L_MiddleDistal",
        "L_MiddleTip",

        "L_RingProximal",
        "L_RingIntermediate",
        "L_RingDistal",
        "L_RingTip",

        "L_ThumbMetacarpal",
        "L_ThumbProximal",
        "L_ThumbDistal",
        "L_ThumbTip"
    };

    private static string[] openXRBoneNames_Right = new string[21]
    {
        "R_Wrist",

        "R_RingProximal",
        "R_RingProximal",
        "R_RingProximal",
        "R_RingProximal",

        "R_IndexProximal",
        "R_IndexIntermediate",
        "R_IndexDistal",
        "R_IndexTip",

        "R_MiddleProximal",
        "R_MiddleIntermediate",
        "R_MiddleDistal",
        "R_MiddleTip",

        "R_RingProximal",
        "R_RingIntermediate",
        "R_RingDistal",
        "R_RingTip",

        "R_ThumbMetacarpal",
        "R_ThumbProximal",
        "R_ThumbDistal",
        "R_ThumbTip"
    };



    public void DetectBones()
    {
        string[] boneNames; //will be of the appropriate length
        if (rightHand)
            boneNames = layout == BoneLayout.OpenXR ? openXRBoneNames_Right : defaultBoneNames_Right;
        else
            boneNames = layout == BoneLayout.OpenXR ? openXRBoneNames_Left : defaultBoneNames_Left;

        Transform root = this.avatarWrist != null ? this.avatarWrist : this.transform;
        Transform[] joints = GetFingerTransforms();
        //for (int i=0; i< joints.Length; i++)
        //{
        //    FindBone(root, boneNames[i], ref joints[i], true); //Checking if this can be done wiht a ref - tince Transform is a reference...
        //} //Sadly this does not work x(  So I'll just go for the inefficient version

        FindBone(root, boneNames[0], ref avatarWrist, true);

        FindBone(root, boneNames[1], ref avatarThumbCMC, true);
        FindBone(root, boneNames[2], ref avatarThumbMCP, true);
        FindBone(root, boneNames[3], ref avatarThumbIP, true);
        FindBone(root, boneNames[4], ref avatarThumbTip, true);

        FindBone(root, boneNames[5], ref avatarIndexMCP, true);
        FindBone(root, boneNames[6], ref avatarIndexPIP, true);
        FindBone(root, boneNames[7], ref avatarIndexDIP, true);
        FindBone(root, boneNames[8], ref avatarIndexTIP, true);

        FindBone(root, boneNames[9], ref avatarMiddleMCP, true);
        FindBone(root, boneNames[10], ref avatarMiddlePIP, true);
        FindBone(root, boneNames[11], ref avatarMiddleDIP, true);
        FindBone(root, boneNames[12], ref avatarMiddleTIP, true);

        FindBone(root, boneNames[13], ref avatarRingMCP, true);
        FindBone(root, boneNames[14], ref avatarRingPIP, true);
        FindBone(root, boneNames[15], ref avatarRingDIP, true);
        FindBone(root, boneNames[16], ref avatarRingTIP, true);

        FindBone(root, boneNames[17], ref avatarPinkyMCP, true);
        FindBone(root, boneNames[18], ref avatarPinkyPIP, true);
        FindBone(root, boneNames[19], ref avatarPinkyDIP, true);
        FindBone(root, boneNames[20], ref avatarPinkyTIP, true);

    }


    public static void FindBone(Transform root, string name, ref Transform assignTo, bool onlyIfNull)
    {
        if (onlyIfNull && assignTo != null)
            return; //skip bone assignment if its NULL
        assignTo = FindChildRecursive(root, name, System.StringComparison.OrdinalIgnoreCase);
    }

    private static Transform FindChildRecursive(Transform parent, string name, System.StringComparison comparison)
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

}





#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(PHap_DidimoHandInfo))]
public class PHap_DidimoHandInfoEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        PHap_DidimoHandInfo script = (PHap_DidimoHandInfo)target;

        UnityEditor.EditorGUI.BeginChangeCheck();




        DrawDefaultInspector();
        UnityEditor.EditorGUILayout.Space();
        GUILayout.Label("Mapping", UnityEditor.EditorStyles.boldLabel);

        if (GUILayout.Button($"Auto Detect {script.layout.ToString()} bones"))
        {
            Debug.Log($"Detecting Hand Bones using the {script.layout.ToString()} layour");
            script.DetectBones();
        }


        if (GUI.changed)
            UnityEditor.EditorUtility.SetDirty(target);

        if (UnityEditor.EditorGUI.EndChangeCheck())
            UnityEditor.EditorUtility.SetDirty(script);
    }

}

#endif