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
    public void ToggleCheckmark()
    {
        cellCheckmark.gameObject.SetActive(_isCheckmarkActive);
        _isCheckmarkActive = !_isCheckmarkActive;
    }
}