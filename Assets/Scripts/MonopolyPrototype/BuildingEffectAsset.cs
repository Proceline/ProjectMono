using UnityEngine;

namespace MonopolyPrototype
{
    public abstract class BuildingEffectAsset : ScriptableObject
    {
        public abstract BuildingEffectType EffectType { get; }

        public abstract BuildingEffectDefinition ToDefinition();

        public BuildingEffectCommand ToCommand()
        {
            return BuildingRuleResolver.CreateCommand(ToDefinition());
        }
    }
}
