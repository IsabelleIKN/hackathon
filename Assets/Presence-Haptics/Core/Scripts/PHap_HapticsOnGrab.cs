using SG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Add this component to an SG_Interactable / Grabable, and it will activate a Haptic Effect on Grab. It will also stop the effect when a Release happens
 * 
 * author:
 * max@senseglove.com
 */

namespace Presence
{
    [RequireComponent(typeof(SG_Interactable))] //must be attached to an SG_Interactable for now.
    public class PHap_HapticsOnGrab : MonoBehaviour
    {
        [SerializeField] private PHap_HapticEffect[] hapticEffects = new PHap_HapticEffect[0];

        [SerializeField] private SG_Interactable grabableObject = null;

        [SerializeField] private bool playOnHandPalm = false;
        [SerializeField] private bool playOnThumb = true;
        [SerializeField] private bool playOnIndex = true;
        [SerializeField] private bool playOnMiddle = true;
        [SerializeField] private bool playOnRing = true;
        [SerializeField] private bool playOnPinky = true;

        private PHap_EffectLocation[] leftLocations = new PHap_EffectLocation[0];
        private PHap_EffectLocation[] rightLocations = new PHap_EffectLocation[0];


        private void ObjectGrabbed(SG_Interactable obj, SG_GrabScript grabbedBy)
        {
            PHap_EffectLocation[] locations = grabbedBy.IsRight ? rightLocations : leftLocations;
            if (locations.Length == 0)
                return;
            foreach (PHap_HapticEffect effect in hapticEffects)
            {
                if (effect != null)
                {
                    foreach (PHap_EffectLocation location in locations)
                        effect.PlayEffect(location);
                }
            }
        }

        private void ObjectReleased(SG_Interactable obj, SG_GrabScript grabbedBy)
        {
            StopEffects(grabbedBy.IsRight);
        }


        private void StopEffects(bool rightHand)
        {
            PHap_EffectLocation[] locations = rightHand ? rightLocations : leftLocations;
            if (locations.Length == 0)
                return;
            foreach (PHap_HapticEffect effect in hapticEffects)
            {
                if (effect != null)
                {
                    foreach (PHap_EffectLocation location in locations)
                        effect.StopEffect(location);
                }
            }
        }

        private void ObjectDestroyed()
        {
            StopEffects(true);
            StopEffects(false);
        }

        private void GenerateLocations()
        {
            List<PHap_EffectLocation> loc_r = new List<PHap_EffectLocation>();
            List<PHap_EffectLocation> loc_l = new List<PHap_EffectLocation>();
            if (playOnHandPalm)
            {
                loc_r.Add(new PHap_EffectLocation(PHap_BodyPart.RightHandPalm));
                loc_l.Add(new PHap_EffectLocation(PHap_BodyPart.LeftHandPalm));
            }
            if (playOnThumb)
            {
                loc_r.Add(new PHap_EffectLocation(PHap_BodyPart.RightThumb));
                loc_l.Add(new PHap_EffectLocation(PHap_BodyPart.LeftThumb));
            }
            if (playOnIndex)
            {
                loc_r.Add(new PHap_EffectLocation(PHap_BodyPart.RightIndexFinger));
                loc_l.Add(new PHap_EffectLocation(PHap_BodyPart.LeftIndexFinger));
            }
            if (playOnMiddle)
            {
                loc_r.Add(new PHap_EffectLocation(PHap_BodyPart.RightMiddleFinger));
                loc_l.Add(new PHap_EffectLocation(PHap_BodyPart.LeftMiddleFinger));
            }
            if (playOnRing)
            {
                loc_r.Add(new PHap_EffectLocation(PHap_BodyPart.RightRingFinger));
                loc_l.Add(new PHap_EffectLocation(PHap_BodyPart.LeftRingFinger));
            }
            if (playOnPinky)
            {
                loc_r.Add(new PHap_EffectLocation(PHap_BodyPart.RightPinky));
                loc_l.Add(new PHap_EffectLocation(PHap_BodyPart.LeftPinky));
            }
            rightLocations = loc_r.ToArray();
            leftLocations = loc_l.ToArray();
        }

        private void Awake()
        {
            GenerateLocations();
        }

        private void OnEnable()
        {
            if (grabableObject == null)
                grabableObject = GetComponent<SG_Interactable>();
            if (grabableObject != null)
            {
                grabableObject.ObjectGrabbed.AddListener(ObjectGrabbed);
                grabableObject.ObjectReleased.AddListener(ObjectReleased);
                grabableObject.ObjectDestroyed.AddListener(ObjectDestroyed);
            }
        }

        private void OnDisable()
        {
            if (grabableObject != null)
            {
                grabableObject.ObjectGrabbed.RemoveListener(ObjectGrabbed);
                grabableObject.ObjectReleased.RemoveListener(ObjectReleased);
                grabableObject.ObjectDestroyed.RemoveListener(ObjectDestroyed);
            }
        }

    }
}