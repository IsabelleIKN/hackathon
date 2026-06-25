using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/*
 * Class responsible for converting / creating SenseGlove-related assets out of .hjif file(s).
 * 
 * @author
 * max@senseglove.com
 */


namespace Presence
{

    /// <summary> SenseGlove Transcoding Implementation. </summary>
    public class SG_Transcoding : MonoBehaviour
    {
        public const int sgId = 42069;


        /// <summary> Give that a try... </summary>
        /// <param name="filePath"></param>
        /// <param name="hjifObject"></param>
        /// <returns></returns>
        public static bool GetHjifObject(string filePath, out JObject hjifObject)
        {
            hjifObject = null;
            if (PHap_Transcoding.GetHjifString(filePath, out string hjifString))
            {
                hjifObject = JObject.Parse(hjifString);
            }
            return hjifObject != null;
        }


        /// <summary> Converts an HJIF file into a SenseGlove one of the appropriate type. Returns the filePath to this .asset. </summary>
        /// <param name="hjifFilePath"></param>
        /// <returns></returns>
        public static string Hjif_to_SG(string hjifFilePath, string effectName, string targetDirectory)
        {
            //Todo: Just vibration for now, but I need to somehow tell by my format what type it is when decoding as runtime :I
            //Debug.Log("Decoding HJIF into SenseGlove Format...");

            //Step 1: Is this a SenseGlove format or nah?

            if (PHap_Transcoding.GetHjifString(hjifFilePath, out string hjifString))
            {
                JObject parsedHJIF = JObject.Parse(hjifString);
                if (parsedHJIF == null)
                    return "";
                /*
                  "version": "CRM3.2",
                  "profile": "Main",
                  "level": 1,
                  "date": "",
                  "description": "",
                  "timescale": 1000,
                  "avatars": [],
                  "perceptions": [...] //every perception has a 'modality' and 'reference device'
                  "syncs": []
                 */

                float timeScale = parsedHJIF[PHap_Transcoding.timeScalesKey] != null ? (float)parsedHJIF[PHap_Transcoding.timeScalesKey] : 1.0f;
                //Grab all perceptions
                JArray perceptionsArray = (JArray)parsedHJIF[PHap_Transcoding.perceptionsKey];
                if (perceptionsArray == null)
                    return ""; //no perceptions array found :<


                PHap_HapticModality effectType = PHap_HapticModality.Unknown;
                string referenceDevice = "";
                JObject chosenPerception = null;
                for (int i = 0; i < perceptionsArray.Count; i++)
                {
                    JObject perception = (JObject)perceptionsArray[i];
                    string perc = (string)perception[PHap_Transcoding.modalityKey];
                    if (PHap_Transcoding.Hjif_ParseModality(perc, out effectType)) //it's a valid modality. But do we support it?
                    {
                        if (effectType != PHap_HapticModality.Vibrotactile && effectType != PHap_HapticModality.Force && effectType != PHap_HapticModality.Stiffness)
                            continue; //go to the next loop since senseglove can't use this modality

                        //valid modality; let's the the reference device.
                        chosenPerception = perception; //for easier access later
                        referenceDevice = PHap_Transcoding.ExtractFirstReferenceDevice((JArray)perception["reference_devices"]);
                        break;
                    }
                }
                if (effectType == PHap_HapticModality.Unknown)
                    return ""; //no valid modality found

                //Debug.Log("Unpacked a " + effectType.ToString() + " effect made for " + referenceDevice);
                referenceDevice = referenceDevice.ToLower();
                if (!referenceDevice.Contains("senseglove"))
                {
                    //TODO: Parse non-senseglove effects(.haps) and split these into multiple SG_Waveforms.
                    Debug.LogWarning(effectName + " is not an effect created by SenseGlove. We will need to 'chain' SenseGlove waveforms together for that." +
                        " At the moment, the SenseGlove PHap Implementation does not support this functionality yet.");

                    if (effectType == PHap_HapticModality.Vibrotactile)
                    {
                        Debug.LogWarning("Creating a placeholder waveform...");
                        string outputPath = System.IO.Path.Combine(PHap_Transcoding.TranscodeOutputFolder_Resources, effectName + ".asset"); //SenseGlove should not be placed in a StreamingAssets, but in a Resources folder
                        SG_PremadeWaveform vibroEffect = LoadOrCreate<SG_PremadeWaveform>(outputPath);

                        vibroEffect.amplitude = 1.0f;
                        vibroEffect.sustainTime = 0.2f;
                        vibroEffect.startFrequency = 180;
                        vibroEffect.endFrequency = 180;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(vibroEffect);
#endif
                        return outputPath;
                    }
                    return ""; //This is not an effect made for a SenseGlove device
                }
                else
                {
                    //it's a SenseGlove effect. 
                    if (effectType == PHap_HapticModality.Vibrotactile)
                        return CreateVibro_FromSGHjif(chosenPerception, timeScale, effectName, targetDirectory);
                    else if (effectType == PHap_HapticModality.Force)
                        return CreateForce_FromSGHjif(chosenPerception, timeScale, effectName, targetDirectory);
                    else if (effectType == PHap_HapticModality.Stiffness)
                        return CreateStiffness_FromSGHjif(chosenPerception, timeScale, effectName, targetDirectory);
                }
            }
            return "";
        }


