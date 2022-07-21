using System.Collections.Generic;

namespace Zekzek.Stats
{
    public class StatBlock
    {
        private readonly IDictionary<StatType, float> _amounts = new Dictionary<StatType, float>();
        private readonly IDictionary<StatType, float> _multipliers = new Dictionary<StatType, float>();
        private ISet<SlotType> _openSlots = new HashSet<SlotType>();
        private readonly Dictionary<SlotType, StatBlock> _equipment = new Dictionary<SlotType, StatBlock>();
        private readonly List<StatBlock> _buffs = new List<StatBlock>();
        private readonly List<StatBlock> _debuffs = new List<StatBlock>();

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
    }
}