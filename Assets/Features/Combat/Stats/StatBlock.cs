using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.Combat
{
    public class StatBlock
    {
        private readonly IDictionary<StatType, float> _amounts = new Dictionary<StatType, float>();
        private readonly IDictionary<StatType, float> _multipliers = new Dictionary<StatType, float>();
        private readonly IDictionary<StatType, float> _missing = new Dictionary<StatType, float>();
        private readonly ISet<SlotType> _openSlots = new HashSet<SlotType>();
        private readonly Dictionary<SlotType, StatBlock> _equipment = new Dictionary<SlotType, StatBlock>();
        private readonly List<StatBlock> _buffs = new List<StatBlock>();
        private readonly List<StatBlock> _debuffs = new List<StatBlock>();

        //TODO: cache totals? seems like its being recalculated frequently
        public void AddAmount(StatType statType, float amount)
        {
            if (_amounts.ContainsKey(statType)) {
                _amounts[statType] += amount;
            } else {
                _amounts.Add(statType, amount);
            }
        }

        public void AddMultiplier(StatType statType, float amount)
        {
            if (_multipliers.ContainsKey(statType)) {
                _multipliers[statType] += amount;
            } else {
                _multipliers.Add(statType, 1 + amount);
            }
        }

        public void AddSlot(SlotType slotType) { _openSlots.Add(slotType); }
        
        public void AddDelta(StatType statType, float amount)
        {
            if (!_missing.ContainsKey(statType)) { _missing.Add(statType, 0); }
            float total = GetTotalValue(statType);
            _missing[statType] = Mathf.Clamp(GetMissing(statType) - amount, 0, total);
        }
        
        public void RemoveSlot(SlotType slotType) { _openSlots.Remove(slotType); }

        public bool Equip(SlotType slot, StatBlock statBlock)
        {
            if (_openSlots.Contains(slot)) {
                _openSlots.Remove(slot);
                _equipment.Add(slot, statBlock);
                return true;
            }
            return false;
        }

        public bool Unequip(SlotType slot)
        {
            if (_equipment.ContainsKey(slot)) {
                _equipment.Remove(slot);
                _openSlots.Add(slot);
                return true;
            }
            return false;
        }

        //TODO: need a better way to track and remove buffs/debuffs?
        public void AddBuff(StatBlock statBlock) { _buffs.Add(statBlock); }
        public void RemoveBuff(StatBlock statBlock) { _buffs.Remove(statBlock); }

        public void AddDebuff(StatBlock statBlock) { _debuffs.Add(statBlock); }
        public void RemoveDebuff(StatBlock statBlock) { _debuffs.Remove(statBlock); }

        public float GetTotalValue(StatType statType)
        {
            float amount = GetAmount(statType) * GetMultiplier(statType);
            foreach (StatBlock block in _equipment.Values) {
                amount += block.GetTotalValue(statType) * GetMultiplier(statType);
            }
            return amount;
        }

        public float GetAmount(StatType statType)
        {
            return _amounts != null && _amounts.ContainsKey(statType) ? _amounts[statType] : 0;
        }

        public float GetMultiplier(StatType statType)
        {
            return _multipliers != null && _multipliers.ContainsKey(statType) ? _multipliers[statType] : 1;
        }

        public float GetCurrent(StatType statType)
        {
            return GetTotalValue(statType) - GetMissing(statType);
        }

        public float GetPercent(StatType statType)
        {
            float missing = GetMissing(statType);
            if (missing == 0) { return 1; }
            float total = GetTotalValue(statType);
            return (total - missing) / total;
        }

        private float GetMissing(StatType statType)
        {
            return _missing != null && _missing.ContainsKey(statType) ? _missing[statType] : 0;
        }
    }
}