using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * Monitors wwhen assets are moved into a (proper) folder.
 * TODO: If it's a more generic format like .wav, we propt the user to choose whether to interpret this or not?
 * 
 * @authors
 * max@senseglove.com
 */

namespace Presence
{
    /// <summary>  </summary>
    public class PHap_AssetMoveCheck : UnityEditor.AssetModificationProcessor
    {

        //// This method is called when an asset is about to be moved or renamed
        //public static AssetMoveResult OnWillMoveAsset(string oldPath, string newPath)
        //{
        //    if (PHap_Transcoding.IsSupportedFileType(oldPath))
        //    {
        //        Debug.Log($"Asset is being moved from: {oldPath} to {newPath}");

        //        //TODO: Check if this is a relevant file that did not (yet) have its contents Transcoded.

        //    }
        //    return AssetMoveResult.DidNotMove; //Returning DidNotMove means this script did not move the asset, so Unity can do it instead.
        //}


    }
}