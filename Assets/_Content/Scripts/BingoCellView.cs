using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BingoCellView : MonoBehaviour
{
    [SerializeField] private TMP_Text cellText;
    [SerializeField] private Image cellCheckmark;
    
    private bool _isCheckmarkActive;
    
    public TMP_Text CellText => cellText;
    public Image CellCheckmark => cellCheckmark;
    
    public void RenderText(string text) => cellText.text = text;
    public void SetMarked(bool marked)
    {
        _isCheckmarkActive = marked;
        cellCheckmark.gameObject.SetActive(marked);
    }

    public void ToggleCheckmark() => SetMarked(!_isCheckmarkActive);
}
