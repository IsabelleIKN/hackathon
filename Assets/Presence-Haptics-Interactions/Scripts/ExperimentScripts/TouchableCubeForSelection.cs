using UnityEngine;

public class TouchableCubeForSelection : MonoBehaviour
{
    private float leftPressTime = 0f;
    private float rightPressTime = 0f;
    private float ChoiceTime = 1.5f; // Time to hold the key to select
    private ExperimentManager experimentManager;

    [SerializeField]
    private ExperimentManager assignedExperimentManager; 
    

    void Start()
    {
        experimentManager = assignedExperimentManager != null ? assignedExperimentManager : FindObjectOfType<ExperimentManager>();
        if (experimentManager == null)
            Debug.LogError("ExperimentManager not found in scene! Assign it in the Inspector or add it to a GameObject.");
    }


    void Update()
    {
        if (experimentManager != null && experimentManager.isCountdownActive)
        {
            leftPressTime = 0f;
            rightPressTime = 0f;
            return;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            leftPressTime += Time.deltaTime;

            if (leftPressTime >= ChoiceTime && transform.localPosition.x < 0) // Cube on the left
            {
                Debug.Log("Choice DONE");
                string choice = gameObject.name == "Encoded" ? "Encoded" : "Original";
                if (experimentManager != null)
                {
                    experimentManager.RecordChoice(choice);
                    Debug.Log(gameObject.name + " selected as " + choice + " (Left)!");
                }
                else
                {
                    Debug.LogError("ExperimentManager is null when trying to record choice!");
                }
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

            if (rightPressTime >= ChoiceTime && transform.localPosition.x > 0) // Cube on the right
            {
                Debug.Log("Choice DONE");
                string choice = gameObject.name == "Encoded" ? "Encoded" : "Original";
                if (experimentManager != null)
                {
                    experimentManager.RecordChoice(choice);
                    Debug.Log(gameObject.name + " selected as " + choice + " (Right)!");
                }
                else
                {
                    Debug.LogError("ExperimentManager is null when trying to record choice!");
                }
                rightPressTime = 0f;
            }
        }
        else
        {
            rightPressTime = 0f;
        }
    }
}