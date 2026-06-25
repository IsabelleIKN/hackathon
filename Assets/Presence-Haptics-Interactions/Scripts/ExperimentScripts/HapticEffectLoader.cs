//#define IH_PACKAGE_PRESENT

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

#if IH_PACKAGE_PRESENT
using Interhaptics;
using Interhaptics.Core;
#endif
using Presence;

public class HapticEffectLoader : MonoBehaviour
{
    private static Dictionary<PHap_BaseEffect, int> m_loadedMaterials = new Dictionary<PHap_BaseEffect, int>();

#if UNITY_EDITOR
    [MenuItem("Haptics/Load Haptic Effects")]
#endif
    public static void LoadHapticEffects()
    {
        m_loadedMaterials.Clear();

#if IH_PACKAGE_PRESENT

    #if UNITY_EDITOR
        string hapticFolderPath = "Assets/Resources/Materials";
        string fullFolderPath = Path.Combine(Application.dataPath, hapticFolderPath.Replace("Assets/", ""));

        //Debug.Log($"Checking for folder: {fullFolderPath}");
        if (!Directory.Exists(fullFolderPath))
        {
            Debug.LogError($"Haptic effects folder not found at: {hapticFolderPath}. Please create the folder and add .haps files.");
            return;
        }

        //Debug.Log("Refreshing AssetDatabase...");
        AssetDatabase.Refresh();

        Debug.Log($"Searching for .haps files in {fullFolderPath}...");
        string[] hapsFiles = Directory.GetFiles(fullFolderPath, "*.haps", SearchOption.TopDirectoryOnly);
        Debug.Log($"Found {hapsFiles.Length} .haps files: {string.Join(", ", hapsFiles)}");

        if (hapsFiles.Length == 0)
        {
            Debug.LogWarning($"No .haps files found in {hapticFolderPath}. Ensure files have a .haps extension (lowercase).");
            return;
        }

        // List to store all PHap_BaseEffect assets before saving
        List<PHap_BaseEffect> baseEffects = new List<PHap_BaseEffect>();

        // Step 1: Create all PHap_BaseEffect assets in memory
        foreach (string hapsFilePath in hapsFiles)
        {
            //Debug.Log($"Processing file: {hapsFilePath}");
            string relativeHapsPath = "Assets" + hapsFilePath.Replace(Application.dataPath, "").Replace("\\", "/");
            //Debug.Log($"Relative path: {relativeHapsPath}");

            // Load the .haps file as a HapticMaterial (Interhaptics SDK)
            HapticMaterial hapsAsset = AssetDatabase.LoadAssetAtPath<HapticMaterial>(relativeHapsPath);
            if (hapsAsset == null)
            {
                Debug.LogWarning($"Failed to load .haps file at: {relativeHapsPath} as HapticMaterial. Ensure the Interhaptics SDK is set up correctly and the file is imported as a HapticMaterial.");
                continue;
            }
            //Debug.Log($"Successfully loaded HapticMaterial: {hapsAsset.name}");

            // Create a PHap_BaseEffect instance (Presence SDK)
            PHap_BaseEffect effect = ScriptableObject.CreateInstance<PHap_BaseEffect>();
            effect.name = Path.GetFileNameWithoutExtension(hapsFilePath);
            //Debug.Log($"Created PHap_BaseEffect: {effect.name}");

            // Assign the originalFile as the HapticMaterial (only in Editor)
            effect.originalFile = hapsAsset;
            //Debug.Log($"Assigned HapticMaterial to originalFile: {hapsAsset.name}");

            // Set the effect type to Vibrotactile (as shown in the image)
            effect.SetEffectType(PHap_HapticModality.Vibrotactile);
            //Debug.Log($"Set effect type to Vibrotactile");

            // Set a default duration (matching the image)
            //effect.SetDuration(1.0f);
            //Debug.Log($"Set duration to 1.0 seconds");

            // Create a temporary PHap_HapticEffect to use LoadEffect (Presence SDK)
            GameObject tempObject = new GameObject("TempHapticEffect");
            PHap_HapticEffect hapticEffect = tempObject.AddComponent<PHap_HapticEffect>();
            hapticEffect.BaseEffect = effect;

            // Load the effect using Presence SDK logic
            if (!LoadEffect(hapticEffect, relativeHapsPath))
            {
                Debug.LogWarning($"Failed to load effect for: {relativeHapsPath}. Proceeding to save PHap_BaseEffect anyway...");
            }
            else
            {
                //Debug.Log($"Successfully validated effect for: {relativeHapsPath}");
            }

            // Get the HapticMaterial (Interhaptics SDK, loaded via Presence SDK's PHap_Util)
            HapticMaterial IHMat;
            if (!PHap_Util.TryLoadScriptableObject(relativeHapsPath, out IHMat))
            {
                Debug.LogWarning($"Unable to load {relativeHapsPath} as HapticMaterial via PHap_Util. Proceeding to save PHap_BaseEffect anyway...");
            }
            else
            {
                //Debug.Log($"Successfully loaded HapticMaterial via PHap_Util: {IHMat.name}");
            }

            // Get the HapticMaterialID using Interhaptics SDK
            int HmatID = IHMat != null ? HAR.AddHM(IHMat) : -1;
            if (HmatID == -1)
            {
                Debug.LogWarning($"Failed to get HapticMaterialID for {effect.name}. Setting to -1.");
            }
            m_loadedMaterials.Add(effect, HmatID);

            // Add the effect to the list for later saving
            baseEffects.Add(effect);

            // Clean up the temporary object
            DestroyImmediate(tempObject);
        }

        // Step 2: Save all PHap_BaseEffect assets at once
        //Debug.Log($"Saving {baseEffects.Count} PHap_BaseEffect assets...");
        foreach (PHap_BaseEffect effect in baseEffects)
        {
            string effectAssetPath = $"{hapticFolderPath}/{effect.name}.asset";
            //Debug.Log($"Saving PHap_BaseEffect to: {effectAssetPath}");
            AssetDatabase.CreateAsset(effect, effectAssetPath);
        }
        AssetDatabase.SaveAssets();
        //Debug.Log("Finished saving PHap_BaseEffect assets.");

        // Step 3: Transcode all PHap_BaseEffect assets
        //Debug.Log("Starting transcoding of PHap_BaseEffect assets...");
        foreach (PHap_BaseEffect effect in baseEffects)
        {
            //Debug.Log($"Transcoding effect: {effect.name}");
            try
            {
                PHap_Transcoding.TranscodeAgain(effect);
                //Debug.Log($"Successfully transcoded {effect.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to transcode {effect.name}: {e.Message}");
            }
        }
        foreach (PHap_BaseEffect effect in baseEffects)
        {
            //Debug.Log($"Transcoding effect: {effect.name}");

                PHap_Transcoding.TranscodeAgain(effect);
                //Debug.Log($"Successfully transcoded {effect.name}");

        }
        //Debug.Log("Finished transcoding PHap_BaseEffect assets.");

        // Log the loaded pairs
        Debug.Log($"Loaded {m_loadedMaterials.Count} haptic effect pairs:");
        foreach (var pair in m_loadedMaterials)
        {
            Debug.Log($"Haptic Effect: {pair.Key.name}, HapticMaterialID: {pair.Value}, Duration: {pair.Key.GetDuration()}");
        }
    #endif
#endif
    }


