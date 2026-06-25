using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Detects PHap_HapticReceivers (body parts) and can pass on Haptic Effects to the body parts within.
 * Use it for buttons, etc: "PLay this haptic effect on every haptic receiver in the area"
 * You'll need to pass a LocalPosition as a parameter to generate the PHap_EffectLocation.
 * 
 * author: max@senseglove.com
 */

namespace Presence
{
    public class PHap_ReceiverDetector : MonoBehaviour
    {
       
        //---------------------------------------------------------------------------------------------------------------------------
        // Member Variables

        [Tooltip("Leave this empty detect ALL kinds of PHap_BodyParts. Or add specific body parts to respond to.")]
        public PHap_BodyPart[] bodyPartFiler = new PHap_BodyPart[0];


        /// <summary> Flexible detector list that will handle most of the detection work. We're only interested in the Grabable scripts it provides. </summary>
        protected SG.SG_ScriptDetector<PHap_HapticReceiver> colliderDetection = new SG.SG_ScriptDetector<PHap_HapticReceiver>();


        public DetectionEvent ReceiverDetected = new DetectionEvent();
        public DetectionEvent ReceiverRemoved = new DetectionEvent();

        //---------------------------------------------------------------------------------------------------------------------------
        // Basic interface

        /// <summary> Returns true if this component is detecting any PHap_HapticReceiver in its Trigger volume. </summary>
        /// <returns></returns>
        public virtual bool IsDetecting()
        {
            return this.colliderDetection.DetectedCount() > 0;
        }


        /// <summary> Returns tue if this component is detecting this specific PHap_HapticReceiver </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public virtual bool IsDetecting(PHap_HapticReceiver rec)
        {
            return this.colliderDetection.IsDetected(rec);
        }

        /// <summary> Returns tue if this component is detecting any Phap_HapticRceiver with the chosen body part. </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public virtual bool IsDetecting(PHap_BodyPart part)
        {
            PHap_HapticReceiver[] detectedScripts = this.colliderDetection.GetDetectedScripts();
            foreach (PHap_HapticReceiver rec in detectedScripts)
            {
                if (rec.BodyPart == part)
                    return true;
            }
            return false;
        }

        /// <summary> Returns true if this component can detect the chosen body parts by its filers. </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public virtual bool CanDetect(PHap_BodyPart part)
        {
            if (bodyPartFiler.Length == 0)
                return true; //#nofilter

            foreach (PHap_BodyPart bp in bodyPartFiler)
            {
                if (bp == part)
                    return true;
            }
            return false;
        }


        /// <summary> Returns the currently detected Haptic Receivers inside this zone. </summary>
        /// <returns></returns>
        public PHap_HapticReceiver[] GetDetectedReceivers()
        {
            return this.colliderDetection.GetDetectedScripts();
        }


        //---------------------------------------------------------------------------------------------------------------------------
        // Haptic Functions

        /// <summary> Plays a Haptic Effect on a certain world location on all PHap_HapticReceiver(s) detected by this component </summary>
        /// <param name="effect"></param>
        /// <param name="worldLocation"></param>
        /// <param name="effectSize"></param>
        /// <returns></returns>
        public virtual bool PlayHapticEffect(PHap_HapticEffect effect, Vector3 worldLocation, float effectSize)
        {
            PHap_HapticReceiver[] detectedScripts = this.colliderDetection.GetDetectedScripts();
            foreach (PHap_HapticReceiver rec in detectedScripts)
            {
                Vector3 localPos = rec.Origin.InverseTransformPoint(worldLocation);
                PHap_EffectLocation location = new PHap_EffectLocation(rec.BodyPart, localPos, effectSize, rec.BoundingBoxCenter, rec.BoundingBoxWidth);
                PHap_Core.PlayHapticEffect(effect, location);
            }
            return false;
        }


