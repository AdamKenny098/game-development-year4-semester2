using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class TyrantHUDBuildEditor
{
    private const string CanvasName = "CA3_Tyrant_HUD";
    private const string OverlayName = "Tyrant_Debug_Overlay";

    [MenuItem("Tools/CA3/Build Tyrant HUD")]
    public static void BuildHud()
    {
        GameObject existing = GameObject.Find(CanvasName);

        if (existing != null)
        {
            Selection.activeGameObject = existing;
            Debug.LogWarning($"[Tyrant HUD Builder] '{CanvasName}' already exists. Delete it first if you want to rebuild it.");
            return;
        }

        GameObject canvasObject = new GameObject(CanvasName);
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Tyrant HUD");

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        GameObject overlayObject = CreateUIObject(OverlayName, canvasObject.transform);
        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = new Vector2(0f, 1f);
        overlayRect.anchorMax = new Vector2(0f, 1f);
        overlayRect.pivot = new Vector2(0f, 1f);
        overlayRect.anchoredPosition = new Vector2(20f, -20f);
        overlayRect.sizeDelta = new Vector2(620f, 720f);

        Image overlayBackground = overlayObject.AddComponent<Image>();
        overlayBackground.color = new Color(0f, 0f, 0f, 0.72f);

        VerticalLayoutGroup layout = overlayObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.spacing = 8;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        CreateTextBlock(
            "Title",
            overlayObject.transform,
            "CA3 TYRANT AI DEBUG",
            28,
            FontStyles.Bold,
            new Color(1f, 0.85f, 0.45f, 1f),
            46
        );

        TMP_Text behaviourText = CreateTextBlock(
            "Behaviour_Text",
            overlayObject.transform,
            "=== TYRANT BEHAVIOUR ===\nState: -\nBranch: -\nTask: -",
            21,
            FontStyles.Normal,
            Color.white,
            120
        );

        TMP_Text perceptionText = CreateTextBlock(
            "Perception_Text",
            overlayObject.transform,
            "=== PERCEPTION ===\nCan See Player: -\nHears Noise: -",
            21,
            FontStyles.Normal,
            Color.white,
            130
        );

        TMP_Text blackboardText = CreateTextBlock(
            "Blackboard_Text",
            overlayObject.transform,
            "=== BLACKBOARD ===\nTarget: None\nLast Known Player: -\nSearch Target: -",
            21,
            FontStyles.Normal,
            Color.white,
            220
        );

        TMP_Text navText = CreateTextBlock(
            "Nav_Text",
            overlayObject.transform,
            "=== NAVMESH ===\nHas Path: -\nPath Status: -\nDestination: -",
            21,
            FontStyles.Normal,
            Color.white,
            180
        );

        GameObject objectiveObject = CreateUIObject("Objective_Panel", canvasObject.transform);
        RectTransform objectiveRect = objectiveObject.GetComponent<RectTransform>();
        objectiveRect.anchorMin = new Vector2(1f, 1f);
        objectiveRect.anchorMax = new Vector2(1f, 1f);
        objectiveRect.pivot = new Vector2(1f, 1f);
        objectiveRect.anchoredPosition = new Vector2(-20f, -20f);
        objectiveRect.sizeDelta = new Vector2(520f, 150f);

        Image objectiveBackground = objectiveObject.AddComponent<Image>();
        objectiveBackground.color = new Color(0f, 0f, 0f, 0.65f);

        TMP_Text objectiveText = CreateTextBlock(
            "Objective_Text",
            objectiveObject.transform,
            "OBJECTIVE\nSurvive the Tyrant encounter.\nObserve Patrol, Search, Chase, and Attack states.",
            24,
            FontStyles.Bold,
            Color.white,
            140
        );

        RectTransform objectiveTextRect = objectiveText.GetComponent<RectTransform>();
        objectiveTextRect.anchorMin = Vector2.zero;
        objectiveTextRect.anchorMax = Vector2.one;
        objectiveTextRect.offsetMin = new Vector2(16f, 10f);
        objectiveTextRect.offsetMax = new Vector2(-16f, -10f);

        OverlayBT overlay = overlayObject.AddComponent<OverlayBT>();
        WireOverlayFields(overlay, behaviourText, perceptionText, blackboardText, navText);

        Selection.activeGameObject = canvasObject;
        Debug.Log("[Tyrant HUD Builder] CA3 Tyrant HUD created and OverlayBT fields wired.");
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private static TMP_Text CreateTextBlock(
        string name,
        Transform parent,
        string startingText,
        int fontSize,
        FontStyles fontStyle,
        Color color,
        float preferredHeight)
    {
        GameObject textObject = CreateUIObject(name, parent);

        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = startingText;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableWordWrapping = true;
        text.raycastTarget = false;

        LayoutElement layout = textObject.AddComponent<LayoutElement>();
        layout.preferredHeight = preferredHeight;

        return text;
    }

    private static void WireOverlayFields(
        OverlayBT overlay,
        TMP_Text behaviourText,
        TMP_Text perceptionText,
        TMP_Text blackboardText,
        TMP_Text navText)
    {
        SerializedObject serializedOverlay = new SerializedObject(overlay);

        serializedOverlay.FindProperty("behaviourText").objectReferenceValue = behaviourText;
        serializedOverlay.FindProperty("perceptionText").objectReferenceValue = perceptionText;
        serializedOverlay.FindProperty("blackboardText").objectReferenceValue = blackboardText;
        serializedOverlay.FindProperty("navText").objectReferenceValue = navText;

        serializedOverlay.ApplyModifiedProperties();
        EditorUtility.SetDirty(overlay);
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");

        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }
}
