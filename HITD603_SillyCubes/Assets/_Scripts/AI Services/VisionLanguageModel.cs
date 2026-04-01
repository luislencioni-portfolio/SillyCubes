using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VisionLanguageModel : MonoBehaviour
{
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

    [Header("Camera Input Parameters")]
    [Tooltip("Select whether camera feed should come from virtual camera that is attached to MainCamera or from the user's first webcam")]
    public CameraInputType cameraInput = CameraInputType.VirtualCamera;

    [Tooltip("User interface element that shows camera feed")]
    public RawImage cameraImage;

    [Tooltip("User interface element with camera feed is hidden if set to False")]
    public bool showCameraImage = true;

    public Action<string> ResponseReceivedAction;

    public void Start()
    {
        if (cameraInput == CameraInputType.Webcam)
        {
            WebCamTexture webcamTexture = new WebCamTexture();
            cameraImage.texture = webcamTexture;
            cameraImage.material.mainTexture = webcamTexture;
            webcamTexture.Play();
        }

        if (showCameraImage)
        {
            cameraImage.gameObject.SetActive(true);
        }
        else
        {
            cameraImage.gameObject.SetActive(false);
        }
    }

    private byte[] CreateImage()
    {
        byte[] image = null;

        if (cameraInput == CameraInputType.Webcam)
        {
            WebCamTexture webcamTexture = cameraImage.texture as WebCamTexture;
            Texture2D rawTexture = new Texture2D(webcamTexture.width, webcamTexture.height);
            
            rawTexture.SetPixels(webcamTexture.GetPixels());
            rawTexture.Apply();

            image = rawTexture.EncodeToPNG();
            Debug.Log("Image created with the following size: " + image.Length);
            Destroy(rawTexture);
        }
        else if (cameraInput == CameraInputType.VirtualCamera)
        {
            RenderTexture renderTexture = cameraImage.texture as RenderTexture;
            Texture2D rawTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

            RenderTexture mainRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            rawTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            rawTexture.Apply();
            RenderTexture.active = mainRenderTexture;

            image = rawTexture.EncodeToPNG();
            Debug.Log("Image created with the following size: " + image.Length);
            Destroy(rawTexture);
        }

        return image;
    }

    /// <summary>
    /// Answers questions regarding an image.
    /// </summary>
    /// <param name="prompt">The prompt to generate a completion for.</param>
    /// <param name="systemPrompt">System prompt that provides instructions and contextual information to the virtual agent. If null, uses value of class variable.</param>
    /// <param name="maxTokens">The maximum number of tokens to generate in the completion. If -1, uses value of class variable.</param>
    /// <param name="temperature">What sampling temperature to use. If -1, uses value of class variable.</param>
    /// <param name="stopSequence">A text sequence where the API will stop generating further tokens. If null, uses value of class variable.</param>
    public void AskQuestion(string prompt, string m_systemPrompt = null, string m_model = null, int m_maxTokens = -1, float m_temperature = -1f, string m_stopSequence = null)
    {
        // Parameter priority: method parameter > class variable > default value
        string finalSystemPrompt = m_systemPrompt ?? systemPrompt;
        int finalMaxTokens = m_maxTokens >= 0 ? m_maxTokens : maxTokens;
        float finalTemperature = m_temperature >= 0 ? m_temperature : temperature;
        string finalStopSequence = m_stopSequence ?? stopSequence;

        StartCoroutine(SendRequestToServer(prompt, finalSystemPrompt, CreateImage(), finalMaxTokens, finalTemperature, finalStopSequence));
    }

    private IEnumerator SendRequestToServer(string prompt, string systemPrompt, byte[] image, int maxTokens, float temperature, string stopSequence)
    {
        WWWForm form = new WWWForm();
        form.AddField("prompt", prompt);
        form.AddField("systemPrompt", systemPrompt);
        form.AddBinaryData("image", image, "image.png", "image/png");
        form.AddField("max_tokens", maxTokens);
        form.AddField("temperature", Convert.ToString(temperature, CultureInfo.InvariantCulture));
        form.AddField("stop", stopSequence);

        using (UnityWebRequest req = UnityWebRequest.Post(Server.nlp_address + "vlm", form))
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
    public enum CameraInputType
    {
        VirtualCamera,
        Webcam
    }
}