        private static T LoadOrCreate<T>(string filePath) where T : ScriptableObject
        {
#if UNITY_EDITOR
            if (!System.IO.Directory.Exists(PHap_Transcoding.TranscodeOutputFolder_Resources))
                System.IO.Directory.CreateDirectory(PHap_Transcoding.TranscodeOutputFolder_Resources);

            if (System.IO.File.Exists(filePath))
            {
                T loadedEffect = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(filePath);
                if (loadedEffect != null)
                    return loadedEffect;
            }

            T res = ScriptableObject.CreateInstance<T>();
            UnityEditor.AssetDatabase.CreateAsset(res, filePath);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(res);
            return res;
#else
            Debug.LogError("Transcoding function called at Runtime! This should be done in the Editor, and indicates a problem!");
            return null;
#endif
        }

        private static string CreateVibro_FromSGHjif(JObject perception, float timeScale, string effectName, string targetDirector)
        {
            // a perception created by SG has one channel with one band with multiple effects (each one being the same keypoints at a different stating time.
            JArray channels = (JArray)perception[PHap_Transcoding.channelsKey];
            if (channels == null || channels.Count == 0)
                return "";

            JArray bands = (JArray)channels[0][PHap_Transcoding.bandsKey]; //should have one channel
            if (bands == null || bands.Count == 0)
                return "";

            //the one band contains X amount of effects that are identical. Just with a different StartTime determined by repeatAmount
            JArray effects = (JArray)bands[0][PHap_Transcoding.effectsKey]; //should have one band
            if (effects == null || effects.Count == 0)
                return "";

            int repeatAmount = effects.Count;

            JObject baseEffect = (JObject)effects[0];


            SGCore.WaveformType waveFormType = ToWaveformType((string)baseEffect["base_signal"]);

            //every effect has 5 keyframes
            JArray keyFrames = (JArray)baseEffect[PHap_Transcoding.keyFramesKey]; //should be one effect
            if (keyFrames == null || keyFrames.Count < 5) //less than 5 means its not one made by us :/
                return "";

            //Now that we're here, I'm confident we can create the asset.
            string outputPath = System.IO.Path.Combine(PHap_Transcoding.TranscodeOutputFolder_Resources, effectName + ".asset"); //SenseGlove should not be placed in a StreamingAssets, but in a Resources folder
            SG_PremadeWaveform vibroEffect = LoadOrCreate<SG_PremadeWaveform>(outputPath);

            //At this point, the vibro-effect has been created.
            vibroEffect.RepeatAmount = repeatAmount;
            vibroEffect.waveformType = waveFormType;
            vibroEffect.intendedMotor = VibrationLocation.Handpalm; //handpalm for now, since we're passing a different location anywy when playing.

            JObject k0 = (JObject)keyFrames[0];
            JObject k1 = (JObject)keyFrames[1];
            JObject k2 = (JObject)keyFrames[2];
            JObject k3 = (JObject)keyFrames[3];
            JObject k4 = (JObject)keyFrames[4];

            vibroEffect.amplitude = (float)k1[PHap_Transcoding.ampl_modKey]; //the amplitude modulation indicated in the 2nd frame
            vibroEffect.startFrequency = (int)k0[PHap_Transcoding.freq_modKey]; //the start frequency is indicated in the first frame
            vibroEffect.endFrequency = (int)k3[PHap_Transcoding.freq_modKey]; //the start frequency is indicated in the second to last frame?

            //Since I'm re-using these. t0 is 0, so that one is not relevant.
            float t1 = (float)k1[PHap_Transcoding.relPosKey] / timeScale;
            float t2 = (float)k2[PHap_Transcoding.relPosKey] / timeScale;
            float t3 = (float)k3[PHap_Transcoding.relPosKey] / timeScale;
            float t4 = (float)k4[PHap_Transcoding.relPosKey] / timeScale;

            vibroEffect.attackTime = t1;
            vibroEffect.sustainTime = t2 - t1;
            vibroEffect.decayTime = t3 - t2;
            vibroEffect.pauseTime = t4 - t3;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(vibroEffect);
#endif
            return outputPath;
        }


