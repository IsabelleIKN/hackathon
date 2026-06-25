using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A scriptableObject that is loaded at runtime (and inside the editor) to link API Implementations and to control various settings.
 * 
 * author
 * max@senseglove.com
 */

namespace Presence
{
    //[CreateAssetMenu(fileName = "PHapSettings", menuName = "Presence/Settings", order = 1)]
    public class PHap_Settings : ScriptableObject
    {
        public List<PHap_DeviceImplementation> implementations = new List<PHap_DeviceImplementation>();

        /// <summary> Container for Settings that _should_ be accessible throughout the API </summary>
        private static PHap_Settings _settings = null;
        private const string defaultSettingsPath = "Assets/Presence-Haptics/Core/Resources/";

        /// <summary>  </summary>
        /// <returns></returns>
        public static PHap_Settings GetSettings()
        {
            if (_settings == null)
            {
                //Let's try to collect if from the Resources folder.
                PHap_Settings localSett = Resources.Load<PHap_Settings>("PHapSettings");
                if (localSett == null) //it does (not yet) exist.
                {
                    Debug.Log("No Settings file detected! Creating one inside \"" + defaultSettingsPath + "\"");
                    if (!System.IO.Directory.Exists(defaultSettingsPath))
                    {
                        System.IO.Directory.CreateDirectory(defaultSettingsPath);
                    }

                    //TODO: Check if this is Valid for a Build(?)
#if UNITY_EDITOR
                    //if we get here, then the original asset does not exist.
                    localSett = ScriptableObject.CreateInstance<PHap_Settings>();
                    UnityEditor.AssetDatabase.CreateAsset(localSett, System.IO.Path.Combine(defaultSettingsPath, "PHapSettings.asset"));
                    UnityEditor.AssetDatabase.SaveAssets();
#endif
                }
                _settings = localSett;
            }
            return _settings;
        }


        /// <summary> Returns a list of the combined Supported Haptic Modalities of every implementation linked to these Settings. </summary>
        /// <returns></returns>
        public PHap_HapticModality[] GetAllSupportedHapticTypes()
        {
            List<PHap_HapticModality> allModalities = new List<PHap_HapticModality>();
            foreach (PHap_DeviceImplementation impl in this.implementations)
            {
                PHap_HapticModality[] implSupports = impl.GetSupportedHapticTypes();
                foreach (PHap_HapticModality supportType in implSupports)
                {
                    if (!allModalities.Contains(supportType)) //add it only once so there's no Duplicate Entries.
                        allModalities.Add(supportType);
                }
                //IF we have em all already, no need to check the others.
                if (PHap_Util.ContainsAllTypes(allModalities))
                {
                    break;
                }
            }
            return allModalities.ToArray();
        }


        /// <summary> Returns a list of the combined Supported Haptic Modalities of every implementation linked to these Settings. </summary>
        /// <returns></returns>
        public PHap_BodyPart[] GetAllSupportedHapticLocations()
        {
            List<PHap_BodyPart> allModalities = new List<PHap_BodyPart>();
            foreach (PHap_DeviceImplementation impl in this.implementations)
            {
                PHap_BodyPart[] implSupports = impl.GetSupportedHapticLocations();
                foreach (PHap_BodyPart supportType in implSupports)
                {
                    if (!allModalities.Contains(supportType)) //add it only once so there's no Duplicate Entries.
                        allModalities.Add(supportType);
                }
                //IF we have em all already, no need to check the others.
                if (PHap_Util.ContainsAllLocations(allModalities))
                {
                    break;
                }
            }
            return allModalities.ToArray();
        }


        /// <summary> Finds and links all PHap_DeviceImplementation(s) in the project and links these to the PHap_Settings. </summary>
        public static void LinkAllDeviceImplmentations()
        {
#if UNITY_EDITOR
            PHap_Settings sett = PHap_Settings.GetSettings();

            //TODO: Automatic Search as opposed to hard coded paths.

            string sgPath = "Assets/Presence-Haptics/SenseGlove/ScriptableObjects/PHap_SenseGlove.asset";
            string ihPath = "Assets/Presence-Haptics/Interhaptics/PHap_Interhaptics.asset";
            string actrPath = "Assets/Presence-Haptics/Actronika/PHap_Actronika.asset";

            string[] loadPaths = new string[] { sgPath, ihPath, actrPath };

            sett.implementations.Clear(); //it's a full on reset.
            for (int i = 0; i < loadPaths.Length; i++)
            {
                if (PHap_Util.TryLoadScriptableObject(loadPaths[i], out PHap_DeviceImplementation impl))
                {
                    if (!sett.implementations.Contains(impl))
                    {
                        sett.implementations.Add(impl);
                        Debug.Log("Added " + impl.name + " to the Implementations.");
                    }
                }
            }
            Debug.Log("The project contains " + sett.implementations.Count + " implementations.");
            UnityEditor.EditorUtility.SetDirty(sett); //tells the editor change(s) have occured so they will be stored between sessions.
#endif
        }
    }
}