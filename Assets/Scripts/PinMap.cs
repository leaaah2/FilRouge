using UnityEngine;

public class PinMap : MonoBehaviour
{
    public Transform map; // Référence à l'objet carte
    public TextMesh coordinateText; // Texte UI pour afficher les coordonnées

    // Limites de la carte en coordonnées réelles
    public Vector2 realWorldBottomLeft = new Vector2(48.84f, 2.34f); // Latitude, Longitude
    public Vector2 realWorldTopRight = new Vector2(48.86f, 2.36f);

    void Update()
    {
        // Convertit la position locale du pin en coordonnées réelles
        Vector3 localPosition = transform.localPosition;
        Vector2 normalizedPosition = new Vector2(
            Mathf.InverseLerp(-map.localScale.x / 2, map.localScale.x / 2, localPosition.x),
            Mathf.InverseLerp(-map.localScale.z / 2, map.localScale.z / 2, localPosition.z)
        );

        // Calculer les coordonnées réelles
        float latitude = Mathf.Lerp(realWorldBottomLeft.x, realWorldTopRight.x, normalizedPosition.y);
        float longitude = Mathf.Lerp(realWorldBottomLeft.y, realWorldTopRight.y, normalizedPosition.x);

        // Mettre à jour le texte UI
        coordinateText.text = $"Lat: {latitude:F4}, Long: {longitude:F4}";
    }
}