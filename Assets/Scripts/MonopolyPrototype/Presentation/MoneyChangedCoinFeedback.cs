using System.Collections;
using UnityEngine;

namespace MonopolyPrototype
{
    [ExecuteAlways]
    public sealed class MoneyChangedCoinFeedback : MonoBehaviour
    {
        [SerializeField] private MoneyChangedSOEvent moneyChangedEvent;
        [SerializeField] private Transform feedbackAnchor;
        [SerializeField] private Camera feedbackCamera;
        [SerializeField, Min(0.1f)] private float feedbackDuration = 0.9f;
        [SerializeField, Min(0.1f)] private float riseDistance = 1.1f;
        [SerializeField, Min(0.01f)] private float coinDiameter = 0.55f;
        [SerializeField, Min(0.001f)] private float coinThickness = 0.08f;
        [SerializeField, Min(0.01f)] private float textCharacterSize = 0.14f;
        [SerializeField, Min(8)] private int fontSize = 48;
        [SerializeField] private Color positiveColor = new Color(1f, 0.84f, 0.2f, 1f);
        [SerializeField] private Color negativeColor = new Color(1f, 0.36f, 0.28f, 1f);

        private MoneyChangedSOEvent subscribedEvent;

        public MoneyChangedSOEvent MoneyChangedEvent => moneyChangedEvent;
        public Transform FeedbackAnchor => feedbackAnchor;
        public Camera FeedbackCamera => feedbackCamera;
        public int FeedbackCount { get; private set; }
        public int ActiveFeedbackCount { get; private set; }
        public string LastFeedbackText { get; private set; } = string.Empty;
        public MoneyChangeResult LastResult { get; private set; }
        public GameObject LastSpawnedFeedback { get; private set; }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(MoneyChangedSOEvent eventAsset)
        {
            Unsubscribe();
            moneyChangedEvent = eventAsset;
            Subscribe();
        }

        public void OnMoneyChanged(MoneyChangeResult result)
        {
            if (result == null)
            {
                return;
            }

            LastResult = result;
            LastFeedbackText = FormatFeedbackText(result);
            FeedbackCount++;

            if (Application.isPlaying)
            {
                PlayVisualFeedback(result, LastFeedbackText);
            }
        }

        private void Subscribe()
        {
            if (!isActiveAndEnabled || moneyChangedEvent == null || subscribedEvent != null)
            {
                return;
            }

            moneyChangedEvent.Register(OnMoneyChanged);
            subscribedEvent = moneyChangedEvent;
        }

        private void Unsubscribe()
        {
            if (subscribedEvent == null)
            {
                return;
            }

            subscribedEvent.Unregister(OnMoneyChanged);
            subscribedEvent = null;
        }

        private void PlayVisualFeedback(MoneyChangeResult result, string feedbackText)
        {
            var popupObject = new GameObject("Money Coin Feedback");
            popupObject.transform.position = feedbackAnchor != null
                ? feedbackAnchor.position
                : transform.position;
            popupObject.transform.SetParent(transform, true);

            var color = GetFeedbackColor(result);
            var coinObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coinObject.name = "Coin";
            coinObject.transform.SetParent(popupObject.transform, false);
            coinObject.transform.localPosition = new Vector3(0f, coinThickness, 0f);
            coinObject.transform.localScale = new Vector3(coinDiameter, coinThickness, coinDiameter);
            SetRendererColor(coinObject.GetComponent<Renderer>(), color);

            var collider = coinObject.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var textObject = new GameObject("Amount");
            textObject.transform.SetParent(popupObject.transform, false);
            textObject.transform.localPosition = new Vector3(0f, coinThickness * 2.5f, 0f);

            var amountText = textObject.AddComponent<TextMesh>();
            amountText.text = feedbackText;
            amountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            amountText.fontSize = fontSize;
            amountText.characterSize = textCharacterSize;
            amountText.fontStyle = FontStyle.Bold;
            amountText.alignment = TextAlignment.Center;
            amountText.anchor = TextAnchor.MiddleCenter;
            amountText.color = color;

            FaceCamera(amountText);
            LastSpawnedFeedback = popupObject;
            ActiveFeedbackCount++;
            StartCoroutine(AnimatePopup(popupObject, amountText));
        }

        private IEnumerator AnimatePopup(GameObject popupObject, TextMesh amountText)
        {
            var duration = Mathf.Max(0.1f, feedbackDuration);
            var startPosition = popupObject.transform.position;
            var elapsed = 0f;

            while (elapsed < duration && popupObject != null)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                var easedProgress = 1f - Mathf.Pow(1f - progress, 2f);
                popupObject.transform.position = startPosition + Vector3.up * (riseDistance * easedProgress);
                FaceCamera(amountText);
                yield return null;
            }

            if (popupObject != null)
            {
                Destroy(popupObject);
                ActiveFeedbackCount = Mathf.Max(0, ActiveFeedbackCount - 1);
            }
        }

        private void FaceCamera(TextMesh amountText)
        {
            if (amountText == null)
            {
                return;
            }

            var camera = feedbackCamera != null ? feedbackCamera : Camera.main;
            if (camera != null)
            {
                amountText.transform.LookAt(camera.transform.position);
            }
        }

        private static void SetRendererColor(Renderer renderer, Color color)
        {
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = color;
            }
        }

        private Color GetFeedbackColor(MoneyChangeResult result)
        {
            return result.AppliedDelta < 0 ? negativeColor : positiveColor;
        }

        private static string FormatFeedbackText(MoneyChangeResult result)
        {
            if (result.AppliedDelta > 0)
            {
                return $"+{result.AppliedDelta}";
            }

            return result.AppliedDelta.ToString();
        }
    }
}