        /// <summary> Extracts all KeyFrame XY values from a perception </summary>
        /// <param name="perception"></param>
        /// <returns></returns>
        private static Vector2[] GetKeyframes(JObject perception, string keyX, string keyY)
        {
            // a perception created by SG has one channel with one band with multiple effects (each one being the same keypoints at a different stating time.
            JArray channels = (JArray)perception[PHap_Transcoding.channelsKey];
            if (channels == null || channels.Count == 0)
                return new Vector2[0];

            JArray bands = (JArray)channels[0][PHap_Transcoding.bandsKey]; //should have one channel
            if (bands == null || bands.Count == 0)
                return new Vector2[0];

            //the one band contains X amount of effects that are identical. Just with a different StartTime determined by repeatAmount
            JArray effects = (JArray)bands[0][PHap_Transcoding.effectsKey]; //should have one band
            if (effects == null || effects.Count == 0)
                return new Vector2[0];

            //every effect has keyframes
            JArray keyFrames = (JArray)effects[0][PHap_Transcoding.keyFramesKey]; //should be one effect
            if (keyFrames == null || keyFrames.Count == 0) //less than 5 means its not one made by us :/
                return new Vector2[0];

            //when we get here, we finally have al ze keyframes.
            Vector2[] res = new Vector2[keyFrames.Count];
            for (int i = 0; i < keyFrames.Count; i++)
            {
                JObject obj = (JObject)keyFrames[i];
                res[i] = new Vector2
                (
                    (float)obj[keyX],
                    (float)obj[keyY]
                );
            }
            return res;
        }

        private static bool ExtractLinears(Vector2[] bkeyFrames, float timeScale, out float x0, out float y0, out float x1, out float y1, out float endTime)
        {
            if (bkeyFrames.Length < 4)
            {
                x0 = 0.0f;
                y0 = 0.0f;
                x1 = 1.0f;
                y1 = 1.0f;
                endTime = 1.0f;
                return false;
            }

            float unscaledEnd = bkeyFrames[bkeyFrames.Length - 1].x; //this is the max. timeStamp. We'll div eveything by this to make th ex scale to 0.0 1.0
            x0 = bkeyFrames[1].x / unscaledEnd;
            y0 = bkeyFrames[1].y;
            x1 = bkeyFrames[2].x / unscaledEnd;
            y1 = bkeyFrames[2].y;
            endTime = unscaledEnd / timeScale;
            return true;
        }

        private static string CreateForce_FromSGHjif(JObject perception, float timeScale, string effectName, string targetDirectory)
        {
            Vector2[] bkeyFrames = GetKeyframes(perception, PHap_Transcoding.relPosKey, PHap_Transcoding.ampl_modKey);
            //keyframe x is still in a tiemscale
            if (ExtractLinears(bkeyFrames, timeScale, out float start, out float startForce, out float end, out float endForce, out float endTime)) //2 to 4?
            {
                //Now that we're here, I'm confident we can create the asset.
                string outputPath = System.IO.Path.Combine(PHap_Transcoding.TranscodeOutputFolder_Resources, effectName + ".asset"); //SenseGlove should not be placed in a StreamingAssets, but in a Resources folder
                SG_PremadeForce effect = LoadOrCreate<SG_PremadeForce>(outputPath);

                effect.start = start;
                effect.forceAtStart = startForce;
                effect.end = end;
                effect.forceAtEnd = endForce;
                effect.effectDuration = endTime;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(effect);
#endif
                return outputPath;
            }
            return "";
        }

