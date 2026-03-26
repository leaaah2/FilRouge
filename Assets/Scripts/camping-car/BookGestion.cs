using UnityEngine;

public class GestionLivres : MonoBehaviour
{
    // État actuel du livre (0 = Pages 1, 1 = Pages 2, etc.)
    private int etatPage = 0;

    [Header("Configuration des Pages")]
    public GameObject pageGauche1; // Canvas Gauche initial
    public GameObject pageDroite1; // Canvas Droit initial
    public GameObject pageGauche2; // Canvas Gauche suivant
    public GameObject pageDroite2; // Canvas Droit suivant

    void Start()
    {
        // Initialisation : afficher seulement la première paire
        MettreAJourAffichage();
    }

    // Fonction appelée par le bouton "Page Suivante" (sur la page de droite)
    public void AllerPageDroite()
    {
        if (etatPage == 0)
        {
            etatPage = 1;
            MettreAJourAffichage();
            // Ici, vous pourriez déclencher une animation de rotation
        }
    }

    // Fonction appelée par le bouton "Page Précédente" (sur la page de gauche)
    public void AllerPageGauche()
    {
        if (etatPage == 1)
        {
            etatPage = 0;
            MettreAJourAffichage();
        }
    }

    private void MettreAJourAffichage()
    {
        if (etatPage == 0)
        {
            // Afficher la paire 1
            if (pageGauche1) pageGauche1.SetActive(true);
            if (pageDroite1) pageDroite1.SetActive(true);

            // Masquer la paire 2
            if (pageGauche2) pageGauche2.SetActive(false);
            if (pageDroite2) pageDroite2.SetActive(false);
        }
        else if (etatPage == 1)
        {
            // Masquer la paire 1
            if (pageGauche1) pageGauche1.SetActive(false);
            if (pageDroite1) pageDroite1.SetActive(false);

            // Afficher la paire 2
            if (pageGauche2) pageGauche2.SetActive(true);
            if (pageDroite2) pageDroite2.SetActive(true);
        }
    }
}