/*
 * Actronika implementation of the Presence Haptics API. Contains stubs for Actronika Devs to implement.
 * 
 * author:
 * max@senseglove.com
 */


using Newtonsoft.Json.Linq;
using Skinetic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Skinetic.SkineticDevice;

namespace Presence
{
    //[CreateAssetMenu(fileName = "PHap_Actronika", menuName = "Presence/Implementations/Actronika", order = 1)]
    public class PHap_ActronikaImpl : PHap_DeviceImplementationHelper
    {

        //---------------------------------------------------------------------------------------------------------------------------------------------
        // Metadata

        public override string GetImplementationID()
        {
            return "PHap_Actronika";
        }

        public override bool SupportsHapticsType(PHap_HapticModality effectType)
        {
            switch (effectType)
            {
                //TODO: Add additional supported format(s).
                case PHap_HapticModality.Vibrotactile:
                    return true;
                default:
                    return false;
            }
        }

        public override bool SupportsHapticLocation(PHap_BodyPart onBodyPart)
        {
            switch (onBodyPart)
            {
                //TODO: Add additional supported format(s).
                case PHap_BodyPart.Torso:
                    return true;
                default:
                    return false;
            }
        }

        public override bool SupportsHaptics(PHap_HapticModality effectType, PHap_BodyPart bodyPart)
        {
            return SupportsHapticsType(effectType) & SupportsHapticLocation(bodyPart);
        }

        //------------------------------------------------------------------------------------------------------------
        // Transcoding

        public override bool IsMyCustomFormat(string customFilePath)
        {
            return customFilePath.EndsWith(".spn");
        }

        public override string GetExtraDataFromFile(string customFilePath)
        {
            try
            {
                string res = System.IO.File.ReadAllText(customFilePath);
                return res;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            return "";
        }

        public override PHap_BaseEffectMetaData FallbackGetEffectParameters(string customFilePath)
        {
            // this function is called when you have not implemented CustomFormat to HJIF. 
            //it is up to you to provide the data we would normally extract from the HJIF file to ensure you've encoded it properly.
            PHap_BaseEffectMetaData metaData = new PHap_BaseEffectMetaData();
            metaData.Duration = 1.0f;
            metaData.SetModality(PHap_HapticModality.Vibrotactile);
            return metaData;
        }

        //------------------------------------------------------------------------------------------------------------
        // Runtime Functions (Load / Play etc)

        private Dictionary<PHap_HapticEffect, JObject> m_loadedPatterns = new Dictionary<PHap_HapticEffect, JObject>();
        private Dictionary<PHap_HapticEffect, int> m_effectIDs = new Dictionary<PHap_HapticEffect, int>();
        private Dictionary<PHap_HapticEffect, List<JObject>> m_prefetchVicinities = new Dictionary<PHap_HapticEffect, List<JObject>>();


        public override bool Initialize()
        {
            Actronika_PHapRuntime.TryInitialize();
            return true;
        }

        public override bool Deinitialize()
        {
            //todo destroy singleton? Ressources are linked to the instance
            Actronika_PHapRuntime.TryDeinitialize();
            return true;
        }


        /// <summary> Load an effect at runtime from StreamingAssets / Resources so it can be played instantly later. Note; this might be an asynchronous effect, so make sure to do so at the start. </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public override bool LoadEffect(PHap_HapticEffect effect)
        {
            JObject parsedSPN;
            List<JObject> vicinities = new List<JObject>();

            if (m_loadedPatterns.ContainsKey(effect))
                return false;

            if (effect.BaseEffect.GetEffectType() != PHap_HapticModality.Vibrotactile)
                return false;

            string filePath;
            string extraData; //TODO: Use MetaData to store this file...?
            if (!effect.BaseEffect.GetEffectPath(this, out filePath, out extraData))
                return false;

            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("Invalid filePath! Empty or NULL");
                return false;
            }
#if UNITY_EDITOR //outside of the editor, this filepath should be loaded from StreamingAssets, which I do from TryLoadScriptableObject()
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError("The file " + filePath + " does not exist!");
                return false;
            }
#endif
            if (!filePath.ToLower().EndsWith(".spn"))
            {
                Debug.LogError(filePath + ": Invalid FileType! Expected .spn but got " + System.IO.Path.GetExtension(filePath));
                return false;
            }
            //TODO: Access file via SteamingAssets rather than IO.File in Builds.
            string json; // = System.IO.File.ReadAllText(filePath);
            //if (!PHap_Util.LoadFromStreamingAssets(filePath, out json))
            //{
            //    Debug.LogError(filePath + ": Could not be loaded from StreamingAssets!");
            //    return false;
            //}
            json = extraData;

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError(filePath + ": is empty!");
                return false;
            }
            try
            {
                parsedSPN = JObject.Parse(json);
            }
            catch{
                Debug.LogError(filePath + ": Invalid Json!");
                return false;
            }

            JArray jTracks = (JArray)parsedSPN["tracks"];
            if (jTracks == null)
            {
                Debug.LogError(filePath + ": Invalid .spn (no track)!");
                return false;
            }
            
