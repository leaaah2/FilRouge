using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class RVScenePortal : MonoBehaviour
{
    [Header("Scene")]
    public string targetSceneName;

    [Header("Distance")]
    public Transform playerHead;
    public float interactDistance = 3f;

    [Header("Fade")]
    public ScreenFader screenFader;
    public float fadeOutDuration = 1f;
    public float delayBeforeLoad = 0.1f;

    [Header("Debug")]
    public bool isPlayerInRange;

    private XRSimpleInteractable _interactable;
    private bool _isTransitioning = false;

    private void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        _interactable.selectEntered.AddListener(OnSelected);
    }

    private void OnDestroy()
    {
        _interactable.selectEntered.RemoveListener(OnSelected);
    }

    private void Update()
    {
        if (playerHead == null)
        {
            _interactable.enabled = false;
            return;
        }

        float distance = Vector3.Distance(playerHead.position, transform.position);
        isPlayerInRange = distance <= interactDistance;

        _interactable.enabled = !_isTransitioning && isPlayerInRange;
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (_isTransitioning) return;
        if (!isPlayerInRange) return;

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        _isTransitioning = true;
        _interactable.enabled = false;

        if (screenFader != null)
            yield return screenFader.FadeToBlack(fadeOutDuration);

        if (delayBeforeLoad > 0f)
            yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(targetSceneName);
    }
}