        private static string CreateStiffness_FromSGHjif(JObject perception, float timeScale, string effectName, string targetDirectory)
        {
            Vector2[] bkeyFrames = GetKeyframes(perception, PHap_Transcoding.relPosKey, PHap_Transcoding.ampl_modKey);
            //keyframe x is still in a tiemscale
            if (ExtractLinears(bkeyFrames, timeScale, out float start, out float startForce, out float end, out float endForce, out float endTime)) //2 to 4?
            {
                //Now that we're here, I'm confident we can create the asset.
                string outputPath = System.IO.Path.Combine(PHap_Transcoding.TranscodeOutputFolder_Resources, effectName + ".asset"); //SenseGlove should not be placed in a StreamingAssets, but in a Resources folder
                SG_PremadeStiffness effect = LoadOrCreate<SG_PremadeStiffness>(outputPath);

                effect.start = start;
                effect.forceAtStart = startForce;
                effect.end = end;
                effect.forceAtEnd = endForce;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(effect);
#endif
                return outputPath;
            }
            return "";
        }




        public static JObject GetReferenceDevice()
        {
            //TODO: Take this from a (live) SenseGlove Device?

            JObject refDevice = new JObject();
            refDevice["id"] = sgId;
            refDevice["name"] = "SenseGlove Nova 2.0";
            return refDevice;
        }


        /// <summary> if Freq > 1, then that one. </summary>
        /// <param name="timestamp_s"></param>
        /// <param name="ampl"></param>
        /// <param name="freq"></param>
        /// <returns></returns>
        public static JObject MakeKeyFrame(float timestamp_s, float ampl, int freq, float timeScale)
        {
            /*
                * KEYFRAME:
                "relative_position": 0,
                "amplitude_modulation": 1.0,
                "frequency_modulation": 30
            */
            JObject keyFrame = new JObject();
            keyFrame["relative_position"] = Mathf.FloorToInt(timestamp_s * timeScale); //converting form s to ms.
            if (ampl >= 0.0f)
                keyFrame["amplitude_modulation"] = ampl; //these are optional parameters
            if (freq > 0)
                keyFrame["frequency_modulation"] = freq; //these are optional parameters
            return keyFrame;
        }

        public static string ToWaveformType(SGCore.WaveformType type)
        {
            switch (type)
            {
                case SGCore.WaveformType.Sine:
                    return "Sine";
                case SGCore.WaveformType.Triangle:
                    return "Triangle";
                case SGCore.WaveformType.Square:
                    return "Square";
                case SGCore.WaveformType.SawUp:
                    return "SawToothUp";
                case SGCore.WaveformType.SawDown:
                    return "SawToothDown";
                default:
                    return "Unknown";
            }
        }

        public static SGCore.WaveformType ToWaveformType(string type)
        {
            if (string.IsNullOrEmpty(type))
                return SGCore.WaveformType.Sine;

            switch (type)
            {
                case "Sine":
                    return SGCore.WaveformType.Sine;
                case "Triangle":
                    return SGCore.WaveformType.Triangle;
                case "Square":
                    return SGCore.WaveformType.Square;
                case "SawToothUp":
                    return SGCore.WaveformType.SawUp;
                case "SawToothDown":
                    return SGCore.WaveformType.SawDown;
                default:
                    return SGCore.WaveformType.Sine;
            }
        }



