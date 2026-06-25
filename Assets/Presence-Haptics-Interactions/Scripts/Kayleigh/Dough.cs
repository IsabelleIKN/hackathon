using UnityEngine;

public class Dough : MonoBehaviour
{
    public GameObject dough;

    private GameObject egg;
    private GameObject butter;
    private GameObject flour;

    private bool hasEgg;
    private bool hasButter;
    private bool hasFlour;

    private void Start()
    {
        Debug.Log("PlateMixer started");
        if (dough != null)
        {
            dough.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag("Egg"))
        {
            hasEgg = true;
            egg = other.gameObject;
        }

        if (other.CompareTag("Butter"))
        {
            hasButter = true;
            butter = other.gameObject;
        }

        if (other.CompareTag("Flour"))
        {
            hasFlour = true;
            flour = other.gameObject;
        }
         Debug.Log("Entered trigger: " + other.name);
        CheckRecipe();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Egg"))
        {
            hasEgg = false;
            egg = null;
        }

        if (other.CompareTag("Butter"))
        {
            hasButter = false;
            butter = null;
        }

        if (other.CompareTag("Flour"))
        {
            hasFlour = false;
            flour = null;
        }
    }

    private void CheckRecipe()
    {
        if (hasEgg && hasButter && hasFlour)
        {
            egg.SetActive(false);
            butter.SetActive(false);
            flour.SetActive(false);

            dough.SetActive(true);
        }
    }
}