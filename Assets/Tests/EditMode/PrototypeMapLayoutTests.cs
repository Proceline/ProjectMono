using MonopolyPrototype;
using NUnit.Framework;
using UnityEngine;

public class PrototypeMapLayoutTests
{
    [Test]
    public void GetWorldPosition_CentersGridAroundConfiguredCenter()
    {
        var first = PrototypeMapLayout.GetWorldPosition(
            new Vector2Int(0, 0),
            new Vector2Int(6, 6),
            new Vector2(10f, 20f),
            new Vector2(2f, 4f));
        var last = PrototypeMapLayout.GetWorldPosition(
            new Vector2Int(5, 5),
            new Vector2Int(6, 6),
            new Vector2(10f, 20f),
            new Vector2(2f, 4f));

        Assert.AreEqual(5f, first.x);
        Assert.AreEqual(10f, first.y);
        Assert.AreEqual(15f, last.x);
        Assert.AreEqual(30f, last.y);
    }

    [Test]
    public void GetWorldBounds_IncludesTileScaleAtMapEdges()
    {
        var bounds = PrototypeMapLayout.GetWorldBounds(
            new Vector2Int(6, 6),
            Vector2.zero,
            new Vector2(2f, 4f),
            1.5f);

        Assert.AreEqual(11.5f, bounds.size.x);
        Assert.AreEqual(21.5f, bounds.size.y);
        Assert.AreEqual(Vector3.zero, bounds.center);
    }
}
