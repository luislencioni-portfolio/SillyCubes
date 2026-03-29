using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking; // Required for Web Requests

public class ComicEffectManager : MonoBehaviour
{
    public static ComicEffectManager Instance;

    [Header("UI References")]
    public Image effectImage;
    public Sprite[] comicSprites;

    [Header("Audio from URL")]
    public string[] audioURLs = new string[]
    {
        "https://raw.githubusercontent.com/luislencioni-portfolio/SillyCubes/4bbc3827478c208f20ce27500ef75607410a56ff/Sounds/Sound%2002.mp3",
        "https://raw.githubusercontent.com/luislencioni-portfolio/SillyCubes/4bbc3827478c208f20ce27500ef75607410a56ff/Sounds/Sound%203.mp3",
        "https://raw.githubusercontent.com/luislencioni-portfolio/SillyCubes/4bbc3827478c208f20ce27500ef75607410a56ff/Sounds/Sound01.mp3"
    };

    private AudioClip[] downloadedClips;
    private AudioSource audioSource;

    [Header("Animation Settings")]
    public float duration = 0.6f;
    public float swirlSpeed = 600f;
    public float maxScale = 2.0f;

    private bool isDisplaying = false;
    private int currentSpriteIndex = 0;

    void Awake()
    {
        Instance = this;
        if (effectImage != null) effectImage.gameObject.SetActive(false);

        // Setup AudioSource automatically
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        StartCoroutine(DownloadHitSounds());
    }

    IEnumerator DownloadHitSounds()
    {
        downloadedClips = new AudioClip[audioURLs.Length];
        for (int i = 0; i < audioURLs.Length; i++)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioURLs[i], AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    downloadedClips[i] = DownloadHandlerAudioClip.GetContent(www);
                    Debug.Log("Successfully loaded: " + audioURLs[i]);
                }
                else
                {
                    Debug.LogError("Error loading sound: " + www.error);
                }
            }
        }
    }

    public void ShowHitAtPosition(Vector3 worldPosition)
    {
        // Safety check: don't play if busy or no sprites
        if (isDisplaying || effectImage == null || comicSprites.Length == 0) return;

        // 1. Move to collision spot
        effectImage.transform.position = worldPosition;

        // 2. Select next Sprite (Round Robin)
        effectImage.sprite = comicSprites[currentSpriteIndex];
        currentSpriteIndex = (currentSpriteIndex + 1) % comicSprites.Length;

        // 3. Play Random Sound from the downloaded list
        if (downloadedClips != null && downloadedClips.Length > 0)
        {
            AudioClip randomClip = downloadedClips[Random.Range(0, downloadedClips.Length)];
            if (randomClip != null)
            {
                audioSource.PlayOneShot(randomClip, 0.8f);
            }
        }

        StartCoroutine(AnimateSwirl());
    }

    private IEnumerator AnimateSwirl()
    {
        isDisplaying = true;
        effectImage.gameObject.SetActive(true);

        float elapsed = 0;
        float currentZRotation = Random.Range(-20f, 20f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Make image face the VR camera
            if (Camera.main != null)
            {
                effectImage.transform.LookAt(Camera.main.transform);
                effectImage.transform.Rotate(0, 180, 0);
            }

            // Apply Swirl
            currentZRotation += swirlSpeed * Time.deltaTime;
            effectImage.transform.Rotate(0, 0, currentZRotation);

            // Scale Pop
            float scale = Mathf.Sin(t * Mathf.PI) * maxScale;
            effectImage.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        effectImage.gameObject.SetActive(false);
        isDisplaying = false;
    }
}