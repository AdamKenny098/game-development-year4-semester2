using UnityEditor;
using UnityEngine;

public static class OutpostCaptureLayoutGenerator
{
    private const string RootName = "OutpostCapture_LijiangStyle_Blockout";
    private const string GeneratedFolder = "Assets/Generated";
    private const string MaterialFolder = "Assets/Generated/OutpostCaptureMaterials";

    private static Transform root;

    private static Material floorMat;
    private static Material wallMat;
    private static Material spawnBlueMat;
    private static Material spawnRedMat;
    private static Material captureMat;
    private static Material captureZoneMat;
    private static Material coverMat;
    private static Material npcMat;
    private static Material patrolMat;

    [MenuItem("Tools/CA3/Generate Outpost Capture Layout")]
    public static void Generate()
    {
        if (!EditorUtility.DisplayDialog(
                "Generate Outpost Capture Layout",
                "This will delete the existing generated Outpost Capture blockout and create a new symmetric control-point layout.",
                "Generate",
                "Cancel"))
        {
            return;
        }

        DeleteExistingRoot();
        CreateMaterials();

        GameObject rootObject = new GameObject(RootName);
        root = rootObject.transform;
        Undo.RegisterCreatedObjectUndo(rootObject, "Generate Outpost Capture Layout");

        CreateFloorLayout();
        CreateOuterWalls();
        CreateInteriorWalls();
        CreateSpawnZones();
        CreateCentralCapturePoint();
        CreateCoverAndMarketBlocks();
        CreateNpcAndPatrolPoints();
        CreateLighting();
        CreateCamera();

        Selection.activeGameObject = rootObject;

        EditorUtility.DisplayDialog(
            "Generated",
            "Outpost Capture layout generated. Save the scene, bake NavMesh, then attach your capture point and NPC scripts.",
            "OK");
    }

    private static void DeleteExistingRoot()
    {
        GameObject existing = GameObject.Find(RootName);

        if (existing != null)
            Undo.DestroyObjectImmediate(existing);
    }

    private static void CreateMaterials()
    {
        EnsureFolder("Assets", "Generated");
        EnsureFolder(GeneratedFolder, "OutpostCaptureMaterials");

        floorMat = CreateOrLoadMaterial("MAT_Floor_DarkConcrete", "#343A40");
        wallMat = CreateOrLoadMaterial("MAT_Wall_Blockout", "#20242A");
        spawnBlueMat = CreateOrLoadMaterial("MAT_BlueSpawn", "#1E6BFF", true, "#1E6BFF", 1.2f);
        spawnRedMat = CreateOrLoadMaterial("MAT_RedSpawn", "#D82F5E", true, "#D82F5E", 1.2f);
        captureMat = CreateOrLoadMaterial("MAT_CapturePoint", "#F7B733", true, "#F7B733", 1.4f);
        captureZoneMat = CreateOrLoadTransparentMaterial("MAT_CaptureZone_Debug", "#36C8FF", 0.22f);
        coverMat = CreateOrLoadMaterial("MAT_Cover_Block", "#625448");
        npcMat = CreateOrLoadMaterial("MAT_NPC_Guard", "#B82929");
        patrolMat = CreateOrLoadMaterial("MAT_PatrolPoint", "#A855F7", true, "#A855F7", 1.0f);

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

    private static Material CreateOrLoadTransparentMaterial(string materialName, string hex, float alpha)
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
        color.a = alpha;

        material.color = color;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.renderQueue = 3000;

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Color ParseColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;

        return Color.magenta;
    }

