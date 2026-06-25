#define SG_UNITY_HANDS

using SGCore.Kinematics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if SG_UNITY_HANDS
using UnityEngine.XR.Hands;
#endif

namespace SG
{

    /// <summary> Generates a 'Layer' around (OpenXR) Hand Tracking Subsystem(s) so one can use hand tracking within the current SenseGlove Ecosystem. </summary>
    public class SG_XRHandTrackingImplementation : MonoBehaviour, IHandPoseProvider
    {
        //--------------------------------------------------------------------------------------------------
        // Member Variables

        public bool rightHand = true;

        private SGCore.Kinematics.BasicHandModel handModel = null; //for this instance.

        private static SG_HandPose lastRightPose = null, lastLeftPose = null;
        private static int lastRightFrame = -1, lastLeftFrame = -1;

        //--------------------------------------------------------------------------------------------------
        // Tracking Conversion etc

        public static SGCore.Kinematics.Vect3D OpenXR_to_SG_Position_I(Vector3 openXRposition) //inefficient one.... but without REF
        {
            return OpenXR_to_SG_Position(ref openXRposition);
        }

        public static SGCore.Kinematics.Vect3D OpenXR_to_SG_Position(ref Vector3 openXRposition)
        {
            return new SGCore.Kinematics.Vect3D
            (
                openXRposition.z * 1000.0f,   //Z forward in m ->  X forward, in mm
                -openXRposition.x * 1000.0f,  //X pointing Right in m -> Y pointing Left, in mm
                openXRposition.y * 1000.0f    //Y pointing up in m -> Z up in mm
            );
        }

        public static Vector3 SG_to_OpenXR_Position(SGCore.Kinematics.Vect3D sgPositon)
        {
            return new Vector3
            (
                -sgPositon.y / 1000.0f,   // Y pointing Left, in mm -> X pointing Right in m 
                sgPositon.z / 1000.0f,    // Z up, in mm, -> Y up in m
                sgPositon.x / 1000.0f     // X forward, in mm -> Z forward in m
            );
        }


        public static SGCore.Kinematics.Quat OpenXR_to_SG_Rotation_I(Quaternion openXRrotation)
        {
            return OpenXR_to_SG_Rotation(ref openXRrotation);
        }
        public static SGCore.Kinematics.Quat OpenXR_to_SG_Rotation(ref Quaternion openXRrotation)
        {
            return new SGCore.Kinematics.Quat
            (
                -openXRrotation.z,
                -openXRrotation.x,
                -openXRrotation.y,
                openXRrotation.w
            );
        }

        public static Quaternion SG_to_OpenXR_Rotation(SGCore.Kinematics.Quat sgRotation)
        {
            return new Quaternion
            (
                -sgRotation.y,
                -sgRotation.z,
                -sgRotation.x,
                sgRotation.w
            );
        }




        /// <summary> In mm, for SG </summary>
        private static float[][] openXRFingerLengths = new float[5][]
        {
            new float[3] { 0.0325f * 1000f, 0.0338f * 1000f, 0.0246f * 1000f },
            new float[3] { 0.0379f * 1000f, 0.0243f * 1000f, 0.0224f * 1000f },
            new float[3] { 0.0429f * 1000f, 0.0275f * 1000f, 0.0250f * 1000f },
            new float[3] { 0.0390f * 1000f, 0.0266f * 1000f, 0.0244f * 1000f },
            new float[3] { 0.0307f * 1000f, 0.0203f * 1000f, 0.0220f * 1000f }
        };



        //Static internal hand model stuff

        //todo: replace these with 'normal' hard-coded values.

        private static SGCore.Kinematics.Vect3D L_thumbStartPos = OpenXR_to_SG_Position_I(new Vector3(0.0307f, -0.0160f, 0.0347f));
        private static SGCore.Kinematics.Quat L_thumbStartRot = OpenXR_to_SG_Rotation_I(new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));

