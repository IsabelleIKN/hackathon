using Presence;
using UnityEngine;

public class TriggerVestHaptics : MonoBehaviour
{
    public Transform effectLocation; //use it also for size

    public PHap_HapticReceiver torso;

    public KeyCode triggerKey = KeyCode.Space;

    public Transform pointLocation;

    public MappingStrategy mappingStrategy = MappingStrategy.Discrete;

    public ActuatorToTrigger result = ActuatorToTrigger.Unknown;

    public Material defaultMaterial;
    public Material chosenMaterial;

    public MeshRenderer[] visuals = new MeshRenderer[0];

    private Collider col = null;

    public void TestTrigger()
    {
        Vector3 contactPoint = col is MeshCollider && !((MeshCollider)col).convex ? Vector3.zero : col.ClosestPoint(this.effectLocation.position); //Function is not compatibel with non-convex mesh colliders.
                                                                                                                                              //ClosestPoint calculates the closest point on col to my origin. Though if my origin in inside the collider, it returns the input. It's all in World Space.
        Vector3 lscale = effectLocation.lossyScale;
        float effectSize = Mathf.Max(Mathf.Max(lscale.x, lscale.y), lscale.z);
        Vector3 localPos = torso.Origin.InverseTransformPoint(contactPoint);
        PHap_EffectLocation calc = new PHap_EffectLocation(torso.BodyPart, localPos, effectSize, torso.BoundingBoxCenter, torso.BoundingBoxWidth);

        pointLocation.position = contactPoint;
        TestTrigger(calc);
    }

    public static Vector3 NormalizeLocation(PHap_EffectLocation location)
    {
        Vector3 localPos = location.LocalPosition;
        Vector3 boxSize = location.BoundingBoxSize;
        return new Vector3(
            localPos.x / (boxSize.x * 0.5f),
            localPos.y / (boxSize.y * 0.5f),
            localPos.z / (boxSize.z * 0.5f)
            );
    }


    public void SetMaterials(Material color)
    {
        foreach (MeshRenderer rend in this.visuals)
        {
            rend.material = color;
        }
    }

    public void SelectPreview(ActuatorToTrigger trigger)
    {
        int listIndex = ((int)trigger) - 1;
        for (int i=0; i<this.visuals.Length; i++)
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

        //Forwd / Backwd check
        int fwd = 2;    // +Z
        int up = 1;     // +Y
        int right = 0;   // +X

        

        //Only ever trigger one or the other.
        if (this.mappingStrategy == MappingStrategy.Discrete)
        {
            result = ToActuatorLocation_Discrete(location);
        }


        Debug.Log("Result: " + this.result.ToString());
        SelectPreview(result);
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
                if (isUp)
                    return isRight ? ActuatorToTrigger.BackUpperRight : ActuatorToTrigger.BackUpperLeft;
                //else
                //    return isRight ? ActuatorToTrigger.BackLowerRight : ActuatorToTrigger.BackLowerLeft;
            }
        }
        return ActuatorToTrigger.Unknown;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        col = torso.GetComponent<Collider>();
        SelectPreview(ActuatorToTrigger.Unknown);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            TestTrigger();
        }
    }
}
