using System.Collections.Generic;
using UnityEngine;

namespace MonopolyPrototype
{
    [CreateAssetMenu(menuName = "Monopoly Prototype/Building")]
    public sealed class BuildingConfig : ScriptableObject
    {
        [SerializeField] private string buildingName = "Building";
        [SerializeField] private BuildingTriggerMode triggerMode = BuildingTriggerMode.Stop;
        [SerializeField] private List<BuildingEffectAsset> effects = new List<BuildingEffectAsset>();
        [SerializeField] private BuildingEventProfile eventProfile;

        public string BuildingName => buildingName;
        public IReadOnlyList<BuildingEffectAsset> Effects => effects;
        public BuildingEventProfile EventProfile => eventProfile;

        public BuildingDefinition ToDefinition()
        {
            var definitions = new List<BuildingEffectDefinition>();
            if (effects == null)
            {
                return new BuildingDefinition(buildingName, triggerMode, definitions);
            }

            for (var i = 0; i < effects.Count; i++)
            {
                if (effects[i] != null)
                {
                    definitions.Add(effects[i].ToDefinition().WithEffectIndex(i));
                }
            }

            return new BuildingDefinition(buildingName, triggerMode, definitions);
        }

        public void Configure(string name, BuildingTriggerMode trigger, IReadOnlyList<BuildingEffectAsset> effectAssets)
        {
            buildingName = name ?? string.Empty;
            triggerMode = trigger;
            effects = effectAssets == null
                ? new List<BuildingEffectAsset>()
                : new List<BuildingEffectAsset>(effectAssets);
        }

        public bool TryValidate(out string error)
        {
            if (effects == null)
            {
                error = "Building effects list is missing.";
                return false;
            }

            var confirmationCount = 0;
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null)
                {
                    error = $"Building effect {i + 1} is missing.";
                    return false;
                }

                if (effect.EffectType == BuildingEffectType.RequestConfirmation)
                {
                    confirmationCount++;
                    if (confirmationCount > 1)
                    {
                        error = "A building can contain at most one RequestConfirmation effect.";
                        return false;
                    }
                }
            }

            error = string.Empty;
            return true;
        }

        private void OnValidate()
        {
            if (!TryValidate(out var error))
            {
                Debug.LogError($"Invalid building config '{name}': {error}", this);
            }
        }
    }
}