        private static SGCore.Kinematics.Vect3D L_indexStartPos = OpenXR_to_SG_Position_I(new Vector3(0.0238f, -0.0074f, 0.0971f));
        private static SGCore.Kinematics.Quat L_indexStartRot = OpenXR_to_SG_Rotation_I(new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));

        private static SGCore.Kinematics.Vect3D L_middleStartPos = OpenXR_to_SG_Position_I(new Vector3(0.0017f, -0.0026f, 0.0967f));
        private static SGCore.Kinematics.Quat L_middleStartRot = OpenXR_to_SG_Rotation_I(new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));

        private static SGCore.Kinematics.Vect3D L_ringStartPos = OpenXR_to_SG_Position_I(new Vector3(-0.0177f, -0.0066f, 0.0897f));
        private static SGCore.Kinematics.Quat L_ringStartRot = OpenXR_to_SG_Rotation_I(new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));

        private static SGCore.Kinematics.Vect3D L_pinkyStartPos = OpenXR_to_SG_Position_I(new Vector3(-0.0355f, -0.0138f, 0.0788f));
        private static SGCore.Kinematics.Quat L_pinkyStartRot = OpenXR_to_SG_Rotation_I(new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));

        private static SGCore.Kinematics.BasicHandModel leftModel = null;

        //Hand Dimensions: TODO: Convert these back into 
        public static SGCore.Kinematics.BasicHandModel GetLeftOpenXRModel()
        {
            if (leftModel == null)
            {
                float[][] lengths = openXRFingerLengths;
                SGCore.Kinematics.Vect3D[] startPositions = new SGCore.Kinematics.Vect3D[5]
                {
                    L_thumbStartPos,
                    L_indexStartPos,
                    L_middleStartPos,
                    L_ringStartPos,
                    L_pinkyStartPos
                };
                SGCore.Kinematics.Quat[] startRotations = new SGCore.Kinematics.Quat[5]
                {
                    L_thumbStartRot,
                    L_indexStartRot,
                    L_middleStartRot,
                    L_ringStartRot,
                    L_pinkyStartRot
                };
                leftModel = new SGCore.Kinematics.BasicHandModel(false, lengths, startPositions, startRotations);
            }
            return leftModel;
        }




        //Static internal hand model stuff
        private static SGCore.Kinematics.Vect3D R_thumbStartPos = OpenXR_to_SG_Position_I(new Vector3(-0.0279f, -0.0204f, 0.0371f));
        private static SGCore.Kinematics.Quat R_thumbStartRot = OpenXR_to_SG_Rotation_I(new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));

        private static SGCore.Kinematics.Vect3D R_indexStartPos = OpenXR_to_SG_Position_I(new Vector3(-0.0241f, -0.0075f, 0.0981f));
        private static SGCore.Kinematics.Quat R_indexStartRot = OpenXR_to_SG_Rotation_I(new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));

        private static SGCore.Kinematics.Vect3D R_middleStartPos = OpenXR_to_SG_Position_I(new Vector3(-0.0018f, -0.0026f, 0.0978f));
        private static SGCore.Kinematics.Quat R_middleStartRot = OpenXR_to_SG_Rotation_I(new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));

        private static SGCore.Kinematics.Vect3D R_ringStartPos = OpenXR_to_SG_Position_I(new Vector3(0.0179f, -0.0067f, 0.0907f));
        private static SGCore.Kinematics.Quat R_ringStartRot = OpenXR_to_SG_Rotation_I(new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));

        private static SGCore.Kinematics.Vect3D R_pinkyStartPos = OpenXR_to_SG_Position_I(new Vector3(0.0358f, -0.0140f, 0.0796f));
        private static SGCore.Kinematics.Quat R_pinkyStartRot = OpenXR_to_SG_Rotation_I(new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));

        private static SGCore.Kinematics.BasicHandModel rightModel = null;

        public static SGCore.Kinematics.BasicHandModel GetRightOpenXRModel()
        {
            if (rightModel == null)
            {
                float[][] lengths = openXRFingerLengths;
                SGCore.Kinematics.Vect3D[] startPositions = new SGCore.Kinematics.Vect3D[5]
                {
                    R_thumbStartPos,
                    R_indexStartPos,
                    R_middleStartPos,
                    R_ringStartPos,
                    R_pinkyStartPos
                };
                SGCore.Kinematics.Quat[] startRotations = new SGCore.Kinematics.Quat[5]
                {
                    R_thumbStartRot,
                    R_indexStartRot,
                    R_middleStartRot,
                    R_ringStartRot,
                    R_pinkyStartRot
                };
                rightModel = new SGCore.Kinematics.BasicHandModel(true, lengths, startPositions, startRotations);
            }
            return rightModel;
        }

