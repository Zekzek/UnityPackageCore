using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Zekzek.Combat
{
    public class MenuTextEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textField;
        [SerializeField] private GameObject _highlight;


        public int PreferredWidth;
        public int PreferredHeight;
        private bool _selected;

        private void Start()
        {
            LayoutElement layout = GetComponent<LayoutElement>();
            layout.preferredWidth = PreferredWidth;
            layout.preferredHeight = PreferredHeight;
        }

        public void Set(string entry)
        {
            _textField.text = entry;
            Select(false);
        }

        public void Select(bool value)
        {
            _selected = value;

            _textField.color = _selected ? new Color(0.7f, 1, 1) : Color.white;
            _highlight.SetActive(_selected);
        }
    }
}