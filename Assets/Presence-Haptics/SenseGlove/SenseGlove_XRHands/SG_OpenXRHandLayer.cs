#define SG_UNITY_HANDS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if SG_UNITY_HANDS
using UnityEngine.XR.Hands; 
#endif

/*
 * A singleton-pattern that creates a GameObject in the scene if one does not exist. Subscribes to HandSubSystem(s) and chaches data for SG to use.
 * 
 * author
 * max@senseglove.com
 */

public class SG_OpenXRHandLayer : MonoBehaviour
{
    /// <summary> Instance that is created when a function is called. Internal functions should call Instance as opposed to using s_instance directly! </summary>
    private static SG_OpenXRHandLayer s_instance = null;

    /// <summary> Gets the Instance of PHap_Core. Creates one if it has yet to exist. </summary>
    public static SG_OpenXRHandLayer Instance
    {
        get
        {
            TryInitialize();
            return s_instance;
        }
    }

    /// <summary> Call this method to ensure the creation of a PHap_Core instance and initialize the API's. Will be automatically called when you grab the Instance. </summary>
    public static void TryInitialize()
    {
        if (s_instance != null) //after the first function call, s_instance should never be NULL again (unless someone deleted the Instance).
            return;

        s_instance = GameObject.FindObjectOfType<SG_OpenXRHandLayer>(); //first try and grab if from the scene. If it had existed, it would have called SetupInstance() already.
        if (s_instance == null) //Still NULL which means we could not find it.
        {
            GameObject coreObj = new GameObject("SG_OpenXRHandLayer");
            s_instance = coreObj.AddComponent<SG_OpenXRHandLayer>(); // Calls SetupInstance();
            Debug.Log("Created a new instance of SG_OpenXRHandLayer.");
        }
    }


    /// <summary> Called by instances of this class on Awake() to either register themselves as the active instance, or to delete themselves with a log(?) </summary>
    private void SetupInstance()
    {
        if (s_instance == null)
        {
            s_instance = this;
            DontDestroyOnLoad(s_instance);
            Setup();
        }
        else if (s_instance != this)
        {
            Debug.Log("SG_PresenceAPI Instance already exists. So we're deleting this instance");
            GameObject.Destroy(this);
        }
    }

    /// <summary>  Called by instances of this class on Awake() to either de-register themselves as the active instance. </summary>
    private void DisposeInstance()
    {
        if (s_instance == this)
        {
            //De Initialize
            Dispose();
            s_instance = null; //explicitly so we don't rely on GC to clear s_instance whenever.
        }
    }



    //-------------------------------------------------------------------------------------------------------------------------
    // Monobehaviour - Singleton

    private void Awake()
    {
        SetupInstance();
    }

    private void OnDestroy() //Won't be called during scene changes, since it's DontDestoryOnLoad. But someone might explictly call Destroy.
    {
        DisposeInstance();
    }



    /// <summary> Official Setup, considering this is the 'official' instance </summary>
    private void Setup()
    {

    }


    /// <summary>  Official Dispose, considering this is the 'official' instance </summary>
    private void Dispose()
    {
#if SG_UNITY_HANDS
        UnsubscribeHandSubsystem();
        m_Subsystem = null;
#endif
    }

    //-------------------------------------------------------------------------------------------------------------------------
    // Member Variables (Instance)

#if SG_UNITY_HANDS
    /// <summary> The subSystem to be used accross SG_OpenXRHandTracking implementation(s).  </summary>
    private XRHandSubsystem m_Subsystem;
    static readonly List<XRHandSubsystem> s_SubsystemsReuse = new List<XRHandSubsystem>();
    private bool openXR_leftTracked = false, openXR_rightTracked = false;
    private XRHand openXR_LeftHandData, openXR_RightHandData;



    //-------------------------------------------------------------------------------------------------------------------------
    // Functions (Instance)

    void SubscribeHandSubsystem()
    {
        if (m_Subsystem == null)
            return;

       // m_Subsystem.trackingAcquired += OnTrackingAcquired;
        m_Subsystem.trackingLost += OnTrackingLost;
        m_Subsystem.updatedHands += OnUpdatedHands;
    }

    void UnsubscribeHandSubsystem()
    {
        openXR_leftTracked = false;
        openXR_rightTracked = false;

        if (m_Subsystem == null)
            return;

        //m_Subsystem.trackingAcquired -= OnTrackingAcquired;
        m_Subsystem.trackingLost -= OnTrackingLost;
        m_Subsystem.updatedHands -= OnUpdatedHands;
    }


    //void OnTrackingAcquired(XRHand hand)
    //{
    //    switch (hand.handedness)
    //    {
    //        case Handedness.Left:
    //            //Left Hand is now visible eyyyy. But is it tracked...?
    //            break;

    //        case Handedness.Right:
    //            //Righteft Hand is now visible eyyyy. But is it tracked...?
    //            break;
    //    }
    //}


    void OnTrackingLost(XRHand hand)
    {
        switch (hand.handedness)
        {
            case Handedness.Left:
                openXR_leftTracked = false;
                break;

            case Handedness.Right:
                openXR_rightTracked = false;
                break;
        }
    }
     

    void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        // We have no game logic depending on the Transforms, so early out here
        // (add game logic before this return here, directly querying from
        // subsystem.leftHand and subsystem.rightHand using GetJoint on each hand)
        if (updateType == XRHandSubsystem.UpdateType.Dynamic)
            return;

        openXR_leftTracked = subsystem.leftHand.isTracked;
        openXR_LeftHandData = subsystem.leftHand;

        openXR_rightTracked = subsystem.rightHand.isTracked;
        openXR_RightHandData = subsystem.rightHand;
    }


    //-------------------------------------------------------------------------------------------------------------------------
    // Monobehaviour - other



    private void Update()
    {
        if (m_Subsystem != null && m_Subsystem.running) //don't do this check  is the subsystem is valid...
            return;

        SubsystemManager.GetSubsystems(s_SubsystemsReuse);
        var foundRunningHandSubsystem = false;
        for (var i = 0; i < s_SubsystemsReuse.Count; ++i)
        {
            var handSubsystem = s_SubsystemsReuse[i];
            if (handSubsystem.running)
            {
                UnsubscribeHandSubsystem();
                m_Subsystem = handSubsystem;
                foundRunningHandSubsystem = true;
                break;
            }
        }

        if (!foundRunningHandSubsystem)
            return;

        SubscribeHandSubsystem();
    }



    //-------------------------------------------------------------------------------------------------------------------------
    // External Function Calls (static)

    /// <summary> Returns true if the left or right hand is currently being tracked. </summary>
    /// <param name="rightHand"></param>
    /// <returns></returns>
    public static bool IsTracked(bool rightHand)
    {
        SG_OpenXRHandLayer inst = Instance;
        if (rightHand)
            return inst.openXR_rightTracked;
        else
            return inst.openXR_rightTracked;
    }

    public static bool GetXRHandPose(bool rightHand, out XRHand handPose)
    {
        SG_OpenXRHandLayer inst = Instance;
        bool tracked = rightHand ? inst.openXR_rightTracked : inst.openXR_leftTracked;
        if (tracked)
        {
            handPose = rightHand ? inst.openXR_RightHandData : inst.openXR_LeftHandData;
            return true;
        }
        handPose = new XRHand();
        return false;
    }
    
#endif

}
