using System.Collections.Generic;
using UnityEngine;
using Zekzek.HexWorld;

public class RelationshipComponent : WorldObjectComponent
{
    public override WorldComponentType ComponentType => WorldComponentType.Relationship;

    private readonly IDictionary<RelationshipType, IDictionary<uint, float>> _valueByTypeAndId = new Dictionary<RelationshipType, IDictionary<uint, float>>();
    private readonly IDictionary<RelationshipType, IDictionary<uint, float>> _affinityByTypeAndId = new Dictionary<RelationshipType, IDictionary<uint, float>>();

    public RelationshipComponent(uint worldObjectId) : base(worldObjectId) { }

    public void Add(RelationshipType type, uint targetId, float value)
    {
        if (!_valueByTypeAndId.ContainsKey(type)) { _valueByTypeAndId.Add(type, new Dictionary<uint, float>()); }
        if (!_valueByTypeAndId[type].ContainsKey(targetId)) { _valueByTypeAndId[type].Add(targetId, 0.5f * GetAffinity(type, targetId)); }
        _valueByTypeAndId[type][targetId] = Mathf.Clamp(Get(type, targetId) + ScaleChangeByAffinity(type, targetId, value), -1f, 1f);
    }

    public void AddDefaultAffinity(RelationshipType type, float value) { AddAffinity(type, 0, value); }

    public void AddAffinity(RelationshipType type, uint targetId, float value)
    {
        if (!_affinityByTypeAndId.ContainsKey(type)) { _affinityByTypeAndId.Add(type, new Dictionary<uint, float>()); }
        if (!_affinityByTypeAndId[type].ContainsKey(targetId)) { _affinityByTypeAndId[type].Add(targetId, 0); }
        _affinityByTypeAndId[type][targetId] = Mathf.Clamp(GetAffinity(type, targetId) + value, -1f, 1f);
    }

    public float Get(RelationshipType type, uint targetId)
    {
        if (!_valueByTypeAndId.ContainsKey(type)) { return 0; }
        if (!_valueByTypeAndId[type].ContainsKey(targetId)) { return targetId == 0 ? 0 : Get(type, 0); } 
        return _valueByTypeAndId[type][targetId];
    }

    public float GetAffinity(RelationshipType type, uint targetId)
    {
        if (!_affinityByTypeAndId.ContainsKey(type)) { return 0; }
        if (!_affinityByTypeAndId[type].ContainsKey(targetId)) { return targetId == 0 ? 0 : GetAffinity(type, 0); }
        return _affinityByTypeAndId[type][targetId];
    }

    private float ScaleChangeByAffinity(RelationshipType type, uint targetId, float value)
    {
        float multiplier = GetAffinity(type, targetId) * 0.5f;
        return value * (value > 0 ? 1 + multiplier : 1 - multiplier);
    }
}