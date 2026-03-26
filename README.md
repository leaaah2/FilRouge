# MeteoRide - Interface Météo Immersive en VR

![Unity](https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![VR](https://img.shields.io/badge/VR-Virtual_Reality-blue?style=for-the-badge)

## Description Générale

MeteoRide est une application de réalité virtuelle (VR) dédiée à la visualisation interactive de données météorologiques. Le projet propose une interface météo en environnement immersif, dans laquelle les conditions climatiques sont représentées visuellement et sonorement dans la scène.

L’application utilise des données météo réelles pour mettre à jour plusieurs systèmes de la scène, notamment le ciel, les précipitations, l’audio ambiant et certains éléments de l’environnement.

## Fonctionnalités

### Météo dynamique en 3D

Mise à jour de la scène à partir de données météo réelles. Le système gère notamment :
- le ciel et l’éclairage (soleil, lune, étoiles, brouillard)
- les nuages
- les précipitations (pluie, neige, transition entre les deux)
- l’audio météo

### Précipitations réactives

Les précipitations sont gérées avec des systèmes de particules qui réagissent à plusieurs paramètres météo :
- intensité des précipitations
- température
- direction et vitesse du vent

### Environnement réactif

Les conditions météo influencent également certains éléments de la scène :
- accumulation et fonte progressive de la neige
- montée du niveau d’eau sous la pluie
- gel des flaques selon la température
- variation de certains éléments de végétation selon la température et l’humidité

### Interface Utilisateur (UI) spatiale

L’application utilise des panneaux interactifs en espace 3D pour :
- sélectionner une destination
- afficher les informations liées à la destination choisie
- naviguer entre les scènes
- accéder à des outils de test

### Interactions VR

Les interactions sont gérées avec les contrôleurs VR via XR Interaction Toolkit :
- pointage
- activation de boutons
- interaction avec des sockets
- validation d’actions dans l’interface

### Audio dynamique

Le système audio s’adapte en temps réel aux conditions météo :
- pluie légère / pluie forte
- vent
- ambiance de jour / ambiance de nuit
- neige / blizzard
- audio environnemental spécifique à certains objets (ex. feu de camp)

### Données en temps réel

Connexion à l’API météo Open-Meteo pour récupérer :
- température
- humidité relative
- couverture nuageuse
- précipitations
- vitesse et direction du vent
- code météo

### Transitions entre scènes

Le projet comprend une scène de sélection de destination et une scène principale de visualisation météo. Les changements de scène utilisent un système de fondu au noir afin d’assurer une transition visuelle cohérente.

### Outils de debug

Un panneau de debug accessible en jeu permet de modifier certaines variables météo directement dans la scène pour tester les différents systèmes sans passer par l’inspecteur Unity.

## Technologies et Outils Utilisés

- Moteur 3D : Unity 3D
- Langage de programmation : C#
- Framework VR : XR Interaction Toolkit / OpenXR
- Gestion de version : GitHub
- Rendu : Universal Render Pipeline (URP)
- API météo : Open-Meteo

## Prérequis

Pour ouvrir et éditer ce projet, vous aurez besoin de :

- Unity Editor (version recommandée : Unity 6.3)
- Un casque de réalité virtuelle compatible (ex : Meta Quest 3)
- L’application native de votre casque (ex : Meta Quest Link, SteamVR) installée et configurée

## Installation et Configuration

### Cloner le dépôt

```bash
git clone https://github.com/leaaah2/FilRouge.git
```

### Ouvrir le projet

1. Lancez Unity Hub.
2. Cliquez sur **Add** et sélectionnez le dossier `FilRouge` que vous venez de cloner.
3. Ouvrez le projet avec la version d’Unity correspondante.

### Configuration VR

Assurez-vous que les packages XR de Unity sont bien installés et configurés dans :

`Edit > Project Settings > XR Plug-in Management`

### Lancer la scène

1. Naviguez dans le dossier `Assets/Scenes`.
2. Ouvrez la première scène, `camping-car`.
3. Lancez le projet avec le casque VR connecté.

## Utilisation

Une fois dans l’application, utilisez les contrôleurs VR pour interagir avec la carte et les interfaces spatiales.

### Dans la scène `camping-car`
- sélectionnez une destination
- validez la destination choisie

### Dans la scène `Forest`
- laissez la météo se charger pour la ville sélectionnée
- observez les mises à jour du ciel, des précipitations, de l’audio et des autres systèmes environnementaux
- utilisez le panneau de debug si nécessaire pour tester différentes conditions

## Démonstration
[Vidéo](https://uqac.ca.panopto.com/Panopto/Pages/Viewer.aspx?id=28ca4814-1792-4755-991f-b4190093938e)
