using UnityEngine;

public class GestionLivres : MonoBehaviour
{
    // Assignez vos Canvas dans l'inspecteur Unity
    public GameObject canvasGauche;
    public GameObject canvasDroit;
    public GameObject canvasGauche2;
    public GameObject canvasDroit2;

    // Fonction appelée par le bouton "Droit" du Canvas Droit
    public void AllerPageDroite()
    {
        // Désactiver le Canvas Droit actuel
        if (canvasDroit != null) canvasDroit.SetActive(false);

        // Activer le Canvas 3
        if (canvasGauche2 != null) canvasGauche2.SetActive(true);
    }

    // Fonction appelée par le bouton "Gauche" du Canvas Gauche
    public void AllerPageGauche()
    {
        // Désactiver le Canvas Gauche actuel
        if (canvasGauche != null) canvasGauche.SetActive(false);

        // Activer le Canvas 4
        if (canvasDroit2 != null) canvasDroit2.SetActive(true);
    }
}