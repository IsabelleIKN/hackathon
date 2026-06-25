using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Presence
{
    [CustomEditor(typeof(PHap_BaseEffect))] // This binds the custom inspector to the Effect class
    public class Phap_BaseEffectEditor : Editor
    {
        public static readonly bool drawDefaultInspector = false;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (drawDefaultInspector)
            {
                DrawDefaultInspector();
                UnityEditor.EditorGUILayout.Space();
                GUILayout.Label("Custom Editor:", UnityEditor.EditorStyles.boldLabel);
            }


            PHap_BaseEffect effectScript = (PHap_BaseEffect)target;

            //Catching the super rare case where one tries to nest a PHap_BaseEffect as the original file for another PHap_BaseEffect.
            if ( PHap_Util.Editor_IsScribtableObjectType<PHap_BaseEffect>(effectScript.GetOriginalFilePath()) )
            {
                EditorGUILayout.HelpBox("Cannot use a BaseEffect the original file for another BaseEffect! Please use a valid file created by a Device Implementation (e.g. .spn or .haps).", MessageType.Error);


                EditorGUI.BeginChangeCheck();
                // Draw the Object field
                effectScript.originalFile = EditorGUILayout.ObjectField(
                    "Original File",                   // Label
                    effectScript.originalFile,       // Current value
                    typeof(UnityEngine.Object),                // Type filter (can be any Unity Object)
                    false                           // Allow scene objects
                );

                // Optionally, save changes to the object
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);
                }

                // Check if any changes were made
                if (EditorGUI.EndChangeCheck())
                {
                    // Mark the ScriptableObject as dirty to ensure changes are saved
                    EditorUtility.SetDirty(effectScript);

                    //// Trigger OnValidate manually
                    effectScript.OnValidate();
                }

                return;
            }


            PHap_HapticModality effectType = effectScript.GetEffectType();
            EditorGUILayout.LabelField("Effect Type", effectType.ToString());
            EditorGUILayout.LabelField("Estimated Duration (s)", effectScript.GetDuration().ToString("0.00000") );

            EditorGUI.BeginChangeCheck();

            // Draw enum as dropdown
            effectScript.bodyPartFilter = (PHap_BodyPartFilter)EditorGUILayout.EnumPopup("(Optional) Body Part Filter", effectScript.bodyPartFilter);
            if (effectScript.bodyPartFilter != PHap_BodyPartFilter.NoFilter)
            {
                //in both cases, we show the list. They're just... otherwise engaged
                //Can I make a list somehow?
                var flagsProp = serializedObject.FindProperty("bodyPartSelection");
                EditorGUILayout.PropertyField(flagsProp);
                serializedObject.ApplyModifiedProperties();
            }

            // Draw the Object field
            effectScript.originalFile = EditorGUILayout.ObjectField(
                "Original File",                   // Label
                effectScript.originalFile,       // Current value
                typeof(UnityEngine.Object),                // Type filter (can be any Unity Object)
                false                           // Allow scene objects
            );

            string originalFilePath = effectScript.GetOriginalFilePath();
            EditorGUILayout.LabelField("Original File Path (Editor)", originalFilePath);

            if (string.IsNullOrEmpty(originalFilePath))
            {
                EditorGUILayout.HelpBox("Missing original file! Please re-link it via the inspector or risk making this base-effect unuseable!", MessageType.Error);
            }
            else if ( !PHap_Transcoding.InSafeFolder(originalFilePath) )
            {
                EditorGUILayout.HelpBox("The Original file is not located inside a StreamingAssets or Resources folder. This means it cannot be loaded in a Build! Please move the original File to a StreamingAssets Folder (or a subfolder within StreamingAssets) before building...", MessageType.Error);
            }


            string hjifPath = effectScript.GetHjifFilePath();
            if ( !string.IsNullOrEmpty(originalFilePath) && !originalFilePath.EndsWith(".hjif") ) //the original file is NOT empty and not an HJIF file...
            {
                if ( string.IsNullOrEmpty(hjifPath) )
                {
                    EditorGUILayout.HelpBox("Hjif File not (yet) created! Without it, we cannot encode the original file into other formats. Press the Transcode button and pay attention to the console.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.LabelField("HJIF File Path", hjifPath);
                    if ( !PHap_Transcoding.InSafeFolder(hjifPath) )
                        EditorGUILayout.HelpBox("The HJIF file is not located inside a StreamingAssets or Resources folder. This means it cannot be loaded in a Build. If there is an implementation that relies on hjif files, you might run into trouble during a build.", MessageType.Error);
                }
            }

            // Now show any of the transcoded files IF the HJIF File path exists...

            EditorGUILayout.Space();
            PHap_BaseEffect.PHap_ImplementationFileLink[] fileLinks = effectScript.GetAllFileLinks();
            if (fileLinks.Length == 0)
            {
                if (!string.IsNullOrEmpty(hjifPath))
                {
                    GUILayout.Label("Transcoded Files: ", UnityEditor.EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox("This file is not (yet) converted into other formats. This might be because of an error, or because no other implementation supports "
                    + effectType.ToString() + " effects. Use the \"Transcode\" button to try again, and check your Console.", MessageType.Info);
                }
            }
            else
            {
                GUILayout.Label("Transcoded Files: ", UnityEditor.EditorStyles.boldLabel);
                for (int i = 0; i < fileLinks.Length; i++)
                {
                    PHap_DeviceImplementation impl = fileLinks[i].GetLinkedImplementation();
                    string editorLink = fileLinks[i].EditorFileLink;
                    string implName = impl != null ? impl.GetImplementationID() : "MISSING IMPLEMENTATION";
                    string value = editorLink;
                    EditorGUILayout.LabelField(implName, value);

                    if ( string.IsNullOrEmpty(editorLink) )
                    {
                        EditorGUILayout.HelpBox("There is no file link available for " + implName + ". Perhaps something was corrupted.", MessageType.Error);
                    }
                    else if ( !editorLink.Equals(originalFilePath) ) //if it does equal then I'll have already put my warnings below the OriginalFile.
                    {
                        if ( !System.IO.File.Exists(editorLink) )
                        {
                            EditorGUILayout.HelpBox(editorLink + " does not exist! It may have been moved or deleted. Please Transcode this file again to create it anew.", MessageType.Error);
                        }
                        else if (!PHap_Transcoding.InSafeFolder(editorLink)) //todo: if this isn't the original file, we should probably not bee
                        {
                            EditorGUILayout.HelpBox("This file is not located inside the Resources or StreamingAssets folder! " + implName
                                + " Implementation must make sure it ends up in such a folder during Transcoding!", MessageType.Error);
                        }
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if ( !string.IsNullOrEmpty(originalFilePath) )
            {
                if (GUILayout.Button("Transcode"))
                {
                    PHap_Transcoding.TranscodeAgain(effectScript);
                    AssetDatabase.Refresh();
                    EditorUtility.SetDirty(effectScript); //so it gets updated - TODO: Implement this inside PHap_Transcoding, I suppose.
                }
            }

            // Optionally, save changes to the object
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }

            // Check if any changes were made
            if (EditorGUI.EndChangeCheck())
            {
                // Mark the ScriptableObject as dirty to ensure changes are saved
                EditorUtility.SetDirty(effectScript);

                //// Trigger OnValidate manually
                effectScript.OnValidate();
            }
        }
    }

    [CustomPropertyDrawer(typeof(PHap_BodyPartSelection))]
    public class PHap_BodyPartSelectionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Use Enum.ToObject to convert int to enum safely
            PHap_BodyPartSelection currentMask = (PHap_BodyPartSelection)property.intValue;

            EditorGUI.BeginProperty(position, label, property);

            PHap_BodyPartSelection newMask = (PHap_BodyPartSelection)EditorGUI.EnumFlagsField(position, label, currentMask);

            property.intValue = (int)(object)newMask;

            EditorGUI.EndProperty();
        }
    }

}