using System.Collections.Generic;
using UnityEngine;

public class MenuTextColumn : MonoBehaviour
{
    private MenuTextColumn _textColumnPrefab;
    [SerializeField] private MenuTextEntry _textEntryPrefab;
    [SerializeField] private Transform _childContainer;
    [SerializeField] private Transform _contentContainer;

    public uint Id { get; private set; }

    private readonly List<MenuTextEntry> _containedEntries = new List<MenuTextEntry>();
    private MenuTextColumn _child;

    public void Set(uint id, MenuTextColumn columnPrefab = null)
    {
        if (columnPrefab != null) { _textColumnPrefab = columnPrefab; }
        if (id == Id) { return; }

        Id = id;
        Collapse();
        FillById(id);
    }

    private void FillById(uint id)
    {
        //TODO: fill this for real
        Dictionary<uint, string> entries = new Dictionary<uint, string> {
            { 1, "waffle" },
            { 2, "burrito" },
            { 3, "taco" }
        };

        while(entries.Count > _containedEntries.Count) {
            _containedEntries.Add(Instantiate(_textEntryPrefab, _contentContainer));
        }

        foreach (MenuTextEntry entry in _containedEntries) {
            entry.gameObject.SetActive(false);
        }

        int count = 0;
        foreach (KeyValuePair<uint, string> pair in entries) {
            _containedEntries[count].Set(pair.Key, pair.Value, this);
            _containedEntries[count].gameObject.SetActive(true);
            count++;
        }
    }

    public void Expand(uint id)
    {
        if (_child == null) { 
            _child = Instantiate(_textColumnPrefab, _childContainer);
            RectTransform childTransform = _child.transform as RectTransform;
            childTransform.offsetMin = new Vector2(_textEntryPrefab.PreferredWidth, childTransform.offsetMin.y);
        }

        _child.Set(id);
        _child.gameObject.SetActive(true);
    }

    public void Collapse() { if (_child != null) { _child?.gameObject.SetActive(false); } }
}