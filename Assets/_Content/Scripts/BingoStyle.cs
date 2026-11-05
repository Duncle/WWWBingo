using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class BingoStyle
{
    public static readonly Color Paper = new Color32(252, 251, 248, 255);
    public static readonly Color Ink = new Color32(18, 18, 18, 255);
    public static readonly Color Red = new Color32(179, 0, 0, 255);
    public static readonly Color Gray = new Color32(139, 137, 133, 255);
    public const float Width = 1080;
    public const float Height = 1760;

    public static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(w, h);
        return rect;
    }

    public static Image Fill(Transform parent, string name, float x, float y, float w, float h, Color color)
    {
        var image = Rect(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static TMP_Text Text(Transform parent, string name, string value, TMP_FontAsset font,
        float size, float x, float y, float w, float h, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
    {
        var text = Rect(parent, name, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        return text;
    }

    public static void Border(Transform parent, float w, float h, Color color, float thickness = 1.6f)
    {
        Fill(parent, "Top rule", 0, 0, w, thickness, color);
        Fill(parent, "Bottom rule", 0, h - thickness, w, thickness, color);
        Fill(parent, "Left rule", 0, 0, thickness, h, color);
        Fill(parent, "Right rule", w - thickness, 0, thickness, h, color);
    }

    public static BingoIcon Icon(Transform parent, BingoIcon.Shape shape, float x, float y, float size, Color color)
    {
        var icon = Rect(parent, shape.ToString(), x, y, size, size).gameObject.AddComponent<BingoIcon>();
        icon.Kind = shape;
        icon.color = color;
        icon.raycastTarget = false;
        return icon;
    }

    public static Button Button(Transform parent, TMP_FontAsset font, string label, float x, float y, float w,
        float h, UnityAction action, bool primary = false, BingoIcon.Shape? icon = null)
    {
        var background = Fill(parent, label, x, y, w, h, primary ? Red : Paper);
        background.raycastTarget = true;
        var button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        var colors = button.colors;
        colors.highlightedColor = new Color(.91f, .87f, .84f);
        colors.pressedColor = new Color(.77f, .71f, .67f);
        colors.selectedColor = new Color(.94f, .91f, .87f);
        button.colors = colors;
        if (!primary) Border(background.transform, w, h, Ink);
        Text(background.transform, "Label", label, font, 30, icon.HasValue ? 68 : 16, 0,
            w - (icon.HasValue ? 92 : 32), h, primary ? Color.white : Ink, TextAlignmentOptions.Center);
        if (icon.HasValue) Icon(background.transform, icon.Value, 25, (h - 32) / 2, 32, primary ? Color.white : Red);
        button.onClick.AddListener(action);
        return button;
    }

    public static string ModeName(BingoType type)
    {
        switch (type)
        {
            case BingoType.SeriesFinal: return "Финал серии";
            case BingoType.YearFinal: return "Финал года";
            default: return "Обычная игра";
        }
    }

    public static string Lines(int count)
    {
        int last = count % 10, pair = count % 100;
        string ending = pair >= 11 && pair <= 14 ? "линий" : last == 1 ? "линия" : last >= 2 && last <= 4 ? "линии" : "линий";
        return count + " " + ending;
    }

    public static void Header(Transform parent, TMP_FontAsset regular, TMP_FontAsset bold, float w, string issue, bool wide)
    {
        float margin = 48, center = w / 2;
        Fill(parent, "Masthead top", margin, 30, w - margin * 2, 2, Ink);
        Fill(parent, "Masthead bottom", margin, 104, w - margin * 2, 2, Ink);
        if (!string.IsNullOrEmpty(issue))
        {
            Text(parent, "Club", "<color=#B30000>●</color>  КЛУБ ИНТЕЛЛЕКТУАЛОВ", regular, 20, margin, 36, center - 100, 60, Ink);
            Text(parent, "Issue", issue, regular, 24, center + 80, 36, center - 128, 60, Red, TextAlignmentOptions.MidlineRight);
        }
        Fill(parent, "Emblem paper", center - 64, 15, 128, 108, Paper);
        Icon(parent, BingoIcon.Shape.Owl, center - 49, 17, 98, Ink);
        Text(parent, "Title", "БИНГО", bold, wide ? 88 : 152, margin, wide ? 108 : 126, w - margin * 2, wide ? 104 : 178, Ink, TextAlignmentOptions.Center);
        float ruleY = wide ? 220 : 320;
        Fill(parent, "Title rule left", center - 315, ruleY, 292, 2, Red);
        Fill(parent, "Title rule right", center + 23, ruleY, 292, 2, Red);
        Icon(parent, BingoIcon.Shape.Diamond, center - 10, ruleY - 9, 20, Red);
        Text(parent, "Subtitle", "«ЧТО? ГДЕ? КОГДА?»", bold, wide ? 40 : 58, margin, ruleY + 13, w - margin * 2, wide ? 52 : 85, Ink, TextAlignmentOptions.Center);
    }

    public static void Footer(Transform parent, TMP_FontAsset font, float w, float y)
    {
        Fill(parent, "Footer rule", 48, y, w - 96, 1.5f, Ink);
        Text(parent, "Motto", "ИНТЕЛЛЕКТ  ·  АЗАРТ  ·  КОМАНДА", font, 18, 48, y + 12, (w - 96) * .6f, 38, Ink);
        Text(parent, "Footer title", "ЧТО? ГДЕ? КОГДА?", font, 18, w * .6f, y + 12, w * .4f - 48, 38, Red, TextAlignmentOptions.MidlineRight);
    }
}
