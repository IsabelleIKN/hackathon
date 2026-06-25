using UnityEngine;
using SG;
using Presence;
using SGCore.Kinematics;

/// <summary> Creates a SG_HandModel out of these, and a series of 'rotation offsets' to apply (local?) quaternion rotations to these fingers.. </summary>
public class Phap_DidimoAvatarMapper : MonoBehaviour
{
    public SG_TrackedHand sgHandForMapping;

    public PHap_DidimoHandInfo didimoHandInfo;

    public PHap_DidimoMapInfo mappingOutput;

    //TOOD: Wrist Offset (local position / rotation)


    public SGCore.Kinematics.BasicHandModel CalculateSGHandModel(Transform[] jointTransforms, bool rightHand)
    {
        float[][] handLenghts = new float[5][];
        SGCore.Kinematics.Vect3D[] startPositions = new SGCore.Kinematics.Vect3D[5];

        Transform[][] fingerJoints = new Transform[5][]; //place it in a 5x4 array
        for (int f = 0; f < 5; f++)
        {
            int startIndex = (f * 4) + 1; // +1 because we begin at the wrist, and we have 4 joints per finger
            fingerJoints[f] = new Transform[4] 
            {
                jointTransforms[startIndex],
                jointTransforms[startIndex + 1],
                jointTransforms[startIndex + 2],
                jointTransforms[startIndex + 3]
            };
        }


        SGCore.Kinematics.BasicHandModel defaultH = SGCore.Kinematics.BasicHandModel.Default(rightHand);

        for (int f = 0; f < 5; f++)
        {
            // SGCore.Kinematics.Vect3D lastPos = fingerJoints[f].Length > 0 ? SG_Util.ToPosition(fingerJoints[f][0].position, true) 
            //     : defaultH.GetJointPosition((SGCore.Finger)f);

            SGCore.Kinematics.Vect3D lastPos = fingerJoints[f].Length > 0 ? RelativePosition(fingerJoints[f][0], jointTransforms[0])
                : defaultH.GetJointPosition((SGCore.Finger)f);

            startPositions[f] = lastPos; //the first position is the starting position.

            handLenghts[f] = new float[3];
            float[] defaultL = handLenghts[f] = defaultH.GetFingerLengths((SGCore.Finger)f);

            for (int j = 1; j < 4; j++)
            {
                if (fingerJoints[f].Length > j)
                {
                    SGCore.Kinematics.Vect3D currPos = RelativePosition(fingerJoints[f][j], jointTransforms[0]);
                    handLenghts[f][j - 1] = (currPos.x - lastPos.x); //converts from m to mm
                    lastPos = currPos; //update
                }
                else
                {
                    handLenghts[f][j - 1] = defaultL[j - 1];
                }
            }
        }
        SGCore.Kinematics.BasicHandModel HM = new SGCore.Kinematics.BasicHandModel(rightHand, handLenghts, startPositions);
        //Debug.Log("Collected HandModelInfo: " + HM.ToString(true));
        return HM;
    }

    /// <summary> Returns the position of a joint relative to the wrist. Without scaling. </summary>
    /// <param name="jointTransform"></param>
    /// <param name="wristTransform"></param>
    /// <returns></returns>
    private SGCore.Kinematics.Vect3D RelativePosition(Transform jointTransform, Transform wristTransform)
    {
        Vector3 localPos = Quaternion.Inverse(wristTransform.rotation) * (jointTransform.position - wristTransform.position);
        //SG_Util.ToPosition(fingerJoints[f][0].position, true);
        return SG.Util.SG_Conversions.ToPosition(localPos, true);
    }


    public void CalculateMapping()
    {
        if (mappingOutput == null)
        {
            Debug.LogError("No mappingOutput hans been assigned ", this);
            return;
        }

        bool rightHand = sgHandForMapping.TracksRightHand();

        Transform[] jointTransforms = didimoHandInfo.GetFingerTransforms();  //Wrist is at 0.
        Quaternion[] jointCorrections = new Quaternion[jointTransforms.Length];
       
        Quaternion invWrist = Quaternion.Inverse(jointTransforms[0].rotation); //I'll be re-using this one a bunch


        jointCorrections[0] = sgHandForMapping.transform.localRotation;
        //if (didimoHandInfo.layout == PHap_DidimoHandInfo.BoneLayout.OpenXR)
        //    jointCorrections[0] = rightHand ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0f, -90f, 180f);
        //else
        //    jointCorrections[0] = rightHand ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0f, -90f, 180f);

        for (int i=1; i< jointTransforms.Length; i++) //skipping 1 because that is the wrist
        {
            //this.iFingerCorrections[f][j] = Quaternion.Inverse(this.wristTransform.rotation) * joints[f][j].rotation;
            jointCorrections[i] = invWrist * jointTransforms[i].rotation; //TOOD: Also add a ... 90 degree rotation somewhere.
        }
        mappingOutput.SetJointCorrections(jointCorrections);

        BasicHandModel handModel = CalculateSGHandModel(jointTransforms, rightHand);
        mappingOutput.SetSGHandModel(handModel);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(mappingOutput);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
}




#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(Phap_DidimoAvatarMapper))]
public class SG_DidimoAvatarMappingEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Phap_DidimoAvatarMapper script = (Phap_DidimoAvatarMapper)target;

        UnityEditor.EditorGUI.BeginChangeCheck();




        DrawDefaultInspector();
        UnityEditor.EditorGUILayout.Space();
        GUILayout.Label("Mapping", UnityEditor.EditorStyles.boldLabel);

        if (GUILayout.Button("Calculate Mapping"))
        {
            Debug.Log("Caluclate Mapping!");
            script.CalculateMapping();
        }


        if (GUI.changed)
            UnityEditor.EditorUtility.SetDirty(target);

        if (UnityEditor.EditorGUI.EndChangeCheck())
            UnityEditor.EditorUtility.SetDirty(script);
    }

}

#endif