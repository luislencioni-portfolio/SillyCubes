using System.Collections;
using UnityEngine;
public class SillyCube_LL : MonoBehaviour
{
    /// <summary>
    /// Rules of Silly Cube:
    /// Dont create a crash or hang the program.
    /// Your contactOtherCube() must be significantly different from everyone else's contactOtherCube().
    /// </summary>

    #region Framework
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
        // IF FROZEN, DO NOT MOVE AT ALL
        if (isFrozen) return;

        //Time.deltatime is the time between Update frames. 
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

    //Detects when one cube touches another.
    private void OnTriggerEnter(Collider other)
    {
        if (colliderReady)
        {
            contactOtherCube(other);
        }
    }

    IEnumerator changeRotation()
    {
        yield return new WaitForSeconds(1);
        colliderReady = true;
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
    #region Editable Methods

    [Header("Visual Reaction Settings")]
    public bool allowGlow = true;
    float angryTimer = 0f;
    float angryDuration = 0.5f;
    float shakeStrength = 0.2f;
    float rainbowHue = 0f;
    float rainbowSpeed = 2f;

    [Header("Freeze Settings")]
    bool isFrozen = false;
    float freezeTimer = 0f;
    float freezeDuration = 2.0f; // How long it stays frozen after the ray leaves

    [Header("Material & Fade Settings")]
    Material[] cubeMats;
    Color[] originalBaseColors;
    float emissionFadeSpeed = 10f;
    float baseFadeSpeed = 15f;

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
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void updateCube()
    {
        if (cubeMats == null || cubeMats.Length == 0) return;

        // --- THE FREEZE OVERRIDE ---
        if (cubeMats == null || cubeMats.Length == 0) return;

        if (isFrozen)
        {
            // Count down the tiny buffer
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0) isFrozen = false;

            // Small vibration
            shakeOffset = Vector3.Lerp(shakeOffset, Random.insideUnitSphere * 0.1f, Time.deltaTime * 30f);
            return;
        }

        // --- NORMAL BEHAVIOR ---
        if (angryTimer > 0)
        {
            angryTimer -= Time.deltaTime;
            shakeOffset = Vector3.Lerp(shakeOffset, Random.insideUnitSphere * shakeStrength, Time.deltaTime * 20f);

            if (allowGlow)
            {
                rainbowHue += Time.deltaTime * rainbowSpeed;
                if (rainbowHue > 1f) rainbowHue -= 1f;
                Color rainbowColor = Color.HSVToRGB(rainbowHue, 1f, 1f);
                foreach (Material m in cubeMats) m.SetColor("_EmissionColor", rainbowColor * 3f);
            }
        }
        else
        {
            shakeOffset = Vector3.zero;
            FadeMaterialsToNormal();
        }
    }

    void FadeMaterialsToNormal()
    {
        for (int i = 0; i < cubeMats.Length; i++)
        {
            Color currentEmission = cubeMats[i].GetColor("_EmissionColor");
            cubeMats[i].SetColor("_EmissionColor", Vector4.MoveTowards(currentEmission, Color.black, Time.deltaTime * emissionFadeSpeed));
            cubeMats[i].SetColor("_BaseColor", Color.Lerp(cubeMats[i].GetColor("_BaseColor"), originalBaseColors[i], Time.deltaTime * baseFadeSpeed));
        }
    }

    // This detects the Cone Ray
    private void OnTriggerStay(Collider other)
    {
        // Find the input script on the Batsignal (Parent)
        BatSignalInput input = other.GetComponentInParent<BatSignalInput>();

        if (other.name.Contains("Cone") && input != null)
        {
            if (input.isTriggerPressed)
            {
                isFrozen = true;
                freezeTimer = 0.15f;
                Debug.Log("<color=cyan>CUBE FROZEN</color>");
            }
            else
            {
                // If the beam is touching but button is NOT pressed, unfreeze!
                isFrozen = false;
            }
        }
    }

    void contactOtherCube(Collider other)
    {
        if (isFrozen) return;

        if (other is BoxCollider)
        {
            if (ComicEffectManager.Instance != null) ComicEffectManager.Instance.ShowHitAtPosition(transform.position);
            angryTimer = angryDuration;
            shakeStrength = Random.Range(0.1f, 0.4f);
            if (MusicManager.Instance != null) MusicManager.Instance.OnCubeHit(Random.Range(0.2f, 1f));
        }
    }
    #endregion
}