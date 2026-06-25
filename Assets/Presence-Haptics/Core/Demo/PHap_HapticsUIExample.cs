using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Example on how to use the "Presence Haptics API" via PHap_Core (no VR, animation or other complicated settings).
 * This one uses UI Buttons that are associated with a Body part to play an effect
 * The effect to play is chosen using buttons from a different list.
 * 
 * author:
 * max@senseglove.com
 */

namespace Presence
{

    /// <summary> Uses Buttons Connected with PHap_HapticReceivers to play Haptics at a certain location </summary>
    public class PHap_HapticsUIExample : MonoBehaviour
    {
        /// <summary> All buttons to play effects on locations </summary>
        public PHap_UIButtonHaptics[] hapticsButtons = new PHap_UIButtonHaptics[0];

        /// <summary> All buttons to scwitch effects </summary>
        public PHap_EffectUIButton[] effectButtons = new PHap_EffectUIButton[0];

        /// <summary> EffectSize in pixel the current Unity Rect. </summary>
        public float effectSize = 100.0f;

        /// <summary> Currrently chosen haptic effect </summary>
        private PHap_EffectUIButton activeEffect = null;

        /// <summary> To lost the implementations and connection states. </summary>
        public TMPro.TMP_Text implementationText;


        private PHap_HapticEffect lastNonTimedEffect = null;
        private PHap_EffectLocation lastNonTimedLocation = null;

        /// <summary> Text that shows which implementations are loaded and if their associated device(s) are connected. </summary>
        public string ImplText
        {
            get { return this.implementationText != null ? implementationText.text : ""; }
            set { if (this.implementationText != null) { implementationText.text = value; } }
        }


        private void StopNonTimed()
        {
            if (lastNonTimedEffect != null)
            {
                PHap_Core.StopHapticEffect(lastNonTimedEffect, lastNonTimedLocation);
                lastNonTimedEffect = null;
                lastNonTimedLocation = null;
            }
        }

        private void CheckNonTimed(PHap_HapticEffect effect, PHap_EffectLocation location)
        {
            if (!effect.IsTimedEffect)
            {
                lastNonTimedEffect = effect;
                lastNonTimedLocation = location;
            }
        }

       
        /// <summary> Coroutine to keep updating the list implentation(s) and device(s). </summary>
        /// <returns></returns>
        private IEnumerator UpdateImplementationStates()
        {
            while (this.isActiveAndEnabled)
            {
                PHap_Settings settings = PHap_Settings.GetSettings();
                if (settings.implementations.Count > 0)
                {
                    string txt = "";
                    for (int i=0; i<settings.implementations.Count; i++)
                    {
                        if (i != 0) { txt += "\n"; }
                        txt += settings.implementations[i].GetImplementationID() + ":\n" + (settings.implementations[i].DeviceConnected() ? "Device(s) connected" : "No device(s)") + "\n";
                    }
                    ImplText = txt;
                }
                else
                {
                    ImplText = "N\\A";
                }
                yield return new WaitForSeconds(1.0f); //update every second 
            }
        }

