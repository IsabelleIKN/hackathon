using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/*
 * Editor window for the PHap_Settings, so it can be edited in the editor.
 * 
 * author
 * max@senseglove.com
 */

namespace Presence
{

    public class PHap_SettingsEditor : EditorWindow
    {
        private static PHap_SettingsEditor _window;
        private static SerializedObject _serializedSettings;

        /// <summary> Opening the window </summary>
        [MenuItem("Presence-Haptics/Check Settings")]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            GetWindow();
        }

        private static void GetWindow()
        {
            _window = (PHap_SettingsEditor)EditorWindow.GetWindow(typeof(PHap_SettingsEditor), false, "Presence-Haptics Settings");
            _window.Show();
        }


        private void OnGUI()
        {
            //TODO: Render a proper menu.
            PHap_Settings sett = PHap_Settings.GetSettings();
            if (sett == null)
            {
                EditorGUILayout.HelpBox("Could not draw Settings Box as the file is missing! Please re-import your plugin and try again!", MessageType.Error);
                return;
            }

            if (sett.implementations.Count == 0)
            {
                EditorGUILayout.HelpBox("There are no integrations for Presence Haptics linked inside this project! Transcoding and haptic effects will not happen!", MessageType.Error);
            }
            else
            {
                GUILayout.Label("There are " + sett.implementations.Count + " integrations in this project:", UnityEditor.EditorStyles.boldLabel);

                PHap_HapticModality[] supportedTypes;
                PHap_BodyPart[] supportedLocations;
                string supportTypeString, supportBodyString;
                
                foreach (PHap_DeviceImplementation impl in sett.implementations)
                {
                    supportedTypes = impl.GetSupportedHapticTypes();
                    supportTypeString = PHap_Util.ContainsAllTypes(supportedTypes) ? "[All Haptic Modalities]" : PHap_Util.PrintContents(supportedTypes);

                    supportedLocations = impl.GetSupportedHapticLocations();
                    supportBodyString = PHap_Util.ContainsAllLocations(supportedLocations) ? "[All Body Parts]" : PHap_Util.PrintContents(supportedLocations);

                    UnityEditor.EditorGUILayout.Space();
                    EditorGUILayout.LabelField(impl.GetImplementationID(), supportTypeString);
                    EditorGUILayout.LabelField(" ", supportBodyString);
                }

                //Which means this is our overall support:
                supportedTypes = sett.GetAllSupportedHapticTypes();
                supportTypeString = PHap_Util.ContainsAllTypes(supportedTypes) ? "[All Haptic Modalities]" : PHap_Util.PrintContents(supportedTypes);

                supportedLocations = sett.GetAllSupportedHapticLocations();
                supportBodyString = PHap_Util.ContainsAllLocations(supportedLocations) ? "[All Body Parts]" : PHap_Util.PrintContents(supportedLocations);

                UnityEditor.EditorGUILayout.Space();
                EditorGUILayout.LabelField("Overall Supporting", supportTypeString);
                EditorGUILayout.LabelField(" ", supportBodyString);

            }


            //GUILayout.Label("Presence Haptics Settings", UnityEditor.EditorStyles.boldLabel);



        }




        //    // Draw UI for different field types
        //    private void DrawFieldUI(FieldInfo field, PHap_Settings sett)
        //    {
        //        var fieldValue = field.GetValue(sett);
        //        string label = ObjectNames.NicifyVariableName(field.Name);

        //        EditorGUI.BeginChangeCheck();

