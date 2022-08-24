using System;
using UnityEngine;

namespace Zekzek.Ability
{
    [Serializable]
    public class AbilityModifierData
    {
        // Lightning >> Water
        // Lightning << Earth
        // Fire >> Ice
        // Water >> Fire
        // Life << Death
        // Death << Life
        // Earth + Water = Life?
        [Flags]
        public enum Elements
        {
            None = 0,

            Fire = 1 << 0,      // Red
            Ice = 1 << 1,       // Green
            Lightning = 1 << 2, // Blue
            Earth = 1 << 3,     // Green & Blue
            Wind = 1 << 4,      // Blue & Red
            Water = 1 << 5,     // Red & Green

            Life = 1 << 6,      // Blue
            Death = 1 << 7,     // Blue

            Slash = 1 << 8,     // Red & Green
            Pierce = 1 << 9,     // Green
            Crush = 1 << 10,    // Red
        }

        // Reach: Ice bolt firing
        // Range: Lightning created at a far away tile
        // Spread: Fire exploding outward

        // Reach + Range: Earth erupts from a tile and skewers in a direction
        // Reach + Spread: Water wave surges forward
        // Range + Spread: Wind coallesces in a larger area

        [SerializeField] private string id;
        [SerializeField] private string name;

        // Affinity
        [SerializeField] private float redAffinity; // strength & power & crush
        [SerializeField] private float greenAffinity; // dexterity & speed & pierce
        [SerializeField] private float blueAffinity; // intelligence & magic & slash

        // Core (From Weapons)
        [SerializeField] private float speed; // inverse multiplier on delay between attacks (two-handers are slow)
        [SerializeField] private float power; // multiplier on damage for atacks, ability to break defenses, etc (two-handers are powerful)
        [SerializeField] private float potency; // multiplier on additional effects
        [SerializeField] private string[] damageTypes; // what damage type the attack can deal

        // Location 
        [SerializeField] private int spread; // 1 for forward, 6 for burst
        [SerializeField] private int reach; // how many hexes each line of the attack can effect (spear)
        [SerializeField] private int range; // how far away the unit the attack can be started (bow, wand, staff)

        // Additional Effect
        [SerializeField] private float paralyzeChance; // chance to paralyze an enmey on hit
        [SerializeField] private float bleedChance; // chance to bleed an enmey on hit (while bleeding, moving causes damage)
        [SerializeField] private float pushChance; // chance to push a target away
        [SerializeField] private float pullChance; // chance to pull a target closer

        // Fundamental Change to Ability 
        [SerializeField] private float retaliate; // chance to retaliate when struck (spiked shield, hand-to-hand)
        [SerializeField] private int summonDuration; // when greater than 0, ability instead summons an entity to perform the action

        public string Id => id;
        public string Name => name;
        public int Spread => spread;
        public int Reach => reach;
        public int Range => range;

        private Elements DamageType {
            get {
                Elements combined = Elements.None;
                foreach (string damageType in damageTypes) {
                    if (Enum.TryParse(damageType, out Elements element)) {
                        combined |= element;
                    }
                }
                return combined;
            }
        }
    }
}