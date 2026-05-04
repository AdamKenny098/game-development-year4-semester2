using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class OutpostCaptureUISetupEditor
{
    private const string UIRootName = "OutpostCapture_UI";
    private const string MaterialFolder = "Assets/Generated/OutpostCaptureMaterials";

    private static Material neutralMaterial;
    private static Material blueCapturingMaterial;
    private static Material redCapturingMaterial;
    private static Material blueOwnedMaterial;
    private static Material redOwnedMaterial;

    [MenuItem("Tools/CA3/Setup Capture Point UI And Blue Player")]
    public static void SetupCapturePointUIAndBluePlayer()
    {
        if (!EditorUtility.DisplayDialog(
                "Setup Capture Point UI",
                "This will replace the existing OutpostCapture_UI object if one exists, create the local capture UI, and configure one Blue player for testing.",
                "Setup",
                "Cancel"))
        {
            return;
        }

        CreateMaterials();

        CapturePointLocal capturePoint = FindOrCreateCapturePoint();

        if (capturePoint == null)
        {
            EditorUtility.DisplayDialog(
                "Setup Failed",
                "Could not find CaptureZone_Trigger and could not create the capture point setup.",
                "OK");

            return;
        }

        DeleteExistingUI();

        Canvas canvas = CreateCanvas();
        EnsureEventSystem();

        TMP_Text statusText = CreateText(
            "StatusText",
            canvas.transform,
            "CONTROL POINT NEUTRAL",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -34f),
            new Vector2(700f, 40f),
            28,
            Color.white,
            TextAlignmentOptions.Center);

        Slider captureSlider = CreateSlider(
            "CaptureSlider",
            canvas.transform,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -78f),
            new Vector2(430f, 22f),
            ParseColor("#F7B733"));

        TMP_Text blueScoreText = CreateText(
            "BlueScoreText",
            canvas.transform,
            "BLUE 0%",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(32f, -32f),
            new Vector2(260f, 36f),
            24,
            ParseColor("#4DA3FF"),
            TextAlignmentOptions.Left);

        Slider blueScoreSlider = CreateSlider(
            "BlueScoreSlider",
            canvas.transform,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(32f, -74f),
            new Vector2(260f, 20f),
            ParseColor("#1E6BFF"));

        TMP_Text redScoreText = CreateText(
            "RedScoreText",
            canvas.transform,
            "RED 0%",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-32f, -32f),
            new Vector2(260f, 36f),
            24,
            ParseColor("#FF4D6D"),
            TextAlignmentOptions.Right);

        Slider redScoreSlider = CreateSlider(
            "RedScoreSlider",
            canvas.transform,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-32f, -74f),
            new Vector2(260f, 20f),
            ParseColor("#D82F5E"));

        AssignReferences(
            capturePoint,
            statusText,
            captureSlider,
            blueScoreText,
            blueScoreSlider,
            redScoreText,
            redScoreSlider);

        GameObject bluePlayer = FindOrCreateBluePlayer();

        if (bluePlayer != null)
        {
            ConfigureBluePlayer(bluePlayer);
        }

        EditorUtility.SetDirty(capturePoint);
        EditorUtility.DisplayDialog(
            "Setup Complete",
            "Capture point UI has been created and assigned. A Blue player has also been configured for local testing.",
            "OK");
    }

    [MenuItem("Tools/CA3/Move Blue Player To Capture Point")]
    public static void MoveBluePlayerToCapturePoint()
    {
        GameObject bluePlayer = FindOrCreateBluePlayer();

        if (bluePlayer == null)
        {
            EditorUtility.DisplayDialog(
                "Move Failed",
                "Could not find or create a Blue player.",
                "OK");

            return;
        }

        ConfigureBluePlayer(bluePlayer);

        Transform target = FindCaptureTarget();

        Vector3 destination = target != null
            ? target.position + Vector3.up
            : new Vector3(0f, 1f, -14f);

        Undo.RecordObject(bluePlayer.transform, "Move Blue Player To Capture Point");
        bluePlayer.transform.position = destination;
        EditorUtility.SetDirty(bluePlayer);

        EditorUtility.DisplayDialog(
            "Blue Player Moved",
            "The Blue player has been moved onto the capture point. Press Play to test Blue capturing the point.",
            "OK");
    }

    private static CapturePointLocal FindOrCreateCapturePoint()
    {
        CapturePointLocal capturePoint =
            Object.FindFirstObjectByType<CapturePointLocal>();

        if (capturePoint != null)
            return capturePoint;

        GameObject captureZone = GameObject.Find("CaptureZone_Trigger");

        if (captureZone == null)
            return null;

        BoxCollider boxCollider = captureZone.GetComponent<BoxCollider>();

        if (boxCollider == null)
            boxCollider = Undo.AddComponent<BoxCollider>(captureZone);

        boxCollider.isTrigger = true;

        capturePoint = Undo.AddComponent<CapturePointLocal>(captureZone);

        return capturePoint;
    }

    private static void AssignReferences(
        CapturePointLocal capturePoint,
        TMP_Text statusText,
        Slider captureSlider,
        TMP_Text blueScoreText,
        Slider blueScoreSlider,
        TMP_Text redScoreText,
        Slider redScoreSlider)
    {
        SerializedObject serialized = new SerializedObject(capturePoint);

        SetFloat(serialized, "captureDuration", 8f);
        SetFloat(serialized, "scoreDuration", 60f);
        SetInt(serialized, "maxCaptureSpeedPlayers", 3);
        SetBool(serialized, "startsLocked", false);
        SetFloat(serialized, "unlockDelay", 10f);

        SetObject(serialized, "statusText", statusText);
        SetObject(serialized, "captureSlider", captureSlider);

        SetObject(serialized, "blueScoreText", blueScoreText);
        SetObject(serialized, "blueScoreSlider", blueScoreSlider);

        SetObject(serialized, "redScoreText", redScoreText);
        SetObject(serialized, "redScoreSlider", redScoreSlider);

        Renderer beaconRenderer = FindRenderer("Capture_Beacon");
        Light captureLight = FindLight("Capture_Point_Light");

        if (beaconRenderer != null)
            SetObject(serialized, "beaconRenderer", beaconRenderer);

        if (captureLight != null)
            SetObject(serialized, "captureLight", captureLight);

        SetObject(serialized, "neutralMaterial", neutralMaterial);
        SetObject(serialized, "blueCapturingMaterial", blueCapturingMaterial);
        SetObject(serialized, "redCapturingMaterial", redCapturingMaterial);
        SetObject(serialized, "blueOwnedMaterial", blueOwnedMaterial);
        SetObject(serialized, "redOwnedMaterial", redOwnedMaterial);

        serialized.ApplyModifiedProperties();
    }

    private static void SetObject(SerializedObject serialized, string fieldName, Object value)
    {
        SerializedProperty property = serialized.FindProperty(fieldName);

        if (property != null)
            property.objectReferenceValue = value;
    }

    private static void SetFloat(SerializedObject serialized, string fieldName, float value)
    {
        SerializedProperty property = serialized.FindProperty(fieldName);

        if (property != null)
            property.floatValue = value;
    }

    private static void SetInt(SerializedObject serialized, string fieldName, int value)
    {
        SerializedProperty property = serialized.FindProperty(fieldName);

        if (property != null)
            property.intValue = value;
    }

    private static void SetBool(SerializedObject serialized, string fieldName, bool value)
    {
        SerializedProperty property = serialized.FindProperty(fieldName);

        if (property != null)
            property.boolValue = value;
    }

    private static Renderer FindRenderer(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<Renderer>() : null;
    }

    private static Light FindLight(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<Light>() : null;
    }

    private static Transform FindCaptureTarget()
    {
        GameObject target = GameObject.Find("CapturePoint_Center");

        if (target != null)
            return target.transform;

        target = GameObject.Find("CaptureZone_Trigger");

        if (target != null)
            return target.transform;

        return null;
    }

    private static void DeleteExistingUI()
    {
        GameObject existing = GameObject.Find(UIRootName);

        if (existing != null)
            Undo.DestroyObjectImmediate(existing);
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject(UIRootName);
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Capture Point UI");

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        CreateTopBar(canvas.transform);

        return canvas;
    }

    private static void CreateTopBar(Transform parent)
    {
        GameObject panel = new GameObject("TopHUD_Background");
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 120f);

        Image image = panel.AddComponent<Image>();
        Color color = ParseColor("#05070A");
        color.a = 0.72f;
        image.color = color;
    }

    private static void EnsureEventSystem()
    {
        EventSystem existing = Object.FindFirstObjectByType<EventSystem>();

        if (existing != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");

        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        string textValue,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        int fontSize,
        Color color,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = false;

        return text;
    }

    private static Slider CreateSlider(
        string name,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Color fillColor)
    {
        GameObject sliderObject = new GameObject(name);
        sliderObject.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.anchorMin = anchorMin;
        sliderRect.anchorMax = anchorMax;
        sliderRect.pivot = pivot;
        sliderRect.anchoredPosition = anchoredPosition;
        sliderRect.sizeDelta = sizeDelta;

        Image backgroundImage = sliderObject.AddComponent<Image>();
        Color backgroundColor = ParseColor("#161A20");
        backgroundColor.a = 0.95f;
        backgroundImage.color = backgroundColor;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.wholeNumbers = false;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.direction = Slider.Direction.LeftToRight;

        GameObject fillAreaObject = new GameObject("Fill Area");
        fillAreaObject.transform.SetParent(sliderObject.transform, false);

        RectTransform fillAreaRect = fillAreaObject.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(3f, 3f);
        fillAreaRect.offsetMax = new Vector2(-3f, -3f);

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(fillAreaObject.transform, false);

        RectTransform fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = fillColor;

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;

        Navigation navigation = slider.navigation;
        navigation.mode = Navigation.Mode.None;
        slider.navigation = navigation;

        return slider;
    }

    private static GameObject FindOrCreateBluePlayer()
    {
        GameObject player = FindExistingPlayer();

        if (player != null)
            return player;

        Transform spawn = FindSpawnPoint();

        Vector3 spawnPosition = spawn != null
            ? spawn.position
            : new Vector3(-44f, 1f, -3f);

        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "Blue_Test_Player";
        capsule.transform.position = spawnPosition;

        Undo.RegisterCreatedObjectUndo(capsule, "Create Blue Test Player");

        Rigidbody rigidbody = capsule.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;

        return capsule;
    }

    private static GameObject FindExistingPlayer()
    {
        GameObject taggedPlayer = null;

        try
        {
            taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        }
        catch
        {
            taggedPlayer = null;
        }

        if (taggedPlayer != null)
            return taggedPlayer;

        GameObject selected = Selection.activeGameObject;

        if (selected == null)
            return null;

        string selectedName = selected.name.ToLowerInvariant();

        bool looksLikePlayer =
            selectedName.Contains("player") ||
            selected.GetComponent<CharacterController>() != null ||
            selected.GetComponentInChildren<Camera>() != null ||
            selected.GetComponent<Rigidbody>() != null;

        return looksLikePlayer ? selected : null;
    }

    private static void ConfigureBluePlayer(GameObject player)
    {
        Undo.RecordObject(player.transform, "Configure Blue Player");

        OutpostPlayerTeam team = player.GetComponent<OutpostPlayerTeam>();

        if (team == null)
            team = Undo.AddComponent<OutpostPlayerTeam>(player);

        Undo.RecordObject(team, "Set Player Team");
        team.SetTeam(OutpostTeam.Blue);
        EditorUtility.SetDirty(team);

        TrySetPlayerTag(player);

        bool hasCollider = player.GetComponent<Collider>() != null || player.GetComponentInChildren<Collider>() != null;
        bool hasCharacterController = player.GetComponent<CharacterController>() != null;
        bool hasRigidbody = player.GetComponent<Rigidbody>() != null;

        if (!hasCollider && !hasCharacterController)
            Undo.AddComponent<CapsuleCollider>(player);

        if (!hasRigidbody && !hasCharacterController)
        {
            Rigidbody rigidbody = Undo.AddComponent<Rigidbody>(player);
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }

        Transform spawn = FindSpawnPoint();

        if (spawn != null)
        {
            Undo.RecordObject(player.transform, "Move Blue Player To Spawn");
            player.transform.position = spawn.position;
        }

        EditorUtility.SetDirty(player);
    }

    private static void TrySetPlayerTag(GameObject player)
    {
        try
        {
            player.tag = "Player";
        }
        catch
        {
            // Player tag should normally exist in Unity.
            // If it does not, the capture logic still works because it uses OutpostPlayerTeam.
        }
    }

    private static Transform FindSpawnPoint()
    {
        GameObject spawn = GameObject.Find("PlayerSpawn_Blue_01");

        if (spawn != null)
            return spawn.transform;

        spawn = GameObject.Find("PlayerSpawn_A");

        if (spawn != null)
            return spawn.transform;

        return null;
    }

    private static void CreateMaterials()
    {
        EnsureFolder("Assets", "Generated");
        EnsureFolder("Assets/Generated", "OutpostCaptureMaterials");

        neutralMaterial = CreateOrLoadMaterial("MAT_Point_Neutral", "#8A8A8A");
        blueCapturingMaterial = CreateOrLoadMaterial("MAT_Blue_Capturing", "#4DA3FF", true, "#4DA3FF", 1.2f);
        redCapturingMaterial = CreateOrLoadMaterial("MAT_Red_Capturing", "#FF4D6D", true, "#FF4D6D", 1.2f);
        blueOwnedMaterial = CreateOrLoadMaterial("MAT_Blue_Owned", "#1E6BFF", true, "#1E6BFF", 1.5f);
        redOwnedMaterial = CreateOrLoadMaterial("MAT_Red_Owned", "#D82F5E", true, "#D82F5E", 1.5f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";

        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static Material CreateOrLoadMaterial(
        string materialName,
        string hex,
        bool emission = false,
        string emissionHex = "#000000",
        float emissionIntensity = 1f)
    {
        string path = $"{MaterialFolder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        Color color = ParseColor(hex);

        material.color = color;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        if (emission)
        {
            Color emissionColor = ParseColor(emissionHex) * emissionIntensity;
            material.EnableKeyword("_EMISSION");

            if (material.HasProperty("_EmissionColor"))
                material.SetColor("_EmissionColor", emissionColor);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Color ParseColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;

        return Color.magenta;
    }
}