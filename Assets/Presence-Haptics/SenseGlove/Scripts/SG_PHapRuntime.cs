using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Since the SenseGlove Implementation relies on calling an "Update" function to do things like "updating force over time / distance", this helper class is Generated. 
 * 
 * author:
 * max@senseglove.com
 */

namespace Presence
{

    /// <summary> This instance calls runtime implement </summary>
    public class SG_PHapRuntime : MonoBehaviour
    {

        public UnityEngine.Events.UnityEvent UpdateHapticsEvent = new UnityEngine.Events.UnityEvent();

        //-------------------------------------------------------------------------------------------------------------------------
        // Singleton Pattern but via a GameObject + Component + DontDestroyOnLoad - Allows UnityEvents.

        /// <summary> Instance that is created when a function is called. Internal functions should call Instance as opposed to using s_instance directly! </summary>
        private static SG_PHapRuntime s_instance = null;

        /// <summary> Gets the Instance of PHap_Core. Creates one if it has yet to exist. </summary>
        public static SG_PHapRuntime Instance
        {
            get
            {
                TryInitialize();
                return s_instance;
            }
        }



        /// <summary> Call this method to ensure the creation of a PHap_Core instance and initialize the API's. Will be automatically called when you grab the Instance. </summary>
        public static void TryInitialize()
        {
            if (s_instance != null) //after the first function call, s_instance should never be NULL again (unless someone deleted the Instance).
                return;

            s_instance = GameObject.FindObjectOfType<SG_PHapRuntime>(); //first try and grab if from the scene. If it had existed, it would have called SetupInstance() already.
            if (s_instance == null) //Still NULL which means we could not find it.
            {
                Debug.Log("Creating a new instance of SG_PHapRuntime.");
                GameObject coreObj = new GameObject("SG_PHap Runtime");
                s_instance = coreObj.AddComponent<SG_PHapRuntime>(); // Calls SetupInstance();
            }
        }


        /// <summary> Called by instances of this class on Awake() to either register themselves as the active instance, or to delete themselves with a log(?) </summary>
        private void SetupInstance()
        {
            if (s_instance == null)
            {
                s_instance = this;
                DontDestroyOnLoad(s_instance);
            }
            else if (s_instance != this)
            {
                Debug.Log("SG_PHapRuntime Instance already exists. So we're deleting this instance");
                GameObject.Destroy(this);
            }
        }

        /// <summary>  Called by instances of this class on Awake() to either de-register themselves as the active instance. </summary>
        private void DisposeInstance()
        {
            if (s_instance == this)
            {
                //De Initialize
                s_instance = null; //explicitly so we don't rely on GC to clear s_instance whenever.
            }
        }


        //-------------------------------------------------------------------------------------------------------------------------
        // Playing Forces / timed haptics mixing...


        private SG_TimedEffectTracker<SG_PremadeForce>[] leftHandForces = new SG_TimedEffectTracker<SG_PremadeForce>[5];
        private SG_TimedEffectTracker<SG_PremadeForce>[] rightHandForces = new SG_TimedEffectTracker<SG_PremadeForce>[5];

        private SG_TimedEffectTracker<SG_PremadeStiffness>[] leftHandStiffness = new SG_TimedEffectTracker<SG_PremadeStiffness>[5]; //Stiffness does not need timing...
        private SG_TimedEffectTracker<SG_PremadeStiffness>[] rightHandStiffness = new SG_TimedEffectTracker<SG_PremadeStiffness>[5];


        private SG_TimedEffectTracker<SG_PremadeForce> leftPalmForce = null;
        private SG_TimedEffectTracker<SG_PremadeForce> rightPalmForce = null;




        /// <summary> Returns the current time, used to keep track of effect timing. Placed inside its own function to guaratee it's consistent. </summary>
        /// <returns></returns>
        public static float GetCurrentTime()
        {
            return Time.time;
        }

