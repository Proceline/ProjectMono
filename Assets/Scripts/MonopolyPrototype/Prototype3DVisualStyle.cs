using UnityEngine;

namespace MonopolyPrototype
{
    public readonly struct Prototype3DVisualStyle
    {
        public Prototype3DVisualStyle(
            Color tileColor,
            Color markerColor,
            PrimitiveType markerPrimitive,
            Vector3 markerScale,
            bool hasMarker)
        {
            TileColor = tileColor;
            MarkerColor = markerColor;
            MarkerPrimitive = markerPrimitive;
            MarkerScale = markerScale;
            HasMarker = hasMarker;
        }

        public Color TileColor { get; }
        public Color MarkerColor { get; }
        public PrimitiveType MarkerPrimitive { get; }
        public Vector3 MarkerScale { get; }
        public bool HasMarker { get; }

        public static Prototype3DVisualStyle For(BuildingDefinition building)
        {
            if (building == null)
            {
                return new Prototype3DVisualStyle(
                    new Color(0.28f, 0.32f, 0.36f),
                    Color.white,
                    PrimitiveType.Cube,
                    Vector3.one,
                    false);
            }

            if (building.Name == "Start")
            {
                return Create(
                    new Color(0.98f, 0.84f, 0.44f),
                    new Color(0.95f, 0.72f, 0.22f),
                    PrimitiveType.Cylinder,
                    new Vector3(0.48f, 0.42f, 0.48f));
            }

            if (building.Name == "Bank")
            {
                return Create(
                    new Color(0.64f, 0.82f, 0.68f),
                    new Color(0.12f, 0.42f, 0.25f),
                    PrimitiveType.Cube,
                    new Vector3(0.56f, 0.7f, 0.56f));
            }

            if (building.Name == "Shop")
            {
                return Create(
                    new Color(0.95f, 0.76f, 0.44f),
                    new Color(0.76f, 0.38f, 0.12f),
                    PrimitiveType.Cube,
                    new Vector3(0.62f, 0.58f, 0.62f));
            }

            if (building.Name == "Market")
            {
                return Create(
                    new Color(0.91f, 0.67f, 0.35f),
                    new Color(0.67f, 0.28f, 0.12f),
                    PrimitiveType.Cube,
                    new Vector3(0.7f, 0.42f, 0.5f));
            }

            if (building.Name == "Gate")
            {
                return Create(
                    new Color(0.91f, 0.58f, 0.48f),
                    new Color(0.76f, 0.22f, 0.16f),
                    PrimitiveType.Cube,
                    new Vector3(0.55f, 0.82f, 0.55f));
            }

            if (building.Name == "Station")
            {
                return Create(
                    new Color(0.54f, 0.7f, 0.9f),
                    new Color(0.14f, 0.36f, 0.7f),
                    PrimitiveType.Cylinder,
                    new Vector3(0.48f, 0.52f, 0.48f));
            }

            if (building.Name == "Harbor")
            {
                return Create(
                    new Color(0.48f, 0.78f, 0.84f),
                    new Color(0.08f, 0.4f, 0.55f),
                    PrimitiveType.Cylinder,
                    new Vector3(0.58f, 0.36f, 0.58f));
            }

            if (building.Name == "Park")
            {
                return Create(
                    new Color(0.58f, 0.84f, 0.62f),
                    new Color(0.20f, 0.62f, 0.38f),
                    PrimitiveType.Sphere,
                    new Vector3(0.62f, 0.52f, 0.62f));
            }

            if (building.Name == "Library")
            {
                return Create(
                    new Color(0.58f, 0.72f, 0.88f),
                    new Color(0.2f, 0.34f, 0.58f),
                    PrimitiveType.Cube,
                    new Vector3(0.56f, 0.72f, 0.42f));
            }

            if (building.Name == "Museum")
            {
                return Create(
                    new Color(0.72f, 0.62f, 0.86f),
                    new Color(0.42f, 0.24f, 0.62f),
                    PrimitiveType.Cylinder,
                    new Vector3(0.5f, 0.68f, 0.5f));
            }

            if (building.Name == "Hotel")
            {
                return Create(
                    new Color(0.66f, 0.82f, 0.82f),
                    new Color(0.12f, 0.48f, 0.52f),
                    PrimitiveType.Cube,
                    new Vector3(0.62f, 0.76f, 0.62f));
            }

            if (building.Name == "Clinic")
            {
                return Create(
                    new Color(0.68f, 0.88f, 0.76f),
                    new Color(0.16f, 0.56f, 0.34f),
                    PrimitiveType.Capsule,
                    new Vector3(0.46f, 0.62f, 0.46f));
            }

            if (building.Name == "Theater")
            {
                return Create(
                    new Color(0.82f, 0.55f, 0.7f),
                    new Color(0.62f, 0.18f, 0.4f),
                    PrimitiveType.Cylinder,
                    new Vector3(0.52f, 0.62f, 0.52f));
            }

            return Create(
                new Color(0.56f, 0.68f, 0.82f),
                new Color(0.32f, 0.48f, 0.68f),
                PrimitiveType.Cube,
                new Vector3(0.58f, 0.58f, 0.58f));
        }

        private static Prototype3DVisualStyle Create(
            Color tileColor,
            Color markerColor,
            PrimitiveType markerPrimitive,
            Vector3 markerScale)
        {
            return new Prototype3DVisualStyle(
                tileColor,
                markerColor,
                markerPrimitive,
                markerScale,
                true);
        }
    }
}
