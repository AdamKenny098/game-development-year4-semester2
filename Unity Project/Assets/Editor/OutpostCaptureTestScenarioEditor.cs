using UnityEditor;
using UnityEngine;

public static class OutpostCaptureTestScenarioEditor
{
    private const string BluePlayerName = "Blue_Test_Player";
    private const string RedPlayerName = "Red_Test_Player";
    private const string MaterialFolder = "Assets/Generated/OutpostCaptureMaterials";

    [MenuItem("Tools/CA3/Test/Setup Red Test Player")]
    public static void SetupRedTestPlayer()
    {
        GameObject redPlayer = FindOrCreatePlayer(
            RedPlayerName,
            OutpostTeam.Red,
            FindSpawnPosition("PlayerSpawn_Red_01", new Vector3(44f, 1f, 3f)),
            "#D82F5E"
        );

        Selection.activeGameObject = redPlayer;

        EditorUtility.DisplayDialog(
            "Red Test Player Ready",
            "Red_Test_Player has been created/configured. Move it onto the point to test Red capture or contesting.",
            "OK"
        );
    }

    [MenuItem("Tools/CA3/Test/Move Red Player To Capture Point")]
    public static void MoveRedPlayerToCapturePoint()
    {
        GameObject redPlayer = FindOrCreatePlayer(
            RedPlayerName,
            OutpostTeam.Red,
            FindSpawnPosition("PlayerSpawn_Red_01", new Vector3(44f, 1f, 3f)),
            "#D82F5E"
        );

        MoveObject(redPlayer, FindCapturePosition());
        Selection.activeGameObject = redPlayer;
    }

    [MenuItem("Tools/CA3/Test/Move Blue Player To Blue Spawn")]
    public static void MoveBluePlayerToBlueSpawn()
    {
        GameObject bluePlayer = GameObject.Find(BluePlayerName);

        if (bluePlayer == null)
        {
            bluePlayer = FindExistingBluePlayer();
        }

        if (bluePlayer == null)
        {
            EditorUtility.DisplayDialog(
                "Blue Player Not Found",
                "Could not find Blue_Test_Player or an existing Blue player.",
                "OK"
            );

            return;
        }

        MoveObject(bluePlayer, FindSpawnPosition("PlayerSpawn_Blue_01", new Vector3(-44f, 1f, -3f)));
        Selection.activeGameObject = bluePlayer;
    }

    [MenuItem("Tools/CA3/Test/Setup Fast Red Capture Test")]
    public static void SetupFastRedCaptureTest()
    {
        SetFastCaptureTimings();

        GameObject redPlayer = FindOrCreatePlayer(
            RedPlayerName,
            OutpostTeam.Red,
            FindSpawnPosition("PlayerSpawn_Red_01", new Vector3(44f, 1f, 3f)),
            "#D82F5E"
        );

        GameObject bluePlayer = GameObject.Find(BluePlayerName);

        if (bluePlayer == null)
            bluePlayer = FindExistingBluePlayer();

        if (bluePlayer != null)
            MoveObject(bluePlayer, FindSpawnPosition("PlayerSpawn_Blue_01", new Vector3(-44f, 1f, -3f)));

        MoveObject(redPlayer, FindCapturePosition());

        Selection.activeGameObject = redPlayer;

        EditorUtility.DisplayDialog(
            "Fast Red Capture Test Ready",
            "Red is on the point, Blue is moved out, and capture timings are fast. Press Play to test Red capturing and reaching 100%.",
            "OK"
        );
    }

    [MenuItem("Tools/CA3/Test/Set Fast Capture Timings")]
    public static void SetFastCaptureTimings()
    {
        CapturePointLocal capturePoint = Object.FindFirstObjectByType<CapturePointLocal>();

        if (capturePoint == null)
        {
            EditorUtility.DisplayDialog(
                "Capture Point Not Found",
                "Could not find CapturePointLocal in the scene.",
                "OK"
            );

            return;
        }

        SerializedObject serialized = new SerializedObject(capturePoint);

        SetFloat(serialized, "captureDuration", 2f);
        SetFloat(serialized, "scoreDuration", 6f);
        SetInt(serialized, "maxCaptureSpeedPlayers", 3);
        SetBool(serialized, "startsLocked", false);

        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(capturePoint);

        Debug.Log("[CA3 Test] Fast capture timings set: captureDuration = 2, scoreDuration = 6");
    }