        public void PlayForceEffectOnFinger(SG_PremadeForce effect, int finger, bool rightHand, float intensity, bool looping, int repeatAmount)
        {
            SG_TimedEffectTracker<SG_PremadeForce>[] forces = rightHand ? rightHandForces : leftHandForces;
            forces[finger] = new SG_TimedEffectTracker<SG_PremadeForce>(effect, GetCurrentTime(), intensity); //minus deltaTime because I will be adding 
            //This should then be evaluated in LateUpdate
            forces[finger].Looping = looping;
            forces[finger].RepeatAmount = repeatAmount;
        }

        public void StopForceEffectOnFinger(int finger, bool rightHand)
        {
            SG_TimedEffectTracker<SG_PremadeForce>[] forces = rightHand ? rightHandForces : leftHandForces;
            if (forces[finger] != null)
            {
                forces[finger] = null; //it will stop being checked for, we already set it to 0. If during the LateUpdate there is a ForceEffect at play, we'll use that.
                SGCore.HandLayer.QueueCmd_FFBLevel(rightHand, finger, 0.0f, false);
            }
        }


        /// <summary>
        /// Stiffness on Wrist is not allowed
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="location"></param>
        public void PlayStiffnessEffectOnFinger(SG_PremadeStiffness effect, int finger, bool rightHand, float intensity)
        {
            Debug.Log("Play Stiffness on finger " + finger + " of " + (rightHand ? "right hand" : "left hand"));
            SG_TimedEffectTracker<SG_PremadeStiffness>[] stiffnesses = rightHand ? rightHandStiffness : leftHandStiffness;
            stiffnesses[finger] = new SG_TimedEffectTracker<SG_PremadeStiffness>(effect, GetCurrentTime(), intensity);
            //This should then be evaluated in LateUpdate
        }


        public void StopStiffnessEffectOnFinger(int finger, bool rightHand)
        {
            Debug.Log("Stop Stiffness on finger " + finger + " of " + (rightHand ? "right hand" : "left hand"));
            SG_TimedEffectTracker<SG_PremadeStiffness>[] stiffnesses = rightHand ? rightHandStiffness : leftHandStiffness;
            if (stiffnesses[finger] != null)
            {
                stiffnesses[finger] = null; //it will stop being checked for, we already set it to 0. If during the LateUpdate there is a ForceEffect at play, we'll use that.
                SGCore.HandLayer.QueueCmd_FFBLevel(rightHand, finger, 0.0f, false);
            }
        }




        /// <summary> retruns true if there is a  change and I need to affect the actual finger. </summary>
        /// <param name="timedForce"></param>
        /// <param name="stiffness"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private void UpdateFingerLevel(bool rightHand)
        {
            bool assessFlexions = SG.SG_HandTracking.GetNormalizedFlexions(rightHand, out float[] flexions);
            float elapsedTime;
            SG_TimedEffectTracker<SG_PremadeForce>[] timedForces = rightHand ? rightHandForces : leftHandForces;
            SG_TimedEffectTracker<SG_PremadeStiffness>[] stiffnesses = rightHand ? rightHandStiffness : leftHandStiffness;

            for (int f=0; f<5; f++)
            {
                if (timedForces[f] != null) //a timed force at play!
                {
                    elapsedTime = GetCurrentTime() - timedForces[f].StartTime;
                    if (elapsedTime >= timedForces[f].SGEffect.effectDuration)
                    {
                        timedForces[f].Repeats++;
                        timedForces[f].StartTime = GetCurrentTime(); //reset this for evaluation purposes in the next frame.
                        elapsedTime = 0.0f; //this one too
                    }

                    if (timedForces[f].Repeats < timedForces[f].RepeatAmount || timedForces[f].Looping) //if we still need to go...
                    {
                        float level = timedForces[f].SGEffect.GetCurrentForceLevel(elapsedTime, timedForces[f].ScaleFactor);
                        SGCore.HandLayer.QueueCmd_FFBLevel(rightHand, f, level, false);
                        //Debug.Log("Force " + elapsedTime.ToString() + " -> " + level.ToString("0.00"));
                        continue; //we don't check the Stiffness.
                    }
                    else
                    {
                        SGCore.HandLayer.QueueCmd_FFBLevel(rightHand, f, 0.0f, false); //turn off this finger and clear effect
                        timedForces[f] = null;
                    }
                }
                if (stiffnesses[f] != null)
                {
                    
                    float level = assessFlexions ? stiffnesses[f].SGEffect.GetCurrentForceLevel(flexions[f], stiffnesses[f].ScaleFactor) : stiffnesses[f].LastLevel;
                    stiffnesses[f].LastLevel = level; //update the last level.
                    //Debug.Log("Stiffness " + flexions[f].ToString("0.00") + " -> " + level.ToString("0.00"));
                    SGCore.HandLayer.QueueCmd_FFBLevel(rightHand, f, level, false);
                }
            }


        }



