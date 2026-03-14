using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class LocalisationDataHolder : MonoBehaviour
{
    // Variables publiques pour être accessibles depuis d'autres scripts
    public string cityName;
    public float latitude;
    public float longitude;
    public bool hasData = false;

    public void SaveData(string city, float lat, float lon)
    {
        cityName = city;
        latitude = lat;
        longitude = lon;
        hasData = true;
        Debug.Log($"Données sauvegardées : {cityName} ({latitude}, {longitude})");
    }

    // Optionnel : Nettoyer les données si nécessaire
    public void ClearData()
    {
        hasData = false;
        cityName = "";
        latitude = 0;
        longitude = 0;
    }
}