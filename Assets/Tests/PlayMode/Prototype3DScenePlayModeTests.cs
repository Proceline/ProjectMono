using System.Collections;
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
        Assert.IsNotNull(GameObject.Find("Board Platform"));

        var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        Assert.GreaterOrEqual(renderers.Length, 20);

        var token = Object.FindFirstObjectByType<PlayerToken>();
        Assert.IsNotNull(token);
        Assert.Greater(token.transform.position.y, 0.5f);

        var rollButtonObject = GameObject.Find("Roll Button");
        Assert.IsNotNull(rollButtonObject);
        var rollButton = rollButtonObject.GetComponent<Button>();
        Assert.IsNotNull(rollButton);

        rollButton.onClick.Invoke();
        yield return null;

        Assert.IsFalse(rollButton.interactable);
    }
}