        public void PlayForceEffectOnPalm(SG_PremadeForce forceEffect, bool rightHand, float scaleFactor, bool looping, int repeatAmount)
        {
            SG_TimedEffectTracker<SG_PremadeForce> effect = new SG_TimedEffectTracker<SG_PremadeForce>(forceEffect, GetCurrentTime(), scaleFactor);
            effect.RepeatAmount = repeatAmount;
            effect.Looping = looping;
            if (rightHand)
                rightPalmForce = effect;
            else
                leftPalmForce = effect;
        }

        public void StopForceEffectOnPalm(bool rightHand)
        {
            if (rightHand)
            {
                rightPalmForce = null; //clears the effect.
                SGCore.HandLayer.QueueCmd_WristSqueeze(rightHand, 0.0f, false);
            }
            else
            {
                leftPalmForce = null;
                SGCore.HandLayer.QueueCmd_WristSqueeze(rightHand, 0.0f, false);
            }
        }


        public void UpdatePalmFeedback(bool rightHand)
        {
            //always update this one.
            SG_TimedEffectTracker<SG_PremadeForce> effect = rightHand ? rightPalmForce : leftPalmForce;
            if (effect == null)
            {
                SGCore.HandLayer.QueueCmd_WristSqueeze(rightHand, 0.0f, false);
            }
            else
            {
                float elapsedTime = GetCurrentTime() - effect.StartTime;
                if (elapsedTime >= effect.SGEffect.effectDuration)
                {
                    effect.Repeats++;
                    effect.StartTime = GetCurrentTime(); //reset this for evaluation purposes in case we're repeatin (e.g. effect of 1s at 
                    elapsedTime = 0.0f;
                }
                if (effect.Repeats < effect.RepeatAmount || effect.Looping) //if we still need to go...
                {
                    float lvl = effect.SGEffect.GetCurrentForceLevel(elapsedTime, effect.ScaleFactor);
                    SGCore.HandLayer.QueueCmd_WristSqueeze(rightHand, lvl, false);
                }
                else
                {
                    StopForceEffectOnPalm(rightHand); //clears effect, queues a 0.0f.
                }
            }
        }


        public void UpdateInternalHaptics()
        {
            //Update and Queue Fingers
            UpdateFingerLevel(true);
            UpdateFingerLevel(false);

            //Update and queue Active Strap
            UpdatePalmFeedback(true);
            UpdatePalmFeedback(false);

            //"Flush" haptics for the lest hand right hand.
            SGCore.HandLayer.SendHaptics(true);
            SGCore.HandLayer.SendHaptics(false);
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

        private void LateUpdate()
        {
            UpdateHapticsEvent.Invoke();
            UpdateInternalHaptics();
        }


        //-------------------------------------------------------------------------------------------------------------------------
        // Helper Script


        public class SG_TimedEffectTracker<T>
        {
            public float StartTime { get; set; }

            public float ScaleFactor { get; set; }
            
            public float LastLevel { get; set; }

            public bool Looping { get; set; }

            public int RepeatAmount { get; set; }

            public int Repeats { get; set; }

            public T SGEffect { get; set; }

            public SG_TimedEffectTracker(T eff, float currTime, float scale)
            {
                SGEffect = eff;
                StartTime = currTime;
                ScaleFactor = scale;
                LastLevel = 0.0f;
                RepeatAmount = 1;
                Looping = false;
                Repeats = 0;
            }
        }




    }
}