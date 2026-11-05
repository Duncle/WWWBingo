using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BingoView : MonoBehaviour
{
    [Header("3X3")]
    [SerializeField] private GameObject grid3X3;
    [SerializeField] private BingoCellView[] cells3X3;
    
    [Header("5X5")]
    [SerializeField] private GameObject grid5X5;
    [SerializeField] private BingoCellView[] cells5X5;

    private BingoIcon[] _marks;
    private TMP_Text _markedLabel;
    private TMP_Text _linesLabel;
    public Button SaveButton { get; private set; }

    public void Build(BingoModel model, TMP_FontAsset regular, TMP_FontAsset bold, bool wide, bool exporting,
        Action<int> toggle = null, Action back = null, Action save = null, Action finish = null)
    {
        float width = wide ? 1440 : BingoStyle.Width;
        BingoStyle.Header(transform, regular, bold, width, "ИГРА № " + model.Session.number, wide);
        float metaY = wide ? 292 : 426;
        BingoStyle.Fill(transform, "Mode rule top", 48, metaY, width - 96, 2, BingoStyle.Ink);
        BingoStyle.Text(transform, "Mode", BingoStyle.ModeName(model.Session.type) + "   <color=#B30000>•</color>   " +
            model.Session.size + "×" + model.Session.size, regular, 30, 48, metaY + 5, width - 96, 60, BingoStyle.Ink);
        BingoStyle.Fill(transform, "Mode rule bottom", 48, metaY + 72, width - 96, 2, BingoStyle.Ink);

        float gridX = 48, gridY = wide ? 386 : 660;
        float gridW = wide ? 870 : width - 96, gridH = wide ? 508 : 770;
        float statW = wide ? 426 : (width - 116) / 2;
        _markedLabel = Stat(regular, "ОТМЕЧЕНО", wide ? 956 : 48, wide ? 386 : 518, statW, wide ? 112 : 122);
        _linesLabel = Stat(regular, "БИНГО", wide ? 956 : 68 + statW, 518, statW, wide ? 112 : 122);
        int n = model.Session.size;
        const float gap = 12;
        float cellW = (gridW - (n - 1) * gap) / n, cellH = (gridH - (n - 1) * gap) / n;
        _marks = new BingoIcon[n * n];
        for (int i = 0; i < _marks.Length; i++)
        {
            int index = i;
            var background = BingoStyle.Fill(transform, "Cell " + (i + 1), gridX + (i % n) * (cellW + gap),
                gridY + (i / n) * (cellH + gap), cellW, cellH, BingoStyle.Paper);
            BingoStyle.Border(background.transform, cellW, cellH, BingoStyle.Ink);
            float fontSize = n == 3 ? 36 : 27;
            var text = BingoStyle.Text(background.transform, "Phrase", model.Session.phrases[i], regular, fontSize,
                13, 24, cellW - 26, cellH - 40, BingoStyle.Ink, TextAlignmentOptions.Center);
            text.enableAutoSizing = true;
            text.fontSizeMax = fontSize;
            text.fontSizeMin = wide ? 13 : 17;
            float markSize = n == 3 ? 64 : 47;
            _marks[i] = BingoStyle.Icon(background.transform, BingoIcon.Shape.Mark, cellW - markSize, 0, markSize, BingoStyle.Red);
            if (toggle != null)
            {
                background.raycastTarget = true;
                var button = background.gameObject.AddComponent<Button>();
                button.targetGraphic = background;
                // Clicks toggle the bingo mark without keeping Unity's selection tint.
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                var colors = button.colors;
                colors.highlightedColor = new Color(.94f, .92f, .89f);
                colors.pressedColor = new Color(.87f, .83f, .79f);
                colors.selectedColor = new Color(.96f, .94f, .91f);
                button.colors = colors;
                button.onClick.AddListener(() => toggle(index));
            }
        }

        if (!exporting)
        {
            float bx = wide ? 956 : 48, by = wide ? 654 : 1450, bw = wide ? 426 : width - 96, bh = wide ? 62 : 76;
            BingoStyle.Button(transform, regular, "ВЕРНУТЬСЯ В МЕНЮ", bx, by, bw, bh, () => back?.Invoke(), false, BingoIcon.Shape.Arrow);
            SaveButton = BingoStyle.Button(transform, regular, "СОХРАНИТЬ В PNG", bx, by + bh + 14, bw, bh, () => save?.Invoke(), true, BingoIcon.Shape.Download);
            BingoStyle.Button(transform, regular, "ЗАВЕРШИТЬ ПАРТИЮ", bx, by + (bh + 14) * 2, bw, bh, () => finish?.Invoke(), false, BingoIcon.Shape.Flag);
        }
        BingoStyle.Footer(transform, regular, width, exporting ? 1454 : wide ? 914 : 1718);
        Render(model);
    }

    public void Render(BingoModel model)
    {
        _markedLabel.text = model.MarkedCount + "/" + model.Session.marked.Length;
        _linesLabel.text = BingoStyle.Lines(model.CompletedLines);
        for (int i = 0; i < _marks.Length; i++) _marks[i].gameObject.SetActive(model.Session.marked[i]);
    }

    private TMP_Text Stat(TMP_FontAsset font, string title, float x, float y, float w, float h)
    {
        var panel = BingoStyle.Rect(transform, title, x, y, w, h);
        BingoStyle.Border(panel, w, h, BingoStyle.Gray, 1);
        BingoStyle.Fill(panel, "Red accent", 0, 0, 3, h, BingoStyle.Red);
        BingoStyle.Text(panel, "Title", title, font, 24, 30, 12, w - 48, 32, BingoStyle.Red);
        return BingoStyle.Text(panel, "Value", "", font, 60, 30, 42, w - 48, h - 44, BingoStyle.Ink);
    }
}
