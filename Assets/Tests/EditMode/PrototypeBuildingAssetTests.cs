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

        var bank = catalog.Find("Bank");
        Assert.IsNotNull(bank);
        var bankDefinition = bank.ToDefinition();
        Assert.AreEqual(BuildingTriggerMode.PassOrStop, bankDefinition.TriggerMode);
        Assert.AreEqual(BuildingEffectType.AddMoney, bankDefinition.Effects[0].EffectType);
        Assert.AreEqual(100, bankDefinition.Effects[0].MoneyAmount);
        Assert.AreEqual("Bank bonus: +100 money.", bankDefinition.Effects[1].Message);

        var gate = catalog.Find("Gate");
        Assert.IsNotNull(gate);
        var gateDefinition = gate.ToDefinition();
        Assert.AreEqual(BuildingTriggerMode.PassOrStop, gateDefinition.TriggerMode);
        Assert.AreEqual(BuildingEffectType.ShowFeedback, gateDefinition.Effects[0].EffectType);
        Assert.AreEqual("Gate checkpoint cleared.", gateDefinition.Effects[0].Message);

        var shop = catalog.Find("Shop");
        Assert.IsNotNull(shop);
        var shopDefinition = shop.ToDefinition();
        Assert.AreEqual(BuildingTriggerMode.Stop, shopDefinition.TriggerMode);
        Assert.AreEqual(BuildingEffectType.SubtractMoney, shopDefinition.Effects[0].EffectType);
        Assert.AreEqual(40, shopDefinition.Effects[0].MoneyAmount);
        Assert.AreEqual("Shop fee: -40 money.", shopDefinition.Effects[1].Message);

        var harbor = catalog.Find("Harbor");
        Assert.IsNotNull(harbor);
        var harborDefinition = harbor.ToDefinition();
        Assert.AreEqual(BuildingTriggerMode.PassOrStop, harborDefinition.TriggerMode);
        Assert.AreEqual(BuildingEffectType.Teleport, harborDefinition.Effects[0].EffectType);
        Assert.AreEqual(0, harborDefinition.Effects[0].TargetTileIndex);
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
