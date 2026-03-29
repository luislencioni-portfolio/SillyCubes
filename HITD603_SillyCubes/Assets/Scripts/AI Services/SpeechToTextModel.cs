using UnityEngine;
using UnityEngine.UI;
using NativeWebSocket;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Vosk Streaming STT Client
/// Implements real-time speech recognition with interim results
///
/// Required: Install NativeWebSocket library:
/// https://github.com/endel/NativeWebSocket
/// Package Manager -> Add package from git URL -> https://github.com/endel/NativeWebSocket.git#upm
/// </summary>
public class SpeechToTextModel : MonoBehaviour
{
    [Tooltip("Audio chunk size (samples) - 100ms = 1600 samples at 16kHz")]
    public int chunkSize = 1600;  // 16000 / 10 = 1600

    [Header("UI References")]
    [Tooltip("Real-time display of partial results")]
    public Text interimText;

    [Tooltip("Final result display")]
    public Text finalText;

    // Private variables
    private int sampleRate = 16000; // Sample rate - must be 16000Hz (Vosk requirement)
    private WebSocket websocket;
    private AudioClip microphoneClip;
    private int lastSamplePosition = 0;
    private bool isStreaming = false;
    private bool isConnected = false;
    private bool ignoringPartialResults = false;
    private bool enableDebugLogs = false;

    // Message queue (for main thread processing)
    private Queue<string> messageQueue = new Queue<string>();

    public Action<string> ResponseReceivedAction;

    // ==================== Unity Lifecycle ====================

    async void Start()
    {
        Log("Initializing Streaming STT Client");

        // Check UI configuration
        if (interimText == null)
        {
            LogError("Interim Text UI component not configured");
        }

        if (finalText == null)
        {
            LogError("Final Text UI component not configured");
        }

        if (interimText != null && finalText != null && interimText == finalText)
        {
            LogError("WARNING: Interim Text and Final Text point to the same UI component!");
            LogError("This will cause Final Text to be cleared. Please assign different UI Text components in Inspector");
        }

        // Check microphone permission
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            LogError("Microphone permission denied");
            enabled = false;
            return;
        }

        // Check microphone device
        if (Microphone.devices.Length == 0)
        {
            LogError("No microphone device detected");
            enabled = false;
            return;
        }

        Log($"Using microphone: {Microphone.devices[0]}");

