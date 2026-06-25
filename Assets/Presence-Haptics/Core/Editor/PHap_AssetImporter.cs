using UnityEngine;
using UnityEditor;

/*
 * Monitors asset importing. If we're importing a (custom) file that we recognize, we transcode it into different formats.
 * TODO: If it's a more generic format like .wav, we propt the user to choose whether to interpret this or not?
 * 
 * @authors
 * max@senseglove.com
 */ 

namespace Presence
{
    public class PHap_AssetImporter : AssetPostprocessor
    {
        // This function will be called when ANY asset is imported
        private void OnPreprocessAsset()
        {
            // Perform actions after the import is complete
            if (assetPath.EndsWith(".hjif"))
            {
                HandleHjifFileImport(assetPath);
            }
        }
         



        // Custom logic for handling the specific file
        private void HandleHjifFileImport(string path)
        {
            //if (!PHap_Transcoding.IsValidFolder(path)) //We don't do it.... yet?
            //{
            //    Debug.LogError(path + " is not in a folder marked as 'Resources'. Which means it might not be included in your build(s)! Please move it in a folder named Resources..." +
            //        " e.g. \"Presence-Haptics/Resources\"");
            //    return;
            //}

            //Debug.Log($"HJIF handling of the file: {path}");
            //PHap_BaseEffect.CreateFromHjifFile(path);
        }

    }
}