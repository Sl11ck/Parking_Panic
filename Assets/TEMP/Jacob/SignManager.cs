using UnityEngine;

public class SignManager : MonoBehaviour
{
    [SerializeField] private int maxMistakes = 5;

    [SerializeField] GameObject car;
    [SerializeField] UI_Script _ui;

    [Header("Testing Mode")]
    [SerializeField] private bool enableTestMode = false;
    [SerializeField][Range(-80f, 80f)] private float testSpeed = 0f;
    [SerializeField][Range(1, 5)] private int testGear = 1;

    [Header("Test Controls")]
    [SerializeField] private bool testAddMistake = false; // Check this to add a mistake
    [SerializeField] private bool testCompleteObjective = false; // Check this to complete objective
    [SerializeField] private bool testResetTest = false; // Check this to reset the test

    private int currentMistakes = 0;
    private bool testEnded = false;

    void Update()
    {
        if (!testEnded)
        {
            _ui.UpdateSpeedometer(GetCarSpeed());
            _ui.UpdateGear(GetCurrentGear());
        }

        HandleTestControls();
    }

    private void HandleTestControls()
    {
        // Test adding mistakes
        if (testAddMistake)
        {
            testAddMistake = false; // Reset the toggle
            RegisterMistake();
            Debug.Log($"Test: Added mistake. Current mistakes: {currentMistakes}/{maxMistakes}");
        }

        // Test completing objective
        if (testCompleteObjective)
        {
            testCompleteObjective = false; // Reset the toggle
            CompleteObjective();
            Debug.Log("Test: Completed objective");
        }

        // Test resetting
        if (testResetTest)
        {
            testResetTest = false; // Reset the toggle
            ResetTest();
            Debug.Log("Test: Reset test");
        }
    }
    
    private int GetCurrentGear()
    {
        // In test mode, use test gear
        if (enableTestMode)
        {
            return testGear;
        }

        return car.GetComponent<GearShifting>().GetGear();
    }

    private float GetCarSpeed()
    {
        if (enableTestMode)
        {
            return Mathf.Abs(testSpeed);
        }

        return car.GetComponent<Rigidbody2D>().linearVelocity.magnitude;
    }

    public void RegisterMistake()
    {
        if (testEnded) return;

        currentMistakes++;
        if (currentMistakes >= maxMistakes)
        {
            _ui.UpdateCheckboard(true);
        }
        else
        {
            _ui.UpdateCheckboard(false);
        }
        
        _ui.AddMistakeMark();

        // Check if player has failed
        if (currentMistakes >= maxMistakes)
        {
            // mark ended and present failed UI
            testEnded = true;
            _ui.ShowFailedUI();
        }
    }

    public void CompleteObjective()
    {
        if (testEnded) return;

        if (currentMistakes < maxMistakes)
        {
            testEnded = true;
            _ui.ShowPassedUI();
            Debug.Log("Test Passed!");
        }
        else
        {
            testEnded = true;
            _ui.ShowFailedUI();
            Debug.Log("Test Failed!");
        }
    }

    public void ResetTest()
    {
        currentMistakes = 0;
        testEnded = false;

        if (_ui != null)
        {
            _ui.ResetTest();
        }
    }
}
