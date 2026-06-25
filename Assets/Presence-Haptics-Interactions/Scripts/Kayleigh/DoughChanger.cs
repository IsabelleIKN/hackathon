using UnityEngine;

public class DoughChanger : MonoBehaviour
{
    public GameObject doughTwo;

    private void Start()
    {
        if (doughTwo != null)
        {
            doughTwo.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dough"))
        {
            other.gameObject.SetActive(false);
            doughTwo.transform.position = other.transform.position;
            doughTwo.transform.rotation = other.transform.rotation;
            doughTwo.SetActive(true);

            Debug.Log("Dough changed into DoughTwo");
        }
    }
}