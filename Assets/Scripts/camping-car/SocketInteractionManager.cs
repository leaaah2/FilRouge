using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using Debug = UnityEngine.Debug;
using SelectEnterEventArgs = UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs;
using TMPro;

public class SocketInteractionManager : MonoBehaviour
{
    [Header("Canvas & UI")]
    public Canvas canvasToHide;
    public Canvas canvasToShow;
    public TextMeshProUGUI cityTextDisplay;

    [Header("Scène")]
    public string nextSceneName;

    [Header("Données de localisation")]
    public Localisation localisationData;
    public SelectedLocationSO selectedLocation;

    [Header("Fade")]
    public ScreenFader screenFader;
    public float fadeOutDuration = 1f;
    public float delayBeforeLoad = 0.1f;

    private bool _isLoading = false;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor;

    private static SocketInteractionManager currentActiveManager;

    void Awake()
    {
        socketInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();

        if (canvasToShow != null) canvasToShow.gameObject.SetActive(false);
        if (canvasToHide != null) canvasToHide.gameObject.SetActive(true);

        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(OnPinPlaced);
        }
    }

    private void OnDestroy()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnPinPlaced);
        }

        if (currentActiveManager == this)
            currentActiveManager = null;
    }

    private void OnPinPlaced(SelectEnterEventArgs args)
    {
        currentActiveManager = this;

        if (localisationData == null)
        {
            Debug.LogWarning("[CampingCar] Localisation data missing on selected socket.");
            return;
        }

        Debug.Log("Pin détecté ! Ville : " + localisationData.cityName);
        Debug.Log("[CampingCar] Prochaine destination: " + localisationData.cityName);

        if (canvasToHide != null) canvasToHide.gameObject.SetActive(false);

        if (canvasToShow != null)
        {
            canvasToShow.gameObject.SetActive(true);

            if (cityTextDisplay != null)
            {
                cityTextDisplay.text = "Destination : " + localisationData.cityName;
            }
            else
            {
                Debug.LogWarning("Text component missing on Canvas 2!");
            }
        }
    }

    // Cette méthode est celle à brancher sur le bouton Validate
    public void OnConfirmButtonClick()
    {
        if (currentActiveManager == null)
        {
            Debug.LogWarning("[CampingCar] No active socket manager selected.");
            return;
        }

        currentActiveManager.ConfirmSelection();
    }

    private void ConfirmSelection()
    {
        if (_isLoading) return;
        if (localisationData == null)
        {
            Debug.LogWarning("[CampingCar] Localisation data is null on confirm.");
            return;
        }

        _isLoading = true;
        StartCoroutine(ConfirmAndLoadRoutine());
    }

    private IEnumerator ConfirmAndLoadRoutine()
    {
        selectedLocation.locationName = localisationData.cityName;
        selectedLocation.latitude = localisationData.latitude;
        selectedLocation.longitude = localisationData.longitude;

        Debug.Log("Données sauvegardées. Chargement de : " + nextSceneName);
        Debug.Log($"[CampingCar] Sending to forest: {localisationData.cityName} | lat={localisationData.latitude} lon={localisationData.longitude}");

        if (screenFader != null)
            yield return screenFader.FadeToBlack(fadeOutDuration);

        if (delayBeforeLoad > 0f)
            yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(nextSceneName);
    }
}