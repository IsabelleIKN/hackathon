using UnityEngine;
using UnityEngine.UI;

public class OvenDial : MonoBehaviour
{
    [SerializeField] private Image gauge;
    private float temperature = 0f;

    void Start()
    {
        gauge.fillAmount = temperature;
    }

    public void UpDial()
    {
        temperature += 0.1f;
        gauge.fillAmount = temperature;
    }
}