            foreach (JObject track in jTracks)
            {
                JArray jSamples = (JArray)track["samples"];
                if (jSamples == null)
                {
                    Debug.LogError(filePath + ": Invalid .spn (no samples)!");
                    return false;
                }
                foreach (JObject sample in jSamples)
                {
                    JArray jSpatKeyframes = (JArray)sample["spatKeyframes"];
                    if (jSpatKeyframes == null)
                    {
                        Debug.LogError(filePath + ": Invalid .spn (no spatKeyframes)!");
                        return false;
                    }
                    foreach (JObject spatKeyframe in jSpatKeyframes)
                    {
                        JArray jVicinities = (JArray)spatKeyframe["vicinities"];
                        if (jVicinities == null)
                        {
                            Debug.LogError(filePath + ": Invalid .spn (no vicinities)!");
                            return false;
                        }
                        foreach (JObject jVicinity in jVicinities)
                        {
                            vicinities.Add(jVicinity);
                        }
                    }
                }
            }
            m_loadedPatterns.Add(effect, parsedSPN);
            m_prefetchVicinities.Add(effect, vicinities);
            return true;
        }


        /// <summary> Unloads an effect at runtime. WARNING: Multiple effects could be using the same BaseEffect? </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public override bool UnloadEffect(PHap_HapticEffect effect)
        {
            return m_loadedPatterns.Remove(effect);
        }



        public override string GetDeviceName()
        {
            return "Skinetic";
        }


        /// <summary> Return true if (at least one) device relevant to this implementation is currently connected. </summary>
        /// <returns></returns>
        public override bool DeviceConnected()
        {
            return Actronika_PHapRuntime.Instance.DeviceConnected();
        }

        public override bool PlayHapticEffect(PHap_HapticEffect effect, PHap_EffectLocation location)
        {
            if (m_effectIDs.ContainsKey(effect)) //not playing if the effect is already playing
                if (Actronika_PHapRuntime.Instance.SkineticInstance.GetEffectState(m_effectIDs[effect]) != HapticEffect.State.E_STOP)
                    return false;

            if (location.BodyPart != PHap_BodyPart.Torso || effect.EffectType != PHap_HapticModality.Vibrotactile)
                return false; //this is not an effect that we can play, so no need to check

            if (!m_loadedPatterns.ContainsKey(effect))
                return false;

            int repeatCount = 1;
            if (effect.IsLooping)
                repeatCount = 0;
            else
                repeatCount = effect.RepeatAmount;

            SkineticDevice.EffectProperties effectProperties = new SkineticDevice.EffectProperties(5, 100, 1, repeatCount, 0, 0, 0, 0, false, 0, 0, 0, false, false, false, false, false, false);

            effectProperties.volume = effect.Intensity * 100 / PHap_HapticEffect.maxIntensity;

            string json = m_loadedPatterns[effect].ToString();
            foreach (JObject vicinity in m_prefetchVicinities[effect])
            {
                Vector3 rescaledPosition, rescaleSize;
                TransformPosition(out rescaledPosition, out rescaleSize, ref location);
                vicinity["position"][0] = rescaledPosition.x;
                vicinity["position"][1] = rescaledPosition.y;
                vicinity["position"][2] = rescaledPosition.z;

                vicinity["scale"][0] = rescaleSize.x;
                vicinity["scale"][1] = rescaleSize.y;
                vicinity["scale"][2] = rescaleSize.z; 
                Debug.Log($"Position: [{vicinity["position"][0]}, {vicinity["position"][1]}, {vicinity["position"][2]}]   --- Size: [{vicinity["scale"][0]}, {vicinity["scale"][1]}, {vicinity["scale"][2]}] ");

            }
            json = m_loadedPatterns[effect].ToString();

            int patternID = Actronika_PHapRuntime.Instance.SkineticInstance.LoadPatternFromJSON(json);
            
            m_effectIDs[effect] = Actronika_PHapRuntime.Instance.SkineticInstance.PlayEffect(patternID, effectProperties);
            

            Actronika_PHapRuntime.Instance.SkineticInstance.UnloadPattern(patternID);

            return true;
        }

        public override bool StopHapticEffect(PHap_HapticEffect effect, PHap_EffectLocation location)
        {
            int ret;
            if (!m_effectIDs.ContainsKey(effect)) //not playing if the effect is already playing
                return false;
            ret = Actronika_PHapRuntime.Instance.SkineticInstance.StopEffect(m_effectIDs[effect], 0);
            return !(ret < 0);
        }


        private const float refHeight = 0.42f;
        private const float refWidth = 0.24f;
        private const float minSize = 0.15f;
        private void TransformPosition(out Vector3 rescaledPosition, out Vector3 rescaledSize, ref PHap_EffectLocation location)
        {
            rescaledPosition = new Vector3();
            rescaledSize = new Vector3();
            rescaledPosition[0] = (location.LocalPosition[2] - location.BoundingBoxCenter[2]) * refWidth / location.BoundingBoxSize[2];
            rescaledPosition[1] = (location.LocalPosition[1] - location.BoundingBoxCenter[1]) * refHeight / location.BoundingBoxSize[1];
            rescaledPosition[2] = (location.LocalPosition[0] - location.BoundingBoxCenter[0]) * refWidth / location.BoundingBoxSize[0];

            rescaledSize[0] = Mathf.Max(location.EffectSize * refWidth / location.BoundingBoxSize[2], minSize);
            rescaledSize[1] = Mathf.Max(location.EffectSize * refHeight / location.BoundingBoxSize[1], minSize);
            rescaledSize[2] = Mathf.Max(location.EffectSize * refWidth / location.BoundingBoxSize[0], minSize);
        }


    }
}