using SG;
using System.Collections;
using UnityEngine;

/*
 * This one animates a Didimo avatar hand based on information collected by SG_DidimoAvatarMapping.
 * It does so by applying a world rotation based on an SG_HandPose and the current avatarWrist location.
 * Please do so -after- IK has been applied.
 * 
 * author:
 * max@senseglove.com
 */

public class PHap_DidimoHandAnimator : MonoBehaviour
{
    public SG_TrackedHand sgHand;

    public PHap_DidimoHandInfo handInfo;

    public PHap_DidimoMapInfo mappingInfo;

    private Quaternion[] m_jointCorrections = new Quaternion[0];

    public bool alsoCopyWrist = false;
    public Transform wristIKTarget;

    /// <summary> Updates the finger tracking based on an SG_HandPose collected from a SG_TrackedHand and the current avatarWrist location. Uses mappingInfo to ensure a proper offset </summary>
    /// <param name="handPose"></param>
    public void UpdateFingerTracking(SG_HandPose handPose)
    {
        //GetJointLocations gives us all the joint locations 
        handPose.GetJointRotations(false, out Quaternion[] jointRotations);
        
        //Grab the transforms, and use the 
        Transform[] fingerTransforms = handInfo.GetFingerTransforms();

        if (alsoCopyWrist)
        {
            fingerTransforms[0].rotation = wristIKTarget.rotation;
            fingerTransforms[0].position = wristIKTarget.position;
        }


        Quaternion baseWristCorrection = m_jointCorrections[0];
        Quaternion currAvatarWristRot = fingerTransforms[0].rotation;
        
        Quaternion Q_convert = m_jointCorrections[0]; //Quaternion.Euler(0, -90, 0);

        int maxBones = Mathf.Min(jointRotations.Length, fingerTransforms.Length, m_jointCorrections.Length);
        for (int i=1; i<maxBones; i++) //skippin 0 because that is the wrist; it's already positioned.
        {
            //From SG_HandAnimator; fingerJoints[f][j].rotation = handModelInfo.wristTransform.rotation * angles[f][j] * corrections[f][j];

            //rotations are relative to the SenseGlove wrist(s); thus X-forward.
            //fingerTransforms[i].rotation = (currAvatarWristRot * m_jointCorrections[0]) * ((jointRotations[i] * baseWristCorrection) * m_jointCorrections[i]); //which would have been it it it's a one on one mapping.
            Quaternion Qrelative = Q_convert * jointRotations[i] * Quaternion.Inverse(Q_convert);
            fingerTransforms[i].rotation = currAvatarWristRot * Qrelative * m_jointCorrections[i];
        }
    }

    private IEnumerator ApplyHandModel()
    {
        yield return null; //wait one frame
        if (this.mappingInfo.TryGetSGHandModel(out SGCore.Kinematics.BasicHandModel model))
        {
            SGCore.HandLayer.SetDefaultHandModel(model);
            this.sgHand.SetKinematics(model);
        }
    }


    private void Start()
    {
        m_jointCorrections = mappingInfo.GetJointCorrections();
        StartCoroutine(ApplyHandModel());
        if (wristIKTarget == null)
            wristIKTarget = sgHand.GetTransform(SG_TrackedHand.TrackingLevel.RenderPose, HandJoint.Wrist);
    }

    

    private void LateUpdate()
    {
        SG_HandPose renderPose = sgHand.GetHandPose(SG_TrackedHand.TrackingLevel.RenderPose); //Returns the Sg Hand pose to where it is supposed to be rendered.
        UpdateFingerTracking(renderPose);
    }

}
