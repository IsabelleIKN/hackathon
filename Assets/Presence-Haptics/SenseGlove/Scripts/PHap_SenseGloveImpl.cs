/*
 * SenseGlove implementation of the Presence Haptics API based on a singleton patter.
 * 
 * author:
 * max@senseglove.com
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Presence
{
    //[CreateAssetMenu(fileName = "PHap_SenseGlove", menuName = "Presence/Implementations/SenseGlove", order = 1)]
    public class PHap_SenseGloveImpl : Presence.PHap_DeviceImplementationHelper
    {
        //---------------------------------------------------------------------------------------------------------------------------------------------
        // Metadata

        public override string GetImplementationID()
        {
            return "PHap_SenseGlove";
        }



        public override bool SupportsHapticLocation(PHap_BodyPart onBodyPart)
        {
            switch (onBodyPart)
            {
                case PHap_BodyPart.LeftHandPalm:
                case PHap_BodyPart.LeftThumb:
                case PHap_BodyPart.LeftIndexFinger:
                case PHap_BodyPart.LeftMiddleFinger:
                case PHap_BodyPart.LeftRingFinger:
                case PHap_BodyPart.LeftPinky:
                case PHap_BodyPart.RightHandPalm:
                case PHap_BodyPart.RightThumb:
                case PHap_BodyPart.RightIndexFinger:
                case PHap_BodyPart.RightMiddleFinger:
                case PHap_BodyPart.RightRingFinger:
                case PHap_BodyPart.RightPinky:
                    //Since this is the Nova 2.0, ignore the rest for now.
                    return true;
                default:
                    return false;
            }
        }

        public override bool SupportsHapticsType(PHap_HapticModality effectType)
        {
            switch (effectType)
            {
                case PHap_HapticModality.Vibrotactile:
                case PHap_HapticModality.Force:
                case PHap_HapticModality.Stiffness:
            //    case PHap_HapticModality.Pressure:
                //TODO: Add additional supported format(s).
                    return true;
                default:
                    return false;
            }
        }

        public override bool SupportsHaptics(PHap_HapticModality effectType, PHap_BodyPart bodyPart)
        {
            switch (bodyPart)
            {
                case PHap_BodyPart.LeftHandPalm:
                case PHap_BodyPart.RightHandPalm:
                    return effectType == PHap_HapticModality.Force || effectType == PHap_HapticModality.Vibrotactile; //separate since active strap might be used for pressure instead of force?

                case PHap_BodyPart.LeftThumb:
                case PHap_BodyPart.RightThumb:
                case PHap_BodyPart.LeftIndexFinger:
                case PHap_BodyPart.RightIndexFinger:
                    return effectType == PHap_HapticModality.Force || effectType == PHap_HapticModality.Vibrotactile || effectType == PHap_HapticModality.Stiffness; //Thumb / Index are the only ones that have both force and vibro


                case PHap_BodyPart.LeftMiddleFinger:
                case PHap_BodyPart.RightMiddleFinger:
                case PHap_BodyPart.LeftRingFinger:
                case PHap_BodyPart.RightRingFinger:
                    return effectType == PHap_HapticModality.Force || effectType == PHap_HapticModality.Stiffness; //Only ffb for middle + ring

                //No pinky on any of the Nova Gloves.

                default:
                    return false;
            }
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------
        // Transcoding (Editor Only)

        public override bool IsMyCustomFormat(string customFilePath)
        {
#if UNITY_EDITOR
            if (customFilePath.EndsWith(".asset")) //then maybe yes! It's a ScriptableObject. Let's see what we can do.
            {
                if (customFilePath.Contains("StreamingAssets"))
                {
                    Debug.LogWarning(customFilePath + " might be a SenseGlove ScriptableObject, but since it is located inside the StreamingAssets folder, it cannot be loaded. " +
                        "Is this is indeed a SenseGlove Asset you're trying to decone, please make sure it is placed inside a Resources folder instead, and try again. ");
                    return false; //A ScritableObject placed inside the StreamingAssets folder will no longer be treated as such, and instead becomes an "Object". The function below will then fail.
                }
                return PHap_Util.Editor_IsScribtableObjectType<SG_PremadeWaveform>(customFilePath) 
                    || PHap_Util.Editor_IsScribtableObjectType<SG_PremadeForce>(customFilePath)
                    || PHap_Util.Editor_IsScribtableObjectType<SG_PremadeStiffness>(customFilePath);
            }
#endif
            return false;
        }



        public override bool CustomFormat_ToHjif(string customFilePath, string hjifOutputDir, string fileName, out string hjifFilePath)
        {
            string hjifString = "";
            hjifFilePath = System.IO.Path.Combine(hjifOutputDir, fileName + ".hjif").Replace('\\', '/'); //might be using this at some point...
            if (PHap_Util.Editor_LoadScriptableObjectType<SG_PremadeWaveform>(customFilePath, out SG_PremadeWaveform waveForm))
            {
                //it's a PremadeWaveform
                if (!SG_Transcoding.SG_to_HJIFString(waveForm, out hjifString))
                {
                    Debug.LogError("Failed to decode " + waveForm.name + " into an HJIF String", waveForm);
                    return false;
                }
            }
            else if (PHap_Util.Editor_LoadScriptableObjectType<SG_PremadeForce>(customFilePath, out SG_PremadeForce forceEffect))
            {
                //it's a Force Effect
                if (!SG_Transcoding.SG_to_HJIFString(forceEffect, out hjifString))
                {
                    Debug.LogError("Failed to decode " + forceEffect.name + " into an HJIF String", forceEffect);
                    return false;
                }
            }
            else if (PHap_Util.Editor_LoadScriptableObjectType<SG_PremadeStiffness>(customFilePath, out SG_PremadeStiffness stiffnessEffect))
            {
                //it's a Force Effect
                if (!SG_Transcoding.SG_to_HJIFString(stiffnessEffect, out hjifString))
                {
                    Debug.LogError("Failed to decode " + stiffnessEffect.name + " into an HJIF String", stiffnessEffect);
                    return false;
                }
            }
            //TODO: Something else we can convert into an HJIF string, like Force-Feedback?

            //If I get here I've succesfully created the hjifString from a SenseGlove Format. Yippeeee
            if (hjifString.Length > 0)
            {
                try
                {
                    //Now create the HJIF String.
                    if (!System.IO.Directory.Exists(hjifOutputDir))
                    {
                        System.IO.Directory.CreateDirectory(hjifOutputDir);
                    }
                    System.IO.File.WriteAllText(hjifFilePath, hjifString);
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error Saving HJIF String to " + hjifFilePath + ": " + ex.Message);
                }
            }
            return false;
        }


        public override bool Hjif_ToCustomFormat(string hjifFilePath, PHap_BaseEffectMetaData metaData, string effectName, string targetDirectory, out string decodedPath, out string additionalData)
        {
            decodedPath = SG_Transcoding.Hjif_to_SG(hjifFilePath, effectName, targetDirectory);
            additionalData = "";
            return decodedPath.Length > 0;
        }




        //---------------------------------------------------------------------------------------------------------------------------------------------
        // Runtime Implementation



        public override bool Initialize()
        {
            //TODO: Launch SenseCom / SenseGlove Back-End if it isn't already there.
            SG.SG_Core.Setup(); //make sure this one exists
          //  SG_PHapRuntime.TryInitialize(); //I don;t have to make sure this one exists yet until there's an effect I'll be able to play...
            return true;
        }

        public override bool Deinitialize()
        {
            //TODO: Android: Clean up resources...
            return true;
        }

        public override bool DeviceConnected()
        {
            return SGCore.HandLayer.DeviceConnected(true) || SGCore.HandLayer.DeviceConnected(false);
        }

        /// <summary> Vector Index (0 = x, 1 = y, z = 2) that determies which coordinate to use for left - right </summary>
        public const int palmLRIndex = 1;

        /// <summary> Separate Function because for the Hand Palm we wish to determine left / rightedness. </summary>
        /// <param name="effectLocation"></param>
        /// <param name="rightHand"></param>
        /// <param name="sgLocation"></param>
        /// <returns></returns>
        public static bool ToSGVibrationLocation(PHap_EffectLocation effectLocation, out bool rightHand, out SGCore.HapticLocation sgLocation)
        {
            switch (effectLocation.BodyPart)
            {
                // case PHap_BodyPart.LeftHandPalm:
                case PHap_BodyPart.LeftThumb:
                case PHap_BodyPart.RightThumb:
                    sgLocation = SGCore.HapticLocation.Thumb_Tip;
                    rightHand = effectLocation.BodyPart == PHap_BodyPart.RightThumb;
                    return true;

                case PHap_BodyPart.LeftIndexFinger:
                case PHap_BodyPart.RightIndexFinger:
                    sgLocation = SGCore.HapticLocation.Index_Tip;
                    rightHand = effectLocation.BodyPart == PHap_BodyPart.RightIndexFinger;
                    return true;

                case PHap_BodyPart.LeftHandPalm:
                case PHap_BodyPart.RightHandPalm:

                    rightHand = effectLocation.BodyPart == PHap_BodyPart.RightHandPalm;
                    //TODO: Properly Determine which palm motor to use based on where on the palm we're located...
                    sgLocation = effectLocation.LocalPosition[palmLRIndex] >= 0.0f ? SGCore.HapticLocation.Palm_IndexSide : SGCore.HapticLocation.Palm_PinkySide;
                    return true;

                default:
                    rightHand = true;
                    sgLocation = SGCore.HapticLocation.Unknown;
                    return false;
            }

        }

        public static bool UnpackFinger(PHap_BodyPart bodyPart, out int fingerIndex, out bool rightHand)
        {
            switch (bodyPart)
            {
                case PHap_BodyPart.LeftThumb:
                case PHap_BodyPart.RightThumb:
                    rightHand = bodyPart == PHap_BodyPart.RightThumb;
                    fingerIndex = 0;
                    return true;
                case PHap_BodyPart.LeftIndexFinger:
                case PHap_BodyPart.RightIndexFinger:
                    rightHand = bodyPart == PHap_BodyPart.RightIndexFinger;
                    fingerIndex = 1;
                    return true;
                case PHap_BodyPart.LeftMiddleFinger:
                case PHap_BodyPart.RightMiddleFinger:
                    rightHand = bodyPart == PHap_BodyPart.RightMiddleFinger;
                    fingerIndex = 2;
                    return true;
                case PHap_BodyPart.LeftRingFinger:
                case PHap_BodyPart.RightRingFinger:
                    rightHand = bodyPart == PHap_BodyPart.RightRingFinger;
                    fingerIndex = 3;
                    return true;
                case PHap_BodyPart.LeftPinky:
                case PHap_BodyPart.RightPinky:
                    rightHand = bodyPart == PHap_BodyPart.RightPinky;
                    fingerIndex = 4;
                    return true;
                default:
                    rightHand = true;
                    fingerIndex = -1;
                    return false;
            }
        }


        public override bool PlayHapticEffect(PHap_HapticEffect effect, PHap_EffectLocation location)
        {
          //  base.PlayHapticEffect(effect, location); //report that it is happening
            PHap_BaseEffect toPlay = effect.BaseEffect;
            int listIndex, fingerIndex;
            bool rightHand;
            switch (toPlay.GetEffectType())
            {
                case PHap_HapticModality.Vibrotactile:

                    listIndex = ListIndex(toPlay, waveforms);
                    if (listIndex > -1) //we have a loaded waveform and can actually play it...
                    {
                        //TODO: Support Repeating of Custom Waveforms

                        if (ToSGVibrationLocation(location, out rightHand, out SGCore.HapticLocation sgLocation))
                        {
                            SG_PremadeWaveform wfToPlay = waveforms[listIndex].SGEffect;
                            //Now to do something about it...
                            SGCore.CustomWaveform wf = wfToPlay.InternalWaveform;
                            wf.Amplitude = Mathf.Clamp01( wf.Amplitude * effect.EffectiveIntensity );
                            wf.Infinite = effect.IsLooping;
                            return SGCore.HandLayer.SendCustomWaveform(rightHand, wf, sgLocation);
                        }
                    }
                    return false; //otherwise this is not an effect I can play :<


                case PHap_HapticModality.Force: //forces I'll need to update too but based on time...

                    listIndex = ListIndex(toPlay, forces);
                    if (listIndex > -1) //we have a loaded force-effect and can actually play it...
                    {
                        if (location.BodyPart == PHap_BodyPart.LeftHandPalm || location.BodyPart == PHap_BodyPart.RightHandPalm)
                        {
                            Debug.Log("Play force effect on palm!");
                            SG_PHapRuntime.Instance.PlayForceEffectOnPalm(forces[listIndex].SGEffect, location.BodyPart == PHap_BodyPart.RightHandPalm, effect.EffectiveIntensity, effect.IsLooping, effect.RepeatAmount);
                            return true;
                        }
                        else if (UnpackFinger(location.BodyPart, out fingerIndex, out rightHand))
                        {   //we can extract a finger location. Bayum.
                            SG_PHapRuntime.Instance.PlayForceEffectOnFinger(forces[listIndex].SGEffect, fingerIndex, rightHand, effect.EffectiveIntensity, effect.IsLooping, effect.RepeatAmount);
                            return true;
                        }
                    }
                    return false;

                case PHap_HapticModality.Stiffness: //stiffness effects I'll need to continously update

                    listIndex = ListIndex(toPlay, stiffnesses);
                    if (listIndex > -1) //we have a loaded stiffness effect and can actually play it...
                    {
                        if (UnpackFinger(location.BodyPart, out fingerIndex, out rightHand)) //stiffness only works on fingers.
                        {   //we can extract a finger location. Bayum.
                            SG_PHapRuntime.Instance.PlayStiffnessEffectOnFinger(stiffnesses[listIndex].SGEffect, fingerIndex, rightHand, effect.EffectiveIntensity);
                            return true;
                        }
                    }
                    return false;

                default:
                    return false;
            }
        }



        

        public override bool StopHapticEffect(PHap_HapticEffect effect, PHap_EffectLocation location)
        {
           // base.StopHapticEffect(effect, location); //report that it is happening
            int fingerIndex;
            switch (effect.BaseEffect.GetEffectType())
            {
                case PHap_HapticModality.Vibrotactile:
                    
                    //this is an effect we can play
                    if (ToSGVibrationLocation(location, out bool rightHand, out SGCore.HapticLocation sgLocation))
                    {
                        //I actually don't even need to know which waveform it is; since mixing is not supported. So just stopping it on this location is fine.
                        SGCore.CustomWaveform stopWF = new SGCore.CustomWaveform(0.0f, 1.0f, 180.0f); //Amplitude of 0.0f -> Nothing to play.
                        SGCore.HandLayer.SendCustomWaveform(rightHand, stopWF, sgLocation);

                        // Since our Haptics API only has a generic "Hand Palm" location, we turn off the other motor as well just in case.
                        if (sgLocation == SGCore.HapticLocation.Palm_IndexSide)
                            SGCore.HandLayer.SendCustomWaveform(rightHand, stopWF, SGCore.HapticLocation.Palm_PinkySide);
                        if (sgLocation == SGCore.HapticLocation.Palm_PinkySide)
                            SGCore.HandLayer.SendCustomWaveform(rightHand, stopWF, SGCore.HapticLocation.Palm_IndexSide);

                    }
                    return false;
 

                case PHap_HapticModality.Force:
                    
                    //TODO: Wrist Squeeze is classified as force. Could either make it register as a Pressure or Make certain Effects only be applied to certain body parts.

                    if (location.BodyPart == PHap_BodyPart.LeftHandPalm || location.BodyPart == PHap_BodyPart.RightHandPalm)
                    {
                        SG_PHapRuntime.Instance.StopForceEffectOnPalm(location.BodyPart == PHap_BodyPart.RightHandPalm);
                    }
                    else if (UnpackFinger(location.BodyPart, out fingerIndex, out rightHand))
                    {   //we can extract a finger location. Bayum.
                        SG_PHapRuntime.Instance.StopForceEffectOnFinger(fingerIndex, rightHand);
                        return true;
                    }
                    return false;

                case PHap_HapticModality.Stiffness:

                    if (UnpackFinger(location.BodyPart, out fingerIndex, out rightHand))
                    {   //we can extract a finger location. Bayum.
                        SG_PHapRuntime.Instance.StopStiffnessEffectOnFinger(fingerIndex, rightHand);
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }






        //-------------------------------------------------------------------------------------------------------------------------
        // Material Loading / Unloading / Preprocessing.


        /// <summary> Helper class to link BaseEffect to their respective SenseGlove Implementation. </summary>
        public class SG_EffectLink<T>
        {
            public PHap_BaseEffect Effect { get; set; }

            public T SGEffect { get; set; }

            public SG_EffectLink(PHap_BaseEffect eff, T wf)
            {
                Effect = eff;
                SGEffect = wf;
            }
        }

        private static List< SG_EffectLink<SG_PremadeWaveform> > waveforms = new List< SG_EffectLink<SG_PremadeWaveform> >();
        private static List< SG_EffectLink<SG_PremadeStiffness> > stiffnesses = new List< SG_EffectLink<SG_PremadeStiffness> >();
        private static List< SG_EffectLink<SG_PremadeForce> > forces = new List< SG_EffectLink<SG_PremadeForce> >();

        private int ListIndex<T>(PHap_BaseEffect effect, List< SG_EffectLink<T> > effectList)
        {
            for (int i = 0; i < effectList.Count; i++)
            {
                if (effectList[i].Effect == effect)
                    return i;
            }
            return -1;
        }



        public const bool buildLogsEnabled = true;

        /// <summary> For extra logging in builds. </summary>
        /// <param name="message"></param>
        private static void BuildLog(string message)
        {
#if !UNITY_EDITOR
            if (buildLogsEnabled)
            {
                Debug.Log("SG_Presence: " + message);
            }
#endif
        }

        public override bool LoadEffect(PHap_HapticEffect effect)
        {
            //base.LoadEffect(effect); //to report that it's happeneing
            PHap_BaseEffect toLoad = effect.BaseEffect;
            int listIndex; string filePath;
            switch (toLoad.GetEffectType())
            {
                case PHap_HapticModality.Vibrotactile:

                    if (toLoad.GetEffectPath(this, out filePath)) //there is a FilePath available for SenseGlove. Yay.
                    {
                        //Attempt to load this ScriptableObject from file...
                        if (PHap_Util.TryLoadScriptableObject(filePath, out SG_PremadeWaveform waveform))
                        {
                            listIndex = ListIndex(toLoad, waveforms);
                            if (listIndex > -1) //aka We have already loaded the baseEffect
                            {
                                waveforms[listIndex] = new SG_EffectLink<SG_PremadeWaveform>(toLoad, waveform);
                                BuildLog("Updated " + toLoad.name + " as SG Waveform");
                            }
                            else
                            {
                                waveforms.Add(new SG_EffectLink<SG_PremadeWaveform>(toLoad, waveform));
                                BuildLog("Loaded " + toLoad.name + " as SG Waveform");
                            }
                            return true;
                        }
                        else
                            BuildLog("Could not load ScriptableObject at " + filePath + " as SG_PremadeWaveform!");
                    }
                    else
                        BuildLog("There was no SenseGlove EffectPath available for " + toLoad.name + " " + toLoad.PrintFileLinks());
                    return false;

                case PHap_HapticModality.Force:

                    if (toLoad.GetEffectPath(this, out filePath)) //there is a FilePath available for SenseGlove. Yay.
                    {
                        //Attempt to load this ScriptableObject from file...
                        if (PHap_Util.TryLoadScriptableObject(filePath, out SG_PremadeForce force))
                        {
                            listIndex = ListIndex(toLoad, forces);
                            if (listIndex > -1) //aka We have already loaded the baseEffect
                            {
                                forces[listIndex] = new SG_EffectLink<SG_PremadeForce>(toLoad, force);
                                BuildLog("Updated " + toLoad.name + " as SG Force");
                            }
                            else
                            {
                                forces.Add(new SG_EffectLink<SG_PremadeForce>(toLoad, force));
                                BuildLog("Loaded " + toLoad.name + " as SG Force");
                            }
                            return true;
                        }
                        else
                            BuildLog("Could not load ScriptableObject at " + filePath + " as SG_PremadeForce!");
                    }
                    else
                        BuildLog("There was no SenseGlove EffectPath available for " + toLoad.name + " " + toLoad.PrintFileLinks());

                    return false;

                case PHap_HapticModality.Stiffness:

                    if (toLoad.GetEffectPath(this, out filePath)) //there is a FilePath available for SenseGlove. Yay.
                    {
                        //Attempt to load this ScriptableObject from file...
                        if (PHap_Util.TryLoadScriptableObject(filePath, out SG_PremadeStiffness stiffness))
                        {
                            listIndex = ListIndex(toLoad, stiffnesses);
                            if (listIndex > -1) //aka We have already loaded the baseEffect
                            {
                                stiffnesses[listIndex] = new SG_EffectLink<SG_PremadeStiffness>(toLoad, stiffness);
                                BuildLog("Updated " + toLoad.name + " as SG Stiffness");
                            }
                            else
                            {
                                stiffnesses.Add(new SG_EffectLink<SG_PremadeStiffness>(toLoad, stiffness));
                                BuildLog("Loaded " + toLoad.name + " as SG Stiffness");
                            }
                            return true;
                        }
                        else
                            BuildLog("Could not load ScriptableObject at " + filePath + " as SG_PremadeStiffness!");
                    }
                    else
                        BuildLog("There was no SenseGlove EffectPath available for " + toLoad.name + " " + toLoad.PrintFileLinks());

                    return false;

                default:
                    return false;
            }
        }

        public override bool UnloadEffect(PHap_HapticEffect effect)
        {
            PHap_BaseEffect toLoad = effect.BaseEffect;
            int listIndex;

            switch (toLoad.GetEffectType())
            {
                case PHap_HapticModality.Vibrotactile:

                    listIndex = ListIndex(toLoad, waveforms);
                    if (listIndex > -1) //aka We have already loaded the baseEffect
                        waveforms.RemoveAt(listIndex);
                    return true;

                case PHap_HapticModality.Force:

                    listIndex = ListIndex(toLoad, forces);
                    if (listIndex > -1) //aka We have already loaded the baseEffect
                        forces.RemoveAt(listIndex);
                    return true;

                case PHap_HapticModality.Stiffness:

                    listIndex = ListIndex(toLoad, stiffnesses);
                    if (listIndex > -1) //aka We have already loaded the baseEffect
                        stiffnesses.RemoveAt(listIndex);
                    return true;


                default:
                    return true;
            }
        }



       

    }


    


}