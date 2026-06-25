using Presence;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SG_TestTranscoding : MonoBehaviour
{
    public SG_PremadeForce testForce;
    public SG_PremadeStiffness testStiffness;
    public SG_PremadeWaveform testWaveform;

    public PHap_SenseGloveImpl senseGloveImpl;

    public bool randomizeValues = false; //if true, I randomize the values for forces etc
    public const float floatOffset = 0.001f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TestEffects()
    {
        Debug.Log("Testing Encoding / Decoding of SenseGlove Effects");
        TestWaveform();
        TestForce();
        TestStiffness();
    }


    public void TestWaveform()
    {

        //ForEach SenseGlove Effect; create a Phap Base Effect (TODO: make this a generic function)
        //Only requires a FilePath and... that's it. Though maybe it's better to do a BaseEffect in general...?
        if (randomizeValues)
        {
            testWaveform.amplitude = Random.Range(0.1f, 1.0f);
            testWaveform.attackTime = Random.Range(0.0f, 1.0f);
            testWaveform.decayTime = Random.Range(0.0f, 1.0f);
            testWaveform.endFrequency = Mathf.RoundToInt(Random.Range(SGCore.CustomWaveform.freqRangeMin, SGCore.CustomWaveform.freqRangeMax));
            testWaveform.intendedMotor = Presence.VibrationLocation.Handpalm;
            testWaveform.pauseTime = Random.Range(0.0f, 1.0f);
            testWaveform.RepeatAmount = Random.Range(1, 5);
            testWaveform.startFrequency = Mathf.RoundToInt(Random.Range(SGCore.CustomWaveform.freqRangeMin, SGCore.CustomWaveform.freqRangeMax));
            testWaveform.sustainTime = Random.Range(0.0f, 1.0f);
            testWaveform.waveformType = (SGCore.WaveformType)Random.Range((int)SGCore.WaveformType.Sine, (int)SGCore.WaveformType.Noise);
        }
        if (ToHjifAndBack(this.testWaveform, senseGloveImpl, out SG_PremadeWaveform recodedWaveForm))
        {
            bool[] res = new bool[10];
            string report = "";

            report += EvaluateFloat("Amplitude", testWaveform.amplitude, recodedWaveForm.amplitude, out res[0]);
            report += EvaluateFloat("Attack Time", testWaveform.attackTime, recodedWaveForm.attackTime, out res[1]);
            report += EvaluateFloat("Decay Time", testWaveform.decayTime, recodedWaveForm.decayTime, out res[2]);
            report += EvaluateFloat("End Frequency", testWaveform.endFrequency, recodedWaveForm.endFrequency, out res[3]);
            report += EvaluateInt("Motor", (int)testWaveform.intendedMotor, (int)recodedWaveForm.intendedMotor, out res[4]);
            report += EvaluateFloat("Pause Time", testWaveform.pauseTime, recodedWaveForm.pauseTime, out res[5]);
            report += EvaluateInt("RepeatAmount", (int)testWaveform.RepeatAmount, (int)recodedWaveForm.RepeatAmount, out res[6]);
            report += EvaluateFloat("StartFrequency", testWaveform.startFrequency, recodedWaveForm.startFrequency, out res[7]);
            report += EvaluateFloat("SustainTime", testWaveform.sustainTime, recodedWaveForm.sustainTime, out res[8]);
            report += EvaluateInt("Waveform", (int)testWaveform.waveformType, (int)recodedWaveForm.waveformType, out res[9]);

            bool fullTest = true;
            for (int i = 0; i < res.Length; i++)
            {
                if (!res[i])
                {
                    fullTest = false;
                    break;
                }
            }
            Debug.Log("Full Test for WaveForm: " + (fullTest ? "PASS" : "FAIL") + "\n" + report);
        }
    }

    public void TestForce()
    {
        //ForEach SenseGlove Effect; create a Phap Base Effect (TODO: make this a generic function)
        //Only requires a FilePath and... that's it. Though maybe it's better to do a BaseEffect in general...?
        if (randomizeValues)
        {
            float t0 = Random.Range(0.0f, 1.0f);
            float t1 = Random.Range(0.0f, 1.0f);

            testForce.start = Mathf.Min(t0, t1);
            testForce.end = Mathf.Max(t0, t1);
            testForce.effectDuration = Random.Range(0.0f, 1.0f);
            testForce.forceAtStart = Random.Range(0.0f, 1.0f);
            testForce.forceAtEnd = Random.Range(0.0f, 1.0f);
        }
        if (ToHjifAndBack(this.testForce, senseGloveImpl, out SG_PremadeForce recodedForce))
        {
            bool[] res = new bool[5];
            string report = "";

            report += EvaluateFloat("Start",            testForce.start,            recodedForce.start,             out res[0]);
            report += EvaluateFloat("End",              testForce.end,              recodedForce.end,               out res[1]);
            report += EvaluateFloat("EffectDuration",   testForce.effectDuration,   recodedForce.effectDuration,    out res[2]);
            report += EvaluateFloat("ForceAtStart",     testForce.forceAtStart,     recodedForce.forceAtStart,      out res[3]);
            report += EvaluateFloat("ForceAtEnd",       testForce.forceAtEnd,       recodedForce.forceAtEnd,        out res[4]);

            bool fullTest = true;
            for (int i = 0; i < res.Length; i++)
            {
                if (!res[i])
                {
                    fullTest = false;
                    break;
                }
            }
            Debug.Log("Full Test for Force: " + (fullTest ? "PASS" : "FAIL") + "\n" + report);
        }
    }

    public void TestStiffness()
    {
        //ForEach SenseGlove Effect; create a Phap Base Effect (TODO: make this a generic function)
        //Only requires a FilePath and... that's it. Though maybe it's better to do a BaseEffect in general...?
        if (randomizeValues)
        {
            float t0 = Random.Range(0.0f, 1.0f);
            float t1 = Random.Range(0.0f, 1.0f);

            testStiffness.start = Mathf.Min(t0, t1);
            testStiffness.end = Mathf.Max(t0, t1);
            testStiffness.forceAtStart = Random.Range(0.0f, 1.0f);
            testStiffness.forceAtEnd = Random.Range(0.0f, 1.0f);
        }
        if (ToHjifAndBack(this.testStiffness, senseGloveImpl, out SG_PremadeStiffness recodedStiffness))
        {
            bool[] res = new bool[4];
            string report = "";

            report += EvaluateFloat("Start",            testStiffness.start,            recodedStiffness.start,             out res[0]);
            report += EvaluateFloat("End",              testStiffness.end,              recodedStiffness.end,               out res[1]);
            report += EvaluateFloat("ForceAtStart",     testStiffness.forceAtStart,     recodedStiffness.forceAtStart,      out res[2]);
            report += EvaluateFloat("ForceAtEnd",       testStiffness.forceAtEnd,       recodedStiffness.forceAtEnd,        out res[3]);

            bool fullTest = true;
            for (int i = 0; i < res.Length; i++)
            {
                if (!res[i])
                {
                    fullTest = false;
                    break;
                }
            }
            Debug.Log("Full Test for Stiffness: " + (fullTest ? "PASS" : "FAIL") + "\n" + report);
        }
    }


    public static string EvaluateFloat(string testName, float reference, float result, out bool passed)
    {
        passed = Mathf.Abs(reference - result) < floatOffset;
        return "\n" + testName + ": " + reference.ToString("0.00") + " == " + result.ToString("0.00") + " => " + (passed ? "PASS" : "FAIL");
    }

    public static string EvaluateInt(string testName, int reference, int result, out bool passed)
    {
        passed = reference == result;
        return "\n" + testName + ": " + reference.ToString() + " == " + result.ToString() + " => " + (passed ? "PASS" : "FAIL");
    }


    /// <summary> Convert a ScriptableObject effect into an HJIF and then back into the same original effect.  </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="originalObj"></param>
    /// <param name="reconvertedObj"></param>
    /// <returns></returns>
    public static bool ToHjifAndBack<T>(T originalObj, PHap_DeviceImplementation impl, out T reconvertedObj) where T : ScriptableObject
    {
        reconvertedObj = null;
        if (impl == null || originalObj == null)
            return false;
#if UNITY_EDITOR
        string originalPath = AssetDatabase.GetAssetPath(originalObj);
        //Debug.Log("Attempting to recode " + originalPath);
        if ( !impl.IsMyCustomFormat(originalPath) )
        {
            Debug.LogError(originalObj.name + " does not belong to " + impl.GetImplementationID());
            return false;
        }
        string outputDir = System.IO.Path.GetDirectoryName(originalPath);
        string baseFileName = System.IO.Path.GetFileNameWithoutExtension(originalPath);
        string hjifName = baseFileName + "_Encoded";
        string recodeName = baseFileName + "_Decoded";
        //Debug.Log(originalObj.name + " belongs to " + impl.GetImplementationID() + ". Saving to " + originalPath);
        if ( !impl.CustomFormat_ToHjif(originalPath, outputDir, hjifName, out string hjifPath) )
        {
            Debug.LogError("Could not decode " + originalPath + " using " + impl.GetImplementationID());
            return false;
        }
        AssetDatabase.Refresh();

        //it's now converted into an HJIf. Yay. Let's convert it back.
        //Debug.Log("Converted " + originalPath + " into " + hjifPath);
        if ( !PHap_Transcoding.PreProcessHJIF(hjifPath, out PHap_BaseEffectMetaData metaData) )
        {
            Debug.LogError("Could not parse MetatData of " + hjifPath);
            return false;
        }
        if ( !impl.Hjif_ToCustomFormat(hjifPath, metaData, recodeName, outputDir, out string recodedPath, out string additionalData) )
        {
            Debug.LogError("Failed to decode " + hjifPath + " back into its original format");
            return false;
        }

        AssetDatabase.Refresh();
        if ( !PHap_Util.TryLoadScriptableObject(recodedPath, out reconvertedObj) )
        {
            Debug.LogError("Failed to load " + recodedPath + " as a ScriptableObject");
        }
#endif
        return reconvertedObj != null;
    }


}


#if UNITY_EDITOR

[CustomEditor(typeof(SG_TestTranscoding))] // This binds the custom inspector to the Effect class
public class SG_TestTranscodingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UnityEditor.EditorGUILayout.Space();


        SG_TestTranscoding script = (SG_TestTranscoding)target;

        if (GUILayout.Button("Test Decoding"))
        { 
            script.TestEffects();
        }


    }
}

#endif