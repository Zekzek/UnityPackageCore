using System;
using System.Collections.Generic;
using Zekzek.HexWorld;

namespace Zekzek.Combat
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
            Add(abilityData.Id, _abilityGroups, abilityData.Location.Split('/'));
            _knownAbilities.Add(abilityData);
        }

        private void Add(string abilityId, Dictionary<string, object> group, params string[] location)
        {
            if (location == null || location.Length == 0) {
                group.Add(abilityId, null);
                return;
            }

            string key = location[0];
            if (!group.ContainsKey(key)) {
                group.Add(key, new Dictionary<string, object>());
            }

            string[] remainingLocation = new string[location.Length - 1];
            Array.Copy(location, 1, remainingLocation, 0, remainingLocation.Length);
            Add(abilityId, group[key] as Dictionary<string, object>, remainingLocation);
        }

        public List<string> GetOptions(string[] parentLocation, string childLocation)
        {
            string[] location = new string[parentLocation.Length + 1];
            Array.Copy(parentLocation, location, parentLocation.Length);
            location[location.Length - 1] = childLocation;
            return GetOptions(location);
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

        public AbilityData GetDataById(string abilityId)
        {
            foreach (AbilityData data in _knownAbilities) {
                if (abilityId.Equals(data.Id)) {
                    return data;
                }
            }
            return null;
        }

        private AbilityData GetAt(Dictionary<string, object> groups, params string[] location)
        {
            if (groups == null || location == null || location.Length == 0) { return null; }

            if (location.Length == 1) {
                return JsonContent.ContentUtil.LoadData<AbilityData>(location[0]);
            }

            string key = location[0];
            if (groups.ContainsKey(key)) {
                string[] remainingLocation = new string[location.Length - 1];
                Array.Copy(location, 1, remainingLocation, 0, remainingLocation.Length);
                return GetAt(groups[key] as Dictionary<string, object>, remainingLocation);
            }

            return null;
        }
    }
}