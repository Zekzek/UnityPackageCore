using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuTextEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textField;
    [SerializeField] private GameObject _highlight;


    public int PreferredWidth;
    public int PreferredHeight;
    private bool _selected;

    public int Id { get; private set; }

    private MenuTextColumn _column;

    private void Start()
    {
        LayoutElement layout = GetComponent<LayoutElement>();
        layout.preferredWidth = PreferredWidth;
        layout.preferredHeight = PreferredHeight;
    }

    public void Set(int id, string entry, MenuTextColumn column)
    {
        Id = id;
        _textField.text = entry;
        _column = column;
        Select(false);
    }

    public void Select(bool value)
    {
        _selected = value;

        _textField.color = _selected ? new Color(0.7f, 1, 1) : Color.white;
        _highlight.SetActive(_selected);
    }
}
