using MonopolyPrototype;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class PrototypeBuildingAssetTests
{
    [Test]
    public void BuildingAssets_ContainConfiguredPrototypeBuildings()
    {
        var expectedNames = new[]
        {
            "Start", "Bank", "Gate", "Shop", "Station", "Park", "Library",
            "Museum", "Hotel", "Market", "Clinic", "Theater", "Harbor",
        };
        for (var i = 0; i < expectedNames.Length; i++)
        {
            Assert.IsNotNull(LoadBuilding(expectedNames[i]), expectedNames[i]);
        }

        AssertBuilding("Start", BuildingTriggerMode.Stop,
            new[] { BuildingEffectType.ShowFeedback },
            new[] { "Stopped at Start." });
        AssertBuilding("Bank", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.ShowFeedback, BuildingEffectType.AddMoney, BuildingEffectType.ShowFeedback },
            new[] { "Passed Bank: auto bonus feedback.", string.Empty, "Bank bonus: +100 money." });
        AssertBuilding("Gate", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.RequestConfirmation, BuildingEffectType.ShowFeedback },
            new[] { "Gate checkpoint: confirm before moving on.", "Gate checkpoint cleared." });
        AssertBuilding("Shop", BuildingTriggerMode.Stop,
            new[] { BuildingEffectType.RequestConfirmation, BuildingEffectType.SubtractMoney, BuildingEffectType.ShowFeedback },
            new[] { "Shop visit: confirm the stop action.", string.Empty, "Shop fee: -40 money." });
        AssertBuilding("Station", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.RequestConfirmation },
            new[] { "Station crossing: confirm the train signal." });
        AssertBuilding("Park", BuildingTriggerMode.Stop,
            new[] { BuildingEffectType.ShowFeedback },
            new[] { "Stopped at Park." });
        AssertBuilding("Library", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.ShowFeedback },
            new[] { "Passed Library: quiet auto feedback." });
        AssertBuilding("Museum", BuildingTriggerMode.Stop,
            new[] { BuildingEffectType.RequestConfirmation },
            new[] { "Museum visit: confirm the exhibit action." });
        AssertBuilding("Hotel", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.ShowFeedback },
            new[] { "Passed Hotel: lobby feedback." });
        AssertBuilding("Market", BuildingTriggerMode.Stop,
            new[] { BuildingEffectType.ShowFeedback },
            new[] { "Stopped at Market." });
        AssertBuilding("Clinic", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.ShowFeedback },
            new[] { "Passed Clinic: auto health feedback." });
        AssertBuilding("Theater", BuildingTriggerMode.Stop,
            new[] { BuildingEffectType.RequestConfirmation },
            new[] { "Theater visit: confirm the show action." });
        AssertBuilding("Harbor", BuildingTriggerMode.PassOrStop,
            new[] { BuildingEffectType.RequestConfirmation, BuildingEffectType.Teleport },
            new[] { "Harbor crossing: confirm ship traffic.", string.Empty });
    }

    private static void AssertBuilding(
        string name,
        BuildingTriggerMode expectedTrigger,
        BuildingEffectType[] expectedEffects,
        string[] expectedMessages)
    {
        var config = LoadBuilding(name);
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
    public void BuildingAssets_PreserveMoneyAndTeleportPayloads()
    {
        Assert.AreEqual(100, LoadBuilding("Bank").ToDefinition().Effects[1].MoneyAmount);
        Assert.AreEqual(40, LoadBuilding("Shop").ToDefinition().Effects[1].MoneyAmount);
        Assert.AreEqual(0, LoadBuilding("Harbor").ToDefinition().Effects[1].TargetTileIndex);
    }

    [Test]
    public void SampleScene_ReferencesPrototypeMapData()
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
            var mapData = serializedBootstrapper.FindProperty("mapData").objectReferenceValue;
            Assert.IsNotNull(mapData);
            Assert.IsInstanceOf<PrototypeMapData>(mapData);
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static BuildingConfig LoadBuilding(string name)
    {
        return AssetDatabase.LoadAssetAtPath<BuildingConfig>($"Assets/Data/Buildings/{name}.asset");
    }
}
