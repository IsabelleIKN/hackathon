using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Contains helper functions and settings for Transcoding functionality, which happens inside the editor.
 * 
 * @author
 * max@senseglove.com
 */

namespace Presence
{
    /// <summary> Class that's responsible for creating new file from HJIF </summary>
    public class PHap_Transcoding
    {

        /// <summary> Where all BaseEffects are created by Default </summary>
        public readonly static string DefaultBaseEffectFolder = "Assets/Presence-Haptics/BaseEffects/";

        /// <summary> Specific output folder for transcoded assets. These are BaseEffects that are NOT in a ScritpableObject format, are automatically converted and are therefore not editable! </summary>
        public static string TranscodeOutputFolder
        {
            get
            {
                return "Assets/StreamingAssets/TranscodingOutput/";
                //return System.IO.Path.Combine(Application.streamingAssetsPath.Replace('\\', '/'), "TranscodingOutput/");
            }
        }

        public static string TranscodeOutputFolder_Resources
        {
            get
            {
                return "Assets/Resources/TranscodingOutput/";
                //return System.IO.Path.Combine(Application.streamingAssetsPath.Replace('\\', '/'), "TranscodingOutput/");
            }
        }

        public static string TranscodeInputFolder
        {
            get
            {
                return "Assets/StreamingAssets/TranscodingInput/";
                //return System.IO.Path.Combine(Application.streamingAssetsPath.Replace('\\', '/'), "TranscodingOutput/");
            }
        }


        /// <summary> Returns true if the specified File is inside a folder that is included in Build(s). </summary>
        /// <returns></returns>
        public static bool InSafeFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            string comp = path.Replace('\\', '/');
            return comp.Contains("/Resources/") || comp.Contains("/StreamingAssets/");
        }


        
            
        /// <summary> Make sure an output folder eists (in the editor) </summary>
        /// <param name="folderName"></param>
        public static void CheckOutputFolder(string folderName)
        {
            if (!System.IO.Directory.Exists(folderName))
            {
                System.IO.Directory.CreateDirectory(folderName);
            }
        }


        



