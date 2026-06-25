/*
 * The "Base Effect" represents a pre-defined haptic effect that can be re-used and modulated at runtime.
 * E.g: You can define a 'simple click' for a button press that you can then repeat and/or play at various 'volumes' depending on how hard you're pressing.
 * Transcoded into multiple format(s) so it can be used by various APIs.
 * 
 * 
 * Note: 
 * Even though the HJIF effect can contain multiple effect formats, we'll stick to one Haptic Modality per baseEffect for now to vastly reduce complexity.
 * Especially since we'll likely only take files made by IH / Skinetic / SG, and converting these into HJIF.
 * 
 * Variables that will be hidden later I'll mark with a 'h_' for 'hide'
 * 
 * authors:
 * max@senseglove.com
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Presence
{
    public enum PHap_BodyPartFilter
    {
        /// <summary> The effect can be played on all body parts </summary>
        NoFilter,
        /// <summary> The effect will be applied to all body parts EXCEPT the ones listed below </summary>
        AllowedBodyParts,
        /// <summary> The effect will only be applied to the body parts in the listed below. </summary>
        ExcludingBodyParts
    }



    [CreateAssetMenu(fileName = "BaseEffect", menuName = "Presence/Base Effect", order = 1)] //this will eventually be replaced by OnImport functions
    public class PHap_BaseEffect : ScriptableObject
    {
        //-------------------------------------------------------------------------------------------------------------------------------------------------------
        // Runtime Variables (locked in after build)

        [SerializeField] private PHap_HapticModality h_hapticEffectType = PHap_HapticModality.Unknown;

        [SerializeField] private float h_effectDuration = 0.0f;

        [SerializeField] private string h_editorHjifFilePath = "";

        [SerializeField] private string[] h_serializedLinks = new string[0];

        /// <summary> Created when required from the serialized version(s)? </summary>
        [SerializeField] private PHap_ImplementationFileLink[] h_fileLinks = new PHap_ImplementationFileLink[0]; //TODO: Store this as a Dictionary instead of an Array? --->> dictionnary are not serializable by Unity

        /// <summary> Optional filter to not play this effect on certain body parts </summary>
        [SerializeField] public PHap_BodyPartFilter bodyPartFilter = PHap_BodyPartFilter.NoFilter;

        /// <summary> This list of body parts to either include or exclude. </summary>
        [SerializeField] public PHap_BodyPartSelection bodyPartSelection = PHap_BodyPartSelection.None;

        //-------------------------------------------------------------------------------------------------------------------------------------------------------
        // Editor Variables (used inside the Unity Editor only because that would be faster and easier)

#if UNITY_EDITOR

        /// <summary> The original file from which this BaseEffect was created. Stored as an Object so that we do not lose a reference to it if the file moves. </summary>
        public Object originalFile;

#endif



        //-------------------------------------------------------------------------------------------------------------------------------------------------------
        // BaseEffect Functions (for Device Implementations)


        /// <summary> Returns true if this BaseEffect can be loaded / played. It needs to at least have one Implementation that can play it. </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return this.h_serializedLinks.Length > 0; //have at least one thingy.
        }

        /// <summary> Returns the duration of this effect from its (pre-processed) HJIF file. </summary>
        /// <returns></returns>
        public float GetDuration()
        {
            return h_effectDuration;
        }

        public void SetDuration(float duration_s)
        {
            h_effectDuration = duration_s;
        }

        /// <summary> Returns the Effect type of this particular BaseEfect (Force-Feedback, Vibrotactile, etc) </summary>
        /// <remarks> Even though the HJIF effect can contain multiple effect formats, we'll stick to one per baseEffect for now to vastly reduce complexity. 
        /// Especially since we'll likely only take files made by IH / Skinetic / SG, and converting these into HJIF. </remarks>
        /// <returns></returns>
        public PHap_HapticModality GetEffectType()
        {
            return h_hapticEffectType;
        }

        /// <summary> Sets the effect type. Should only be used by  </summary>
        /// <returns></returns>
        public void SetEffectType(PHap_HapticModality hapticsType)
        {
            h_hapticEffectType = hapticsType;
        }


        public string GetHjifFilePath()
        {
            return h_editorHjifFilePath;
        }

        public void SetHjifFilePath(string hjifFilePath)
        {
            h_editorHjifFilePath = hjifFilePath;
        }




        /// <summary> Returns a filePath (relative to the relevant folder(s) that corresponds to a specific implementation, if one exists. </summary>
        /// <param name="forImplementation"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool GetEffectPath(PHap_DeviceImplementation forImplementation, out string filePath)
        {
            UnpackFileList();
            int listIndex = LinkListIndex(forImplementation);
            if (listIndex > -1)
            {
                filePath = h_fileLinks[listIndex].EditorFileLink;
                return true;
            }
            filePath = "";
            return false;
        }


        /// <summary> Returns a filePath (relative to the relevant folder(s) that corresponds to a specific implementation, if one exists. </summary>
        /// <param name="forImplementation"></param>
        /// <param name="filePath"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        public bool GetEffectPath(PHap_DeviceImplementation forImplementation, out string filePath, out string extraData)
        {
            UnpackFileList();
            int listIndex = LinkListIndex(forImplementation);
            if (listIndex > -1)
            {
                filePath = h_fileLinks[listIndex].EditorFileLink;
                extraData = h_fileLinks[listIndex].ExtraData;
                return true;
            }
            filePath = "";
            extraData = "";
            return false;
        }


        private void UnpackFileList()
        {
           // Debug.Log(this.name + ".UnpackFileList. FileList = " + (this.h_fileLinks != null ? this.h_fileLinks.Length.ToString() : "NULL")
           //     + ", h_serializedLinks.Length = " + h_serializedLinks.Length);
//#if !UNITY_EDITOR 
 //           if (h_fileLinks.Length == h_serializedLinks.Length)
 //               return;
//#endif      //but inside the Unity Editor there's always a chance things have changed :/
            h_fileLinks = new PHap_ImplementationFileLink[this.h_serializedLinks.Length];
            for (int i = 0; i < h_serializedLinks.Length; i++)
            {
                h_fileLinks[i] = PHap_ImplementationFileLink.Deserialize(h_serializedLinks[i]);
            }
            //Debug.Log(this.name + " Unpacked File Links:\n" + I_PrintFileLinks());
        }

        private string I_PrintFileLinks() //internal so as to avoid calling UnpackFileList recursively...
        {
            string res = "[";
            for (int i = 0; i < this.h_fileLinks.Length; i++)
            {
                if (i != 0)
                    res += ", ";
                res += h_fileLinks[i].ImplementationID + " / " + System.IO.Path.GetFileName(h_fileLinks[i].EditorFileLink);
            }
            return res + "]";
        }

        public string PrintFileLinks()
        {
            UnpackFileList();
            return I_PrintFileLinks();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------
        // Transcoding Functions (Core API Related)


        /// <summary> Clears all references except for the original file. </summary>
        public void ClearFileLinks()
        {
            this.h_editorHjifFilePath = "";
            this.h_serializedLinks = new string[0];
        }

        public PHap_ImplementationFileLink[] GetAllFileLinks()
        {
            UnpackFileList();
            return this.h_fileLinks;
        }

        public string GetOriginalFilePath()
        {
#if UNITY_EDITOR
            return PHap_Util.Editor_GetFilePath(originalFile); //extract it from the file link.
#else
            Debug.LogError("TODO: Implement Runtime Original File Retrieval"); //This is slightly more complex since we need a relative path in StreamingAssets / Resources, no?
             return ""; //extract it from the file link.
#endif
        }


        /// <summary> Links this baseEffect to a file generated by a particular Implementation - with the option for additional MetaData. </summary>
        /// <param name="forImplementation"></param>
        /// <param name="filePath"></param>
        /// <param name="metaData"></param>
        public void SetFileLink(PHap_DeviceImplementation forImplementation, string editorFilePath, string additionalData)
        {
            //Debug.Log(this.name + ": " + "SetFileLink(" + forImplementation.GetImplementationID() + ", " + editorFilePath + ", \"" + additionalData + "\");");

            PHap_ImplementationFileLink newLink = new PHap_ImplementationFileLink(forImplementation, editorFilePath, additionalData);

            UnpackFileList();
            int listIndex = LinkListIndex(forImplementation);
            if (listIndex > -1)
            {
                h_serializedLinks[listIndex] = newLink.Serialize();
            }
            else //add it!
            {
                int lastElement = h_serializedLinks.Length; //so I can use it later cause I'm lazy
                string[] newList = new string[h_serializedLinks.Length + 1];
                for (int i = 0; i < h_serializedLinks.Length; i++)
                    newList[i] = h_serializedLinks[i];
                newList[lastElement] = newLink.Serialize();
                h_serializedLinks = newList; //actually store it
            }
        }

        private int LinkListIndex(PHap_DeviceImplementation impl)
        {
            UnpackFileList();
            for (int i=0; i<this.h_fileLinks.Length; i++)
            {
                if (h_fileLinks[i].BelongsTo(impl))
                {
                    return i;
                }
            }
            return -1;
        }


        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Monobehaviour (Auto-Transcode when a new file is added...

#if UNITY_EDITOR

        private Object previousOriginalFile = null;

        public void OnValidate()
        {
            if (previousOriginalFile != this.originalFile)
            {
                previousOriginalFile = originalFile;
                if (this.originalFile != null)
                {
                    //Catch the case where someone sets a PHap_BaseEffect as a Original File for a PHap_BaseEffect. Nesting is not allowed
                    if ( PHap_Util.Editor_IsScribtableObjectType<PHap_BaseEffect>( GetOriginalFilePath() ) )
                    {
                        Debug.LogError("A PHap_BaseEffect cannot be used as the original file for another PHap_BaseEffect. Nesting BaseEffects is not possible. Please add a file made by a Device Implementation instead.");
                        this.ClearFileLinks();
                    }
                    else
                    {
                        //Debug.Log(this.name + ": Original File Changed: Checking Transcoding...", this);
                        PHap_Transcoding.TranscodeAgain(this);
                    }
                }
                else
                {
                    //Debug.Log(this.name + ": Original File Deleted: Clearing Transcoding...", this);
                    this.ClearFileLinks();
                }
            }
        }

#endif


        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Utility file to link files and store them in between sessions.

        /// <summary> Stored in its Serialized form to maintain it among sessions in the editor. </summary>
        [System.Serializable]
        public class PHap_ImplementationFileLink
        {
            //---------------------------------------------------------------------------------------------
            // Member variables

            /// <summary> Implementation that encoded this version of the file. Used to retrieve the proper file format for each implementation. </summary>
            private PHap_DeviceImplementation DeviceImplementation; //I don't want this serialized

            /// <summary> The ImplementationID belonging to the implementation that uses the file in this link. Used to retireve these later in a build. </summary>
            public string ImplementationID;

            /// <summary> Where to fin this file inside the Editor (relative to Assets) </summary>
            public string EditorFileLink;

            /// <summary> Additional data added by the Implementation that Encoded it. version numbers / whatever you'd like. </summary>
            public string ExtraData;


            //---------------------------------------------------------------------------------------------
            // Construcotr

            public PHap_ImplementationFileLink()
            {
                DeviceImplementation = null;
                EditorFileLink = "";
                ExtraData = "";
                ImplementationID = "";
            }

            public PHap_ImplementationFileLink(PHap_DeviceImplementation forImplementation, string editorFileLink, string extraData)
            {
                DeviceImplementation = forImplementation;
                ImplementationID = forImplementation.GetImplementationID();
                EditorFileLink = editorFileLink;
                ExtraData = extraData;
            }

            //---------------------------------------------------------------------------------------------
            // Function(s)

            public void CheckImplementationLink()
            {
                if (this.ImplementationID.Length == 0)
                    return;

                PHap_Settings sett = PHap_Settings.GetSettings();
                foreach (PHap_DeviceImplementation impl in sett.implementations)
                {
                    if (impl.GetImplementationID().Equals(this.ImplementationID))
                    {
                        this.DeviceImplementation = impl;
                        return;
                    }
                }
                //if we get here, it does not or no longer exists!
                Debug.LogError(this.EditorFileLink + " does not or no longer has an implementation with the ID " + ImplementationID + "! You'll probably want to transcode the base-effect again!");
            }

            public bool BelongsTo(PHap_DeviceImplementation impl)
            {
                return impl == this.DeviceImplementation;
            }

            public PHap_DeviceImplementation GetLinkedImplementation()
            {
                return this.DeviceImplementation;
            }

            //---------------------------------------------------------------------------------------------
            // Serialize / Deserialize

            public string Serialize()
            {
                return JsonUtility.ToJson(this);
            }


            public static PHap_ImplementationFileLink Deserialize(string serializedString)
            {
                PHap_ImplementationFileLink deserializedData = JsonUtility.FromJson<PHap_ImplementationFileLink>(serializedString);
                if (deserializedData == null) //somehow something went wrong?
                    return new PHap_ImplementationFileLink();
                deserializedData.CheckImplementationLink();
                return deserializedData;
            }

        }
    }




}