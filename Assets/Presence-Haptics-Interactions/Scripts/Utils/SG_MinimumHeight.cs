using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Presence
{
    /// <summary>
    /// This script sets the xr rig to a height that is minimal the minHeight at the moment of reset or start of the scene
    /// </summary>
    public class SG_MinimumHeight : MonoBehaviour
    {
        [Header("What is the minimum height we want the players eyes to have")]
        [Range(0, 2)] public float minHeight = 1.7f;

        [Header("What is the minimum height we want the players eyes to have")]
        [Range(0, 2)] public float maxHeight = 1.9f;

        [Header("Does the camera also recenter to the middle of the XR rig")]
        public bool recenterPos = false;

        [Header("The XRRig and the Main Camera")]
        public GameObject xrRig;
        public GameObject cameraOffset;
        public GameObject headCamera;

        [Header("The Object that stands at the position the camera resets to")]
        public GameObject resetPosition;

        [Header("Recenter on start of the scene?")]
        public bool onStart = false;

        [Header("Hotkey to recenter")]
        public KeyCode recenterHotKey = KeyCode.None;

        // private vars
        private Vector3 xrRigPosStart;
        private Vector3 xrRigRotStart;

        private void Start()
        {
            xrRigPosStart = resetPosition.transform.position;
            xrRigRotStart = resetPosition.transform.localEulerAngles;

            // wait a second for the headset to have its height adjusted before recentering
            if (onStart)
                StartCoroutine(ExecuteAfterTime(1));
        }

        private void Update()
        {
            if (Input.GetKeyDown(recenterHotKey))
                StartCoroutine(ExecuteAfterTime(0.2f));
        }

        public void Recenter()
        {
            /// Position Y
            // check to see if the camera needs to go higher
            if (headCamera.transform.position.y < minHeight)
            {
                float amount = headCamera.transform.position.y - minHeight;
                amount = Mathf.Abs(amount);

                xrRig.transform.position = new Vector3(xrRig.transform.position.x, xrRig.transform.position.y + amount, xrRig.transform.position.z);
            }
            else if (headCamera.transform.position.y > maxHeight)
            {
                float amount = headCamera.transform.position.y - maxHeight;
                amount = Mathf.Abs(amount);

                xrRig.transform.position = new Vector3(xrRigPosStart.x, xrRig.transform.position.y - amount, xrRigPosStart.z);
            }

            /// Rotation Y
            // set the Y rotation of the XRRig so the camera looks forward
            //xrRig.transform.localEulerAngles = xrRigRotStart;
            float amountRot = headCamera.transform.localEulerAngles.y;
            amountRot = -amountRot;
            amountRot = amountRot + xrRigRotStart.y;

            xrRig.transform.localEulerAngles = new Vector3(0, amountRot, 0);

            /// Position X and Z
            // Set the transform of the XR rig so the camera is in the center
            if (recenterPos)
            {
                float amountX = headCamera.transform.position.x - xrRig.transform.position.x;
                float amountZ = headCamera.transform.position.z - xrRig.transform.position.z;

                xrRig.transform.position = new Vector3(xrRigPosStart.x - amountX, xrRig.transform.position.y, xrRigPosStart.z - amountZ);
            }
        }

        IEnumerator ExecuteAfterTime(float time)
        {
            yield return new WaitForSeconds(time);

            Recenter();
        }
    }
}
