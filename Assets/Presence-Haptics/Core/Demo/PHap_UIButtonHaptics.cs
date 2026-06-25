using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * A UI Button that fires an event when pressed, generating parameters for a PHap_EffectLocation
 * 
 * @author
 * max@senseglove.com
 */

namespace Presence
{

    /// <summary> Attached to a button that, when clicked, fires an event that another script can use to determine what haptics to play... </summary>
    public class PHap_UIButtonHaptics : MonoBehaviour, IPointerClickHandler
    {
        /// <summary> First vector2 is the local position, second is the same but normalized between -1.0 ... 1.0 </summary>
        [System.Serializable] public class PHap_UIButtonEvent : UnityEngine.Events.UnityEvent<PHap_UIButtonHaptics, Vector2, Vector2> { }

        /// <summary> UnityEngine UI Button to be clicked </summary>
        public UnityEngine.UI.Button hapticButton;

        /// <summary> Haptic Effect Location </summary>
        public PHap_BodyPart hapticLocation = PHap_BodyPart.Unknown;

        /// <summary> Fires when this button is clicked, passes additional parameters. </summary>
        public PHap_UIButtonEvent OnButtonClicked = new PHap_UIButtonEvent();

        /// Rect to retreive the 2D clic position
        private RectTransform clickRect;


        /// <summary> Whether or nog the button can be interacted with. e.g. If we don't support the body part, we turn it off. </summary>
        public bool BtnInteractable
        {
            get { return hapticButton != null ? hapticButton.interactable : false; }
            set { if (hapticButton != null) { hapticButton.interactable = value; } }
        }



        /// <summary> Event handler for Clicking on this button </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                clickRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

            //Normalize this to 01...1..Though it's probably not well suited for torso feedback yet
            //TODO: Normalize from a different point / origin? 
            //float normalizedX = (2.0f * localPoint.x) / bodyPartZero.rect.width; //x2. because w is normalized to -1 ... 1.
            //float normalizedY = (2.0f * localPoint.y) / bodyPartZero.rect.height;
            //  -> it is better to simply pass the raw data here and handle that in the later call

            // Log or use the localPoint (x, y position relative to button's center)
            Debug.Log($"Click position relative to button: {localPoint}, {clickRect.rect.size}");
            OnButtonClicked.Invoke(this, localPoint, clickRect.rect.size);
        }



        private void OnEnable()
        {
            if (hapticButton == null)
                hapticButton = this.GetComponent<UnityEngine.UI.Button>();
            if (hapticButton != null)
                this.clickRect = hapticButton.GetComponent<RectTransform>();
        }

        private void Reset()
        {
            if (hapticButton == null)
                hapticButton = this.GetComponent<UnityEngine.UI.Button>();
        }
    }
}