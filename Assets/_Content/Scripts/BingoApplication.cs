using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public sealed class BingoApplication : MonoBehaviour
{
    [SerializeField] private BingoPreset[] presets;
    [SerializeField] private Font regularFont;
    [SerializeField] private Font boldFont;
    private TMP_FontAsset _regular, _bold;
    private BingoSessionStore _store;
    private BingoPresenter _presenter;
    private RectTransform _canvasRect, _page;
    private BingoView _view;
    private GameObject _modal;
    private bool _showingBoard;
    private int _screenWidth, _screenHeight;

    public BingoPreset[] Presets => presets;
    public TMP_FontAsset RegularFont => _regular;
    public TMP_FontAsset BoldFont => _bold;

    private void Awake() => Initialize();

    public void Initialize(BingoSessionStore store = null)
    {
        if (_presenter != null) return;
        _regular = TMP_FontAsset.CreateFontAsset(regularFont, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048);
        _bold = TMP_FontAsset.CreateFontAsset(boldFont, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048);
        _regular.name = "PT Serif Regular (runtime)";
        _bold.name = "PT Serif Bold (runtime)";
        _store = store ?? new BingoSessionStore();
        _presenter = new BingoPresenter(_store);
        _presenter.Changed += RenderBoard;
        var canvas = new GameObject("Bingo Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.transform.SetParent(transform, false);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvasRect = (RectTransform)canvas.transform;
        var background = BingoStyle.Fill(canvas.transform, "Paper", 0, 0, 0, 0, BingoStyle.Paper);
        background.rectTransform.anchorMin = Vector2.zero;
        background.rectTransform.anchorMax = Vector2.one;
        background.rectTransform.offsetMin = background.rectTransform.offsetMax = Vector2.zero;
        if (Application.isPlaying && EventSystem.current == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).transform.SetParent(transform, false);
        if (Application.isPlaying && Camera.main == null)
        {
            var camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
            camera.transform.SetParent(transform, false);
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BingoStyle.Paper;
            camera.cullingMask = 0;
        }
        Rebuild();
    }

    private void Update()
    {
        if (_screenWidth != Screen.width || _screenHeight != Screen.height) Rebuild();
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (_modal != null) CloseModal();
        else if (_showingBoard) ShowMenu();
    }

    private void Rebuild()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        bool wide = _screenWidth > _screenHeight * 1.2f;
        float w = wide ? 1440 : BingoStyle.Width, h = wide ? 980 : BingoStyle.Height + 24;
        if (_page != null) { _page.gameObject.SetActive(false); Dispose(_page.gameObject); }
        _modal = null;
        _page = BingoStyle.Rect(_canvasRect, "Bingo page", 0, 0, w, h);
        _page.anchorMin = _page.anchorMax = new Vector2(.5f, .5f);
        _page.pivot = new Vector2(.5f, .5f);
        _page.anchoredPosition = Vector2.zero;
        float scale = Mathf.Min(_screenWidth / w, _screenHeight / h);
        _page.localScale = Vector3.one * scale;
        _view = null;
        if (_showingBoard)
        {
            _view = _page.gameObject.AddComponent<BingoView>();
            _view.Build(_presenter.Model, _regular, _bold, wide, false, ToggleCell, ShowMenu, Export, ConfirmFinish);
        }
        else _page.gameObject.AddComponent<MainMenuView>().Build(presets, _store, _regular, _bold, wide, Open);
    }

    private void Open(BingoPreset preset)
    {
        try
        {
            _presenter.Open(preset);
            _showingBoard = true;
            Rebuild();
        }
        catch (Exception error) { ReportError("Не удалось открыть партию", error); }
    }

    private void ToggleCell(int index)
    {
        try { _presenter.Toggle(index); }
        catch (Exception error)
        {
            RenderBoard();
            ReportError("Не удалось сохранить изменения", error);
        }
    }

    private void RenderBoard()
    {
        if (_view != null && _presenter.Model != null) _view.Render(_presenter.Model);
    }

    private void ShowMenu() { _showingBoard = false; Rebuild(); }

    private void ConfirmFinish()
    {
        var panel = Modal("ЗАВЕРШИТЬ ПАРТИЮ?", "Текущая карточка и отметки будут сброшены.\nПеред завершением можно сохранить PNG.");
        BingoStyle.Button(panel, _regular, "ПРОДОЛЖИТЬ ИГРУ", 28, 266, 356, 70, CloseModal);
        BingoStyle.Button(panel, _regular, "ЗАВЕРШИТЬ", 408, 266, 356, 70, () =>
        {
            try { _presenter.Finish(); ShowMenu(); }
            catch (Exception error) { ReportError("Не удалось завершить партию", error); }
        }, true);
    }

    private void Export()
    {
        try
        {
            _view.SaveButton.interactable = false;
            byte[] png = BingoPngExporter.Render(_presenter.Model, _regular, _bold);
            string path = BingoPngExporter.Save(png, _presenter.Model.Session);
            if (path != null)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                ShowMessage("КАРТОЧКА ГОТОВА", "PNG передан браузеру для скачивания.");
#else
                ShowMessage("КАРТОЧКА СОХРАНЕНА", path);
#endif
            }
        }
        catch (Exception error) { ReportError("Не удалось сохранить PNG", error); }
        finally { if (_view != null) _view.SaveButton.interactable = true; }
    }

    private RectTransform Modal(string title, string description)
    {
        CloseModal();
        var overlay = BingoStyle.Fill(_page, "Dialog overlay", 0, 0, _page.rect.width, _page.rect.height, new Color(0, 0, 0, .3f));
        overlay.raycastTarget = true;
        _modal = overlay.gameObject;
        var panel = BingoStyle.Fill(overlay.transform, "Dialog", (_page.rect.width - 792) / 2,
            (_page.rect.height - 374) / 2, 792, 374, BingoStyle.Paper);
        panel.raycastTarget = true;
        BingoStyle.Border(panel.transform, 792, 374, BingoStyle.Ink, 2);
        BingoStyle.Text(panel.transform, "Title", title, _bold, 36, 28, 30, 736, 62, BingoStyle.Ink);
        BingoStyle.Text(panel.transform, "Description", description, _regular, 28, 28, 106, 736, 136, BingoStyle.Ink);
        return panel.rectTransform;
    }

    private void ShowMessage(string title, string description)
    {
        var panel = Modal(title, description);
        BingoStyle.Button(panel, _regular, "ПОНЯТНО", 28, 266, 736, 70, CloseModal, true);
    }

    private void CloseModal()
    {
        if (_modal == null) return;
        _modal.SetActive(false);
        Dispose(_modal);
        _modal = null;
    }

    private void ReportError(string title, Exception error)
    {
        Debug.LogException(error);
        ShowMessage(title, "Попробуйте ещё раз. Для сохранения партии\nбраузер должен разрешать локальное хранилище.");
    }

    private void OnApplicationPause(bool paused) { if (paused) Flush(); }
    private void OnApplicationFocus(bool focused) { if (!focused) Flush(); }
    private void Flush()
    {
        if (_presenter?.Model == null) return;
        try { _store.Save(_presenter.Model); }
        catch (Exception error) { Debug.LogException(error); }
    }

    private void OnDestroy()
    {
        if (_presenter != null) _presenter.Changed -= RenderBoard;
        ReleaseFont(_regular);
        ReleaseFont(_bold);
    }

    private static void ReleaseFont(TMP_FontAsset font)
    {
        if (font == null) return;
        foreach (Texture2D atlas in font.atlasTextures) if (atlas != null) Dispose(atlas);
        if (font.material != null) Dispose(font.material);
        Dispose(font);
    }

    private static void Dispose(UnityEngine.Object value)
    {
        if (Application.isPlaying) Destroy(value);
        else DestroyImmediate(value);
    }
}
