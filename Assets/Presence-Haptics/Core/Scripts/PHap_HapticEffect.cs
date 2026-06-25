/*
 * Represents a Haptic Effect to be played inside your simulation.
 * A combination of base effect(s) and parameters that modify the base effect.
 * 
 * authors:
 * max@senseglove.com
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Presence
{
    /// <summary> A Component that exists with the scene, to play a base effect with various modifiers. </summary>
    /// <remarks> It's currently a Monobehaviour so it's easier to keep track of the effect. 
    /// It's possible to refactor this into a simple class with constructor so it can be called from anywhere instead,
    /// though this would require a monobehaviour wrapper to easily modify elements via the Inspector. </remarks>
    public class PHap_HapticEffect : MonoBehaviour
    {
        //--------------------------------------------------------------------------------------------------
        // Member variables

        /// <summary> The Base effect that this HapticEffect modifies in terms of volume / playback. </summary>
        [SerializeField] protected PHap_BaseEffect baseEffect;

        /// <summary> The maximum intensity this effect reaches. </summary>
        public const float maxIntensity = 5.0f;

        /// <summary> Amplitude of the base effect to play </summary>
        [SerializeField] [Range(0.0f, maxIntensity)] protected float intensity = 1.0f;

        /// <summary> The amount of times the effect will be repeated. Ignored if 'looping' is selected. </summary>
        [SerializeField] protected int repeatAmount = 1;

        /// <summary> If true, this effect will be called until someone stops it. </summary>
        [SerializeField] protected bool looping = false;

        /// <summary> Global volume multiplier for this effect (0.0 = silent, 1.0 = full volume) </summary>
        [SerializeField][Range(0.0f, 1.0f)] protected float volume = 1.0f; 
        
        
        public float Volume
        {
            get { return volume; }
            set { volume = Mathf.Clamp01(value); }
        }


        ////private int repeats = 0;
        //private Coroutine repeatRoutine = null;

        /// <summary> The total duration of this Haptic Effect - not taking into account looping </summary>
        public float TotalDuration
        {
            get { return BaseDuration * this.repeatAmount; }
        }


        /// <summary> The duration in seconds of the base effect linked to this script. </summary>
        public float BaseDuration
        {
            get { return baseEffect != null ? baseEffect.GetDuration() : 0.0f; }
        }

        public PHap_HapticModality EffectType
        {
            get { return baseEffect != null ? baseEffect.GetEffectType() : PHap_HapticModality.Unknown; }
        }

        public bool IsTimedEffect
        {
            get { return PHap_Core.IsTimedEffect(EffectType); }
        }

        public bool PlaysInfinite
        {
            get { return IsLooping || !IsTimedEffect; }
        }

        //--------------------------------------------------------------------------------------------------
        // Accessors

        /// <summary> Access the base effect that makes up this vibration </summary>
        public PHap_BaseEffect BaseEffect
        {
            get { return baseEffect; }
            set 
            { 
                baseEffect = value;
                if (baseEffect != null)
                    PHap_Core.LoadEffect(this); //(re)load the Haptic effect in relevant implementation(s).
            }
        }

        /// <summary> Value between 0.0f and maxIntensity that determines the amplitude of the base effect. </summary>
        public float Intensity
        {
            get { return intensity; }
            set { intensity = Mathf.Clamp(value, 0.0f, maxIntensity); }
        }

        /// <summary> The effective intensity after applying volume multiplier. This is what should be used when playing effects. </summary>
        public float EffectiveIntensity
        {
            get { return intensity * volume; }
        }

        public bool IsLooping
        {
            get { return this.looping; }
            set { this.looping = value; }
        }

        public int RepeatAmount
        {
            get { return this.repeatAmount; }
            set { this.repeatAmount = value; }
        }

        public override string ToString()
        {
            float effectiveIntensity = EffectiveIntensity;
            return "Effect [" + baseEffect.name + "]. Repeat " + (looping ? "until stopped" : repeatAmount.ToString() + " time(s)") + 
                   " at " + Mathf.RoundToInt(effectiveIntensity * 100 / maxIntensity).ToString() + "% effective intensity (base: " + 
                   Mathf.RoundToInt(Intensity * 100 / maxIntensity).ToString() + "%, volume: " + Mathf.RoundToInt(Volume * 100).ToString() + "%)";
        }




        //--------------------------------------------------------------------------------------------------
        // Effect Playing


        /// <summary> Plays this effect on a particular location </summary>
        /// <param name="location"></param>
        public void PlayEffect(PHap_EffectLocation location)
        {
            PHap_HapticModality myType = EffectType;
            if (myType == PHap_HapticModality.Unknown)
                return; //we cannot play unkown effects because that means it's not been loaded

            if ( !PHap_Core.IsTimedEffect(myType) ) //if it's a non-timing based effect (stiffness), we only play it once
            {
                PHap_Core.PlayHapticEffect(this, location);
                return;
            }

            float baseDuration = this.BaseDuration;
            if (baseDuration <= 0.0f)
                return; //it's a timing based effect but we could not read the effect duration :<

            //StopRoutine(); //if any is already playing.

            if (this.RepeatAmount < 2 && !this.IsLooping) //playing only once, basically
            {
                PHap_Core.PlayHapticEffect(this, location);
                return;
            }

            //StartCoroutine(RepeatEffectRoutine(baseDuration, location));
        }

        //private void StopRoutine()
        //{
        //    if (repeatRoutine != null)
        //    {
        //        StopCoroutine(repeatRoutine);
        //        repeatRoutine = null;
        //    }
        //}

        private IEnumerator RepeatEffectRoutine(float effectDuration, PHap_EffectLocation location)
        {
            uint repeats = 0; //uint in the rare case where someone repeast and effect a billion times.
            do
            {
                repeats++;
                PHap_Core.PlayHapticEffect(this, location);
                yield return new WaitForSeconds(effectDuration);
            }
            while (repeats < this.RepeatAmount || this.IsLooping);
        }

        /// <summary> Stops playing this effect. </summary>
        public void StopEffect(PHap_EffectLocation location)
        {
            //StopRoutine(); //stop me from sending it again
            PHap_Core.StopHapticEffect(this, location); //before notifying the Core.
        }



        private void Start()
        {
            PHap_Core.LoadEffect(this);
        }

        //private void OnDisable()
        //{
        //    StopRoutine(); //pretty sure it does already but I'm saving my sanity.
        //}

    }
}