        /// <summary> Convert the CustomWaveform into a single effect (self contained) WITHOUT a  </summary>
        /// <param name="waveform"></param>
        /// <returns></returns>
        public static JObject GetBaseEffect(SG_PremadeWaveform waveform, float timeScale, out int finalPosition)
        {
            JObject baseEffect = new JObject();
            /*
             * EFFECT:
                "effect_type": "Basis",
                "position": 0,
                "phase": 0.0,
                "base_signal": "Sine",
                "keyframes": []
            */
            baseEffect["effect_type"] = "Basis";
            //We skip position as that will be either 0 or N*effectDuration when we repeat it
            baseEffect["phase"] = 0.0f;
            baseEffect["base_signal"] = ToWaveformType(waveform.waveformType);

            //Throw together Keyframes :3
            JArray keyFrames = new JArray();

            float ts = 0.0f;
            keyFrames.Add(MakeKeyFrame(ts, 0.0f, Mathf.RoundToInt(waveform.startFrequency), timeScale));
            ts += waveform.attackTime;
            keyFrames.Add(MakeKeyFrame(ts, waveform.amplitude, -1, timeScale));
            ts += waveform.sustainTime;
            keyFrames.Add(MakeKeyFrame(ts, waveform.amplitude, -1, timeScale));
            ts += waveform.decayTime;
            keyFrames.Add(MakeKeyFrame(ts, 0.0f, Mathf.RoundToInt(waveform.endFrequency), timeScale));
            ts += waveform.pauseTime;
            keyFrames.Add(MakeKeyFrame(ts, 0.0f, -1, timeScale));

            baseEffect["keyframes"] = keyFrames;

            finalPosition = (int)keyFrames[keyFrames.Count - 1]["relative_position"]; //the final position of my last KeyFrame, so I don't have to access it later...

            return baseEffect;
        }


        public static bool SG_to_HJIFString(SG_PremadeWaveform waveform, out string hjifString)
        {
            //Debug.Log("Encoding " + waveform.name + " into HJIF");
            try
            {
                // Create the root JObject
                JObject rootObject = new JObject();
                /*
                  "version": "CRM3.2",
                  "profile": "Main",
                  "level": 1,
                  "date": "",
                  "description": "",
                  "timescale": 1000,
                  "avatars": [],
                  "perceptions": [...]
                  "syncs": []
                 */
                rootObject["version"] = "CRM3.2";
                rootObject["profile"] = "Main";
                rootObject["level"] = 1;
                rootObject["date"] = System.DateTime.Now.ToShortDateString();
                rootObject["description"] = "";

                float timeScale = 1000.0f;
                rootObject["timescale"] = Mathf.RoundToInt(timeScale);

                JArray avatarsArray = new JArray();
                //TODO: Body mapping
                rootObject["avatars"] = avatarsArray;

                JArray perceptionsArray = new JArray();
                //Since this is a single Vibrotactile effect, we encode it into a single perception:
                JObject vibratonPerception = new JObject();
                {
                    /*
                        "id": 2,
                        "avatar_id": -1,
                        "description": "",
                        "perception_modality": "Vibrotactile",
                        "reference_devices": [], //SENSEGLOVE HERE
                        "effect_library": [],
                        "channels": [ ... ]
                     */
                    vibratonPerception["id"] = 621; //TODO: Find out what this does
                    vibratonPerception["avatar_id"] = -1; //TODO: Find out what this does
                    vibratonPerception["description"] = ""; //TODO: Find out what this does
                    vibratonPerception["perception_modality"] = "Vibrotactile";

                    JArray referenceDevices = new JArray();
                    referenceDevices.Add(GetReferenceDevice()); //so I can go "if ReferenceDevice == Nova (2.0) just interpret waveform directly"
                    vibratonPerception["reference_devices"] = referenceDevices;

                    vibratonPerception["effect_library"] = new JArray(); //TODO: Find out what this does


                    //This vibration has 1 channel with X band(s)
                    JObject myOneChannel = new JObject();
                    /*
                        "id": -1,
                        "description": "",
                        "gain": 1.0,
                        "mixing_weight": 1.0,
                        "body_part_mask": 0,
                        "bands" [ ... ]
                    */
                    myOneChannel["id"] = -1; //don't care
                    myOneChannel["description"] = ""; //don't care
                    myOneChannel["gain"] = 1.0f; //don't care
                    myOneChannel["mixing_weight"] = 1.0f; //don't care
                    myOneChannel["body_part_mask"] = 1; //Maybe Use location? 0 indictaes nowhere.

                    JArray channelBands = new JArray(); //All of the "Bands" in a channel, containing multiple KeyFrames.
                    {
                        //Every band contains an effect;

                        /*
                            "band_type": "Transient",
                            "curve_type": "Unknown",
                            "block_length": 0.0,
                            "lower_frequency_limit": 0,
                            "upper_frequency_limit": 1000,
                            "effects": [ ... ]
                         */
                        JObject myOneBand = new JObject();
                        myOneBand["band_type"] = "Transient"; //Transient bands represent short momentary haptic effects of fixed duration, described with amplitude and frequency parameters. 
                        myOneBand["curve_type"] = "Linear"; //only relevant for curves, but eh
                        myOneBand["block_length"] = 0.0f;
                        myOneBand["lower_frequency_limit"] = Mathf.RoundToInt(SGCore.CustomWaveform.freqRangeMin); //this is mandatory. So might as well add this one?
                        myOneBand["upper_frequency_limit"] = Mathf.RoundToInt(SGCore.CustomWaveform.freqRangeMax); //this is mandatory. So might as well add this one?

                        JArray bandEffects = new JArray();
                        {
                            int baseDuration;
                            JObject baseEffect = GetBaseEffect(waveform, timeScale, out baseDuration);
                            for (int i = 0; i < waveform.RepeatAmount; i++) //now add a copy of that effect for every repeat? TODO: ASK IF THERE"S A BETTER WAY TO DO THIS
                            {
                                JObject eff = (JObject)baseEffect.DeepClone();
                                eff["position"] = i * baseDuration;
                                bandEffects.Add(eff);
                            }
                        }
                        myOneBand["effects"] = bandEffects;
                        channelBands.Add(myOneBand);
                    }
                    myOneChannel["bands"] = channelBands;

                    JArray channels = new JArray();
                    channels.Add(myOneChannel);
                    vibratonPerception["channels"] = channels;
                }
                perceptionsArray.Add(vibratonPerception);
                rootObject["perceptions"] = perceptionsArray;

                rootObject["syncs"] = new JArray(); //empty

                // Output the constructed JSON
                hjifString = rootObject.ToString();
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error trying to generate an HJIF file out of " + waveform.name + ": " + ex.Message, waveform);
            }
            hjifString = "";
            return false;
        }




