using System.Collections.Generic;

namespace Zekzek.Stats
{
    public class StatBlock
    {
        private IDictionary<StatType, float> _amounts;
        private IDictionary<StatType, float> _multipliers;
        private ISet<SlotType> _openSlots;
        private readonly Dictionary<SlotType, StatBlock> _blocks = new Dictionary<SlotType, StatBlock>();

        public StatBlock(IDictionary<StatType, float> amounts, IDictionary<StatType, float> multipliers, ISet<SlotType> slots)
        {
            _amounts = amounts;
            _multipliers = multipliers;
            _openSlots = slots;
        }

        public float GetAccumulatedAmount(StatType statType)
        {
            float amount = _amounts != null && _amounts.ContainsKey(statType) ? _amounts[statType] : 0;
            foreach (StatBlock block in _blocks.Values) {
                amount += block.GetAccumulatedAmount(statType);
            }
            return amount;
        }

        public float GetAccumulatedMultiplier(StatType statType)
        {
            float multiplier = _multipliers != null && _multipliers.ContainsKey(statType) ? _multipliers[statType] : 0;
            foreach (StatBlock block in _blocks.Values) {
                multiplier += block.GetAccumulatedMultiplier(statType);
            }
            return multiplier;
        }

        public float GetTotalValue(StatType statType)
        {
            return GetAccumulatedAmount(statType) * (1 + GetAccumulatedMultiplier(statType));
        }

        public bool Equip(SlotType slot, StatBlock statBlock)
        {
            if (_openSlots.Contains(slot)) {
                _openSlots.Remove(slot);
                _blocks.Add(slot, statBlock);
                return true;
            }
            return false;
        }

        public bool Unequip(SlotType slot)
        {
            if (_blocks.ContainsKey(slot)) {
                _blocks.Remove(slot);
                _openSlots.Add(slot);
                return true;
            }
            return false;
        }
    }
}