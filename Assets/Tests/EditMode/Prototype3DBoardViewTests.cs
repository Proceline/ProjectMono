using MonopolyPrototype;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class Prototype3DBoardViewTests
{
    private GameObject viewObject;

    [TearDown]
    public void TearDown()
    {
        if (viewObject != null)
        {
            Object.DestroyImmediate(viewObject);
        }
    }

    [Test]
    public void Build_PreservesMapOrderAndCreates3DVisualChildren()
    {
        var mapData = AssetDatabase.LoadAssetAtPath<PrototypeMapData>(
            "Assets/Data/Maps/PrototypeMapData.asset");
        var tilePrefab = AssetDatabase.LoadAssetAtPath<PrototypeBoardTileView>(
            "Assets/Prefabs/PrototypeBoardTile.prefab");
        Assert.IsNotNull(mapData);
        Assert.IsNotNull(tilePrefab);

        viewObject = new GameObject("3D Board View Test");
        var view = viewObject.AddComponent<Prototype3DBoardView>();

        var tiles = view.Build(
            mapData,
            Vector2.zero,
            new Vector2(1.8f, 1.7f),
            1.5f,
            tilePrefab);

        Assert.AreEqual(mapData.Tiles.Count, tiles.Count);
        Assert.AreEqual("Start", tiles[0].TileName);
        Assert.AreEqual("Harbor", tiles[tiles.Count - 1].TileName);
        Assert.GreaterOrEqual(tiles[0].transform.childCount, 2);
        Assert.IsNotNull(tiles[0].GetComponentInChildren<MeshRenderer>());
        Assert.IsTrue(PrefabUtility.IsPartOfPrefabInstance(tiles[0].gameObject));
    }

    [Test]
    public void DefaultTilePrefab_UsesBuiltInMeshAndPresentationComponent()
    {
        var tilePrefab = AssetDatabase.LoadAssetAtPath<PrototypeBoardTileView>(
            "Assets/Prefabs/PrototypeBoardTile.prefab");

        Assert.IsNotNull(tilePrefab);
        Assert.IsNotNull(tilePrefab.GetComponent<BoardTile>());
        Assert.IsNotNull(tilePrefab.GetComponentInChildren<MeshFilter>());
        Assert.IsNotNull(tilePrefab.GetComponentInChildren<MeshFilter>().sharedMesh);
        Assert.AreEqual("Cube", tilePrefab.GetComponentInChildren<MeshFilter>().sharedMesh.name);
    }
}
