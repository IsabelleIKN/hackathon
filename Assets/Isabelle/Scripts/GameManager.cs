using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI eggTxt;
    [SerializeField] private TextMeshProUGUI flourTxt;
    [SerializeField] private TextMeshProUGUI butterTxt;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void EggSwitch (bool egg)
    {
        if(egg) 
        {
            eggTxt.text = "Y";
        }
        else
        {
            eggTxt.text = "N";
        }
    }

    public void FlourSwitch (bool flour)
    {
        if(flour) 
        {
            flourTxt.text = "Y";
        }
        else
        {
            flourTxt.text = "N";
        }
    }

    public void ButterSwitch (bool butter)
    {
        if(butter) 
        {
            butterTxt.text = "Y";
        }
        else
        {
            butterTxt.text = "N";
        }
    }
}
