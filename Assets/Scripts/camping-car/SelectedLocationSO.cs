using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

[CreateAssetMenu(menuName = "Weather/Selected Location")]
public class SelectedLocationSO : ScriptableObject
{
    // 1. Don't initialize Unity API calls here
    LocalisationDataHolder holder;

    public string locationName = "Chicoutimi";
    public double latitude = 48.4280529;
    public double longitude = -71.0684923;

    public void Set()
    {
        // 2. Find the object only when the Set method is actually called
        holder = FindObjectOfType<LocalisationDataHolder>();

        if (holder != null)
        {
            locationName = holder.cityName;
            latitude = holder.latitude;
            longitude = holder.longitude;
        }
        else
        {
            Debug.LogError("SelectedLocationSO: Could not find LocalisationDataHolder in the scene!");
        }
    }
}