        //        // Handle field types
        //        if (typeof(List<PHap_DeviceImplementation>).IsAssignableFrom(field.FieldType))
        //        {
        //            DrawDeviceList((List<PHap_DeviceImplementation>)fieldValue, label);
        //        }
        //        else if (field.FieldType == typeof(int))
        //        {
        //            int newValue = EditorGUILayout.IntField(label, (int)fieldValue);
        //            if (EditorGUI.EndChangeCheck())
        //            {
        //                field.SetValue(sett, newValue);
        //            }
        //        }
        //        else if (field.FieldType == typeof(float))
        //        {
        //            float newValue = EditorGUILayout.FloatField(label, (float)fieldValue);
        //            if (EditorGUI.EndChangeCheck())
        //            {
        //                field.SetValue(sett, newValue);
        //            }
        //        }
        //        else if (field.FieldType == typeof(string))
        //        {
        //            string newValue = EditorGUILayout.TextField(label, (string)fieldValue);
        //            if (EditorGUI.EndChangeCheck())
        //            {
        //                field.SetValue(sett, newValue);
        //            }
        //        }
        //        else if (field.FieldType == typeof(bool))
        //        {
        //            bool newValue = EditorGUILayout.Toggle(label, (bool)fieldValue);
        //            if (EditorGUI.EndChangeCheck())
        //            {
        //                field.SetValue(sett, newValue);
        //            }
        //        }
        //        else if (field.FieldType == typeof(Vector3))
        //        {
        //            Vector3 newValue = EditorGUILayout.Vector3Field(label, (Vector3)fieldValue);
        //            if (EditorGUI.EndChangeCheck())
        //            {
        //                field.SetValue(sett, newValue);
        //            }
        //        }
        //        else if (field.FieldType == typeof(Color))
        //        {
        //            Color newValue = EditorGUILayout.ColorField(label, (Color)fieldValue);
        //            if (EditorGUI.EndChangeCheck())
        //            {
        //                field.SetValue(sett, newValue);
        //            }
        //        }
        //        // Add more types as needed...

        //        // Handle Object references (e.g. textures, prefabs, etc.)
        //        else if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
        //        {
        //            UnityEngine.Object newValue = EditorGUILayout.ObjectField(label, (UnityEngine.Object)fieldValue, field.FieldType, false);
        //            if (EditorGUI.EndChangeCheck())
        //            {
        //                field.SetValue(sett, newValue);
        //            }
        //        }
        //        else
        //        {
        //            EditorGUILayout.LabelField($"{label} type not supported");
        //        }
        //    }


        //    // Draw UI for List<Device>
        //    private void DrawDeviceList(List<PHap_DeviceImplementation> deviceList, string label)
        //    {
        //        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        //        if (deviceList == null)
        //        {
        //            EditorGUILayout.HelpBox("Device list is null.", MessageType.Error);
        //            return;
        //        }

        //        // Display all devices in the list
        //        for (int i = 0; i < deviceList.Count; i++)
        //        {
        //            EditorGUILayout.BeginHorizontal();

        //            // Show each device's name and allow editing
        //            deviceList[i] = (PHap_DeviceImplementation)EditorGUILayout.ObjectField($"Device {i + 1}", deviceList[i], typeof(PHap_DeviceImplementation), false);

        //            // Button to remove the device from the list
        //            if (GUILayout.Button("Remove", GUILayout.Width(70)))
        //            {
        //                deviceList.RemoveAt(i);
        //                i--; // Decrement index because we just removed an element
        //                continue;
        //            }

        //            EditorGUILayout.EndHorizontal();

        //            //// Optionally, display individual fields of the Device (name, ID, etc.)
        //            //if (deviceList[i] != null)
        //            //{
        //            //    EditorGUI.indentLevel++;
        //            //    deviceList[i].deviceName = EditorGUILayout.TextField("Device Name", deviceList[i].deviceName);
        //            //    deviceList[i].deviceID = EditorGUILayout.IntField("Device ID", deviceList[i].deviceID);
        //            //    deviceList[i].isActive = EditorGUILayout.Toggle("Is Active", deviceList[i].isActive);
        //            //    EditorGUI.indentLevel--;
        //            //}
        //        }

        //        EditorGUILayout.Space();

        //        // Add button to add a new Device to the list
        //        if (GUILayout.Button("Add Device"))
        //        {
        //            PHap_DeviceImplementation newDevice = CreateInstance<PHap_DeviceImplementation>(); // Create a new instance of the Device
        //            newDevice.name = "New Device"; // Set a default name
        //            deviceList.Add(newDevice); // Add it to the list

        //            // Optionally, save the new device as an asset
        //            AssetDatabase.CreateAsset(newDevice, "Assets/Resources/NewDevice.asset");
        //            AssetDatabase.SaveAssets();
        //        }
        //    }




    }

}