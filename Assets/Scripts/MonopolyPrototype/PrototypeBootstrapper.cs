using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MonopolyPrototype
{
    public sealed class PrototypeBootstrapper : MonoBehaviour
    {
        [SerializeField] private PrototypeMapData mapData;
        [SerializeField] private Prototype3DBoardView boardView;
        [SerializeField] private Vector2 boardCenter = PrototypeMapLayout.DefaultCenter;
        [SerializeField] private Vector2 tileSpacing = PrototypeMapLayout.DefaultSpacing;
        [SerializeField, Min(0.1f)] private float tileScale = PrototypeMapLayout.DefaultTileScale;
        [SerializeField] private bool fitCameraToBoard = true;
        [SerializeField, Min(0f)] private float cameraPadding = 0.8f;
        [SerializeField, Range(25f, 80f)] private float cameraFieldOfView = 50f;
        [SerializeField] private Vector3 cameraDirection = new Vector3(0.65f, 1f, -0.8f);

        private void Awake()
        {
            SetupCamera();
            EnsureEventSystem();

            var tiles = CreateBoard();
            var token = CreateToken();
            var ui = CreateUi();

            var controller = gameObject.AddComponent<BoardController>();
            controller.Configure(tiles, token, ui.LogView, ui.ConfirmationView, ui.RollButton);
        }

        private void SetupCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            var mapSize = mapData == null
                ? new Vector2Int(6, 6)
                : new Vector2Int(mapData.Width, mapData.Height);
            var boardBounds = PrototypeMapLayout.GetWorldBounds(
                mapSize,
                boardCenter,
                tileSpacing,
                Mathf.Max(0.1f, tileScale));
            var target = new Vector3(boardCenter.x, 0f, boardCenter.y);
            var halfWidth = boardBounds.size.x * 0.5f;
            var halfDepth = boardBounds.size.y * 0.5f;
            var boardRadius = Mathf.Sqrt(halfWidth * halfWidth + halfDepth * halfDepth);
            var safeFieldOfView = Mathf.Clamp(cameraFieldOfView, 25f, 80f);
            var requiredDistance = boardRadius
                / Mathf.Tan(safeFieldOfView * 0.5f * Mathf.Deg2Rad)
                + cameraPadding;
            var direction = cameraDirection.sqrMagnitude > 0.001f
                ? cameraDirection.normalized
                : new Vector3(0.65f, 1f, -0.8f).normalized;

            camera.orthographic = false;
            camera.fieldOfView = safeFieldOfView;
            camera.transform.position = fitCameraToBoard
                ? target + direction * requiredDistance
                : target + direction * Mathf.Max(8f, requiredDistance);
            camera.transform.LookAt(target);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.1f, 0.13f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = Mathf.Max(100f, requiredDistance * 4f);
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        private IReadOnlyList<BoardTile> CreateBoard()
        {
            if (mapData == null)
            {
                Debug.LogError("Prototype Bootstrapper needs a PrototypeMapData asset.");
                return new List<BoardTile>();
            }

            if (boardView == null)
            {
                var viewObject = new GameObject("Prototype 3D Board View");
                viewObject.transform.SetParent(transform, false);
                boardView = viewObject.AddComponent<Prototype3DBoardView>();
            }

            return boardView.Build(mapData, boardCenter, tileSpacing, tileScale);
        }

        private static PlayerToken CreateToken()
        {
            var tokenObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            tokenObject.name = "Player Token";
            tokenObject.transform.localScale = new Vector3(0.45f, 0.75f, 0.45f);
            var renderer = tokenObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.95f, 0.78f, 0.22f);
            }

            return tokenObject.AddComponent<PlayerToken>();
        }

        private static (GameLogView LogView, ConfirmationView ConfirmationView, Button RollButton) CreateUi()
        {
            var canvasObject = new GameObject("Prototype UI");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            var rollButton = CreateRollButton(canvasObject.transform);
            var logView = CreateLogView(canvasObject.transform);
            var confirmationView = CreateConfirmationView(canvasObject.transform);
            return (logView, confirmationView, rollButton);
        }

        private static Button CreateRollButton(Transform parent)
        {
            var buttonObject = new GameObject("Roll Button");
            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.95f, 0.78f, 0.22f);

            var button = buttonObject.AddComponent<Button>();
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(24f, -24f);
            rect.sizeDelta = new Vector2(150f, 48f);

            var label = CreateUiText("Roll", buttonObject.transform, 24, TextAnchor.MiddleCenter, Color.black);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.sizeDelta = Vector2.zero;

            return button;
        }

        private static GameLogView CreateLogView(Transform parent)
        {
            var panel = new GameObject("Log Panel");
            panel.transform.SetParent(parent, false);
            var background = panel.AddComponent<Image>();
            background.color = new Color(0.02f, 0.025f, 0.03f, 0.82f);

            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.anchoredPosition = new Vector2(-24f, 24f);
            panelRect.sizeDelta = new Vector2(380f, 210f);

            var text = CreateUiText("Click Roll to move.", panel.transform, 18, TextAnchor.UpperLeft, Color.white);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(16f, 14f);
            text.rectTransform.offsetMax = new Vector2(-16f, -14f);

            var logView = panel.AddComponent<GameLogView>();
            logView.Configure(text);
            return logView;
        }

        private static ConfirmationView CreateConfirmationView(Transform parent)
        {
            var panel = new GameObject("Confirmation Panel");
            panel.transform.SetParent(parent, false);
            var background = panel.AddComponent<Image>();
            background.color = new Color(0.03f, 0.035f, 0.04f, 0.94f);

            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(420f, 170f);

            var message = CreateUiText("Confirm building action.", panel.transform, 20, TextAnchor.MiddleCenter, Color.white);
            message.rectTransform.anchorMin = new Vector2(0f, 0.35f);
            message.rectTransform.anchorMax = new Vector2(1f, 1f);
            message.rectTransform.offsetMin = new Vector2(24f, 0f);
            message.rectTransform.offsetMax = new Vector2(-24f, -16f);

            var buttonObject = new GameObject("Confirm Button");
            buttonObject.transform.SetParent(panel.transform, false);
            var buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.95f, 0.78f, 0.22f);
            var button = buttonObject.AddComponent<Button>();
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.anchoredPosition = new Vector2(0f, 22f);
            buttonRect.sizeDelta = new Vector2(150f, 44f);

            var label = CreateUiText("Confirm", buttonObject.transform, 20, TextAnchor.MiddleCenter, Color.black);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.sizeDelta = Vector2.zero;

            var confirmationView = panel.AddComponent<ConfirmationView>();
            confirmationView.Configure(panel, message, button);
            return confirmationView;
        }

        private static Text CreateUiText(string content, Transform parent, int fontSize, TextAnchor alignment, Color color)
        {
            var textObject = new GameObject("Text");
            textObject.transform.SetParent(parent, false);
            var text = textObject.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

    }
}
