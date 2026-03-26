using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioSource musicSource;

    [Header("Volume")]
    public float normalVolume = 0.5f;
    public float hitVolume = 0.2f;
    public float volumeRecoverSpeed = 1.5f;

    [Header("Filter")]
    public AudioLowPassFilter lowPassFilter;
    public float normalCutoff = 22000f;
    public float hitCutoff = 800f;
    public float filterRecoverSpeed = 2000f;

    private float targetVolume;
    private float targetCutoff;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        musicSource.loop = true;
        musicSource.volume = normalVolume;
        musicSource.Play();

        targetVolume = normalVolume;
        targetCutoff = normalCutoff;
    }

    void Update()
    {
        // Smooth volume recovery
        musicSource.volume = Mathf.Lerp(musicSource.volume, targetVolume, Time.deltaTime * volumeRecoverSpeed);

        // Smooth filter recovery
        if (lowPassFilter != null)
        {
            lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, targetCutoff, Time.deltaTime * 5f);
        }
    }

    public void OnCubeHit(float intensity)
    {
        if (musicSource == null || lowPassFilter == null) return;

        // HARD CUT (super obvious)
        musicSource.volume = 0.1f;
        lowPassFilter.cutoffFrequency = 150f;

        StopAllCoroutines();
        StartCoroutine(Recover());
    }

    IEnumerator Recover()
    {
        yield return new WaitForSeconds(0.5f);

        musicSource.volume = 0.5f;
        lowPassFilter.cutoffFrequency = 22000f;
    }

    System.Collections.IEnumerator PulseBack()
    {
        yield return new WaitForSeconds(0.1f);

        targetVolume = normalVolume;
        targetCutoff = normalCutoff;
    }
}