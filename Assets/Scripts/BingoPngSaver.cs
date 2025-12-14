using System.IO;
using UnityEngine;

public class BingoPngSaver : MonoBehaviour
{
    [SerializeField] private Camera renderCamera; // Камера, которая рендерит только нужный объект
    [SerializeField] private GameObject targetObject; // Объект, который нужно сохранить
    [SerializeField] private int imageWidth = 512; // Ширина PNG
    [SerializeField] private int imageHeight = 512; // Высота PNG

    public void SaveAsPNG()
    {
        // Создаем RenderTexture
        RenderTexture renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        renderCamera.targetTexture = renderTexture;

        // Рендерим сцену
        RenderTexture.active = renderTexture;
        renderCamera.Render();

        // Создаем текстуру для сохранения
        Texture2D texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        texture.Apply();

        // Очищаем RenderTexture
        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Конвертируем текстуру в PNG
        byte[] bytes = texture.EncodeToPNG();

        // Освобождаем текстуру из памяти
        Destroy(texture);

        // Сохраняем PNG файл
        string filePath = Path.Combine(Application.persistentDataPath, "SavedObject.png");
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("Сохранено PNG: " + filePath);
    }
}