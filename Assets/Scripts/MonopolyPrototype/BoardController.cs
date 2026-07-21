using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyPrototype
{
    public sealed class BoardController : MonoBehaviour
    {
        [SerializeField] private List<BoardTile> route = new List<BoardTile>();
        [SerializeField] private PlayerToken playerToken;
        [SerializeField] private GameLogView logView;
        [SerializeField] private ConfirmationView confirmationView;
        [SerializeField] private Button rollButton;

        private int currentIndex;
        private bool isMoving;

        public void Configure(
            IReadOnlyList<BoardTile> boardRoute,
            PlayerToken token,
            GameLogView gameLogView,
            ConfirmationView facilityConfirmationView,
            Button button)
        {
            route = boardRoute.ToList();
            playerToken = token;
            logView = gameLogView;
            confirmationView = facilityConfirmationView;
            rollButton = button;
            WireButton();
            ResetToken();
        }

        private void Awake()
        {
            WireButton();
        }

        private void Start()
        {
            ResetToken();
        }

        private void WireButton()
        {
            if (rollButton == null)
            {
                return;
            }

            rollButton.onClick.RemoveListener(RollAndMove);
            rollButton.onClick.AddListener(RollAndMove);
        }

        private void ResetToken()
        {
            if (route.Count == 0 || playerToken == null)
            {
                return;
            }

            currentIndex = Mathf.Clamp(currentIndex, 0, route.Count - 1);
            playerToken.SnapTo(route[currentIndex]);
        }

        private void RollAndMove()
        {
            if (isMoving || route.Count == 0 || playerToken == null)
            {
                return;
            }

            var steps = Random.Range(1, 7);
            StartCoroutine(MoveRoutine(steps));
        }

        private IEnumerator MoveRoutine(int steps)
        {
            isMoving = true;
            if (rollButton != null)
            {
                rollButton.interactable = false;
            }

            logView?.AddLine($"Rolled {steps}.");

            var definitions = route.Select(tile => tile.ToDefinition()).ToList();
            var result = BoardMoveResolver.ResolveMove(definitions, currentIndex, steps);

            for (var step = 1; step <= steps; step++)
            {
                var nextIndex = (currentIndex + 1) % route.Count;
                yield return playerToken.MoveTo(route[nextIndex]);
                currentIndex = nextIndex;

                var isFinalStep = step == steps;
                if (!isFinalStep)
                {
                    var moveEvent = result.Events.FirstOrDefault(evt => evt.TileIndex == currentIndex && evt.Timing == MoveEventTiming.Pass);
                    if (!string.IsNullOrWhiteSpace(moveEvent.Message))
                    {
                        logView?.AddLine(moveEvent.Message);
                        if (moveEvent.RequiresConfirmation && confirmationView != null)
                        {
                            yield return confirmationView.WaitForConfirmation(moveEvent.Message);
                        }
                    }
                }
            }

            currentIndex = result.EndIndex;
            var stopEvent = result.Events.FirstOrDefault(evt => evt.TileIndex == currentIndex && evt.Timing == MoveEventTiming.Stop);
            if (!string.IsNullOrWhiteSpace(stopEvent.Message))
            {
                logView?.AddLine(stopEvent.Message);
                if (stopEvent.RequiresConfirmation && confirmationView != null)
                {
                    yield return confirmationView.WaitForConfirmation(stopEvent.Message);
                }
            }

            if (rollButton != null)
            {
                rollButton.interactable = true;
            }

            isMoving = false;
        }
    }
}
