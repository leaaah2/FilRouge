using UnityEngine;

[CreateAssetMenu(menuName = "Weather/Selected Location")]
public class SelectedLocationSO : ScriptableObject
{
    public string locationName = "Chicoutimi";
    public double latitude = 48.4280529;
    public double longitude = -71.0684923;

    public void Set(string name, double lat, double lon)
    {
        locationName = name;
        latitude = lat;
        longitude = lon;
    }
}