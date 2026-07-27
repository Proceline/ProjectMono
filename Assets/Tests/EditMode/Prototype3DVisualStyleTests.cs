using MonopolyPrototype;
using NUnit.Framework;
using UnityEngine;

public class Prototype3DVisualStyleTests
{
    [Test]
    public void For_BlankDefinition_UsesNeutralTileAndNoMarker()
    {
        var style = Prototype3DVisualStyle.For(null);

        Assert.IsFalse(style.HasMarker);
        Assert.AreEqual(PrimitiveType.Cube, style.MarkerPrimitive);
        Assert.AreEqual(new Color(0.28f, 0.32f, 0.36f), style.TileColor);
    }

    [Test]
    public void For_Start_UsesCylinderMarkerAndGoldPalette()
    {
        var style = Prototype3DVisualStyle.For(CreateBuilding("Start"));

        Assert.IsTrue(style.HasMarker);
        Assert.AreEqual(PrimitiveType.Cylinder, style.MarkerPrimitive);
        Assert.AreEqual(new Color(0.95f, 0.72f, 0.22f), style.MarkerColor);
    }

    [Test]
    public void For_Park_UsesSphereMarkerAndGreenPalette()
    {
        var style = Prototype3DVisualStyle.For(CreateBuilding("Park"));

        Assert.IsTrue(style.HasMarker);
        Assert.AreEqual(PrimitiveType.Sphere, style.MarkerPrimitive);
        Assert.AreEqual(new Color(0.20f, 0.62f, 0.38f), style.MarkerColor);
    }

    [Test]
    public void For_UnknownBuilding_UsesSafeCubeFallback()
    {
        var style = Prototype3DVisualStyle.For(CreateBuilding("Unknown Building"));

        Assert.IsTrue(style.HasMarker);
        Assert.AreEqual(PrimitiveType.Cube, style.MarkerPrimitive);
        Assert.AreEqual(new Color(0.32f, 0.48f, 0.68f), style.MarkerColor);
    }

    private static BuildingDefinition CreateBuilding(string name)
    {
        return new BuildingDefinition(
            name,
            BuildingTriggerMode.Stop,
            new BuildingEffectDefinition[0]);
    }
}
