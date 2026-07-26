using System.Collections.Generic;

namespace MonopolyPrototype
{
    public enum BuildingTriggerMode
    {
        Pass,
        Stop,
        PassOrStop
    }

    public enum BuildingEffectType
    {
        AddMoney,
        SubtractMoney,
        Teleport,
        RequestConfirmation,
        ShowFeedback
    }

    public sealed class BuildingDefinition
    {
        public BuildingDefinition(string name, BuildingTriggerMode triggerMode, IReadOnlyList<BuildingEffectDefinition> effects)
        {
            Name = name ?? string.Empty;
            TriggerMode = triggerMode;
            Effects = effects ?? new List<BuildingEffectDefinition>();
        }

        public string Name { get; }
        public BuildingTriggerMode TriggerMode { get; }
        public IReadOnlyList<BuildingEffectDefinition> Effects { get; }
    }

    public readonly struct BuildingEffectDefinition
    {
        private BuildingEffectDefinition(BuildingEffectType effectType, int moneyAmount, int targetTileIndex, string message)
        {
            EffectType = effectType;
            MoneyAmount = moneyAmount;
            TargetTileIndex = targetTileIndex;
            Message = message ?? string.Empty;
        }

        public BuildingEffectType EffectType { get; }
        public int MoneyAmount { get; }
        public int TargetTileIndex { get; }
        public string Message { get; }

        public static BuildingEffectDefinition AddMoney(int amount)
        {
            return new BuildingEffectDefinition(BuildingEffectType.AddMoney, amount, 0, string.Empty);
        }

        public static BuildingEffectDefinition SubtractMoney(int amount)
        {
            return new BuildingEffectDefinition(BuildingEffectType.SubtractMoney, amount, 0, string.Empty);
        }

        public static BuildingEffectDefinition TeleportTo(int targetTileIndex)
        {
            return new BuildingEffectDefinition(BuildingEffectType.Teleport, 0, targetTileIndex, string.Empty);
        }

        public static BuildingEffectDefinition RequestConfirmation(string message)
        {
            return new BuildingEffectDefinition(BuildingEffectType.RequestConfirmation, 0, 0, message);
        }

        public static BuildingEffectDefinition ShowFeedback(string message)
        {
            return new BuildingEffectDefinition(BuildingEffectType.ShowFeedback, 0, 0, message);
        }
    }

    public readonly struct BuildingEffectCommand
    {
        public BuildingEffectCommand(BuildingEffectType effectType, int moneyDelta, int targetTileIndex, string message)
        {
            EffectType = effectType;
            MoneyDelta = moneyDelta;
            TargetTileIndex = targetTileIndex;
            Message = message ?? string.Empty;
        }

        public BuildingEffectType EffectType { get; }
        public int MoneyDelta { get; }
        public int TargetTileIndex { get; }
        public string Message { get; }
        public bool RequiresConfirmation => EffectType == BuildingEffectType.RequestConfirmation;
    }

    public static class BuildingRuleResolver
    {
        public static IReadOnlyList<BuildingEffectCommand> Resolve(BuildingDefinition building, MoveEventTiming timing)
        {
            if (building == null || !MatchesTrigger(building.TriggerMode, timing))
            {
                return new List<BuildingEffectCommand>();
            }

            var commands = new List<BuildingEffectCommand>();
            for (var i = 0; i < building.Effects.Count; i++)
            {
                commands.Add(ToCommand(building.Effects[i]));
            }

            return commands;
        }

        private static bool MatchesTrigger(BuildingTriggerMode triggerMode, MoveEventTiming timing)
        {
            return triggerMode == BuildingTriggerMode.PassOrStop
                || triggerMode == BuildingTriggerMode.Pass && timing == MoveEventTiming.Pass
                || triggerMode == BuildingTriggerMode.Stop && timing == MoveEventTiming.Stop;
        }

        private static BuildingEffectCommand ToCommand(BuildingEffectDefinition effect)
        {
            switch (effect.EffectType)
            {
                case BuildingEffectType.AddMoney:
                    return new BuildingEffectCommand(effect.EffectType, effect.MoneyAmount, 0, string.Empty);
                case BuildingEffectType.SubtractMoney:
                    return new BuildingEffectCommand(effect.EffectType, -effect.MoneyAmount, 0, string.Empty);
                case BuildingEffectType.Teleport:
                    return new BuildingEffectCommand(effect.EffectType, 0, effect.TargetTileIndex, string.Empty);
                case BuildingEffectType.RequestConfirmation:
                case BuildingEffectType.ShowFeedback:
                    return new BuildingEffectCommand(effect.EffectType, 0, 0, effect.Message);
                default:
                    return new BuildingEffectCommand(effect.EffectType, 0, 0, string.Empty);
            }
        }
    }
}
