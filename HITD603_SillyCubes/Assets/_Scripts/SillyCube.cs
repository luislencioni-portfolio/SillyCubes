using System.Collections;
using UnityEngine;
using UnityEngine.Networking;   // ---------------------------------------------------------- Luis Lencioni - Additions to the code. I had to add this in order to call sounds from URL. That's the only addition to the top here.

public class SillyCube_LL : MonoBehaviour
{
    /// <summary>
    /// Rules of Silly Cube:
    /// Dont create a crash or hang the program.
    /// Your contactOtherCube() must be significantly different from everyone else's contactOtherCube().
    /// </summary>

    #region("Framework")
    string initials = "LL";

    //do not modify
    const float maxDist = 10f;
    const float interval = 5f;
    Quaternion rotationTarget;
    Vector3 agentTarget;
    Vector3 agentTargetslow;
    bool colliderReady;

    public static float cubeSpeed = 5f;

    public cubeModes cubeMode;
    public enum cubeModes { fly, agent }

    //Audio Source
    AudioSource audioSource;

    //Shake system
    Vector3 shakeOffset = Vector3.zero;

    //Start is called before the first frame update
    void Start()
    {
        startCube();
        name = "Cube_" + initials;
        //Setup collision
        colliderReady = false;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (!rb) { rb = gameObject.AddComponent<Rigidbody>(); }
        if (rb) { rb.isKinematic = true; }
        Collider col = GetComponent<Collider>();
        if (col) { col.isTrigger = true; }
        StartCoroutine(changeRotation());

        // Start preloading audio
        StartCoroutine(PreloadAudio());

    }

    // Update is called once per frame, as fast as the computer will run it. Load dependant.
    void Update()
    {
        moveAround();
        updateCube();
        Debug.DrawLine(transform.position, Vector3.up, Color.green);
        Debug.DrawRay(transform.position, transform.forward, Color.blue);
    }

