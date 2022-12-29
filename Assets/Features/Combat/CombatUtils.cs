using System;
using System.Collections.Generic;

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

        private static Dictionary<Elements[], float> affinity = new Dictionary<Elements[], float> {
            { new Elements[]{ Elements.Fire, Elements.Ice }, 2f }
        };

        private static float GetAffinityMultiplier(Elements abilityElement, Elements targetElement)
        {
            float multiplier = 1;
            int count = 0;

            Elements[] elementTypes = Enum.GetValues(typeof(Elements)) as Elements[];
            foreach (Elements elementType in elementTypes) {
                foreach (Elements elementType2 in elementTypes) {
                    if (abilityElement.HasFlag(elementType) && targetElement.HasFlag(elementType2)) {
                        var key = new Elements[] { elementType, elementType2 };
                        multiplier += affinity.ContainsKey(key) ? affinity[key] : 0;
                        count++;
                    }
                }
            }

            return count == 0 ? 1 : 1 + (multiplier - 1) / count;
        }
    }
}