        /// <summary> Plays a Haptic Effect on a certain world location on all PHap_HapticReceiver(s) detected by this component, but only to one specific body part </summary>
        /// <param name="effect"></param>
        /// <param name="worldLocation"></param>
        /// <param name="effectSize"></param>
        /// <param name="onBodyPart"></param>
        /// <returns></returns>
        public virtual bool PlayHapticEffect(PHap_HapticEffect effect, Vector3 worldLocation, float effectSize, PHap_BodyPart onBodyPart)
        {
            return PlayHapticEffect(effect, worldLocation, effectSize, new PHap_BodyPart[] { onBodyPart });
        }


        /// <summary> Plays a Haptic Effect on a certain world location on all PHap_HapticReceiver(s) detected by this component, but only to specific body parts.  </summary>
        /// <param name="effect"></param>
        /// <param name="worldLocation"></param>
        /// <param name="effectSize"></param>
        /// <param name="onBodyParts"></param>
        /// <returns></returns>
        public virtual bool PlayHapticEffect(PHap_HapticEffect effect, Vector3 worldLocation, float effectSize, PHap_BodyPart[] onBodyParts)
        {
            PHap_HapticReceiver[] detectedScripts = this.colliderDetection.GetDetectedScripts();
            foreach (PHap_HapticReceiver rec in detectedScripts)
            {
                if (ArrayContains(onBodyParts, rec.BodyPart))
                {
                    Vector3 localPos = rec.Origin.InverseTransformPoint(worldLocation);
                    PHap_EffectLocation location = new PHap_EffectLocation(rec.BodyPart, localPos, effectSize, rec.BoundingBoxCenter, rec.BoundingBoxWidth);
                    PHap_Core.PlayHapticEffect(effect, location);
                }
            }
            return false;
        }
        
        /// <summary> Returns true if list contains the enum part. </summary>
        /// <param name="list"></param>
        /// <param name="part"></param>
        /// <returns></returns>
        public static bool ArrayContains(PHap_BodyPart[] list, PHap_BodyPart part)
        {
            foreach (PHap_BodyPart listPart in list)
            {
                if (part == listPart)
                    return true;
            }
            return false;
        }

        //---------------------------------------------------------------------------------------------------------------------------
        // Detection Functions

        /// <summary> Called OnTirggerEnter. Attempts to add a collider to our ColliderDetection script. If succesful, we also generate Detection Arguments. </summary>
        /// <param name="col"></param>
        protected virtual void TryAddCollider(Collider col)
        {
            PHap_HapticReceiver receiver = col.GetComponent<PHap_HapticReceiver>();
            if (receiver != null && this.CanDetect(receiver.BodyPart))
            {
                //Step 2 : Check if this is one relevant to us
                int collidersInZone = this.colliderDetection.AddToList(receiver, col, this); //returns the (new) number of colliders associated with this script
                if (collidersInZone == 1) //this is the first collider of this script we've found. Do something!
                {
                    this.ReceiverDetected.Invoke(receiver, this);
                }
            }
        }


        /// <summary> Called during OnTriggerExit. Attempt to remove a collider from colliderDetection. If that's the last collider, remove the matching HandDetectionArgs. </summary>
        /// <param name="col"></param>
        protected virtual void TryRemoveCollider(Collider col)
        {
            PHap_HapticReceiver removedItem;
            int removeCode = this.colliderDetection.TryRemoveList(col, out removedItem);
            if (removeCode == 2) //if removeCode == 2, then the last collider of removedItem just left the zone
            {
                this.ReceiverRemoved.Invoke(removedItem, this);
            }
        }


        //---------------------------------------------------------------------------------------------------------------------------
        // Monobehaviour


        protected virtual void OnTriggerEnter(Collider other)
        {
            TryAddCollider(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            TryRemoveCollider(other);
        }


        //---------------------------------------------------------------------------------------------------------------------------
        // Event Class

        [System.Serializable] public class DetectionEvent : UnityEngine.Events.UnityEvent<PHap_HapticReceiver, PHap_ReceiverDetector> { }



    }
}