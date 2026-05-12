using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GameAIStationLevelShellGenerator
{
    private const string RootName = "GameAI_CA3_Station_LevelShell";

    private const float CellSize = 4.0f;
    private const float FloorThickness = 0.18f;
    private const float CeilingThickness = 0.18f;
    private const float WallHeight = 3.6f;
    private const float WallThickness = 0.28f;
    private const float DoorMarkerThickness = 0.08f;
    private const float DoorOpeningWidth = 2.4f;
    private const float DoorSideWallMinimum = 0.15f;
    private const float DoorMarkerLength = DoorOpeningWidth;
    private const float DoorMarkerWidth = 0.55f;

    private const bool GenerateCeilings = true;
    private const bool CeilingsVisibleByDefault = false;

    private static readonly Dictionary<Vector2Int, CellData> Cells = new Dictionary<Vector2Int, CellData>();
    private static readonly Dictionary<GridEdge, DoorData> DoorEdges = new Dictionary<GridEdge, DoorData>();
    private static readonly HashSet<GridEdge> OpenEdges = new HashSet<GridEdge>();
    private static readonly HashSet<GridEdge> WallEdges = new HashSet<GridEdge>();
    private static readonly List<AreaData> Areas = new List<AreaData>();
    private static readonly List<string> ValidationErrors = new List<string>();
    private static readonly List<string> ValidationWarnings = new List<string>();

    private static Transform root;
    private static Transform floorsRoot;
    private static Transform wallsRoot;
    private static Transform ceilingsRoot;
    private static Transform doorsRoot;
    private static Transform debugRoot;

    private static Material floorMaterial;
    private static Material wallMaterial;
    private static Material ceilingMaterial;
    private static Material doorMaterial;

    [MenuItem("Tools/Game AI CA3/Station Shell/Generate Large Station Shell")]
    public static void GenerateLargeStationShell()
    {
        ClearExistingShell();

        ResetState();
        BuildMaterialReferences();
        BuildAreaDefinitions();
        BuildDoorDefinitions();
        BuildOpenConnectionDefinitions();
        ValidateLayout();

        if (ValidationErrors.Count > 0)
        {
            PrintValidationMessages();
            EditorUtility.DisplayDialog(
                "Station Shell Validation Failed",
                "The station shell was not generated because validation errors were found. Check the Console.",
                "OK"
            );
            return;
        }

        CreateRootObjects();
        GenerateFloors();
        GenerateCeiling();
        GenerateWalls();
        GenerateDoorWallStubs();
        GenerateDoorMarkers();
        FinaliseHierarchy();
        PrintValidationMessages();

        Selection.activeGameObject = root.gameObject;

        Debug.Log("[Game AI CA3] Large station level shell generated successfully.");
    }

    [MenuItem("Tools/Game AI CA3/Station Shell/Clear Large Station Shell")]
    public static void ClearExistingShell()
    {
        GameObject existing = GameObject.Find(RootName);

        if (existing != null)
        {
            UnityEngine.Object.DestroyImmediate(existing);
            Debug.Log("[Game AI CA3] Existing station shell cleared.");
        }
    }

    [MenuItem("Tools/Game AI CA3/Station Shell/Validate Layout Only")]
    public static void ValidateOnly()
    {
        ResetState();
        BuildAreaDefinitions();
        BuildDoorDefinitions();
        BuildOpenConnectionDefinitions();
        ValidateLayout();
        PrintValidationMessages();

        if (ValidationErrors.Count == 0)
            Debug.Log("[Game AI CA3] Layout validation passed.");
    }

    private static void ResetState()
    {
        Cells.Clear();
        DoorEdges.Clear();
        OpenEdges.Clear();
        WallEdges.Clear();
        Areas.Clear();
        ValidationErrors.Clear();
        ValidationWarnings.Clear();

        root = null;
        floorsRoot = null;
        wallsRoot = null;
        ceilingsRoot = null;
        doorsRoot = null;
        debugRoot = null;
    }

    private static void BuildAreaDefinitions()
    {
        AddArea(
            "EntranceVestibule",
            -2,
            -12,
            5,
            3,
            AreaKind.Room
        );

        AddArea(
            "MainHall",
            -3,
            -9,
            7,
            12,
            AreaKind.Hall
        );

        AddArea(
            "NorthGallery",
            -4,
            3,
            9,
            3,
            AreaKind.Hall
        );

        AddArea(
            "WestAccessHall",
            -7,
            -5,
            4,
            2,
            AreaKind.Corridor
        );

        AddArea(
            "WestMainCorridor",
            -14,
            -5,
            7,
            2,
            AreaKind.Corridor
        );

        AddArea(
            "WestLowerCorridor",
            -14,
            -12,
            4,
            7,
            AreaKind.Corridor
        );

        AddArea(
            "WestVerticalCorridor",
            -14,
            -3,
            1,
            8,
            AreaKind.Corridor
        );

        AddArea(
            "WestNorthCorridor",
            -13,
            4,
            9,
            2,
            AreaKind.Corridor
        );

        AddArea(
            "WestOffice",
            -20,
            -7,
            6,
            5,
            AreaKind.Room
        );

        AddArea(
            "DarkRoom",
            -20,
            -12,
            6,
            5,
            AreaKind.Room
        );

        AddArea(
            "EvidenceRoom",
            -10,
            -12,
            3,
            5,
            AreaKind.Room
        );

        AddArea(
            "OperationsRoom",
            -20,
            4,
            6,
            5,
            AreaKind.Room
        );

        AddArea(
            "WestStorage",
            -13,
            6,
            4,
            4,
            AreaKind.Room
        );

        AddArea(
            "Armory",
            -8,
            6,
            4,
            4,
            AreaKind.Room
        );

        AddArea(
            "EastAccessHall",
            4,
            -5,
            3,
            2,
            AreaKind.Corridor
        );

        AddArea(
            "EastMainCorridor",
            7,
            -5,
            6,
            2,
            AreaKind.Corridor
        );

        AddArea(
            "EastLowerCorridor",
            13,
            -8,
            2,
            4,
            AreaKind.Corridor
        );

        AddArea(
            "EastVerticalCorridor",
            13,
            -4,
            2,
            8,
            AreaKind.Corridor
        );

        AddArea(
            "EastNorthCorridor",
            5,
            4,
            9,
            2,
            AreaKind.Corridor
        );

        AddArea(
            "EastOffice",
            15,
            -8,
            6,
            5,
            AreaKind.Room
        );

        AddArea(
            "InterrogationRoom",
            21,
            -8,
            4,
            3,
            AreaKind.Room
        );

        AddArea(
            "ObservationRoom",
            21,
            -4,
            4,
            3,
            AreaKind.Room
        );

        AddArea(
            "WaitingRoom",
            15,
            1,
            6,
            5,
            AreaKind.Room
        );

        AddArea(
            "RecordsEast",
            5,
            6,
            6,
            4,
            AreaKind.Room
        );

        AddArea(
            "BreakRoom",
            15,
            6,
            6,
            4,
            AreaKind.Room
        );

        AddArea(
            "EastStorage",
            21,
            2,
            4,
            4,
            AreaKind.Room
        );

        AddArea(
            "ReceptionOffice",
            -7,
            -12,
            5,
            3,
            AreaKind.Room
        );

        AddArea(
            "SecurityOffice",
            3,
            -12,
            5,
            3,
            AreaKind.Room
        );
    }

    private static void BuildDoorDefinitions()
    {
        AddDoor(
            "Door_Entrance_To_MainHall",
            new Vector2Int(0, -9),
            Direction.South
        );

        AddDoor(
            "Door_Entrance_To_Reception",
            new Vector2Int(-2, -11),
            Direction.West
        );

        AddDoor(
            "Door_Entrance_To_Security",
            new Vector2Int(2, -11),
            Direction.East
        );

        AddDoor(
            "Door_Main_To_NorthGallery",
            new Vector2Int(0, 2),
            Direction.North
        );

        AddDoor(
            "Door_Main_To_WestAccess",
            new Vector2Int(-3, -5),
            Direction.West
        );

        AddDoor(
            "Door_WestAccess_To_WestCorridor",
            new Vector2Int(-7, -5),
            Direction.West
        );

        AddDoor(
            "Door_WestCorridor_To_WestLower",
            new Vector2Int(-14, -5),
            Direction.South
        );

        AddDoor(
            "Door_WestCorridor_To_WestVertical",
            new Vector2Int(-14, -4),
            Direction.North
        );

        AddDoor(
            "Door_WestCorridor_To_WestOffice",
            new Vector2Int(-14, -5),
            Direction.West
        );

        AddDoor(
            "Door_WestLower_To_DarkRoom",
            new Vector2Int(-14, -10),
            Direction.West
        );

        AddDoor(
            "Door_WestLower_To_Evidence",
            new Vector2Int(-11, -10),
            Direction.East
        );

        AddDoor(
            "Door_WestVertical_To_Operations",
            new Vector2Int(-14, 4),
            Direction.West
        );

        AddDoor(
            "Door_WestVertical_To_WestNorth",
            new Vector2Int(-14, 4),
            Direction.East
        );

        AddDoor(
            "Door_WestNorth_To_WestStorage",
            new Vector2Int(-11, 5),
            Direction.North
        );

        AddDoor(
            "Door_WestNorth_To_Armory",
            new Vector2Int(-6, 5),
            Direction.North
        );

        AddDoor(
            "Door_NorthGallery_To_WestNorth",
            new Vector2Int(-4, 4),
            Direction.West
        );

        AddDoor(
            "Door_Main_To_EastAccess",
            new Vector2Int(3, -5),
            Direction.East
        );

        AddDoor(
            "Door_EastAccess_To_EastCorridor",
            new Vector2Int(6, -5),
            Direction.East
        );

        AddDoor(
            "Door_EastCorridor_To_EastLower",
            new Vector2Int(12, -5),
            Direction.East
        );

        AddDoor(
            "Door_EastCorridor_To_EastVertical",
            new Vector2Int(12, -4),
            Direction.East
        );

        AddDoor(
            "Door_EastLower_To_EastOffice",
            new Vector2Int(14, -7),
            Direction.East
        );

        AddDoor(
            "Door_EastOffice_To_Interrogation",
            new Vector2Int(20, -7),
            Direction.East
        );

        AddDoor(
            "Door_EastOffice_To_Observation",
            new Vector2Int(20, -4),
            Direction.East
        );

        AddDoor(
            "Door_EastVertical_To_Waiting",
            new Vector2Int(14, 2),
            Direction.East
        );

        AddDoor(
            "Door_EastVertical_To_EastNorth",
            new Vector2Int(13, 3),
            Direction.North
        );

        AddDoor(
            "Door_NorthGallery_To_EastNorth",
            new Vector2Int(4, 4),
            Direction.East
        );

        AddDoor(
            "Door_EastNorth_To_Records",
            new Vector2Int(8, 5),
            Direction.North
        );

        AddDoor(
            "Door_Waiting_To_BreakRoom",
            new Vector2Int(17, 5),
            Direction.North
        );

        AddDoor(
            "Door_Waiting_To_EastStorage",
            new Vector2Int(20, 3),
            Direction.East
        );
    }

    private static void BuildOpenConnectionDefinitions()
    {
        AddOpenConnection(
            "Wide_MainHall_Entrance_Left",
            new Vector2Int(-1, -9),
            Direction.South
        );

        AddOpenConnection(
            "Wide_MainHall_Entrance_Right",
            new Vector2Int(1, -9),
            Direction.South
        );

        AddOpenConnection(
            "Wide_MainHall_North_Left",
            new Vector2Int(-1, 2),
            Direction.North
        );

        AddOpenConnection(
            "Wide_MainHall_North_Right",
            new Vector2Int(1, 2),
            Direction.North
        );

        AddOpenConnection(
            "Wide_WestAccess_Main_Lane",
            new Vector2Int(-3, -4),
            Direction.West
        );

        AddOpenConnection(
            "Wide_WestAccess_Corridor_Lane",
            new Vector2Int(-7, -4),
            Direction.West
        );

        AddOpenConnection(
            "Wide_EastAccess_Main_Lane",
            new Vector2Int(3, -4),
            Direction.East
        );

        AddOpenConnection(
            "Wide_EastAccess_Corridor_Lane",
            new Vector2Int(6, -4),
            Direction.East
        );

        AddOpenConnection(
            "Wide_NorthGallery_West_Lane",
            new Vector2Int(-4, 5),
            Direction.West
        );

        AddOpenConnection(
            "Wide_NorthGallery_East_Lane",
            new Vector2Int(4, 5),
            Direction.East
        );
    }

    private static void AddArea(
        string areaName,
        int startX,
        int startZ,
        int width,
        int depth,
        AreaKind kind
    )
    {
        if (width <= 0)
        {
            ValidationErrors.Add($"Area '{areaName}' has invalid width: {width}");
            return;
        }

        if (depth <= 0)
        {
            ValidationErrors.Add($"Area '{areaName}' has invalid depth: {depth}");
            return;
        }

        AreaData area = new AreaData(
            areaName,
            startX,
            startZ,
            width,
            depth,
            kind
        );

        Areas.Add(area);

        for (int x = startX; x < startX + width; x++)
        {
            for (int z = startZ; z < startZ + depth; z++)
            {
                Vector2Int cell = new Vector2Int(x, z);

                if (Cells.ContainsKey(cell))
                {
                    string existing = Cells[cell].AreaName;
                    ValidationErrors.Add(
                        $"Cell overlap at {cell}. '{areaName}' overlaps existing area '{existing}'."
                    );
                    continue;
                }

                Cells.Add(
                    cell,
                    new CellData(
                        areaName,
                        kind
                    )
                );
            }
        }
    }

    private static void AddDoor(
        string doorName,
        Vector2Int cell,
        Direction direction
    )
    {
        GridEdge edge = GridEdge.FromCellAndDirection(
            cell,
            direction
        );

        if (DoorEdges.ContainsKey(edge))
        {
            ValidationErrors.Add(
                $"Duplicate door edge '{doorName}' conflicts with '{DoorEdges[edge].Name}'."
            );
            return;
        }

        DoorEdges.Add(
            edge,
            new DoorData(
                doorName,
                edge,
                direction
            )
        );
    }

    private static void AddOpenConnection(
        string connectionName,
        Vector2Int cell,
        Direction direction
    )
    {
        GridEdge edge = GridEdge.FromCellAndDirection(
            cell,
            direction
        );

        if (OpenEdges.Contains(edge))
        {
            ValidationWarnings.Add(
                $"Duplicate open connection ignored: '{connectionName}'."
            );
            return;
        }

        if (DoorEdges.ContainsKey(edge))
        {
            ValidationErrors.Add(
                $"Open connection '{connectionName}' conflicts with door '{DoorEdges[edge].Name}'."
            );
            return;
        }

        OpenEdges.Add(edge);
    }

    private static void ValidateLayout()
    {
        ValidateAreaCount();
        ValidateDoorEdges();
        ValidateOpenEdges();
        ValidateConnectivity();
        CalculateWallEdges();
        ValidateWallEdgeDuplicates();
    }

    private static void ValidateAreaCount()
    {
        if (Areas.Count == 0)
            ValidationErrors.Add("No areas were defined.");

        if (Cells.Count == 0)
            ValidationErrors.Add("No occupied cells were created.");
    }

    private static void ValidateDoorEdges()
    {
        foreach (DoorData door in DoorEdges.Values)
        {
            Vector2Int a = door.Edge.CellA;
            Vector2Int b = door.Edge.CellB;

            if (!Cells.ContainsKey(a))
            {
                ValidationErrors.Add(
                    $"Door '{door.Name}' has no occupied cell A at {a}."
                );
            }

            if (!Cells.ContainsKey(b))
            {
                ValidationErrors.Add(
                    $"Door '{door.Name}' has no occupied cell B at {b}."
                );
            }

            if (Cells.ContainsKey(a) && Cells.ContainsKey(b))
            {
                string areaA = Cells[a].AreaName;
                string areaB = Cells[b].AreaName;

                if (areaA == areaB)
                {
                    ValidationWarnings.Add(
                        $"Door '{door.Name}' connects two cells in the same area '{areaA}'. It will still be generated."
                    );
                }
            }
        }
    }

    private static void ValidateOpenEdges()
    {
        foreach (GridEdge edge in OpenEdges)
        {
            if (!Cells.ContainsKey(edge.CellA))
            {
                ValidationErrors.Add(
                    $"Open connection has no occupied cell A at {edge.CellA}."
                );
            }

            if (!Cells.ContainsKey(edge.CellB))
            {
                ValidationErrors.Add(
                    $"Open connection has no occupied cell B at {edge.CellB}."
                );
            }
        }
    }

    private static void ValidateConnectivity()
    {
        if (Cells.Count == 0)
            return;

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        Vector2Int start = default;
        bool foundStart = false;

        foreach (Vector2Int cell in Cells.Keys)
        {
            start = cell;
            foundStart = true;
            break;
        }

        if (!foundStart)
            return;

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            TryVisitNeighbour(current, Direction.North, visited, queue);
            TryVisitNeighbour(current, Direction.South, visited, queue);
            TryVisitNeighbour(current, Direction.East, visited, queue);
            TryVisitNeighbour(current, Direction.West, visited, queue);
        }

        if (visited.Count != Cells.Count)
        {
            ValidationWarnings.Add(
                $"Connectivity check: {Cells.Count - visited.Count} cells are not reachable through same-area links, doors, or open connections."
            );
        }
    }

    private static void TryVisitNeighbour(
        Vector2Int cell,
        Direction direction,
        HashSet<Vector2Int> visited,
        Queue<Vector2Int> queue
    )
    {
        Vector2Int neighbour = cell + DirectionToVector(direction);

        if (!Cells.ContainsKey(neighbour))
            return;

        if (visited.Contains(neighbour))
            return;

        if (!CanMoveBetween(cell, neighbour))
            return;

        visited.Add(neighbour);
        queue.Enqueue(neighbour);
    }

    private static bool CanMoveBetween(
        Vector2Int a,
        Vector2Int b
    )
    {
        if (!Cells.ContainsKey(a))
            return false;

        if (!Cells.ContainsKey(b))
            return false;

        if (Cells[a].AreaName == Cells[b].AreaName)
            return true;

        GridEdge edge = GridEdge.FromTwoCells(a, b);

        if (DoorEdges.ContainsKey(edge))
            return true;

        if (OpenEdges.Contains(edge))
            return true;

        return false;
    }

    private static void CalculateWallEdges()
    {
        WallEdges.Clear();

        foreach (Vector2Int cell in Cells.Keys)
        {
            EvaluateWallEdge(cell, Direction.North);
            EvaluateWallEdge(cell, Direction.South);
            EvaluateWallEdge(cell, Direction.East);
            EvaluateWallEdge(cell, Direction.West);
        }
    }

    private static void EvaluateWallEdge(
        Vector2Int cell,
        Direction direction
    )
    {
        Vector2Int neighbour = cell + DirectionToVector(direction);
        GridEdge edge = GridEdge.FromCellAndDirection(
            cell,
            direction
        );

        if (!Cells.ContainsKey(neighbour))
        {
            WallEdges.Add(edge);
            return;
        }

        if (Cells[cell].AreaName == Cells[neighbour].AreaName)
            return;

        if (DoorEdges.ContainsKey(edge))
            return;

        if (OpenEdges.Contains(edge))
            return;

        WallEdges.Add(edge);
    }

    private static void ValidateWallEdgeDuplicates()
    {
        HashSet<GridEdge> seen = new HashSet<GridEdge>();

        foreach (GridEdge edge in WallEdges)
        {
            if (!seen.Add(edge))
            {
                ValidationErrors.Add(
                    $"Duplicate wall edge detected at {edge}."
                );
            }
        }
    }

    private static void BuildMaterialReferences()
    {
        floorMaterial = GetOrCreateMaterial(
            "MAT_GameAI_Shell_Floor",
            new Color(0.22f, 0.22f, 0.22f, 1f)
        );

        wallMaterial = GetOrCreateMaterial(
            "MAT_GameAI_Shell_Wall",
            new Color(0.12f, 0.12f, 0.12f, 1f)
        );

        ceilingMaterial = GetOrCreateMaterial(
            "MAT_GameAI_Shell_Ceiling",
            new Color(0.16f, 0.16f, 0.16f, 1f)
        );

        doorMaterial = GetOrCreateMaterial(
            "MAT_GameAI_Shell_DoorMarker",
            new Color(0.95f, 0.66f, 0.08f, 1f)
        );
    }

    private static void CreateRootObjects()
    {
        root = new GameObject(RootName).transform;

        floorsRoot = CreateChildRoot(
            "Floors"
        );

        wallsRoot = CreateChildRoot(
            "Walls"
        );

        ceilingsRoot = CreateChildRoot(
            "Ceilings"
        );

        doorsRoot = CreateChildRoot(
            "DoorMarkers"
        );

        debugRoot = CreateChildRoot(
            "GeneratedDebug"
        );
    }

    private static Transform CreateChildRoot(
        string childName
    )
    {
        GameObject child = new GameObject(childName);
        child.transform.SetParent(root);
        return child.transform;
    }

    private static void GenerateFloors()
    {
        foreach (KeyValuePair<Vector2Int, CellData> entry in Cells)
        {
            CreateFloorTile(
                entry.Key,
                entry.Value
            );
        }
    }

    private static void CreateFloorTile(
        Vector2Int cell,
        CellData data
    )
    {
        Vector3 position = CellToWorldCenter(cell);
        position.y = -FloorThickness * 0.5f;

        GameObject tile = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        tile.name = $"Floor_{data.AreaName}_{cell.x}_{cell.y}";
        tile.transform.SetParent(floorsRoot);
        tile.transform.position = position;
        tile.transform.localScale = new Vector3(
            CellSize,
            FloorThickness,
            CellSize
        );

        Renderer renderer = tile.GetComponent<Renderer>();

        if (renderer != null)
            renderer.sharedMaterial = floorMaterial;

        tile.isStatic = true;
    }

    private static void GenerateCeiling()
    {
        if (!GenerateCeilings)
            return;

        foreach (KeyValuePair<Vector2Int, CellData> entry in Cells)
        {
            CreateCeilingTile(
                entry.Key,
                entry.Value
            );
        }
    }

    private static void CreateCeilingTile(
        Vector2Int cell,
        CellData data
    )
    {
        Vector3 position = CellToWorldCenter(cell);
        position.y = WallHeight + CeilingThickness * 0.5f;

        GameObject tile = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        tile.name = $"Ceiling_{data.AreaName}_{cell.x}_{cell.y}";
        tile.transform.SetParent(ceilingsRoot);
        tile.transform.position = position;
        tile.transform.localScale = new Vector3(
            CellSize,
            CeilingThickness,
            CellSize
        );

        Renderer renderer = tile.GetComponent<Renderer>();

        if (renderer != null)
            renderer.sharedMaterial = ceilingMaterial;

        tile.isStatic = true;
    }

    private static void GenerateWalls()
    {
        foreach (GridEdge edge in WallEdges)
        {
            CreateWall(edge);
        }
    }

    private static void GenerateDoorWallStubs()
    {
        foreach (DoorData door in DoorEdges.Values)
        {
            CreateDoorWallStubs(door);
        }
    }

    private static void CreateDoorWallStubs(
        DoorData door
    )
    {
        GridEdge edge = door.Edge;
        float sideLength = (CellSize - DoorOpeningWidth) * 0.5f;

        if (sideLength < DoorSideWallMinimum)
            return;

        if (edge.Orientation == EdgeOrientation.Horizontal)
        {
            float z = (edge.AnchorCell.y + 0.5f) * CellSize;
            float centerX = edge.AnchorCell.x * CellSize;

            float leftX = centerX - (DoorOpeningWidth * 0.5f) - (sideLength * 0.5f);
            float rightX = centerX + (DoorOpeningWidth * 0.5f) + (sideLength * 0.5f);

            CreateDoorStubWall(
                $"{door.Name}_LeftWallStub",
                new Vector3(leftX, WallHeight * 0.5f, z),
                new Vector3(sideLength, WallHeight, WallThickness)
            );

            CreateDoorStubWall(
                $"{door.Name}_RightWallStub",
                new Vector3(rightX, WallHeight * 0.5f, z),
                new Vector3(sideLength, WallHeight, WallThickness)
            );

            return;
        }

        float x = (edge.AnchorCell.x + 0.5f) * CellSize;
        float centerZ = edge.AnchorCell.y * CellSize;

        float lowerZ = centerZ - (DoorOpeningWidth * 0.5f) - (sideLength * 0.5f);
        float upperZ = centerZ + (DoorOpeningWidth * 0.5f) + (sideLength * 0.5f);

        CreateDoorStubWall(
            $"{door.Name}_LowerWallStub",
            new Vector3(x, WallHeight * 0.5f, lowerZ),
            new Vector3(WallThickness, WallHeight, sideLength)
        );

        CreateDoorStubWall(
            $"{door.Name}_UpperWallStub",
            new Vector3(x, WallHeight * 0.5f, upperZ),
            new Vector3(WallThickness, WallHeight, sideLength)
        );
    }

    private static void CreateDoorStubWall(
        string wallName,
        Vector3 position,
        Vector3 scale
    )
    {
        GameObject wall = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        wall.name = wallName;
        wall.transform.SetParent(wallsRoot);
        wall.transform.position = position;
        wall.transform.localScale = scale;

        Renderer renderer = wall.GetComponent<Renderer>();

        if (renderer != null)
            renderer.sharedMaterial = wallMaterial;

        wall.isStatic = true;
    }

    private static void CreateWall(
        GridEdge edge
    )
    {
        Vector3 position;
        Vector3 scale;

        if (edge.Orientation == EdgeOrientation.Horizontal)
        {
            position = new Vector3(
                edge.AnchorCell.x * CellSize,
                WallHeight * 0.5f,
                (edge.AnchorCell.y + 0.5f) * CellSize
            );

            scale = new Vector3(
                CellSize,
                WallHeight,
                WallThickness
            );
        }
        else
        {
            position = new Vector3(
                (edge.AnchorCell.x + 0.5f) * CellSize,
                WallHeight * 0.5f,
                edge.AnchorCell.y * CellSize
            );

            scale = new Vector3(
                WallThickness,
                WallHeight,
                CellSize
            );
        }

        GameObject wall = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        wall.name = $"Wall_{edge.Orientation}_{edge.AnchorCell.x}_{edge.AnchorCell.y}";
        wall.transform.SetParent(wallsRoot);
        wall.transform.position = position;
        wall.transform.localScale = scale;

        Renderer renderer = wall.GetComponent<Renderer>();

        if (renderer != null)
            renderer.sharedMaterial = wallMaterial;

        wall.isStatic = true;
    }

    private static void GenerateDoorMarkers()
    {
        foreach (DoorData door in DoorEdges.Values)
        {
            CreateDoorMarker(door);
        }
    }

    private static void CreateDoorMarker(
        DoorData door
    )
    {
        Vector3 position = door.Edge.GetWorldMidpoint(CellSize);
        position.y = DoorMarkerThickness * 0.5f + 0.02f;

        Vector3 scale;

        if (door.Edge.Orientation == EdgeOrientation.Horizontal)
        {
            scale = new Vector3(
                DoorMarkerLength,
                DoorMarkerThickness,
                DoorMarkerWidth
            );
        }
        else
        {
            scale = new Vector3(
                DoorMarkerWidth,
                DoorMarkerThickness,
                DoorMarkerLength
            );
        }

        GameObject marker = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        marker.name = door.Name;
        marker.transform.SetParent(doorsRoot);
        marker.transform.position = position;
        marker.transform.localScale = scale;

        Renderer renderer = marker.GetComponent<Renderer>();

        if (renderer != null)
            renderer.sharedMaterial = doorMaterial;

        Collider collider = marker.GetComponent<Collider>();

        if (collider != null)
            UnityEngine.Object.DestroyImmediate(collider);

        marker.isStatic = true;
    }

    private static void FinaliseHierarchy()
    {
        if (ceilingsRoot != null)
            ceilingsRoot.gameObject.SetActive(CeilingsVisibleByDefault);

        CreateBoundsDebugObject();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateBoundsDebugObject()
    {
        if (Cells.Count == 0)
            return;

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minZ = int.MaxValue;
        int maxZ = int.MinValue;

        foreach (Vector2Int cell in Cells.Keys)
        {
            minX = Mathf.Min(minX, cell.x);
            maxX = Mathf.Max(maxX, cell.x);
            minZ = Mathf.Min(minZ, cell.y);
            maxZ = Mathf.Max(maxZ, cell.y);
        }

        GameObject bounds = new GameObject(
            $"LayoutBounds_Cells_{Cells.Count}_Areas_{Areas.Count}_Doors_{DoorEdges.Count}"
        );

        bounds.transform.SetParent(debugRoot);

        bounds.transform.position = new Vector3(
            ((minX + maxX) * 0.5f) * CellSize,
            0f,
            ((minZ + maxZ) * 0.5f) * CellSize
        );
    }

    private static Material GetOrCreateMaterial(
        string materialName,
        Color color
    )
    {
        string folder = "Assets/GameAI_CA3/Materials/Generated";
        EnsureFolderPath(folder);

        string path = $"{folder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material != null)
            return material;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        material = new Material(shader);
        material.name = materialName;
        material.color = color;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        AssetDatabase.CreateAsset(
            material,
            path
        );

        AssetDatabase.SaveAssets();

        return material;
    }

    private static void EnsureFolderPath(
        string path
    )
    {
        string[] parts = path.Split('/');

        if (parts.Length == 0)
            return;

        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";

            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(
                    current,
                    parts[i]
                );
            }

            current = next;
        }
    }

    private static Vector3 CellToWorldCenter(
        Vector2Int cell
    )
    {
        return new Vector3(
            cell.x * CellSize,
            0f,
            cell.y * CellSize
        );
    }

    private static Vector2Int DirectionToVector(
        Direction direction
    )
    {
        switch (direction)
        {
            case Direction.North:
                return Vector2Int.up;

            case Direction.South:
                return Vector2Int.down;

            case Direction.East:
                return Vector2Int.right;

            case Direction.West:
                return Vector2Int.left;

            default:
                return Vector2Int.zero;
        }
    }

    private static void PrintValidationMessages()
    {
        foreach (string warning in ValidationWarnings)
            Debug.LogWarning($"[Game AI CA3] {warning}");

        foreach (string error in ValidationErrors)
            Debug.LogError($"[Game AI CA3] {error}");

        Debug.Log(
            $"[Game AI CA3] Areas: {Areas.Count}, Cells: {Cells.Count}, Doors: {DoorEdges.Count}, Open Links: {OpenEdges.Count}, Walls: {WallEdges.Count}"
        );
    }

    private enum AreaKind
    {
        Room,
        Hall,
        Corridor
    }

    private enum Direction
    {
        North,
        South,
        East,
        West
    }

    private enum EdgeOrientation
    {
        Horizontal,
        Vertical
    }

    private readonly struct CellData
    {
        public readonly string AreaName;
        public readonly AreaKind Kind;

        public CellData(
            string areaName,
            AreaKind kind
        )
        {
            AreaName = areaName;
            Kind = kind;
        }
    }

    private readonly struct AreaData
    {
        public readonly string Name;
        public readonly int StartX;
        public readonly int StartZ;
        public readonly int Width;
        public readonly int Depth;
        public readonly AreaKind Kind;

        public AreaData(
            string name,
            int startX,
            int startZ,
            int width,
            int depth,
            AreaKind kind
        )
        {
            Name = name;
            StartX = startX;
            StartZ = startZ;
            Width = width;
            Depth = depth;
            Kind = kind;
        }
    }

    private readonly struct DoorData
    {
        public readonly string Name;
        public readonly GridEdge Edge;
        public readonly Direction OriginalDirection;

        public DoorData(
            string name,
            GridEdge edge,
            Direction originalDirection
        )
        {
            Name = name;
            Edge = edge;
            OriginalDirection = originalDirection;
        }
    }

    private readonly struct GridEdge : IEquatable<GridEdge>
    {
        public readonly Vector2Int AnchorCell;
        public readonly EdgeOrientation Orientation;
        public readonly Vector2Int CellA;
        public readonly Vector2Int CellB;

        private GridEdge(
            Vector2Int anchorCell,
            EdgeOrientation orientation,
            Vector2Int cellA,
            Vector2Int cellB
        )
        {
            AnchorCell = anchorCell;
            Orientation = orientation;

            if (CompareCells(cellA, cellB) <= 0)
            {
                CellA = cellA;
                CellB = cellB;
            }
            else
            {
                CellA = cellB;
                CellB = cellA;
            }
        }

        public static GridEdge FromCellAndDirection(
            Vector2Int cell,
            Direction direction
        )
        {
            Vector2Int neighbour = cell + DirectionToVectorStatic(direction);

            switch (direction)
            {
                case Direction.North:
                    return new GridEdge(
                        cell,
                        EdgeOrientation.Horizontal,
                        cell,
                        neighbour
                    );

                case Direction.South:
                    return new GridEdge(
                        neighbour,
                        EdgeOrientation.Horizontal,
                        cell,
                        neighbour
                    );

                case Direction.East:
                    return new GridEdge(
                        cell,
                        EdgeOrientation.Vertical,
                        cell,
                        neighbour
                    );

                case Direction.West:
                    return new GridEdge(
                        neighbour,
                        EdgeOrientation.Vertical,
                        cell,
                        neighbour
                    );

                default:
                    return new GridEdge(
                        cell,
                        EdgeOrientation.Horizontal,
                        cell,
                        neighbour
                    );
            }
        }

        public static GridEdge FromTwoCells(
            Vector2Int a,
            Vector2Int b
        )
        {
            Vector2Int delta = b - a;

            if (delta == Vector2Int.up)
                return FromCellAndDirection(a, Direction.North);

            if (delta == Vector2Int.down)
                return FromCellAndDirection(a, Direction.South);

            if (delta == Vector2Int.right)
                return FromCellAndDirection(a, Direction.East);

            if (delta == Vector2Int.left)
                return FromCellAndDirection(a, Direction.West);

            throw new ArgumentException(
                $"Cells {a} and {b} are not adjacent."
            );
        }

        public Vector3 GetWorldMidpoint(
            float cellSize
        ) 
        {
            if (Orientation == EdgeOrientation.Horizontal)
            {
                return new Vector3(
                    AnchorCell.x * cellSize,
                    0f,
                    (AnchorCell.y + 0.5f) * cellSize
                );
            }

            return new Vector3(
                (AnchorCell.x + 0.5f) * cellSize,
                0f,
                AnchorCell.y * cellSize
            );
        }

        public bool Equals(
            GridEdge other
        )
        {
            return AnchorCell.Equals(other.AnchorCell)
                   && Orientation == other.Orientation
                   && CellA.Equals(other.CellA)
                   && CellB.Equals(other.CellB);
        }

        public override bool Equals(
            object obj
        )
        {
            return obj is GridEdge other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = AnchorCell.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Orientation;
                hashCode = (hashCode * 397) ^ CellA.GetHashCode();
                hashCode = (hashCode * 397) ^ CellB.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{Orientation} edge at {AnchorCell} between {CellA} and {CellB}";
        }

        private static int CompareCells(
            Vector2Int a,
            Vector2Int b
        )
        {
            int xCompare = a.x.CompareTo(b.x);

            if (xCompare != 0)
                return xCompare;

            return a.y.CompareTo(b.y);
        }

        private static Vector2Int DirectionToVectorStatic(
            Direction direction
        )
        {
            switch (direction)
            {
                case Direction.North:
                    return Vector2Int.up;

                case Direction.South:
                    return Vector2Int.down;

                case Direction.East:
                    return Vector2Int.right;

                case Direction.West:
                    return Vector2Int.left;

                default:
                    return Vector2Int.zero;
            }
        }
    }
}