    //Move the cube by randomly rotating it, then moving directly forward at set speed.
    void moveAround()
    {
        //Time.deltatime is the time between Update frames. 
        //It changes the motion to be more consistent across different speed CPUs
        if (cubeMode == cubeModes.fly)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, rotationTarget, Time.deltaTime);
            transform.position += transform.forward * Time.deltaTime * cubeSpeed;
        }
        else if (cubeMode == cubeModes.agent)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Camera.main.transform.position - transform.position, Vector3.up), Time.deltaTime);
            agentTargetslow = Vector3.Lerp(agentTargetslow, agentTarget, Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, Camera.main.transform.position + (Camera.main.transform.forward * 10) + (agentTargetslow * 2f), Time.deltaTime);
        }

        //Apply shake AFTER movement so it is not overridden
        transform.position += shakeOffset;
    }

    //Detects when one cube touches another. Different from OnCollisionEnter. This detects intersections only, not collision.
    private void OnTriggerEnter(Collider other)
    {
        if (colliderReady)
        {
            //Debug.Log(string.Concat(name," hit ",other.gameObject.name));
            contactOtherCube(other);
        }
    }

    IEnumerator changeRotation()
    {
        yield return new WaitForSeconds(1);
        //dirty way to block collisions until the object has a chance to move away, otherwise potential chance for recursive collisions.
        colliderReady = true;
        // while(true) means this loop will be on for as long as the object exists.
        while (true)
        {
            if (Vector3.Distance(transform.position, Vector3.zero) > maxDist)
            {
                rotationTarget = Quaternion.LookRotation(Vector3.zero - transform.position, Vector3.up);
            }
            else
            {
                rotationTarget = Random.rotation;
            }
            agentTarget = Random.insideUnitSphere;

            yield return new WaitForSeconds(Random.Range(0f, interval));
        }
    }
    #endregion

    /// <summary>
    /// Edit these methods.
    /// </summary>
    #region("Editable Methods")

    // ---------------------------------------------------------- Luis Lencioni - Additions to the code ** Note I had to include an extra line at the top of the code. It is also highlighted.

    string[] audioURLs = new string[] { };
    AudioClip[] downloadedClips;

    Vector3 angryOffset;
    float angryTimer = 0f;
    float angryDuration = 0.5f;
    float shakeStrength = 0.2f;
    float rainbowHue = 0f;
    float rainbowSpeed = 2f;
    float soundCooldown = 2f;
    float lastSoundTime = -10f;
    Material[] cubeMats;
    Color[] originalBaseColors;
    float emissionFadeSpeed = 10f;
    float baseFadeSpeed = 15f;
    public bool allowGlow = true;

    void startCube()
    {
        Renderer r = GetComponent<Renderer>();
        cubeMats = r.materials;
        originalBaseColors = new Color[cubeMats.Length];

        for (int i = 0; i < cubeMats.Length; i++)
        {
            cubeMats[i] = new Material(cubeMats[i]);
            cubeMats[i].EnableKeyword("_EMISSION");
            cubeMats[i].SetColor("_EmissionColor", Color.black);
            originalBaseColors[i] = cubeMats[i].GetColor("_BaseColor");
        }

        r.materials = cubeMats;

        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void updateCube()
    {
        if (cubeMats == null || cubeMats.Length == 0) return;
        if (!allowGlow) return;

        if (angryTimer > 0)
        {
            angryTimer -= Time.deltaTime;

            // ✅ FIXED SHAKE (no longer overridden)
            shakeOffset = Vector3.Lerp(
                shakeOffset,
                Random.insideUnitSphere * shakeStrength,
                Time.deltaTime * 20f
            );

            // Animate rainbow emission
            rainbowHue += Time.deltaTime * rainbowSpeed;
            if (rainbowHue > 1f) rainbowHue -= 1f;
            Color rainbowColor = Color.HSVToRGB(rainbowHue, 1f, 1f);

            for (int i = 0; i < cubeMats.Length; i++)
            {
                cubeMats[i].SetColor("_EmissionColor", rainbowColor * 3f); // ------ Glow intensity
            }
        }
        else
        {
            // Reset shake
            shakeOffset = Vector3.zero;

            for (int i = 0; i < cubeMats.Length; i++)
            {
                Color currentEmission = cubeMats[i].GetColor("_EmissionColor");
                Vector4 newEmission = Vector4.MoveTowards(currentEmission, Color.black, Time.deltaTime * emissionFadeSpeed);
                cubeMats[i].SetColor("_EmissionColor", newEmission);

                Color currentBase = cubeMats[i].GetColor("_BaseColor");
                Color targetBase = originalBaseColors[i];
                Color newBase = Color.Lerp(currentBase, targetBase, Time.deltaTime * baseFadeSpeed);
                cubeMats[i].SetColor("_BaseColor", newBase);
            }
        }
    }

    void contactOtherCube(Collider other)
    {
        if (other is BoxCollider)
        {
            Debug.Log(name + " reacted to " + other.gameObject.name);

            angryTimer = angryDuration;
            shakeStrength = Random.Range(0.1f, 0.4f);
            float intensity = Random.Range(0.2f, 1f); // you can improve this later

            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.OnCubeHit(intensity);
            }

            if (audioSource != null && downloadedClips != null && downloadedClips.Length > 0)
            {
                AudioClip clipToPlay = downloadedClips[Random.Range(0, downloadedClips.Length)];
                if (clipToPlay != null && Time.time - lastSoundTime > soundCooldown)
                {
                    audioSource.PlayOneShot(clipToPlay, 0.7f);
                    lastSoundTime = Time.time;
                    soundCooldown = Random.Range(1.5f, 3f);
                }
            }
        }
    }

    IEnumerator PreloadAudio()
    {
        downloadedClips = new AudioClip[audioURLs.Length];

        for (int i = 0; i < audioURLs.Length; i++)
        {
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioURLs[i], AudioType.MPEG);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                downloadedClips[i] = DownloadHandlerAudioClip.GetContent(www);
                Debug.Log("Loaded audio: " + audioURLs[i]);
            }
            else
            {
                Debug.Log("Failed to load: " + www.error);
            }
        }
    }

    #endregion
}