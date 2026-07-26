using UnityEngine;

namespace MonopolyPrototype
{
    public static class PrototypeMapLayout
    {
        public static readonly Vector2 DefaultCenter = Vector2.zero;
        public static readonly Vector2 DefaultSpacing = new Vector2(1.8f, 1.7f);
        public const float DefaultTileScale = 1.5f;

        public static Vector3 GetWorldPosition(
            Vector2Int gridPosition,
            Vector2Int mapSize,
            Vector2 center,
            Vector2 spacing)
        {
            var mapCenter = new Vector2(
                (mapSize.x - 1) * 0.5f,
                (mapSize.y - 1) * 0.5f);
            return new Vector3(
                center.x + (gridPosition.x - mapCenter.x) * spacing.x,
                center.y + (gridPosition.y - mapCenter.y) * spacing.y,
                0f);
        }

        public static Bounds GetWorldBounds(
            Vector2Int mapSize,
            Vector2 center,
            Vector2 spacing,
            float tileScale)
        {
            var width = Mathf.Max(0f, mapSize.x - 1) * Mathf.Abs(spacing.x) + Mathf.Abs(tileScale);
            var height = Mathf.Max(0f, mapSize.y - 1) * Mathf.Abs(spacing.y) + Mathf.Abs(tileScale);
            return new Bounds(
                new Vector3(center.x, center.y, 0f),
                new Vector3(width, height, 0f));
        }
    }
}
