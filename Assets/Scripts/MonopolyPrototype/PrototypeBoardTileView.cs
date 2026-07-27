using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MonopolyPrototype
{
    public sealed class PrototypeBoardTileView : MonoBehaviour
    {
        private const float TileSurfaceHeight = 0.3f;
        private const float TileSurfaceTop = 0.3f;
        private const float MarkerGap = 0.03f;

        [SerializeField] private Renderer tileSurfaceRenderer;
        [SerializeField] private Transform markerAnchor;
        [SerializeField] private TextMesh label;

        private GameObject markerObject;

        public void Configure(
            BuildingDefinition building,
            string tileName,
            float tileScale)
        {
            var style = Prototype3DVisualStyle.For(building);
            ConfigureSurface(style.TileColor, tileScale);
            ConfigureLabel(tileName, tileScale);
            ClearMarker();

            if (!style.HasMarker || markerAnchor == null)
            {
                return;
            }

            var markerHeight = GetPrimitiveHeight(style.MarkerPrimitive) * style.MarkerScale.y;
            markerObject = GameObject.CreatePrimitive(style.MarkerPrimitive);
            markerObject.name = "Building Marker";
            markerObject.transform.SetParent(markerAnchor, false);
            markerObject.transform.localPosition = new Vector3(
                0f,
                TileSurfaceTop + MarkerGap + markerHeight * 0.5f,
                0f);
            markerObject.transform.localScale = style.MarkerScale;

            SetRendererColor(markerObject.GetComponent<Renderer>(), style.MarkerColor);
        }

        private void ConfigureSurface(Color color, float tileScale)
        {
            if (tileSurfaceRenderer == null)
            {
                return;
            }

            tileSurfaceRenderer.transform.localPosition = new Vector3(
                0f,
                TileSurfaceHeight * 0.5f,
                0f);
            tileSurfaceRenderer.transform.localScale = new Vector3(
                Mathf.Max(0.1f, tileScale),
                TileSurfaceHeight,
                Mathf.Max(0.1f, tileScale));
            SetRendererColor(tileSurfaceRenderer, color);
        }

        private void ConfigureLabel(string tileName, float tileScale)
        {
            if (label == null)
            {
                return;
            }

            label.text = tileName;
            label.transform.localPosition = new Vector3(
                0f,
                TileSurfaceTop + 0.02f,
                -Mathf.Max(0.32f, Mathf.Max(0.1f, tileScale) * 0.3f));
            label.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private void ClearMarker()
        {
            if (markerObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(markerObject);
            }
            else
            {
                DestroyImmediate(markerObject);
            }

            markerObject = null;
        }

        private static void SetRendererColor(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            var material = Application.isPlaying
                ? renderer.material
                : renderer.sharedMaterial;
#if UNITY_EDITOR
            if (!Application.isPlaying && material != null && AssetDatabase.Contains(material))
            {
                material = new Material(material);
                material.name = $"{material.name} (Prototype Tile Instance)";
                renderer.sharedMaterial = material;
            }
#endif
            if (material != null)
            {
                material.color = color;
            }
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
