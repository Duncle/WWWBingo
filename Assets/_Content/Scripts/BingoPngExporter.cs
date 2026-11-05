using System;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public static class BingoPngExporter
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern int BingoDownloadPng(byte[] bytes, int length, string filename);
#endif

    // Render a separate complete card, independent of screen dimensions and browser scaling.
    public static byte[] Render(BingoModel model, TMP_FontAsset regular, TMP_FontAsset bold)
    {
        const int width = 1080, height = 1536;
        var root = new GameObject("PNG card");
        var texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D pixels = null;
        RenderTexture previous = RenderTexture.active;
        try
        {
            var camera = root.AddComponent<Camera>();
            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BingoStyle.Paper;
            camera.orthographic = true;
            camera.orthographicSize = height / 2f;
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 10;
            camera.cullingMask = 1 << 31;
            camera.targetTexture = texture;
            var canvas = new GameObject("Card canvas", typeof(RectTransform), typeof(Canvas)).GetComponent<Canvas>();
            canvas.transform.SetParent(root.transform, false);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
            var page = BingoStyle.Rect(canvas.transform, "Card", 0, 0, width, height);
            page.anchorMin = page.anchorMax = new Vector2(.5f, .5f);
            page.pivot = new Vector2(.5f, .5f);
            page.anchoredPosition = Vector2.zero;
            page.gameObject.AddComponent<BingoView>().Build(model, regular, bold, false, true);
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 31;
            Canvas.ForceUpdateCanvases();
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true)) text.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = texture;
            pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            pixels.Apply();
            return pixels.EncodeToPNG();
        }
        finally
        {
            RenderTexture.active = previous;
            var camera = root.GetComponent<Camera>();
            if (camera != null) camera.targetTexture = null;
            root.SetActive(false);
            Dispose(root);
            if (pixels != null) Dispose(pixels);
            texture.Release();
            Dispose(texture);
        }
    }

    public static string Save(byte[] bytes, BingoSession session)
    {
        string filename = "chgk-bingo-" + session.number + "-" + session.size + "x" + session.size + ".png";
#if UNITY_WEBGL && !UNITY_EDITOR
        if (BingoDownloadPng(bytes, bytes.Length, filename) == 0) throw new IOException("Browser download failed.");
        return filename;
#elif UNITY_EDITOR
        string path = UnityEditor.EditorUtility.SaveFilePanel("Сохранить бинго", "", filename, "png");
        if (string.IsNullOrEmpty(path)) return null;
        File.WriteAllBytes(path, bytes);
        return path;
#else
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, bytes);
        Application.OpenURL(new Uri(Path.GetDirectoryName(path)).AbsoluteUri);
        return path;
#endif
    }

    private static void Dispose(UnityEngine.Object value)
    {
        if (Application.isPlaying) UnityEngine.Object.Destroy(value);
        else UnityEngine.Object.DestroyImmediate(value);
    }
}
