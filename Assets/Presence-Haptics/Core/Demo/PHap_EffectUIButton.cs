using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Button to choose which effect to play. Has an associated HapticEffect and fires an event.
 * 
 * author
 * max@senseglove.com
 */

namespace Presence
{
    /// <summary> Button to choose which effect to play. Has an associated HapticEffect and fires an event. </summary>
    public class PHap_EffectUIButton : MonoBehaviour
    {
        [System.Serializable] public class PHap_UIButtonEvent : UnityEngine.Events.UnityEvent<PHap_EffectUIButton> { }

        /// <summary> The button to press </summary>
        public UnityEngine.UI.Button button;

        /// <summary> Button Text (for changing the name and colour) </summary>
        private TMPro.TMP_Text btnText = null;

        /// <summary> The associated Haptic Effect to play </summary>
        public List<PHap_HapticEffect> associatedEffect; //todo temporary list until transcoding is complete

        /// <summary> Event to hook into </summary>
        public PHap_UIButtonEvent ButtonPressed;


        /// <summary> Set the button color </summary>
        public Color ButtonColor
        {
            get 
            {
                UnityEngine.UI.ColorBlock btnCol = button.colors;
                return btnCol.normalColor;
            }
            set 
            {
                UnityEngine.UI.ColorBlock btnCol = button.colors;
                btnCol.normalColor = value;
                btnCol.pressedColor = value;
                btnCol.highlightedColor = value;
                btnCol.disabledColor = value;
                btnCol.selectedColor = value;
                button.colors = btnCol;
            }
        }

        /// <summary> Set the button text colour </summary>
        public Color ButtonTextColor
        {
            get
            {
                return btnText != null ? btnText.color : Color.black;
            }
            set
            {
                if (btnText != null) { btnText.color = value; }
            }
        }

        /// <summary> Set button color and text colour at the same time </summary>
        /// <param name="btnColor"></param>
        /// <param name="txtColor"></param>
        public void SetColors(Color btnColor, Color txtColor)
        {
            this.ButtonColor = btnColor;
            this.ButtonTextColor = txtColor;
        }


        /// <summary> Event handler for when the button is pressed. </summary>
        private void OnButtonPressed()
        {
            this.ButtonPressed.Invoke(this);
        }



        private void OnEnable()
        {
            if (button == null)
            {
                button = this.gameObject.GetComponent<UnityEngine.UI.Button>();
            }
            if (btnText == null && button != null)
            {
                btnText = button.GetComponentInChildren<TMPro.TMP_Text>();
            }

            if (button != null)
            {
                button.onClick.AddListener(OnButtonPressed);
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonPressed);
            }
        }

    }
}