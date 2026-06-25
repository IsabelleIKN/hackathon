/*
 * The 'Core' API through which one interfaces with the WP3 Haptics Implementation in the Presence side.
 * This is where classes are defined and where Haptic functions are called.
 * 
 * authors:
 * max@senseglove.com
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Presence
{
    
    /// <summary> Where on the body the effect should take place </summary>
    /// <summary> Prepresents a (global) location of a body part. TODO: Synchronize these with OpenXR Body representation(s)? </summary>
    public enum PHap_BodyPart
    {
        Unknown,

        LeftHead,
        RightHead,
        Neck,

        Torso,

        LeftUpperArm,
        LeftLowerArm,
        LeftHandPalm,

        LeftThumb,
        LeftIndexFinger,
        LeftMiddleFinger,
        LeftRingFinger,
        LeftPinky,


        RightUpperArm,
        RightLowerArm,
        RightHandPalm,

        RightThumb,
        RightIndexFinger,
        RightMiddleFinger,
        RightRingFinger,
        RightPinky,

        RightChest,
        RightWaist,

        LeftChest,
        LeftWaist,

        LeftUpperLeg,
        LeftLowerLeg,
        LeftFoot,

        RightUpperLeg,
        RightLowerLeg,
        RightFoot,

        All //Utility Value
    }

    /// <summary> Variation on PHap_BodyPart when a multi-selection is required. </summary>
    [System.Flags]
    public enum PHap_BodyPartSelection
    {
        None = 0,

        Head = 1 << 0,
        Neck = 1 << 1,

        Torso = 1 << 2,

        LeftUpperArm = 1 << 3,
        LeftLowerArm = 1 << 4,
        LeftHandPalm = 1 << 5,

        LeftThumb = 1 << 6,
        LeftIndexFinger = 1 << 7,
        LeftMiddleFinger = 1 << 8,
        LeftRingFinger = 1 << 9,
        LeftPinky = 1 << 10,


        RightUpperArm = 1 << 11,
        RightLowerArm = 1 << 12,
        RightHandPalm = 1 << 13,

        RightThumb = 1 << 14,
        RightIndexFinger = 1 << 15,
        RightMiddleFinger = 1 << 16,
        RightRingFinger = 1 << 17,
        RightPinky = 1 << 18,

        LeftUpperLeg = 1 << 19,
        LeftLowerLeg = 1 << 20,
        LeftFoot = 1 << 21,

        RightUpperLeg = 1 << 22,
        RightLowerLeg = 1 << 23,
        RightFoot = 1 << 24,
    }

    /// <summary> The kind of Haptic Perception / Modality of an effect. E.g. Vibrotactile / Force Feedback etc. </summary>
    public enum PHap_HapticModality
    {
        Unknown,
        /// <summary> -1/1 per s </summary>
        Other,

        /// <summary> Pa per s </summary>
        Pressure,
        /// <summary> m/s^2 per s </summary>
        Accelleration,
        /// <summary> m/s per s </summary>
        Velocity,
        /// <summary> m per s </summary>
        Position,
        /// <summary> K per s </summary>
        Temperature,
        /// <summary> -1/1 per s </summary>
        Vibrotactile,
        /// <summary> m^3 per s </summary>
        Water,
        /// <summary> m per s </summary>
        Wind,
        /// <summary> N per s </summary>
        Force,
        /// <summary> -1/1 per s </summary>
        Electrotactile,
        /// <summary> -1/1 per m ummary>
        VibrotactileTexture,
        /// <summary> N per m </summary>
        Stiffness,
        /// <summary> -1/1 per m </summary>
        Friction,

        All //utility value.
    }

    /// <summary> Describes where a Haptic Effect should take place. </summary>
    public class PHap_EffectLocation
    {
        /// <summary> The body part to play the effect on. </summary>
        public PHap_BodyPart BodyPart { get; set; }


        /// <summary> The position of the effect relative to the body part's origin. </summary>
        public Vector3 LocalPosition { get; set; }

        /// <summary> Bounding Box of the space for the effect </summary>
        public Vector3 BoundingBoxCenter { get; set; }


        /// <summary> bounding Box size of the space for the effect </summary>
        public Vector3 BoundingBoxSize { get; set; }


        /// <summary> The size of the effect - assuming it is spherical </summary>
        public float EffectSize { get; set; }


        /// <summary> Creates a simple effect centered on a BodyPart. Effect locaton will be at the origin of the BodyPart. </summary>
        /// <param name="bodyPart"></param>
        public PHap_EffectLocation(PHap_BodyPart bodyPart)
        {
            BodyPart = bodyPart;
            EffectSize = 1.0f; //TODO: Find a good default value / Unit.
        }

        /// <summary>
        /// Plays effect at the bodypart at a specific position, with a certain radius around it (scale).
        /// </summary>
        /// <param name="bodyPart">Overall bodypart</param>
        /// <param name="localPos">Location of the effect on this bodypart represented a vector between the Origin of this receiver to the contact point in the local space of the origin. </param>
        /// <param name="size">Radius in meters of the effect.</param>
        public PHap_EffectLocation(PHap_BodyPart bodyPart, Vector3 localPos, float size)
        {
            BodyPart = bodyPart;
            LocalPosition = localPos;
            EffectSize = size;
            //if not Bounding Box is passed, we assume bb is at the center and localPos is at the furthest corner.
            BoundingBoxCenter = Vector3.zero;
            BoundingBoxSize = localPos * 2.0f; // the application point will always be on the edge of the shape. 
        }

        public PHap_EffectLocation(PHap_BodyPart bodyPart, Vector3 localPos, float size, Vector3 bbCenter, Vector3 bbSize)
        {
            BodyPart = bodyPart;
            LocalPosition = localPos;
            EffectSize = size;
            BoundingBoxCenter = bbCenter;
            BoundingBoxSize = bbSize;
        }

        /// <summary> ToString </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return BodyPart.ToString() + " - " + LocalPosition.ToString() + ", " + EffectSize.ToString("0.00");
        }

        public Vector3 GetNormalizedPosition()
        {
            return new Vector3(
                LocalPosition.x / (BoundingBoxSize.x * 0.5f),
                LocalPosition.y / (BoundingBoxSize.y * 0.5f),
                LocalPosition.z / (BoundingBoxSize.z * 0.5f)
            );
        }
    }






    /// <summary> Presence Haptics Core API. </summary>
    public sealed class PHap_Core : MonoBehaviour
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // Core API Functions

        /// <summary> Returns true if at least one device supports the chosen modality on a particular bodypart. </summary>
        /// <param name="effectType"></param>
        /// <param name="bodyPart"></param>
        /// <returns></returns>
        public static bool SupportsHaptics(PHap_HapticModality effectType, PHap_BodyPart bodyPart)
        {
            foreach (PHap_DeviceImplementation impl in Implementations)
            {
                if (impl.SupportsHaptics(effectType, bodyPart))
                    return true;
            }
            return false;
        }


        /// <summary> Returns true if the Haptic effect type is based on timing (and can therefore be looped) or if it's based on other parameters (such as distance) such that timing does not matter. </summary>
        /// <returns></returns>
        public static bool IsTimedEffect(PHap_HapticModality effectType)
        {
            switch (effectType)
            {
                case PHap_HapticModality.VibrotactileTexture:
                case PHap_HapticModality.Stiffness:
                case PHap_HapticModality.Friction:
                    return false;
                default:
                    return true;
            }
        }


        /// <summary> Loads a Haptic Effect into memory. Specifically, it loads the BaseEffect from 'Beneath.' </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public static bool LoadEffect(PHap_HapticEffect effect)
        {
            if (effect.BaseEffect == null)
            {
                Debug.LogWarning(effect.name + " has no associated BaseEffect and can therefore not be Loaded!", effect);
                return false;
            }
            if ( !effect.BaseEffect.IsValid() )
            {
                Debug.LogWarning(effect.name + "'s BaseEffect is not valid (missing formats, or is otherwise broken), and it can therefore not be loaded or played!", effect);
                return false;
            }

            bool loaded = false;
            foreach (PHap_DeviceImplementation impl in Implementations)
            {
                loaded = impl.LoadEffect(effect) || loaded; //function should be to the left, since if the left side boolean is true, the other side is not evaluated.
            }
            if (!loaded)
            {
                Debug.LogWarning("There was no implementation that could load the PHap_HapticEffect " + effect.name + ". ", effect);
            }
            return loaded;
        }

        public static bool UnloadEffect(PHap_HapticEffect effect)
        {
            bool unloaded = false;
            foreach (PHap_DeviceImplementation impl in Implementations)
            {
                unloaded = impl.UnloadEffect(effect) || unloaded; //function should be to the left, since if the left side boolean is true, the other side is not evaluated.
            }
            return unloaded; //no wanring needed for unloading, I don't think.
        }





        /// <summary>  </summary>
        /// <param name="effect"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static bool PlayHapticEffect(PHap_HapticEffect effect, PHap_EffectLocation location)
        {
           
            if (effect.BaseEffect == null)
                return false;

            if ( !PlayAllowed(location.BodyPart, effect.BaseEffect.bodyPartFilter, effect.BaseEffect.bodyPartSelection) )
                return false;

            if (effect.BaseEffect.name.EndsWith("_recoded"))
                Debug.Log("Playing effect: " + effect.BaseEffect?.name);

            bool played = false;
            foreach (PHap_DeviceImplementation impl in Implementations)
            {
                if (impl.PlayHapticEffect(effect, location)) //TODO: Determine how to handle multiple devices that can play this effect (eg Nova Glove attached to Quest Controller). Perhaps a priority system? Currently, it's just based on the other of implementations.
                    return true;
            }
            //TODO: Callback if there was no implementation that could play the effect
            return played;
        }


        /// <summary> Stops a specific active effect.  </summary>
        /// <param name="effect"></param>
        /// <param name="targetBodyPart"></param>
        /// <returns></returns>
        public static bool StopHapticEffect(PHap_HapticEffect effect, PHap_EffectLocation location)
        {
            bool stopped = false;
            foreach (PHap_DeviceImplementation impl in Implementations)
            {
                stopped = impl.StopHapticEffect(effect, location) || stopped;  //function should be to the left, since if the left side boolean is true, the other side is not evaluated.
            }
            return stopped;
        }


        public static bool PlayAllowed(PHap_BodyPart bodyPart, PHap_BodyPartFilter filter, PHap_BodyPartSelection list)
        {
            //convert bodyPart into a list option.
            if (filter == PHap_BodyPartFilter.NoFilter)
                return true;

            int bitIndex = ((int)bodyPart) - 1; //this converts the body part into the bit
            if (((int)list & (1 << bitIndex)) != 0)
            {
                //Debug.Log($"Flag at index {bitIndex} is enabled");
                //If we get here, the bodyPart is on our list, now we return based on the filter
                return filter == PHap_BodyPartFilter.AllowedBodyParts; //returns true if the filter is set to allowed, false if it isn't.
            }
            //if we get here, it's not on the list, so only true if we exclude on this list
            return filter == PHap_BodyPartFilter.ExcludingBodyParts;
        }

        

        //-------------------------------------------------------------------------------------------------------------------------
        // Singleton Pattern but via a GameObject + Component + DontDestroyOnLoad - Allows UnityEvents.

        /// <summary> Instance that is created when a function is called. Internal functions should call Instance as opposed to using s_instance directly! </summary>
        private static PHap_Core s_instance = null;

        /// <summary> Gets the Instance of PHap_Core. Creates one if it has yet to exist. </summary>
        public static PHap_Core Instance
        {
            get
            {
                TryInitialize();
                return s_instance;
            }
        }


        // Variables


        /// <summary> Call this method to ensure the creation of a PHap_Core instance and initialize the API's. Will be automatically called when you grab the Instance. </summary>
        public static void TryInitialize()
        {
            if (s_instance != null) //after the first function call, s_instance should never be NULL again (unless someone deleted the Instance).
                return;

            s_instance = GameObject.FindObjectOfType<PHap_Core>(); //first try and grab if from the scene. If it had existed, it would have called SetupInstance() already.
            if (s_instance == null) //Still NULL which means we could not find it.
            {
                Debug.Log("Creating a new instance of PHap_Core.");
                GameObject coreObj = new GameObject("PHap_Core");
                s_instance = coreObj.AddComponent<PHap_Core>(); // Calls SetupInstance();
            }
        }


        /// <summary> Called by instances of this class on Awake() to either register themselves as the active instance, or to delete themselves with a log(?) </summary>
        private void SetupInstance()
        {
            if (s_instance == null)
            {
                s_instance = this;
                DontDestroyOnLoad(s_instance);
                //TODO: Load & Initialize Implementations
                PHap_Settings sett = PHap_Settings.GetSettings();
                foreach (PHap_DeviceImplementation impl in sett.implementations)
                {
                    impl.Initialize();
                }
            }
            else if (s_instance != this)
            {
                Debug.Log("PHap_Core Instance already exists. So we're deleting this instance");
                GameObject.Destroy(this);
            }
        }

        /// <summary>  Called by instances of this class on Awake() to either de-register themselves as the active instance. </summary>
        private void DisposeInstance()
        {
            if (s_instance == this)
            {
                //De Initialize
                PHap_Settings sett = PHap_Settings.GetSettings();
                foreach (PHap_DeviceImplementation impl in sett.implementations)
                {
                    impl.Deinitialize();
                }
                s_instance = null; //explicitly so we don't rely on GC to clear s_instance whenever.
            }
        }


        /// <summary> Retirves the currently linked implementations  </summary>
        public static List<PHap_DeviceImplementation> Implementations
        {
            get
            {
                if (Application.isPlaying)
                {
                    TryInitialize();
                }
                return PHap_Settings.GetSettings().implementations;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // Monobehaviour

        private void Awake()
        {
            SetupInstance();
        }

        private void OnDestroy() //Won't be called during scene changes, since it's DontDestoryOnLoad. But someone might explictly call Destroy.
        {
            DisposeInstance();
        }



    }

}