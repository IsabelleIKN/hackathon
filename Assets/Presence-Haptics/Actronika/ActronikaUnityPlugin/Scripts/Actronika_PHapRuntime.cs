using Skinetic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Presence
{
    public class Actronika_PHapRuntime : MonoBehaviour
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // Singleton Pattern but via a GameObject + Component + DontDestroyOnLoad - Allows UnityEvents.

        /// <summary> Instance that is created when a function is called. Internal functions should call Instance as opposed to using s_instance directly! </summary>
        private static Actronika_PHapRuntime s_runtimeInstance = null;
        private static Skinetic.ISkinetic m_skineticInstance = null;

        [SerializeField]
        private bool m_enableAutoConnection = false;
        [SerializeField]
        private SkineticDevice.OutputType m_outputType = SkineticDevice.OutputType.E_AUTODETECT;
        [SerializeField]
        private Int32 m_serialNumber = 0;

        /// <summary> Gets the Instance of PHap_Core. Creates one if it has yet to exist. </summary>
        public static Actronika_PHapRuntime Instance
        {
            get
            {
                TryInitialize();
                return s_runtimeInstance;
            }
        }

        /// <summary> Gets the Instance of Skinetic. Creates one if it has yet to exist. </summary>
        public ISkinetic SkineticInstance
        {
            get
            {
                return m_skineticInstance;
            }
        }

        /// <summary> Call this method to ensure the creation of a PHap_Core instance and initialize the API's. Will be automatically called when you grab the Instance. </summary>
        public static void TryInitialize()
        {
            if (s_runtimeInstance != null) //after the first function call, s_runtimeInstance should never be NULL again (unless someone deleted the Instance).
                return;

            s_runtimeInstance = GameObject.FindObjectOfType<Actronika_PHapRuntime>(); //first try and grab if from the scene. If it had existed, it would have called SetupInstance() already.
            if (s_runtimeInstance == null) //Still NULL which means we could not find it.
            {
                Debug.Log("Creating a new instance of Actronika_PHapRuntime.");
                GameObject coreObj = new GameObject("Actronika_PHap Runtime");
                s_runtimeInstance = coreObj.AddComponent<Actronika_PHapRuntime>(); // Calls SetupInstance();
            }

            //init m_skineticInstance using define

        }

        public static void TryDeinitialize()
        {
            if (s_runtimeInstance == null) //already de-initialized
                return;

            s_runtimeInstance.InitDisconnect();
        }

        /// <summary> Called by instances of this class on Awake() to either register themselves as the active instance, or to delete themselves with a log(?) </summary>
        private void SetupInstance()
        {
            if (s_runtimeInstance == null)
            {
                s_runtimeInstance = this;
                DontDestroyOnLoad(s_runtimeInstance);

                if(m_skineticInstance == null)
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                    m_skineticInstance = new SkineticAndroid();
#else
                    m_skineticInstance = new SkineticWrapping();
#endif
                    m_skineticInstance.InitInstance();
                }
            }
            else if (s_runtimeInstance != this)
            {
                Debug.Log("SG_PHapRuntime Instance already exists. So we're deleting this instance");
                if(m_skineticInstance != null)
                {
                    m_skineticInstance.DeinitInstance();
                    m_skineticInstance = null;
                }
                GameObject.Destroy(this);
            }
        }

        /// <summary>  Called by instances of this class on Awake() to either de-register themselves as the active instance. </summary>
        private void DisposeInstance()
        {
            if (s_runtimeInstance == this)
            {
                if (m_skineticInstance != null)
                {
                    m_skineticInstance.DeinitInstance();
                    m_skineticInstance = null;
                }
                //De Initialize
                s_runtimeInstance = null; //explicitly so we don't rely on GC to clear s_instance whenever.
            }
        }

   


        //-------------------------------------------------------------------------------------------------------------------------
        // Monobehaviour

        private void Awake()
        {
            SetupInstance();
            if (m_enableAutoConnection)
                StartCoroutine(AutoConnectionCoroutine());
            else
                InitConnect();
        }

        private void OnDestroy() //Won't be called during scene changes, since it's DontDestoryOnLoad. But someone might explictly call Destroy.
        {
            StopAllCoroutines();
            m_skineticInstance.Disconnect(); //fast disconnection initiated 
            DisposeInstance();
        }


        //------------------------------------------------------------------------------------------------------------
        // Connection handling

        public bool DeviceConnected()
        {
            SkineticDevice.ConnectionState state = m_skineticInstance.ConnectionStatus();
            switch(state)
            {
                case SkineticDevice.ConnectionState.E_CONNECTING:  //connection is not valid yet but answer is yes
                    return true;
                case SkineticDevice.ConnectionState.E_CONNECTED:
                    return true;
                default: return false;
            }
        }

        public void InitConnect()
        {
            StartCoroutine(ConnectCoroutine());
        }

        public void InitDisconnect()
        {
            StartCoroutine(DisconnectCoroutine());
        }

        IEnumerator ConnectCoroutine()
        {
            SkineticDevice.ConnectionState state = m_skineticInstance.ConnectionStatus();
            m_skineticInstance.Connect(m_outputType, (uint)m_serialNumber);
            while (state != SkineticDevice.ConnectionState.E_CONNECTED)
            {
                yield return null;
                state = m_skineticInstance.ConnectionStatus();
            }
            yield return null;
        }

        IEnumerator DisconnectCoroutine()
        {
            SkineticDevice.ConnectionState state = m_skineticInstance.ConnectionStatus();
            m_skineticInstance.Disconnect();
            while(state != SkineticDevice.ConnectionState.E_DISCONNECTED)
            {
                yield return null;
                state = m_skineticInstance.ConnectionStatus();
            }
            yield return null;
        }

        IEnumerator AutoConnectionCoroutine()
        {
            while (m_enableAutoConnection)
            {
                if(m_skineticInstance.ConnectionStatus() == SkineticDevice.ConnectionState.E_DISCONNECTED)
                    m_skineticInstance.Connect(m_outputType, (uint)m_serialNumber);
                yield return new WaitForSeconds(10);
            }
            yield return null;
        }

    }
}