    [MenuItem("Tools/CA3/Test/Set Submission Capture Timings")]
    public static void SetSubmissionCaptureTimings()
    {
        CapturePointLocal capturePoint = Object.FindFirstObjectByType<CapturePointLocal>();

        if (capturePoint == null)
        {
            EditorUtility.DisplayDialog(
                "Capture Point Not Found",
                "Could not find CapturePointLocal in the scene.",
                "OK"
            );

            return;
        }

        SerializedObject serialized = new SerializedObject(capturePoint);

        SetFloat(serialized, "captureDuration", 8f);
        SetFloat(serialized, "scoreDuration", 60f);
        SetInt(serialized, "maxCaptureSpeedPlayers", 3);
        SetBool(serialized, "startsLocked", false);

        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(capturePoint);

        Debug.Log("[CA3 Test] Submission capture timings restored: captureDuration = 8, scoreDuration = 60");
    }

    private static GameObject FindOrCreatePlayer(string name, OutpostTeam team, Vector3 position, string colourHex)
    {
        GameObject player = GameObject.Find(name);

        if (player == null)
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = name;
            player.transform.position = position;

            Undo.RegisterCreatedObjectUndo(player, $"Create {name}");

            Rigidbody rb = player.GetComponent<Rigidbody>();

            if (rb == null)
                rb = player.AddComponent<Rigidbody>();

            rb.isKinematic = true;
            rb.useGravity = false;
        }

        ConfigurePlayer(player, team, colourHex);
        return player;
    }

    private static void ConfigurePlayer(GameObject player, OutpostTeam team, string colourHex)
    {
        OutpostPlayerTeam playerTeam = player.GetComponent<OutpostPlayerTeam>();

        if (playerTeam == null)
            playerTeam = Undo.AddComponent<OutpostPlayerTeam>(player);

        playerTeam.SetTeam(team);
        EditorUtility.SetDirty(playerTeam);

        TrySetPlayerTag(player);

        Collider collider = player.GetComponent<Collider>();

        if (collider == null)
            Undo.AddComponent<CapsuleCollider>(player);

        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb == null)
            rb = Undo.AddComponent<Rigidbody>(player);

        rb.isKinematic = true;
        rb.useGravity = false;

        Renderer renderer = player.GetComponent<Renderer>();

        if (renderer != null)
            renderer.sharedMaterial = CreateOrLoadMaterial($"MAT_{team}_Test_Player", colourHex);
    }

    private static GameObject FindExistingBluePlayer()
    {
        OutpostPlayerTeam[] teams = Object.FindObjectsByType<OutpostPlayerTeam>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (OutpostPlayerTeam team in teams)
        {
            if (team.Team == OutpostTeam.Blue)
                return team.gameObject;
        }

        return null;
    }

    private static Vector3 FindSpawnPosition(string spawnName, Vector3 fallback)
    {
        GameObject spawn = GameObject.Find(spawnName);
        return spawn != null ? spawn.transform.position : fallback;
    }

    private static Vector3 FindCapturePosition()
    {
        GameObject captureCenter = GameObject.Find("CapturePoint_Center");

        if (captureCenter != null)
            return captureCenter.transform.position + Vector3.up;

        GameObject captureZone = GameObject.Find("CaptureZone_Trigger");

        if (captureZone != null)
            return captureZone.transform.position + Vector3.up;

        return new Vector3(0f, 1f, -14f);
    }

    private static void MoveObject(GameObject obj, Vector3 position)
    {
        Undo.RecordObject(obj.transform, $"Move {obj.name}");
        obj.transform.position = position;
        EditorUtility.SetDirty(obj);
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

    private static void TrySetPlayerTag(GameObject player)
    {
        try
        {
            player.tag = "Player";
        }
        catch
        {
            // Ignore if the Player tag does not exist.
        }
    }

    private static Material CreateOrLoadMaterial(string materialName, string hex)
    {
        EnsureFolder("Assets", "Generated");
        EnsureFolder("Assets/Generated", "OutpostCaptureMaterials");

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

        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();

        return material;
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";

        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static Color ParseColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;

        return Color.magenta;
    }
}