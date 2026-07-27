using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MonopolyPrototype
{
    public sealed class Prototype3DBoardView : MonoBehaviour
    {
        private const float BoardPlatformHeight = 0.35f;

        [SerializeField] private PrototypeBoardTileView tilePrefab;

        private Transform boardRoot;

        public IReadOnlyList<BoardTile> Build(
            PrototypeMapData mapData,
            Vector2 boardCenter,
            Vector2 tileSpacing,
            float tileScale)
        {
            return Build(mapData, boardCenter, tileSpacing, tileScale, tilePrefab);
        }

        public IReadOnlyList<BoardTile> Build(
            PrototypeMapData mapData,
            Vector2 boardCenter,
            Vector2 tileSpacing,
            float tileScale,
            PrototypeBoardTileView tileVisualPrefab)
        {
            if (mapData == null)
            {
                Debug.LogError("Prototype 3D Board View needs a PrototypeMapData asset.");
                return new List<BoardTile>();
            }

            if (!mapData.TryValidateClosedLoop(out var error))
            {
                Debug.LogError($"Prototype map data is invalid: {error}");
                return new List<BoardTile>();
            }

            var prefab = tileVisualPrefab != null ? tileVisualPrefab : tilePrefab;
            if (prefab == null)
            {
                Debug.LogError("Prototype 3D Board View needs a PrototypeBoardTileView prefab.");
                return new List<BoardTile>();
            }

            var safeTileScale = Mathf.Max(0.1f, tileScale);
            boardRoot = new GameObject("Prototype Board").transform;
            boardRoot.SetParent(transform, false);

            var mapSize = new Vector2Int(mapData.Width, mapData.Height);
            var bounds = PrototypeMapLayout.GetWorldBounds(
                mapSize,
                boardCenter,
                tileSpacing,
                safeTileScale);
            CreatePrimitive(
                PrimitiveType.Cube,
                "Board Platform",
                boardRoot,
                new Vector3(boardCenter.x, -BoardPlatformHeight * 0.5f, boardCenter.y),
                new Vector3(bounds.size.x + 1.1f, BoardPlatformHeight, bounds.size.y + 1.1f),
                new Color(0.12f, 0.19f, 0.23f));

            var tiles = new List<BoardTile>();
            for (var i = 0; i < mapData.Tiles.Count; i++)
            {
                var mapTile = mapData.Tiles[i];
                var definition = mapTile.ToDefinition();
                var tileView = InstantiateTile(prefab, boardRoot);
                var tileObject = tileView.gameObject;

                var position = PrototypeMapLayout.GetWorldPosition(
                    mapTile.GridPosition,
                    mapSize,
                    boardCenter,
                    tileSpacing);
                tileObject.transform.position = new Vector3(position.x, 0f, position.y);

                var tile = tileObject.GetComponent<BoardTile>();
                if (tile == null)
                {
                    tile = tileObject.AddComponent<BoardTile>();
                }
                tile.Configure(definition.Name, mapTile.BuildingConfig);
                tileView.Configure(definition.Building, definition.Name, safeTileScale);
                tiles.Add(tile);
            }

            return tiles;
        }

        private static PrototypeBoardTileView InstantiateTile(
            PrototypeBoardTileView prefab,
            Transform parent)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var editorInstance = PrefabUtility.InstantiatePrefab(prefab, parent)
                    as PrototypeBoardTileView;
                if (editorInstance != null)
                {
                    return editorInstance;
                }
            }
#endif

            return Instantiate(prefab, parent);
        }

        private static GameObject CreatePrimitive(
            PrimitiveType primitiveType,
            string objectName,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color)
        {
            var primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = objectName;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;

            var renderer = primitive.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = Application.isPlaying
                    ? renderer.material
                    : renderer.sharedMaterial;
                if (material != null)
                {
                    material.color = color;
                }
            }

            return primitive;
        }
    }
}
