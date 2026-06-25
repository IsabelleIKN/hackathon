using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
 * Used to access functions / quick actions that are useful for the Presence Haptics API. Such as a 'quick' call to transcode (again) inside the editor.
 * 
 * author:
 * max@senseglove.com
 */


namespace Presence
{
    /// <summary> For quick actions inside the Presence Project </summary>
    public class PHap_QuickActions : MonoBehaviour
    {

        [MenuItem("Presence-Haptics/Find Implementations")]
        static void FindImplementations()
        {
            PHap_Settings.LinkAllDeviceImplmentations();
        }

        [MenuItem("Presence-Haptics/Transcode All BaseEffects")]
        static void TranscodeAll()
        {
            Debug.Log("Transcoding all Phap_BaseEffect ScriptableObjects within this project...");
            var type = typeof(PHap_BaseEffect);
            // Get all asset paths in the Assets folder.
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (string path in allAssetPaths)
            {
                // Load the asset at the path and check if it's of the specified type.
                Object asset = AssetDatabase.LoadAssetAtPath(path, type);

                if (asset is PHap_BaseEffect baseEffect)
                {
                    // Call the desired function on the ScriptableObject instance.
                    PHap_Transcoding.TranscodeAgain(baseEffect); // Replace with your specific function
                   // Debug.Log($"Processed ScriptableObject at path: {path}");
                }
            }
            AssetDatabase.Refresh();
        }
    }
}
