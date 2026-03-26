using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoListener : MonoBehaviour
{
    public SerialController serialController;

    // Cooldown to avoid spamming Arduino
    public float sendCooldown = 0.2f;
    private float lastSendTime = 0f;

    // Singleton instance for easy access if needed
    public static ArduinoListener Instance;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update   
    void Start()
    {
        if (serialController != null)
        {
            serialController.OnMessageReceived += OnMessageReceived;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Change to any key you want
        {
            serialController.SendSerialMessage("Hey Jude!!"); // Replace with your actual message
        }
    }

    // Send a blink message to Arduino
    public void SendBlink()
    {
        if (serialController == null) return;

        if (Time.time - lastSendTime > sendCooldown)
        {
            serialController.SendSerialMessage("BLINK");
            lastSendTime = Time.time;
            Debug.Log("Sent: BLINK");
        }
    }

    void OnMessageReceived(string message)
    {
        Debug.Log("Received from Arduino: " + message);
    }

    void OnDestroy()
    {
        if (serialController != null)
        {
            serialController.OnMessageReceived -= OnMessageReceived;
        }
    }
}