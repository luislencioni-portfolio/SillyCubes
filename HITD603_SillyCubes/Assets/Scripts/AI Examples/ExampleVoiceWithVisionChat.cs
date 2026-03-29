using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExampleVoiceWithVisionChat : MonoBehaviour
{
    public SpeechToTextModel stt;
    public VisionLanguageModel vlm;
    public TextToSpeechModel tts;
    public bool useAgentMemory = false;

    private string history;

    void Start()
    {
        stt.ResponseReceivedAction += UserRequestReceived;
        vlm.ResponseReceivedAction += AgentResponseReceived;
    }

    private void UserRequestReceived(string prompt)
    {
        Debug.Log("User request: " + prompt);

        history += "User: " + prompt + "\n";
        history += "Agent: ";

        if (useAgentMemory)
        {
            vlm.AskQuestion(history);
        }
        else
        {
            vlm.AskQuestion(prompt);
        }
    }

    private void AgentResponseReceived(string responseText)
    {
        history += responseText + "\n";

        Debug.Log(responseText);
        tts.PlayAudio(responseText, PlaybackEnded, PlaybackStarted);
    }

    private void PlaybackStarted(string response)
    {
    }

    private void PlaybackEnded()
    {
    }
}