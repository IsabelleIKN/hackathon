using Presence;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;

public class PHap_ElitacDemo : MonoBehaviour
{

    public TMP_Dropdown locationSelect;
    public TMP_Dropdown effectSelect;

    public Toggle loopingToggle;

    public Button btn_playEffect;
    public Button btn_stopAllHaptics;

    public PHap_BodyPart[] bodyParts = new PHap_BodyPart[]
    {
        PHap_BodyPart.Torso,
        PHap_BodyPart.LeftChest,
        PHap_BodyPart.RightChest,
        PHap_BodyPart.LeftWaist,
        PHap_BodyPart.RightWaist,
        PHap_BodyPart.LeftUpperLeg,
        PHap_BodyPart.RightUpperLeg,
        PHap_BodyPart.LeftHandPalm //DEBUG for SG Effect.
    };


    public bool grabEffectsFromChildren = false;
    public PHap_HapticEffect[] testEffects = new PHap_HapticEffect[0];

    public PHap_InterhapticsImpl IH_Instance;
    public TMP_Text lbl_IHPresent;
    private Coroutine updateRoutine = null;

    private bool keepTimer = false;

    private IEnumerator UpdateConnection()
    {
        keepTimer = true;
        do
        {
            lbl_IHPresent.text = IH_Instance.DeviceConnected() ? "Connected" : "Not Connected";
            yield return new WaitForSeconds(1.0f);

        } while (keepTimer);
    }


    public PHap_HapticEffect GetCurrentEffect()
    {
        int selEff = effectSelect.value;
        return (selEff > -1 && selEff < testEffects.Length) ? testEffects[selEff] : null;
    }

    private void PlayEffect()
    {
        Debug.Log("Playing effect");

        
        PHap_HapticEffect eff = GetCurrentEffect();

        int selLoc = locationSelect.value;
        PHap_BodyPart loc = (selLoc > -1 && selLoc < bodyParts.Length) ? bodyParts[selLoc] : PHap_BodyPart.Unknown;

        if (eff != null && loc != PHap_BodyPart.Unknown)
        {
            PHap_Core.PlayHapticEffect(eff, new PHap_EffectLocation(loc));
        }
        else
        {
            Debug.Log("Invalid location or effect");
        }
    }

    /// <summary> Stop any and all active Haptic effects from this script. </summary>
    private void StopHaptics()
    {
        Debug.Log("Stopping Active Effects");
        foreach (PHap_BodyPart part in bodyParts)
        {
            PHap_EffectLocation loc = new PHap_EffectLocation(part);
            foreach (PHap_HapticEffect effect in testEffects)
            {
                PHap_Core.StopHapticEffect(effect, loc);
            }
        }
    }


    private void Start()
    {
        locationSelect.ClearOptions();
        var options = bodyParts
    .       Select(d => d.ToString())
            .ToList();
        locationSelect.AddOptions(options);

        locationSelect.value = 0;
        locationSelect.RefreshShownValue();


        if (grabEffectsFromChildren)
        {
            List<PHap_HapticEffect> temp = new List<PHap_HapticEffect>();
            foreach (var eff in testEffects)
            {
                if (eff != null && !temp.Contains(eff))
                    temp.Add(eff);
            }

            PHap_HapticEffect[] found = this.gameObject.GetComponentsInChildren<PHap_HapticEffect>();
            foreach (PHap_HapticEffect eff in found)
            {
                if (eff != null && !temp.Contains(eff))
                    temp.Add(eff);
            }
            this.testEffects = temp.ToArray();
        }

        effectSelect.ClearOptions();
        options = testEffects
    .           Select(d => d.name)
                .ToList();
        effectSelect.AddOptions(options);

        effectSelect.value = 0;
        effectSelect.RefreshShownValue();
    }




    private void OnEnable()
    {
        btn_playEffect.onClick.AddListener(PlayEffect);
        btn_stopAllHaptics.onClick.AddListener(StopHaptics);

        updateRoutine = StartCoroutine(UpdateConnection());
    }

    private void OnDisable()
    {
        btn_playEffect.onClick.RemoveListener(PlayEffect);
        btn_stopAllHaptics.onClick.RemoveListener(StopHaptics);

        keepTimer = false;
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }
    }


}
