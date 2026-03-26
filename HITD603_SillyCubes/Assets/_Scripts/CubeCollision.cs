using UnityEngine;

public class CubeCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(gameObject.name + " triggered with " + other.gameObject.name);

        if (ArduinoListener.Instance != null)
        {
            ArduinoListener.Instance.SendBlink();
        }
    }
}