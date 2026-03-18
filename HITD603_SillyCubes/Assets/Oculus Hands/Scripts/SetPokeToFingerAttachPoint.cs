using UnityEngine;

public class SetPokeToFingerAttachPoint : MonoBehaviour
{
    public Transform PokeAttachPoint;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor _xrPokeInteractor;
    // Start is called before the first frame update
    void Start()
    {
        _xrPokeInteractor = transform.parent.parent.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor>();
        SetPokeAttachPoint();
    }

    void SetPokeAttachPoint()
    {
        if (PokeAttachPoint == null) { Debug.Log("Poke Attach Point is null"); return; }

        if (_xrPokeInteractor == null) { Debug.Log("XR Poke Interactor is null"); return; }
        
        _xrPokeInteractor.attachTransform = PokeAttachPoint;
    }
}
