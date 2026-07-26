using System.Collections.Generic;
using UnityEngine;

namespace MonopolyPrototype
{
    [CreateAssetMenu(menuName = "Monopoly Prototype/Building")]
    public sealed class BuildingConfig : ScriptableObject
    {
        [SerializeField] private string buildingName = "Building";
        [SerializeField] private BuildingTriggerMode triggerMode = BuildingTriggerMode.Stop;
        [SerializeField] private List<BuildingEffectConfig> effects = new List<BuildingEffectConfig>();

        public string BuildingName => buildingName;

        public BuildingDefinition ToDefinition()
        {
            var definitions = new List<BuildingEffectDefinition>();
            for (var i = 0; i < effects.Count; i++)
            {
                definitions.Add(effects[i].ToDefinition());
            }

            return new BuildingDefinition(buildingName, triggerMode, definitions);
        }

        public void Configure(string name, BuildingTriggerMode trigger, IReadOnlyList<BuildingEffectConfig> effectConfigs)
        {
            buildingName = name ?? string.Empty;
            triggerMode = trigger;
            effects = effectConfigs == null
                ? new List<BuildingEffectConfig>()
                : new List<BuildingEffectConfig>(effectConfigs);
        }
    }

    [System.Serializable]
    public struct BuildingEffectConfig
    {
        [SerializeField] private BuildingEffectType effectType;
        [SerializeField] private int moneyAmount;
        [SerializeField] private int targetTileIndex;
        [SerializeField] private string message;

        private BuildingEffectConfig(BuildingEffectType type, int amount, int targetIndex, string feedbackMessage)
        {
            effectType = type;
            moneyAmount = amount;
            targetTileIndex = targetIndex;
            message = feedbackMessage ?? string.Empty;
        }

        public BuildingEffectDefinition ToDefinition()
        {
            switch (effectType)
            {
                case BuildingEffectType.AddMoney:
                    return BuildingEffectDefinition.AddMoney(moneyAmount);
                case BuildingEffectType.SubtractMoney:
                    return BuildingEffectDefinition.SubtractMoney(moneyAmount);
                case BuildingEffectType.Teleport:
                    return BuildingEffectDefinition.TeleportTo(targetTileIndex);
                case BuildingEffectType.RequestConfirmation:
                    return BuildingEffectDefinition.RequestConfirmation(message);
                case BuildingEffectType.ShowFeedback:
                    return BuildingEffectDefinition.ShowFeedback(message);
                default:
                    return BuildingEffectDefinition.ShowFeedback(string.Empty);
            }
        }

        public static BuildingEffectConfig AddMoney(int amount)
        {
            return new BuildingEffectConfig(BuildingEffectType.AddMoney, amount, 0, string.Empty);
        }

        public static BuildingEffectConfig SubtractMoney(int amount)
        {
            return new BuildingEffectConfig(BuildingEffectType.SubtractMoney, amount, 0, string.Empty);
        }

        public static BuildingEffectConfig TeleportTo(int targetTileIndex)
        {
            return new BuildingEffectConfig(BuildingEffectType.Teleport, 0, targetTileIndex, string.Empty);
        }

        public static BuildingEffectConfig RequestConfirmation(string message)
        {
            return new BuildingEffectConfig(BuildingEffectType.RequestConfirmation, 0, 0, message);
        }

        public static BuildingEffectConfig ShowFeedback(string message)
        {
            return new BuildingEffectConfig(BuildingEffectType.ShowFeedback, 0, 0, message);
        }
    }
}
