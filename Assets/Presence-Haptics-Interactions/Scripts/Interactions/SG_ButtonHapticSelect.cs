using SG;
using SG.Util;
using UnityEditor;
using UnityEngine;

namespace Presence
{

#if UNITY_EDITOR
    [CustomEditor(typeof(SG_ButtonHapticSelect)), CanEditMultipleObjects]
    public class SG_ButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SG_ButtonHapticSelect button = target as SG_ButtonHapticSelect;

            EditorGUILayout.LabelField("Will the button light up if pressed", EditorStyles.boldLabel);
            button.lightsUp = EditorGUILayout.Toggle("Light up on press:", button.lightsUp);

            if (button.lightsUp)
            {
                button.off = (Material)EditorGUILayout.ObjectField("Button Material when off:", button.off, typeof(Material), true);
                button.on = (Material)EditorGUILayout.ObjectField("Button Material when on:", button.on, typeof(Material), true);
            }
        }
    }
#endif

    // event that fires a boolean if triggered
    [System.Serializable]
    public class SG_ButtonHapticSelectPressedEvent : UnityEngine.Events.UnityEvent<bool> { }

    public class SG_ButtonHapticSelect : MonoBehaviour
    {
        [Header("Top part of the button that pushes in")]
        public GameObject button;

        [Header("Distance to activate button")]
        public float maxButtonMovement = 0.003F;

        [Header("Angle of the button movement")]
        public MoveAxis moveAxis = MoveAxis.X;

        [Header("Haptic effect to play when pressed")]
        public PHap_HapticEffect hapticEffect;

        [Header("Which location to play the haptic effect if pressing the button")]
        public HandPart whichPart = HandPart.Index;

        [Header("Event that triggers when the button is pressed")]
        public SG_ButtonHapticSelectPressedEvent buttonPressed = new SG_ButtonHapticSelectPressedEvent();

        [Header("The speed the button returns to original state")]
        public float buttonReturnSpeed = 20f;

        [Header("When does the button gets reactivated so you can activate again")]
        public float timerSize = 0.5f;

        [Header("The collider the button might collide with and ignores that collision")]
        public GameObject table;

        // public vars in the editor custom script 
        [HideInInspector]
        public bool lightsUp = true;
        [HideInInspector]
        public Material off;
        [HideInInspector]
        public Material on;

        // vars that are public available but not usefull for the editor
        [HideInInspector]
        public SG_TrackedHand interactingGlove;
        [HideInInspector]
        public Vector3 localStartPos;

        // private vars
        private float timer = 5F;
        private bool switched = false;
        private bool isOn = false;

        private void Awake()
        {
            localStartPos = button.transform.localPosition;

            // ignore the collider that the button is placed on
            if (table != null && table.GetComponent<Collider>() != null)
                Physics.IgnoreCollision(table.GetComponent<Collider>(), button.GetComponent<Collider>());
        }

        void Update()
        {
            // timer to make sure the button doesn't fire multiple time in quick succession
            if (timer < timerSize)
            {
                timer = timer + Time.deltaTime;
            }
            else
            {
                switched = false;
            }

            // if the max range is reached then fire of the event and a haptic command
            if (!switched)
            {
                switch (moveAxis)
                {
                    case MoveAxis.X:
                        if (button.transform.localPosition.x >= (localStartPos.x + maxButtonMovement))
                            ButtonActivated();
                        break;
                    case MoveAxis.NegativeX:
                        if (button.transform.localPosition.x <= (localStartPos.x - maxButtonMovement))
                            ButtonActivated();
                        break;
                    case MoveAxis.Y:
                        if (button.transform.localPosition.y >= (localStartPos.y + maxButtonMovement))
                            ButtonActivated();
                        break;
                    case MoveAxis.NegativeY:
                        if (button.transform.localPosition.y <= (localStartPos.y - maxButtonMovement))
                            ButtonActivated();
                        break;
                    case MoveAxis.Z:
                        if (button.transform.localPosition.z >= (localStartPos.z + maxButtonMovement))
                            ButtonActivated();
                        break;
                    case MoveAxis.NegativeZ:
                        if (button.transform.localPosition.z <= (localStartPos.z - maxButtonMovement))
                            ButtonActivated();
                        break;
                }
            }

            // update the button position to the start position
            UpdateButtonPosition();
        }

        private void ButtonActivated()
        {
            SendHapticCommand();
            buttonPressed.Invoke(isOn);

            if (lightsUp)
                ChangeColour(isOn);

            isOn = !isOn;
            timer = 0;
            switched = true;
        }

        // create a haptic effect on the glove when you pressed the button deep enough to trigger the event
        private void SendHapticCommand()
        {
            PHap_EffectLocation location = new PHap_EffectLocation(Presence.PHap_BodyPart.RightIndexFinger);

            if (interactingGlove != null)
            {
                if (interactingGlove.TracksRightHand())
                {
                    switch (whichPart)
                    {
                        case (HandPart.Index):
                            location = new PHap_EffectLocation(Presence.PHap_BodyPart.RightIndexFinger);
                            break;
                        case (HandPart.Thumb):
                            location = new PHap_EffectLocation(Presence.PHap_BodyPart.RightThumb);
                            break;
                        case (HandPart.Palm):
                            location = new PHap_EffectLocation(Presence.PHap_BodyPart.RightHandPalm);
                            break;
                    }  
                }
                else
                {
                    switch (whichPart)
                    {
                        case (HandPart.Index):
                            location = new PHap_EffectLocation(Presence.PHap_BodyPart.LeftIndexFinger);
                            break;
                        case (HandPart.Thumb):
                            location = new PHap_EffectLocation(Presence.PHap_BodyPart.LeftThumb);
                            break;
                        case (HandPart.Palm):
                            location = new PHap_EffectLocation(Presence.PHap_BodyPart.LeftHandPalm);
                            break;
                    }
                }

                PHap_Core.PlayHapticEffect(hapticEffect, location);
            }
                
        }

        // Change the colour of the button or light it up a bit
        private void ChangeColour(bool isOn)
        {
            if (button.GetComponent<Renderer>().material != null)
                button.GetComponent<Renderer>().material = isOn ? off : on;
        }

        // detect which glove is interacting with the button to give the haptic effect to the right glove.
        public void HandDetected(SG_TrackedHand glove)
        {
            interactingGlove = glove;
        }

        // Update the button position to move slowly back to its starting position
        private void UpdateButtonPosition()
        {
            switch (moveAxis)
            {
                case MoveAxis.X:
                    button.transform.localPosition = button.transform.localPosition.x > (localStartPos.x + maxButtonMovement) ?
                        new Vector3(localStartPos.x + maxButtonMovement, localStartPos.y, localStartPos.z) :
                        new Vector3(button.transform.localPosition.x, localStartPos.y, localStartPos.z);
                    break;
                case MoveAxis.NegativeX:
                    button.transform.localPosition = button.transform.localPosition.x < (localStartPos.x - maxButtonMovement) ?
                        new Vector3(localStartPos.x - maxButtonMovement, localStartPos.y, localStartPos.z) :
                        new Vector3(button.transform.localPosition.x, localStartPos.y, localStartPos.z);
                    break;
                case MoveAxis.Y:
                    button.transform.localPosition = button.transform.localPosition.y > (localStartPos.y + maxButtonMovement) ?
                        new Vector3(localStartPos.x, localStartPos.y + maxButtonMovement, localStartPos.z) :
                        new Vector3(localStartPos.x, button.transform.localPosition.y, localStartPos.z);
                    break;
                case MoveAxis.NegativeY:
                    button.transform.localPosition = button.transform.localPosition.y < (localStartPos.y - maxButtonMovement) ?
                        new Vector3(localStartPos.x, localStartPos.y - maxButtonMovement, localStartPos.z) :
                        new Vector3(localStartPos.x, button.transform.localPosition.y, localStartPos.z);
                    break;
                case MoveAxis.Z:
                    button.transform.localPosition = button.transform.localPosition.z > (localStartPos.z + maxButtonMovement) ?
                        new Vector3(localStartPos.x, localStartPos.y, localStartPos.z + maxButtonMovement) :
                        new Vector3(localStartPos.x, localStartPos.y, button.transform.localPosition.z);
                    break;
                case MoveAxis.NegativeZ:
                    button.transform.localPosition = button.transform.localPosition.z < (localStartPos.z - maxButtonMovement) ?
                        new Vector3(localStartPos.x, localStartPos.y, localStartPos.z - maxButtonMovement) :
                        new Vector3(localStartPos.x, localStartPos.y, button.transform.localPosition.z);
                    break;
            }

            // Return button to startPosition
            button.transform.localPosition = Vector3.Lerp(button.transform.localPosition, localStartPos, Time.deltaTime * buttonReturnSpeed);
        }
    }
}
