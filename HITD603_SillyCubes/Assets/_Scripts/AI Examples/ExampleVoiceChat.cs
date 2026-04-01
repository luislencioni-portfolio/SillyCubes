using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExampleVoiceChat : MonoBehaviour
{
    public SpeechToTextModel stt;
    public LargeLanguageModel llm;
    public TextToSpeechModel tts;

    public Text userTextDisplay;
    public Text agentTextDisplay;
    public float hideDelay = 5f;

    private Coroutine hideRoutine;

    void Start()
    {
        // Note: The STT now talks to Gatekeeper, 
        // and Gatekeeper talks to LLM. 
        // This script just listens for the final results to show on UI.
        llm.ResponseReceivedAction += AgentResponseReceived;
    }

    // Call this from your Gatekeeper's OnValidatedSpeech action
    public void UserRequestReceived(string prompt)
    {
        if (userTextDisplay != null) userTextDisplay.text = "You: " + prompt;
        llm.AskQuestion(prompt);
    }

    private void AgentResponseReceived(string responseText)
    {
        // Add this line to make sure we can see the reply!
        if (stt.gatekeeper.uiPanel != null) stt.gatekeeper.uiPanel.SetActive(true);

        if (agentTextDisplay != null) agentTextDisplay.text = "Agent: " + responseText;

        tts.PlayAudio(responseText);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideTextAfterDelay());
    }

    private IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);

        // Clear text
        if (userTextDisplay != null) userTextDisplay.text = "";
        if (agentTextDisplay != null) agentTextDisplay.text = "";

        // Hide the panel via the Gatekeeper reference
        if (stt != null && stt.gatekeeper != null && stt.gatekeeper.uiPanel != null)
        {
            stt.gatekeeper.uiPanel.SetActive(false);
        }
    }
}