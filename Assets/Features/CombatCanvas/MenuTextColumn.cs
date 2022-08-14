using System.Collections.Generic;
using UnityEngine;

public class MenuTextColumn : MonoBehaviour
{
    private static MenuTextColumn _textColumnPrefab;
    [SerializeField] private MenuTextEntry _textEntryPrefab;
    [SerializeField] private Transform _childContainer;
    [SerializeField] private Transform _contentContainer;

    public int Id { get; private set; }

    private readonly List<MenuTextEntry> _containedEntries = new List<MenuTextEntry>();
    private MenuTextColumn _child;
    private int _selected;

    private void Start()
    {
        Set(0);
        Select(0);
    }

    public static void Init(MenuTextColumn columnPrefab)
    {
        _textColumnPrefab = columnPrefab;
    }

    public void Set(int id)
    {
        //if (id == Id) { return; }

        Id = id;
        Collapse();
        FillById(id);
    }

    private void FillById(int id)
    {
        //TODO: fill this for real
        Dictionary<int, string> entries = new Dictionary<int, string> {
            { 1, "waffle" },
            { 2, "burrito" },
            { 3, "taco" }
        };

        while (entries.Count > _containedEntries.Count) {
            _containedEntries.Add(Instantiate(_textEntryPrefab, _contentContainer));
        }

        foreach (MenuTextEntry entry in _containedEntries) {
            entry.gameObject.SetActive(false);
        }

        int count = 0;
        foreach (KeyValuePair<int, string> pair in entries) {
            _containedEntries[count].Set(pair.Key, pair.Value, this);
            _containedEntries[count].gameObject.SetActive(true);
            count++;
        }
    }

    public void HandleUp()
    {
        if (IsChildActive()) { 
            _child.HandleUp(); 
        } else if (_containedEntries.Count > 0) {
            Select((_containedEntries.Count + _selected - 1) % _containedEntries.Count);
        }
    }

    public void HandleDown()
    {
        if (IsChildActive()) {
            _child.HandleDown();
        } else if (_containedEntries.Count > 0) {
            Select((_selected + 1) % _containedEntries.Count);
        }
    }

    public void HandleExpand()
    {
        if (IsChildActive()) {
            _child.HandleExpand();
        } else {
            Expand(_selected);
        }
    }

    public void HandleCollapse()
    {
        if (IsChildActive()) {
            _child.HandleCollapse();
        } else {
            Collapse();
        }
    }

    private void Select(int index)
    {
        if (_selected < _containedEntries.Count) {
            _containedEntries[_selected].Select(false);
        }
        if (index < _containedEntries.Count) {
            _containedEntries[index].Select(true);
            _selected = index;
        }
    }

    private void Expand(int id)
    {
        if (_child == null) { 
            _child = Instantiate(_textColumnPrefab, _childContainer);
            RectTransform childTransform = _child.transform as RectTransform;
            childTransform.offsetMin = new Vector2(_textEntryPrefab.PreferredWidth, childTransform.offsetMin.y);
        }

        _child.Set(id);
        _child.gameObject.SetActive(true);
    }

    public void Collapse() {
        if (_child != null) { _child.gameObject.SetActive(false); } 
    }

    private bool IsChildActive()
    {
        return _child != null && _child.gameObject.activeSelf;
    }
}