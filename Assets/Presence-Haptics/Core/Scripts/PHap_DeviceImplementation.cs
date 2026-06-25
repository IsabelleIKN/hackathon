using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Contains your device's implementation of the Presense Haptics API.
 * Consider this the entry point for various methods, that includes functions called inside the editor.
 * 
 */

namespace Presence
{

    public abstract class PHap_DeviceImplementation : ScriptableObject
    {
        //------------------------------------------------------------------------------------------------------------
        // Editor Functions (Transcoding etc)

        /// <summary> For GUI / Referencing inside the editor. By default; it's the name of the ScriptableObject. </summary>
        /// <returns></returns>
        public abstract string GetImplementationID();


        /// <summary> Returns true if this Implementation supports the type of haptics. If not, we won't bother doing any Transcoding. (e.g. When my Haptic vest does not support Temperature Feedback). </summary>
        /// <param name="effectType"></param>
        /// <returns></returns>
        public abstract bool SupportsHapticsType(PHap_HapticModality effectType);

        /// <summary> Retrieves a list of all Haptic Modalities this Implementation Support. </summary>
        /// <returns></returns>
        public virtual PHap_HapticModality[] GetSupportedHapticTypes()
        {
            List<PHap_HapticModality> supported = new List<PHap_HapticModality>();
            for (int i = 0; i < (int)PHap_HapticModality.All; i++)
            {
                PHap_HapticModality mod = (PHap_HapticModality)i;
                if (this.SupportsHapticsType(mod))
                {
                    supported.Add(mod);
                }
            }
            return supported.ToArray();
        }


        /// <summary> Returns true if your Implementation supports Haptics on this particlar Body Part. </summary>
        /// <param name="onBodyPart"></param>
        /// <returns></returns>
        public abstract bool SupportsHapticLocation(PHap_BodyPart onBodyPart);

        /// <summary> Retrieves a list of all Haptic Modalities this Implementation Support. </summary>
        /// <returns></returns>
        public virtual PHap_BodyPart[] GetSupportedHapticLocations()
        {
            List<PHap_BodyPart> supported = new List<PHap_BodyPart>();
            for (int i = 0; i < (int)PHap_BodyPart.All; i++)
            {
                PHap_BodyPart loc = (PHap_BodyPart)i;
                if (this.SupportsHapticLocation(loc))
                {
                    supported.Add(loc);
                }
            }
            return supported.ToArray();
        }

        /// <summary> Take an HJIF File and convert it to your own custom format (e.g. .haps, .wav). Use the metaData to check if you support this effect type, for instance.
        /// If you don't support the type of effect, simply return false. </summary>
        /// <param name="hjifFilePath"></param>
        /// <param name="decodedPath"></param>
        /// <returns></returns>
        public abstract bool Hjif_ToCustomFormat(string hjifFilePath, PHap_BaseEffectMetaData metaData, string effectName, string targetDirectory, out string decodedPath, out string additionalData);



        /// <summary> Return true if a filename e.g. ""testBump.haps" is of a file format that this implementation uses. If true, we will ask you to convert it from your format into HJIF. </summary>
        /// <param name="customFilePath"></param>
        /// <returns></returns>
        public abstract bool IsMyCustomFormat(string customFilePath);


        /// <summary> Return extra data from an original file. Done when we determine that customFilePath belongs to this implementation. You can assume the path is valid and the file exists in the editor. </summary>
        /// <param name="customFilePath"></param>
        /// <returns></returns>
        public abstract string GetExtraDataFromFile(string customFilePath);

        /// <summary> Convert your own custom file format into an HJIF implementation. If you cannot, return false.  </summary>
        /// <param name="customFileFormat"></param>
        /// <param name="hjifOutputDir"></param>
        /// <param name="hjifFilePath"></param>
        /// <returns></returns>
        public abstract bool CustomFormat_ToHjif(string customFilePath, string hjifOutputDir, string fileName, out string hjifFilePath);


        /// <summary> Special use case </summary>
        /// <param name="customFilePath"></param>
        /// <param name="effect"></param>
        public abstract PHap_BaseEffectMetaData FallbackGetEffectParameters(string customFilePath);


        //------------------------------------------------------------------------------------------------------------
        // Runtime Functions (Load / Play etc)


        public abstract bool Initialize();

        public abstract bool Deinitialize();


        /// <summary> Load an effect at runtime from StreamingAssets / Resources so it can be played instantly later. Note; this might be an asynchronous effect, so make sure to do so at the start. </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public abstract bool LoadEffect(PHap_HapticEffect effect);


        /// <summary> Unloads an effect at runtime. WARNING: Multiple effects could be using the same BaseEffect? </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public abstract bool UnloadEffect(PHap_HapticEffect effect);



        public abstract string GetDeviceName();


        /// <summary> Return true if (at least one) device relevant to this implementation is currently connected. </summary>
        /// <returns></returns>
        public abstract bool DeviceConnected();
        
        public abstract bool PlayHapticEffect(PHap_HapticEffect effect, PHap_EffectLocation location);

        public abstract bool StopHapticEffect(PHap_HapticEffect effect, PHap_EffectLocation location);



        /// <summary> Query this implementation; does it support haptics X on location Y </summary>
        /// <param name="bodyPart"></param>
        /// <param name="effectType"></param>
        /// <returns></returns>
        public abstract bool SupportsHaptics(PHap_HapticModality effectType, PHap_BodyPart bodyPart);
    }

}