#if SG_UNITY_HANDS
        private static XRHandJointID[][] HandJointAccessors = new XRHandJointID[5][]
        {
            new XRHandJointID[4] { XRHandJointID.ThumbMetacarpal, XRHandJointID.ThumbProximal, XRHandJointID.ThumbDistal, XRHandJointID.ThumbTip },
            new XRHandJointID[4] { XRHandJointID.IndexProximal, XRHandJointID.IndexIntermediate, XRHandJointID.IndexDistal, XRHandJointID.IndexTip },
            new XRHandJointID[4] { XRHandJointID.MiddleProximal, XRHandJointID.MiddleIntermediate, XRHandJointID.MiddleDistal, XRHandJointID.MiddleTip },
            new XRHandJointID[4] { XRHandJointID.RingProximal, XRHandJointID.RingIntermediate, XRHandJointID.RingDistal, XRHandJointID.RingTip },
            new XRHandJointID[4] { XRHandJointID.LittleProximal, XRHandJointID.LittleIntermediate, XRHandJointID.LittleDistal, XRHandJointID.LittleTip }
        };





        public static Vector3 ToSGPosition(Vector3 openXRPosition)
        {
            //SG Hand Pose is X forward, Y up, Z 'towards the left'
            //OpenXR POse is z forward, Y up, Z 'towards the right'
            return new Vector3(
                openXRPosition.z, //from z formward to x forward
                openXRPosition.y, //y up stays the same
                -openXRPosition.x  //from their x to the right to my z to the left. 
            );
        }

        public static Quaternion QtoSG = Quaternion.Euler(0.0f, -90.0f, 0.0f);

        public static Quaternion ToSGRotation(Quaternion openXRRotation)
        {
            return openXRRotation * QtoSG;
            //return new Quaternion(
            //    openXRRotation.x,
            //    openXRRotation.y,
            //    openXRRotation.z,
            //    openXRRotation.w
            //);
        }


        /// <summary> Extracts a single location in world space. </summary>
        /// <param name="openXRPose"></param>
        /// <param name="jointId"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public static void ExtractLocation(ref XRHand openXRPose, XRHandJointID jointId, out Vector3 position, out Quaternion rotation, bool inSGCoords)
        {
            try
            {
                if (openXRPose.GetJoint(jointId).TryGetPose(out Pose pose))
                {
                    position = inSGCoords ? ToSGPosition(pose.position) : pose.position;
                    rotation = inSGCoords ? ToSGRotation(pose.rotation) : pose.rotation;
                    return;
                }
            }
            catch (System.Exception)
            {
                //TODO: DebugLog?
            }
            position = Vector3.zero;
            rotation = Quaternion.identity;
        }


        /// <summary> Extracts multiple locations in world space </summary>
        /// <param name="openXRPose"></param>
        /// <param name="jointIds"></param>
        /// <param name="positions"></param>
        /// <param name="rotations"></param>
        public static void ExtractLocations(ref XRHand openXRPose, XRHandJointID[] jointIds, out Vector3[] positions, out Quaternion[] rotations, bool inSGCoords)
        {
            positions = new Vector3[jointIds.Length];
            rotations = new Quaternion[jointIds.Length];
            for (int i = 0; i < jointIds.Length; i++)
            {
                ExtractLocation(ref openXRPose, jointIds[i], out positions[i], out rotations[i], inSGCoords);
            }
        }

        /// <summary> Extracts multiple locations in world space </summary>
        /// <param name="openXRPose"></param>
        /// <param name="jointIds"></param>
        /// <param name="positions"></param>
        /// <param name="rotations"></param>
        public static void ExtractLocations(ref XRHand openXRPose, XRHandJointID[][] jointIds, out Vector3[][] positions, out Quaternion[][] rotations, bool inSGCoords)
        {
            positions = new Vector3[jointIds.Length][];
            rotations = new Quaternion[jointIds.Length][];
            for (int i = 0; i < jointIds.Length; i++)
            {
                ExtractLocations(ref openXRPose, jointIds[i], out positions[i], out rotations[i], inSGCoords);
            }
        }




        public static void Extract_RelativeToWrist(ref XRHand openXRPose, XRHandJointID jointId, ref Vector3 wristPos, ref Quaternion wristRot, out Vector3 position, out Quaternion rotation, bool inSGCoords)
        {
            try
            {
                if (openXRPose.GetJoint(jointId).TryGetPose(out Pose pose))
                {
                    Vector3 worldPos = inSGCoords ? ToSGPosition(pose.position) : pose.position;
                    Quaternion worldRot = inSGCoords ? ToSGRotation(pose.rotation) : pose.rotation;
                    SG.Util.SG_Util.CalculateOffsets(worldPos, worldRot, wristPos, wristRot, out position, out rotation);
                    return;
                }
            }
            catch (System.Exception)
            {
                //TODO: DebugLog?
            }
            position = Vector3.zero;
            rotation = Quaternion.identity;
        }

        public static void Extract_RelativeToWrist(ref XRHand openXRPose, XRHandJointID[] jointIds, ref Vector3 wristPos, ref Quaternion wristRot, out Vector3[] positions, out Quaternion[] rotations, bool inSGCoords)
        {
            positions = new Vector3[jointIds.Length];
            rotations = new Quaternion[jointIds.Length];
            for (int i = 0; i < jointIds.Length; i++)
            {
                Extract_RelativeToWrist(ref openXRPose, jointIds[i], ref wristPos, ref wristRot, out positions[i], out rotations[i], inSGCoords);
            }
        }

        public static void Extract_RelativeToWrist(ref XRHand openXRPose, XRHandJointID[][] jointIds, ref Vector3 wristPos, ref Quaternion wristRot, out Vector3[][] positions, out Quaternion[][] rotations, bool inSGCoords)
        {
            positions = new Vector3[jointIds.Length][];
            rotations = new Quaternion[jointIds.Length][];
            for (int i = 0; i < jointIds.Length; i++)
            {
                Extract_RelativeToWrist(ref openXRPose, jointIds[i], ref wristPos, ref wristRot, out positions[i], out rotations[i], inSGCoords);
            }
        }


        /// <summary> TODO: What if the player moves? </summary>
        /// <param name="openXRPose"></param>
        /// <returns></returns>
        public static SG_HandPose ConvertOpenXRPose(ref XRHand openXRPose)
        {
            bool rightHand = openXRPose.handedness == Handedness.Right;

            Vector3 wristPos_openXR; Quaternion wristRot_openXR;
            ExtractLocation(ref openXRPose, XRHandJointID.Wrist, out wristPos_openXR, out wristRot_openXR, false);
            Quaternion sgWristRot = ToSGRotation(wristRot_openXR);

            Extract_RelativeToWrist(ref openXRPose, HandJointAccessors, ref wristPos_openXR, ref sgWristRot, out Vector3[][] openXRpositions, out Quaternion[][] openXRRotations, false);



            Quaternion[][] jointRotations = new Quaternion[5][];
            Vector3[][] jointPositons = new Vector3[5][];
            float[] normalizedFlex = new float[5];
            Vector3[][] handAngles = new Vector3[5][];
            for (int f = 0; f < 5; f++)
            {
                //TODO: Extract local rotations.
                jointRotations[f] = new Quaternion[4] { ToSGRotation(openXRRotations[f][0]), ToSGRotation(openXRRotations[f][1]), ToSGRotation(openXRRotations[f][2]), ToSGRotation(openXRRotations[f][3]) };
                jointPositons[f] = new Vector3[4] { openXRpositions[f][0], openXRpositions[f][1], openXRpositions[f][2], openXRpositions[f][3] };

                //handangles is current - previous
                handAngles[f] = new Vector3[3]
                {
                    SG.Util.SG_Util.NormalizeAngles( jointRotations[f][0].eulerAngles),
                    SG.Util.SG_Util.NormalizeAngles( (Quaternion.Inverse(jointRotations[f][0]) * jointRotations[f][1]).eulerAngles ),
                    SG.Util.SG_Util.NormalizeAngles( (Quaternion.Inverse(jointRotations[f][1]) * jointRotations[f][2]).eulerAngles )
                };

                float sumFlex = handAngles[f][0].z + handAngles[f][1].z + handAngles[f][2].z; 

                if (f == 0)
                    normalizedFlex[f] = SGCore.Kinematics.Anatomy.NormalizeThumbFlex(-sumFlex * Mathf.Deg2Rad);
                else
                    normalizedFlex[f] = SGCore.Kinematics.Anatomy.NormalizeFingerFlex(-sumFlex * Mathf.Deg2Rad);
            }

            //and finally, we need to shift the real wirst with our XR CameraRig!
            Transform rig = SG_XR_SceneTrackingLinks.SceneXRRig;
            Quaternion finalWristRot = rig != null ? rig.rotation * sgWristRot : sgWristRot;
            Vector3 finalWristPos = rig != null ? rig.position + (rig.rotation * wristPos_openXR) : wristPos_openXR;

            return new SG_HandPose(handAngles, jointRotations, jointPositons, rightHand, finalWristPos, finalWristRot, normalizedFlex); //openXR positon is the same, But rotation from there is different!
        }
