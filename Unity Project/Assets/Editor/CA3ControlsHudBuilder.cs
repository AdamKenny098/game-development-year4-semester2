using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CA3ControlsHudBuilder
{
    private const string HudName = "CA3_Controls_HUD";

    [MenuItem("Tools/CA3/Add Controls HUD")]
    public static void AddControlsHud()
    {
        Canvas canvas = GetOrCreateCanvas();

        Transform existing = canvas.transform.Find(HudName);

        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        GameObject hud = new GameObject(HudName);
        hud.transform.SetParent(canvas.transform, false);

        RectTransform hudRect = hud.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0.5f, 0f);
        hudRect.anchorMax = new Vector2(0.5f, 0f);
        hudRect.pivot = new Vector2(0.5f, 0f);
        hudRect.anchoredPosition = new Vector2(0f, 18f);
        hudRect.sizeDelta = new Vector2(980f, 82f);

        Image background = hud.AddComponent<Image>();
        background.color = new Color(0.03f, 0.035f, 0.045f, 0.82f);

        AddText(
            hud.transform,
            "CONTROLS",
            new Vector2(0f, 23f),
            new Vector2(940f, 26f),
            18,
            FontStyles.Bold,
            new Color(0.95f, 0.95f, 0.95f, 1f)
        );

        AddText(
            hud.transform,
            "WASD: Move Player     RMB + Mouse: Orbit Camera     MMB + Mouse: Pan Camera     Scroll: Zoom     Objective: Stand on Point     Guard Contact: Return to Spawn",
            new Vector2(0f, -10f),
            new Vector2(940f, 32f),
            14,
            FontStyles.Normal,
            new Color(0.82f, 0.85f, 0.9f, 1f)
        );

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log("[CA3ControlsHudBuilder] Controls HUD added to Canvas.");
    }

    private static Canvas GetOrCreateCanvas()
    {
        Canvas existingCanvas = Object.FindFirstObjectByType<Canvas>();

        if (existingCanvas != null)
            return existingCanvas;

        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvas;
    }

    private static void AddText(
        Transform parent,
        string text,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize,
        FontStyles fontStyle,
        Color color)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = textObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = fontStyle;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
    }
}