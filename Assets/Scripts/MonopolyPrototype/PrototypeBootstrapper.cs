using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MonopolyPrototype
{
    public sealed class PrototypeBootstrapper : MonoBehaviour
    {
        private static readonly Vector2[] TilePositions =
        {
            new Vector2(-4.5f, -2.5f), new Vector2(-2.7f, -2.5f), new Vector2(-0.9f, -2.5f), new Vector2(0.9f, -2.5f),
            new Vector2(2.7f, -2.5f), new Vector2(4.5f, -2.5f), new Vector2(4.5f, -0.8f), new Vector2(4.5f, 0.9f),
            new Vector2(2.7f, 0.9f), new Vector2(0.9f, 0.9f), new Vector2(-0.9f, 0.9f), new Vector2(-2.7f, 0.9f),
            new Vector2(-4.5f, 0.9f), new Vector2(-4.5f, -0.8f),
        };

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

        private static void SetupCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.orthographic = true;
            camera.orthographicSize = 4.8f;
            camera.transform.position = new Vector3(0f, -0.65f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.09f, 0.11f);
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

        private static IReadOnlyList<BoardTile> CreateBoard()
        {
            var parent = new GameObject("Prototype Board").transform;
            var sprite = CreateSquareSprite();
            var definitions = new[]
            {
                ("Start", FacilityInteractionType.StopAutoFeedback, "Stopped at Start."),
                ("Bank", FacilityInteractionType.PassAutoFeedback, "Passed Bank: auto bonus feedback."),
                ("Blank", FacilityInteractionType.None, ""),
                ("Gate", FacilityInteractionType.PassConfirmFeedback, "Gate checkpoint: confirm before moving on."),
                ("Shop", FacilityInteractionType.StopConfirmFeedback, "Shop visit: confirm the stop action."),
                ("Station", FacilityInteractionType.PassConfirmFeedback, "Station crossing: confirm the train signal."),
                ("Park", FacilityInteractionType.StopAutoFeedback, "Stopped at Park."),
                ("Library", FacilityInteractionType.PassAutoFeedback, "Passed Library: quiet auto feedback."),
                ("Museum", FacilityInteractionType.StopConfirmFeedback, "Museum visit: confirm the exhibit action."),
                ("Hotel", FacilityInteractionType.PassAutoFeedback, "Passed Hotel: lobby feedback."),
                ("Market", FacilityInteractionType.StopAutoFeedback, "Stopped at Market."),
                ("Clinic", FacilityInteractionType.PassAutoFeedback, "Passed Clinic: auto health feedback."),
                ("Theater", FacilityInteractionType.StopConfirmFeedback, "Theater visit: confirm the show action."),
                ("Harbor", FacilityInteractionType.PassConfirmFeedback, "Harbor crossing: confirm ship traffic."),
            };

            var tiles = new List<BoardTile>();
            for (var i = 0; i < TilePositions.Length; i++)
            {
                var tileObject = new GameObject($"Tile - {definitions[i].Item1}");
                tileObject.transform.SetParent(parent);
                tileObject.transform.position = new Vector3(TilePositions[i].x, TilePositions[i].y, 0f);

                var renderer = tileObject.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = GetTileColor(definitions[i].Item2);
                renderer.sortingOrder = 0;

                var tile = tileObject.AddComponent<BoardTile>();
                tile.Configure(definitions[i].Item1, definitions[i].Item2, definitions[i].Item3);
                tiles.Add(tile);

                CreateTileLabel(tileObject.transform, definitions[i].Item1);
            }

            return tiles;
        }

        private static PlayerToken CreateToken()
        {
            var tokenObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tokenObject.name = "Player Token";
            tokenObject.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            var renderer = tokenObject.GetComponent<Renderer>();
            renderer.material.color = new Color(0.95f, 0.78f, 0.22f);
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

            var message = CreateUiText("Confirm facility action.", panel.transform, 20, TextAnchor.MiddleCenter, Color.white);
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

        private static void CreateTileLabel(Transform parent, string label)
        {
            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(parent);
            labelObject.transform.localPosition = new Vector3(0f, 0f, -0.2f);

            var textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = label;
            textMesh.fontSize = 36;
            textMesh.characterSize = 0.08f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;
        }

        private static Color GetTileColor(FacilityInteractionType interactionType)
        {
            switch (interactionType)
            {
                case FacilityInteractionType.StopAutoFeedback:
                    return new Color(0.21f, 0.46f, 0.64f);
                case FacilityInteractionType.StopConfirmFeedback:
                    return new Color(0.45f, 0.33f, 0.68f);
                case FacilityInteractionType.PassConfirmFeedback:
                    return new Color(0.73f, 0.35f, 0.24f);
                case FacilityInteractionType.PassAutoFeedback:
                    return new Color(0.22f, 0.57f, 0.39f);
                default:
                    return new Color(0.34f, 0.34f, 0.34f);
            }
        }

        private static Sprite CreateSquareSprite()
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 0.65f);
        }
    }
}
