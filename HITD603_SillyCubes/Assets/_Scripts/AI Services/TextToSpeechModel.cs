using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;

public class TextToSpeechModel : MonoBehaviour
{
  //  [Tooltip("GameObject with SpeechToText component to restart recording when the chat bot stopped talking.")]
  //  public SpeechToText stt;
    [Tooltip("Audio source that plays the agent's speech (should be the same as for lip sync).")]
    public AudioSource audioSource;

    /// <summary>
    /// Synthesizes speech output for a given text.
    /// </summary>
    /// <param name="response">Text to be transformed into speech.</param>
    /// <param name="PlaybackEndedAction">Is called once when the response passed to this method was played.</param>
    /// <param name="PlaybackStartedAction">Is called once when the audio play starts.</param>
    public void PlayAudio(string response, Action PlaybackEndedAction = null, Action<string> PlaybackStartedAction = null)
    {
        PlaybackStartedAction?.Invoke(response);
        StartCoroutine(SendTextToServer(response, PlaybackEndedAction, PlaybackStartedAction));
    }

    private IEnumerator SendTextToServer(string text, Action PlaybackEndedAction = null, Action<string> PlaybackStartedAction = null)
    {
        // Sanitize and limit text length to prevent TTS server errors
        text = SanitizeTextForTTS(text);

        // Limit text length to 500 characters to prevent server overload
        if (text.Length > 500)
        {
            text = text.Substring(0, 500);
        }

        string url = Server.tts_address + "tts?text=" + UnityWebRequest.EscapeURL(text);

        using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"TTS Error: {req.error} - Response Code: {req.responseCode}");
                Debug.LogError($"Failed TTS text: {text}");
                Debug.LogError($"Response body: {req.downloadHandler.text}");
                PlaybackEndedAction?.Invoke();
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
                yield return PlayClip(clip);
                PlaybackEndedAction?.Invoke();
                // stt.Active = true;
            }
        }
    }

    /// <summary>
    /// Sanitizes text for TTS by removing problematic characters
    /// </summary>
    private string SanitizeTextForTTS(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Remove markdown formatting
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*\*([^*]+)\*\*", "$1"); // Bold
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*([^*]+)\*", "$1"); // Italic
        text = System.Text.RegularExpressions.Regex.Replace(text, @"`([^`]+)`", "$1"); // Code

        // Remove excessive punctuation that might confuse TTS
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[—–]", "-"); // Replace em/en dashes
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[""]", "\""); // Normalize quotes
        text = System.Text.RegularExpressions.Regex.Replace(text, @"['']", "'"); // Normalize apostrophes

        // Remove emoji and special Unicode characters that TTS might not handle
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[^\u0000-\u007F]+", " ");

        // Clean up extra whitespace
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

        return text;
    }

    private IEnumerator PlayClip(AudioClip clip)
    {
        // stt.Active = false; // Agent should not listen to its own responses

        if (Application.isPlaying && clip != null)
        {
            audioSource.spatialBlend = 0.0f;
            audioSource.loop = false;
            audioSource.clip = clip;
            audioSource.Play();

            yield return WaitForClipEnd(audioSource);
        }

        yield break;
    }

    private IEnumerator WaitForClipEnd(AudioSource source)
    {
        while (source.isPlaying)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1);

        source.clip = null;
    }
}
