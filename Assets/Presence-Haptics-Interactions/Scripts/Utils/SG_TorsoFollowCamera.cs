using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Presence
{

    public class SG_TorsoFollowCamera : MonoBehaviour
    {
        [Header("The XRRig")]
        public GameObject xrRig;

        [Header("The main camera inside the XRRig")]
        public GameObject camera;

        [Header("The main camera inside the XRRig")]
        public GameObject Torso;

        [Header("The distance between the camera and the torso in height")]
        [Range(0, 1)]public float heightDifference = 0.5f;

        [Header("The distance the torso gets behind the camera")]
        [Range(0, 1)] public float backOff = 0.2f;

        void Start()
        {
            SetTorso();
        }

        void Update()
        {
            SetTorso();
        }

        private void SetTorso()
        {
            this.transform.position = new Vector3 (camera.transform.position.x, camera.transform.position.y - heightDifference, camera.transform.position.z);
            
            this.transform.localEulerAngles = new Vector3(0, camera.transform.localEulerAngles.y + xrRig.transform.localEulerAngles.y, 0);

            Torso.transform.localPosition = new Vector3(0, 0, -backOff);
        }
    }
}
