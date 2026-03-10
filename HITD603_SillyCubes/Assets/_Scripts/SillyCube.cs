using System.Collections;
using UnityEngine;
using UnityEngine.Networking;   // ---------------------------------------------------------- Luis Lencioni - Additions to the code. I had to add this in order to call sounds from URL. That's the only addition to the top here.

public class SillyCube_LL: MonoBehaviour
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

    // Start is called before the first frame update
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
    }

    //detects when one cube touches another. Different from OnCollisionEnter. This detects intersections only, not collision.
    private void OnTriggerEnter(Collider other)
    {
        if (colliderReady)
        {
            //Debug.Log(string.Concat(name," hit ",other.gameObject.name));
            contactOtherCube(other);
        }
    }

    //IEnumerators are coroutines that run outside of Update(). You can control your own timing and events in them. 
    IEnumerator changeRotation()
    {
        yield return new WaitForSeconds(1);
        //dirty way to block collisions until the object has a chance to move away, otherwise potential chance for recursive collisions.
        colliderReady = true;
        // while(true) means this loop will be on for as long as the object exists.
        while (true)
        {
            //detects if cube goes beyond 10units from the center. Automatically creates rotation looking at center.
            if (Vector3.Distance(transform.position, Vector3.zero) > maxDist)
            {
                rotationTarget = Quaternion.LookRotation(Vector3.zero - transform.position, Vector3.up);
            }
            else
            {
                rotationTarget = Random.rotation;
            }
            agentTarget = Random.insideUnitSphere;

            //Every while() loop should include a 'yield null" or other yield type, otherwise Unity may crash if the logic never resolves.
            yield return new WaitForSeconds(Random.Range(0f, interval));
        }
    }
    #endregion

    /// <summary>
    /// Edit these methods.
    /// </summary>
    #region("Editable Methods")

    // ---------------------------------------------------------- Luis Lencioni - Additions to the code ** Note I had to include an extra line at the top of the code. It is also highlighted.

    //Audio Source
    string[] audioURLs = new string[]
    {
       // "https://raw.githubusercontent.com/luislencioni-portfolio/SillyCubes/16f4546057f135a3b21db73926fa44d5c03b821a/Audios/freesound_community-funny-yay-6273.mp3",
      //  "https://raw.githubusercontent.com/luislencioni-portfolio/SillyCubes/16f4546057f135a3b21db73926fa44d5c03b821a/Audios/freesound_community-angry-grunt-103204.mp3" ,
       // "https://raw.githubusercontent.com/luislencioni-portfolio/SillyCubes/16f4546057f135a3b21db73926fa44d5c03b821a/Audios/freesound_community-fart-83471.mp3"
    };
    AudioClip[] downloadedClips;

Vector3 angryOffset;
float angryTimer = 0f;
float angryDuration = 0.5f;
float shakeStrength = 0.2f;
float rainbowHue = 0f;
float rainbowSpeed = 2f;
float soundCooldown = 2f;   
float lastSoundTime = -10f;   

void startCube()
{
    GetComponent<Renderer>().material.color = Color.black;
    audioSource = GetComponent<AudioSource>();
    if (!audioSource)
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }
}

void updateCube()
{
    Renderer r = GetComponent<Renderer>();

    if (angryTimer > 0)
    {
        angryTimer -= Time.deltaTime;

        // Shake effect
        angryOffset = Random.insideUnitSphere * shakeStrength;
        transform.position += angryOffset;

        // Animate rainbow color
        rainbowHue += Time.deltaTime * rainbowSpeed;
        if (rainbowHue > 1f) rainbowHue -= 1f;
        Color rainbowColor = Color.HSVToRGB(rainbowHue, 1f, 1f);
        r.material.color = rainbowColor;
    }
    else
    {
        // Calm down
        r.material.color = Vector4.MoveTowards(r.material.color, Color.black, Time.deltaTime * 2f);
    }
}

void contactOtherCube(Collider other)
{
if (other is BoxCollider && gameObject.GetInstanceID() < other.gameObject.GetInstanceID())
{
    Debug.Log(name + " reacted to " + other.gameObject.name);

    angryTimer = angryDuration;
    shakeStrength = Random.Range(0.1f, 0.3f);

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

// Loading multiple-files from URL
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