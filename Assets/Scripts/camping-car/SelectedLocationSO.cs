using UnityEngine;

[CreateAssetMenu(menuName = "Weather/Selected Location")]
public class SelectedLocationSO : ScriptableObject
{

    LocalisationDataHolder holder = FindObjectOfType<LocalisationDataHolder>();

    public string locationName = "Chicoutimi";
    public double latitude = 48.4280529;
    public double longitude = -71.0684923;

    public void Set()
    {
        locationName = holder.cityName;
        latitude = holder.latitude;
        longitude = holder.longitude;
    }
}