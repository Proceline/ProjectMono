using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyPrototype
{
    public sealed class GameLogView : MonoBehaviour
    {
        [SerializeField] private Text logText;
        [SerializeField] private int maxLines = 8;

        private readonly Queue<string> lines = new Queue<string>();

        public void Configure(Text targetText)
        {
            logText = targetText;
            Redraw();
        }

        public void AddLine(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            lines.Enqueue(message);
            while (lines.Count > maxLines)
            {
                lines.Dequeue();
            }

            Redraw();
        }

        private void Redraw()
        {
            if (logText == null)
            {
                return;
            }

            logText.text = lines.Count == 0
                ? "Click Roll to move."
                : string.Join("\n", lines.Reverse());
        }
    }
}
