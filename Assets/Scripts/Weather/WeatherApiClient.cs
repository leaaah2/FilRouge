using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherApiClient : MonoBehaviour
{
    public SelectedLocationSO selectedLocation;

    [Header("Show debug prints")]
    public bool debug = false;

    public event Action<WeatherResponse> OnWeatherLoaded;

    [ContextMenu("Fetch Weather")]
    public void FetchWeatherNow()
    {
        if (selectedLocation == null)
        {
            Debug.LogError("SelectedLocationSO is not assigned.");
            return;
        }

        StartCoroutine(GetWeather(selectedLocation.latitude, selectedLocation.longitude));
    }

    private void Start()
    {
        FetchWeatherNow();
    }

    private IEnumerator GetWeather(double lat, double lon)
    {
        string url =
            "https://api.open-meteo.com/v1/forecast?" +
            "latitude=" + lat +
            "&longitude=" + lon +
            "&hourly=temperature_2m,cloud_cover,precipitation,weather_code,wind_speed_10m,wind_direction_10m" +
            "&past_days=2&forecast_days=3&timezone=auto";

        using UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Weather API Error: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;
        WeatherResponse response = JsonUtility.FromJson<WeatherResponse>(json);

        if (response == null || response.hourly == null || response.hourly.time == null || response.hourly.time.Length == 0)
        {
            Debug.LogError("Weather parsing failed.");
            yield break;
        }

        Debug.Log($"Weather loaded for {selectedLocation.locationName} | {response.timezone} | {response.hourly.time.Length} hourly samples");
        OnWeatherLoaded?.Invoke(response);
    }
}

[Serializable]
public class WeatherResponse
{
    public int utc_offset_seconds;
    public string timezone;
    public string timezone_abbreviation;
    public HourlyData hourly;
}

[Serializable]
public class HourlyData
{
    public string[] time;
    public float[] temperature_2m;
    public float[] cloud_cover;
    public float[] precipitation;
    public int[] weather_code;
    public float[] wind_speed_10m;
    public float[] wind_direction_10m;
}