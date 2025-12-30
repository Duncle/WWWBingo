using UnityEngine;

public class BingoSizeCalculator : MonoBehaviour
{
    [SerializeField] private RectTransform bingoParentRect;
    [SerializeField] private Canvas canvas;

    void Start()
    {
        float pixelsPerUnit = 100f;
        float scaleFactor = canvas.scaleFactor;
        
        float sizeInUnitsX = bingoParentRect.sizeDelta.x / scaleFactor / pixelsPerUnit;
        float sizeInUnitsY = bingoParentRect.sizeDelta.y / scaleFactor / pixelsPerUnit;

        Debug.Log($"Bingo size in Unity units: Width = {sizeInUnitsX}, Height = {sizeInUnitsY}");
    }
}
