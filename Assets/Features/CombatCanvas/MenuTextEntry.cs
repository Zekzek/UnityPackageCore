using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuTextEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textField;
    public int PreferredWidth;
    public int PreferredHeight;

    public uint Id { get; private set; }

    private MenuTextColumn _column;

    private void Start()
    {
        LayoutElement layout = GetComponent<LayoutElement>();
        layout.preferredWidth = PreferredWidth;
        layout.preferredHeight = PreferredHeight;
    }

    public void Set(uint id, string entry, MenuTextColumn column)
    {
        Id = id;
        _textField.text = entry;
        _column = column;
    }

    public void Expand() { if (_column != null) { _column.Expand(Id); } }
    public void Collapse() { if (_column != null) { _column.Collapse(); } }
}
