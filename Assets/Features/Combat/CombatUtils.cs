using System;

namespace Zekzek.Combat
{
    public static class CombatUtils
    {
        public static bool TryGetResult(StatBlock source, AbilityData ability, StatBlock target, out StatChange sourceChange, out StatChange targetChange)
        {
            sourceChange = new StatChange();
            targetChange = new StatChange();
            targetChange.AddStatChange(StatType.Health, -10);
            return true;
        }

        public static void Apply(StatChange userChange, StatComponent stats)
        {
            foreach (System.Collections.Generic.KeyValuePair<StatType, float> pair in userChange._changed) {
                stats.StatBlock.AddDelta(pair.Key, pair.Value);
            }
            //TODO: handle buffs/debuffs
        }
    }
}