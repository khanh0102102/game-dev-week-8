# Game Dev Week 8 — UI Design Elements

Unity 6.6 upgrade of the Week 1 airplane scene from the BaiTapTuan1_MayBay repository.

## Included

- TextMesh Pro popup using a Canvas, dim overlay, styled panel, title, message, Continue button, and Restart button.
- GameManager.cs loads the popup title, message, and button labels from code.
- EnterGame.cs opens the popup only after the airplane has taken off and enters the Enter Wall trigger.
- Continue loads NextScene; Restart reloads SampleScene.
- The Week 1 scene contained the airplane model but no flight controller or Enter Wall, so a simple controller was added. Press W, Up Arrow, or Space once to begin flying.

## Setup in Unity 6.6

1. Open this repository with Unity 6000.6.6f1 and wait for Package Manager and compilation to finish.
2. If TMP font assets are missing, choose Window > TextMeshPro > Import TMP Essential Resources, then wait for import to finish.
3. Run Tools > Week 8 > Setup UI Popup Practice.
4. Open Assets/Scenes/SampleScene.unity, press Play, then press W or Up Arrow once.
5. The airplane moves forward, climbs, and reaches the orange Enter Wall. The popup appears on trigger contact after takeoff.
6. Click Continue to open Assets/Scenes/NextScene.unity, or Restart to reload SampleScene. The destination scene includes a Back to Airplane button.

## Code layout

- Assets/Scripts/GameManager.cs: dynamic UI content and Continue/Restart actions.
- Assets/Scripts/EnterGame.cs: trigger logic and takeoff validation.
- Assets/Scripts/AirplaneFlightController.cs: input-driven takeoff and camera follow.
- Assets/Scripts/NextSceneMenu.cs: destination-scene return button.
- Assets/Editor/GameMenuPracticeSetup.cs: generates the UI, Enter Wall, runway, materials, EventSystem, destination scene, and Build Settings entries.

Generated UI and NextScene.unity are saved locally when you run the setup menu. Commit the generated scene files and their .meta files if you want those objects persisted on GitHub.
