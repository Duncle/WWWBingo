using System;
using TMPro;
using UnityEngine;

public class MainMenuView : MonoBehaviour
{
    public void Build(BingoPreset[] presets, BingoSessionStore store, TMP_FontAsset regular, TMP_FontAsset bold,
        bool wide, Action<BingoPreset> open)
    {
        float width = wide ? 1440 : BingoStyle.Width;
        BingoStyle.Header(transform, regular, bold, width, "ВАШ ИГРОВОЙ ВЕЧЕР", wide);
        float top = wide ? 352 : 444;
        BingoStyle.Text(transform, "Choose", "ВЫБЕРИТЕ ИГРУ", regular, 24, 48, top, width - 96, 50, BingoStyle.Red);
        BingoStyle.Text(transform, "Intro", "Смотрите. Замечайте. Собирайте бинго.", regular, 32, 48, top + 58, width - 96, 70, BingoStyle.Ink);
        for (int i = 0; i < presets.Length; i++)
        {
            BingoPreset preset = presets[i];
            BingoModel saved = store.Load(preset.BingoType);
            float cardW = wide ? 432 : width - 96, cardH = wide ? 280 : 240;
            float x = wide ? 48 + i * (cardW + 24) : 48, y = wide ? top + 164 : top + 172 + i * (cardH + 26);
            var panel = BingoStyle.Rect(transform, BingoStyle.ModeName(preset.BingoType), x, y, cardW, cardH);
            BingoStyle.Border(panel, cardW, cardH, BingoStyle.Ink);
            BingoStyle.Text(panel, "Index", "0" + (i + 1) + "   /   " + preset.GridSize + "×" + preset.GridSize,
                regular, 22, 28, 20, cardW - 56, 40, BingoStyle.Red);
            BingoStyle.Text(panel, "Mode", BingoStyle.ModeName(preset.BingoType), bold, 40, 28, 64, cardW - 56, 65, BingoStyle.Ink);
            string description = saved == null ? (preset.GridSize * preset.GridSize) + " событий одного вечера" :
                "Игра № " + saved.Session.number + "   ·   " + saved.MarkedCount + "/" + saved.Session.marked.Length + " отмечено";
            BingoStyle.Text(panel, "Description", description, regular, 24, 28, 130, cardW - 56, 40, BingoStyle.Gray);
            BingoStyle.Button(panel, regular, saved == null ? "НАЧАТЬ ИГРУ" : "ПРОДОЛЖИТЬ", 28,
                cardH - 66, cardW - 56, 48, () => open(preset), saved != null);
        }
        float infoY = wide ? 828 : 1470;
        BingoStyle.Fill(transform, "Info rule", 48, infoY - 20, width - 96, 1, BingoStyle.Gray);
        BingoStyle.Text(transform, "How to play", "Отмечайте события по ходу эфира.\nСоберите строку, столбец или диагональ — это бинго.",
            regular, wide ? 24 : 28, 48, infoY, width - 96, wide ? 70 : 100, BingoStyle.Ink);
        if (!wide) BingoStyle.Text(transform, "Autosave", "Партия сохраняется автоматически.\nВернуться к ней можно в любой момент.",
            regular, 25, 48, 1590, width - 96, 90, BingoStyle.Gray);
        BingoStyle.Footer(transform, regular, width, wide ? 914 : 1718);
    }
}
