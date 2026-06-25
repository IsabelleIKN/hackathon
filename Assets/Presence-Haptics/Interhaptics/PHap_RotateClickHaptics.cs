using Presence;
using UnityEngine;
using UnityEngine.UI;
using static TriggerVestHaptics;

public class PHap_RotateClickHaptics : MonoBehaviour
{
    public Slider slider;
    public Transform cylinder;

    public Vector3 startRotation;
    public Vector3 rotationAxis = Vector3.up;


    public Camera cam;
    public LayerMask hitMask = ~0; // everything by default

    public Transform previewObj;

    public PHap_HapticReceiver torso;


    public MappingStrategy mappingStrategy = MappingStrategy.Discrete;

    public ActuatorToTrigger result = ActuatorToTrigger.Unknown;

    public Material defaultMaterial;
    public Material chosenMaterial;

    public MeshRenderer[] visuals = new MeshRenderer[0];

    public PHap_ElitacDemo demoScript;

    public void SelectPreview(ActuatorToTrigger trigger)
    {
        int listIndex = ((int)trigger) - 1;
        for (int i = 0; i < this.visuals.Length; i++)
        {
            this.visuals[i].material = i == listIndex ? this.chosenMaterial : this.defaultMaterial;
        }
    }


    public enum ActuatorToTrigger
    {
        Unknown = 0,
        FrontChestLeft,
        FrontChestRight,
        FrontBellyLeft,
        FrontBellyRight,
        BackUpperLeft,
        BackUpperRight,
        //BackLowerLeft,
        //BackLowerRight
    }

    public void TestTrigger(Collider col)
    {
        Vector3 contactPoint = col is MeshCollider && !((MeshCollider)col).convex ? Vector3.zero : col.ClosestPoint(this.previewObj.position); //Function is not compatibel with non-convex mesh colliders.
                                                                                                                                                   //ClosestPoint calculates the closest point on col to my origin. Though if my origin in inside the collider, it returns the input. It's all in World Space.
        Vector3 lscale = previewObj.lossyScale;
        float effectSize = Mathf.Max(Mathf.Max(lscale.x, lscale.y), lscale.z);
        Vector3 localPos = torso.Origin.InverseTransformPoint(contactPoint);
        PHap_EffectLocation calc = new PHap_EffectLocation(torso.BodyPart, localPos, effectSize, torso.BoundingBoxCenter, torso.BoundingBoxWidth);
        TestTrigger(calc);
    }


    public enum MappingStrategy
    {
        Discrete, //Simplest form based on the coordinates relative to the origin. Works 80% of the time.
    }

    public void TestTrigger(PHap_EffectLocation location)
    {
        this.result = ActuatorToTrigger.Unknown;

        //entry point into the Presence API.
        Vector3 normalized = NormalizeLocation(location);

        Debug.Log(normalized);


        //Only ever trigger one or the other.
        if (this.mappingStrategy == MappingStrategy.Discrete)
        {
            result = ToActuatorLocation_Discrete(location);
        }

        PHap_HapticEffect eff = demoScript.GetCurrentEffect();

        //PHap_BodyPart bPart = Actuator_to_BP(result);
        //location.BodyPart = bPart;
        PHap_Core.PlayHapticEffect(eff, location); //we're letting the IHImplementation decide this one based on their mapping strategy

        Debug.Log("Result: " + this.result.ToString());
        SelectPreview(result);
    }


    //This is for the one specific prototype!
    public static PHap_BodyPart Actuator_to_BP(ActuatorToTrigger bodyPart)
    {
        switch (bodyPart)
        {
            case ActuatorToTrigger.FrontChestLeft:
                return PHap_BodyPart.LeftChest;
            case ActuatorToTrigger.FrontChestRight:
                return PHap_BodyPart.RightChest;

            case ActuatorToTrigger.FrontBellyLeft:
                return PHap_BodyPart.LeftWaist;
            case ActuatorToTrigger.FrontBellyRight:
                return PHap_BodyPart.RightWaist;

            case ActuatorToTrigger.BackUpperLeft:
                return PHap_BodyPart.LeftUpperLeg;
            case ActuatorToTrigger.BackUpperRight:
                return PHap_BodyPart.RightUpperLeg;

            default:
                return PHap_BodyPart.Unknown;
        }
    }




    public static ActuatorToTrigger ToActuatorLocation_Discrete(PHap_EffectLocation location)
    {
        //entry point into the Presence API.
        Vector3 normalized = NormalizeLocation(location);

        Debug.Log(normalized);

        //Forwd / Backwd check
        int fwd = 2;    // +Z
        int up = 1;     // +Y
        int right = 0;   // +X

        bool isFront = normalized[fwd] > 0;
        bool isRight = normalized[right] > 0;
        bool isUp = normalized[up] > -1;

        switch (location.BodyPart)
        {
            case PHap_BodyPart.LeftChest:
                return isFront ? ActuatorToTrigger.FrontChestLeft : ActuatorToTrigger.BackUpperLeft;
            case PHap_BodyPart.RightChest:
                return isFront ? ActuatorToTrigger.FrontChestRight : ActuatorToTrigger.BackUpperRight;
            case PHap_BodyPart.LeftWaist:
                return ActuatorToTrigger.FrontBellyLeft;
            case PHap_BodyPart.RightWaist:
                return ActuatorToTrigger.FrontBellyRight;
        }
        if (location.BodyPart == PHap_BodyPart.Torso)
        {
            if (isFront) //in the front of the vest, we have 4 of them there
            {
                if (isUp)
                    return isRight ? ActuatorToTrigger.FrontChestRight : ActuatorToTrigger.FrontChestLeft;
                else
                    return isRight ? ActuatorToTrigger.FrontBellyRight : ActuatorToTrigger.FrontBellyLeft;
            }
            else //in the back, so there's two options
            {
                //if (isUp)
                    return isRight ? ActuatorToTrigger.BackUpperRight : ActuatorToTrigger.BackUpperLeft;
                //else
                //    return isRight ? ActuatorToTrigger.BackLowerRight : ActuatorToTrigger.BackLowerLeft;
            }
        }
        return ActuatorToTrigger.Unknown;
    }






    private void Start()
    {
        if (cam == null)
            cam = Camera.main;

        slider.minValue = 0f;
        slider.maxValue = 1f;

        slider.onValueChanged.AddListener(OnSliderValueChanged);

        OnSliderValueChanged(slider.value);
    }

    private void OnSliderValueChanged(float value)
    {
        float angle = value * -360f;

        Quaternion rotation =
            Quaternion.Euler(startRotation) *
            Quaternion.AngleAxis(angle, rotationAxis);

        cylinder.rotation = rotation;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hitMask))
            {
                //Debug.Log("Hit object: " + hit.collider.name);
                //Debug.Log("Hit point: " + hit.point);

                //// If you specifically want CapsuleCollider:
                //CapsuleCollider capsule = hit.collider as CapsuleCollider;
                //if (capsule != null)
                //{
                //    Debug.Log("Hit a CapsuleCollider!");

                //    Vector3 localPoint =
                //        capsule.transform.InverseTransformPoint(hit.point);

                //    Debug.Log("Local hit position: " + localPoint);
                //}

                if (previewObj != null)
                {
                    previewObj.position = hit.point;
                    TestTrigger(hit.collider);
                }
            }
        }
    }
}
