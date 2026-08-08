using System.Collections;
using System.Linq;
using MonopolyPrototype;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class Prototype3DScenePlayModeTests
{
    [UnityTest]
    public IEnumerator SampleScene_CreatesPlayable3DBoard()
    {
        SceneManager.LoadScene("Assets/Scenes/SampleScene.unity", LoadSceneMode.Single);
        yield return null;
        yield return null;

        var camera = Camera.main;
        Assert.IsNotNull(camera);
        Assert.IsFalse(camera.orthographic);

        var boardView = Object.FindFirstObjectByType<Prototype3DBoardView>();
        Assert.IsNotNull(boardView);
        Assert.IsNotNull(Object.FindFirstObjectByType<PrototypeBoardTileView>());
        Assert.IsNotNull(GameObject.Find("Board Platform"));

        var moneyFeedback = Object.FindFirstObjectByType<MoneyChangedCoinFeedback>();
        Assert.IsNotNull(moneyFeedback);
        Assert.IsNotNull(moneyFeedback.MoneyChangedEvent);

        var cameraDownwardAlignment = Vector3.Dot(camera.transform.forward, Vector3.down);
        Assert.Greater(cameraDownwardAlignment, 0.75f);
        Assert.Less(cameraDownwardAlignment, 0.95f);

        var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        Assert.GreaterOrEqual(renderers.Length, 20);

        var token = Object.FindFirstObjectByType<PlayerToken>();
        Assert.IsNotNull(token);
        Assert.Greater(token.transform.position.y, 0.5f);
        Assert.IsNotNull(token.GetComponent<CapsuleCollider>());

        var rollButtonObject = GameObject.Find("Roll Button");
        Assert.IsNotNull(rollButtonObject);
        var rollButton = rollButtonObject.GetComponent<Button>();
        Assert.IsNotNull(rollButton);

        rollButton.onClick.Invoke();
        yield return null;

        Assert.IsFalse(rollButton.interactable);

        moneyFeedback.MoneyChangedEvent.Raise(CreateResult(100, 25));
        yield return null;

        Assert.That(moneyFeedback.LastFeedbackText, Is.EqualTo("+25"));
        Assert.GreaterOrEqual(moneyFeedback.ActiveFeedbackCount, 1);
        Assert.IsNotNull(moneyFeedback.LastSpawnedFeedback);
        Assert.IsNotNull(moneyFeedback.LastSpawnedFeedback.GetComponentInChildren<TextMesh>());
        Assert.IsNotNull(moneyFeedback.LastSpawnedFeedback.GetComponentInChildren<MeshRenderer>());
    }

    [UnityTest]
    public IEnumerator BankMoveAppliesMoneyAndRaisesChangedFeedback()
    {
        SceneManager.LoadScene("Assets/Scenes/SampleScene.unity", LoadSceneMode.Single);
        yield return null;
        yield return null;

        var moneyFeedback = Object.FindFirstObjectByType<MoneyChangedCoinFeedback>();
        var controller = Object.FindFirstObjectByType<BoardController>();
        var token = Object.FindFirstObjectByType<PlayerToken>();
        var rollButton = GameObject.Find("Roll Button").GetComponent<Button>();
        var route = Object.FindObjectsByType<BoardTile>(FindObjectsSortMode.None)
            .OrderBy(tile => tile.transform.GetSiblingIndex())
            .ToList();

        Assert.IsNotNull(moneyFeedback);
        Assert.IsNotNull(controller);
        Assert.IsNotNull(token);
        Assert.That(route.Count, Is.GreaterThan(1));

        controller.Configure(
            route,
            token,
            Object.FindFirstObjectByType<GameLogView>(),
            Object.FindFirstObjectByType<ConfirmationView>(),
            rollButton,
            new FixedDiceRoller(1));

        rollButton.onClick.Invoke();
        yield return new WaitUntil(() => rollButton.interactable);

        Assert.That(moneyFeedback.FeedbackCount, Is.EqualTo(1));
        Assert.That(moneyFeedback.LastFeedbackText, Is.EqualTo("+100"));
        Assert.IsNotNull(moneyFeedback.LastResult);
        Assert.That(moneyFeedback.LastResult.BalanceBefore, Is.EqualTo(0));
        Assert.That(moneyFeedback.LastResult.BalanceAfter, Is.EqualTo(100));
    }

    [UnityTest]
    public IEnumerator MoneyFeedback_PlacesAmountBesideCoinAndFacesCamera()
    {
        SceneManager.LoadScene("Assets/Scenes/SampleScene.unity", LoadSceneMode.Single);
        yield return null;
        yield return null;

        var moneyFeedback = Object.FindFirstObjectByType<MoneyChangedCoinFeedback>();
        var camera = Camera.main;

        Assert.IsNotNull(moneyFeedback);
        Assert.IsNotNull(camera);

        moneyFeedback.MoneyChangedEvent.Raise(CreateResult(100, 25));
        yield return null;

        var popup = moneyFeedback.LastSpawnedFeedback;
        var coin = popup.transform.Find("Coin");
        var amount = popup.transform.Find("Amount");
        var amountText = amount.GetComponent<TextMesh>();
        var toCamera = (camera.transform.position - amountText.transform.position).normalized;
        var screenHorizontalDistance = Mathf.Abs(Vector3.Dot(
            amountText.transform.position - coin.position,
            camera.transform.right));

        Assert.Greater(
            Vector3.Dot(-amountText.transform.forward, toCamera),
            0.95f,
            "Legacy TextMesh should face the camera with its readable front side.");
        Assert.Greater(
            screenHorizontalDistance,
            0.45f,
            "The amount should be horizontally separated from the coin.");
    }

    private static MoneyChangeResult CreateResult(int requestDelta, int appliedDelta)
    {
        var request = new MoneyChangeRequest(
            "Bank",
            "Bank_AdjustMoney",
            BuildingEffectType.AdjustMoney,
            1,
            4,
            MoveEventTiming.Stop,
            0,
            requestDelta);

        return new MoneyChangeResult(
            request,
            appliedDelta,
            balanceBefore: 100,
            balanceAfter: 100 + appliedDelta,
            succeeded: true);
    }

    private sealed class FixedDiceRoller : IDiceRoller
    {
        private readonly int steps;

        public FixedDiceRoller(int steps)
        {
            this.steps = steps;
        }

        public int Roll()
        {
            return steps;
        }
    }
}
