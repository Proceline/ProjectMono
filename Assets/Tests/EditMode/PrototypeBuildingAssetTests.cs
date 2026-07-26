using MonopolyPrototype;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class PrototypeBuildingAssetTests
{
    [Test]
    public void CatalogAsset_ContainsConfiguredPrototypeBuildings()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<PrototypeBuildingCatalog>(
            "Assets/Data/Buildings/PrototypeBuildingCatalog.asset");

        Assert.IsNotNull(catalog);

        var expectedNames = new[]
        {
            "Start", "Bank", "Gate", "Shop", "Station", "Park", "Library",
            "Museum", "Hotel", "Market", "Clinic", "Theater", "Harbor",
        };
        for (var i = 0; i < expectedNames.Length; i++)
        {
            Assert.IsNotNull(catalog.Find(expectedNames[i]), expectedNames[i]);
        }
        Assert.IsNull(catalog.Find("Blank"));

        AssertBuilding(catalog, "Start", BuildingTriggerMode.Stop,
            new[] { BuildingEffectType.ShowFeedback },
            new[] { "Stopped at Start." });
        AssertBuilding(catalog, "Bank", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.ShowFeedback, BuildingEffectType.AddMoney, BuildingEffectType.ShowFeedback },
            new[] { "Passed Bank: auto bonus feedback.", string.Empty, "Bank bonus: +100 money." });
        AssertBuilding(catalog, "Gate", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.RequestConfirmation, BuildingEffectType.ShowFeedback },
            new[] { "Gate checkpoint: confirm before moving on.", "Gate checkpoint cleared." });
        AssertBuilding(catalog, "Shop", BuildingTriggerMode.Stop,
            new[] { BuildingEffectType.RequestConfirmation, BuildingEffectType.SubtractMoney, BuildingEffectType.ShowFeedback },
            new[] { "Shop visit: confirm the stop action.", string.Empty, "Shop fee: -40 money." });
        AssertBuilding(catalog, "Station", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.RequestConfirmation },
            new[] { "Station crossing: confirm the train signal." });
        AssertBuilding(catalog, "Park", BuildingTriggerMode.Stop,
            new[] { BuildingEffectType.ShowFeedback },
            new[] { "Stopped at Park." });
        AssertBuilding(catalog, "Library", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.ShowFeedback },
            new[] { "Passed Library: quiet auto feedback." });
        AssertBuilding(catalog, "Museum", BuildingTriggerMode.Stop,
            new[] { BuildingEffectType.RequestConfirmation },
            new[] { "Museum visit: confirm the exhibit action." });
        AssertBuilding(catalog, "Hotel", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.ShowFeedback },
            new[] { "Passed Hotel: lobby feedback." });
        AssertBuilding(catalog, "Market", BuildingTriggerMode.Stop,
            new[] { BuildingEffectType.ShowFeedback },
            new[] { "Stopped at Market." });
        AssertBuilding(catalog, "Clinic", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.ShowFeedback },
            new[] { "Passed Clinic: auto health feedback." });
        AssertBuilding(catalog, "Theater", BuildingTriggerMode.Stop,
            new[] { BuildingEffectType.RequestConfirmation },
            new[] { "Theater visit: confirm the show action." });
        AssertBuilding(catalog, "Harbor", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.RequestConfirmation, BuildingEffectType.Teleport },
            new[] { "Harbor crossing: confirm ship traffic.", string.Empty });
    }

    private static void AssertBuilding(
        PrototypeBuildingCatalog catalog,
        string name,
        BuildingTriggerMode expectedTrigger,
        BuildingEffectType[] expectedEffects,
        string[] expectedMessages)
    {
        var config = catalog.Find(name);
        Assert.IsNotNull(config, name);

        var definition = config.ToDefinition();
        Assert.AreEqual(expectedTrigger, definition.TriggerMode, name);
        Assert.AreEqual(expectedEffects.Length, definition.Effects.Count, name);
        for (var i = 0; i < expectedEffects.Length; i++)
        {
            Assert.AreEqual(expectedEffects[i], definition.Effects[i].EffectType, $"{name} effect {i}");
            Assert.AreEqual(expectedMessages[i], definition.Effects[i].Message, $"{name} effect {i} message");
        }
    }

    [Test]
    public void CatalogAssets_PreserveMoneyAndTeleportPayloads()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<PrototypeBuildingCatalog>(
            "Assets/Data/Buildings/PrototypeBuildingCatalog.asset");

        Assert.AreEqual(100, catalog.Find("Bank").ToDefinition().Effects[1].MoneyAmount);
        Assert.AreEqual(40, catalog.Find("Shop").ToDefinition().Effects[1].MoneyAmount);
        Assert.AreEqual(0, catalog.Find("Harbor").ToDefinition().Effects[1].TargetTileIndex);
    }

    [Test]
    public void SampleScene_ReferencesPrototypeBuildingCatalog()
    {
        const string scenePath = "Assets/Scenes/SampleScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        try
        {
            PrototypeBootstrapper bootstrapper = null;
            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                bootstrapper = roots[i].GetComponent<PrototypeBootstrapper>();
                if (bootstrapper != null)
                {
                    break;
                }
            }

            Assert.IsNotNull(bootstrapper);
            var serializedBootstrapper = new SerializedObject(bootstrapper);
            var catalog = serializedBootstrapper.FindProperty("buildingCatalog").objectReferenceValue;
            var expectedCatalog = AssetDatabase.LoadAssetAtPath<PrototypeBuildingCatalog>(
                "Assets/Data/Buildings/PrototypeBuildingCatalog.asset");

            Assert.AreSame(expectedCatalog, catalog);
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}
