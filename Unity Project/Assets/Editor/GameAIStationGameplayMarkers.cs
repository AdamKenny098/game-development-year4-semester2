using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GameAIStationGameplayMarkers
{
    private const string ShellRootName = "GameAI_CA3_Station_LevelShell";
    private const string MarkerRootName = "GameAI_CA3_GameplayMarkers";

    private const string GeneratedMaterialFolder = "Assets/GameAI_CA3/Materials/Generated";

    private static readonly Color PlayerSpawnColor = Hex("#2E86AB");
    private static readonly Color WardenSpawnColor = Hex("#D94F4F");
    private static readonly Color MedalColor = Hex("#D6B85A");
    private static readonly Color ExitColor = Hex("#B33A3A");
    private static readonly Color PatrolColor = Hex("#7DCE82");
    private static readonly Color SearchColor = Hex("#F2C14E");
    private static readonly Color FleeColor = Hex("#8E6AD8");
    private static readonly Color NoiseColor = Hex("#E68A3A");
    private static readonly Color ObjectiveColor = Hex("#C9C9C9");

    private static Transform root;
    private static Transform spawnsRoot;
    private static Transform objectivesRoot;
    private static Transform aiRoot;
    private static Transform patrolRoot;
    private static Transform searchRoot;
    private static Transform fleeRoot;
    private static Transform utilityRoot;

    private static readonly List<GameObject> createdObjects = new List<GameObject>();

    [MenuItem("Tools/Game AI CA3/Station Gameplay/Generate Gameplay Markers")]
    public static void GenerateGameplayMarkers()
    {
        if (!ValidateShellExists())
            return;

        GameObject existing = GameObject.Find(MarkerRootName);

        if (existing != null)
        {
            bool replace = EditorUtility.DisplayDialog(
                "Replace Gameplay Markers?",
                "A gameplay marker root already exists in this scene. Replace it?",
                "Replace",
                "Cancel"
            );

            if (!replace)
                return;

            UnityEngine.Object.DestroyImmediate(existing);
        }

        createdObjects.Clear();

        CreateRootHierarchy();
        CreateSpawnMarkers();
        CreateObjectiveMarkers();
        CreatePatrolMarkers();
        CreateSearchMarkers();
        CreateFleeMarkers();
        CreateUtilityMarkers();
        CreateReadmeObject();
        FinaliseGeneratedObjects();

        Selection.activeGameObject = root.gameObject;

        Debug.Log("[Game AI CA3] Gameplay markers generated successfully.");
    }

    [MenuItem("Tools/Game AI CA3/Station Gameplay/Clear Gameplay Markers")]
    public static void ClearGameplayMarkers()
    {
        GameObject existing = GameObject.Find(MarkerRootName);

        if (existing == null)
        {
            Debug.Log("[Game AI CA3] No gameplay marker root found to clear.");
            return;
        }

        UnityEngine.Object.DestroyImmediate(existing);
        Debug.Log("[Game AI CA3] Gameplay markers cleared.");
    }

    [MenuItem("Tools/Game AI CA3/Station Gameplay/Select Gameplay Marker Root")]
    public static void SelectGameplayMarkerRoot()
    {
        GameObject existing = GameObject.Find(MarkerRootName);

        if (existing == null)
        {
            EditorUtility.DisplayDialog(
                "Gameplay Markers Not Found",
                "No GameAI_CA3_GameplayMarkers object exists in the current scene.",
                "OK"
            );
            return;
        }

        Selection.activeGameObject = existing;
    }

    private static bool ValidateShellExists()
    {
        GameObject shellRoot = GameObject.Find(ShellRootName);

        if (shellRoot != null)
            return true;

        bool continueAnyway = EditorUtility.DisplayDialog(
            "Station Shell Not Found",
            $"Could not find {ShellRootName} in the current scene.\n\nGenerate the station shell first, or continue if the shell has been renamed.",
            "Continue Anyway",
            "Cancel"
        );

        return continueAnyway;
    }

    private static void CreateRootHierarchy()
    {
        root = new GameObject(MarkerRootName).transform;

        spawnsRoot = CreateChildRoot(root, "Spawns");
        objectivesRoot = CreateChildRoot(root, "Objectives");
        aiRoot = CreateChildRoot(root, "AI_Route_Data");
        patrolRoot = CreateChildRoot(aiRoot, "PatrolPoints");
        searchRoot = CreateChildRoot(aiRoot, "SearchPoints");
        fleeRoot = CreateChildRoot(aiRoot, "FleePoints");
        utilityRoot = CreateChildRoot(root, "UtilityMarkers");
    }

    private static Transform CreateChildRoot(Transform parent, string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        createdObjects.Add(obj);
        return obj.transform;
    }

    private static void CreateSpawnMarkers()
    {
        CreateMarker(new MarkerDefinition
        {
            Name = "PlayerSpawn",
            Parent = spawnsRoot,
            Position = new Vector3(0f, 0.08f, -44f),
            RotationY = 0f,
            Shape = MarkerShape.Cylinder,
            Scale = new Vector3(1.5f, 0.16f, 1.5f),
            Color = PlayerSpawnColor,
            Description = "Player starts at the southern entrance facing into the main hall."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "WardenSpawn",
            Parent = spawnsRoot,
            Position = new Vector3(0f, 0.08f, -10f),
            RotationY = 180f,
            Shape = MarkerShape.Cylinder,
            Scale = new Vector3(1.5f, 0.16f, 1.5f),
            Color = WardenSpawnColor,
            Description = "Main Warden enemy spawn. Place the AI prefab here after baking NavMesh."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "OptionalSecondEnemySpawn_DisabledForCA3",
            Parent = spawnsRoot,
            Position = new Vector3(52f, 0.08f, -18f),
            RotationY = 270f,
            Shape = MarkerShape.Cube,
            Scale = new Vector3(1f, 0.1f, 1f),
            Color = WardenSpawnColor * 0.65f,
            Description = "Optional reference only. Do not use unless the final demo is already stable."
        });
    }

    private static void CreateObjectiveMarkers()
    {
        CreateMarker(new MarkerDefinition
        {
            Name = "ExitDoor",
            Parent = objectivesRoot,
            Position = new Vector3(0f, 1.55f, -49.85f),
            RotationY = 0f,
            Shape = MarkerShape.Cube,
            Scale = new Vector3(6.6f, 3.1f, 0.32f),
            Color = ExitColor,
            KeepCollider = true,
            Description = "Locked main exit. The player escapes here after collecting all three medals."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "ExitInteractionPoint",
            Parent = objectivesRoot,
            Position = new Vector3(0f, 0.08f, -46.5f),
            RotationY = 0f,
            Shape = MarkerShape.Cylinder,
            Scale = new Vector3(2.4f, 0.12f, 2.4f),
            Color = ExitColor,
            Description = "Trigger/interaction location for checking whether all medals have been collected."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "Medal_01_WestOffice",
            Parent = objectivesRoot,
            Position = new Vector3(-68f, 0.75f, -20f),
            RotationY = 0f,
            Shape = MarkerShape.Sphere,
            Scale = new Vector3(0.9f, 0.9f, 0.9f),
            Color = MedalColor,
            KeepCollider = true,
            TriggerCollider = true,
            Description = "First medal. Far west office. Good early objective after leaving main hall."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "Medal_02_WestStorage",
            Parent = objectivesRoot,
            Position = new Vector3(-44f, 0.75f, 32f),
            RotationY = 0f,
            Shape = MarkerShape.Sphere,
            Scale = new Vector3(0.9f, 0.9f, 0.9f),
            Color = MedalColor,
            KeepCollider = true,
            TriggerCollider = true,
            Description = "Second medal. Upper west storage. Forces the player through the north-west corridor."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "Medal_03_RecordsEast",
            Parent = objectivesRoot,
            Position = new Vector3(32f, 0.75f, 32f),
            RotationY = 0f,
            Shape = MarkerShape.Sphere,
            Scale = new Vector3(0.9f, 0.9f, 0.9f),
            Color = MedalColor,
            KeepCollider = true,
            TriggerCollider = true,
            Description = "Third medal. East records room. Creates a full-station route before returning to the exit."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "MedalPedestal_01_WestOffice",
            Parent = objectivesRoot,
            Position = new Vector3(-68f, 0.25f, -20f),
            RotationY = 0f,
            Shape = MarkerShape.Cube,
            Scale = new Vector3(1.8f, 0.5f, 1.8f),
            Color = ObjectiveColor,
            Description = "Pedestal for Medal 01."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "MedalPedestal_02_WestStorage",
            Parent = objectivesRoot,
            Position = new Vector3(-44f, 0.25f, 32f),
            RotationY = 0f,
            Shape = MarkerShape.Cube,
            Scale = new Vector3(1.8f, 0.5f, 1.8f),
            Color = ObjectiveColor,
            Description = "Pedestal for Medal 02."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "MedalPedestal_03_RecordsEast",
            Parent = objectivesRoot,
            Position = new Vector3(32f, 0.25f, 32f),
            RotationY = 0f,
            Shape = MarkerShape.Cube,
            Scale = new Vector3(1.8f, 0.5f, 1.8f),
            Color = ObjectiveColor,
            Description = "Pedestal for Medal 03."
        });
    }

    private static void CreatePatrolMarkers()
    {
        CreatePatrolPoint("PatrolPoint_A_MainHallSouth", new Vector3(0f, 0.08f, -30f), 0f, "South main hall patrol point. The Warden should pass near the entrance sightline.");
        CreatePatrolPoint("PatrolPoint_B_MainHallNorth", new Vector3(0f, 0.08f, 4f), 180f, "North main hall patrol point near the gallery connection.");
        CreatePatrolPoint("PatrolPoint_C_WestAccess", new Vector3(-28f, 0.08f, -18f), 270f, "West access corridor patrol point.");
        CreatePatrolPoint("PatrolPoint_D_WestLongCorridor", new Vector3(-52f, 0.08f, -18f), 90f, "Long west corridor patrol point. Good for hearing investigation and chase routing.");
        CreatePatrolPoint("PatrolPoint_E_WestNorthCorridor", new Vector3(-40f, 0.08f, 20f), 90f, "Upper west corridor patrol point.");
        CreatePatrolPoint("PatrolPoint_F_NorthGallery", new Vector3(0f, 0.08f, 20f), 180f, "Central north gallery patrol point.");
        CreatePatrolPoint("PatrolPoint_G_EastNorthCorridor", new Vector3(40f, 0.08f, 20f), 270f, "Upper east corridor patrol point.");
        CreatePatrolPoint("PatrolPoint_H_EastLongCorridor", new Vector3(48f, 0.08f, -18f), 270f, "East corridor patrol point.");
        CreatePatrolPoint("PatrolPoint_I_ReturnToMainHall", new Vector3(16f, 0.08f, -18f), 90f, "Return point from east wing back into the main hall route.");
    }

    private static void CreatePatrolPoint(string name, Vector3 position, float rotationY, string description)
    {
        CreateMarker(new MarkerDefinition
        {
            Name = name,
            Parent = patrolRoot,
            Position = position,
            RotationY = rotationY,
            Shape = MarkerShape.Cylinder,
            Scale = new Vector3(0.85f, 0.12f, 0.85f),
            Color = PatrolColor,
            Description = description
        });
    }

    private static void CreateSearchMarkers()
    {
        CreateSearchPoint("SearchPoint_MainHall", new Vector3(0f, 0.08f, -12f), "Central search point for lost target recovery in the main hall.");
        CreateSearchPoint("SearchPoint_Entrance", new Vector3(0f, 0.08f, -40f), "Search point near the entrance if the player breaks line of sight near the exit.");
        CreateSearchPoint("SearchPoint_WestOffice", new Vector3(-68f, 0.08f, -20f), "Search point in the west office near Medal 01.");
        CreateSearchPoint("SearchPoint_DarkRoom", new Vector3(-68f, 0.08f, -40f), "Search point in the lower west dark room.");
        CreateSearchPoint("SearchPoint_EvidenceRoom", new Vector3(-36f, 0.08f, -40f), "Search point in the evidence room / lower west side room.");
        CreateSearchPoint("SearchPoint_Operations", new Vector3(-68f, 0.08f, 24f), "Search point in the upper west operations room.");
        CreateSearchPoint("SearchPoint_WestStorage", new Vector3(-44f, 0.08f, 32f), "Search point near Medal 02.");
        CreateSearchPoint("SearchPoint_RecordsEast", new Vector3(32f, 0.08f, 32f), "Search point near Medal 03.");
        CreateSearchPoint("SearchPoint_WaitingRoom", new Vector3(72f, 0.08f, 8f), "Search point in the eastern waiting room.");
        CreateSearchPoint("SearchPoint_EastOffice", new Vector3(72f, 0.08f, -24f), "Search point in the lower east office.");
    }

    private static void CreateSearchPoint(string name, Vector3 position, string description)
    {
        CreateMarker(new MarkerDefinition
        {
            Name = name,
            Parent = searchRoot,
            Position = position,
            RotationY = 0f,
            Shape = MarkerShape.Cylinder,
            Scale = new Vector3(0.8f, 0.1f, 0.8f),
            Color = SearchColor,
            Description = description
        });
    }

    private static void CreateFleeMarkers()
    {
        CreateFleePoint("FleePoint_WestStorage", new Vector3(-44f, 0.08f, 34f), "Primary flee point. Upper west storage gives the Warden distance from the player.");
        CreateFleePoint("FleePoint_Operations", new Vector3(-72f, 0.08f, 28f), "Secondary flee point in the upper west operations room.");
        CreateFleePoint("FleePoint_EastStorage", new Vector3(88f, 0.08f, 12f), "Backup flee point on the east side if the Warden is chased across the map.");
    }

    private static void CreateFleePoint(string name, Vector3 position, string description)
    {
        CreateMarker(new MarkerDefinition
        {
            Name = name,
            Parent = fleeRoot,
            Position = position,
            RotationY = 0f,
            Shape = MarkerShape.Cylinder,
            Scale = new Vector3(1f, 0.14f, 1f),
            Color = FleeColor,
            Description = description
        });
    }

    private static void CreateUtilityMarkers()
    {
        CreateMarker(new MarkerDefinition
        {
            Name = "NoiseDemoZone_MainHallWest",
            Parent = utilityRoot,
            Position = new Vector3(-16f, 0.07f, -28f),
            RotationY = 0f,
            Shape = MarkerShape.Cylinder,
            Scale = new Vector3(2f, 0.1f, 2f),
            Color = NoiseColor,
            Description = "Stand here and trigger player noise to demonstrate hearing perception."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "LineOfSightBreak_WestCorner",
            Parent = utilityRoot,
            Position = new Vector3(-28f, 0.07f, -18f),
            RotationY = 0f,
            Shape = MarkerShape.Cube,
            Scale = new Vector3(1.2f, 0.1f, 1.2f),
            Color = SearchColor,
            Description = "Recommended corner for breaking line of sight during the video demo."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "LineOfSightBreak_EastCorner",
            Parent = utilityRoot,
            Position = new Vector3(28f, 0.07f, -18f),
            RotationY = 0f,
            Shape = MarkerShape.Cube,
            Scale = new Vector3(1.2f, 0.1f, 1.2f),
            Color = SearchColor,
            Description = "Backup line-of-sight break corner on the east side."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "NavMeshDemoRoute_Start",
            Parent = utilityRoot,
            Position = new Vector3(-52f, 0.07f, -18f),
            RotationY = 0f,
            Shape = MarkerShape.Cube,
            Scale = new Vector3(0.9f, 0.1f, 0.9f),
            Color = ObjectiveColor,
            Description = "Use with NavMeshDemoRoute_End to show pathing through corridors rather than direct movement."
        });

        CreateMarker(new MarkerDefinition
        {
            Name = "NavMeshDemoRoute_End",
            Parent = utilityRoot,
            Position = new Vector3(48f, 0.07f, -18f),
            RotationY = 0f,
            Shape = MarkerShape.Cube,
            Scale = new Vector3(0.9f, 0.1f, 0.9f),
            Color = ObjectiveColor,
            Description = "Use with NavMeshDemoRoute_Start to show route cost through the station corridors."
        });
    }

    private static void CreateReadmeObject()
    {
        GameObject readme = new GameObject("README_MARKER_USAGE");
        readme.transform.SetParent(root);
        readme.transform.localPosition = Vector3.zero;
        readme.transform.localRotation = Quaternion.identity;
        readme.transform.localScale = Vector3.one;
        createdObjects.Add(readme);

        TextAsset markerNotes = CreateMarkerNotesAsset();
        if (markerNotes != null)
        {
            Debug.Log("[Game AI CA3] Marker usage notes written to Assets/GameAI_CA3/Docs/Generated/GameplayMarkerNotes.txt");
        }
    }

    private static TextAsset CreateMarkerNotesAsset()
    {
        string folder = "Assets/GameAI_CA3/Docs/Generated";
        EnsureAssetFolder(folder);

        string path = $"{folder}/GameplayMarkerNotes.txt";

        string contents =
            "Game AI CA3 Gameplay Marker Notes\n" +
            "=================================\n\n" +
            "Generated by GameAIStationGameplayMarkers.\n\n" +
            "Core markers:\n" +
            "- PlayerSpawn: move/place the player prefab here.\n" +
            "- WardenSpawn: move/place the main AI enemy here.\n" +
            "- ExitDoor: locked exit object for the three-medal objective.\n" +
            "- ExitInteractionPoint: trigger location for the escape check.\n" +
            "- Medal_01_WestOffice, Medal_02_WestStorage, Medal_03_RecordsEast: objective pickups.\n\n" +
            "AI route markers:\n" +
            "- PatrolPoint_*: assign these to the Warden patrol route.\n" +
            "- SearchPoint_*: use as fallback investigation/search destinations if needed.\n" +
            "- FleePoint_*: use for low-health flee behaviour.\n\n" +
            "Demo sequence:\n" +
            "1. Start at PlayerSpawn.\n" +
            "2. Collect Medal 01 in the west office.\n" +
            "3. Trigger/hear noise near NoiseDemoZone_MainHallWest.\n" +
            "4. Let the Warden see and chase the player.\n" +
            "5. Break line of sight near LineOfSightBreak_WestCorner.\n" +
            "6. Collect Medal 02 and Medal 03.\n" +
            "7. Return to ExitInteractionPoint and unlock ExitDoor.\n";

        File.WriteAllText(path, contents);
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
    }

    private static GameObject CreateMarker(MarkerDefinition definition)
    {
        GameObject marker;

        switch (definition.Shape)
        {
            case MarkerShape.Cube:
                marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case MarkerShape.Sphere:
                marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case MarkerShape.Cylinder:
                marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;
            default:
                marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
        }

        marker.name = definition.Name;
        marker.transform.SetParent(definition.Parent);
        marker.transform.position = definition.Position;
        marker.transform.rotation = Quaternion.Euler(0f, definition.RotationY, 0f);
        marker.transform.localScale = definition.Scale;

        Renderer renderer = marker.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = GetOrCreateMaterial($"MAT_Marker_{SanitiseName(definition.Name)}", definition.Color);

        Collider collider = marker.GetComponent<Collider>();
        if (collider != null)
        {
            if (!definition.KeepCollider)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
            else
            {
                collider.isTrigger = definition.TriggerCollider;
            }
        }

        MarkerDescription description = marker.AddComponent<MarkerDescription>();
        description.description = definition.Description;

        createdObjects.Add(marker);
        return marker;
    }

    private static void FinaliseGeneratedObjects()
    {
        foreach (GameObject obj in createdObjects)
        {
            if (obj == null)
                continue;

            Undo.RegisterCreatedObjectUndo(obj, "Generate Game AI CA3 Gameplay Markers");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static Material GetOrCreateMaterial(string materialName, Color color)
    {
        EnsureAssetFolder(GeneratedMaterialFolder);

        string materialPath = $"{GeneratedMaterialFolder}/{materialName}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (existing != null)
        {
            existing.color = color;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        material.name = materialName;
        material.color = color;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        AssetDatabase.CreateAsset(material, materialPath);
        return material;
    }

    private static void EnsureAssetFolder(string folderPath)
    {
        string[] parts = folderPath.Split('/');

        if (parts.Length == 0 || parts[0] != "Assets")
            throw new ArgumentException("Folder path must start with Assets.");

        string currentPath = "Assets";

        for (int i = 1; i < parts.Length; i++)
        {
            string nextPath = $"{currentPath}/{parts[i]}";

            if (!AssetDatabase.IsValidFolder(nextPath))
                AssetDatabase.CreateFolder(currentPath, parts[i]);

            currentPath = nextPath;
        }
    }

    private static string SanitiseName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "Unnamed";

        foreach (char invalid in Path.GetInvalidFileNameChars())
            raw = raw.Replace(invalid, '_');

        return raw.Replace(" ", "_");
    }

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }

    private enum MarkerShape
    {
        Cube,
        Sphere,
        Cylinder
    }

    private struct MarkerDefinition
    {
        public string Name;
        public Transform Parent;
        public Vector3 Position;
        public float RotationY;
        public MarkerShape Shape;
        public Vector3 Scale;
        public Color Color;
        public bool KeepCollider;
        public bool TriggerCollider;
        public string Description;
    }

    private sealed class MarkerDescription : MonoBehaviour
    {
        [TextArea(2, 6)]
        public string description;
    }
}
