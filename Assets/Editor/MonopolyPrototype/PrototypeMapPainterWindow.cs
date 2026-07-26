using System.Collections.Generic;
using System.Linq;
using MonopolyPrototype;
using UnityEditor;
using UnityEngine;

namespace MonopolyPrototype.EditorTools
{
    public sealed class PrototypeMapPainterWindow : EditorWindow
    {
        private const int MinMapSize = 2;
        private const int MaxMapSize = 20;
        private static readonly Vector2 SceneGridOrigin = PrototypeMapLayout.DefaultOrigin;
        private static readonly Vector2 SceneGridSpacing = PrototypeMapLayout.DefaultSpacing;

        [SerializeField] private int mapSize = 6;
        [SerializeField] private PrototypeMapData loadedMap;
        [SerializeField] private BuildingConfig selectedBuilding;
        [SerializeField] private bool eraseMode;
        [SerializeField] private Vector2 paletteScroll;

        [SerializeField] private List<PrototypeMapTileData> path = new List<PrototypeMapTileData>();
        [SerializeField] private bool pathInitialized;
        private List<BuildingConfig> palette = new List<BuildingConfig>();
        private string status = "Select a building, then click adjacent cells to draw the path.";
        private MessageType statusType = MessageType.Info;

        [MenuItem("Monopoly Prototype/Map Painter")]
        public static void Open()
        {
            GetWindow<PrototypeMapPainterWindow>("Map Painter");
        }

