using System;
using System.Collections.Generic;
using UnityEngine;
using Zekzek.Ability;

public class MenuTextColumn : MonoBehaviour
{
    private static MenuTextColumn _textColumnPrefab;
    [SerializeField] private MenuTextEntry _textEntryPrefab;
    [SerializeField] private Transform _childContainer;
    [SerializeField] private Transform _contentContainer;

    private AbilityComponent _abilityComponent;
    private string[] _location;
    private List<string> _options;
    private readonly List<MenuTextEntry> _containedEntries = new List<MenuTextEntry>();
    private MenuTextColumn _child;
    private int _selected;

    public static void InitPrefab(MenuTextColumn columnPrefab)
    {
        _textColumnPrefab = columnPrefab;
    }

    private void OnEnable()
    {
        Select(0);
    }

    public void Set(AbilityComponent component, string[] location)
    {
        _abilityComponent = component;
        _location = location ?? new string[0];
        _options = component.GetOptions(location);
        if (_options == null) {
            Collapse();
            gameObject.SetActive(false);
        } else {
            Collapse();
            Fill(_options);
            Select(0);
        }
    }

    private void Fill(List<string> options)
    {
        while (options.Count > _containedEntries.Count) {
            _containedEntries.Add(Instantiate(_textEntryPrefab, _contentContainer));
        }

        foreach (MenuTextEntry entry in _containedEntries) {
            entry.gameObject.SetActive(false);
        }

        int count = 0;
        foreach (string option in options) {
            _containedEntries[count].Set(option);
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
        } else if (_options != null && _selected < _options.Count) {
            Expand(_options[_selected]);
        }
    }

    public void HandleCollapse()
    {
        if (IsChildActive() && _child.IsChildActive()) {
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

    private void Expand(string option)
    {
        if (_child == null) { 
            _child = Instantiate(_textColumnPrefab, _childContainer);
            RectTransform childTransform = _child.transform as RectTransform;
            childTransform.offsetMin = new Vector2(_textEntryPrefab.PreferredWidth, childTransform.offsetMin.y);
        }

        string[] childLocation = new string[_location.Length + 1];
        Array.Copy(_location, childLocation, _location.Length);
        childLocation[_location.Length] = option;

        _child.Set(_abilityComponent, childLocation);
        _child.gameObject.SetActive(true);
        _child.Select(0);
    }

    public void Collapse() {
        if (_child != null) {
            _child.Collapse();
            _child.gameObject.SetActive(false); 
        }
    }

    private bool IsChildActive()
    {
        return _child != null && _child.gameObject.activeSelf;
    }
}