        /// <summary>Event handler for when one of our Play effect here buttons is pressed. </summary>
        /// <param name="button"></param>
        /// <param name="localPosition"></param>
        /// <param name="normalizedPosition"></param>
        public void HapticsButtonClicked(PHap_UIButtonHaptics button, Vector2 localPosition, Vector2 localSize)
        {
            Debug.Log("Playing effect(s) on " + button.hapticLocation);

            if (activeEffect == null)
                return;

            /* Received 2D coordinate in a centered rect, lets transform it into an approximate 
             * 3D coordinate assuming the bodypart is a cylinder. Any transformation is valide 
             * as long as is close enough of a human shape. Here we will apply the rect to a 
             * semi cylinder to represente the front face of the bodypart to match the UI visual.
              * The half cylinder will have a radius of half of the width of the button and the same height as the button.*/

            PHap_EffectLocation location = new PHap_EffectLocation(
                button.hapticLocation,
                new Vector3(localPosition.x, localPosition.y, Mathf.Sqrt(Mathf.Pow(localSize.x / 2, 2) - Mathf.Pow(localPosition.x, 2))),
                effectSize,
                Vector3.zero, //area is centered
                new Vector3(localSize.x, localSize.y, localSize.x)
            );
            Debug.Log($"Position: [{localPosition.x}, {localPosition.y}, {Mathf.Sqrt(Mathf.Pow(localSize.x / 2, 2) - Mathf.Pow(localPosition.x, 2))}]   --- Size: [{localSize.x}, {localSize.y}, {localSize.x}] ");

            StopNonTimed(); //stops any non-timed haptic effects that are still playing.
            PHap_HapticEffect effect = activeEffect.associatedEffect.Count > 1 && button.hapticLocation == PHap_BodyPart.Torso ? activeEffect.associatedEffect[1] : activeEffect.associatedEffect[0];
            PHap_Core.PlayHapticEffect(effect, location);
            CheckNonTimed(effect, location);
        }


        /// <summary> Event handler for when an effect button is clicked. </summary>
        /// <param name="button"></param>
        public void EffectButtonClicked(PHap_EffectUIButton button)
        {
            //Debug.Log("Selecting button to play " + button.associatedEffect.name);
            SelectEffect(button);
        }

        private void SelectEffect(PHap_EffectUIButton effect)
        {
            if (effect == this.activeEffect)
                return; //no need to update if the button is pressed twice.

            for (int i=0; i<effectButtons.Length; i++)
            {
                if (effectButtons[i] == effect)
                {
                    activeEffect = effect;
                    effectButtons[i].SetColors(Color.black, Color.white);
                }
                else
                {
                    effectButtons[i].SetColors(Color.white, Color.black);
                }
            }
            //TODO: Enable / Disable all body part buttons that support / not support the chosen modality
            PHap_HapticModality type = activeEffect.associatedEffect[0].EffectType; //Retrieve the effect type from this button.
            for (int i = 0; i < this.hapticsButtons.Length; i++)
            {
                hapticsButtons[i].BtnInteractable = PHap_Core.SupportsHaptics(type, hapticsButtons[i].hapticLocation);
            }

            StopNonTimed();
        }

        /// <summary> Returns true if this Body Part exists inside a list </summary>
        /// <param name="bp"></param>
        /// <param name="bpList"></param>
        /// <returns></returns>
        private static bool InList(PHap_BodyPart bp, PHap_BodyPart[] bpList)
        {
            foreach (PHap_BodyPart part in bpList)
            {
                if (part == bp)
                    return true;
            }
            return false;
        }



        private void OnEnable()
        {
            PHap_Core.TryInitialize(); //make sure it;s up and running, though this is optional.

            PHap_BodyPart[] supportedBodyParts = PHap_Settings.GetSettings().GetAllSupportedHapticLocations();
            for (int i=0; i<hapticsButtons.Length; i++)
            {
                hapticsButtons[i].BtnInteractable = InList(hapticsButtons[i].hapticLocation, supportedBodyParts); //if this button's body part does not exist in one we support, disable it
                hapticsButtons[i].OnButtonClicked.AddListener(HapticsButtonClicked);
            }

            for (int i = 0; i < this.effectButtons.Length; i++)
            {
                effectButtons[i].ButtonPressed.AddListener(EffectButtonClicked);
            }
            if (effectButtons.Length > 0)
            {
                SelectEffect( effectButtons[0] );
            }

            StartCoroutine(UpdateImplementationStates());
        }

        private void OnDisable()
        {
            for (int i = 0; i < hapticsButtons.Length; i++)
            {
                hapticsButtons[i].OnButtonClicked.RemoveListener(HapticsButtonClicked);
            }
            for (int i = 0; i < this.effectButtons.Length; i++)
            {
                effectButtons[i].ButtonPressed.RemoveListener(EffectButtonClicked);
            }
        }



    }
}