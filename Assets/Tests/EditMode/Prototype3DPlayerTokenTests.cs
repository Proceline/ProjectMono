using MonopolyPrototype;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class Prototype3DPlayerTokenTests
{
    private GameObject tokenObject;

    [TearDown]
    public void TearDown()
    {
        if (tokenObject != null)
        {
            Object.DestroyImmediate(tokenObject);
        }
    }

    [Test]
    public void DefaultTokenOffset_RaisesTokenAboveTile()
    {
        tokenObject = new GameObject("3D Token Test");
        tokenObject.AddComponent<PlayerToken>();

        var serializedToken = new SerializedObject(tokenObject.GetComponent<PlayerToken>());
        var offset = serializedToken.FindProperty("tileOffset").vector3Value;

        Assert.Greater(offset.y, 0.5f);
        Assert.AreEqual(0f, offset.x);
        Assert.AreEqual(0f, offset.z);
    }
}
