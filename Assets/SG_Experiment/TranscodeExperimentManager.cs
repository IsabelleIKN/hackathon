using Presence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public class TranscodeExperimentManager : MonoBehaviour
{
    [SerializeField]
    private string userID;

    [Header("Original Cube")]
    public PHap_HapticEmitter originalEmitter;
    public PHap_HapticEffect originalsEffect;

    [Header("Left Cube")]
    public PHap_HapticEmitter leftEmitter;
    public PHap_HapticEffect leftsEffect;

    [Header("Right Cube")]
    public PHap_HapticEmitter rightEmitter;
    public PHap_HapticEffect rightsEffect;


    public PHap_BaseEffect[] baseEffects = new PHap_BaseEffect[0]; //all fo the experimental effects. Assinged via inpector because I'm very lazy.
    [SerializeField] private PHap_BaseEffect[] reconvertedEffects = new PHap_BaseEffect[0]; //all fo the experimental effects. Assinged via inpector because I'm very lazy.

    [Header("Selection Criteria")]



    [Header("UI Elements")]
    public Canvas uiCanvas;
    public TMP_Text trialCounterText;
    public TMP_Text countdownText;
    public TMP_Text messageText;
    public bool isCountdownActive = false;

    private float leftPressTime = 0f;
    private float rightPressTime = 0f;
    private float ChoiceTime = 1.5f; // Time to hold the key to select

    private bool rightIsOriginal = false;


    private int trialCount;
    private int currentTrial = 0;
    private string userId;
    private string gender;
    private int age;
    private string effectName;
    private string userChoice;
    private string csvPath;
    private bool isFinished = false;
    private List<string> effectNames = new List<string>();
    private int effectIndex = 0;


    public static readonly string recodedOutputDir = "Assets/SG_Experiment/RecodedMaterials/";

    private int[] experimentOrder = new int[0]; //we'll go through this list in order. It contains the index for both baseEffect and reconvertedEffects.


    public static string buildUserKey = "userIDKey";
    public static readonly string userIDKey = "userIDKey";

    //-------------------------------------------------------------------------------------------------------------------------------
    // Transcoding

    /// <summary> Generic Solution that works for any type of effect; SenseGlove, Actro, IH </summary>
    /// <param name="effect"></param>
    /// <param name="reconvertedEffect"></param>
    /// <returns></returns>
    private static bool ReconvertFile(PHap_BaseEffect effect, out PHap_BaseEffect reconvertedEffect)
    {
        reconvertedEffect = null;
#if UNITY_EDITOR
        //Check for the original file
        if (effect.originalFile == null)
        {
            Debug.LogError(effect.name + " has no original file!");
            return false;
        }

        string originalPath = effect.GetOriginalFilePath();
        if (!System.IO.File.Exists(originalPath))
        {
            Debug.LogError(effect.name + " 's original file no longer exists'!");
            return false;
        }


        List<PHap_DeviceImplementation> impls = PHap_Settings.GetSettings().implementations;
        PHap_DeviceImplementation originalImpl = null;
        foreach (PHap_DeviceImplementation impl in impls)
        {
            if (impl.IsMyCustomFormat(originalPath))
            {
                originalImpl = impl;
                break;
            }
        }
        if (originalImpl == null)
        {
            Debug.LogError(originalPath + " was not recognized by any of the (" + impls.Count + ") Implementations in this project");
            return false;
        }

        //Encode it into an HJIF if it hasn't been already
        string baseName = System.IO.Path.GetFileNameWithoutExtension(originalPath);
        if (!originalImpl.CustomFormat_ToHjif(originalPath, PHap_Transcoding.TranscodeOutputFolder, baseName + "_encoded", out string hjifPath))
        {
            Debug.LogError(originalImpl.GetImplementationID() + " could not convert " + originalPath + " into an HJIF!");
            return false;
        }
        if ( !System.IO.File.Exists(hjifPath) )
        {
            Debug.LogError(effect.name + " did not produce an HJIF File, despite telling us it did?");
            return false;
        }

        ////Decode it into a different file, of the same original type
        //if (!PHap_Transcoding.PreProcessHJIF(hjifPath, out PHap_BaseEffectMetaData metaData))
        //{
        //    Debug.LogError("Could not parse MetatData of " + hjifPath);
        //    return false;
        //}
        //if (!originalImpl.Hjif_ToCustomFormat(hjifPath, metaData, baseName + "_decoded", PHap_Transcoding.TranscodeOutputFolder, 
        //    out string recodedPath, out string additionalData))
        //{
        //    Debug.LogError("Failed to decode " + hjifPath + " back into its original format");
        //    return false;
        //}
        //if (!System.IO.File.Exists(recodedPath))
        //{
        //    Debug.LogError(effect.name + " did not produce an HJIF File, despite telling us it did?");
        //    return false;
        //}
        //Debug.Log(originalPath + " converted from " + System.IO.Path.GetExtension(originalPath) + " to HJIF and back as " + recodedPath);

        //Make a new BaseEffect in a folder and assign the Decoded effect as BaseEffect. This should re-transcode it too.
        if ( !CreateBaseEffect(effect.name + "_recoded", recodedOutputDir, out reconvertedEffect) )
        {
            Debug.LogError("Could not reconvert " + reconvertedEffect + " somehow...");
            return false;
        }

        UnityEngine.Object hjifObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(hjifPath);
        reconvertedEffect.originalFile = hjifObj;
        PHap_Transcoding.TranscodeAgain(reconvertedEffect);
#endif
        return true;
    }


    public static bool CreateBaseEffect(string name, string inDirectory, out PHap_BaseEffect effect)
    {
        effect = null;
#if UNITY_EDITOR
        string outputPath = System.IO.Path.Combine(inDirectory, name + ".asset"); //where it will go!
        if (System.IO.File.Exists(outputPath))
        {
            //the path already exists, so load it as a baseeffect
            effect = UnityEditor.AssetDatabase.LoadAssetAtPath<PHap_BaseEffect>(outputPath);
            if (effect == null)
            {
                Debug.LogError("Found a file at " + outputPath + " but it is not of type PHap_BaseEffect??");
                return false;
            }
            return true;
        }
        if (!System.IO.Directory.Exists(inDirectory))
        {
            System.IO.Directory.CreateDirectory(inDirectory);
        }
        effect = ScriptableObject.CreateInstance<PHap_BaseEffect>();
        UnityEditor.AssetDatabase.CreateAsset(effect, outputPath);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
        return true;
    }


    public void GenerateEffects()
    {
        //TODO:: In Unity Editor only

        //take a BaseEffect, convert it into an HJIF, then back into it's original file format. Use that for a secondary BaseEffect
        reconvertedEffects = new PHap_BaseEffect[baseEffects.Length];
        for (int i=0; i<this.baseEffects.Length; i++)
        {
            ReconvertFile(baseEffects[i], out reconvertedEffects[i]);
        }
    }



    //-------------------------------------------------------------------------------------------------------------------------------
    // Experiment

    void SetupUI()
    {
        uiCanvas.worldCamera = Camera.main;
        trialCounterText.text = "Trial: 1/" + trialCount;
        countdownText.gameObject.SetActive(false);
        messageText.gameObject.SetActive(false);
    }

    private void Init()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string dataDirectory = Path.Combine(desktopPath, "ExperimentalData");

        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        csvPath = Path.Combine(dataDirectory, "ExperimentData_" + userId + ".csv");

        bool fileExists = File.Exists(csvPath);

        if (!fileExists)
        {
            File.WriteAllText(csvPath, "Trial,UserID,Timestamp,EffectName,Choice\n");
            Debug.Log($"Created new experiment data file for user {userId}");
        }
        else
        {
            Debug.Log($"Appending to existing experiment data file for user {userId}");
        }
    }



    void LoadEffectNames()
    {
        this.experimentOrder = new int[this.baseEffects.Length];
        if (baseEffects.Length == 1)
        {
            this.experimentOrder[0] = 0;
        }
        else if (baseEffects.Length > 0)
        {
            List<int> remainingEffects = new List<int>(baseEffects.Length);
            for (int i=0; i<this.baseEffects.Length; i++)
            {
                remainingEffects.Add(i);
            }
            //should now have a list on [0, 1, 2... N]

            int expIndex = 0;
            while (remainingEffects.Count > 1) //because I'm going to be removing them...
            {
                int randmIndex = UnityEngine.Random.Range(0, remainingEffects.Count);
                experimentOrder[expIndex] = remainingEffects[randmIndex];
                expIndex++;
                remainingEffects.RemoveAt(randmIndex);
            }
            //There is now only one left.
            this.experimentOrder[expIndex] = remainingEffects[0];
        }

        effectNames = new List<string>(experimentOrder.Length);
        for (int i=0; i<experimentOrder.Length; i++)
        {
            int effectIndex = experimentOrder[i];
            effectNames.Add( baseEffects[effectIndex].name ); //baseEffect.name
        }
    }



    void StartNextTrial(bool isInitialTrial = false)
    {
        if (!isInitialTrial)
        {
            currentTrial++;
        }
        else
        {
            currentTrial = 0;
        }

        trialCounterText.text = "Trial: " + Mathf.Clamp(currentTrial +1, 0, this.baseEffects.Length) + "/" + trialCount;
        
        effectName = effectNames[effectIndex];
        Debug.Log("Effect Name: " + effectName + "EffectIndex =" + effectIndex);

        ApplyEffectToCubes();
        effectIndex = (effectIndex + 1) % effectNames.Count;
        messageText.text = "New trial started";
        messageText.gameObject.SetActive(true);
        StartCoroutine(HideMessage(1.0f));

    }

    void ApplyEffectToCubes()
    {
        //based on efectIndex
        int baseIndex = experimentOrder[effectIndex]; //the correct BaseEffect / RecodedEffect we'll be testing.
        //Assign the correct effect to either cube.


        PHap_BaseEffect original = baseEffects[baseIndex];
        PHap_BaseEffect recoded = reconvertedEffects[baseIndex];

        //TODO: Stop any active effects - which is why I assign the Emitters
        originalEmitter.StopAllHapticEffects();
        leftEmitter.StopAllHapticEffects();
        rightEmitter.StopAllHapticEffects();

        
        // Then assign new BaseEfects; - which is why I assign the Effects as well.
        originalsEffect.BaseEffect = original;

        float random = UnityEngine.Random.Range(0.0f, 1.0f);
        rightIsOriginal = random > 0.5f;

        rightsEffect.BaseEffect = rightIsOriginal ? original : recoded;
        rightEmitter.gameObject.name = rightIsOriginal ? "Original" : "Encoded";
        
        leftsEffect.BaseEffect  = rightIsOriginal ? recoded : original;
        leftEmitter.gameObject.name  = rightIsOriginal ? "Encoded" : "Original";
        
    }


    public void RecordChoice(string choice)
    {
        if (isCountdownActive)
        {
            return;
        }
        userChoice = choice;
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string data = $"{currentTrial},{userId},{timestamp},{effectName},{userChoice}\n";
        print("Data recorded: " + data);
        File.AppendAllText(csvPath, data);
        StartCoroutine(ShowCountdown());
    }

    IEnumerator ShowCountdown()
    {
        isCountdownActive = true;
        countdownText.gameObject.SetActive(true);
        for (int i = 5; i > 0; i--)
        {
            countdownText.text = "Countdown: " + i;
            yield return new WaitForSeconds(1.0f);
        }
        countdownText.gameObject.SetActive(false);
        isCountdownActive = false;
        if (currentTrial < trialCount - 1) //-1 because we'll do a ++ during StartNextTrial
            StartNextTrial();
        else
            EndExperiment();
    }

    IEnumerator HideMessage(float delay)
    {
        yield return new WaitForSeconds(delay);
        messageText.gameObject.SetActive(false);
    }

    void EndExperiment()
    {
        messageText.text = "Thank you! The Experiment is finished";
        messageText.gameObject.SetActive(true);

        // Add a delay before quitting to ensure message is visible
        StartCoroutine(QuitAfterDelay(3.0f));
    }

    IEnumerator QuitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

