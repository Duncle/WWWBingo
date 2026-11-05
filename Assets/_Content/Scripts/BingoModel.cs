using System;
using System.Collections.Generic;

[Serializable]
public sealed class BingoSession
{
    public int version = 1;
    public BingoType type;
    public int size;
    public int number;
    public string[] phrases;
    public bool[] marked;
}

public sealed class BingoModel
{
    public BingoSession Session { get; }
    public int MarkedCount
    {
        get
        {
            int count = 0;
            foreach (bool value in Session.marked) if (value) count++;
            return count;
        }
    }

    public int CompletedLines
    {
        get
        {
            int count = 0, n = Session.size;
            for (int i = 0; i < n; i++)
            {
                bool row = true, column = true;
                for (int j = 0; j < n; j++)
                {
                    row &= Session.marked[i * n + j];
                    column &= Session.marked[j * n + i];
                }
                if (row) count++;
                if (column) count++;
            }
            bool diagonal = true, reverse = true;
            for (int i = 0; i < n; i++)
            {
                diagonal &= Session.marked[i * n + i];
                reverse &= Session.marked[i * n + n - 1 - i];
            }
            return count + (diagonal ? 1 : 0) + (reverse ? 1 : 0);
        }
    }

    public BingoModel(BingoSession session)
    {
        if (!IsValid(session)) throw new ArgumentException("Invalid bingo session.", nameof(session));
        Session = session;
    }

    public void Toggle(int index)
    {
        if (index < 0 || index >= Session.marked.Length) throw new ArgumentOutOfRangeException(nameof(index));
        Session.marked[index] = !Session.marked[index];
    }

    public static bool IsValid(BingoSession session)
    {
        if (session == null || session.version != 1 || (session.size != 3 && session.size != 5) ||
            !Enum.IsDefined(typeof(BingoType), session.type) || session.number < 1 ||
            session.phrases == null || session.marked == null ||
            session.phrases.Length != session.size * session.size || session.marked.Length != session.phrases.Length)
            return false;
        foreach (string phrase in session.phrases) if (string.IsNullOrWhiteSpace(phrase)) return false;
        return true;
    }

    public static BingoModel Generate(BingoPreset preset, int number, Random random = null)
    {
        if (preset == null) throw new ArgumentNullException(nameof(preset));
        if (preset.GridSize != 3 && preset.GridSize != 5) throw new InvalidOperationException("Only 3×3 and 5×5 grids are supported.");
        random = random ?? new Random();
        var candidates = new List<BingoPhrase>(preset.BingoPhrases);
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            BingoPhrase temp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = temp;
        }
        var groups = new HashSet<BingoCategory>();
        var texts = new HashSet<string> { preset.CenterText };
        var selected = new List<string>();
        int count = preset.GridSize * preset.GridSize;
        foreach (BingoPhrase phrase in candidates)
        {
            if (phrase == null || string.IsNullOrWhiteSpace(phrase.Text) || texts.Contains(phrase.Text)) continue;
            if (phrase.Category != BingoCategory.Uncategorized && groups.Contains(phrase.Category)) continue;
            selected.Add(phrase.Text);
            texts.Add(phrase.Text);
            if (phrase.Category != BingoCategory.Uncategorized) groups.Add(phrase.Category);
            if (selected.Count == count - 1) break;
        }
        if (selected.Count != count - 1) throw new InvalidOperationException("Not enough distinct phrases/categories in " + preset.name);
        var session = new BingoSession
        {
            type = preset.BingoType, size = preset.GridSize, number = number,
            phrases = new string[count], marked = new bool[count]
        };
        int next = 0;
        for (int i = 0; i < count; i++) session.phrases[i] = i == count / 2 ? preset.CenterText : selected[next++];
        return new BingoModel(session);
    }
}
