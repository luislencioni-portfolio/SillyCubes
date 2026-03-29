using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class BatSignalInput : MonoBehaviour
{
    public bool isTriggerPressed = false;

    [Header("Visual Components")]
    public Light beamLight;  // <--- Drag the 'Light' object
    public Renderer coneRenderer;  // <--- Drag the 'Cone' object
    public Renderer coneFaceRenderer; // <--- Drag the 'Cone Face' object

    public float maxIntensity = 5f;
    public float beamRange = 50f;

    void Update()
    {
        // --- 1. VR INPUT LOGIC
        float triggerValue = 0f;
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, devices);
        if (devices.Count > 0) devices[0].TryGetFeatureValue(CommonUsages.trigger, out triggerValue);

        isTriggerPressed = (triggerValue > 0.1f) || Input.GetButton("Fire1") || Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);

        // --- 2. VISUAL FEEDBACK LOGIC
        if (isTriggerPressed)
        {
            // Turn EVERYTHING on
            if (coneRenderer != null) coneRenderer.enabled = true;
            if (coneFaceRenderer != null) coneFaceRenderer.enabled = true;

            if (beamLight != null)
            {
                beamLight.enabled = true;
                beamLight.intensity = Mathf.Lerp(beamLight.intensity, maxIntensity, Time.deltaTime * 10f);
                beamLight.range = beamRange;
            }
        }
        else
        {
            // Turn EVERYTHING off
            if (coneRenderer != null) coneRenderer.enabled = false;
            if (coneFaceRenderer != null) coneFaceRenderer.enabled = false;

            if (beamLight != null)
            {
                beamLight.intensity = 0;
                beamLight.enabled = false;
            }
        }
    }
}