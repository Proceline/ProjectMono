using System.Collections.Generic;
using UnityEngine;

namespace MonopolyPrototype
{
    [CreateAssetMenu(menuName = "Monopoly Prototype/Building Catalog")]
    public sealed class PrototypeBuildingCatalog : ScriptableObject
    {
        [SerializeField] private List<BuildingConfig> buildings = new List<BuildingConfig>();

        public void Configure(IReadOnlyList<BuildingConfig> configs)
        {
            buildings = configs == null
                ? new List<BuildingConfig>()
                : new List<BuildingConfig>(configs);
        }

        public BuildingConfig Find(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            for (var i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];
                if (building != null && building.BuildingName == name)
                {
                    return building;
                }
            }

            return null;
        }
    }
}
