#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameMenuPracticeSetup
{
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    private const string NextScenePath = "Assets/Scenes/NextScene.unity";
    private const string MaterialsPath = "Assets/Materials";

    [MenuItem("Tools/Week 8/Setup UI Popup Practice")]
    public static void SetupPopupPractice()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        TMP_FontAsset font = FindFont();
        if (font == null)
        {
            EditorUtility.DisplayDialog("TMP resources missing",
                "Choose Window > TextMeshPro > Import TMP Essential Resources. Wait for import, then run this setup again.", "OK");
            return;
        }
        if (!File.Exists(AbsolutePath(SampleScenePath)))
        {
            EditorUtility.DisplayDialog("Scene missing", "Assets/Scenes/SampleScene.unity was not found.", "OK");
            return;
        }

        Scene sample = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        SetupFlightScene(font);
        EditorSceneManager.MarkSceneDirty(sample);
        EditorSceneManager.SaveScene(sample, SampleScenePath);
        CreateNextScene(font);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(SampleScenePath, true),
            new EditorBuildSettingsScene(NextScenePath, true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        EditorUtility.DisplayDialog("Week 8 setup complete",
            "Popup, Continue/Restart buttons, Enter Wall, flight controls, and NextScene are ready. Press Play, then press W or Up Arrow once.", "OK");
        Debug.Log("Week 8 popup practice is ready. Press Play in SampleScene and press W or Up Arrow once.");
    }

    private static void SetupFlightScene(TMP_FontAsset font)
    {
        GameObject plane = GameObject.Find("AirPlane");
        Camera camera = Camera.main;
        if (plane == null || camera == null)
            throw new InvalidOperationException("The original scene must contain AirPlane and Main Camera.");

        DestroyNamed("GameMenuCanvas");
        DestroyNamed("Runway");
        DestroyNamed("Enter Wall");
        DestroyNamed("FlightMarkers");
        DestroyNamed("EventSystem");

        Rigidbody body = plane.GetComponent<Rigidbody>();
        if (body == null) body = Undo.AddComponent<Rigidbody>(plane);
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        AirplaneFlightController flight = plane.GetComponent<AirplaneFlightController>();
        if (flight == null) flight = Undo.AddComponent<AirplaneFlightController>(plane);
        flight.forwardSpeed = 5f;
        flight.takeoffAfterDistance = 2.5f;
        flight.climbRate = 1.4f;
        flight.cruiseAltitude = 1.6f;
        flight.takeoffHeight = 0.65f;

        // The model's tail is toward positive Z; fly toward negative Z.
        camera.transform.position = new Vector3(0f, 2.5f, 10f);
        camera.transform.rotation = Quaternion.Euler(7f, 180f, 0f);
        camera.fieldOfView = 60f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.035f, 0.065f, 0.12f, 1f);

        Material runwayMat = MakeMaterial("RunwayMaterial.mat", new Color(0.075f, 0.105f, 0.15f, 1f));
        Material markingMat = MakeMaterial("RunwayMarkingMaterial.mat", new Color(0.95f, 0.72f, 0.22f, 1f));
        Material wallMat = MakeMaterial("EnterWallMaterial.mat", new Color(1f, 0.28f, 0.08f, 1f));

        GameObject runway = GameObject.CreatePrimitive(PrimitiveType.Cube);
        runway.name = "Runway";
        runway.transform.position = new Vector3(0f, -1.2f, -9f);
        runway.transform.localScale = new Vector3(9f, 0.2f, 30f);
        runway.GetComponent<Renderer>().sharedMaterial = runwayMat;

        GameObject markers = new GameObject("FlightMarkers");
        for (int i = 0; i < 6; i++)
        {
            GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mark.name = "RunwayMarking_" + (i + 1);
            mark.transform.SetParent(markers.transform, true);
            mark.transform.position = new Vector3(0f, -1.085f, 1f - 4f * i);
            mark.transform.localScale = new Vector3(0.18f, 0.025f, 1.45f);
            mark.GetComponent<Renderer>().sharedMaterial = markingMat;
            Collider col = mark.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);
        }

        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Enter Wall";
        wall.transform.position = new Vector3(0f, 2.4f, -17f);
        wall.transform.localScale = new Vector3(8f, 7f, 0.55f);
        wall.GetComponent<Renderer>().sharedMaterial = wallMat;
        wall.GetComponent<BoxCollider>().isTrigger = true;
        EnterGame enter = Undo.AddComponent<EnterGame>(wall);

        GameObject canvas = CreateCanvas("GameMenuCanvas", true);
        Image overlay = CreateImage("DimOverlay", canvas.transform, new Color(0.012f, 0.028f, 0.06f, 0.82f));
        Stretch((RectTransform)overlay.transform);
        Image panel = CreateImage("PopupPanel", canvas.transform, new Color(0.025f, 0.075f, 0.13f, 0.99f));
        Center((RectTransform)panel.transform, Vector2.zero, new Vector2(850f, 470f));
        Outline outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.12f, 0.76f, 0.95f, 0.7f);
        outline.effectDistance = new Vector2(2f, -2f);
        Image accent = CreateImage("TopAccent", panel.transform, new Color(0.16f, 0.82f, 1f, 1f));
        Center((RectTransform)accent.transform, new Vector2(0f, 226f), new Vector2(850f, 8f));

        GameObject contentRoot = new GameObject("PopupContent", typeof(RectTransform));
        contentRoot.transform.SetParent(panel.transform, false);
        Stretch((RectTransform)contentRoot.transform);

        CreateText("Eyebrow", contentRoot.transform, font, "FLIGHT CONTROL / CHECKPOINT 08", 16f,
            FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0f, 182f), new Vector2(720f, 32f), new Color(0.25f, 0.82f, 0.98f, 1f));
        TMP_Text title = CreateText("Title", contentRoot.transform, font, "FLIGHT CHECKPOINT", 42f,
            FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0f, 125f), new Vector2(740f, 70f), new Color(0.93f, 0.98f, 1f, 1f));
        TMP_Text message = CreateText("Message", contentRoot.transform, font,
            "Your airplane has taken off and reached the Enter Wall. Continue to the next scene or restart the flight.", 24f,
            FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0f, 30f), new Vector2(700f, 112f), new Color(0.78f, 0.87f, 0.93f, 1f));

        Button continueButton = MakeButton(contentRoot.transform, "ContinueButton", "Continue", font,
            new Color(0.08f, 0.48f, 0.75f, 1f), new Vector2(-155f, -132f), new Vector2(260f, 70f));
        Button restartButton = MakeButton(contentRoot.transform, "RestartButton", "Restart", font,
            new Color(0.19f, 0.26f, 0.34f, 1f), new Vector2(155f, -132f), new Vector2(260f, 70f));

        GameManager manager = camera.GetComponent<GameManager>();
        if (manager == null) manager = Undo.AddComponent<GameManager>(camera.gameObject);
        manager.popupCanvas = canvas;
        manager.titleText = title;
        manager.messageText = message;
        manager.continueButton = continueButton;
        manager.restartButton = restartButton;
        manager.continueButtonText = continueButton.GetComponentInChildren<TMP_Text>();
        manager.restartButtonText = restartButton.GetComponentInChildren<TMP_Text>();
        manager.nextSceneName = "NextScene";
        enter.gameManager = manager;
        // Serialize the same On Click() links the lab asks you to set in the Inspector.
        UnityEventTools.AddPersistentListener(continueButton.onClick, manager.NextScene);
        UnityEventTools.AddPersistentListener(restartButton.onClick, manager.Restart);
        EnsureEventSystem();
        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(enter);
        canvas.SetActive(false);
    }

    private static void CreateNextScene(TMP_FontAsset font)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cam.tag = "MainCamera";
        Camera camera = cam.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.035f, 0.065f, 0.12f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        cam.transform.position = new Vector3(0f, 0f, -10f);

        GameObject canvas = CreateCanvas("MissionCompleteCanvas", true);
        Image bg = CreateImage("Background", canvas.transform, new Color(0.035f, 0.065f, 0.12f, 1f));
        Stretch((RectTransform)bg.transform);
        Image panel = CreateImage("MissionPanel", canvas.transform, new Color(0.025f, 0.075f, 0.13f, 0.99f));
        Center((RectTransform)panel.transform, Vector2.zero, new Vector2(800f, 430f));
        Outline outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.12f, 0.76f, 0.95f, 0.7f);
        outline.effectDistance = new Vector2(2f, -2f);
        CreateText("Title", panel.transform, font, "NEXT FLIGHT READY", 40f, FontStyles.Bold,
            TextAlignmentOptions.Center, new Vector2(0f, 110f), new Vector2(700f, 75f), new Color(0.93f, 0.98f, 1f, 1f));
        CreateText("Message", panel.transform, font, "You reached the entry checkpoint successfully. Your next scene is loaded.", 24f,
            FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0f, 25f), new Vector2(660f, 100f), new Color(0.78f, 0.87f, 0.93f, 1f));
        Button back = MakeButton(panel.transform, "ReturnButton", "Back to Airplane", font,
            new Color(0.08f, 0.48f, 0.75f, 1f), new Vector2(0f, -115f), new Vector2(300f, 72f));
        NextSceneMenu controller = cam.AddComponent<NextSceneMenu>();
        controller.returnButton = back;
        EditorUtility.SetDirty(controller);
        EnsureEventSystem();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, NextScenePath);
    }

    private static GameObject CreateCanvas(string name, bool active)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        go.SetActive(active);
        return go;
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = name == "DimOverlay";
        return img;
    }

    private static TMP_Text CreateText(string name, Transform parent, TMP_FontAsset font, string value, float size,
        FontStyles style, TextAlignmentOptions alignment, Vector2 position, Vector2 dimensions, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        Center((RectTransform)go.transform, position, dimensions);
        TMP_Text text = go.GetComponent<TMP_Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static Button MakeButton(Transform parent, string name, string label, TMP_FontAsset font, Color color, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Center((RectTransform)go.transform, pos, size);
        Image img = go.GetComponent<Image>();
        img.color = color;
        Button button = go.GetComponent<Button>();
        button.targetGraphic = img;
        ColorBlock block = button.colors;
        block.normalColor = color;
        block.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        block.pressedColor = Color.Lerp(color, Color.black, 0.18f);
        button.colors = block;
        TMP_Text text = CreateText("Text (TMP)", go.transform, font, label, 25f, FontStyles.Bold,
            TextAlignmentOptions.Center, Vector2.zero, size, Color.white);
        RectTransform rect = (RectTransform)text.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(14f, 4f);
        rect.offsetMax = new Vector2(-14f, -4f);
        return button;
    }

    private static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length > 0) return;
        GameObject go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    private static TMP_FontAsset FindFont()
    {
        // TMP_Settings can be absent until Essential Resources are imported.
        TMP_Settings settings = TMP_Settings.GetSettings();
        if (settings != null && TMP_Settings.defaultFontAsset != null)
            return TMP_Settings.defaultFontAsset;

        string[] ids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
        if (ids.Length == 0)
            ids = AssetDatabase.FindAssets("t:TMP_FontAsset");

        foreach (string id in ids)
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(id));
            if (font != null)
                return font;
        }
        return null;
    }

    private static Material MakeMaterial(string name, Color color)
    {
        string path = MaterialsPath + "/" + name;
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (mat == null)
        {
            mat = new Material(shader);
            mat.name = Path.GetFileNameWithoutExtension(name);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Center(RectTransform rect, Vector2 position, Vector2 dimensions)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        rect.localScale = Vector3.one;
    }

    private static void DestroyNamed(string name)
    {
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.name == name && obj.scene == SceneManager.GetActiveScene())
            {
                Undo.DestroyObjectImmediate(obj);
                return;
            }
        }
    }

    private static string AbsolutePath(string assetPath)
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(root, assetPath.Replace('/', Path.DirectorySeparatorChar));
    }
}
#endif
