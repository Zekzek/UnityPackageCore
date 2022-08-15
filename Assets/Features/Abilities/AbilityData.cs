using System;
using UnityEngine;

namespace Zekzek.Ability
{
    [Serializable]
    public class AbilityData
    {
        [SerializeField] private string id;
        [SerializeField] private string name;
        [SerializeField] private string location;

        public string Name => name;
        public string Location => location;
    }
}