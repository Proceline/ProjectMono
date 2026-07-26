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

        private IDiceRoller diceRoller = new UnityRandomDiceRoller();
        private int currentIndex;
        private bool isMoving;

        public void Configure(
            IReadOnlyList<BoardTile> boardRoute,
            PlayerToken token,
            GameLogView gameLogView,
            ConfirmationView confirmationView,
            Button button,
            IDiceRoller roller = null)
        {
            route = boardRoute.ToList();
            playerToken = token;
            logView = gameLogView;
            this.confirmationView = confirmationView;
            rollButton = button;
            diceRoller = roller ?? new UnityRandomDiceRoller();
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

            var steps = diceRoller.Roll();
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
                    yield return HandleMoveEvent(moveEvent);
                }
            }

            currentIndex = result.EndIndex;
            var stopEvent = result.Events.FirstOrDefault(evt => evt.TileIndex == currentIndex && evt.Timing == MoveEventTiming.Stop);
            yield return HandleMoveEvent(stopEvent);

            if (rollButton != null)
            {
                rollButton.interactable = true;
            }

            isMoving = false;
        }

        private IEnumerator HandleMoveEvent(BoardMoveResolver.MoveEvent moveEvent)
        {
            if (moveEvent.BuildingCommands == null)
            {
                yield break;
            }

            for (var i = 0; i < moveEvent.BuildingCommands.Count; i++)
            {
                var command = moveEvent.BuildingCommands[i];
                switch (command.EffectType)
                {
                    case BuildingEffectType.AddMoney:
                    case BuildingEffectType.SubtractMoney:
                        logView?.AddLine($"Money change: {command.MoneyDelta:+#;-#;0}.");
                        break;
                    case BuildingEffectType.Teleport:
                        logView?.AddLine($"Teleport requested to tile {command.TargetTileIndex}.");
                        break;
                    case BuildingEffectType.ShowFeedback:
                        if (!string.IsNullOrWhiteSpace(command.Message))
                        {
                            logView?.AddLine(command.Message);
                        }

                        break;
                    case BuildingEffectType.RequestConfirmation:
                        var message = string.IsNullOrWhiteSpace(command.Message)
                            ? "Confirm building effect."
                            : command.Message;
                        logView?.AddLine(message);
                        if (confirmationView != null)
                        {
                            yield return confirmationView.WaitForConfirmation(message);
                        }

                        break;
                }
            }
        }
    }
}
