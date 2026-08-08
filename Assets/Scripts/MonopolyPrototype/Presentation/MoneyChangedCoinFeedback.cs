using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyPrototype
{
    public sealed class MoneyChangedCoinFeedback : MonoBehaviour
    {
        [SerializeField] private MoneyChangedSOEvent moneyChangedEvent;
        [SerializeField, Min(0.1f)] private float feedbackDuration = 0.9f;
        [SerializeField] private float riseDistance = 48f;
        [SerializeField, Min(8)] private int fontSize = 32;
        [SerializeField] private int canvasSortingOrder = 100;
        [SerializeField] private Color positiveColor = new Color(1f, 0.84f, 0.2f, 1f);
        [SerializeField] private Color negativeColor = new Color(1f, 0.36f, 0.28f, 1f);

        private MoneyChangedSOEvent subscribedEvent;
        private RectTransform feedbackRoot;

        public MoneyChangedSOEvent MoneyChangedEvent => moneyChangedEvent;
        public int FeedbackCount { get; private set; }
        public string LastFeedbackText { get; private set; } = string.Empty;
        public MoneyChangeResult LastResult { get; private set; }

        private void OnEnable()
        {
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
            var root = EnsureFeedbackRoot();
            var popupObject = new GameObject("Money Coin Feedback", typeof(RectTransform), typeof(CanvasGroup), typeof(Text));
            popupObject.transform.SetParent(root, false);

            var popupRect = popupObject.GetComponent<RectTransform>();
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);
            popupRect.anchoredPosition = new Vector2(0f, ((FeedbackCount - 1) % 4) * 18f);
            popupRect.sizeDelta = new Vector2(320f, 64f);

            var text = popupObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = GetFeedbackColor(result);
            text.text = feedbackText;
            text.raycastTarget = false;

            var canvasGroup = popupObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            StartCoroutine(AnimatePopup(popupRect, canvasGroup));
        }

        private RectTransform EnsureFeedbackRoot()
        {
            if (feedbackRoot != null)
            {
                return feedbackRoot;
            }

            var canvasObject = new GameObject("Money Coin Feedback Canvas", typeof(RectTransform));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = canvasSortingOrder;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            feedbackRoot = canvasObject.GetComponent<RectTransform>();
            return feedbackRoot;
        }

        private IEnumerator AnimatePopup(RectTransform popupRect, CanvasGroup canvasGroup)
        {
            var duration = Mathf.Max(0.1f, feedbackDuration);
            var startPosition = popupRect.anchoredPosition;
            var elapsed = 0f;

            while (elapsed < duration && popupRect != null)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                popupRect.anchoredPosition = startPosition + Vector2.up * (riseDistance * progress);
                canvasGroup.alpha = 1f - progress;
                yield return null;
            }

            if (popupRect != null)
            {
                Destroy(popupRect.gameObject);
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