        /// <summary> if Freq > 1, then that one. </summary>
        /// <param name="timestamp_s"></param>
        /// <param name="ampl"></param>
        /// <param name="freq"></param>
        /// <returns></returns>
        public static JObject MakeForceKeyFrame(float x, float level)
        {
            /*
                * KEYFRAME:
                "relative_position": 0,
                "amplitude_modulation": 1.0,
                "frequency_modulation": 30
            */
            JObject keyFrame = new JObject();
            keyFrame["relative_position"] = x; //converting form s to ms.
            keyFrame["amplitude_modulation"] = level; //these are optional parameters
            return keyFrame;
        }




        public static JObject GetLinearBandEffect(SG_ForceResponse force, float xScale) //effect is in 0.0 .. 1.0 range, scaled by xScale.
        {
            JObject baseEffect = new JObject();
            /*
             * EFFECT:
                "effect_type": "Basis",
                "position": 0,
                "phase": 0.0,
                "base_signal": "Sine",
                "keyframes": []
            */
            baseEffect["effect_type"] = "Basis";
            //We skip position as that will be either 0 or N*effectDuration when we repeat it
            //baseEffect["phase"] = 0.0f;
            //baseEffect["base_signal"] = ToWaveformType(waveform.waveformType);

            //Throw together Keyframes :3
            JArray keyFrames = new JArray();

            keyFrames.Add(MakeForceKeyFrame(0.0f, force.forceAtStart));

            keyFrames.Add(MakeForceKeyFrame(force.start * xScale, force.forceAtStart));
            keyFrames.Add(MakeForceKeyFrame(force.end * xScale, force.forceAtEnd));

            keyFrames.Add(MakeForceKeyFrame(1.0f * xScale, force.forceAtEnd));

            baseEffect["keyframes"] = keyFrames;
            return baseEffect;
        }


