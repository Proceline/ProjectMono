using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyPrototype
{
    public sealed class ConfirmationView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text messageText;
        [SerializeField] private Button confirmButton;

        private bool isConfirmed;

        public void Configure(GameObject targetPanel, Text targetMessageText, Button targetConfirmButton)
        {
            panel = targetPanel;
            messageText = targetMessageText;
            confirmButton = targetConfirmButton;
            Hide();
        }

        public IEnumerator WaitForConfirmation(string message)
        {
            if (panel == null || messageText == null || confirmButton == null)
            {
                yield break;
            }

            isConfirmed = false;
            messageText.text = message;
            panel.SetActive(true);
            confirmButton.onClick.RemoveListener(Confirm);
            confirmButton.onClick.AddListener(Confirm);

            while (!isConfirmed)
            {
                yield return null;
            }

            confirmButton.onClick.RemoveListener(Confirm);
            Hide();
        }

        private void Confirm()
        {
            isConfirmed = true;
        }

        private void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
