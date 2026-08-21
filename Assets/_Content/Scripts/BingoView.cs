using UnityEngine;

public class BingoView : MonoBehaviour
{
    [Header("3X3")]
    [SerializeField] private GameObject grid3X3;
    [SerializeField] private BingoCellView[] cells3X3;
    
    [Header("5X5")]
    [SerializeField] private GameObject grid5X5;
    [SerializeField] private BingoCellView[] cells5X5;
}
