using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SG;
using UnityEngine.Events;
using System;

namespace Presence
{

    public class SG_Dial : SG_Grabable
    {
        public SG.Util.MoveAxis rotateAround = SG.Util.MoveAxis.Y;

        [Header("How many rotations degree for an event to fire")]
        public int step = 10;

        [Header("Haptic effect")]
        public PHap_HapticEffect hapticEffect;

        [Header("Which location to play the haptic effect if pressing the button")]
        public HandPart whichPart = HandPart.Index;

        [Header("--- Events ---")]
        public UnityEvent stepEvent;

        [HideInInspector]
        public SG_TrackedHand interactingGlove;

        // private vars
        private Vector3 startrotation = Vector3.zero;
        private float oldRotation = 0f;

        protected override void Start()
        {
            base.Start();

            startrotation = this.MyTransform.localEulerAngles;
        }

        protected override void MoveToTargetLocation(Vector3 targetPosition, Quaternion targetRotation, float dT)
        {
            Transform baseTransform = this.MyTransform;
            baseTransform.rotation = targetRotation;

            Vector3 localEuler = baseTransform.localEulerAngles;

            int axIndex = SG.Util.SG_Util.AxisIndex(rotateAround);
            for (int i = 0; i < 3; i++)
            {
                if (i != axIndex)
                {
                    localEuler[i] = 0;
                }
                else
                {
                    // check if a haptic effect needs to be played
                    if (CalcStep(oldRotation) != CalcStep(SG.Util.SG_Util.NormalizeAngle(localEuler[i])))
                    {
                        PlayHaptics();
                        stepEvent.Invoke();
                    }
                    // set the oldraotion for the next frame
                    oldRotation = SG.Util.SG_Util.NormalizeAngle(localEuler[i]);
                }
            }
            baseTransform.localEulerAngles = localEuler;
        }

        private void PlayHaptics()
        {
            PHap_EffectLocation location = new PHap_EffectLocation(Presence.PHap_BodyPart.RightIndexFinger);

            interactingGlove = this.grabbedBy[0].TrackedHand;

            if (interactingGlove != null)
            {
                if (interactingGlove.TracksRightHand())
                {
                    switch (whichPart)
                    {
                        case (HandPart.Index):
                            location = new PHap_EffectLocation(Presence.PHap_BodyPart.RightIndexFinger);
                            break;
                        case (HandPart.Thumb):
                            location = new PHap_EffectLocation(Presence.PHap_BodyPart.RightThumb);
                            break;
                        case (HandPart.Palm):
                            location = new PHap_EffectLocation(Presence.PHap_BodyPart.RightHandPalm);
                            break;
                    }
                }
                else
                {
                    switch (whichPart)
                    {
                        case (HandPart.Index):
                            location = new PHap_EffectLocation(Presence.PHap_BodyPart.LeftIndexFinger);
                            break;
                        case (HandPart.Thumb):
                            location = new PHap_EffectLocation(Presence.PHap_BodyPart.LeftThumb);
                            break;
                        case (HandPart.Palm):
                            location = new PHap_EffectLocation(Presence.PHap_BodyPart.LeftHandPalm);
                            break;
                    }
                }
                //print("play haptic effect on: " + location);
                PHap_Core.PlayHapticEffect(hapticEffect, location);
            }
        }

        // check in which step of the dial the value sits
        private int CalcStep(float angle)
        {
            angle = Mathf.Abs(angle);

            float stepSize = 360 / step;
            int currentStep = (int)(angle / stepSize);

            return currentStep;
        }

        protected override void SetupScript()
        {
            base.SetupScript();
            if (this.physicsBody != null)
            {
                this.physicsBody.isKinematic = true;
            }
        }

        // make sure the object can't been moved
        protected virtual void OnValidate()
        {
            this.moveSpeed = 0.0f;
        }
    }
}
