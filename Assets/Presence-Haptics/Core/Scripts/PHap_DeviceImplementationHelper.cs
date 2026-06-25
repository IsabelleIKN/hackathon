/*
 * 'Helper' Class for those looking to create their own PHap_DeviceImplementation from scratch.
 * It implements all abstract functions by reporting on the function call + parameters to let you when they are called.
 * But you're not immedeately required to implement all abstarct functions.
 * 
 * authors:
 * max@senseglove.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Presence
{
    /// <summary> Helper Implementation of PHap_DeviceImplementation for Device Implementers </summary>
    public class PHap_DeviceImplementationHelper : PHap_DeviceImplementation
    {
        //----------------------------------------------------------------------------------------------------------------------------------
        // MetaData

        /// <summary> Used to identify one's Implementation among others. This should not change at risk of breaking the links of your existing (transcoding) info. </summary>
        /// <returns></returns>
        public override string GetImplementationID()
        {
            Debug.Log(this.name + " is asked for its ImplementationID. This should be a unique string that you use to distinguish it from other API Implementations. " +
                "Please override GetImplementationID() as soon as you're able, and do not change it over the project(s) lifetime.", this);
            return this.name;
        }

        /// <summary> Return true if your implementation supports this Haptics type. We do so to avoid asking you to parse a file or play an effect. </summary>
        /// <param name="effectType"></param>
        /// <returns></returns>
        public override bool SupportsHapticsType(PHap_HapticModality effectType)
        {
            Debug.Log(this.GetImplementationID() + ": SupportsHapticsType(" + effectType.ToString() + "); Please override SupportsHapticsType(PHap_HapticModality effectType) as soon as you're able", this);
            return effectType != PHap_HapticModality.Unknown; //return true by default. Which may be a wast e of resources at transfocing but at least you'll still always see the message above.
        }


        /// <summary> Return true if this Implementation supports Haptics on a specific location. </summary>
        /// <param name="onBodyPart"></param>
        /// <returns></returns>
        public override bool SupportsHapticLocation(PHap_BodyPart onBodyPart)
        {
            Debug.Log(this.GetImplementationID() + ": SupportsHapticLocation(" + onBodyPart.ToString() + "); Please override SupportsHapticLocation(PHap_BodyPart onBodyPart) as soon as you're able", this);
            return onBodyPart != PHap_BodyPart.Unknown; //return true by default. Which may be a wast e of resources at transfocing but at least you'll still always see the message above.
        }


        /// <summary> Retun true if this implementation suppports haptics of a specific type on a specific body part. This is generally used for debugging / diagnostics. </summary>
        /// <param name="effectType"></param>
        /// <param name="bodyPart"></param>
        /// <returns></returns>
        public override bool SupportsHaptics(PHap_HapticModality effectType, PHap_BodyPart bodyPart)
        {
            Debug.Log(this.GetImplementationID() + ": SupportsHaptics(" + effectType.ToString() + ", " + bodyPart.ToString() + ");", this);
            return SupportsHapticsType(effectType) && SupportsHapticLocation(bodyPart); //Note: This may not give the intended result if you can play mixed effects (eg force and vibrotactile on different fingers)
        }




        //----------------------------------------------------------------------------------------------------------------------------------
        // Editor / Transcoding

        
        /// <summary> Editor only: Convert an HJIF file into my own custom format and return a variable to store it as part of the BaseEffect. </summary>
        /// <param name="hjifFilePath"></param>
        /// <param name="metaData"></param>
        /// <param name="effectName"></param>
        /// <param name="targetDirectory"></param>
        /// <param name="decodedPath"></param>
        /// <returns></returns>
        public override bool Hjif_ToCustomFormat(string hjifFilePath, PHap_BaseEffectMetaData metaData, string effectName, string targetDirectory, out string decodedPath, out string additionalData)
        {
            Debug.Log(this.name + ": Hjif_ToCustomFormat(" + hjifFilePath+ ", " + metaData.ToString() + ", " + effectName + ", " + targetDirectory + ", , out string decodedPath);", this);
            decodedPath = "";
            additionalData = "";
            return false;
        }


        /// <summary> Convert a filePath that contains this impplementation's Custom File Format, into  </summary>
        /// <param name="customFilePath"></param>
        /// <param name="hjifOutputDir"></param>
        /// <param name="fileName"></param>
        /// <param name="hjifFilePath"></param>
        /// <returns></returns>
        public override bool CustomFormat_ToHjif(string customFilePath, string hjifOutputDir, string fileName, out string hjifFilePath)
        {
            Debug.Log(this.GetImplementationID() + ": CustomFormat_ToHjif(" + customFilePath + ", " + hjifOutputDir + ", " + fileName + "out string hjifFilePath);", this);
            hjifFilePath = "";
            return false;
        }

        /// <summary> Used to discover which implementation a random (non-HJIF) file belongs to for Transcoding Purposes. </summary>
        /// <param name="customFilePath"></param>
        /// <returns></returns>
        public override bool IsMyCustomFormat(string customFilePath)
        {
            Debug.Log(this.GetImplementationID() + ": IsMyCustomFormat(" + customFilePath + ");", this);
            return false;
        }


        /// <summary> Return extra data from an original file. Done when we determine that customFilePath belongs to this implementation. You can assume the path is valid and the file exists in the editor. </summary>
        /// <param name="customFilePath"></param>
        /// <returns></returns>
        public override string GetExtraDataFromFile(string customFilePath)
        {
            Debug.Log(this.GetImplementationID() + ": GetExtraDataFromFile(" + customFilePath + ");", this);
            return "";
        }


        /// <summary> Special use case that happens if we cannot encode an effect into HJIF. In that case, it only has an original effect, and data is set via the implementation itself </summary>
        /// <param name="customFilePath"></param>
        /// <param name="effect"></param>
        public override PHap_BaseEffectMetaData FallbackGetEffectParameters(string customFilePath)
        {
            Debug.Log(this.GetImplementationID() + ": FallbackSetEffectParameters(" + customFilePath + ",);\n" +
                "This usually indicates you have not (properly) implemented the CustomFormat_ToHjif function. In this case, it's up to you to set effect data such as 'duration' and 'effect type'", this);

            PHap_BaseEffectMetaData metaData = new PHap_BaseEffectMetaData();
            metaData.Duration = 1.0f;
            metaData.SetModality(PHap_HapticModality.Unknown);
            return metaData;
        }



        //----------------------------------------------------------------------------------------------------------------------------------
        // Runtime Implementation(s).

        /// <summary> Start up your Implementation resources. Called upon Start() </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            Debug.Log(this.GetImplementationID() + ": Initialize();", this);
            return false;
        }


        /// <summary> Clean up this Implementation's resources, called on OnApplicationQuit() </summary>
        /// <returns></returns>
        public override bool Deinitialize()
        {
            Debug.Log(this.GetImplementationID() + ": Deinitialize();", this);
            return false;
        }


        /// <summary> Loads a BaseEffect into this Implementation, which can include (but is not limited to) unpacking the original file (.asset / .hjif / .haps / etc). </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public override bool LoadEffect(PHap_HapticEffect effect)
        {
            Debug.Log(this.GetImplementationID() + ": LoadEffect(" + effect.ToString() + ");", this);
            return false;
        }


        /// <summary> Clean up the resources for a BaseEffect of this Implementation </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public override bool UnloadEffect(PHap_HapticEffect effect)
        {
            Debug.Log(this.GetImplementationID() + ": UnloadEffect(" + effect.ToString() + ");", this);
            return false;
        }



        /// <summary> Return true if there is a Device related to this implementation is Connected </summary>
        /// <returns></returns>
        public override bool DeviceConnected()
        {
            Debug.Log(this.GetImplementationID() + ": DeviceConnected();", this);
            return false;
        }

        /// <summary> Is there is a device related to this implementation connected to the system, report its name. </summary>
        /// <returns></returns>
        public override string GetDeviceName()
        {
            Debug.Log(this.GetImplementationID() + ": GetDeviceName();", this);
            return this.GetImplementationID() + " (N/A)";
        }


        /// <summary> Starts playing a particular HapticEffect on a particular location </summary>
        /// <param name="effect"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public override bool PlayHapticEffect(PHap_HapticEffect effect, PHap_EffectLocation location)
        {
            Debug.Log(this.GetImplementationID() + ": PlayHapticEffect(" + effect.ToString() + ", " + location.ToString() + ");", this);
            return false;
        }


        /// <summary> Stop playing a particular haptic effect regardless of location(s). Mostly relevant for looping version(s)? </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public override bool StopHapticEffect(PHap_HapticEffect effect, PHap_EffectLocation location)
        {
            Debug.Log(this.GetImplementationID() + ": StopHapticEffect(" + effect.ToString() + ", " + location.ToString() + ");", this);
            return false;
        }





        //----------------------------------------------------------------------------------------------------------------------------------
        // Debug / Redundant


    }
}