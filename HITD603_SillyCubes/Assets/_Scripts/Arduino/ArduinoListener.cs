using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoListener : MonoBehaviour
{
    public SerialController serialController;

    // Start is called before the first frame update
    void Start()
    {
        if (serialController != null)
        {
            serialController.OnMessageReceived += MoveCube;
        } 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Change to any key you want
        {
            serialController.SendSerialMessage("HelloArduino"); // Replace with your actual message
        }
    }

    void MoveCube(string message)
    {
        Debug.Log("Received: " + message);
        Vector3 pos = transform.position;
        pos.x += 0.01f;
        transform.position = pos;
    }

    void OnDestroy()
    {
        if (serialController != null)
        {
            serialController.OnMessageReceived -= MoveCube;
        }
    }
}
