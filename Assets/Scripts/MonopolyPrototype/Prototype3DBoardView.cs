using System.Collections.Generic;
using UnityEngine;

namespace MonopolyPrototype
{
    public sealed class Prototype3DBoardView : MonoBehaviour
    {
        private const float BoardPlatformHeight = 0.35f;
        private const float TileSurfaceHeight = 0.3f;
        private const float TileSurfaceCenterHeight = 0.15f;
        private const float MarkerGap = 0.03f;

        private Transform boardRoot;

        public IReadOnlyList<BoardTile> Build(
            PrototypeMapData mapData,
            Vector2 boardCenter,
            Vector2 tileSpacing,
            float tileScale)
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
                var tileObject = new GameObject($"Tile - {definition.Name}");
                tileObject.transform.SetParent(boardRoot, false);

                var position = PrototypeMapLayout.GetWorldPosition(
                    mapTile.GridPosition,
                    mapSize,
                    boardCenter,
                    tileSpacing);
                tileObject.transform.position = new Vector3(position.x, 0f, position.y);

                var tile = tileObject.AddComponent<BoardTile>();
                tile.Configure(definition.Name, mapTile.BuildingConfig);
                tiles.Add(tile);

                var style = Prototype3DVisualStyle.For(definition.Building);
                CreatePrimitive(
                    PrimitiveType.Cube,
                    "Tile Surface",
                    tileObject.transform,
                    new Vector3(0f, TileSurfaceCenterHeight, 0f),
                    new Vector3(safeTileScale, TileSurfaceHeight, safeTileScale),
                    style.TileColor);

                if (style.HasMarker)
                {
                    CreateBuildingMarker(tileObject.transform, style);
                }

                CreateTileLabel(tileObject.transform, definition.Name, safeTileScale);
            }

            return tiles;
        }

        private static void CreateBuildingMarker(
            Transform parent,
            Prototype3DVisualStyle style)
        {
            var markerHeight = GetPrimitiveHeight(style.MarkerPrimitive) * style.MarkerScale.y;
            CreatePrimitive(
                style.MarkerPrimitive,
                "Building Marker",
                parent,
                new Vector3(
                    0f,
                    TileSurfaceHeight + MarkerGap + markerHeight * 0.5f,
                    0f),
                style.MarkerScale,
                style.MarkerColor);
        }

        private static void CreateTileLabel(
            Transform parent,
            string label,
            float tileScale)
        {
            var labelObject = new GameObject("3D Label");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(
                0f,
                TileSurfaceHeight + 0.02f,
                -Mathf.Max(0.32f, tileScale * 0.3f));
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = label;
            textMesh.fontSize = 32;
            textMesh.characterSize = 0.07f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.color = new Color(0.08f, 0.1f, 0.12f);
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

        private static float GetPrimitiveHeight(PrimitiveType primitiveType)
        {
            return primitiveType == PrimitiveType.Cylinder
                || primitiveType == PrimitiveType.Capsule
                ? 2f
                : 1f;
        }
    }
}
