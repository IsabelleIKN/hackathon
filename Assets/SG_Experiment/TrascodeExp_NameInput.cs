using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrascodeExp_NameInput : MonoBehaviour
{
    public TMPro.TMP_InputField userIDInput;
    public Button startBtn;


    private void GoToNext()
    { 
        if (userIDInput.text.Length > 0)
        {
            PlayerPrefs.SetString(TranscodeExperimentManager.userIDKey, userIDInput.text);
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
    }


    private void OnEnable()
    {
        startBtn.onClick.AddListener(GoToNext);
    }

    private void OnDisable()
    {
        startBtn.onClick.RemoveListener(GoToNext);
    }

    // Start is called before the first frame update
    void Start()
    {
        userIDInput.text = PlayerPrefs.GetString(TranscodeExperimentManager.userIDKey, "");
    }

}
