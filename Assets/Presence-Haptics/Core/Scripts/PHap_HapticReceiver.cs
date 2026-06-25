/*
 * Attached to a collider to denote a body part that cen receive haptic effects from 'emitters' inside the 
 * 
 * TODO: Enure this component's collider(s) are marked as IsTrigger
 * 
 * author:
 * tbr@senseglove.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Presence
{
    //[RequireComponent(typeof(Collider))] //Alternatively, you can attach this to a GameObject with a RigidBody, and a bunch of smaller, primitive colliders as children!
    public class PHap_HapticReceiver : MonoBehaviour
    {
        //--------------------------------------------------------------------------------
        // Member variables

        /// <summary> The Body part that this component + collider(s) represent </summary>
        [SerializeField] protected PHap_BodyPart bodyPart = PHap_BodyPart.Unknown;

        /// <summary> Optional: The origin of this body part for the purposes of Haptics. </summary>
        [SerializeField] protected Transform bodyPartOrigin;

        

        ///// <summary> If false, one is guesstimated based on Physics </summary>
        //public bool manualBoundingBox = true;

        //These are all local (relative to origin... but should they be? Or only use its orientation?)
        public Vector3 BoundingBoxCenter = Vector3.zero;
        public Vector3 BoundingBoxWidth = new Vector3(1.0f, 1.0f, 1.0f);


        //--------------------------------------------------------------------------------
        // Events

        /// <summary> Fires when this Emitter comes into contact with a Receiver, during OnTriggerEnter </summary>
        [Header("Events")]
        public PHap_HapticCollisionEvent OnHapticsEnter = new PHap_HapticCollisionEvent();
        /// <summary> Fires when this Emitter staus in contact with a Receiver, during OnTriggerStay  </summary>
        public PHap_HapticCollisionEvent OnHapticsStay = new PHap_HapticCollisionEvent();
        /// <summary> Fires when this Emitter stops contacting a Receiver, during OnTriggerExit </summary>
        public PHap_HapticCollisionEvent OnHapticsExit = new PHap_HapticCollisionEvent();



        //--------------------------------------------------------------------------------
        // Accessors

        public PHap_BodyPart BodyPart
        {
            get { return bodyPart; }
            set { bodyPart = value; }
        }

        public Transform Origin
        {
            get 
            {
                if (bodyPartOrigin == null)
                    bodyPartOrigin = this.transform;
                return bodyPartOrigin; 
            }
            set { bodyPartOrigin = value; }
        }

        /// <summary> Grab the collider(s) associated with this body part so we can set them up properly. </summary>
        /// <returns></returns>
        public virtual Collider[] GetColliders()
        {
            //TODO: A beter wat to link these that takes up less memory (and could work with multi-colliders?)
            return this.GetComponents<Collider>();
        }




        public static Vector3[] ExtractCorners(Collider collider)
        {
            Bounds bounds = collider.bounds;
            return CubePointsFromCorners(bounds.min, bounds.max);
        }

        /// <summary> Creates 8 points from a Bottom-front-left / Top-back-right corners of a cube  </summary>
        /// <param name="min"></param>
        /// <param name="tbr"></param>
        /// <returns></returns>
        public static Vector3[] CubePointsFromCorners(Vector3 bfl, Vector3 tbr)
        {
            Vector3[] corners = new Vector3[8];
            // Bottom plane
            corners[0] = new Vector3(bfl.x, bfl.y, bfl.z); // Bottom-front-left
            corners[1] = new Vector3(tbr.x, bfl.y, bfl.z); // Bottom-front-right
            corners[2] = new Vector3(bfl.x, bfl.y, tbr.z); // Bottom-back-left
            corners[3] = new Vector3(tbr.x, bfl.y, tbr.z); // Bottom-back-right

            // Top plane
            corners[4] = new Vector3(bfl.x, tbr.y, bfl.z); // Top-front-left
            corners[5] = new Vector3(tbr.x, tbr.y, bfl.z); // Top-front-right
            corners[6] = new Vector3(bfl.x, tbr.y, tbr.z); // Top-back-left
            corners[7] = new Vector3(tbr.x, tbr.y, tbr.z); // Top-back-right

            return corners;
        }


        public Vector3[] GetCurrentBoundingBoxWorldCorners()
        {
            Transform or = this.Origin;
            //Vector3 currCenter = or.position + (or.rotation * boundingBoxCenter);

            Vector3 halfSize = BoundingBoxWidth / 2;
            // Calculate the 8 corners of the cube. TransformPoint puts them in World Space relative to the Origin.
            return new Vector3[8]
            {
                or.TransformPoint( BoundingBoxCenter + new Vector3(-halfSize.x, -halfSize.y,  halfSize.z) ), // Bottom-front-left
                or.TransformPoint( BoundingBoxCenter + new Vector3( halfSize.x, -halfSize.y,  halfSize.z) ), // Bottom-front-right
                or.TransformPoint( BoundingBoxCenter + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z) ), // Bottom-back-left
                or.TransformPoint( BoundingBoxCenter + new Vector3( halfSize.x, -halfSize.y, -halfSize.z) ), // Bottom-back-right

                or.TransformPoint( BoundingBoxCenter + new Vector3(-halfSize.x,  halfSize.y,  halfSize.z) ), // Top-front-left
                or.TransformPoint( BoundingBoxCenter + new Vector3( halfSize.x,  halfSize.y,  halfSize.z) ), // Top-front-right
                or.TransformPoint( BoundingBoxCenter + new Vector3(-halfSize.x,  halfSize.y, -halfSize.z) ), // Top-back-left
                or.TransformPoint( BoundingBoxCenter + new Vector3( halfSize.x,  halfSize.y, -halfSize.z) )  // Top-back-right
            };
        }

        private void DrawCurrentBoundingBox()
        {
            PHap_Util.DrawDebugCube(GetCurrentBoundingBoxWorldCorners(), Color.yellow);
        }

        void OnDrawGizmosSelected()
        {
            DrawCurrentBoundingBox();
        }

    }
}