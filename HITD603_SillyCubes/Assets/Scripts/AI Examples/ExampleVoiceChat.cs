using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ExampleVoiceChat : MonoBehaviour
{
    public SpeechToTextModel stt;
    public LargeLanguageModel llm;
    public TextToSpeechModel tts;
    public bool useAgentMemory = false;

    [Header("UI Display Settings")]
    public Text userTextDisplay;
    public Text agentTextDisplay;
    public float hideDelay = 5f;
    public OrbController orb;

    private string history;
    private Coroutine hideRoutine;

    void Start()
    {
        stt.ResponseReceivedAction += UserRequestReceived;
        llm.ResponseReceivedAction += AgentResponseReceived;
    }

    private void UserRequestReceived(string prompt)
    {
        Debug.Log("User request: " + prompt);

        if (userTextDisplay != null) userTextDisplay.text = "You: " + prompt;

        history += "User: " + prompt + "\n";
        history += "Agent: ";

        if (useAgentMemory)
        {
            llm.AskQuestion(history);
        }
        else
        {
            llm.AskQuestion(prompt);
        }

        // REMOVED: orb.SetStatus("Thinking") is what was causing the error.
        // In a 2-stage setup, the orb stays idle while thinking.
    }

    private void AgentResponseReceived(string responseText)
    {
        Debug.Log("Agent response: " + responseText);

        if (agentTextDisplay != null) agentTextDisplay.text = "Agent: " + responseText;

        history += responseText + "\n";

        // Tell the orb to start the speaking color/size
        if (orb != null) orb.SetSpeaking(true);

        tts.PlayAudio(responseText, PlaybackEnded, PlaybackStarted);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideTextAfterDelay());
    }

    private IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        if (userTextDisplay != null) userTextDisplay.text = "";
        if (agentTextDisplay != null) agentTextDisplay.text = "";
    }

    private void PlaybackStarted(string response) { }

    private void PlaybackEnded()
    {
        // Tell the orb to return to original idle state
        if (orb != null) orb.SetSpeaking(false);
    }
}