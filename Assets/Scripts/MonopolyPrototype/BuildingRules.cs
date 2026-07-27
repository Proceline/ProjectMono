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
        AdjustMoney,
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
        private BuildingEffectDefinition(
            BuildingEffectType effectType,
            int moneyDelta,
            int targetTileIndex,
            string message,
            int effectIndex)
        {
            EffectType = effectType;
            MoneyDelta = moneyDelta;
            TargetTileIndex = targetTileIndex;
            Message = message ?? string.Empty;
            EffectIndex = effectIndex;
        }

        public BuildingEffectType EffectType { get; }
        public int MoneyDelta { get; }
        public int TargetTileIndex { get; }
        public string Message { get; }
        public int EffectIndex { get; }

        public BuildingEffectDefinition WithEffectIndex(int effectIndex)
        {
            return new BuildingEffectDefinition(
                EffectType,
                MoneyDelta,
                TargetTileIndex,
                Message,
                effectIndex);
        }

        public static BuildingEffectDefinition AdjustMoney(int delta)
        {
            return new BuildingEffectDefinition(BuildingEffectType.AdjustMoney, delta, 0, string.Empty, -1);
        }

        public static BuildingEffectDefinition TeleportTo(int targetTileIndex)
        {
            return new BuildingEffectDefinition(BuildingEffectType.Teleport, 0, targetTileIndex, string.Empty, -1);
        }

        public static BuildingEffectDefinition RequestConfirmation(string message)
        {
            return new BuildingEffectDefinition(BuildingEffectType.RequestConfirmation, 0, 0, message, -1);
        }

        public static BuildingEffectDefinition ShowFeedback(string message)
        {
            return new BuildingEffectDefinition(BuildingEffectType.ShowFeedback, 0, 0, message, -1);
        }
    }

    public readonly struct BuildingEffectCommand
    {
        public BuildingEffectCommand(
            BuildingEffectType effectType,
            int moneyDelta,
            int targetTileIndex,
            string message,
            int effectIndex = -1)
        {
            EffectType = effectType;
            MoneyDelta = moneyDelta;
            TargetTileIndex = targetTileIndex;
            Message = message ?? string.Empty;
            EffectIndex = effectIndex;
        }

        public BuildingEffectType EffectType { get; }
        public int MoneyDelta { get; }
        public int TargetTileIndex { get; }
        public string Message { get; }
        public int EffectIndex { get; }
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
                commands.Add(CreateCommand(building.Effects[i]));
            }

            return commands;
        }

        private static bool MatchesTrigger(BuildingTriggerMode triggerMode, MoveEventTiming timing)
        {
            return triggerMode == BuildingTriggerMode.PassOrStop
                || triggerMode == BuildingTriggerMode.Pass && timing == MoveEventTiming.Pass
                || triggerMode == BuildingTriggerMode.Stop && timing == MoveEventTiming.Stop;
        }

        public static BuildingEffectCommand CreateCommand(BuildingEffectDefinition effect)
        {
            switch (effect.EffectType)
            {
                case BuildingEffectType.AdjustMoney:
                    return new BuildingEffectCommand(
                        effect.EffectType,
                        effect.MoneyDelta,
                        0,
                        string.Empty,
                        effect.EffectIndex);
                case BuildingEffectType.Teleport:
                    return new BuildingEffectCommand(
                        effect.EffectType,
                        0,
                        effect.TargetTileIndex,
                        string.Empty,
                        effect.EffectIndex);
                case BuildingEffectType.RequestConfirmation:
                case BuildingEffectType.ShowFeedback:
                    return new BuildingEffectCommand(
                        effect.EffectType,
                        0,
                        0,
                        effect.Message,
                        effect.EffectIndex);
                default:
                    return new BuildingEffectCommand(
                        effect.EffectType,
                        0,
                        0,
                        string.Empty,
                        effect.EffectIndex);
            }
        }
    }
}