        // Initialize WebSocket
        await ConnectWebSocket();
    }

    void Update()
    {
        // Process message queue (on main thread)
        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            ProcessSTTResult(message);
        }

        // If streaming, send audio data
        if (isStreaming && isConnected)
        {
            StreamAudioData();
        }

        // Update WebSocket (NativeWebSocket must be called in Update)
        #if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            websocket.DispatchMessageQueue();
        }
        #endif
    }

    async void OnApplicationQuit()
    {
        // Clean up resources
        StopStreaming();

        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }

    // ==================== WebSocket Connection ====================

    async System.Threading.Tasks.Task ConnectWebSocket()
    {
        Log($"Connecting to WebSocket: {Server.stt_address}");

        websocket = new WebSocket(Server.stt_address);

        // WebSocket event handlers
        websocket.OnOpen += () =>
        {
            Log("WebSocket connected successfully");
            isConnected = true;
            StartStreaming();
        };

        websocket.OnMessage += (bytes) =>
        {
            // Put message in queue, process in Update (thread-safe)
            string message = Encoding.UTF8.GetString(bytes);
            lock (messageQueue)
            {
                messageQueue.Enqueue(message);
            }
        };

        websocket.OnError += (errorMsg) =>
        {
            LogError($"WebSocket error: {errorMsg}");
        };

        websocket.OnClose += (closeCode) =>
        {
            Log($"WebSocket closed: {closeCode}");
            isConnected = false;
            StopStreaming();
        };

        // Connect
        try
        {
            await websocket.Connect();
        }
        catch (Exception e)
        {
            LogError($"Connection failed: {e.Message}");
        }
    }

    // ==================== Audio Streaming ====================

    void StartStreaming()
    {
        if (isStreaming)
        {
            Log("Already streaming");
            return;
        }

        Log("Started recording");

        // Start continuous recording (loop recording, 5s buffer - fixes data loss bug)
        // Note: Sample rate must be 16000Hz (Vosk requirement)
        microphoneClip = Microphone.Start(null, true, 5, sampleRate);

        isStreaming = true;
        lastSamplePosition = 0;
    }

    void StopStreaming()
    {
        if (!isStreaming) return;

        Log("Stopped recording");

        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }

        isStreaming = false;
    }

    void StreamAudioData()
    {
        if (microphoneClip == null) return;

        // Get current recording position
        int currentPosition = Microphone.GetPosition(null);

        // Handle position wrap-around for loop recording
        if (currentPosition < lastSamplePosition)
        {
            lastSamplePosition = 0;
        }

        // Calculate available samples
        int availableSamples = currentPosition - lastSamplePosition;

        // If enough data available (chunkSize samples = 100ms)
        if (availableSamples >= chunkSize)
        {
            SendAudioChunk(chunkSize);
        }
    }

    void SendAudioChunk(int samples)
    {
        // 1. Read audio data
        float[] floatSamples = new float[samples];
        microphoneClip.GetData(floatSamples, lastSamplePosition);

        // 2. Convert to Int16 PCM (Vosk required format)
        byte[] pcmBytes = new byte[samples * 2];  // Int16 = 2 bytes

        for (int i = 0; i < samples; i++)
        {
            // Convert float (-1.0 to 1.0) to Int16 (-32768 to 32767)
            short pcmValue = (short)(Mathf.Clamp(floatSamples[i], -1f, 1f) * 32767);

            // Little-endian byte order
            pcmBytes[i * 2] = (byte)(pcmValue & 0xFF);
            pcmBytes[i * 2 + 1] = (byte)((pcmValue >> 8) & 0xFF);
        }

        // 3. Send via WebSocket
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            websocket.Send(pcmBytes);
            // Log($"Sending audio chunk: {pcmBytes.Length} bytes");  // Optional detailed log
        }

        // 4. Update position
        lastSamplePosition += samples;
    }

    // ==================== STT Result Processing ====================

    void ProcessSTTResult(string json)
    {
        try
        {
            // Parse JSON
            STTMessage msg = JsonUtility.FromJson<STTMessage>(json);

            if (msg == null)
            {
                LogError($"Failed to parse JSON: {json}");
                return;
            }

            if (msg.type == "partial")
            {
                // Ignore partial messages if we just received a final result
                if (ignoringPartialResults)
                {
                    Log($"Ignoring partial message after final: {msg.text}");
                    return;
                }

                // Partial recognition result (realtime subtitle)
                if (!string.IsNullOrEmpty(msg.text))
                {
                    Log($"Partial result: {msg.text}");
                    UpdateFinalText("");
                    UpdateSubtitle(msg.text);
                }
            }
            else if (msg.type == "final")
            {
                // Final recognition result
                if (!string.IsNullOrEmpty(msg.text))
                {
                    Log($"Final result: {msg.text}");

                    // Set flag to ignore subsequent partial messages for 1 second
                    ignoringPartialResults = true;
                    StartCoroutine(ResetPartialIgnoreFlag(1.0f));

                    // Clear subtitle first, then update final text
                    UpdateSubtitle("");
                    UpdateFinalText(msg.text);

                    // Send notification about final text
                    ResponseReceivedAction?.Invoke(msg.text);
                }
            }
            else if (msg.type == "pong")
            {
                Log("Received pong");
            }
        }
        catch (Exception e)
        {
            LogError($"Error processing STT result: {e.Message}");
        }
    }

    void UpdateSubtitle(string text)
    {
        if (interimText != null)
        {
            interimText.text = text;
            Log($"UI: Subtitle updated = '{text}'");
        }
        else
        {
            LogError("UI: Subtitle component is null");
        }
    }

    void UpdateFinalText(string text)
    {
        if (finalText != null)
        {
            finalText.text = text;
            Log($"UI: Final Text updated = '{text}'");
        }
        else
        {
            LogError("UI: Final Text component is null");
        }
    }

    // ==================== Helper Methods ====================

    void Log(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[STT] {message}");
        }
    }

    void LogError(string message)
    {
        Debug.LogError($"[STT] {message}");
    }

    // ==================== Public Control Methods ====================

    /// <summary>
    /// Manual start/stop recording (optional)
    /// </summary>
    public void ToggleStreaming()
    {
        if (isStreaming)
        {
            StopStreaming();
        }
        else
        {
            StartStreaming();
        }
    }

    /// <summary>
    /// Reset recognizer (optional)
    /// </summary>
    public async void ResetRecognizer()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            string command = "{\"action\":\"reset\"}";
            await websocket.SendText(command);
            Log("Sent reset command to recognizer");
        }
    }

    /// <summary>
    /// Send heartbeat (optional)
    /// </summary>
    public async void SendPing()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            string command = "{\"action\":\"ping\"}";
            await websocket.SendText(command);
            Log("Sending ping");
        }
    }

    /// <summary>
    /// Reset the ignore partial flag after a delay
    /// </summary>
    IEnumerator ResetPartialIgnoreFlag(float delay)
    {
        yield return new WaitForSeconds(delay);
        ignoringPartialResults = false;
        Log("Resumed processing partial messages");
    }
}

// ==================== Data Structures ====================

/// <summary>
/// STT server response message format
/// </summary>
[Serializable]
public class STTMessage
{
    public string type;  // "partial", "final", "pong"
    public string text;  // Recognized text
}