#if UNITY_EDITOR
        Debug.Log("Application.Quit() called in Editor - stopping play mode");
        UnityEditor.EditorApplication.isPlaying = false;
#else
           Debug.Log("Application.Quit() called in build - exiting application");
           Application.Quit();
#endif
    }

    //-------------------------------------------------------------------------------------------------------------------------------
    // Cube Selection


    void CheckCubeInput()
    {
        if (this.isCountdownActive)
        {
            leftPressTime = 0f;
            rightPressTime = 0f;
            return;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            leftPressTime += Time.deltaTime;

            if (leftPressTime >= ChoiceTime) // Cube on the left
            {
                Debug.Log("Choice DONE");
                string choice = rightIsOriginal ? "Encoded" : "Original";
                this.RecordChoice(choice);
                Debug.Log("Left Cube chosen as Original (It's actually " + choice + ")");
                //Debug.Log(gameObject.name + " selected as " + choice + " (Left)!");
                leftPressTime = 0f;
            }
        }
        else
        {
            leftPressTime = 0f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            rightPressTime += Time.deltaTime;

            if (rightPressTime >= ChoiceTime) // Cube on the right
            {
                Debug.Log("Choice DONE");
                string choice = rightIsOriginal ? "Original" : "Encoded";
                this.RecordChoice(choice);
                Debug.Log("Right Cube chosen as Original (It's actually " + choice + ")");
                rightPressTime = 0f;
            }
        }
        else
        {
            rightPressTime = 0f;
        }
    }


    //-------------------------------------------------------------------------------------------------------------------------------
    // Monobehaviour


    // Start is called before the first frame update
    void Start()
    {
        //HapticEffectLoader.LoadHapticEffects();
        LoadEffectNames();

#if UNITY_EDITOR
        if (userID.Length > 0)
        {
            userId = userID;
        }
        else
#endif
        {
            //TODO; load from player preds
            userId = PlayerPrefs.GetString(userIDKey, "N-A");
        }
        //always load from PlayerPrefs

        //userId = userID;
        trialCount = baseEffects.Length;
        SetupUI();
        csvPath = Path.Combine(Application.persistentDataPath, "ExperimentalData.csv");
        Init();
        StartNextTrial(true);
    }

    // Update is called once per frame
    void Update()
    {
        CheckCubeInput();   
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(TranscodeExperimentManager))] // This binds the custom inspector to the Effect class
public class TranscodeExperimentManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TranscodeExperimentManager script = (TranscodeExperimentManager)target;
        if (GUILayout.Button("Recode Effects"))
        {
            script.GenerateEffects();
        }
        UnityEditor.EditorGUILayout.Space();
        DrawDefaultInspector();
    }
}
#endif