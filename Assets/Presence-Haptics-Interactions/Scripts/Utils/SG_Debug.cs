//using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Presence
{

    public class SG_Debug : MonoBehaviour
    {
        public bool debugHandColliders = true;

        public GameObject[] debugGameObjects;

        //public bool setDebugText = false;

        //public GameObject canvas;

        //public TMP_Text debugText;

        void Start()
        {
            for (int i = 0; i < debugGameObjects.Length; i++) 
            {  
                MeshRenderer meshRenderer = debugGameObjects[i].GetComponent<MeshRenderer>();

                meshRenderer.enabled = debugHandColliders == true ? true : false;
            }

            //if (canvas != null)
            //{
            //    canvas.SetActive(setDebugText ? true : false);

            //    if (setDebugText)
            //        SetText();
            //}
        }

        private void SetText()
        {
            //debugText.text = "Is connected: " + PhotonNetwork.IsConnected.ToString() + " - Room: " + PhotonNetwork.CurrentRoom;
        }

    }
}
