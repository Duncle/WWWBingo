using UnityEngine;

public class BingoSizeCalculator : MonoBehaviour
{
    [SerializeField] private RectTransform bingoParentRect; // Ссылка на RectTransform бинго
    [SerializeField] private Canvas canvas;                // Ссылка на Canvas

    void Start()
    {
        float pixelsPerUnit = 100f; // Обычно 100 по умолчанию
        float scaleFactor = canvas.scaleFactor;

        // Размеры в Unity-единицах
        float sizeInUnitsX = bingoParentRect.sizeDelta.x / scaleFactor / pixelsPerUnit;
        float sizeInUnitsY = bingoParentRect.sizeDelta.y / scaleFactor / pixelsPerUnit;

        Debug.Log($"Bingo size in Unity units: Width = {sizeInUnitsX}, Height = {sizeInUnitsY}");
    }
}
