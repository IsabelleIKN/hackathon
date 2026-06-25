using UnityEngine;

public class PlateMixer : MonoBehaviour
{
    public GameObject dough;

    private GameObject egg;
    private GameObject butter;

    private bool hasEgg;
    private bool hasButter;

    private void Start()
    {
        // Hide dough at the start of the game
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
    }

    private void CheckRecipe()
    {
        if (hasEgg && hasButter)
        {
            egg.SetActive(false);
            butter.SetActive(false);

            dough.SetActive(true);
        }
    }
}