using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class CA3RenderingPolishPass
{
    private const string RootName = "CA3_RenderingPolish";
    private const string GeneratedFolder = "Assets/Generated";
    private const string MaterialFolder = "Assets/Generated/CA3RenderingMaterials";
    private const string VolumeProfilePath = "Assets/Generated/CA3RenderingMaterials/CA3_GlobalVolumeProfile.asset";

    [MenuItem("Tools/CA3/Apply Rendering Polish Pass")]
    public static void ApplyPolishPass()
    {
        if (!EditorUtility.DisplayDialog(
                "Apply CA3 Rendering Polish Pass",
                "This will create a recording camera, URP post-processing volume, lighting pass, and material pass. It will not change gameplay scripts.",
                "Apply",
                "Cancel"))
        {
            return;
        }

        EnsureFolders();
        DeleteExistingPolishRoot();

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Create CA3 Rendering Polish Root");

        Material floorMat = CreateOrLoadMaterial("MAT_CA3_Floor_DarkConcrete", "#30363D", 0f, false);
        Material wallMat = CreateOrLoadMaterial("MAT_CA3_Walls_Dark", "#1E2229", 0f, false);
        Material foundationMat = CreateOrLoadMaterial("MAT_CA3_Foundation", "#25282D", 0f, false);
        Material coverMat = CreateOrLoadMaterial("MAT_CA3_Cover_WarmConcrete", "#5A4E45", 0f, false);
        Material blueMat = CreateOrLoadMaterial("MAT_CA3_Blue_Emission", "#1E6BFF", 1.1f, true);
        Material redMat = CreateOrLoadMaterial("MAT_CA3_Red_Emission", "#D82F5E", 1.1f, true);
        Material captureMat = CreateOrLoadMaterial("MAT_CA3_Capture_Amber", "#F7B733", 1.35f, true);
        Material guardMat = CreateOrLoadMaterial("MAT_CA3_NeutralGuard", "#B82929", 0.4f, true);

        ApplyMaterialPass(floorMat, wallMat, foundationMat, coverMat, blueMat, redMat, captureMat, guardMat);
        ApplyLightingPass();
        CreatePostProcessingVolume(root.transform);
        CreateRecordingCamera(root.transform);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ParseColor("#171A20");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "CA3 Rendering Polish Applied",
            "Rendering polish pass complete. Test the scene, capture screenshots, then commit.",
            "OK");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(GeneratedFolder))
            AssetDatabase.CreateFolder("Assets", "Generated");

        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder(GeneratedFolder, "CA3RenderingMaterials");
    }

    private static void DeleteExistingPolishRoot()
    {
        GameObject existing = GameObject.Find(RootName);

        if (existing != null)
            Undo.DestroyObjectImmediate(existing);
    }

    private static Material CreateOrLoadMaterial(string name, string hex, float emissionIntensity, bool emission)
    {
        string path = $"{MaterialFolder}/{name}.mat";
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

        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", 0f);

        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.35f);

        if (emission)
        {
            Color emissionColor = color * emissionIntensity;
            material.EnableKeyword("_EMISSION");

            if (material.HasProperty("_EmissionColor"))
                material.SetColor("_EmissionColor", emissionColor);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ApplyMaterialPass(
        Material floorMat,
        Material wallMat,
        Material foundationMat,
        Material coverMat,
        Material blueMat,
        Material redMat,
        Material captureMat,
        Material guardMat)
    {
        ApplyMaterialToGroup("01_Floors", floorMat);
        ApplyMaterialToGroup("01B_Foundation_And_Outer_Enclosure", foundationMat);
        ApplyMaterialToGroup("02_Outer_Walls", wallMat);
        ApplyMaterialToGroup("03_Interior_Walls", wallMat);
        ApplyMaterialToGroup("06_Cover_And_Market_Blocks", coverMat);

        ApplyMaterialByName("BlueSpawn_DebugZone", blueMat);
        ApplyMaterialByName("RedSpawn_DebugZone", redMat);

        ApplyMaterialByName("Capture_Platform", captureMat);
        ApplyMaterialByName("Capture_Platform_Diamond", captureMat);
        ApplyMaterialByName("Capture_Beacon", captureMat);

        ApplyMaterialByName("NPC_Guard_Placeholder", guardMat);
        ApplyMaterialByName("NetworkedNeutralGuard", guardMat);
        ApplyMaterialByName("NetworkedNeutralGuard(Clone)", guardMat);
    }

    private static void ApplyMaterialToGroup(string groupName, Material material)
    {
        GameObject group = GameObject.Find(groupName);

        if (group == null)
            return;

        Renderer[] renderers = group.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
            renderer.sharedMaterial = material;
    }

    private static void ApplyMaterialByName(string objectName, Material material)
    {
        GameObject obj = GameObject.Find(objectName);

        if (obj == null)
            return;

        Renderer renderer = obj.GetComponentInChildren<Renderer>(true);

        if (renderer != null)
            renderer.sharedMaterial = material;
    }

    private static void ApplyLightingPass()
    {
        SetDirectionalLight();
        SetPointLight("Blue_Spawn_Light", "#1E6BFF", 1.8f, 16f, LightShadows.None);
        SetPointLight("Red_Spawn_Light", "#D82F5E", 1.8f, 16f, LightShadows.None);
        SetPointLight("Capture_Point_Light", "#F7B733", 2.4f, 15f, LightShadows.None);
        SetPointLight("Central_Room_Light", "#F0E3C0", 1.15f, 16f, LightShadows.None);
    }

    private static void SetDirectionalLight()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Light light in lights)
        {
            if (light.type != LightType.Directional)
                continue;

            light.name = "CA3_Directional_Key_Light";
            light.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
            light.color = ParseColor("#B7C8FF");
            light.intensity = 0.75f;
            light.shadows = LightShadows.Soft;
            return;
        }

        GameObject lightObject = new GameObject("CA3_Directional_Key_Light");
        Light newLight = lightObject.AddComponent<Light>();

        lightObject.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

        newLight.type = LightType.Directional;
        newLight.color = ParseColor("#B7C8FF");
        newLight.intensity = 0.75f;
        newLight.shadows = LightShadows.Soft;

        Undo.RegisterCreatedObjectUndo(lightObject, "Create CA3 Directional Light");
    }

    private static void SetPointLight(string name, string hex, float intensity, float range, LightShadows shadows)
    {
        GameObject obj = GameObject.Find(name);

        if (obj == null)
            return;

        Light light = obj.GetComponent<Light>();

        if (light == null)
            return;

        light.color = ParseColor(hex);
        light.intensity = intensity;
        light.range = range;
        light.shadows = shadows;
    }

    private static void CreatePostProcessingVolume(Transform parent)
    {
        GameObject volumeObject = new GameObject("CA3_Global_PostProcessing");
        volumeObject.transform.SetParent(parent);

        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);

        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, VolumeProfilePath);
        }

        SetupVolumeProfile(profile);
        volume.profile = profile;

        Undo.RegisterCreatedObjectUndo(volumeObject, "Create CA3 Global Volume");
        EditorUtility.SetDirty(profile);
    }

    private static void SetupVolumeProfile(VolumeProfile profile)
    {
        if (!profile.TryGet(out Bloom bloom))
            bloom = profile.Add<Bloom>(true);

        bloom.active = true;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = 0.45f;
        bloom.threshold.overrideState = true;
        bloom.threshold.value = 1.05f;
        bloom.scatter.overrideState = true;
        bloom.scatter.value = 0.55f;

        if (!profile.TryGet(out Vignette vignette))
            vignette = profile.Add<Vignette>(true);

        vignette.active = true;
        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0.22f;
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = 0.45f;

        if (!profile.TryGet(out ColorAdjustments colorAdjustments))
            colorAdjustments = profile.Add<ColorAdjustments>(true);

        colorAdjustments.active = true;
        colorAdjustments.contrast.overrideState = true;
        colorAdjustments.contrast.value = 18f;
        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.saturation.value = -2f;
        colorAdjustments.colorFilter.overrideState = true;
        colorAdjustments.colorFilter.value = ParseColor("#DCE6FF");

        if (!profile.TryGet(out FilmGrain filmGrain))
            filmGrain = profile.Add<FilmGrain>(true);

        filmGrain.active = true;
        filmGrain.intensity.overrideState = true;
        filmGrain.intensity.value = 0.12f;
        filmGrain.response.overrideState = true;
        filmGrain.response.value = 0.65f;
    }

    private static void CreateRecordingCamera(Transform parent)
    {
        DisableExistingGameplayCameras();

        GameObject cameraObject = new GameObject("CA3_Recording_Camera");
        cameraObject.transform.SetParent(parent);

        cameraObject.transform.position = new Vector3(0f, 32f, -34f);
        cameraObject.transform.rotation = Quaternion.Euler(58f, 0f, 0f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = ParseColor("#090B10");
        camera.orthographic = false;
        camera.fieldOfView = 48f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 250f;
        camera.allowHDR = true;
        camera.enabled = true;

        UniversalAdditionalCameraData cameraData = cameraObject.GetComponent<UniversalAdditionalCameraData>();

        if (cameraData == null)
            cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();

        cameraData.renderPostProcessing = true;
        cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;

        if (Object.FindFirstObjectByType<AudioListener>() == null)
            cameraObject.AddComponent<AudioListener>();

        Undo.RegisterCreatedObjectUndo(cameraObject, "Create CA3 Recording Camera");
    }

    private static void DisableExistingGameplayCameras()
    {
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Camera camera in cameras)
        {
            if (camera == null)
                continue;

            camera.enabled = false;
        }
    }

    private static Color ParseColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;

        return Color.magenta;
    }
}