using System.Collections.Generic;

namespace Zekzek.Combat
{
    public class StatChange
    {
        public IDictionary<StatType, float> _changed { get; private set; } = new Dictionary<StatType, float>();
        public List<StatBlock> _gainedBuffs { get; private set; } = new List<StatBlock>();
        public List<StatBlock> _gainedDebuffs { get; private set; } = new List<StatBlock>();
        public List<StatBlock> _lostBuffs { get; private set; } = new List<StatBlock>();
        public List<StatBlock> _lostDebuffs { get; private set; } = new List<StatBlock>();

        public void AddStatChange(StatType type, float amount)
        {
            if (!_changed.ContainsKey(type)) { 
                _changed.Add(type, amount); 
            } else {
                _changed[type] += amount;
            }
        }

        public void AddGainedBuff(StatBlock buff)
        {
            _gainedBuffs.Add(buff);
        }

        public void AddGainedDebuff(StatBlock debuff)
        {
            _gainedDebuffs.Add(debuff);
        }
        public void AddLostBuff(StatBlock buff)
        {
            _lostBuffs.Add(buff);
        }

        public void AddLostDebuff(StatBlock debuff)
        {
            _lostDebuffs.Add(debuff);
        }
    }
}