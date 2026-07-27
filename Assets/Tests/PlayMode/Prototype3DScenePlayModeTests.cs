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
        Assert.IsNotNull(Object.FindFirstObjectByType<PrototypeBoardTileView>());
        Assert.IsNotNull(GameObject.Find("Board Platform"));

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
    }
}
