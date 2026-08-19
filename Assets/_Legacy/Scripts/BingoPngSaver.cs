using System.IO;
using UnityEngine;

public class BingoPngSaver : MonoBehaviour
{
    [SerializeField] private Camera renderCamera; // Camera that rendering only taget object
    [SerializeField] private GameObject targetObject; // Object to save
    [SerializeField] private int imageWidth = 512; // PNG width
    [SerializeField] private int imageHeight = 512; // PNG height

    public void SaveAsPNG()
    {
        // Create RenderTexture
        RenderTexture renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        renderCamera.targetTexture = renderTexture;

        // Render scene
        RenderTexture.active = renderTexture;
        renderCamera.Render();

        // Create texture for the saving
        Texture2D texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        texture.Apply();

        // Clear RenderTexture
        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Convert texture into PNG
        byte[] bytes = texture.EncodeToPNG();

        // Free texture from memory
        Destroy(texture);

        // Save PNG file
        string filePath = Path.Combine(Application.persistentDataPath, "SavedObject.png");
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("PNG has been saved: " + filePath);
    }
}