        /// <summary> Utility Function. Retrieves an HJIF string from a filePath so it can be parsed by your devive's implementation inside the Unity Engine </summary>
        /// <returns></returns>
        public static bool GetHjifString(string filePath, out string hjifString)
        {
            hjifString = "";
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("Invalid FilePath!");
                return false;
            }
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError(filePath + " does not exist!");
                return false;
            }
            hjifString = System.IO.File.ReadAllText(filePath); //remove all newline entries as well.

            return hjifString.Length > 0;
        }



        public static string ExtractFirstReferenceDevice(JArray referenceDevices)
        {
            if (referenceDevices == null)
                return "";

            string rd = "";
            foreach (JObject refDevice in referenceDevices)
            {
                if (refDevice["name"] != null)
                {
                    rd = (string)refDevice["name"];
                    if (rd.Length > 0)
                        return rd; //skip all others if this is a valid (non empty) reference device
                }
            }
            return rd;
        }



        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Common Functionality for various assets

        /// <summary> Re-usable accross different formats. Creates a new instance of PHap_BaseEffect at the default location and sets the originalFilePath. But does not do any transcoding. </summary>
        /// <param name="originalPath"></param>
        /// <returns></returns>
        private static PHap_BaseEffect CreateBaseFile(string originalEffectPath)
        {
            throw new System.NotImplementedException();
            //string fileName = System.IO.Path.GetFileNameWithoutExtension(originalEffectPath);
            //// Save the ScriptableObject as an asset
            //string assetPath = System.IO.Path.Combine(PHap_Transcoding.DefaultBaseEffectFolder, fileName + ".asset");
            //if (!System.IO.Directory.Exists(PHap_Transcoding.DefaultBaseEffectFolder))
            //{
            //    System.IO.Directory.CreateDirectory(PHap_Transcoding.DefaultBaseEffectFolder);
            //}
            //else if (System.IO.File.Exists(assetPath)) //if the directory did not exist, then surely the also didn't. But it it did, there's a chance...
            //{
            //    //The file already esists...?
            //    //So then return that one instead?
            //    PHap_BaseEffect loadedEffect = UnityEditor.AssetDatabase.LoadAssetAtPath<PHap_BaseEffect>(assetPath);
            //    if (loadedEffect != null)
            //    {
            //        //This effect already exists... so we'll give you the base back..?
            //        if (loadedEffect.OriginalFilePath == null || loadedEffect.OriginalFilePath.Length == 0)
            //        {
            //            Debug.Log("PHap_BaseEffect " + assetPath + " already exists with no original filename, so we'll load that one instead...");
            //            loadedEffect.OriginalFilePath = string.Copy(originalEffectPath); //deep copy.
            //            return loadedEffect;
            //        }
            //        else if (loadedEffect.OriginalFilePath.Equals(originalEffectPath))
            //        {
            //            Debug.Log("PHap_BaseEffect " + assetPath + " already exists and has the same fileName so we'll load that one instead...");
            //            return loadedEffect;
            //        }
            //        else if (System.IO.Path.GetFileName(loadedEffect.OriginalFilePath).Equals(System.IO.Path.GetFileName(fileName)))
            //        {
            //            Debug.LogError("PHap_BaseEffect " + assetPath + " already exists, and has a different filePath (\""
            //                + loadedEffect.OriginalFilePath + "\"), but the same filename as " + originalEffectPath
            //                + ". Since we can't be sure it's the same file, please remove the old file...");
            //            return null;
            //        }
            //        else
            //        {
            //            Debug.LogError("PHap_BaseEffect " + assetPath + " already exists and does not match the original path " + originalEffectPath + ". Skipping this file until you delete or rename one of them...");
            //            return null;
            //        }
            //    }
            //    else
            //    {
            //        Debug.LogError("A ScriptableObject named " + fileName + " already exists, but it's not of type PHap_BaseEffect! Please make sure to move or delete it, then try again.");
            //        return null;
            //    }
            //}

            ////if we get here, then the original asset does not exist.
            //PHap_BaseEffect baseAsset = ScriptableObject.CreateInstance<PHap_BaseEffect>();
            //UnityEditor.AssetDatabase.CreateAsset(baseAsset, assetPath);
            //UnityEditor.AssetDatabase.SaveAssets();
            //baseAsset.OriginalFilePath = originalEffectPath;

            //Debug.Log("Created a new BaseEffect at " + assetPath);
            //return baseAsset;
        }



        /// <summary> Re-Transcodes an existing base effect. In case files get lost or deleted... </summary>
        /// <param name="effect"></param>
        public static void TranscodeAgain(PHap_BaseEffect effect)
        {
#if UNITY_EDITOR
            Debug.Log("Transcoding " + effect.name + " again!");
            effect.ClearFileLinks();

            string originalpath = effect.GetOriginalFilePath();
            if (string.IsNullOrEmpty(originalpath))
            {
                Debug.LogError("Cannot Transcode " + effect.name + ", originalFile is Empty!");
                return;
            }


            PHap_DeviceImplementation baseImpl;
            string hjifPath;

            //Check if the path actually exist 
            CheckOutputFolder(PHap_Transcoding.TranscodeOutputFolder);

            //Step 1: Converting this to an HJIf File (it it's not already)
            if (originalpath.EndsWith(".hjif"))
            {
                baseImpl = null;
                hjifPath = originalpath;
            }
            //TODO: This else-if has been hacked in, and essentially does what is meant to happed in the AttemptHjifCreation function.
            //Actronika was meant to implement "originalpath.EndsWith(".spn")" inside the "IsMyCustomFormat", and implement a proper "CustomFormat_ToHjif"
            //That HJIF would then be parsed for an effectDuration and effectType - as a way to ensure spn -> hjif is working correctly.
            //That's partially on me for not having Device Implementation Documentation ready sooner. Ideally, we make a proper implementation of this in v2.0.
            //else if (originalpath.EndsWith(".spn"))
            //{

            //    effect.SetEffectType(PHap_HapticModality.Vibrotactile);
            //    effect.SetDuration(1);
            //    foreach (PHap_DeviceImplementation impl in PHap_Settings.GetSettings().implementations)
            //    {
            //        if (impl.IsMyCustomFormat(originalpath))
            //            effect.SetFileLink(impl, originalpath, "");
            //    }
            //    return;
            //}
            else
            {
                bool decoded = AttemptHjifCreation(originalpath, out hjifPath, out baseImpl, out string extraData);
                if (decoded)
                    effect.SetFileLink(baseImpl, originalpath, extraData);
                else
                    return; //no Debug.Log needed as AttemptHJIFCreation will already tell us what went wrong.
            }

            //Scpecial use case for DeviceImplementers that do NOT support HJIF encoding (yet).
            if (hjifPath.Length == 0)
            {
                if (baseImpl == null) //no one could decide this...
                    return;
                PHap_BaseEffectMetaData effectData = baseImpl.FallbackGetEffectParameters(originalpath);
                effect.SetEffectType(effectData.GetFirstValidModality());
                effect.SetDuration(effectData.Duration);
            }
            else
            {

                hjifPath = hjifPath.Replace('\\', '/');
                effect.SetHjifFilePath(hjifPath);

                // Step 2 - Process the HJIF File for some Meta Data
                bool validData = PreProcessHJIF(hjifPath, out PHap_BaseEffectMetaData effectData);
                if (!validData)
                {
                    string cause = baseImpl != null ? baseImpl.GetImplementationID() + " may not be creating the file correctly." : "The original program may not have created a valid file!";
                    Debug.LogError("Could not parse MetaData from HJIF File " + hjifPath + "! " + cause);
                    return;
                }

                PHap_HapticModality hapticsType = effectData.GetFirstValidModality();
                if (hapticsType == PHap_HapticModality.Unknown)
                {   //since most implementations won't be able to parse an Unknown haptics type, we need to end things here. Separate from the processing failaing, as that indicates a problem with Decoding.
                    Debug.LogError(hjifPath + " does not contain a valid Haptic Modality.");
                    return;
                }
                if (effectData.GetModalities().Length > 1)
                {   // this does not break anything
                    Debug.LogWarning(hjifPath + " contains more than one effect type. In this implementation, we only support one effect type per file. Other type(s) will be ignored.");
                }

                effect.SetDuration(effectData.Duration);
                effect.SetEffectType(hapticsType); //let the effect know

                // Step 3 - Convert from HJIF to all other formats (except the original implementation if one existed)
                EncodeIntoOtherFormats(hjifPath, effectData, baseImpl, System.IO.Path.GetFileNameWithoutExtension(originalpath), ref effect);
            }
            UnityEditor.EditorUtility.SetDirty(effect); //tells the editor change(s) have occured.
#else
            Debug.LogError("PHap_Transcoding.TranscodeAgain is called in Build, this is not intended use!");
#endif
        }




        private static bool AttemptHjifCreation(string originalFilePath, out string hjifPath, out PHap_DeviceImplementation performedBy, out string extraData)
        {
            hjifPath = "";
            extraData = "";
            performedBy = null;
            //find me an implementation that is associated with the chosen file...
            PHap_Settings sett = PHap_Settings.GetSettings();

            //Validation
            if (sett.implementations.Count == 0)
            {
                Debug.Log("No Device Implementations found in this project. Checking your Assets for some...");
                PHap_Settings.LinkAllDeviceImplmentations();
            }

            foreach (PHap_DeviceImplementation impl in sett.implementations)
            {
                if (impl.IsMyCustomFormat(originalFilePath)) //is this a fileType that you (can) handle?
                {
                    performedBy = impl;
                    Debug.Log(impl.GetImplementationID() + " can handle " + originalFilePath);
                    extraData = impl.GetExtraDataFromFile(originalFilePath); //ask the implementation to provide us with whever extra data they'd like about the original file.
                    break; //no need to search the others.
                }
            }
            if (performedBy == null) //if we cannot find an implementation that can handle this File Type, then we might as well not.
            {
                Debug.LogError("None of the " + sett.implementations.Count + " implementations in this project can handle the custom file format of " + originalFilePath
                    + ". It will therefore not be Transcoded!");
                return false;
            }
            //if we get here, there is one that exists! Exuberace!

            string baseFileName = System.IO.Path.GetFileNameWithoutExtension(originalFilePath);
            bool hjifd = performedBy.CustomFormat_ToHjif(originalFilePath, PHap_Transcoding.TranscodeOutputFolder, baseFileName, out hjifPath);
            if (!hjifd)
            {
                Debug.Log(performedBy.GetImplementationID() + " was unable to decode " + originalFilePath + " into an HJIF File!" +
                    " Please make sure it actually supports the chosen file format and/or check the implementation for more details.");
                //return false;
            }
            else if (string.IsNullOrEmpty(hjifPath))
            {
                Debug.Log(performedBy.GetImplementationID() + " claims to have decoded " + originalFilePath + " into an HJIF File," +
                    " but the resulting hjifFilePath is empty! Please make sure it actually supports the chosen file format and/or check the implementation for more details.");
                //return false;
            }
            else if (!System.IO.File.Exists(hjifPath))
            {
                Debug.Log(performedBy.GetImplementationID() + " claims to have decoded " + originalFilePath + " into an HJIF File," +
                    " but the resulting hjifFilePath does not exist! Please make sure it actually supports the chosen file format and/or check the implementation for more details.");
                //return false;
            }
            return true;
        }

        /// <summary> Convert an HJIF file into all other format except for the original implementation, and adds their link to the base effect. </summary>
        /// <param name="hjifFilePath"></param>
        /// <param name="originalImplementation"></param>
        /// <param name="effect"></param>
        private static void EncodeIntoOtherFormats(string hjifFilePath, PHap_BaseEffectMetaData metaData, PHap_DeviceImplementation originalImplementation, string effectName, ref PHap_BaseEffect effect)
        {
            CheckOutputFolder(PHap_Transcoding.TranscodeOutputFolder); //make sure this exists. Perhaps redundant. But I'm paranoid.
            PHap_Settings sett = PHap_Settings.GetSettings();

            PHap_HapticModality effectType = metaData.GetFirstValidModality();
            int encodeAttempts = 0, encodeErrors = 0;
            foreach (PHap_DeviceImplementation impl in sett.implementations)
            {
                if (impl.SupportsHapticsType(effectType) && (originalImplementation == null || impl != originalImplementation) ) //No need to convert from HJIF back to the original custom format we got our HJIF from in the first place
                {
                    encodeAttempts++;
                    bool encoded = EncodeAndValidate(hjifFilePath, metaData, impl, effectName, ref effect);
                    if (!encoded) { encodeErrors++; }
                }
            }
            //At the end, if a lot of implementations fail to encode the HJIF File, it may be a problem with the file itself, rather than with the implementations.
            if (encodeAttempts > 0 && encodeErrors == encodeAttempts)
            {
                if (originalImplementation != null) //this was purely an HJIF file
                    Debug.LogWarning("All attempts at encoding " + hjifFilePath + " have failed! Please make sure that " + originalImplementation.GetImplementationID() + " has decoded it properly," +
                        " and that other implementation(s) can decode HJIF files into their own format");
                else
                    Debug.LogWarning("All attempts at encoding " + hjifFilePath + " have failed! Please make sure that " + hjifFilePath + " is using the proper format!");
            }
        }

        /// <summary>  </summary>
        /// <param name="hjifFilePath"></param>
        /// <param name="impl"></param>
        /// <param name="effect"></param>
        private static bool EncodeAndValidate(string hjifFilePath, PHap_BaseEffectMetaData metaData, PHap_DeviceImplementation impl, string effectName, ref PHap_BaseEffect effect)
        {
            bool converted = impl.Hjif_ToCustomFormat(hjifFilePath, metaData, effectName, PHap_Transcoding.TranscodeOutputFolder, out string customFilePath, out string additionalData);
            if (!converted)
            {
                Debug.LogWarning(impl.GetImplementationID() + ": failed to encode " + hjifFilePath + " into its own file format, despite supporting " + metaData.GetFirstValidModality().ToString() + " effects", impl);
                return false;
            }
            if (string.IsNullOrEmpty(customFilePath))
            {
                Debug.LogError(impl.GetImplementationID() + ": claims to have encoded " + hjifFilePath + " into its own file format, but the output path is empty!", impl);
                return false;
            }

            customFilePath = customFilePath.Replace('\\', '/');
            if ( !System.IO.File.Exists(customFilePath) )
            {
                Debug.LogError(impl.GetImplementationID() + ": claims to have encoded " + hjifFilePath + " into its own file format, but the output " + customFilePath + " does not exist!", impl);
                return false;
            }

            if ( !customFilePath.Contains(PHap_Transcoding.TranscodeOutputFolder) ) //it's not been placed inside the output folder? :/
            {
                if ( !PHap_Transcoding.InSafeFolder(customFilePath) ) //and also not in a 'safe folder!'
                {
                    Debug.LogError(impl.GetImplementationID() + " has encoded " + hjifFilePath + " into its own file format, but the new file " 
                        + customFilePath + " is not inside a StreamingAssets or Resources folder! This means it will not be accessible in builds. Make sure the implementation outputs to a folder that is included in build(s).");
                }
            }

            effect.SetFileLink(impl, customFilePath, additionalData);
            return true;
        }



        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  HJIF File Processing

        /// <summary> Key inside HJIF File that contains the "Perceptions" array </summary>
        public const string perceptionsKey = "perceptions";
        /// <summary> Key isnide a "Perception" that contains the "type" (force / vibrotactile etc). </summary>
        public const string modalityKey = "perception_modality";

        /// <summary> Pre-Processed an HJIF File. Returns true if we succesfully pre-processed the file. Can be called by various Transcoding effects. </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static bool PreProcessHJIF(string originalPath, out PHap_BaseEffectMetaData metaData)
        {
            metaData = new PHap_BaseEffectMetaData();
            if (string.IsNullOrEmpty(originalPath))
            {
                Debug.LogError("Invalid filePath! Empty or NULL");
                return false;
            }
            if (!System.IO.File.Exists(originalPath))
            {
                Debug.LogError("The file " + originalPath + " does not exist! Transcoding will therefore not happen!");
                return false;
            }
            else if (!originalPath.ToLower().EndsWith(".hjif"))
            {
                Debug.LogError(originalPath + ": Invalid FileType! Expected .hjif but got " + System.IO.Path.GetExtension(originalPath));
                return false;
            }
            if (PHap_Transcoding.GetHjifString(originalPath, out string hjifString))
            {
                JObject parsedHJIF = JObject.Parse(hjifString);
                if (parsedHJIF == null)
                    return false;

                //grab perceptions
                float timeScale = parsedHJIF[timeScalesKey] != null ? (float)parsedHJIF[timeScalesKey] : 1.0f;

                //Grab all perceptions
                JArray perceptionsArray = (JArray)parsedHJIF[perceptionsKey];
                if (perceptionsArray == null)
                    return false; //no perceptions array found :<

                float maxDuration = 0.0f;
                List<PHap_HapticModality> modalities = new List<PHap_HapticModality>();
                foreach (JObject perception in perceptionsArray)
                {
                    string perc = (string)perception[modalityKey];
                    if (Hjif_ParseModality(perc, out PHap_HapticModality mod) && !modalities.Contains(mod)) //avoids duplicate entries.
                    {
                        modalities.Add(mod);
                    }
                    float maxTimeStamp = GetMaxTimestamp(perception);
                    maxDuration = Mathf.Max(maxDuration, maxTimeStamp); //take the 'furthest' keyFrame
                }
                if (modalities.Count == 0)
                    return false;

                metaData.SetModalities(modalities.ToArray());
                metaData.Duration = maxDuration / timeScale; //TODO: Divide by timescale.
                return true; //there is at least one not-unknown modality in this effect type!
            }
            return false;
        }




        public const string timeScalesKey = "timescale";
        public const string channelsKey = "channels";
        public const string bandsKey = "bands";
        public const string effectsKey = "effects";
        public const string keyFramesKey = "keyframes";
        public const string relPosKey = "relative_position";
        public const string freq_modKey = "frequency_modulation";
        public const string ampl_modKey = "amplitude_modulation";


        public static float GetMaxTimestamp(JObject perception)
        {
            /* Each Perception has channels, which have multiple bands which have multiple effects which have multiple keyframes x.x
             * 
             */
            float res = 0.0f;
            JArray channelsArray = (JArray)perception[channelsKey];
            if (channelsArray == null)
                return 0.0f; //perception contains no channels

            foreach (JObject channel in channelsArray)
            {
                JArray bandsArray = (JArray)channel[bandsKey];
                if (bandsArray == null)
                    continue; //channel contains no bands
                foreach (JObject band in bandsArray)
                {
                    JArray effectsArray = (JArray)band[effectsKey];
                    if (effectsArray == null)
                        continue; //band contains no effects
                    foreach (JObject effect in effectsArray)
                    {
                        JArray keyFramesArray = (JArray)effect[keyFramesKey];
                        if (keyFramesArray == null)
                            continue; //effect contains no keyframes
                        foreach (JObject keyframe in keyFramesArray)
                        {
                            //finally, we can interate through keyframes
                            if (keyframe[relPosKey] == null)
                                continue; //keyframe has no relative position (though it should!_)
                            float ts = (float)keyframe[relPosKey];
                            res = Mathf.Max(ts, res); //we;ll always take the 'latest' keyframe
                        }
                    }
                }
            }
            return res;
        }


        /// <summary> Converts a Modality as noted inside an HJIF string into one we can interpret. </summary>
        /// <param name="modalityString"></param>
        /// <param name="modality"></param>
        /// <returns></returns>
        public static bool Hjif_ParseModality(string modalityString, out PHap_HapticModality modality)
        {
            if (!string.IsNullOrEmpty(modalityString))
            {
                switch (modalityString.ToLower())
                {
                    case "vibrotactile":
                        modality = PHap_HapticModality.Vibrotactile;
                        return true;
                    case "force":
                        modality = PHap_HapticModality.Force;
                        return true;
                    case "pressure":
                        modality = PHap_HapticModality.Pressure;
                        return true;
                    case "stiffness":
                        modality = PHap_HapticModality.Stiffness;
                        return true;
                    //TODO: Add other modalities when I know their naming.
                    default:
                        modality = PHap_HapticModality.Other;
                        return true;
                }
            }
            modality = PHap_HapticModality.Unknown;
            return false;
        }



        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // HJIF CREATION

        private static bool TranscodeHjifFile(PHap_BaseEffect effect)
        {
            // If we get here, the original file name is valid and is of type HJIF. file.

            string originalPath = effect.GetOriginalFilePath();
            string baseFileName = effect.name; //just need to add an extension

            //TODO: Extract and Pre-Process the HJIF FIle


            //PHap_BaseEffectMetaData metaData = effect.EffectMetaData;
            throw new System.NotImplementedException();

            /*
            Debug.Log("Base file(s) ready. Transcoding " + originalPath + " to " + baseFileName + ".something");

            // Converting first into the 'common formats'
            // TODO: Make this configurable and/or related to individual API Implementation(s).


            PHap_Settings settings = PHap_Settings.GetSettings();
            if (settings == null)
            {
                Debug.LogError("A BaseEffect was created, but we could not find PHap_Settings. The effect was created, but no Transcoding has occured.");
            }
            else
            {
                string targetDirectory = PHap_Transcoding.TranscodeOutputFolder;
                CheckOutputFolder(targetDirectory);
                foreach (PHap_DeviceImplementation impl in settings.implementations)
                {
                    try //Try Catch in case someone's transcoding file breaks.
                    {
                        if (impl.Hjif_ToCustomFormat(originalPath, metaData, baseFileName, targetDirectory, out string decodedPath)) //TODO: Allow for decoding into multiple paths?
                        {
                            //Validate output
                            if (string.IsNullOrEmpty(decodedPath))
                            {
                                Debug.LogError(impl.GetImplementationID() + ": Hjif_ToCustomFormat returned true but output path is invalid!", impl);
                                continue;
                            }
                            else if (!System.IO.File.Exists(decodedPath))
                            {
                                Debug.LogError(impl.GetImplementationID() + ": Hjif_ToCustomFormat output does not exist! \"" + decodedPath + "\"", impl);
                                continue;
                            }
                            else if (!System.IO.Path.GetFullPath(targetDirectory).Equals(System.IO.Path.GetFullPath(System.IO.Path.GetDirectoryName(decodedPath))))
                            {
                                Debug.LogWarning(impl.GetImplementationID() + ": Hjif_ToCustomFormat output does not output to the specified target folder! Expected \"" + targetDirectory + "\" but returned \""
                                    + decodedPath + "\"", impl);
                            }
                            else if (!System.IO.Path.GetFileNameWithoutExtension(decodedPath).Equals(baseFileName))
                            {
                                Debug.LogWarning(impl.GetImplementationID() + ": Hjif_ToCustomFormat output filename does not match \"" + System.IO.Path.GetFileNameWithoutExtension(decodedPath) + "\"." +
                                    " This may cause issues down the line.", impl);
                            }
                            effect.AddTranscodedFile(decodedPath);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError(ex.Message);
                    }
                }
            }
            //Finally, mark it as dirty so store the serialized values(!)
            UnityEditor.EditorUtility.SetDirty(effect);
            UnityEditor.AssetDatabase.Refresh(); //refreshes Asset Database so the newly created files get picked up.
            return true;
            */
        }

        /// <summary> Creates a new PHap_BaseEffect from an HJIF File, provided everything inside checks out. </summary>
        /// <param name="originalPath"></param>
        /// <returns></returns>
        public static bool CreateFromHjifFile(string originalPath, out PHap_BaseEffect effect)
        {
            throw new System.NotImplementedException();
            /*
            effect = null;
            //First, we do some pre-processing to see if the parth is valid, and extract some metaData. Gotta make sure we can open and use this file before we start linking it
            bool preprocessed = PreProcessHJIF(originalPath, out PHap_BaseEffectMetaData metaData);
            if (!preprocessed)
                return false;

            // Second, actually try to create the file. Maybe it already exists? Maybe 
            effect = CreateBaseFile(originalPath);
            if (effect == null)
                return false; //somehow, a baseFile could not be created?

            //Let's assign the metaData first. We may even need this for support.
            effect.EffectMetaData = metaData;
            bool res = TranscodeHjifFile(effect); //do the (re)transcoding

            return res;
            */
        }


        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  CUSTOM FILE -> HJIF -> OTHER FORMATS CREATION

        private static bool TranscodeCustomFormat(PHap_BaseEffect effect)
        {
            throw new System.NotImplementedException();
            /*
            // If we get here, it's a valid file (HAPS) file. Inside CreateBaseEffect we've received warnings / logs.
            string baseFileName = effect.name; //just need to add an extension
            string originalPath = effect.OriginalFilePath;
            Debug.Log("Base file(s) ready. Transcoding " + originalPath + " to " + baseFileName + " HJIF");

            //Transcoding into the HJIF format;
            string hjifPath = "";//PHap_Transcoding.Haps_to_Hjif(originalPath, baseFileName);
            PHap_DeviceImplementation decoder = null;

            PHap_Settings settings = PHap_Settings.GetSettings();
            if (settings == null)
            {
                Debug.LogError("A BaseEffect was created, but we could not find PHap_Settings. The effect was created, but no Transcoding has occured.");
                return true;
            }

            string fileType = System.IO.Path.GetExtension(originalPath);
            string targetDirectory = PHap_Transcoding.TranscodeOutputFolder;
            CheckOutputFolder(targetDirectory);
            foreach (PHap_DeviceImplementation impl in settings.implementations)
            {
                try //Try Catch in case someone's transcoding file breaks.
                {
                    if (impl.IsMyCustomFormat(fileType))
                    {
                        //we have found one!
                        if (impl.CustomFormat_ToHjif(originalPath, targetDirectory, baseFileName, out hjifPath))
                        {
                            //Validate output
                            if (string.IsNullOrEmpty(hjifPath))
                            {
                                Debug.LogError(impl.GetImplementationID() + ": Hjif_ToCustomFormat returned true but output path is invalid!", impl);
                                continue;
                            }
                            else if (!System.IO.File.Exists(hjifPath))
                            {
                                Debug.LogError(impl.GetImplementationID() + ": Hjif_ToCustomFormat output does not exist! \"" + hjifPath + "\"", impl);
                                continue;
                            }
                            else if (!System.IO.Path.GetFullPath(targetDirectory).Equals(System.IO.Path.GetFullPath(System.IO.Path.GetDirectoryName(hjifPath))))
                            {
                                Debug.LogWarning(impl.GetImplementationID() + ": Hjif_ToCustomFormat output does not output to the specified target folder! Expected \"" + targetDirectory + "\" but returned \""
                                    + hjifPath + "\"", impl);
                            }
                            else if (!System.IO.Path.GetFileNameWithoutExtension(hjifPath).Equals(baseFileName))
                            {
                                Debug.LogWarning(impl.GetImplementationID() + ": Hjif_ToCustomFormat output filename does not match \"" + System.IO.Path.GetFileNameWithoutExtension(hjifPath) + "\"." +
                                    " This may cause issues down the line.", impl);
                            }
                            decoder = impl;
                            UnityEditor.AssetDatabase.Refresh();
                            break;
                        }
                        else
                        {
                            Debug.LogError(impl.name + " uses " + fileType + " as a custom format but failed to Encode it to HJIF!", impl);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }

            if (hjifPath.Length == 0)
            {
                Debug.LogError("There are no implementations that could decode " + originalPath + " into an HJIF File! So while the BaseFile does exist, there is no way for us to transcode into other formats!");
                return false;
            }

            //Now we do some pre-processing over here, to see if the parth is valid, and extract some metaData. Gotta make sure we can open and use this file before we start linking it
            bool preprocessed = PreProcessHJIF(hjifPath, out PHap_BaseEffectMetaData metaData);
            if (!preprocessed)
            {
                Debug.LogError("Unable to pre-process HJIF File. Ensure that the CustomFormat_ToHjif function of " + decoder.name + " is implemented correctly! No transcoding will happen until it does.", decoder);
                return true;
            }

            effect.EffectMetaData = metaData;
            effect.AddTranscodedFile(hjifPath); //lets BaseEffect know where to find this material

            foreach (PHap_DeviceImplementation impl in settings.implementations)
            {
                try //Try Catch in case someone's transcoding file breaks.
                {
                    if (impl == decoder)
                        continue; //in this was this was the original file, so no need to code back into that one.

                    if (impl.Hjif_ToCustomFormat(originalPath, metaData, baseFileName, targetDirectory, out string decodedPath)) //TODO: Allow for decoding into multiple paths?
                    {
                        //Validate output
                        if (string.IsNullOrEmpty(decodedPath))
                        {
                            Debug.LogError(impl.GetImplementationID() + ": Hjif_ToCustomFormat returned true but output path is invalid!", impl);
                            continue;
                        }
                        else if (!System.IO.File.Exists(decodedPath))
                        {
                            Debug.LogError(impl.GetImplementationID() + ": Hjif_ToCustomFormat output does not exist! \"" + decodedPath + "\"", impl);
                            continue;
                        }
                        else if (!System.IO.Path.GetFullPath(targetDirectory).Equals(System.IO.Path.GetFullPath(System.IO.Path.GetDirectoryName(decodedPath))))
                        {
                            Debug.LogWarning(impl.GetImplementationID() + ": Hjif_ToCustomFormat output does not output to the specified target folder! Expected \"" + targetDirectory + "\" but returned \""
                                + decodedPath + "\"", impl);
                        }
                        else if (!System.IO.Path.GetFileNameWithoutExtension(decodedPath).Equals(baseFileName))
                        {
                            Debug.LogWarning(impl.GetImplementationID() + ": Hjif_ToCustomFormat output filename does not match \"" + System.IO.Path.GetFileNameWithoutExtension(decodedPath) + "\"." +
                                " This may cause issues down the line.", impl);
                        }
                        effect.AddTranscodedFile(decodedPath);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }

            //Finally, mark it as dirty so store the serialized values(!)
            UnityEditor.EditorUtility.SetDirty(effect);
            UnityEditor.AssetDatabase.Refresh(); //refreshes Asset Database so the newly created files get picked up.
            return true;
            */
        }



        public static bool CreateFromCustomFormat(string originalPath, out PHap_BaseEffect effect)
        {
            throw new System.NotImplementedException();
            /*
            Debug.Log("PHap_Transcoding.CreateFromCustomFormat(" + originalPath + ", out effect)");
            effect = null;

            //Preposcess the original file. Does it exist etc etc?
            if (string.IsNullOrEmpty(originalPath))
                return false;
            if (!System.IO.File.Exists(originalPath))
                return false;

            // Second, actually try to create the file. Maybe it already exists? Maybe 
            effect = CreateBaseFile(originalPath);
            if (effect == null)
                return false; //somehow, a baseFile could not be created?

            return TranscodeCustomFormat(effect);
            */
        }


    }



    /// <summary> Contains "Meta Data" of a Base-Effect, usually interpreted from its HJIF file format. This informs us as to what type of haptics are contained within the file, for instance. </summary>
    public class PHap_BaseEffectMetaData
    {
        /// <summary> The Type(s) of haptic modalities / perceptions this effect contains. Used to (quickly) filter out effects in a specific device implementation. </summary>
        /// <remarks> It's an Array because HJIF files can contain multiple 'tracks', though most likely, in the course of Presence, there will be only one type of effect inside these files. </remarks>
        private PHap_HapticModality[] hapticModalities;

        public float Duration
        {
            get; set;
        }


        public PHap_BaseEffectMetaData()
        {
            hapticModalities = new PHap_HapticModality[0];
            Duration = 0.0f;
        }



        /// <summary> Set a (copy of) the an array as this Metadata's modalitites. </summary>
        /// <param name="modalities"></param>
        public void SetModalities(PHap_HapticModality[] modalities)
        {
            //Deep copy becasue I'm paranoid.
            hapticModalities = new PHap_HapticModality[modalities.Length];
            for (int i = 0; i < modalities.Length; i++)
            {
                hapticModalities[i] = modalities[i];
            }
        }

        public void SetModality(PHap_HapticModality modality)
        {
            hapticModalities = new PHap_HapticModality[1] { modality };
        }

        public PHap_HapticModality[] GetModalities()
        {
            return this.hapticModalities;
        }


        /// <summary> Retrieve the first (valid) haptic modality from our list... </summary>
        /// <returns></returns>
        public PHap_HapticModality GetFirstValidModality()
        {
            for (int i = 0; i < hapticModalities.Length; i++)
            {
                if (hapticModalities[i] != PHap_HapticModality.Unknown)
                    return hapticModalities[i];
            }
            return PHap_HapticModality.Unknown;
        }

        /// <summary> Returns true if this particular BaseEffect has one track of a specific Haptic Modality (type). </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsHapticsType(PHap_HapticModality type)
        {
            foreach (PHap_HapticModality mod in this.hapticModalities) // Since there's apparently no .Contains() function in Arrays here?
            {
                if (mod == type)
                    return true;
            }
            return false;
        }
    }

}