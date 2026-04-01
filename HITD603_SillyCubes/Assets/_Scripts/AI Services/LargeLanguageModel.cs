using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Globalization;
using static TreeEditor.TreeEditorHelper;
using static UnityEngine.Rendering.STP;

public class LargeLanguageModel : MonoBehaviour
{
    [Tooltip("Select the LLM model to use")]
    [ColoredField(1f, 1f, 0f)]  // Bright yellow text
    public LLMModelType modelType = LLMModelType.Gemma3_12B;


    [Tooltip("Defines the AI's role and behavior")]
    [TextArea(3, 10)]
    public string systemPrompt = "";

    [Header("Generation Parameters")]
    [Tooltip("Controls output randomness. 0=deterministic, 1=balanced, 2=most creative")]
    [ColoredField(0f, 2f, 0f, 1f, 0f)]  // min, max, green color
    public float temperature = 0.0f;

    [Tooltip("Maximum number of tokens to generate")]
    [ColoredField(1, 2048, 0f, 1f, 0f)]  // min, max, green color
    public int maxTokens = 130;

    [Tooltip("Stop generating when this text is encountered (optional)")]
    [ColoredField(0f, 1f, 0f)]  // Bright green text
    public string stopSequence = "";

    public Action<string> ResponseReceivedAction;

    public AIGatekeeper gatekeeper; //_______________________________________________________Luis Lencioni modificaton

    private void Start()
    {
        if (gatekeeper != null)
        {
            gatekeeper.OnValidatedSpeech = (text) =>
            {
                Debug.Log("LLM received validated text: " + text);

                // ADD THIS: Tell the UI script to show your text and ask the question
                ExampleVoiceChat chat = GetComponent<ExampleVoiceChat>();
                if (chat != null)
                {
                    chat.UserRequestReceived(text);
                }
                else
                {
                    // Fallback if component is missing
                    AskQuestion(text);
                }
            };
        }
    }

    /// <summary>
    /// Accesses a large language model.
    /// </summary>
    /// <param name="prompt">The prompt to generate a completion for.</param>
    /// <param name="systemPrompt">System prompt that provides instructions and contextual information to the virtual agent. If null, uses value of class variable.</param>
    /// <param name="model">ID of the model to use. If null, uses value of class variable.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate in the completion. If -1, uses value of class variable.</param>
    /// <param name="temperature">What sampling temperature to use. If -1, uses value of class variable.</param>
    /// <param name="stopSequence">A text sequence where the API will stop generating further tokens. If null, uses value of class variable.</param>
    public void AskQuestion(string prompt, string m_systemPrompt = null, string m_model = null, int m_maxTokens = -1, float m_temperature = -1f, string m_stopSequence = null)
    {
        // Parameter priority: method parameter > class variable > default value
        string finalSystemPrompt = m_systemPrompt ?? systemPrompt;
        string finalModel = m_model ?? LLMModels.GetModelName(modelType);
        int finalMaxTokens = m_maxTokens >= 0 ? m_maxTokens : maxTokens;
        float finalTemperature = m_temperature >= 0 ? m_temperature : temperature;
        string finalStopSequence = m_stopSequence ?? stopSequence;

        StartCoroutine(SendRequestToServer(prompt, finalSystemPrompt, finalModel, finalMaxTokens, finalTemperature, finalStopSequence));
    }

    private IEnumerator SendRequestToServer(string prompt, string systemPrompt, string model, int maxTokens, float temperature, string stopSequence)
    {
        WWWForm form = new WWWForm();
        form.AddField("prompt", prompt);
        form.AddField("systemPrompt", systemPrompt);
        form.AddField("model", model);
        form.AddField("max_tokens", maxTokens);
        form.AddField("temperature", Convert.ToString(temperature, CultureInfo.InvariantCulture));
        form.AddField("stop", stopSequence);

        using (UnityWebRequest req = UnityWebRequest.Post(Server.nlp_address + "llm", form))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(req.error);
            }
            else
            {
                string response = req.downloadHandler.text;
                ResponseReceivedAction?.Invoke(response);
            }
        }
    }
}