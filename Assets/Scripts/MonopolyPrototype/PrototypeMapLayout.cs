using UnityEngine;

namespace MonopolyPrototype
{
    public static class PrototypeMapLayout
    {
        public static readonly Vector2 DefaultOrigin = new Vector2(-4.5f, -2.5f);
        public static readonly Vector2 DefaultSpacing = new Vector2(1.8f, 1.7f);

        public static Vector3 GetWorldPosition(Vector2Int gridPosition, Vector2 origin, Vector2 spacing)
        {
            return new Vector3(
                origin.x + gridPosition.x * spacing.x,
                origin.y + gridPosition.y * spacing.y,
                0f);
        }
    }
}
