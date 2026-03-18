using UnityEngine;

public class CubeGrabberVR : MonoBehaviour
{
    [Header("Hand / Controller Settings")]
    public Transform hand;                 // The VR controller or hand transform
    public Transform laserOrigin;          // Optional: tip of laser; if null, hand.position is used

    [Header("Grab Settings")]
    public float maxGrabDistance = 30f;    // How far the ray can reach
    public float grabRadius = 0.3f;        // SphereCast radius for easier grabbing
    public string grabInput = "Grab";      // Input button name

    private SillyCube_LL heldCube;

    void Update()
    {
        Vector3 origin = (laserOrigin != null) ? laserOrigin.position : hand.position;
        Vector3 direction = (laserOrigin != null) ? laserOrigin.forward : hand.forward;

        // Visual debug ray
        Debug.DrawRay(origin, direction * maxGrabDistance, Color.red);

        // Grab input
        if (Input.GetButtonDown(grabInput))
        {
            TryGrabCube(origin, direction);
        }

        // Release input
        if (Input.GetButtonUp(grabInput))
        {
            ReleaseCube();
        }

        // Make held cube follow the hand
        if (heldCube != null)
        {
            heldCube.transform.position = hand.position;
            heldCube.transform.rotation = hand.rotation;
        }
    }

    void TryGrabCube(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction);
        RaycastHit hit;

        // SphereCast instead of Raycast for moving cubes
        if (Physics.SphereCast(ray, grabRadius, out hit, maxGrabDistance))
        {
            SillyCube_LL cube = hit.collider.GetComponent<SillyCube_LL>();
            if (cube != null)
            {
                GrabCube(cube);
            }
        }
    }

    void GrabCube(SillyCube_LL cube)
    {
        heldCube = cube;

        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;         // Disable physics while holding
            rb.linearVelocity = Vector3.zero;    // Stop movement
            rb.angularVelocity = Vector3.zero;
        }

        cube.transform.SetParent(hand);     // Attach cube to hand
    }

    void ReleaseCube()
    {
        if (heldCube == null) return;

        Rigidbody rb = heldCube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;        // Enable physics again
            rb.linearVelocity = Vector3.zero;    // Optional: prevent flying away
        }

        heldCube.transform.SetParent(null);  // Detach cube
        heldCube = null;
    }
}