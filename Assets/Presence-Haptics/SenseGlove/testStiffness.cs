using Presence;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testStiffness : MonoBehaviour
{
    public UnityEngine.UI.Button toggleBtn;
    public UnityEngine.UI.Text stateText;

    private bool turnedOn = false;

    public PHap_HapticEffect stiffnessEffect;

    private PHap_EffectLocation[] effectLocations = new PHap_EffectLocation[]
    {
        new PHap_EffectLocation(PHap_BodyPart.LeftThumb),
        new PHap_EffectLocation(PHap_BodyPart.LeftIndexFinger),
        new PHap_EffectLocation(PHap_BodyPart.LeftMiddleFinger),
        new PHap_EffectLocation(PHap_BodyPart.LeftRingFinger),
        new PHap_EffectLocation(PHap_BodyPart.LeftPinky),

        new PHap_EffectLocation(PHap_BodyPart.RightThumb),
        new PHap_EffectLocation(PHap_BodyPart.RightIndexFinger),
        new PHap_EffectLocation(PHap_BodyPart.RightMiddleFinger),
        new PHap_EffectLocation(PHap_BodyPart.RightRingFinger),
        new PHap_EffectLocation(PHap_BodyPart.RightPinky),
    };


    void UpdateUI()
    {
        stateText.text = turnedOn ? "Stiffness On" : "Stiffness Off";
    }

    private void BtnClicked()
    {
        turnedOn = !turnedOn;
        UpdateUI();
        if (turnedOn)
        {
            for (int i = 0; i < this.effectLocations.Length; i++)
            {
                PHap_Core.PlayHapticEffect(stiffnessEffect, effectLocations[i]);
            }
        }
        else
        {
            for (int i=0; i<this.effectLocations.Length; i++)
            {
                PHap_Core.StopHapticEffect(stiffnessEffect, effectLocations[i]);
            }
        }
    }

    private void OnEnable()
    {
        toggleBtn.onClick.AddListener(BtnClicked);
    }

    private void OnDisable()
    {
        toggleBtn.onClick.RemoveListener(BtnClicked);
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateUI();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