#endif

        //--------------------------------------------------------------------------------------------------
        // IHandPoseProvider Implementation

        public HandTrackingDevice TrackingType()
        {
            return HandTrackingDevice.OpticalHandTracking;
        }

        public bool TracksRightHand()
        {
            return rightHand;
        }

        public bool IsConnected()
        {
#if SG_UNITY_HANDS
            return SG_OpenXRHandLayer.IsTracked(this.rightHand);
#else
            return false;
#endif
        }

        public bool TryGetBatteryLevel(out float value01)
        {
            value01 = 0.0f;
            return false;
        }



        public void SetKinematics(BasicHandModel handModel)
        {
            if (handModel.IsRight)
                rightModel = handModel;
            else
                leftModel = handModel;
        }

        public BasicHandModel GetKinematics()
        {
            if (this.handModel == null)
            {
                this.handModel = rightHand ? GetRightOpenXRModel() : GetLeftOpenXRModel();
            }
            return this.handModel;
        }



        public bool GetHandPose(out SG_HandPose handPose, bool forcedUpdate = false)
        {
            handPose = rightHand ? lastRightPose : lastLeftPose; //assign this either way
#if SG_UNITY_HANDS
            int lastFrame = rightHand ? lastRightFrame : lastLeftFrame;
            if (Time.frameCount == lastFrame && !forcedUpdate) //we've already converted this frame, so returning the lastpose is fine.
                return true;

            if (SG_OpenXRHandLayer.GetXRHandPose(this.rightHand, out XRHand openXRPose))
            {
                handPose = ConvertOpenXRPose(ref openXRPose);
                if (rightHand)
                {
                    lastRightFrame = Time.frameCount;
                    lastRightPose = handPose;
                }
                else
                {
                    lastLeftFrame = Time.frameCount;
                    lastLeftPose = handPose;
                }
                return true;
            }
#endif
            return false;
        }



        public bool GetNormalizedFlexion(out float[] flexions)
        {
            if (GetHandPose(out SG_HandPose handPose))
            {
                flexions = handPose.normalizedFlexion;
                return true;
            }
            flexions = new float[5];
            return false;
        }

        public float OverrideGrab()
        {
            return 0.0f;
        }

        public float OverrideUse()
        {
            return 0.0f;
        }




        private void Awake()
        {
            //creates some default pose(s) that can be updated later...
            lastLeftPose = SG_HandPose.Idle(false);
            lastLeftFrame = -1;
            lastRightPose = SG_HandPose.Idle(true);
            lastRightFrame = -1;
        }

    }
}