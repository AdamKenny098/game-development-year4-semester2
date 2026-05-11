using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CA3AssetStructureSetup
{
    private const string AdvancedRoot = "Assets/Advanced3D_OutpostCapture";
    private const string GameAIRoot = "Assets/GameAI_CA3";
    private const string SharedRoot = "Assets/Shared";
    private const string ThirdPartyRoot = "Assets/ThirdParty";

    [MenuItem("Tools/CA3 Repo Setup/Create Folder Structure")]
    public static void CreateFolderStructure()
    {
        CreateFolderPath(AdvancedRoot);
        CreateFolderPath($"{AdvancedRoot}/Scenes");
        CreateFolderPath($"{AdvancedRoot}/Scripts");
        CreateFolderPath($"{AdvancedRoot}/Prefabs");
        CreateFolderPath($"{AdvancedRoot}/Resources");
        CreateFolderPath($"{AdvancedRoot}/Generated");
        CreateFolderPath($"{AdvancedRoot}/Editor");

        CreateFolderPath(GameAIRoot);
        CreateFolderPath($"{GameAIRoot}/AI");
        CreateFolderPath($"{GameAIRoot}/Docs");
        CreateFolderPath($"{GameAIRoot}/Materials");
        CreateFolderPath($"{GameAIRoot}/Resources");
        CreateFolderPath($"{GameAIRoot}/Scenes");
        CreateFolderPath($"{GameAIRoot}/Scriptable Objects");
        CreateFolderPath($"{GameAIRoot}/Scripts");
        CreateFolderPath($"{GameAIRoot}/UI");
        CreateFolderPath($"{GameAIRoot}/UnityAssetStore");

        CreateFolderPath(SharedRoot);
        CreateFolderPath(ThirdPartyRoot);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CA3 Setup] Folder structure created.");
    }

    [MenuItem("Tools/CA3 Repo Setup/Move Known Folders")]
    public static void MoveKnownFolders()
    {
        MoveFolderContentsIfExists("Assets/Scenes", $"{AdvancedRoot}/Scenes");
        MoveFolderContentsIfExists("Assets/Scripts", $"{AdvancedRoot}/Scripts");
        MoveFolderContentsIfExists("Assets/Prefabs", $"{AdvancedRoot}/Prefabs");
        MoveFolderContentsIfExists("Assets/Resources", $"{AdvancedRoot}/Resources");
        MoveFolderContentsIfExists("Assets/Generated", $"{AdvancedRoot}/Generated");
        MoveFolderContentsIfExists("Assets/Editor", $"{AdvancedRoot}/Editor");

        MoveFolderIfExists("Assets/Game AI CA2", GameAIRoot);
        MoveFolderIfExists("Assets/GameAI CA2", GameAIRoot);
        MoveFolderIfExists("Assets/Game AI", GameAIRoot);

        MoveFolderIfExists("Assets/Photon", $"{ThirdPartyRoot}/Photon");

        MoveFolderIfExists("Assets/StarterAssets", $"{SharedRoot}/StarterAssets");
        MoveFolderIfExists("Assets/TextMesh Pro", $"{SharedRoot}/TextMesh Pro");
        MoveAssetIfExists("Assets/InputSystem_Actions.inputactions", $"{SharedRoot}/InputSystem_Actions.inputactions");
        MoveFolderIfExists("Assets/Settings", $"{SharedRoot}/Settings");

        DeleteEmptyFolderIfExists("Assets/Scenes");
        DeleteEmptyFolderIfExists("Assets/Scripts");
        DeleteEmptyFolderIfExists("Assets/Prefabs");
        DeleteEmptyFolderIfExists("Assets/Resources");
        DeleteEmptyFolderIfExists("Assets/Generated");
        DeleteEmptyFolderIfExists("Assets/Editor");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CA3 Setup] Known folders moved.");
    }

    [MenuItem("Tools/CA3 Repo Setup/Full Setup")]
    public static void FullSetup()
    {
        CreateFolderStructure();
        MoveKnownFolders();

        Debug.Log("[CA3 Setup] Full setup complete.");
    }

    private static void CreateFolderPath(string fullPath)
    {
        if (AssetDatabase.IsValidFolder(fullPath))
            return;

        string[] parts = fullPath.Split('/');
        string currentPath = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string nextPath = $"{currentPath}/{parts[i]}";

            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, parts[i]);
            }

            currentPath = nextPath;
        }
    }

    private static void MoveFolderIfExists(string sourcePath, string targetPath)
    {
        if (!AssetDatabase.IsValidFolder(sourcePath))
            return;

        if (AssetDatabase.IsValidFolder(targetPath))
        {
            MoveFolderContentsIfExists(sourcePath, targetPath);
            DeleteEmptyFolderIfExists(sourcePath);
            return;
        }

        string targetParent = Path.GetDirectoryName(targetPath)?.Replace("\\", "/");
        string targetName = Path.GetFileName(targetPath);

        if (string.IsNullOrWhiteSpace(targetParent) || string.IsNullOrWhiteSpace(targetName))
        {
            Debug.LogWarning($"[CA3 Setup] Invalid target path: {targetPath}");
            return;
        }

        CreateFolderPath(targetParent);

        string result = AssetDatabase.MoveAsset(sourcePath, targetPath);

        if (!string.IsNullOrEmpty(result))
        {
            Debug.LogWarning($"[CA3 Setup] Could not move {sourcePath} to {targetPath}: {result}");
        }
        else
        {
            Debug.Log($"[CA3 Setup] Moved {sourcePath} to {targetPath}");
        }
    }

    private static void MoveFolderContentsIfExists(string sourcePath, string targetPath)
    {
        if (!AssetDatabase.IsValidFolder(sourcePath))
            return;

        CreateFolderPath(targetPath);

        string[] assetPaths = AssetDatabase.FindAssets("", new[] { sourcePath });

        List<string> directChildren = new List<string>();

        foreach (string guid in assetPaths)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (assetPath == sourcePath)
                continue;

            string parent = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");

            if (parent == sourcePath)
                directChildren.Add(assetPath);
        }

        foreach (string childPath in directChildren)
        {
            string childName = Path.GetFileName(childPath);
            string destinationPath = $"{targetPath}/{childName}";

            MoveAssetIfExists(childPath, destinationPath);
        }
    }

    private static void MoveAssetIfExists(string sourcePath, string targetPath)
    {
        if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath) && !AssetDatabase.IsValidFolder(sourcePath))
            return;

        string targetParent = Path.GetDirectoryName(targetPath)?.Replace("\\", "/");

        if (!string.IsNullOrWhiteSpace(targetParent))
            CreateFolderPath(targetParent);

        if (File.Exists(targetPath) || Directory.Exists(targetPath) || AssetDatabase.IsValidFolder(targetPath))
        {
            Debug.LogWarning($"[CA3 Setup] Target already exists, skipping: {targetPath}");
            return;
        }

        string result = AssetDatabase.MoveAsset(sourcePath, targetPath);

        if (!string.IsNullOrEmpty(result))
        {
            Debug.LogWarning($"[CA3 Setup] Could not move {sourcePath} to {targetPath}: {result}");
        }
        else
        {
            Debug.Log($"[CA3 Setup] Moved {sourcePath} to {targetPath}");
        }
    }

    private static void DeleteEmptyFolderIfExists(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
            return;

        string[] contents = AssetDatabase.FindAssets("", new[] { folderPath });

        if (contents.Length > 0)
            return;

        bool deleted = AssetDatabase.DeleteAsset(folderPath);

        if (deleted)
            Debug.Log($"[CA3 Setup] Deleted empty folder: {folderPath}");
    }
} 