        private void OnEnable()
        {
            RefreshPalette();
            SceneView.duringSceneGui += OnSceneGUI;
            if (loadedMap != null && !pathInitialized)
            {
                LoadMap(loadedMap);
            }
            else
            {
                SelectBuildingByName("Start");
            }
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Map Painter", EditorStyles.boldLabel);

            var newSize = EditorGUILayout.IntSlider("Map Size", mapSize, MinMapSize, MaxMapSize);
            if (newSize != mapSize)
            {
                mapSize = newSize;
                loadedMap = null;
                path.Clear();
                pathInitialized = true;
                status = $"New {mapSize} x {mapSize} map.";
                statusType = MessageType.Info;
                SceneView.RepaintAll();
            }

            var newMap = (PrototypeMapData)EditorGUILayout.ObjectField(
                "Map Data",
                loadedMap,
                typeof(PrototypeMapData),
                false);
            if (newMap != loadedMap)
            {
                if (newMap == null)
                {
                    StartNewMap(mapSize);
                }
                else
                {
                    LoadMap(newMap);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("New"))
                {
                    StartNewMap(mapSize);
                }

                if (GUILayout.Button("Undo Last"))
                {
                    UndoLastTile();
                }

                if (GUILayout.Button("Validate"))
                {
                    ValidateCurrentPath();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save As MapData"))
                {
                    SaveAsMapData();
                }

                using (new EditorGUI.DisabledScope(loadedMap == null))
                {
                    if (GUILayout.Button("Save"))
                    {
                        SaveMapData();
                    }
                }
            }

            EditorGUILayout.HelpBox(status, statusType);
            EditorGUILayout.LabelField($"Path: {path.Count} tiles | {GetLoopStatus()}");

            DrawPalette();
            DrawGridPalette();
        }

        private void DrawPalette()
        {
            EditorGUILayout.LabelField("Building Palette", EditorStyles.boldLabel);
            paletteScroll = EditorGUILayout.BeginScrollView(paletteScroll, GUILayout.Height(72f));
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawPaletteButton("Blank", null);
                DrawEraseButton();
                for (var i = 0; i < palette.Count; i++)
                {
                    DrawPaletteButton(palette[i].BuildingName, palette[i]);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField(
                eraseMode
                    ? "Selected: Erase"
                    : selectedBuilding == null
                        ? "Selected: Blank"
                        : $"Selected: {selectedBuilding.BuildingName}");
        }

        private void DrawPaletteButton(string label, BuildingConfig building)
        {
            var previousColor = GUI.backgroundColor;
            if (!eraseMode && selectedBuilding == building)
            {
                GUI.backgroundColor = new Color(0.95f, 0.78f, 0.22f);
            }

            if (GUILayout.Button(label, GUILayout.Width(92f), GUILayout.Height(36f)))
            {
                SelectBuilding(building);
            }

            GUI.backgroundColor = previousColor;
        }

        private void DrawEraseButton()
        {
            var previousColor = GUI.backgroundColor;
            if (eraseMode)
            {
                GUI.backgroundColor = new Color(0.95f, 0.45f, 0.35f);
            }

            if (GUILayout.Button("Erase", GUILayout.Width(92f), GUILayout.Height(36f)))
            {
                eraseMode = true;
                selectedBuilding = null;
            }

            GUI.backgroundColor = previousColor;
        }

        private void DrawGridPalette()
        {
            EditorGUILayout.LabelField("Path Grid", EditorStyles.boldLabel);
            for (var y = mapSize - 1; y >= 0; y--)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (var x = 0; x < mapSize; x++)
                    {
                        var position = new Vector2Int(x, y);
                        var tileIndex = FindTileIndex(position);
                        var label = tileIndex < 0
                            ? "."
                            : $"{tileIndex + 1}\n{path[tileIndex].TileName}";
                        if (GUILayout.Button(label, GUILayout.Width(54f), GUILayout.Height(44f)))
                        {
                            TryAddTile(position);
                        }
                    }
                }
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            DrawScenePath();

            for (var y = 0; y < mapSize; y++)
            {
                for (var x = 0; x < mapSize; x++)
                {
                    var gridPosition = new Vector2Int(x, y);
                    var worldPosition = GetScenePosition(gridPosition);
                    var tileIndex = FindTileIndex(gridPosition);
                    var previousColor = Handles.color;
                    Handles.color = tileIndex >= 0
                        ? new Color(0.95f, 0.78f, 0.22f, 0.85f)
                        : new Color(0.55f, 0.65f, 0.75f, 0.65f);
                    Handles.DrawWireCube(worldPosition, new Vector3(0.95f, 0.95f, 0.02f));
                    Handles.color = previousColor;

                    if (Handles.Button(
                            worldPosition,
                            Quaternion.identity,
                            0.42f,
                            0.42f,
                            Handles.RectangleHandleCap))
                    {
                        TryAddTile(gridPosition);
                        GUI.changed = true;
                        Repaint();
                        SceneView.RepaintAll();
                    }

                    if (tileIndex >= 0)
                    {
                        Handles.Label(worldPosition, $"{tileIndex + 1}: {path[tileIndex].TileName}");
                    }
                }
            }
        }

        private void DrawScenePath()
        {
            if (path.Count == 0)
            {
                return;
            }

            var points = new List<Vector3>();
            for (var i = 0; i < path.Count; i++)
            {
                points.Add(GetScenePosition(path[i].GridPosition));
            }

            if (IsClosed())
            {
                points.Add(points[0]);
            }

            Handles.color = new Color(0.95f, 0.78f, 0.22f, 0.9f);
            Handles.DrawAAPolyLine(4f, points.ToArray());
            Handles.color = Color.white;
        }

        private void TryAddTile(Vector2Int position)
        {
            var tileIndex = FindTileIndex(position);
            if (tileIndex >= 0)
            {
                if (eraseMode)
                {
                    var removedTileName = path[tileIndex].TileName;
                    path.RemoveAt(tileIndex);
                    status = $"Erased {removedTileName} at ({position.x}, {position.y}).";
                    statusType = MessageType.Info;
                }
                else
                {
                    var replacementTileName = selectedBuilding == null ? "Blank" : selectedBuilding.BuildingName;
                    path[tileIndex] = new PrototypeMapTileData(position, replacementTileName, selectedBuilding);
                    status = $"Replaced tile {tileIndex + 1} with {replacementTileName}.";
                    statusType = MessageType.Info;
                }

                Repaint();
                SceneView.RepaintAll();
                return;
            }

            if (eraseMode)
            {
                status = "Erase mode only removes existing cells.";
                statusType = MessageType.Warning;
                Repaint();
                return;
            }

            if (path.Count > 0 && !IsAdjacent(path[path.Count - 1].GridPosition, position))
            {
                status = "New cells must be adjacent to the previous cell.";
                statusType = MessageType.Warning;
                Repaint();
                return;
            }

            var tileName = selectedBuilding == null ? "Blank" : selectedBuilding.BuildingName;
            path.Add(new PrototypeMapTileData(position, tileName, selectedBuilding));
            status = $"Added {tileName} at ({position.x}, {position.y}).";
            statusType = MessageType.Info;
            Repaint();
            SceneView.RepaintAll();
        }

        private void UndoLastTile()
        {
            if (path.Count == 0)
            {
                return;
            }

            path.RemoveAt(path.Count - 1);
            status = "Removed the last path tile.";
            statusType = MessageType.Info;
            Repaint();
            SceneView.RepaintAll();
        }

        private void StartNewMap(int size)
        {
            mapSize = Mathf.Clamp(size, MinMapSize, MaxMapSize);
            loadedMap = null;
            path.Clear();
            pathInitialized = true;
            SelectBuildingByName("Start");
            status = $"New {mapSize} x {mapSize} map.";
            statusType = MessageType.Info;
            Repaint();
            SceneView.RepaintAll();
        }

        private void LoadMap(PrototypeMapData map)
        {
            loadedMap = map;
            mapSize = Mathf.Clamp(map.Width, MinMapSize, MaxMapSize);
            path.Clear();
            path.AddRange(map.Tiles);
            pathInitialized = true;
            eraseMode = false;
            status = $"Loaded {map.name}.";
            statusType = MessageType.Info;
            Repaint();
            SceneView.RepaintAll();
        }

        private void SaveAsMapData()
        {
            if (!ValidateCurrentPath())
            {
                return;
            }

            var assetPath = EditorUtility.SaveFilePanelInProject(
                "Save Map Data",
                "MapData",
                "asset",
                "Choose where to save the map data asset.",
                "Assets/Data/Maps");
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            var asset = CreateInstance<PrototypeMapData>();
            asset.Configure(mapSize, path);
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            loadedMap = asset;
            status = $"Saved {assetPath}.";
            statusType = MessageType.Info;
            Repaint();
        }

        private void SaveMapData()
        {
            if (loadedMap == null || !ValidateCurrentPath())
            {
                return;
            }

            Undo.RecordObject(loadedMap, "Save map data");
            loadedMap.Configure(mapSize, path);
            EditorUtility.SetDirty(loadedMap);
            AssetDatabase.SaveAssets();
            status = $"Saved {loadedMap.name}.";
            statusType = MessageType.Info;
            Repaint();
        }

        private bool ValidateCurrentPath()
        {
            var validationAsset = CreateInstance<PrototypeMapData>();
            validationAsset.Configure(mapSize, path);
            var valid = validationAsset.TryValidateClosedLoop(out var error);
            DestroyImmediate(validationAsset);

            status = valid ? "Map path is a valid closed loop." : error;
            statusType = valid ? MessageType.Info : MessageType.Error;
            Repaint();
            return valid;
        }

        private void RefreshPalette()
        {
            palette = AssetDatabase.FindAssets("t:BuildingConfig")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<BuildingConfig>)
                .Where(config => config != null)
                .OrderBy(config => config.BuildingName)
                .ToList();
        }

