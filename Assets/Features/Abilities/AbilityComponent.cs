using System;
using System.Collections.Generic;
using Zekzek.HexWorld;

namespace Zekzek.Ability
{
    public class AbilityComponent : WorldObjectComponent
    {
        public override WorldComponentType ComponentType => WorldComponentType.Ability;

        private List<AbilityData> _knownAbilities = new List<AbilityData>();
        private readonly Dictionary<string, object> _abilityGroups = new Dictionary<string, object>();

        public AbilityComponent(uint worldObjectId) : base(worldObjectId) { }

        public void Add(string abilityId)
        {
            AbilityData abilityData = JsonContent.ContentUtil.LoadData<AbilityData>(abilityId);
            Add(abilityData.Name, _abilityGroups, abilityData.Location.Split('/'));
            _knownAbilities.Add(abilityData);
        }

        private void Add(string abilityName, Dictionary<string, object> group, params string[] location)
        {
            if (location == null || location.Length == 0) {
                group.Add(abilityName, null);
                return;
            }

            string key = location[0];
            if (!group.ContainsKey(key)) {
                group.Add(key, new Dictionary<string, object>());
            }

            string[] remainingLocation = new string[location.Length - 1];
            Array.Copy(location, 1, remainingLocation, 0, remainingLocation.Length);
            Add(abilityName, group[key] as Dictionary<string, object>, remainingLocation);
        }

        public List<string> GetOptions(params string[] location)
        {
            return GetOptions(_abilityGroups, location);
        }

        private List<string> GetOptions(Dictionary<string, object> groups, params string[] location)
        {
            if (groups == null) { return null; }

            if (location == null || location.Length == 0) {
                return new List<string>(groups.Keys);
            }

            string key = location[0];
            if (groups.ContainsKey(key)) {
                string[] remainingLocation = new string[location.Length - 1];
                Array.Copy(location, 1, remainingLocation, 0, remainingLocation.Length);
                return GetOptions(groups[key] as Dictionary<string, object>, remainingLocation);
            }

            return null;
        }
    }
}