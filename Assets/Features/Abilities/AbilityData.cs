using System;
using UnityEngine;

namespace Zekzek.Ability
{
    [Serializable]
    public class AbilityData: AbilityModifierData
    {
        [SerializeField] private string location;

        public string Location => location;
    }
}