        private void SelectBuildingByName(string name)
        {
            SelectBuilding(palette.FirstOrDefault(config => config.BuildingName == name));
        }

        private void SelectBuilding(BuildingConfig building)
        {
            eraseMode = false;
            selectedBuilding = building;
        }

        private int FindTileIndex(Vector2Int position)
        {
            for (var i = 0; i < path.Count; i++)
            {
                if (path[i].GridPosition == position)
                {
                    return i;
                }
            }

            return -1;
        }

        private string GetLoopStatus()
        {
            if (path.Count < 4)
            {
                return "open";
            }

            return IsClosed() ? "closed" : "open";
        }

        private bool IsClosed()
        {
            return path.Count >= 4 && IsAdjacent(path[path.Count - 1].GridPosition, path[0].GridPosition);
        }

        private static bool IsAdjacent(Vector2Int first, Vector2Int second)
        {
            return Mathf.Abs(first.x - second.x) + Mathf.Abs(first.y - second.y) == 1;
        }

        private Vector3 GetScenePosition(Vector2Int gridPosition)
        {
            return new Vector3(
                SceneGridOrigin.x + gridPosition.x * SceneGridSpacing.x,
                SceneGridOrigin.y + gridPosition.y * SceneGridSpacing.y,
                0f);
        }
    }
}
