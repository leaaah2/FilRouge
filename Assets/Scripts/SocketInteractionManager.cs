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

    // Références internes
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor;
    private Localisation localisationData;
    private static LocalisationDataHolder dataHolder; // Référence au gardien de données

    void Awake()
    {
        socketInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        localisationData = GetComponent<Localisation>();

        // Initialisation : On cache le Canvas 2 au départ
        if (canvasToShow != null) canvasToShow.gameObject.SetActive(false);
        if (canvasToHide != null) canvasToHide.gameObject.SetActive(true);

        // Gestion du Persistence Manager (Singleton)
        if (dataHolder == null)
        {
            GameObject holderObj = new GameObject("LocalisationDataHolder");
            DontDestroyOnLoad(holderObj);
            dataHolder = holderObj.AddComponent<LocalisationDataHolder>();
        }

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
        if (localisationData == null) return;

        // 3. Sauvegarde des données dans le manager persistant AVANT le changement de scène
        dataHolder.SaveData(localisationData.cityName, localisationData.latitude, localisationData.longitude);

        Debug.Log("Données sauvegardées. Chargement de : " + nextSceneName);

        // 4. Chargement de la scène
        // Si vous devez envoyer à l'API, faites-le ici (Coroutine) avant le LoadScene
        SceneManager.LoadScene(nextSceneName);
    }
}