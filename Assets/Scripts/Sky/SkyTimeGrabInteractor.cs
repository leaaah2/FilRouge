using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class SkyTimeGrabInteractor : MonoBehaviour
{
    public WeatherSkyController skyController;
    public Transform referenceSpace;

    [Header("Time Scrub")]
    public float hoursPerMeter = 8f;
    public bool clampToAvailableRange = true;

    private XRGrabInteractable _grab;
    private IXRSelectInteractor _interactor;
    private Vector3 _grabStartLocal;
    private System.DateTime _timeAtGrabStart;
    private bool _isGrabbed;

    private void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _grab.selectEntered.AddListener(OnSelectEntered);
        _grab.selectExited.AddListener(OnSelectExited);
    }

    private void OnDestroy()
    {
        _grab.selectEntered.RemoveListener(OnSelectEntered);
        _grab.selectExited.RemoveListener(OnSelectExited);
    }

    private void Update()
    {
        if (!_isGrabbed || _interactor == null || skyController == null || !skyController.IsReady())
            return;

        Transform t = _interactor.transform;
        Vector3 worldPos = t.position;

        Vector3 localPos;
        if (referenceSpace != null)
            localPos = referenceSpace.InverseTransformPoint(worldPos);
        else
            localPos = worldPos;

        float deltaX = localPos.x - _grabStartLocal.x;
        float deltaHours = deltaX * hoursPerMeter;

        System.DateTime newTime = _timeAtGrabStart.AddHours(deltaHours);

        if (clampToAvailableRange && skyController.TryGetTimeBounds(out var minTime, out var maxTime))
        {
            if (newTime < minTime) newTime = minTime;
            if (newTime > maxTime) newTime = maxTime;
        }

        skyController.SetCurrentLocalTime(newTime);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        _interactor = args.interactorObject;
        _isGrabbed = true;
        _timeAtGrabStart = skyController.currentLocalTime;

        Transform t = _interactor.transform;
        Vector3 worldPos = t.position;
        _grabStartLocal = referenceSpace != null ? referenceSpace.InverseTransformPoint(worldPos) : worldPos;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        _isGrabbed = false;
        _interactor = null;
    }
}