    private static bool LoadEffect(PHap_HapticEffect effect, string filePath)
    {
#if IH_PACKAGE_PRESENT
        if (m_loadedMaterials.ContainsKey(effect.BaseEffect))
        {
            Debug.LogError("Material has no key");
            return false;
        }

        if (effect.EffectType != PHap_HapticModality.Vibrotactile)
        {
            Debug.LogError($"Wrong effect type, expecting Vibrotactile but it is {effect.EffectType}");
            return false;
        }

        string validatedFilePath;
        if (!effect.BaseEffect.GetEffectPath(null, out validatedFilePath))
        {
            Debug.LogWarning("Invalid filePath in GetEffectPath");
            return false;
        }

        if (string.IsNullOrEmpty(validatedFilePath))
        {
            Debug.LogError("Invalid filePath! Empty or NULL");
            return false;
        }

#if UNITY_EDITOR
        string directoryPath = Path.GetDirectoryName(validatedFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Debug.LogWarning($"The Directory {directoryPath} does not exist!");
            return false;
        }
#endif

        if (!filePath.ToLower().EndsWith(".haps"))
        {
            Debug.LogError($"{filePath}: Invalid FileType! Expected .haps but got {Path.GetExtension(filePath)}");
            return false;
        }

        HapticMaterial IHmat;
        if (!PHap_Util.TryLoadScriptableObject(filePath, out IHmat))
        {
            Debug.LogError($"Unable to load {filePath} as HapticMaterial!");
            return false;
        }

        //Debug.Log($"Loaded {effect.name} as HapticMaterial in LoadEffect");
        return true;
#else
        Debug.LogError("Interhaptics SDK not present");
        return false;
#endif
    }

    public static Dictionary<PHap_BaseEffect, int> GetHapticEffectPairs()
    {
        return m_loadedMaterials;
    }
}