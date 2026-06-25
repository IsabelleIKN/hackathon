using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq; // Add this at the top with the other using statements
using Presence;
using Skinetic;

#if IH_PACKAGE_PRESENT
using Interhaptics;
using Interhaptics.Core;
using Presence;
#endif

public class ExperimentManager : MonoBehaviour
{
    public GameObject baseCube;
    public GameObject encodedCube;
    public GameObject originalCube;
    public Canvas uiCanvas;
    public TMP_Text trialCounterText;
    public TMP_Text countdownText;
    public TMP_Text messageText;
    public bool isCountdownActive = false;
    public List<PHap_BaseEffect> P_effects = new List<PHap_BaseEffect>(); // Made public to access from the inspector

    [SerializeField]
    private string userID;

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
    private List<int> IH_IDs = new List<int>();
    private int effectIndex = 0;
    private Dictionary<string, PHap_BaseEffect> effectDictionary = new Dictionary<string, PHap_BaseEffect>();
    private Dictionary<string, PHap_HapticEmitter> emitterDictionary = new Dictionary<string, PHap_HapticEmitter>();


    void Start()
    {
        HapticEffectLoader.LoadHapticEffects();
        LoadEffectNames();
        userId = userID;
        trialCount = effectNames.Count;
        SetupUI();
        csvPath = Path.Combine(Application.persistentDataPath, "ExperimentalData.csv");
        Init();
        SetupHapticEmitters();
        StartNextTrial(true);
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
        // Get the loaded effects from HapticEffectLoader
        Dictionary<PHap_BaseEffect, int> effectPairs = HapticEffectLoader.GetHapticEffectPairs();
        
        // Create a temporary list to hold effects before shuffling
        List<(string name, PHap_BaseEffect effect, int id)> effectsToShuffle = new List<(string, PHap_BaseEffect, int)>();
        
        foreach (var pair in effectPairs)
        {
            string baseName = pair.Key.name.Replace("_Effect", "");
            Debug.Log("Found Effect: " + baseName);
            
            // Add to our temporary collection - only add unique effects
            if (!effectsToShuffle.Any(e => e.name == baseName))
            {
                effectsToShuffle.Add((baseName, pair.Key, pair.Value));
            }
        }
        
        // Randomize the order using Fisher-Yates shuffle
        System.Random random = new System.Random();
        int n = effectsToShuffle.Count;
        
        // Perform Fisher-Yates shuffle
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            var temp = effectsToShuffle[k];
            effectsToShuffle[k] = effectsToShuffle[n];
            effectsToShuffle[n] = temp;
        }
        
        Debug.Log($"Randomized {effectsToShuffle.Count} effects for the experiment");
        
        // Now populate our lists with the shuffled effects
        foreach (var effect in effectsToShuffle)
        {
            P_effects.Add(effect.effect);
            IH_IDs.Add(effect.id);
            effectDictionary.Add(effect.name, effect.effect);
            effectNames.Add(effect.name);
            
            Debug.Log($"Added randomized effect: {effect.name} at position {effectNames.Count-1}");
        }
    }

    void SetupHapticEmitters()
    {
        // Create a PHap_HapticEmitter for each PHap_BaseEffect on the encodedCube
        foreach (var effectPair in effectDictionary)
        {
            string effectName = effectPair.Key;
            PHap_BaseEffect effect = effectPair.Value;

            // Add a new PHap_HapticEmitter component
            var emitter = encodedCube.AddComponent<PHap_HapticEmitter>();
            var haptic = encodedCube.AddComponent<PHap_HapticEffect>();
            haptic.BaseEffect = effect;
            emitter.SetHapticEffects(new PHap_HapticEffect[] { haptic });
            emitter.enabled = false; // Disable by default
            emitterDictionary.Add(effectName, emitter);
            Debug.Log($"Created emitter for effect: {effectName}");
        }
    }

    void SetupUI()
    {
        uiCanvas.worldCamera = Camera.main;
        trialCounterText.text = "Trial: 0/" + trialCount;
        countdownText.gameObject.SetActive(false);
        messageText.gameObject.SetActive(false);
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

        trialCounterText.text = "Trial: " + currentTrial + "/" + trialCount;
        effectName = effectNames[effectIndex];
        Debug.Log("Effect Name: " + effectName + "EffectIndex =" + effectIndex);

        ApplyEffectToCubes(effectName);
        effectIndex = (effectIndex + 1) % effectNames.Count;
        SwapCubePositions();
        messageText.text = "New trial started";
        messageText.gameObject.SetActive(true);
        StartCoroutine(HideMessage(1.0f));

    }

    void ApplyEffectToCubes(string effectName)
    {

        // Deactivate all PHap_HapticEmitter components on encodedCube
        foreach (var emitter in emitterDictionary.Values)
        {
            emitter.enabled = false;
        }

        // Activate the PHap_HapticEmitter for the current effect
        if (emitterDictionary.TryGetValue(effectName, out var activeEmitter))
        {
            activeEmitter.enabled = true;
            Debug.Log($"Activated emitter for effect: {effectName}");

            // Stop any currently playing effect
            var haptic = activeEmitter.GetComponent<PHap_HapticEffect>();
            var location = new PHap_EffectLocation(PHap_BodyPart.RightHandPalm); 
            PHap_Core.StopHapticEffect(haptic, location);
            var locationL = new PHap_EffectLocation(PHap_BodyPart.LeftHandPalm); 
            PHap_Core.StopHapticEffect(haptic, locationL);
        }
        else
        {
            Debug.LogError($"No emitter found for effect: {effectName}");
        }
#if IH_PACKAGE_PRESENT
        // Assign the same .haps effect to Base and Original
        baseCube.GetComponent<SpatialHapticSource>().hapticMaterial.name = effectName + ".haps";

        baseCube.GetComponent<SpatialHapticSource>().HapticMaterialId = IH_IDs[effectIndex];

        originalCube.GetComponent<SpatialHapticSource>().HapticMaterialId = IH_IDs[effectIndex];
        originalCube.GetComponent<SpatialHapticSource>().hapticMaterial.name = effectName + ".haps";
#endif
    }

    void SwapCubePositions()
    {
        float by = baseCube.transform.localPosition.y;
        if (UnityEngine.Random.value > 0.5f)
        {
            encodedCube.transform.localPosition = new Vector3(0.25f, by, 0);
            originalCube.transform.localPosition = new Vector3(-0.25f, by, 0);
        }
        else
        {
            encodedCube.transform.localPosition = new Vector3(-0.25f, by, 0);
            originalCube.transform.localPosition = new Vector3(0.25f, by, 0);
        }
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
        if (currentTrial < trialCount)
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
}