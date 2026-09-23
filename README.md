# Panda Launcher

Application Windows légère pour organiser et lancer jusqu’à cinq listes de logiciels, puis fermer leurs applications depuis un raccourci dédié.

## Télécharger

La release de l’exécutable sera publiée séparément. Pour le moment, compilez le projet avec les instructions ci-dessous. Gardez l’exécutable à un emplacement stable : vos raccourcis pointent vers ce fichier.

## Utilisation

- Créez un launcher et ajoutez vos exécutables, raccourcis Windows ou scripts, avec le bouton Ajouter ou par glisser-déposer.
- Réorganisez la liste par glisser-déposer. Les doublons sont signalés.
- Dans **Paramètres du launcher**, choisissez un nom et une couleur. Vous pouvez activer un raccourci de fermeture et personnaliser son nom.
- **Enregistrer** conserve seulement les options. Le bouton vert **Créer / Mettre à jour le ou les raccourcis** les applique au Bureau, y compris la suppression du raccourci de fermeture lorsqu’il est désactivé.
- Le raccourci de fermeture utilise automatiquement l’icône Panda-stop de la couleur choisie.

L’état des processus est actualisé toutes les deux secondes. Dans le formulaire d’ajout, l’attente de détection du processus est sélectionnée par défaut. Les options avancées permettent de modifier l’attente, le délai, les arguments, le dossier de travail et les droits administrateur. Les scripts ne permettent pas une détection fiable de leur application finale : choisissez alors l’absence d’attente de confirmation, éventuellement avec un délai.

## Fermeture et droits administrateur

La fermeture concerne les applications de la liste, même décochées ou ouvertes autrement. Elle demande d’abord une fermeture normale. Si le programme reste actif, une confirmation permet de forcer l’arrêt, avec un risque de perte des données non enregistrées.

Si des droits administrateur sont nécessaires, une explication précède la demande Windows. Annuler laisse le logiciel ouvert. L’exécutable n’est pas signé : Windows peut afficher « Éditeur inconnu ». Aucun service permanent ni changement des protections Windows n’est installé.

## Données

Les listes sont enregistrées localement dans `%LOCALAPPDATA%\PandaCoustik\PandaLauncher\config.json`, avec sauvegarde `.bak`. Aucun transfert de données ni serveur. Les raccourcis sont associés à des identifiants stables plutôt qu’à leur nom de fichier.

## Compiler et tester

Windows et le compilateur du .NET Framework 4.x sont nécessaires. Aucun SDK .NET supplémentaire, NuGet, Node.js ou Python.

```powershell
.\build.ps1
.\test-all.ps1
```

La compilation produit `dist\Panda Launcher.exe`. Les tests utilisent des programmes témoins et des Bureaux fictifs ; leurs sorties restent exclues de Git. Les demandes UAC sont simulées dans les tests, pas validées sur chaque logiciel tiers.

Pour régénérer les icônes à partir des PNG fournis :

```powershell
.\assets\make-group-icons.ps1
```

Le projet contient le code WinForms, les ressources utilisées et les tests de la version actuelle.

