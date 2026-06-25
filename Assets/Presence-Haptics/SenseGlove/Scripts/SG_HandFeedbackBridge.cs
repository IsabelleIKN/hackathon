using SG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Links a SenseGlove Hand Prefab to the Prensence Haptics API.
 * 
 * @author: max@senseglove.com
 */

/// <summary> A helper script to assign proper HapticReceivers to the right hand bones of the SenseGlove hand system </summary>
public class SG_HandFeedbackBridge : SG.SG_HandComponent
{

    [Header("SG_HandFeedbackBridge Components")]
    public Presence.PHap_HapticReceiver wristReceiver;
    public Presence.PHap_HapticReceiver thumbReceiver;
    public Presence.PHap_HapticReceiver indexReceiver;
    public Presence.PHap_HapticReceiver middleReceiver;
    public Presence.PHap_HapticReceiver ringReceiver;
    public Presence.PHap_HapticReceiver pinkyReceiver;

    List<Collider> layerColliders = null;


    protected void Link(Presence.PHap_HapticReceiver rec, SG.HandJoint toJoint, SG_TrackedHand ofHand)
    {
        if (ofHand == null)
            return;
        Transform joint = ofHand.GetTransform(SG_TrackedHand.TrackingLevel.RenderPose, toJoint);
        if (joint == null)
            return;
        rec.transform.parent = joint;
    }

    


    protected override List<Collider> CollectPhysicsColliders()
    {
        if (layerColliders != null)
            return layerColliders;

        layerColliders = base.CollectPhysicsColliders();
        Presence.PHap_HapticReceiver[] receviers = new Presence.PHap_HapticReceiver[] //making a list so I can do this iteratively. Though I could have also just made a function out of it
        {
            wristReceiver,
            thumbReceiver,
            indexReceiver,
            middleReceiver,
            ringReceiver,
            pinkyReceiver
        };
        foreach (Presence.PHap_HapticReceiver rec in receviers)
        {
            if (rec == null)
                continue;
            Collider[] colldrs = rec.GetColliders();
            foreach (Collider col in colldrs)
            {
                if (col != null && !layerColliders.Contains(col))
                {
                    layerColliders.Add(col);
                }
            }
        }
        return layerColliders;
    }

    protected override void LinkToHand_Internal(SG_TrackedHand newHand, bool firstLink)
    {
        base.LinkToHand_Internal(newHand, firstLink);
        //TODO: Grab the correct joint transfroms off of these and play...
        Link(wristReceiver,     HandJoint.Wrist,            newHand);
        Link(thumbReceiver,     HandJoint.Thumb_FingerTip,  newHand);
        Link(indexReceiver,     HandJoint.Index_FingerTip,  newHand);
        Link(middleReceiver,    HandJoint.Middle_FingerTip, newHand);
        Link(ringReceiver,      HandJoint.Ring_FingerTip,   newHand);
        Link(pinkyReceiver,     HandJoint.Pinky_FingerTip,  newHand);
    }
}
