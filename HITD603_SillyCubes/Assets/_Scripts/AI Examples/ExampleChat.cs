using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExampleChat : MonoBehaviour
{
    public LargeLanguageModel llm;
    public TMP_InputField input;
    public Text userInputField;
    public Text agentOutputField;
    public bool useAgentMemory = false;

    private string history;

    void Start()
    {
        llm.ResponseReceivedAction += AgentResponseReceived;
    }

    public void SendText()
    {
        string prompt = input.text;
        input.text = "";
        userInputField.text = prompt;
        agentOutputField.text = "...";
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
    }

    private void AgentResponseReceived(string responseText)
    {
        history += responseText + "\n";

        agentOutputField.text = responseText;
    }
}