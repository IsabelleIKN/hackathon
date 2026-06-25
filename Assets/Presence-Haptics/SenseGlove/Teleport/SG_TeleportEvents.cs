using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG
{
    /// <summary> Teleport location (world space) is #3. </summary>
    [System.Serializable] public class SG_TeleportEvent : UnityEngine.Events.UnityEvent<Vector3> { }


    /// <summary> Responsible for generating teleport events, but not for the actual UI or activation. There's a different script responsible for that, so integrators only need this one. </summary>
    public class SG_TeleportEvents : SG_HandComponent
    {
        /// <summary> Used to activate / deactivate Teleport Intent  </summary>
        [Header("Teleport Parameters")]
        [SerializeField] private SG_BasicGesture pointGesture;

        /// <summary> Which layers with allow teleportation, when collided with. </summary>
        [SerializeField] private LayerMask teleportLayers;

        /// <summary> Time, in seconds, before we show the teleportation beam </summary>
        [SerializeField] private float showBeamAfter = 0.75f;
        /// <summary> Time, in seconds, after which the teleportation happens </summary>
        [SerializeField] private float teleportAfter = 1.5f;

        /// <summary> Maximum beam distance. </summary>
        [SerializeField] private float maxBeamDistance = 20.0f;

        public QueryTriggerInteraction collideTriggers = QueryTriggerInteraction.Ignore;

        /// <summary> Material used when a valid teleportation location is selected </summary>
        [Header("UI / Visuals")]
        [SerializeField] private Material validTeleportMaterial;
        /// <summary> Material used when an invalid teleportation location is selected. </summary>
        [SerializeField] private Material invalidTeleportMaterial;

        [SerializeField] private Transform teleportTimerObject;
        [SerializeField] private UnityEngine.UI.Slider timerVisual; //shows you the progress towards a teleport.
        [SerializeField] private GameObject beamMain; //The thing from which we point
        [SerializeField] private Transform beam; //the thing we stretch and scale to represent the beam. TODO: Replace this with a LineRenderer!

        //Events - for your convenience

        [Header("Events")]
        public UnityEngine.Events.UnityEvent TeleportBeamActivated = new UnityEngine.Events.UnityEvent();

        public UnityEngine.Events.UnityEvent TeleportBeamDeactivated = new UnityEngine.Events.UnityEvent();

        public SG_TeleportEvent DidTeleport = new SG_TeleportEvent();


        // Internal Member variables

        private Renderer beamRenderer; 

        private float teleportTimer = 0f;
        private float startTimer = 0f;
        
        

        private bool validHit = false;
        private Vector3 teleportDestination = new Vector3(0, 0, 0);

        // hard-coded values - TODO: Read these from the object proper on Initialize. This will only work for the one blue hand model!
        private static Vector3 rightBeamPos = new Vector3(0.0992f, -0.0161f, 0.0317f);
        private static Vector3 leftBeamPos = new Vector3(0.1033f, -0.0157f, -0.0309f);
        private static Vector3 beamScale = new Vector3(0.005f, 0.005f, 0.005f);
        private static Vector3 beamRot = new Vector3(0, 0, 0);

        private static Vector3 rightCanvasPos = new Vector3(0.0988f, -0.0114f, 0.053f);
        private static Vector3 rightCanvasRot = new Vector3(0, -35, -270);
        private static Vector3 leftCanvasPos = new Vector3(0.1038f, -0.0115f, -0.0529f);
        private static Vector3 leftCanvasRot = new Vector3(0, 40, 90);
        private static Vector3 canvasScale = new Vector3(1, 1, 1);


        protected override void LinkToHand_Internal(SG_TrackedHand newHand, bool firstLink)
        {
            base.LinkToHand_Internal(newHand, firstLink);

            Transform wrist = newHand.GetTransform(SG_TrackedHand.TrackingLevel.RenderPose, HandJoint.Wrist);

            // set the teleport canvas at the right location
            teleportTimerObject.SetParent(wrist);
            teleportTimerObject.localPosition = newHand.TracksRightHand() ? rightCanvasPos : leftCanvasPos;
            teleportTimerObject.localEulerAngles = newHand.TracksRightHand() ? rightCanvasRot : leftCanvasRot;
            teleportTimerObject.localScale = canvasScale;

            // set the teleport beam at the right location
            beamMain.transform.SetParent(wrist.transform);
            beamMain.transform.localPosition = newHand.TracksRightHand() ? rightBeamPos : leftBeamPos;
            beamMain.transform.localEulerAngles = beamRot;
            beamMain.transform.localScale = beamScale;

        }


        //TODO: Introduce a Teleport Cooldown? Currently done by reseting the TeleportTimer, which means you can 'keep walking'
        private void UpdateTeleport(float dT)
        {
            bool teleportDesired = (TrackedHand.gestureLayer.IsGestureMade(pointGesture) || TrackedHand.OverrideUse() > 0.5f) 
                && !TrackedHand.grabScript.IsGrabbing;

            // check if the gesture is made
            if (teleportDesired)
            {
                if (startTimer > showBeamAfter)
                {
                    UpdateBeamCollison(); //updates validHit.
                    if (!beamMain.activeSelf)
                    {
                        beamMain.SetActive(true); //started showing the beam
                        teleportTimerObject.gameObject.SetActive(true);
                        TeleportBeamActivated.Invoke();
                    }
                    teleportTimer = validHit ? teleportTimer + Time.deltaTime : 0; //resets if we do not hit a valid surface.
                    UpdateProgessVisual();
                    if (teleportTimer > teleportAfter)
                    {
                        DoTeleport();
                    }
                }
                startTimer += Time.deltaTime;
            }
            else
            {
                if (beamMain.activeSelf)
                {
                    beamMain.SetActive(false);
                    teleportTimerObject.gameObject.SetActive(false);
                    TeleportBeamDeactivated.Invoke();
                }
                teleportTimer = 0f;
                startTimer = 0f;
            }
        }

        private void UpdateProgessVisual()
        {
            if (this.timerVisual == null)
                return;
            this.timerVisual.value = Mathf.Clamp01(this.teleportTimer / this.teleportAfter);
        }

        /// <summary> Check the collision of the teleporter and update the beam visuals accordingly </summary>
        private void UpdateBeamCollison()
        {
            Ray raycast = new Ray(this.beamMain.transform.position, this.beamMain.transform.right); //right because we're using X forward...
            RaycastHit hit;
            validHit = Physics.Raycast(raycast, out hit, maxBeamDistance, teleportLayers, collideTriggers);

            if (validHit)
            {
                teleportDestination = hit.point;
                float d = (teleportDestination - beamMain.transform.position).magnitude; //size (m between points)
                beamMain.transform.localScale = new Vector3(d, beamMain.transform.localScale.y, beamMain.transform.localScale.z); //TODO: This assumes everything above BeamMain to be at scale 1,1,1
                beamRenderer.material = validTeleportMaterial;
            }
            else
            {
                beamMain.transform.localScale = new Vector3(maxBeamDistance, beamMain.transform.localScale.y, beamMain.transform.localScale.z);
                beamRenderer.material = invalidTeleportMaterial;
            }
        }


        /// <summary> Performs a teleport and resets the timer. </summary>
        private void DoTeleport()
        {
            teleportTimer = 0f;
            if ( SG_XR_SceneTrackingLinks.GetXRRigAndHeadTransforms(out Transform xrRig, out Transform headLocation) )
            {
                Vector3 dPos = headLocation.position - xrRig.transform.position;
                Vector3 newpos = new Vector3(teleportDestination.x - dPos.x, teleportDestination.y, teleportDestination.z - dPos.z);
                xrRig.transform.position = newpos;

                //TODO: Move Physics Hands onto the new location? (Next Frame?)
                StartCoroutine(TeleportPhysicsHands());
            }
            else
            {
                Debug.LogError("Did a Teleport but do not have access to all components (Head, XRRig). So teleportation will not happen.");
            }
            DidTeleport.Invoke(teleportDestination);
        }


        /// <summary> Teleports the Physics Hands to the actual location of the 'real hand' wherever possible. This prevents the 'flying hands' you see </summary>
        /// <returns></returns>
        private IEnumerator TeleportPhysicsHands()
        {
            yield return null;
            if (TrackedHand != null)
            {
                if (TrackedHand.handPhysics != null)
                    TrackedHand.handPhysics.PlaceHandAtRealLocation();
                if (TrackedHand.GetOtherHand(out SG_TrackedHand oppositeHand) && oppositeHand.handPhysics != null)
                {
                    oppositeHand.handPhysics.PlaceHandAtRealLocation();
                }
            }
        }


        protected override void CreateComponents()
        {
            base.CreateComponents();
            if (pointGesture == null)
                pointGesture = this.GetComponent<SG_BasicGesture>();

            beamRenderer = beam.GetComponent<Renderer>();

            beamMain.SetActive(false);
            teleportTimerObject.gameObject.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            SetupComponents();  
        }

        // Update is called once per frame
        void Update()
        {
            UpdateTeleport(Time.deltaTime);
        }

        
        private void OnDisable()
        {
            beamMain.SetActive(false);
            teleportTimerObject.gameObject.SetActive(false);
        }
    }
}