        public static bool GenerateForceResponseHJIF(SG_ForceResponse effect, float effectXScale, string effectType, out string hjifString)
        {
            try
            {
                // Create the root JObject
                JObject rootObject = new JObject();
                /*
                  "version": "CRM3.2",
                  "profile": "Main",
                  "level": 1,
                  "date": "",
                  "description": "",
                  "timescale": 1000,
                  "avatars": [],
                  "perceptions": [...]
                  "syncs": []
                 */
                rootObject["version"] = "CRM3.2";
                rootObject["profile"] = "Main";
                rootObject["level"] = 1;
                rootObject["date"] = System.DateTime.Now.ToShortDateString();
                rootObject["description"] = "";

                float timeScale = 1000.0f;
                rootObject["timescale"] = Mathf.RoundToInt(timeScale);

                JArray avatarsArray = new JArray();
                //TODO: Body mapping
                rootObject["avatars"] = avatarsArray;

                JArray perceptionsArray = new JArray();
                //Since this is a single Vibrotactile effect, we encode it into a single perception:
                JObject vibratonPerception = new JObject();
                {
                    /*
                        "id": 2,
                        "avatar_id": -1,
                        "description": "",
                        "perception_modality": "Vibrotactile",
                        "reference_devices": [], //SENSEGLOVE HERE
                        "effect_library": [],
                        "channels": [ ... ]
                     */
                    vibratonPerception["id"] = 621; //TODO: Find out what this does
                    vibratonPerception["avatar_id"] = -1; //TODO: Find out what this does
                    vibratonPerception["description"] = ""; //TODO: Find out what this does
                    vibratonPerception["perception_modality"] = effectType;

                    JArray referenceDevices = new JArray();
                    referenceDevices.Add(GetReferenceDevice()); //so I can go "if ReferenceDevice == Nova (2.0) just interpret waveform directly"
                    vibratonPerception["reference_devices"] = referenceDevices;

                    vibratonPerception["effect_library"] = new JArray(); //TODO: Find out what this does


                    //This vibration has 1 channel with X band(s)
                    JObject myOneChannel = new JObject();
                    /*
                        "id": -1,
                        "description": "",
                        "gain": 1.0,
                        "mixing_weight": 1.0,
                        "body_part_mask": 0,
                        "bands" [ ... ]
                    */
                    myOneChannel["id"] = -1; //don't care
                    myOneChannel["description"] = ""; //don't care
                    myOneChannel["gain"] = 1.0f; //don't care
                    myOneChannel["mixing_weight"] = 1.0f; //don't care
                    myOneChannel["body_part_mask"] = 1; //Maybe Use location? 0 indictaes nowhere.

                    JArray channelBands = new JArray(); //All of the "Bands" in a channel, containing multiple KeyFrames.
                    {
                        //Every band contains an effect;

                        /*
                            "band_type": "Transient",
                            "curve_type": "Unknown",
                            "block_length": 0.0,
                            "lower_frequency_limit": 0,
                            "upper_frequency_limit": 1000,
                            "effects": [ ... ]
                         */
                        JObject myOneBand = new JObject();
                        myOneBand["band_type"] = "Transient"; //Transient bands represent short momentary haptic effects of fixed duration, described with amplitude and frequency parameters. 
                        myOneBand["curve_type"] = "Linear"; //only relevant for curves, but eh
                        myOneBand["block_length"] = 0.0f;
                        //myOneBand["lower_frequency_limit"] = Mathf.RoundToInt(SGCore.CustomWaveform.freqRangeMin); //this is mandatory. So might as well add this one?
                        //myOneBand["upper_frequency_limit"] = Mathf.RoundToInt(SGCore.CustomWaveform.freqRangeMax); //this is mandatory. So might as well add this one?

                        JArray bandEffects = new JArray() { GetLinearBandEffect(effect, effectXScale * timeScale) }; //just has one single band effect(s).
                        myOneBand["effects"] = bandEffects;
                        channelBands.Add(myOneBand);
                    }
                    myOneChannel["bands"] = channelBands;

                    JArray channels = new JArray();
                    channels.Add(myOneChannel);
                    vibratonPerception["channels"] = channels;
                }
                perceptionsArray.Add(vibratonPerception);
                rootObject["perceptions"] = perceptionsArray;

                rootObject["syncs"] = new JArray(); //empty

                // Output the constructed JSON
                hjifString = rootObject.ToString();
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error trying to generate an HJIF file out of " + effect.name + ": " + ex.Message, effect);
            }
            hjifString = "";
            return false;
        }

        public static bool SG_to_HJIFString(SG_PremadeForce effect, out string hjifString)
        {
            //Debug.Log("Encoding " + effect.name + " into HJIF");
            return GenerateForceResponseHJIF(effect, effect.effectDuration, "Force", out hjifString);
        }

        public static bool SG_to_HJIFString(SG_PremadeStiffness effect, out string hjifString)
        {
            //Debug.Log("Encoding " + effect.name + " into HJIF");
            return GenerateForceResponseHJIF(effect, 1.0f, "Stiffness", out hjifString);
        }


    }
}