using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Pour manipuler Text et Canvas
using UnityEngine.XR.Interaction.Toolkit;
using static System.Net.Mime.MediaTypeNames;
using Debug = UnityEngine.Debug;
using SelectEnterEventArgs = UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs;
using TMPro;

public class SocketInteractionManager : MonoBehaviour
{
    [Header("Canvas & UI")]
    public Canvas canvasToHide;       // Canvas 1 (optionnel, si vous en avez un ouvert)
    public Canvas canvasToShow;       // Canvas 2 (celui avec le bouton et le texte)
    public TextMeshProUGUI cityTextDisplay;      // Le composant Text qui affichera le nom de la ville

    [Header("Scène")]
    public string nextSceneName;      // Nom de la scène suivante

    [Header("Données de localisation")]
    public SelectedLocationSO selectedLocation; // ScriptableObject pour stocker les données de localisation

    [Header("Fade")]
    public ScreenFader screenFader;
    public float fadeOutDuration = 1f;
    public float delayBeforeLoad = 0.1f;

    private bool _isLoading = false;

    // Références internes
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor;
    private Localisation localisationData;


    void Awake()
    {
        socketInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        localisationData = GetComponent<Localisation>();
        
        // Initialisation : On cache le Canvas 2 au départ
        if (canvasToShow != null) canvasToShow.gameObject.SetActive(false);
        if (canvasToHide != null) canvasToHide.gameObject.SetActive(true);

        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(OnPinPlaced);
        }
    }

    private void OnPinPlaced(SelectEnterEventArgs args)
    {
        Debug.Log("Pin détecté ! Ville : " + localisationData.cityName);

        // 1. Gestion des Canvas : On cache le 1, on montre le 2
        if (canvasToHide != null) canvasToHide.gameObject.SetActive(false);

        if (canvasToShow != null)
        {
            canvasToShow.gameObject.SetActive(true);

            // 2. Mise à jour du texte avec le nom de la ville
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

    // Fonction liée au bouton du Canvas 2
    public void OnConfirmButtonClick()
    {
        if (_isLoading) return;
        if (localisationData == null) return;

        _isLoading = true;
        StartCoroutine(ConfirmAndLoadRoutine());
    }

    private System.Collections.IEnumerator ConfirmAndLoadRoutine()
    {
        if (localisationData == null)
            yield break;

        selectedLocation.locationName = localisationData.cityName;
        selectedLocation.latitude = localisationData.latitude;
        selectedLocation.longitude = localisationData.longitude;

        Debug.Log("Données sauvegardées. Chargement de : " + nextSceneName);

        if (screenFader != null)
            yield return screenFader.FadeToBlack(fadeOutDuration);

        if (delayBeforeLoad > 0f)
            yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(nextSceneName);
    }
}