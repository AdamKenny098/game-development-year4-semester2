using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class GameAIStationStrongStylePass
{
    private const string ShellRootName = "GameAI_CA3_Station_LevelShell";
    private const string StyleRootName = "GameAI_CA3_Station_StrongStylePass";

    private const string MaterialsRoot = "Assets/GameAI_CA3/Materials/Generated/StationStrongStylePass";
    private const string TexturesRoot = "Assets/GameAI_CA3/Materials/Generated/StationStrongStylePass/Textures";

    private const float DecorativeLift = 0.035f;
    private const float FloorLineHeight = 0.025f;
    private const float FloorLineThickness = 0.045f;
    private const float WallPanelThickness = 0.055f;
    private const float WallPanelHeight = 1.15f;
    private const float WallTrimHeight = 0.12f;
    private const float WallTrimThickness = 0.08f;

    private static readonly Color FloorBase = Hex("#566064");
    private static readonly Color FloorAlt = Hex("#465055");
    private static readonly Color FloorGrout = Hex("#171B1D");

    private static readonly Color WallUpper = Hex("#657166");
    private static readonly Color WallUpperAlt = Hex("#566158");
    private static readonly Color WallPanel = Hex("#28352F");
    private static readonly Color WallPanelAlt = Hex("#21302A");
    private static readonly Color WallTrim = Hex("#151B18");

    private static readonly Color CeilingBase = Hex("#2B3032");
    private static readonly Color CeilingAlt = Hex("#222729");

    private static readonly Color DoorGold = Hex("#D6A84A");
    private static readonly Color DoorFrame = Hex("#211612");
    private static readonly Color CarpetRed = Hex("#5B1E1E");
    private static readonly Color CarpetDarkRed = Hex("#331212");
    private static readonly Color Wood = Hex("#4A2C1D");
    private static readonly Color DarkWood = Hex("#24150F");
    private static readonly Color Metal = Hex("#41484D");
    private static readonly Color DarkMetal = Hex("#1B1F22");
    private static readonly Color Paper = Hex("#C9C0A8");
    private static readonly Color WarmLight = Hex("#F3CF8A");
    private static readonly Color Blood = Hex("#541313");
    private static readonly Color Dust = Hex("#8A8171");

    private static Material floorMaterial;
    private static Material wallUpperMaterial;
    private static Material ceilingMaterial;
    private static Material doorMarkerMaterial;
    private static Material floorLineMaterial;
    private static Material wallPanelMaterial;
    private static Material trimMaterial;
    private static Material doorFrameMaterial;
    private static Material carpetMaterial;
    private static Material carpetBorderMaterial;
    private static Material woodMaterial;
    private static Material darkWoodMaterial;
    private static Material metalMaterial;
    private static Material darkMetalMaterial;
    private static Material paperMaterial;
    private static Material warmEmissionMaterial;
    private static Material bloodMaterial;
    private static Material dustMaterial;

    [MenuItem("Tools/Game AI CA3/Station Style/Apply Strong Material And Life Pass")]
    public static void ApplyStrongMaterialAndLifePass()
    {
        GameObject shellRoot = GameObject.Find(ShellRootName);

        if (shellRoot == null)
        {
            EditorUtility.DisplayDialog(
                "Station Shell Missing",
                "Could not find GameAI_CA3_Station_LevelShell. Generate the shell first, then run this style pass.",
                "OK"
            );
            return;
        }

        CreateAssetFolders();
        CreateMaterials();
        AssignShellMaterials(shellRoot.transform);
        ClearStylePass();

        Transform styleRoot = new GameObject(StyleRootName).transform;
        styleRoot.SetParent(shellRoot.transform);
        styleRoot.localPosition = Vector3.zero;
        styleRoot.localRotation = Quaternion.identity;
        styleRoot.localScale = Vector3.one;

        List<Transform> floors = GetDirectChildren(shellRoot.transform, "Floors");
        List<Transform> walls = GetDirectChildren(shellRoot.transform, "Walls");
        List<Transform> ceilings = GetDirectChildren(shellRoot.transform, "Ceilings");
        List<Transform> doors = GetDirectChildren(shellRoot.transform, "DoorMarkers");

        Transform floorDetailRoot = CreateRoot(styleRoot, "Floor_Tile_And_Carpet_Details");
        Transform wallDetailRoot = CreateRoot(styleRoot, "Wall_Panels_And_Trim");
        Transform doorDetailRoot = CreateRoot(styleRoot, "Door_Frames_And_Thresholds");
        Transform propRoot = CreateRoot(styleRoot, "Simple_Station_Props");
        Transform lightRoot = CreateRoot(styleRoot, "Mood_Lighting");
        Transform decalRoot = CreateRoot(styleRoot, "Decals_And_Dirt_NoCollision");

        GenerateFloorGridLines(floors, floorDetailRoot);
        GenerateCarpets(floors, floorDetailRoot);
        GenerateWallPanelsAndTrim(walls, wallDetailRoot);
        GenerateDoorFramesAndThresholds(doors, doorDetailRoot);
        RecolourExistingVisualPassProps(shellRoot.transform);
        GenerateExtraProps(floors, propRoot);
        GenerateDecals(floors, decalRoot);
        GenerateMoodLighting(floors, ceilings, lightRoot);
        ApplyRenderSettings();

        Selection.activeGameObject = styleRoot.gameObject;
        EditorUtility.SetDirty(shellRoot);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Game AI CA3] Strong material and life pass applied. Floors, walls, ceilings, doors, panels, trims, carpets, lights, and simple props updated.");
    }

    [MenuItem("Tools/Game AI CA3/Station Style/Clear Strong Style Pass")]
    public static void ClearStylePass()
    {
        GameObject existing = GameObject.Find(StyleRootName);

        if (existing != null)
            UnityEngine.Object.DestroyImmediate(existing);
    }

    [MenuItem("Tools/Game AI CA3/Station Style/Hide Ceilings")]
    public static void HideCeilings()
    {
        SetCeilingVisibility(false);
    }

    [MenuItem("Tools/Game AI CA3/Station Style/Show Ceilings")]
    public static void ShowCeilings()
    {
        SetCeilingVisibility(true);
    }

    private static void SetCeilingVisibility(bool visible)
    {
        GameObject shellRoot = GameObject.Find(ShellRootName);

        if (shellRoot == null)
            return;

        Transform ceilings = shellRoot.transform.Find("Ceilings");

        if (ceilings == null)
            return;

        ceilings.gameObject.SetActive(visible);
        EditorUtility.SetDirty(ceilings.gameObject);
    }

    private static void CreateAssetFolders()
    {
        CreateFolderPath("Assets/GameAI_CA3");
        CreateFolderPath("Assets/GameAI_CA3/Materials");
        CreateFolderPath("Assets/GameAI_CA3/Materials/Generated");
        CreateFolderPath(MaterialsRoot);
        CreateFolderPath(TexturesRoot);
    }

    private static void CreateFolderPath(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string[] parts = path.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }

    private static void CreateMaterials()
    {
        Texture2D floorTexture = CreateOrUpdateTexture("TEX_Strong_Floor_Tiles.png", 512, TextureKind.Floor, FloorBase, FloorAlt, FloorGrout);
        Texture2D wallTexture = CreateOrUpdateTexture("TEX_Strong_Wall_Plaster.png", 512, TextureKind.Noise, WallUpper, WallUpperAlt, Hex("#798270"));
        Texture2D panelTexture = CreateOrUpdateTexture("TEX_Strong_Lower_Wall_Panel.png", 512, TextureKind.VerticalPanel, WallPanel, WallPanelAlt, Hex("#111716"));
        Texture2D ceilingTexture = CreateOrUpdateTexture("TEX_Strong_Ceiling.png", 512, TextureKind.Noise, CeilingBase, CeilingAlt, Hex("#383F42"));
        Texture2D carpetTexture = CreateOrUpdateTexture("TEX_Strong_Carpet.png", 512, TextureKind.Carpet, CarpetRed, CarpetDarkRed, Hex("#7A2C2C"));
        Texture2D woodTexture = CreateOrUpdateTexture("TEX_Strong_Wood.png", 512, TextureKind.Wood, Wood, DarkWood, Hex("#704733"));
        Texture2D metalTexture = CreateOrUpdateTexture("TEX_Strong_Metal.png", 512, TextureKind.Metal, Metal, DarkMetal, Hex("#5D666B"));
        Texture2D paperTexture = CreateOrUpdateTexture("TEX_Strong_Paper.png", 256, TextureKind.Noise, Paper, Hex("#A89F8B"), Hex("#DDD5C0"));
        Texture2D bloodTexture = CreateOrUpdateTexture("TEX_Strong_Blood.png", 256, TextureKind.Noise, Blood, Hex("#2B0808"), Hex("#7A1818"));
        Texture2D dustTexture = CreateOrUpdateTexture("TEX_Strong_Dust.png", 256, TextureKind.Noise, Dust, Hex("#4B453B"), Hex("#A69C88"));

        floorMaterial = CreateOrUpdateMaterial("MAT_Strong_Floor_Tiles_Clear", FloorBase, floorTexture, new Vector2(2f, 2f), 0f, 0.2f);
        wallUpperMaterial = CreateOrUpdateMaterial("MAT_Strong_Wall_Upper_GreenGrey", WallUpper, wallTexture, new Vector2(1.8f, 1.8f), 0f, 0.1f);
        ceilingMaterial = CreateOrUpdateMaterial("MAT_Strong_Ceiling_Dark", CeilingBase, ceilingTexture, new Vector2(2f, 2f), 0f, 0.05f);
        doorMarkerMaterial = CreateOrUpdateMaterial("MAT_Strong_Door_Markers_Brass", DoorGold, null, Vector2.one, 0f, 0.3f);
        floorLineMaterial = CreateOrUpdateMaterial("MAT_Strong_Floor_Grout_Lines", FloorGrout, null, Vector2.one, 0f, 0.0f);
        wallPanelMaterial = CreateOrUpdateMaterial("MAT_Strong_Lower_Wall_Panels", WallPanel, panelTexture, new Vector2(1.5f, 1f), 0f, 0.15f);
        trimMaterial = CreateOrUpdateMaterial("MAT_Strong_Black_Wall_Trim", WallTrim, woodTexture, new Vector2(2f, 1f), 0f, 0.2f);
        doorFrameMaterial = CreateOrUpdateMaterial("MAT_Strong_Dark_Door_Frames", DoorFrame, woodTexture, new Vector2(1.2f, 1f), 0f, 0.25f);
        carpetMaterial = CreateOrUpdateMaterial("MAT_Strong_Carpet_Red", CarpetRed, carpetTexture, new Vector2(2f, 1f), 0f, 0.4f);
        carpetBorderMaterial = CreateOrUpdateMaterial("MAT_Strong_Carpet_Border", CarpetDarkRed, null, Vector2.one, 0f, 0.2f);
        woodMaterial = CreateOrUpdateMaterial("MAT_Strong_Wood_Furniture", Wood, woodTexture, new Vector2(1.5f, 1f), 0f, 0.25f);
        darkWoodMaterial = CreateOrUpdateMaterial("MAT_Strong_Dark_Wood", DarkWood, woodTexture, new Vector2(1.5f, 1f), 0f, 0.2f);
        metalMaterial = CreateOrUpdateMaterial("MAT_Strong_Metal", Metal, metalTexture, new Vector2(1.5f, 1.5f), 0.1f, 0.35f);
        darkMetalMaterial = CreateOrUpdateMaterial("MAT_Strong_Dark_Metal", DarkMetal, metalTexture, new Vector2(1.5f, 1.5f), 0.1f, 0.25f);
        paperMaterial = CreateOrUpdateMaterial("MAT_Strong_Paper", Paper, paperTexture, Vector2.one, 0f, 0.1f);
        bloodMaterial = CreateOrUpdateMaterial("MAT_Strong_Blood_Decal", Blood, bloodTexture, Vector2.one, 0f, 0.65f);
        dustMaterial = CreateOrUpdateMaterial("MAT_Strong_Dust_Decal", Dust, dustTexture, Vector2.one, 0f, 0.2f);
        warmEmissionMaterial = CreateOrUpdateEmissiveMaterial("MAT_Strong_Warm_Light_Emission", WarmLight, 2.4f);
    }

    private static Texture2D CreateOrUpdateTexture(string fileName, int size, TextureKind kind, Color a, Color b, Color c)
    {
        string path = TexturesRoot + "/" + fileName;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = Path.GetFileNameWithoutExtension(fileName);

        switch (kind)
        {
            case TextureKind.Floor:
                FillFloorTexture(texture, a, b, c);
                break;
            case TextureKind.Noise:
                FillNoiseTexture(texture, a, b, c);
                break;
            case TextureKind.VerticalPanel:
                FillPanelTexture(texture, a, b, c);
                break;
            case TextureKind.Carpet:
                FillCarpetTexture(texture, a, b, c);
                break;
            case TextureKind.Wood:
                FillWoodTexture(texture, a, b, c);
                break;
            case TextureKind.Metal:
                FillMetalTexture(texture, a, b, c);
                break;
        }

        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static void FillFloorTexture(Texture2D texture, Color a, Color b, Color grout)
    {
        int size = texture.width;
        int tileSize = 128;
        int groutSize = 8;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool line = x % tileSize < groutSize || y % tileSize < groutSize;
                bool alt = ((x / tileSize) + (y / tileSize)) % 2 == 0;
                Color col = alt ? a : b;
                float n = Mathf.PerlinNoise(x * 0.055f, y * 0.055f);
                col = Color.Lerp(col, Color.black, n * 0.08f);

                if (line)
                    col = grout;

                texture.SetPixel(x, y, col);
            }
        }

        texture.Apply();
    }

    private static void FillNoiseTexture(Texture2D texture, Color a, Color b, Color speckle)
    {
        int size = texture.width;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n1 = Mathf.PerlinNoise(x * 0.022f, y * 0.022f);
                float n2 = Mathf.PerlinNoise((x + 41) * 0.13f, (y + 73) * 0.13f);
                Color col = Color.Lerp(a, b, n1 * 0.55f);

                if (n2 > 0.78f)
                    col = Color.Lerp(col, speckle, 0.22f);

                texture.SetPixel(x, y, col);
            }
        }

        texture.Apply();
    }

    private static void FillPanelTexture(Texture2D texture, Color a, Color b, Color line)
    {
        int size = texture.width;
        int panelWidth = 96;
        int trimWidth = 5;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool vertical = x % panelWidth < trimWidth;
                bool horizontal = y < trimWidth || y > size - trimWidth;
                float n = Mathf.PerlinNoise(x * 0.04f, y * 0.02f);
                Color col = Color.Lerp(a, b, n * 0.35f);

                if (vertical || horizontal)
                    col = line;

                texture.SetPixel(x, y, col);
            }
        }

        texture.Apply();
    }

    private static void FillCarpetTexture(Texture2D texture, Color a, Color b, Color fiber)
    {
        int size = texture.width;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float stripe = Mathf.Sin(x * 0.11f) * 0.5f + 0.5f;
                float n = Mathf.PerlinNoise(x * 0.09f, y * 0.09f);
                Color col = Color.Lerp(a, b, stripe * 0.25f);
                col = Color.Lerp(col, fiber, n * 0.12f);
                texture.SetPixel(x, y, col);
            }
        }

        texture.Apply();
    }

    private static void FillWoodTexture(Texture2D texture, Color a, Color b, Color c)
    {
        int size = texture.width;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float grain = Mathf.Sin((y + Mathf.PerlinNoise(x * 0.03f, y * 0.02f) * 28f) * 0.14f) * 0.5f + 0.5f;
                Color col = Color.Lerp(a, b, grain * 0.5f);

                if (Mathf.PerlinNoise(x * 0.08f, y * 0.08f) < 0.2f)
                    col = Color.Lerp(col, c, 0.3f);

                texture.SetPixel(x, y, col);
            }
        }

        texture.Apply();
    }

    private static void FillMetalTexture(Texture2D texture, Color a, Color b, Color line)
    {
        int size = texture.width;
        int panel = 128;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool edge = x % panel < 4 || y % panel < 4;
                float n = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                Color col = Color.Lerp(a, b, n * 0.5f);

                if (edge)
                    col = line;

                texture.SetPixel(x, y, col);
            }
        }

        texture.Apply();
    }

    private static Material CreateOrUpdateMaterial(string name, Color color, Texture2D texture, Vector2 tiling, float metallic, float smoothness)
    {
        string path = MaterialsRoot + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (mat == null)
        {
            mat = new Material(GetLitShader());
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = GetLitShader();
        }

        SetColor(mat, color);
        SetTexture(mat, texture, tiling);
        SetFloat(mat, "_Metallic", metallic);
        SetFloat(mat, "_Smoothness", smoothness);
        SetFloat(mat, "_Glossiness", smoothness);

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material CreateOrUpdateEmissiveMaterial(string name, Color color, float intensity)
    {
        Material mat = CreateOrUpdateMaterial(name, color, null, Vector2.one, 0f, 0.2f);

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * intensity);
        }

        return mat;
    }

    private static Shader GetLitShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader != null)
            return shader;

        shader = Shader.Find("HDRP/Lit");
        if (shader != null)
            return shader;

        shader = Shader.Find("Standard");
        if (shader != null)
            return shader;

        return Shader.Find("Diffuse");
    }

    private static void SetColor(Material mat, Color color)
    {
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);
    }

    private static void SetTexture(Material mat, Texture2D tex, Vector2 tiling)
    {
        if (tex == null)
            return;

        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", tex);
            mat.SetTextureScale("_BaseMap", tiling);
        }

        if (mat.HasProperty("_MainTex"))
        {
            mat.SetTexture("_MainTex", tex);
            mat.SetTextureScale("_MainTex", tiling);
        }
    }

    private static void SetFloat(Material mat, string property, float value)
    {
        if (mat.HasProperty(property))
            mat.SetFloat(property, value);
    }

    private static void AssignShellMaterials(Transform shellRoot)
    {
        AssignMaterialToGroup(shellRoot, "Floors", floorMaterial);
        AssignMaterialToGroup(shellRoot, "Walls", wallUpperMaterial);
        AssignMaterialToGroup(shellRoot, "Ceilings", ceilingMaterial);
        AssignMaterialToGroup(shellRoot, "DoorMarkers", doorMarkerMaterial);
    }

    private static void AssignMaterialToGroup(Transform root, string groupName, Material mat)
    {
        Transform group = root.Find(groupName);

        if (group == null)
            return;

        Renderer[] renderers = group.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            renderer.sharedMaterial = mat;
            EditorUtility.SetDirty(renderer);
        }
    }

    private static List<Transform> GetDirectChildren(Transform root, string groupName)
    {
        List<Transform> result = new List<Transform>();
        Transform group = root.Find(groupName);

        if (group == null)
            return result;

        for (int i = 0; i < group.childCount; i++)
            result.Add(group.GetChild(i));

        return result;
    }

    private static Transform CreateRoot(Transform parent, string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        return obj.transform;
    }

    private static void GenerateFloorGridLines(List<Transform> floors, Transform parent)
    {
        HashSet<string> edgeKeys = new HashSet<string>();

        foreach (Transform floor in floors)
        {
            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer == null)
                continue;

            Bounds b = renderer.bounds;

            AddFloorLineIfUnique(parent, edgeKeys, b.min.x, b.max.x, b.min.z, b.min.z, true);
            AddFloorLineIfUnique(parent, edgeKeys, b.min.x, b.max.x, b.max.z, b.max.z, true);
            AddFloorLineIfUnique(parent, edgeKeys, b.min.x, b.min.x, b.min.z, b.max.z, false);
            AddFloorLineIfUnique(parent, edgeKeys, b.max.x, b.max.x, b.min.z, b.max.z, false);
        }
    }

    private static void AddFloorLineIfUnique(Transform parent, HashSet<string> keys, float x1, float x2, float z1, float z2, bool horizontal)
    {
        string key = $"{Mathf.RoundToInt(x1 * 10f)}_{Mathf.RoundToInt(x2 * 10f)}_{Mathf.RoundToInt(z1 * 10f)}_{Mathf.RoundToInt(z2 * 10f)}";

        if (keys.Contains(key))
            return;

        keys.Add(key);

        Vector3 pos;
        Vector3 scale;

        if (horizontal)
        {
            pos = new Vector3((x1 + x2) * 0.5f, DecorativeLift, z1);
            scale = new Vector3(Mathf.Abs(x2 - x1), FloorLineHeight, FloorLineThickness);
        }
        else
        {
            pos = new Vector3(x1, DecorativeLift, (z1 + z2) * 0.5f);
            scale = new Vector3(FloorLineThickness, FloorLineHeight, Mathf.Abs(z2 - z1));
        }

        GameObject line = CreateCube(parent, "Floor_Grout_Line", pos, scale, floorLineMaterial, false);
        line.isStatic = true;
    }

    private static void GenerateCarpets(List<Transform> floors, Transform parent)
    {
        Bounds bounds = CalculateBounds(floors);
        Vector3 center = bounds.center;

        CreateCarpet(parent, "Main_Hall_Runner", new Vector3(center.x, 0.055f, center.z - 8f), new Vector3(5.2f, 0.04f, 13f));
        CreateCarpet(parent, "West_Corridor_Runner", new Vector3(center.x - 22f, 0.055f, center.z - 1f), new Vector3(12f, 0.04f, 3f));
        CreateCarpet(parent, "Upper_Gallery_Runner", new Vector3(center.x - 9f, 0.055f, center.z + 14f), new Vector3(14f, 0.04f, 2.6f));
    }

    private static void CreateCarpet(Transform parent, string name, Vector3 position, Vector3 scale)
    {
        GameObject carpet = CreateCube(parent, name, position, scale, carpetMaterial, false);
        CreateCube(parent, name + "_Border_N", position + new Vector3(0f, 0.01f, scale.z * 0.5f), new Vector3(scale.x, 0.045f, 0.12f), carpetBorderMaterial, false);
        CreateCube(parent, name + "_Border_S", position + new Vector3(0f, 0.01f, -scale.z * 0.5f), new Vector3(scale.x, 0.045f, 0.12f), carpetBorderMaterial, false);
        CreateCube(parent, name + "_Border_E", position + new Vector3(scale.x * 0.5f, 0.01f, 0f), new Vector3(0.12f, 0.045f, scale.z), carpetBorderMaterial, false);
        CreateCube(parent, name + "_Border_W", position + new Vector3(-scale.x * 0.5f, 0.01f, 0f), new Vector3(0.12f, 0.045f, scale.z), carpetBorderMaterial, false);
        carpet.isStatic = true;
    }

    private static void GenerateWallPanelsAndTrim(List<Transform> walls, Transform parent)
    {
        foreach (Transform wall in walls)
        {
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer == null)
                continue;

            Bounds b = renderer.bounds;
            Vector3 size = b.size;
            bool horizontal = size.x >= size.z;

            if (horizontal)
            {
                AddWallFaceDetails(parent, b, true, -1f);
                AddWallFaceDetails(parent, b, true, 1f);
            }
            else
            {
                AddWallFaceDetails(parent, b, false, -1f);
                AddWallFaceDetails(parent, b, false, 1f);
            }
        }
    }

    private static void AddWallFaceDetails(Transform parent, Bounds b, bool horizontal, float side)
    {
        if (horizontal)
        {
            float z = side < 0f ? b.min.z - WallPanelThickness * 0.5f : b.max.z + WallPanelThickness * 0.5f;
            CreateCube(parent, "Lower_Wall_Panel", new Vector3(b.center.x, WallPanelHeight * 0.5f, z), new Vector3(b.size.x, WallPanelHeight, WallPanelThickness), wallPanelMaterial, false);
            CreateCube(parent, "Wall_Base_Trim", new Vector3(b.center.x, 0.18f, z + side * 0.02f), new Vector3(b.size.x, WallTrimHeight, WallTrimThickness), trimMaterial, false);
            CreateCube(parent, "Wall_Mid_Trim", new Vector3(b.center.x, WallPanelHeight + 0.08f, z + side * 0.02f), new Vector3(b.size.x, WallTrimHeight, WallTrimThickness), trimMaterial, false);
            CreateCube(parent, "Wall_Top_Trim", new Vector3(b.center.x, 2.75f, z + side * 0.02f), new Vector3(b.size.x, WallTrimHeight, WallTrimThickness), trimMaterial, false);
        }
        else
        {
            float x = side < 0f ? b.min.x - WallPanelThickness * 0.5f : b.max.x + WallPanelThickness * 0.5f;
            CreateCube(parent, "Lower_Wall_Panel", new Vector3(x, WallPanelHeight * 0.5f, b.center.z), new Vector3(WallPanelThickness, WallPanelHeight, b.size.z), wallPanelMaterial, false);
            CreateCube(parent, "Wall_Base_Trim", new Vector3(x + side * 0.02f, 0.18f, b.center.z), new Vector3(WallTrimThickness, WallTrimHeight, b.size.z), trimMaterial, false);
            CreateCube(parent, "Wall_Mid_Trim", new Vector3(x + side * 0.02f, WallPanelHeight + 0.08f, b.center.z), new Vector3(WallTrimThickness, WallTrimHeight, b.size.z), trimMaterial, false);
            CreateCube(parent, "Wall_Top_Trim", new Vector3(x + side * 0.02f, 2.75f, b.center.z), new Vector3(WallTrimThickness, WallTrimHeight, b.size.z), trimMaterial, false);
        }
    }

    private static void GenerateDoorFramesAndThresholds(List<Transform> doors, Transform parent)
    {
        foreach (Transform door in doors)
        {
            Renderer renderer = door.GetComponent<Renderer>();
            if (renderer == null)
                continue;

            Bounds b = renderer.bounds;
            bool xWide = b.size.x >= b.size.z;
            float width = Mathf.Max(xWide ? b.size.x : b.size.z, 1.8f);
            Vector3 c = b.center;
            c.y = 0f;

            if (xWide)
            {
                CreateCube(parent, door.name + "_DoorFrame_Left", new Vector3(c.x - width * 0.5f, 1.15f, c.z), new Vector3(0.18f, 2.3f, 0.3f), doorFrameMaterial, false);
                CreateCube(parent, door.name + "_DoorFrame_Right", new Vector3(c.x + width * 0.5f, 1.15f, c.z), new Vector3(0.18f, 2.3f, 0.3f), doorFrameMaterial, false);
                CreateCube(parent, door.name + "_DoorFrame_Top", new Vector3(c.x, 2.35f, c.z), new Vector3(width + 0.36f, 0.22f, 0.3f), doorFrameMaterial, false);
                CreateCube(parent, door.name + "_Threshold", new Vector3(c.x, 0.08f, c.z), new Vector3(width, 0.08f, 0.55f), doorMarkerMaterial, false);
            }
            else
            {
                CreateCube(parent, door.name + "_DoorFrame_Left", new Vector3(c.x, 1.15f, c.z - width * 0.5f), new Vector3(0.3f, 2.3f, 0.18f), doorFrameMaterial, false);
                CreateCube(parent, door.name + "_DoorFrame_Right", new Vector3(c.x, 1.15f, c.z + width * 0.5f), new Vector3(0.3f, 2.3f, 0.18f), doorFrameMaterial, false);
                CreateCube(parent, door.name + "_DoorFrame_Top", new Vector3(c.x, 2.35f, c.z), new Vector3(0.3f, 0.22f, width + 0.36f), doorFrameMaterial, false);
                CreateCube(parent, door.name + "_Threshold", new Vector3(c.x, 0.08f, c.z), new Vector3(0.55f, 0.08f, width), doorMarkerMaterial, false);
            }
        }
    }

    private static void RecolourExistingVisualPassProps(Transform shellRoot)
    {
        Transform oldVisualPass = shellRoot.Find("GameAI_CA3_Station_VisualPass");

        if (oldVisualPass == null)
            return;

        Renderer[] renderers = oldVisualPass.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            string lower = renderer.name.ToLowerInvariant();

            if (lower.Contains("desk") || lower.Contains("table") || lower.Contains("bench") || lower.Contains("box"))
                renderer.sharedMaterial = woodMaterial;
            else if (lower.Contains("cabinet") || lower.Contains("fixture") || lower.Contains("metal"))
                renderer.sharedMaterial = metalMaterial;
            else if (lower.Contains("paper"))
                renderer.sharedMaterial = paperMaterial;
            else if (lower.Contains("light"))
                renderer.sharedMaterial = warmEmissionMaterial;

            EditorUtility.SetDirty(renderer);
        }
    }

    private static void GenerateExtraProps(List<Transform> floors, Transform parent)
    {
        Bounds b = CalculateBounds(floors);
        Vector3 c = b.center;

        CreateReceptionDesk(parent, new Vector3(c.x, 0f, b.min.z + 8f), 0f);
        CreateDesk(parent, Snap(floors, new Vector3(b.min.x + 10f, 0f, c.z - 2f)), 90f);
        CreateDesk(parent, Snap(floors, new Vector3(b.max.x - 10f, 0f, c.z + 2f)), -90f);
        CreateCabinet(parent, Snap(floors, new Vector3(b.min.x + 6f, 0f, b.max.z - 6f)), 0f);
        CreateCabinet(parent, Snap(floors, new Vector3(b.max.x - 6f, 0f, b.max.z - 6f)), 180f);
        CreateBoxStack(parent, Snap(floors, new Vector3(b.min.x + 7f, 0f, b.min.z + 6f)), 0f);
        CreateBoxStack(parent, Snap(floors, new Vector3(b.max.x - 7f, 0f, b.min.z + 6f)), 0f);
    }

    private static void CreateReceptionDesk(Transform parent, Vector3 pos, float yaw)
    {
        Transform r = CreatePropRoot(parent, "Reception_Desk_Blockout", pos, yaw);
        CreatePropCube(r, "Front", new Vector3(0f, 0.6f, 0f), new Vector3(6f, 1.2f, 0.7f), darkWoodMaterial, true);
        CreatePropCube(r, "Counter", new Vector3(0f, 1.25f, -0.05f), new Vector3(6.4f, 0.18f, 1.1f), woodMaterial, true);
        CreatePropCube(r, "Paper_A", new Vector3(-1.2f, 1.38f, -0.2f), new Vector3(0.7f, 0.03f, 0.45f), paperMaterial, false);
        CreatePropCube(r, "Paper_B", new Vector3(1.1f, 1.38f, 0.1f), new Vector3(0.65f, 0.03f, 0.4f), paperMaterial, false);
    }

    private static void CreateDesk(Transform parent, Vector3 pos, float yaw)
    {
        Transform r = CreatePropRoot(parent, "Office_Desk_Blockout", pos, yaw);
        CreatePropCube(r, "Top", new Vector3(0f, 0.75f, 0f), new Vector3(2.4f, 0.2f, 1.25f), woodMaterial, true);
        CreatePropCube(r, "Left", new Vector3(-0.8f, 0.38f, 0.2f), new Vector3(0.5f, 0.7f, 0.75f), darkWoodMaterial, true);
        CreatePropCube(r, "Right", new Vector3(0.8f, 0.38f, 0.2f), new Vector3(0.5f, 0.7f, 0.75f), darkWoodMaterial, true);
        CreatePropCube(r, "Paper", new Vector3(0.15f, 0.9f, 0.05f), new Vector3(0.6f, 0.03f, 0.42f), paperMaterial, false);
    }

    private static void CreateCabinet(Transform parent, Vector3 pos, float yaw)
    {
        Transform r = CreatePropRoot(parent, "Filing_Cabinet_Blockout", pos, yaw);
        CreatePropCube(r, "Cabinet", new Vector3(0f, 0.7f, 0f), new Vector3(1.1f, 1.4f, 0.8f), metalMaterial, true);
        CreatePropCube(r, "Drawer_A", new Vector3(0f, 0.95f, -0.42f), new Vector3(0.8f, 0.05f, 0.04f), darkMetalMaterial, false);
        CreatePropCube(r, "Drawer_B", new Vector3(0f, 0.55f, -0.42f), new Vector3(0.8f, 0.05f, 0.04f), darkMetalMaterial, false);
    }

    private static void CreateBoxStack(Transform parent, Vector3 pos, float yaw)
    {
        Transform r = CreatePropRoot(parent, "Storage_Box_Stack", pos, yaw);
        CreatePropCube(r, "Box_A", new Vector3(0f, 0.35f, 0f), new Vector3(1.2f, 0.7f, 1f), darkWoodMaterial, true);
        CreatePropCube(r, "Box_B", new Vector3(0.25f, 1.0f, -0.05f), new Vector3(0.9f, 0.6f, 0.85f), woodMaterial, true);
        CreatePropCube(r, "Box_C", new Vector3(-0.55f, 0.32f, 0.85f), new Vector3(0.75f, 0.64f, 0.7f), woodMaterial, true);
    }

    private static void GenerateDecals(List<Transform> floors, Transform parent)
    {
        Bounds b = CalculateBounds(floors);
        Vector3 c = b.center;

        CreateFlatDecal(parent, "Dust_MainHall", new Vector3(c.x + 1f, 0.075f, c.z - 5f), new Vector3(4.5f, 0.035f, 2.4f), dustMaterial, 14f);
        CreateFlatDecal(parent, "Dust_WestWing", new Vector3(b.min.x + 12f, 0.075f, c.z), new Vector3(3.5f, 0.035f, 1.8f), dustMaterial, -20f);
        CreateFlatDecal(parent, "Dark_Stain", new Vector3(c.x - 4f, 0.08f, c.z + 2f), new Vector3(1.9f, 0.035f, 1.2f), bloodMaterial, 33f);

        for (int i = 0; i < Mathf.Min(30, floors.Count); i += 3)
        {
            Transform f = floors[i];
            Vector3 offset = new Vector3(UnityEngine.Random.Range(-1.1f, 1.1f), 0.09f, UnityEngine.Random.Range(-1.1f, 1.1f));
            CreateFlatDecal(parent, "Loose_Paper", f.position + offset, new Vector3(0.55f, 0.03f, 0.38f), paperMaterial, UnityEngine.Random.Range(0f, 360f));
        }
    }

    private static void CreateFlatDecal(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat, float yaw)
    {
        GameObject obj = CreateCube(parent, name, pos, scale, mat, false);
        obj.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    private static void GenerateMoodLighting(List<Transform> floors, List<Transform> ceilings, Transform parent)
    {
        if (floors.Count == 0)
            return;

        Bounds b = CalculateBounds(floors);
        Vector3 c = b.center;

        Vector3[] lightPositions =
        {
            new Vector3(c.x, 2.85f, c.z - 8f),
            new Vector3(c.x, 2.85f, c.z + 2f),
            new Vector3(b.min.x + 12f, 2.85f, c.z),
            new Vector3(b.min.x + 18f, 2.85f, b.max.z - 8f),
            new Vector3(b.max.x - 12f, 2.85f, c.z),
            new Vector3(b.max.x - 12f, 2.85f, b.max.z - 8f)
        };

        foreach (Vector3 p in lightPositions)
            CreateLightFixture(parent, Snap(floors, p));
    }

    private static void CreateLightFixture(Transform parent, Vector3 floorSnappedPosition)
    {
        Vector3 pos = new Vector3(floorSnappedPosition.x, 2.82f, floorSnappedPosition.z);
        CreateCube(parent, "Ceiling_Light_Fixture", pos, new Vector3(2.2f, 0.08f, 0.45f), darkMetalMaterial, false);
        CreateCube(parent, "Ceiling_Light_Emission", pos + new Vector3(0f, -0.055f, 0f), new Vector3(1.8f, 0.05f, 0.28f), warmEmissionMaterial, false);

        GameObject lightObj = new GameObject("Warm_Point_Light");
        lightObj.transform.SetParent(parent);
        lightObj.transform.position = pos + new Vector3(0f, -0.3f, 0f);
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = WarmLight;
        light.intensity = 1.9f;
        light.range = 9f;
        light.shadows = LightShadows.Soft;
    }

    private static void ApplyRenderSettings()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Hex("#1A2020");
        RenderSettings.fog = true;
        RenderSettings.fogColor = Hex("#111415");
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.018f;
    }

    private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool keepCollider)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        obj.isStatic = true;

        if (!keepCollider)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null)
                UnityEngine.Object.DestroyImmediate(col);
        }

        return obj;
    }

    private static Transform CreatePropRoot(Transform parent, string name, Vector3 position, float yaw)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        obj.transform.localScale = Vector3.one;
        return obj.transform;
    }

    private static GameObject CreatePropCube(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material material, bool keepCollider)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = localScale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        obj.isStatic = true;

        if (!keepCollider)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null)
                UnityEngine.Object.DestroyImmediate(col);
        }

        return obj;
    }

    private static Bounds CalculateBounds(List<Transform> transforms)
    {
        if (transforms == null || transforms.Count == 0)
            return new Bounds(Vector3.zero, Vector3.one);

        Renderer firstRenderer = transforms[0].GetComponent<Renderer>();
        Bounds bounds = firstRenderer != null ? firstRenderer.bounds : new Bounds(transforms[0].position, Vector3.one);

        for (int i = 1; i < transforms.Count; i++)
        {
            Renderer renderer = transforms[i].GetComponent<Renderer>();
            if (renderer != null)
                bounds.Encapsulate(renderer.bounds);
        }

        return bounds;
    }

    private static Vector3 Snap(List<Transform> floors, Vector3 desired)
    {
        if (floors == null || floors.Count == 0)
            return desired;

        Transform best = null;
        float bestDistance = float.MaxValue;
        Vector2 desired2 = new Vector2(desired.x, desired.z);

        foreach (Transform floor in floors)
        {
            Vector2 floor2 = new Vector2(floor.position.x, floor.position.z);
            float d = Vector2.Distance(desired2, floor2);

            if (d < bestDistance)
            {
                bestDistance = d;
                best = floor;
            }
        }

        if (best == null)
            return desired;

        return new Vector3(best.position.x, desired.y, best.position.z);
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString(value, out Color color);
        return color;
    }

    private enum TextureKind
    {
        Floor,
        Noise,
        VerticalPanel,
        Carpet,
        Wood,
        Metal
    }
}
