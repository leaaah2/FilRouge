# MeteoRide - Interface Météo Immersive en VR

![Unity](https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![VR](https://img.shields.io/badge/VR-Virtual_Reality-blue?style=for-the-badge)

## Description Générale
**MeteoRide** est une application de réalité virtuelle (VR) qui réinvente la façon dont nous consultons la météo. Au lieu d'une simple application 2D, ce projet propose une **simulation d'interface météo immersive**. 

L'utilisateur est plongé dans un environnement interactif où les conditions météorologiques (pluie, neige, ensoleillement, brouillard) sont retranscrites visuellement et de manière sonore autour de lui, offrant une compréhension spatiale et sensorielle des prévisions.

## Fonctionnalités
* **Météo dynamique en 3D :** Utilisation de systèmes de particules et de shaders pour simuler des conditions météorologiques réalistes autour de l'utilisateur.
* **Interface Utilisateur (UI) Spatiale :** Panneaux flottants interactifs permettant de sélectionner différentes villes ou de naviguer dans les prévisions des jours à venir.
* **Interactions VR :** Manipulation de l'interface et des éléments 3D à l'aide des contrôleurs VR (pointage, saisie, retour haptique).
* **Immersion Audio :** Son spatialisé qui s'adapte en temps réel aux conditions climatiques (ex: bruit de la pluie, souffle du vent).
* **Données en temps réel :** Connexion à une API météo (ex: OpenWeatherMap) pour afficher de vraies prévisions.

## Technologies et Outils Utilisés
* **Moteur 3D :** [Unity 3D](https://unity.com/) 
* **Langage de programmation :** C#
* **Framework VR :** XR Interaction Toolkit / OpenXR
* **Gestion de version :** GitHub
* **Rendu :** Universal Render Pipeline (URP)

## Prérequis
Pour ouvrir et éditer ce projet, vous aurez besoin de :
* **Unity Editor** (Version recommandée : *Unity 6.3*)
* Un casque de réalité virtuelle compatible (ex: MetaQuest 3).
* L'application native de votre casque (ex: Meta Quest Link, SteamVR) installée et configurée.

## Installation et Configuration

1. **Cloner le dépôt :**
   ```bash
   git clone [https://github.com/leaaah2/FilRouge.git](https://github.com/leaaah2/FilRouge.git)
   ```

2. **Ouvrir le projet :**

   Lancez Unity Hub.

   Cliquez sur Add et sélectionnez le dossier FilRouge que vous venez de cloner.

   Ouvrez le projet avec la version d'Unity correspondante.

  3. **Configuration VR :**

     Assurez-vous que les packages XR de Unity sont bien installés et configurés dans Edit > Project Settings > XR Plug-in Management.

  4. **Lancer la scène :**

     Naviguez dans le dossier Assets/Scenes (ou l'emplacement de ta scène principale).

     Double-cliquez sur la scène principale et appuyez sur le bouton Play avec votre casque VR branché.

## Utilisation
Une fois dans l'application, utilisez les pointeurs de vos contrôleurs VR pour interagir avec les menus flottants. 
Sélectionnez une destination pour voir l'environnement changer instantanément autour de vous afin de refléter la météo locale.

