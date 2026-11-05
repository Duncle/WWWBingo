using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

// Repeatable checks use their own preference keys and never touch a player's saved games.
public static class BingoVerification
{
    private const string Output = "Logs/BingoVerification";
    private const string Prefix = "chgk.bingo.verification.";
    [MenuItem("Bingo/Verify logic and render previews")]
    public static void Verify()
    {
        Directory.CreateDirectory(Output);
        TMP_FontAsset regular = null, bold = null;
        var checks = new List<string>();
        try
        {
            BingoPreset[] presets = AssetDatabase.FindAssets("t:BingoPreset", new[] { "Assets/_Content/Data" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<BingoPreset>(AssetDatabase.GUIDToAssetPath(guid)))
                .OrderBy(p => p.BingoType).ToArray();
            Require(presets.Length == 3, "Three game modes");
            foreach (var preset in presets)
            {
                for (int seed = 0; seed < 100; seed++)
                {
                    var model = BingoModel.Generate(preset, seed + 1, new System.Random(seed));
                    Require(model.Session.phrases.Distinct().Count() == model.Session.phrases.Length, "Unique phrases");
                    Require(model.Session.phrases[model.Session.phrases.Length / 2] == preset.CenterText, "Fixed central phrase");
                    Require(model.MarkedCount == 0 && model.CompletedLines == 0, "Fresh board is unmarked");
                    var groups = preset.BingoPhrases.Where(p => model.Session.phrases.Contains(p.Text))
                        .Where(p => p.Category != BingoCategory.Uncategorized).GroupBy(p => p.Category);
                    Require(groups.All(group => group.Count() == 1), "Exclusive categories");
                }
                checks.Add(preset.BingoType + ": 100 random boards, unique phrases and categories, central cell");
            }
            var board = BingoModel.Generate(presets[0], 1, new System.Random(10));
            for (int i = 0; i < 3; i++) board.Toggle(i);
            Require(board.CompletedLines == 1, "Row bingo");
            board.Toggle(3); board.Toggle(6);
            Require(board.CompletedLines == 2, "Column bingo");
            board.Toggle(4); board.Toggle(8);
            Require(board.CompletedLines == 4, "Both diagonal bingos");
            board.Toggle(0);
            Require(board.CompletedLines == 1, "Unmark removes only affected completed lines");
            for (int i = 0; i < board.Session.marked.Length; i++) board.Session.marked[i] = true;
            Require(board.CompletedLines == 8, "Full 3×3 board has 8 lines");
            var year = BingoModel.Generate(presets[2], 2);
            for (int i = 0; i < year.Session.marked.Length; i++) year.Session.marked[i] = true;
            Require(year.CompletedLines == 12, "Full 5×5 board has 12 lines");
            checks.Add("Row, column, diagonal, unmark and full-board line counts");

            CleanupPreferences();
            var store = new BingoSessionStore(Prefix);
            var presenter = new BingoPresenter(store);
            presenter.Open(presets[0]);
            presenter.Toggle(0); presenter.Toggle(4);
            string saved = JsonUtility.ToJson(presenter.Model.Session);
            var restarted = new BingoPresenter(new BingoSessionStore(Prefix));
            restarted.Open(presets[0]);
            Require(JsonUtility.ToJson(restarted.Model.Session) == saved, "Reopen/restart preserves board and marks");
            restarted.Open(presets[2]);
            Require(restarted.Model.Session.number == 2, "Persistent game numbering");
            restarted.Finish();
            Require(store.Load(BingoType.YearFinal) == null && store.Load(BingoType.RegularGame) != null, "Finish clears only selected mode");
            restarted.Open(presets[2]);
            Require(restarted.Model.Session.number == 3 && restarted.Model.MarkedCount == 0, "Next party starts fresh");
            PlayerPrefs.SetString(Prefix + "session.1", "{broken");
            Require(store.Load(BingoType.SeriesFinal) == null, "Corrupt saves are rejected");
            Require(!BingoModel.IsValid(new BingoSession()), "Incomplete saves are rejected");
            checks.Add("Save, reopen, new presenter, mode isolation, finish, numbering and corrupt-save recovery");

            CleanupPreferences();
            var scene = EditorSceneManager.OpenPreviewScene("Assets/_Content/Scenes/BingoApp.unity");
            try
            {
                var app = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<BingoApplication>()).Single();
                Require(app.Presets.Length == 3 && app.Presets.All(preset => preset != null), "Scene has all preset references");
                app.Initialize(new BingoSessionStore(Prefix));
                Click(app, "НАЧАТЬ ИГРУ");
                Click(app, "Cell 1");
                Require(new BingoSessionStore(Prefix).Load(BingoType.RegularGame).MarkedCount == 1, "Cell UI saves mark");
                Click(app, "ВЕРНУТЬСЯ В МЕНЮ");
                Click(app, "ПРОДОЛЖИТЬ");
                Require(new BingoSessionStore(Prefix).Load(BingoType.RegularGame).MarkedCount == 1, "Menu resume preserves mark");
                Click(app, "ЗАВЕРШИТЬ ПАРТИЮ");
                Click(app, "ПРОДОЛЖИТЬ ИГРУ");
                Require(new BingoSessionStore(Prefix).Load(BingoType.RegularGame) != null, "Cancel finish preserves board");
                Click(app, "ЗАВЕРШИТЬ ПАРТИЮ");
                Click(app, "ЗАВЕРШИТЬ");
                Require(new BingoSessionStore(Prefix).Load(BingoType.RegularGame) == null, "Confirm finish clears board");
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
            checks.Add("Actual BingoApp scene references and UI buttons: start, mark, menu, continue, cancel and confirm finish");

            regular = MakeFont("PTSerif-Regular.ttf");
            bold = MakeFont("PTSerif-Bold.ttf");
            foreach (var preset in presets)
            {
                var preview = BingoModel.Generate(preset, 5, new System.Random(29));
                int[] marks = preview.Session.size == 3 ? new[] { 0, 2, 3, 4, 6 } : new[] { 0, 3, 4, 5, 6, 10, 12, 14, 16, 18, 19, 23 };
                foreach (int mark in marks) preview.Toggle(mark);
                byte[] png = BingoPngExporter.Render(preview, regular, bold);
                Require(png.Length > 10000 && png[0] == 137 && png[1] == 80, "PNG output");
                var decoded = new Texture2D(2, 2);
                try
                {
                    Require(decoded.LoadImage(png) && decoded.width == 1080 && decoded.height == 1536, "PNG dimensions");
                    int cellWidth = (984 - (preview.Session.size - 1) * 12) / preview.Session.size;
                    Color markPixel = decoded.GetPixel(48 + cellWidth - 5, 1536 - 660 - 6);
                    Require(markPixel.r > .5f && markPixel.g < .2f && markPixel.b < .2f, "Marked corner is visible in exported PNG");
                    Require(decoded.GetPixel(540, 1536 - 34).r < .2f, "Owl emblem is visible in exported PNG");
                }
                finally { UnityEngine.Object.DestroyImmediate(decoded); }
                File.WriteAllBytes(Output + "/card-" + preset.BingoType + ".png", png);
                RenderUI(Output + "/screen-" + preset.BingoType + ".png", 1080, 1784,
                    page => page.gameObject.AddComponent<BingoView>().Build(preview, regular, bold, false, false));
                RenderUI(Output + "/wide-" + preset.BingoType + ".png", 1440, 980,
                    page => page.gameObject.AddComponent<BingoView>().Build(preview, regular, bold, true, false));
            }
            RenderUI(Output + "/menu.png", 1080, 1784,
                page => page.gameObject.AddComponent<MainMenuView>().Build(presets, store, regular, bold, false, _ => { }));
            RenderUI(Output + "/menu-wide.png", 1440, 980,
                page => page.gameObject.AddComponent<MainMenuView>().Build(presets, store, regular, bold, true, _ => { }));
            checks.Add("Rendered PNG cards, portrait and landscape UI for all modes, both menus");
            File.WriteAllText(Output + "/result.txt", "PASS\n" + string.Join("\n", checks));
            Debug.Log("Bingo verification PASS. Previews: " + Output);
        }
        catch (Exception error)
        {
            File.WriteAllText(Output + "/result.txt", "FAIL\n" + error);
            Debug.LogException(error);
        }
        finally
        {
            CleanupPreferences();
            FreeFont(regular); FreeFont(bold);
        }
    }

    private static TMP_FontAsset MakeFont(string file) => TMP_FontAsset.CreateFontAsset(
        AssetDatabase.LoadAssetAtPath<Font>("Assets/_Content/Fonts/" + file), 90, 9, GlyphRenderMode.SDFAA, 2048, 2048);

    private static void Click(BingoApplication app, string name)
    {
        var button = app.GetComponentsInChildren<Button>().First(item => item.gameObject.name == name);
        button.onClick.Invoke();
    }

    private static void FreeFont(TMP_FontAsset font)
    {
        if (font == null) return;
        foreach (var atlas in font.atlasTextures) if (atlas != null) UnityEngine.Object.DestroyImmediate(atlas);
        UnityEngine.Object.DestroyImmediate(font.material);
        UnityEngine.Object.DestroyImmediate(font);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void CleanupPreferences()
    {
        for (int i = 0; i < 3; i++) PlayerPrefs.DeleteKey(Prefix + "session." + i);
        PlayerPrefs.DeleteKey(Prefix + "number");
        PlayerPrefs.Save();
    }

    private static void RenderUI(string path, int width, int height, Action<RectTransform> build)
    {
        var root = new GameObject("Bingo preview camera");
        var target = new RenderTexture(width, height, 24);
        var previous = RenderTexture.active;
        Texture2D pixels = null;
        try
        {
            var camera = root.AddComponent<Camera>();
            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BingoStyle.Paper;
            camera.orthographic = true;
            camera.orthographicSize = height / 2f;
            camera.nearClipPlane = .1f; camera.farClipPlane = 10;
            camera.cullingMask = 1 << 31;
            camera.targetTexture = target;
            var canvas = new GameObject("Preview canvas", typeof(RectTransform), typeof(Canvas)).GetComponent<Canvas>();
            canvas.transform.SetParent(root.transform, false);
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
            var page = BingoStyle.Rect(canvas.transform, "Page", 0, 0, width, height);
            page.anchorMin = page.anchorMax = page.pivot = new Vector2(.5f, .5f);
            page.anchoredPosition = Vector2.zero;
            build(page);
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 31;
            Canvas.ForceUpdateCanvases();
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true)) text.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = target;
            pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
            File.WriteAllBytes(path, pixels.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            root.GetComponent<Camera>().targetTexture = null;
            UnityEngine.Object.DestroyImmediate(root);
            if (pixels != null) UnityEngine.Object.DestroyImmediate(pixels);
            target.Release(); UnityEngine.Object.DestroyImmediate(target);
        }
    }

    [MenuItem("Bingo/Build Web (local preview)")]
    public static void BuildWeb()
    {
        Directory.CreateDirectory(Output);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/_Content/Scenes/BingoApp.unity" },
            locationPathName = "Builds/WebGL", target = BuildTarget.WebGL, options = BuildOptions.None
        });
        File.WriteAllText(Output + "/build-result.txt", report.summary.result + "\nErrors: " + report.summary.totalErrors);
        if (report.summary.result != BuildResult.Succeeded) Debug.LogError("Bingo Web build failed.");
    }

    public static void VerifyAndBuildWeb()
    {
        Verify();
        if (!File.ReadAllText(Output + "/result.txt").StartsWith("PASS"))
            throw new InvalidOperationException("Bingo verification failed before Web build.");
        BuildWeb();
    }
}
