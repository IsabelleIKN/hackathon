/*
 * Has material properties to 'emit' when it colliders with a HapticReceiver (e.g. the local user's torso)
 * //TODO: Have a Give HapticEffects a Filter as to what body parts they can affect?
 * 
 * author:
 * max@senseglove.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Presence
{
    /// <summary> An event for other developers to hook into an add their own Haptics function call(s). </summary>
    [System.Serializable] public class PHap_HapticCollisionEvent : UnityEngine.Events.UnityEvent<PHap_HapticEmitter, PHap_HapticReceiver, PHap_EffectLocation> { }

    //[RequireComponent(typeof(Collider))] //Alternatively, you can attach this to a GameObject with a RigidBody, and a bunch of smaller, primitive colliders as children!
    public class PHap_HapticEmitter : MonoBehaviour
    {
        

        //--------------------------------------------------------------------------------
        // Member Variables

        /// <summary> Each of the HapticEffects that will be played / stopped when interacting with a HapticEmitter. </summary>
        [SerializeField] protected PHap_HapticEffect[] hapticEffects;

        [SerializeField] protected float effectSize = 10.0f; // The size (world space) of the effect (Relevant for Actro, who prefer the range of 10-100?


        protected List<ReceiverColliderTracker> touchedReceivers = new List<ReceiverColliderTracker>();

        /// <summary> If true, we call the StopEffect when OnTriggerExit on (non-looping)timed effect(s). If set to false, the rest of the effect will play until it ends. </summary>
        [SerializeField] protected bool stopHapticsOnExit = true;




        // Might be removed later, redundant.

        /// <summary> If true, we keep calling the play effect but update the location OnTriggerStay </summary>
        [SerializeField] protected bool keepUpdatingLocation = false;

   
     //   [SerializeField] protected bool useSphereIntersectionForSize = true; // enable this to calculate the effect size based on how much of a sphere collider penetrates the torso. Only works for sphere colliders!




        //--------------------------------------------------------------------------------
        // Events

        /// <summary> Fires when this Emitter comes into contact with a Reciever, during OnTriggerEnter </summary>
        [Header("Events")]
        public PHap_HapticCollisionEvent OnHapticsEnter = new PHap_HapticCollisionEvent();
        /// <summary> Fires when this Emitter staus in contact with a Reciever, during OnTriggerStay  </summary>
        public PHap_HapticCollisionEvent OnHapticsStay = new PHap_HapticCollisionEvent();
        /// <summary> Fires when this Emitter stops contacting a Reciever, during OnTriggerExit </summary>
        public PHap_HapticCollisionEvent OnHapticsExit = new PHap_HapticCollisionEvent();






        //[SerializeField] private Transform DebugHapticsSphere; //will be removed on release and replaced wiht a proper Debug visualization
        //[SerializeField] private Transform DebugHapticsBox; //will be removed on release and replaced wiht a proper Debug visualization



        //--------------------------------------------------------------------------------
        // Accessors


        //--------------------------------------------------------------------------------
        // Collision Handling


        public void SetHapticEffects(PHap_HapticEffect[] newEffects)
        {
            this.hapticEffects = newEffects;
        }


        public PHap_EffectLocation CalculateEffectLocation(ReceiverColliderTracker touchData, Collider col)
        {
            //Debug.DrawLine(touchData.HapticReceiver.Origin.position, this.transform.position, Color.white);

            Vector3 contactPoint = col is MeshCollider && !((MeshCollider)col).convex ? Vector3.zero : col.ClosestPoint(this.transform.position); //Function is not compatibel with non-convex mesh colliders.
            //ClosestPoint calculates the closest point on col to my origin. Though if my origin in inside the collider, it returns the input. It's all in World Space.
            Vector3 localPos = touchData.HapticReceiver.Origin.InverseTransformPoint(contactPoint);
            return new PHap_EffectLocation(touchData.HapticReceiver.BodyPart, localPos, this.effectSize, touchData.HapticReceiver.BoundingBoxCenter, touchData.HapticReceiver.BoundingBoxWidth);
        }

        private void HandleEventOnEnter(ReceiverColliderTracker touchData, Collider col)
        {
            PHap_EffectLocation location = CalculateEffectLocation(touchData, col);
            DrawEffectLocation(location, touchData.HapticReceiver);
            foreach (PHap_HapticEffect hapticEffect in hapticEffects)
            {
                //TODO: Have a Give HapticEffects a Filter as to what body parts they can affect?
                PHap_Core.PlayHapticEffect(hapticEffect, location);
            }
            OnHapticsEnter.Invoke(this, touchData.HapticReceiver, location);
            touchData.HapticReceiver.OnHapticsEnter.Invoke(this, touchData.HapticReceiver, location);
        }


        /// <summary> Only called during OnTriggerStay for receivers I'm still colliding with. </summary>
        /// <param name="touchData"></param>
        private void HandleEventOnStay(ReceiverColliderTracker touchData)
        {
            //TODO: For now, let's assume most HapticReceivers have only one single collider. So I'll just grab the one at index 0 always.
            PHap_EffectLocation location = CalculateEffectLocation(touchData, touchData.GetCollider(0));
            DrawEffectLocation(location, touchData.HapticReceiver);
            OnHapticsStay.Invoke(this, touchData.HapticReceiver, location);
            touchData.HapticReceiver.OnHapticsStay.Invoke(this, touchData.HapticReceiver, location);
        }


        private void HandleEventOnExit(ReceiverColliderTracker touchData, Collider col)
        {
            PHap_EffectLocation location = CalculateEffectLocation(touchData, col);
            DrawEffectLocation(location, touchData.HapticReceiver);
            foreach (PHap_HapticEffect hapticEffect in hapticEffects)
            {
                if (stopHapticsOnExit || hapticEffect.PlaysInfinite) //we have to stop all looping and non-timed effects regardless.
                {
                    PHap_Core.StopHapticEffect(hapticEffect, location);
                } 
            }
            OnHapticsExit.Invoke(this, touchData.HapticReceiver, location);
            touchData.HapticReceiver.OnHapticsExit.Invoke(this, touchData.HapticReceiver, location);
        }



        /// <summary> Returns a valid index in touchedReceivers if col belongs to one of ours. If not, returns -1 </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        private int GetReceiverIndex(PHap_HapticReceiver reciever)
        {
            for (int i = 0; i < this.touchedReceivers.Count; i++)
            {
                if (touchedReceivers[i].HapticReceiver == reciever)
                    return i;
            }
            return -1;
        }


        /// <summary> Stop all my effects on any body part that I've been playing on...? </summary>
        public void StopAllHapticEffects()
        {
            //this.touchedReceivers
            foreach (ReceiverColliderTracker col in touchedReceivers)
            {
                foreach (PHap_HapticEffect effect in this.hapticEffects)
                {
                    PHap_Core.StopHapticEffect(effect, new PHap_EffectLocation( col.HapticReceiver.BodyPart ));
                }
            }
        }


        //--------------------------------------------------------------------------------
        // Monobehaviour

        protected virtual void OnTriggerEnter(Collider col)
        {
            if (!this.enabled)
                return;

            if (PHap_Util.TryGetComponentFromCollision(col, false, out PHap_HapticReceiver reciever)) //Currently I'm assuming HapticReceivers are always attached to the same GameObject as the collider.
            {
                if (!reciever.enabled) //ignore disabled Receivers.
                    return;

                //This collider belongs to a reciever, but which one?
                int colIndex = GetReceiverIndex(reciever);
                if (colIndex < 0) //-1 or lower, this is a new collider...
                {
                    ReceiverColliderTracker newReceiver = new ReceiverColliderTracker(reciever, col);
                    touchedReceivers.Add(newReceiver);
                    HandleEventOnEnter(newReceiver, col);
                }
                else //it's one we already know.
                    touchedReceivers[colIndex].AddCollider(col);
            }
        }

        protected virtual void OnTriggerExit(Collider col)
        {
            for (int i = 0; i < touchedReceivers.Count; i++)
            {
                if (touchedReceivers[i].TryRemoveCollider(col)) //returns true if col belongs to this HapticReceiver and was removed.
                {
                    if (touchedReceivers[i].ColliderCount == 0) //this was the last collider to 'un-touch', so we should clear / stop effect 
                    {
                        OnHapticsExit.Invoke(this, touchedReceivers[i].HapticReceiver, null);
                        HandleEventOnExit(touchedReceivers[i], col);
                        touchedReceivers.RemoveAt(i);
                    }
                    return;
                }
            }
        }

        protected void FixedUpdate() //won't fire if were not enabled.
        {
            if (!keepUpdatingLocation)
                return;
            
            for (int i = 0; i < touchedReceivers.Count; i++)
            {
                if (touchedReceivers[i].HapticReceiver.enabled)
                {
                    HandleEventOnStay(touchedReceivers[i]);
                }
            }
           
        }


        protected void Awake()
        {
            if (hapticEffects.Length == 0)
                hapticEffects = GetComponents<PHap_HapticEffect>();
        }

        private void OnDestroy()
        {
            //Stop all Haptic Effects(?)
        }


        //--------------------------------------------------------------------------------
        // Debug

        public void DrawEffectLocation(PHap_EffectLocation location, PHap_HapticReceiver reciever)
        {
            //if (DebugHapticsSphere != null)
            //{
            //    DebugHapticsSphere.localScale = new Vector3(location.EffectSize, location.EffectSize, location.EffectSize);
            //    DebugHapticsSphere.position = location.LocalPosition;
            //}
            //if (DebugHapticsBox != null)
            //{
            //    DebugHapticsBox.position = location.BoundingBoxCenter;
            //    DebugHapticsBox.localScale = location.BoundingBoxSize;
            //}
            Transform origin = reciever.Origin;
            Vector3 worldPos = origin.TransformPoint(location.LocalPosition);
            Debug.DrawLine(origin.position, worldPos);
        }



        //--------------------------------------------------------------------------------
        // Helper class for colliders

        /// <summary> Keeps track of Haptic Receivers and their colliders that this emitter collides with. </summary>
        public class ReceiverColliderTracker
        {
            /// <summary> The HapticReceiver that is being collided with. </summary>
            public PHap_HapticReceiver HapticReceiver { get; private set; }

            /// <summary> The colliders associated with this HapticReceiver. </summary>
            private List<Collider> colliders;

            public ReceiverColliderTracker(PHap_HapticReceiver reciever, Collider firstCollider)
            {
                HapticReceiver = reciever;
                colliders = new List<Collider>(1);
                colliders.Add(firstCollider);
            }

            public int ColliderCount
            {
                get { return colliders.Count; }
            }

            /// <summary> Returns true if a collider belongs to this Reciever </summary>
            /// <param name="col"></param>
            /// <returns></returns>
            public bool UsesCollider(Collider col)
            {
                return colliders.Contains(col);
            }

            public Collider GetCollider(int index)
            {
                return this.colliders[index];
            }

            public void AddCollider(Collider col)
            {
                if (!colliders.Contains(col))
                {
                    colliders.Add(col);
                }
            }

            /// <summary> Returns true if col belongs to this Reciever, and was removed. </summary>
            /// <param name="col"></param>
            /// <returns></returns>
            public bool TryRemoveCollider(Collider col)
            {
                for (int i = 0; i < this.colliders.Count; i++)
                {
                    if (colliders[i] == col)
                    {
                        colliders.RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }
        }


    }
}