using UnityEngine;
using System;

public class AIGatekeeper : MonoBehaviour
{
    public string wakeWord = "computer";
    public GameObject uiPanel;

    private bool isAwake = false;
    public float awakeDuration = 10.0f; // Set this to 10 or 15 in Inspector
    private float timer = 0f;

    public Action<string> OnValidatedSpeech;

    public void CheckSpeech(string text)
    {
        string lowerText = text.ToLower().Trim();

        if (!isAwake)
        {
            if (lowerText.Contains(wakeWord.ToLower()))
            {
                WakeUp();
            }
        }
        // --- THIS IS THE 'ELSE' BLOCK ---
        else
        {
            // 1. This sends your speech to the AI
            OnValidatedSpeech?.Invoke(text);

            // 2. This sets the internal state back to 'not awake'
            isAwake = false;

            // DELETE OR COMMENT OUT THIS LINE BELOW:
            // if (uiPanel != null) uiPanel.SetActive(false); 

            // WHY: Removing this allows the UI to stay visible while the AI thinks.
            // The ExampleVoiceChat script will hide it later after the AI finishes speaking.
        }
    }

    void WakeUp()
    {
        isAwake = true;
        timer = 0f;
        if (uiPanel != null) uiPanel.SetActive(true);
        Debug.Log("<color=cyan>Gatekeeper: I'm listening...</color>");
    }

    void Update()
    {
        if (isAwake)
        {
            timer += Time.deltaTime;
            if (timer > awakeDuration)
            {
                isAwake = false;
                if (uiPanel != null) uiPanel.SetActive(false);
                Debug.Log("<color=red>Gatekeeper: Timed out.</color>");
            }
        }
    }
}