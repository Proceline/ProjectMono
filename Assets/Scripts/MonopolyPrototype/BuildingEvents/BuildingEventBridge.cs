using System;
using System.Collections.Generic;

namespace MonopolyPrototype
{
    public sealed class BuildingEventBridge
    {
        private readonly IReadOnlyList<BuildingConfig> buildingConfigs;
        private readonly MoneyStateAdapter moneyStateAdapter;

        public BuildingEventBridge(
            IReadOnlyList<BuildingConfig> buildingConfigs,
            MoneyStateAdapter moneyStateAdapter = null)
        {
            this.buildingConfigs = buildingConfigs ?? throw new ArgumentNullException(nameof(buildingConfigs));
            this.moneyStateAdapter = moneyStateAdapter ?? new MoneyStateAdapter();
        }

        public void RaiseBuildingTriggered(BoardMoveResolver.MoveEvent moveEvent)
        {
            if (!TryGetProfile(moveEvent.TileIndex, out var profile) || moveEvent.Building == null)
            {
                return;
            }

            profile.RaiseBuildingTriggered(CreateContext(
                moveEvent,
                BuildingEventPhase.BuildingTriggered,
                default(BuildingEffectCommand)));
        }

        public void RaiseEffectCommandProduced(
            BoardMoveResolver.MoveEvent moveEvent,
            BuildingEffectCommand command)
        {
            if (!TryGetProfile(moveEvent.TileIndex, out var profile) || moveEvent.Building == null)
            {
                return;
            }

            profile.RaiseEffectCommandProduced(CreateContext(
                moveEvent,
                BuildingEventPhase.EffectCommandProduced,
                command));
        }

        public MoneyChangeRequest RaiseMoneyChangeRequested(
            BoardMoveResolver.MoveEvent moveEvent,
            BuildingEffectCommand command)
        {
            if (!IsMoneyChange(command.EffectType) || moveEvent.Building == null)
            {
                return null;
            }

            var effectAsset = GetEffectAsset(moveEvent.TileIndex, command.EffectIndex) as AdjustMoneyEffectAsset;
            var request = new MoneyChangeRequest(
                moveEvent.Building.Name,
                effectAsset != null ? effectAsset.name : string.Empty,
                command.EffectType,
                command.EffectIndex,
                moveEvent.TileIndex,
                moveEvent.Timing,
                transactionId: 0,
                command.MoneyDelta);

            effectAsset?.RaiseMoneyChangeRequested(request);
            return request;
        }

        public MoneyChangeResult ApplyMoneyChange(
            BoardMoveResolver.MoveEvent moveEvent,
            BuildingEffectCommand command,
            MoneyChangeRequest request)
        {
            if (!IsMoneyChange(command.EffectType) || moveEvent.Building == null || request == null)
            {
                return null;
            }

            var result = moneyStateAdapter.Apply(request);
            var effectAsset = GetEffectAsset(moveEvent.TileIndex, command.EffectIndex) as AdjustMoneyEffectAsset;
            effectAsset?.RaiseMoneyChanged(result);
            return result;
        }

        public void RaiseConfirmationCompleted(
            BoardMoveResolver.MoveEvent moveEvent,
            BuildingEffectCommand command)
        {
            if (!TryGetProfile(moveEvent.TileIndex, out var profile) || moveEvent.Building == null)
            {
                return;
            }

            profile.RaiseConfirmationCompleted(CreateContext(
                moveEvent,
                BuildingEventPhase.ConfirmationCompleted,
                command));
        }

        private bool TryGetProfile(int tileIndex, out BuildingEventProfile profile)
        {
            profile = null;
            if (tileIndex < 0 || tileIndex >= buildingConfigs.Count)
            {
                return false;
            }

            var buildingConfig = buildingConfigs[tileIndex];
            profile = buildingConfig != null ? buildingConfig.EventProfile : null;
            return profile != null;
        }

        private BuildingEffectAsset GetEffectAsset(int tileIndex, int effectIndex)
        {
            if (tileIndex < 0 || tileIndex >= buildingConfigs.Count || effectIndex < 0)
            {
                return null;
            }

            var buildingConfig = buildingConfigs[tileIndex];
            if (buildingConfig == null
                || buildingConfig.Effects == null
                || effectIndex >= buildingConfig.Effects.Count)
            {
                return null;
            }

            return buildingConfig.Effects[effectIndex];
        }

        private static bool IsMoneyChange(BuildingEffectType effectType)
        {
            return effectType == BuildingEffectType.AdjustMoney;
        }

        private static BuildingEventContext CreateContext(
            BoardMoveResolver.MoveEvent moveEvent,
            BuildingEventPhase phase,
            BuildingEffectCommand command)
        {
            return new BuildingEventContext(
                moveEvent.Building.Name,
                moveEvent.TileIndex,
                moveEvent.Timing,
                phase,
                command.EffectType,
                command.MoneyDelta,
                command.TargetTileIndex,
                command.Message);
        }
    }
}
