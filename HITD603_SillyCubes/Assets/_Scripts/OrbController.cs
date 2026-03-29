using UnityEngine;

public class OrbController : MonoBehaviour
{
    [Header("Follow & Floating")]
    public Transform cameraTransform;
    public Vector3 offset = new Vector3(0.5f, -0.2f, 1.2f);
    public float followSpeed = 3f;
    public float amplitude = 0.1f; // Adjust this in the Inspector for bobbing height
    public float frequency = 1.5f;

    [Header("Speaking Reaction")]
    public ParticleSystem orbParticles;
    [ColorUsage(true, true)] public Color speakingColor = Color.green;
    public float speakingSizeMult = 1.3f;

    private Color idleColor;
    private float idleSize;
    private bool isSpeaking = false;

    void Awake()
    {
        if (orbParticles != null)
        {
            // Stores the original look from your prefab to return to it later
            idleColor = orbParticles.main.startColor.color;
            idleSize = orbParticles.main.startSize.constant;
        }
    }

    // This is the function name that must match the call in your AI script
    public void SetSpeaking(bool speaking)
    {
        isSpeaking = speaking;
    }

    void Update()
    {
        // Floating logic using your amplitude setting
        float bounce = Mathf.Sin(Time.time * frequency) * amplitude;
        Vector3 targetPos = cameraTransform.TransformPoint(offset) + new Vector3(0, bounce, 0);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
        transform.LookAt(cameraTransform);

        if (orbParticles != null)
        {
            var main = orbParticles.main;
            Color targetColor = isSpeaking ? speakingColor : idleColor;
            float targetSize = isSpeaking ? idleSize * speakingSizeMult : idleSize;

            main.startColor = Color.Lerp(main.startColor.color, targetColor, Time.deltaTime * 5f);
            main.startSize = Mathf.Lerp(main.startSize.constant, targetSize, Time.deltaTime * 5f);
        }
    }
}