    private static Transform CreateGroup(string name)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(root);
        Undo.RegisterCreatedObjectUndo(group, "Create Group");
        return group.transform;
    }

    private static GameObject CreateCube(
        string name,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.transform.SetParent(parent);

        Renderer renderer = cube.GetComponent<Renderer>();

        if (renderer != null && material != null)
            renderer.sharedMaterial = material;

        Undo.RegisterCreatedObjectUndo(cube, "Create Cube");
        return cube;
    }

    private static GameObject CreateEmpty(string name, Vector3 position, Transform parent)
    {
        GameObject empty = new GameObject(name);
        empty.transform.position = position;
        empty.transform.SetParent(parent);

        Undo.RegisterCreatedObjectUndo(empty, "Create Empty");
        return empty;
    }

    private static void CreateFloorLayout()
    {
        Transform group = CreateGroup("01_Floors");

        // Main middle body
        CreateFloor("Central_Control_Room", new Vector3(0f, 0f, 0f), new Vector3(20f, 1f, 16f), group);

        // Lower capture courtyard/protrusion
        CreateFloor("Lower_Capture_Courtyard", new Vector3(0f, 0f, -15f), new Vector3(16f, 1f, 12f), group);
        CreateFloor("Lower_Capture_Neck", new Vector3(0f, 0f, -7f), new Vector3(10f, 1f, 8f), group);

        // Left side mirrored rooms and corridors
        CreateFloor("Blue_Left_Spawn_Main", new Vector3(-42f, 0f, 0f), new Vector3(18f, 1f, 24f), group);
        CreateFloor("Blue_Upper_Room", new Vector3(-24f, 0f, 9f), new Vector3(16f, 1f, 10f), group);
        CreateFloor("Blue_Lower_Room", new Vector3(-24f, 0f, -9f), new Vector3(16f, 1f, 10f), group);
        CreateFloor("Blue_Mid_Corridor", new Vector3(-15f, 0f, 0f), new Vector3(10f, 1f, 8f), group);
        CreateFloor("Blue_Upper_Hall", new Vector3(-32f, 0f, 7f), new Vector3(14f, 1f, 6f), group);
        CreateFloor("Blue_Lower_Hall", new Vector3(-32f, 0f, -7f), new Vector3(14f, 1f, 6f), group);

        // Right side mirrored rooms and corridors
        CreateFloor("Red_Right_Spawn_Main", new Vector3(42f, 0f, 0f), new Vector3(18f, 1f, 24f), group);
        CreateFloor("Red_Upper_Room", new Vector3(24f, 0f, 9f), new Vector3(16f, 1f, 10f), group);
        CreateFloor("Red_Lower_Room", new Vector3(24f, 0f, -9f), new Vector3(16f, 1f, 10f), group);
        CreateFloor("Red_Mid_Corridor", new Vector3(15f, 0f, 0f), new Vector3(10f, 1f, 8f), group);
        CreateFloor("Red_Upper_Hall", new Vector3(32f, 0f, 7f), new Vector3(14f, 1f, 6f), group);
        CreateFloor("Red_Lower_Hall", new Vector3(32f, 0f, -7f), new Vector3(14f, 1f, 6f), group);

        // Angled spawn extensions using rotated rectangles to fake the polygon shape
        GameObject blueTop = CreateFloor("Blue_Spawn_Angled_Top", new Vector3(-47f, 0f, 11f), new Vector3(14f, 1f, 8f), group);
        blueTop.transform.rotation = Quaternion.Euler(0f, -22f, 0f);

        GameObject blueBottom = CreateFloor("Blue_Spawn_Angled_Bottom", new Vector3(-47f, 0f, -11f), new Vector3(14f, 1f, 8f), group);
        blueBottom.transform.rotation = Quaternion.Euler(0f, 22f, 0f);

        GameObject redTop = CreateFloor("Red_Spawn_Angled_Top", new Vector3(47f, 0f, 11f), new Vector3(14f, 1f, 8f), group);
        redTop.transform.rotation = Quaternion.Euler(0f, 22f, 0f);

        GameObject redBottom = CreateFloor("Red_Spawn_Angled_Bottom", new Vector3(47f, 0f, -11f), new Vector3(14f, 1f, 8f), group);
        redBottom.transform.rotation = Quaternion.Euler(0f, -22f, 0f);
    }

    private static GameObject CreateFloor(string name, Vector3 position, Vector3 scale, Transform parent)
    {
        return CreateCube(
            name,
            new Vector3(position.x, -0.05f, position.z),
            new Vector3(scale.x, 0.1f, scale.z),
            floorMat,
            parent);
    }

    private static void CreateOuterWalls()
    {
        Transform group = CreateGroup("02_Outer_Walls");

        // Central top boundary
        CreateWall("Central_Top_Wall", new Vector3(0f, 2f, 8.5f), new Vector3(22f, 4f, 1f), group);

        // Lower capture courtyard walls
        CreateWall("Capture_Left_Wall", new Vector3(-8.5f, 2f, -15f), new Vector3(1f, 4f, 13f), group);
        CreateWall("Capture_Right_Wall", new Vector3(8.5f, 2f, -15f), new Vector3(1f, 4f, 13f), group);
        CreateWall("Capture_Bottom_Wall_Left", new Vector3(-5f, 2f, -21.5f), new Vector3(7f, 4f, 1f), group);
        CreateWall("Capture_Bottom_Wall_Right", new Vector3(5f, 2f, -21.5f), new Vector3(7f, 4f, 1f), group);

        // Left spawn outline
        CreateWall("Blue_Spawn_Back_Wall", new Vector3(-52f, 2f, 0f), new Vector3(1f, 4f, 25f), group);
        CreateWall("Blue_Spawn_Top_Wall", new Vector3(-42f, 2f, 12.5f), new Vector3(18f, 4f, 1f), group);
        CreateWall("Blue_Spawn_Bottom_Wall", new Vector3(-42f, 2f, -12.5f), new Vector3(18f, 4f, 1f), group);

        // Right spawn outline
        CreateWall("Red_Spawn_Back_Wall", new Vector3(52f, 2f, 0f), new Vector3(1f, 4f, 25f), group);
        CreateWall("Red_Spawn_Top_Wall", new Vector3(42f, 2f, 12.5f), new Vector3(18f, 4f, 1f), group);
        CreateWall("Red_Spawn_Bottom_Wall", new Vector3(42f, 2f, -12.5f), new Vector3(18f, 4f, 1f), group);

        // Upper/lower room outer edges
        CreateWall("Blue_Upper_Outer", new Vector3(-24f, 2f, 14.5f), new Vector3(17f, 4f, 1f), group);
        CreateWall("Blue_Lower_Outer", new Vector3(-24f, 2f, -14.5f), new Vector3(17f, 4f, 1f), group);
        CreateWall("Red_Upper_Outer", new Vector3(24f, 2f, 14.5f), new Vector3(17f, 4f, 1f), group);
        CreateWall("Red_Lower_Outer", new Vector3(24f, 2f, -14.5f), new Vector3(17f, 4f, 1f), group);

        // Left and right side boundaries near center
        CreateWall("Blue_Center_Upper_Wall", new Vector3(-11f, 2f, 8.5f), new Vector3(1f, 4f, 7f), group);
        CreateWall("Blue_Center_Lower_Wall", new Vector3(-11f, 2f, -8.5f), new Vector3(1f, 4f, 7f), group);

        CreateWall("Red_Center_Upper_Wall", new Vector3(11f, 2f, 8.5f), new Vector3(1f, 4f, 7f), group);
        CreateWall("Red_Center_Lower_Wall", new Vector3(11f, 2f, -8.5f), new Vector3(1f, 4f, 7f), group);
    }

    private static void CreateInteriorWalls()
    {
        Transform group = CreateGroup("03_Interior_Walls");

        // Left side room separation
        CreateWall("Blue_Upper_Room_Inner_Back", new Vector3(-24f, 2f, 4f), new Vector3(12f, 4f, 1f), group);
        CreateWall("Blue_Lower_Room_Inner_Back", new Vector3(-24f, 2f, -4f), new Vector3(12f, 4f, 1f), group);

        // Right side room separation
        CreateWall("Red_Upper_Room_Inner_Back", new Vector3(24f, 2f, 4f), new Vector3(12f, 4f, 1f), group);
        CreateWall("Red_Lower_Room_Inner_Back", new Vector3(24f, 2f, -4f), new Vector3(12f, 4f, 1f), group);

        // Central room side lips, leaving entries open
        CreateWall("Central_Left_Top_Lip", new Vector3(-8.5f, 2f, 5.5f), new Vector3(1f, 4f, 5f), group);
        CreateWall("Central_Left_Bottom_Lip", new Vector3(-8.5f, 2f, -5.5f), new Vector3(1f, 4f, 5f), group);

        CreateWall("Central_Right_Top_Lip", new Vector3(8.5f, 2f, 5.5f), new Vector3(1f, 4f, 5f), group);
        CreateWall("Central_Right_Bottom_Lip", new Vector3(8.5f, 2f, -5.5f), new Vector3(1f, 4f, 5f), group);

        // Rail between central room and lower capture courtyard, with an opening
        CreateWall("Central_Lower_Rail_Left", new Vector3(-6f, 1.2f, -7.5f), new Vector3(5f, 2.4f, 1f), group);
        CreateWall("Central_Lower_Rail_Right", new Vector3(6f, 1.2f, -7.5f), new Vector3(5f, 2.4f, 1f), group);

        // Small spawn choke blocks
        CreateWall("Blue_Spawn_Choke_Top", new Vector3(-34f, 2f, 5f), new Vector3(1f, 4f, 5f), group);
        CreateWall("Blue_Spawn_Choke_Bottom", new Vector3(-34f, 2f, -5f), new Vector3(1f, 4f, 5f), group);

        CreateWall("Red_Spawn_Choke_Top", new Vector3(34f, 2f, 5f), new Vector3(1f, 4f, 5f), group);
        CreateWall("Red_Spawn_Choke_Bottom", new Vector3(34f, 2f, -5f), new Vector3(1f, 4f, 5f), group);
    }

    private static GameObject CreateWall(string name, Vector3 position, Vector3 scale, Transform parent)
    {
        return CreateCube(name, position, scale, wallMat, parent);
    }

    private static void CreateSpawnZones()
    {
        Transform group = CreateGroup("04_Spawn_Zones");

        GameObject blueSpawnZone = CreateCube(
            "BlueSpawn_DebugZone",
            new Vector3(-43f, 0.08f, 0f),
            new Vector3(12f, 0.12f, 16f),
            spawnBlueMat,
            group);

        GameObject redSpawnZone = CreateCube(
            "RedSpawn_DebugZone",
            new Vector3(43f, 0.08f, 0f),
            new Vector3(12f, 0.12f, 16f),
            spawnRedMat,
            group);

        CreateEmpty("PlayerSpawn_Blue_01", new Vector3(-44f, 1f, -3f), group);
        CreateEmpty("PlayerSpawn_Blue_02", new Vector3(-44f, 1f, 3f), group);

        CreateEmpty("PlayerSpawn_Red_01", new Vector3(44f, 1f, -3f), group);
        CreateEmpty("PlayerSpawn_Red_02", new Vector3(44f, 1f, 3f), group);
    }

    private static void CreateCentralCapturePoint()
    {
        Transform group = CreateGroup("05_Capture_Point");

        CreateCube(
            "Capture_Platform",
            new Vector3(0f, 0.25f, -14f),
            new Vector3(8f, 0.5f, 8f),
            captureMat,
            group);

        GameObject rotated = CreateCube(
            "Capture_Platform_Diamond",
            new Vector3(0f, 0.35f, -14f),
            new Vector3(6.5f, 0.3f, 6.5f),
            captureMat,
            group);

        rotated.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

        GameObject zone = CreateCube(
            "CaptureZone_Trigger",
            new Vector3(0f, 0.8f, -14f),
            new Vector3(11f, 1.4f, 11f),
            captureZoneMat,
            group);

        BoxCollider box = zone.GetComponent<BoxCollider>();
        box.isTrigger = true;

        CreateCube(
            "Capture_Beacon",
            new Vector3(0f, 1.6f, -14f),
            new Vector3(1.5f, 2.6f, 1.5f),
            captureMat,
            group);

        GameObject label = CreateEmpty("CapturePoint_Center", new Vector3(0f, 1f, -14f), group);
        label.name = "CapturePoint_Center";
    }

    private static void CreateCoverAndMarketBlocks()
    {
        Transform group = CreateGroup("06_Cover_And_Market_Blocks");

        // Central room cover
        CreateCube("Central_Cover_Left", new Vector3(-4.5f, 0.75f, 1.5f), new Vector3(2f, 1.5f, 4f), coverMat, group);
        CreateCube("Central_Cover_Right", new Vector3(4.5f, 0.75f, 1.5f), new Vector3(2f, 1.5f, 4f), coverMat, group);
        CreateCube("Central_Back_Counter", new Vector3(0f, 0.75f, 5f), new Vector3(6f, 1.5f, 1.5f), coverMat, group);

        // Lower capture cover
        CreateCube("Capture_Cover_Left", new Vector3(-5f, 0.75f, -17f), new Vector3(2f, 1.5f, 4f), coverMat, group);
        CreateCube("Capture_Cover_Right", new Vector3(5f, 0.75f, -17f), new Vector3(2f, 1.5f, 4f), coverMat, group);

        // Left side market blocks
        CreateCube("Blue_Upper_Market_Block", new Vector3(-24f, 0.75f, 10f), new Vector3(5f, 1.5f, 2f), coverMat, group);
        CreateCube("Blue_Lower_Market_Block", new Vector3(-24f, 0.75f, -10f), new Vector3(5f, 1.5f, 2f), coverMat, group);
        CreateCube("Blue_Corridor_Cover", new Vector3(-15f, 0.75f, 0f), new Vector3(2f, 1.5f, 3f), coverMat, group);

        // Right side market blocks
        CreateCube("Red_Upper_Market_Block", new Vector3(24f, 0.75f, 10f), new Vector3(5f, 1.5f, 2f), coverMat, group);
        CreateCube("Red_Lower_Market_Block", new Vector3(24f, 0.75f, -10f), new Vector3(5f, 1.5f, 2f), coverMat, group);
        CreateCube("Red_Corridor_Cover", new Vector3(15f, 0.75f, 0f), new Vector3(2f, 1.5f, 3f), coverMat, group);

        // Spawn-side decorative cover
        CreateCube("Blue_Spawn_Cover_Top", new Vector3(-42f, 0.75f, 7f), new Vector3(5f, 1.5f, 2f), coverMat, group);
        CreateCube("Blue_Spawn_Cover_Bottom", new Vector3(-42f, 0.75f, -7f), new Vector3(5f, 1.5f, 2f), coverMat, group);

        CreateCube("Red_Spawn_Cover_Top", new Vector3(42f, 0.75f, 7f), new Vector3(5f, 1.5f, 2f), coverMat, group);
        CreateCube("Red_Spawn_Cover_Bottom", new Vector3(42f, 0.75f, -7f), new Vector3(5f, 1.5f, 2f), coverMat, group);
    }

    private static void CreateNpcAndPatrolPoints()
    {
        Transform group = CreateGroup("07_NPC_And_Patrol");

        CreateCube(
            "NPC_Guard_Placeholder",
            new Vector3(0f, 1f, -5f),
            new Vector3(1.4f, 2f, 1.4f),
            npcMat,
            group);

        CreatePatrolPoint("PatrolPoint_01", new Vector3(-6f, 0.5f, -8f), group);
        CreatePatrolPoint("PatrolPoint_02", new Vector3(6f, 0.5f, -8f), group);
        CreatePatrolPoint("PatrolPoint_03", new Vector3(5f, 0.5f, -16f), group);
        CreatePatrolPoint("PatrolPoint_04", new Vector3(-5f, 0.5f, -16f), group);
    }

    private static void CreatePatrolPoint(string name, Vector3 position, Transform parent)
    {
        CreateEmpty(name, position, parent);

        CreateCube(
            $"{name}_Visual",
            new Vector3(position.x, 0.2f, position.z),
            new Vector3(0.7f, 0.4f, 0.7f),
            patrolMat,
            parent);
    }

    private static void CreateLighting()
    {
        Transform group = CreateGroup("08_Lighting");

        GameObject directionalObject = new GameObject("Directional_Light");
        directionalObject.transform.position = new Vector3(0f, 20f, 0f);
        directionalObject.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
        directionalObject.transform.SetParent(group);

        Light directional = directionalObject.AddComponent<Light>();
        directional.type = LightType.Directional;
        directional.intensity = 0.65f;
        directional.color = ParseColor("#B7C8FF");
        directional.shadows = LightShadows.Soft;

        Undo.RegisterCreatedObjectUndo(directionalObject, "Create Directional Light");

        CreatePointLight("Blue_Spawn_Light", new Vector3(-43f, 4f, 0f), ParseColor("#1E6BFF"), 2f, 18f, group);
        CreatePointLight("Red_Spawn_Light", new Vector3(43f, 4f, 0f), ParseColor("#D82F5E"), 2f, 18f, group);
        CreatePointLight("Capture_Point_Light", new Vector3(0f, 5f, -14f), ParseColor("#F7B733"), 2.5f, 16f, group);
        CreatePointLight("Central_Room_Light", new Vector3(0f, 5f, 0f), ParseColor("#F0E3C0"), 1.4f, 18f, group);

        RenderSettings.ambientLight = ParseColor("#171A20");
    }

    private static void CreatePointLight(
        string name,
        Vector3 position,
        Color color,
        float intensity,
        float range,
        Transform parent)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.position = position;
        lightObject.transform.SetParent(parent);

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;

        Undo.RegisterCreatedObjectUndo(lightObject, "Create Point Light");
    }

    private static void CreateCamera()
    {
        Transform group = CreateGroup("09_Preview_Camera");

        GameObject cameraObject = new GameObject("TopDown_Preview_Camera");
        cameraObject.transform.position = new Vector3(0f, 70f, -2f);
        cameraObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        cameraObject.transform.SetParent(group);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 36f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 150f;

        Undo.RegisterCreatedObjectUndo(cameraObject, "Create Camera");
    }
}