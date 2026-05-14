using UnityEngine;

public class SimpleAIDebugOverlay : MonoBehaviour
{
    [SerializeField] private SimpleWardenAI warden;
    [SerializeField] private bool showOverlay = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;

    private GUIStyle style;

    private void Awake()
    {
        if (warden == null)
            warden = FindObjectOfType<SimpleWardenAI>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            showOverlay = !showOverlay;
    }

    private void OnGUI()
    {
        if (!showOverlay || warden == null)
            return;

        if (style == null)
        {
            style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            style.padding = new RectOffset(12, 12, 12, 12);
        }

        string text =
            "AI DEBUG OVERLAY\n" +
            "-------------------------\n" +
            $"State / BT Branch: {warden.CurrentState}\n" +
            $"Goal: {warden.CurrentGoal}\n\n" +
            "BLACKBOARD\n" +
            $"Can See Player: {warden.CanSeePlayer}\n" +
            $"Can Hear Noise: {warden.CanHearNoise}\n" +
            $"Has Last Known Pos: {warden.HasLastKnownPlayerPosition}\n" +
            $"Last Known Pos: {warden.LastKnownPlayerPosition}\n\n" +
            "NAVIGATION\n" +
            $"Destination: {warden.CurrentDestination}\n" +
            $"Remaining Distance: {warden.RemainingDistance:0.00}\n" +
            $"Stuck: {warden.IsStuck}\n\n" +
            "CONTROLS\n" +
            "WASD - Move\n" +
            "Mouse - Look\n" +
            "Y - Emit Noise\n" +
            "Left Click - Damage Warden\n" +
            "F1 - Toggle Overlay";

        GUI.Box(new Rect(20, 20, 430, 340), text, style);
    }
}
