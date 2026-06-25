using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Presence
{

    [UnityEditor.CustomEditor(typeof(SG_PremadeWaveform))]
    public class SG_PremadeWavefromEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            SG_PremadeWaveform wf = (SG_PremadeWaveform)target;
            DrawDefaultInspector();
            UnityEditor.EditorGUILayout.Space();

            //only when SenseCOm is running?
            if (GUILayout.Button("Test Vibration"))
            {
                if (!SGCore.SenseCom.IsRunning())
                {
                    Debug.Log("Staring up SenseCom, hold on a moment...");
                    SGCore.SenseCom.StartupSenseCom();
                }
                else
                {
                    wf.TestBaseEffect();
                }
            }

            //if (GUILayout.Button("Transcode Me"))
            //{
            //    string editor_originalFilePath = UnityEditor.AssetDatabase.GetAssetPath(wf).Replace('\\', '/');
            //    Debug.Log("Transcode path " + editor_originalFilePath);
            //    bool success = PHap_Transcoding.CreateFromCustomFormat(editor_originalFilePath, out PHap_BaseEffect baseEff);
            //    if (success)
            //    {
            //        Debug.Log("Transoded " + this.name + " into (updated) file(s).");
            //    }
            //    else
            //    {
            //        Debug.LogError("Could not Transcode " + editor_originalFilePath, wf);
            //    }
            //}
        }
    }
}