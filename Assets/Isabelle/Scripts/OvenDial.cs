using UnityEngine;
using UnityEngine.UI;

public class OvenDial : MonoBehaviour
{
    [SerializeField] private Image gauge;
    [SerializeField] private Image timerVis;
    private float temperature = 0f;

    private float timer = 0f;
    public bool timerOn = false;

    void Start()
    {
        gauge.fillAmount = temperature;
        timerVis.fillAmount = 0f;
        timerOn = false;
        timer = 0f;
    }

    public void UpDial()
    {
        temperature += 0.1f;
        gauge.fillAmount = temperature;

        if(temperature >= 1)
        {
            timerOn = true;
        }
    }

    void Update()
    {
        if(timerOn)
        {
            timer += 0.1f * Time.deltaTime;
            timerVis.fillAmount = timer;

            if(timer >= 1)
            {
                Debug.Log("Timer done");
            }
        }
    }



}
