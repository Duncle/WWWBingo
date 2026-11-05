using System;
using UnityEngine;

public sealed class BingoSessionStore
{
    private readonly string _prefix;
    public BingoSessionStore(string prefix = "chgk.bingo.v1.") { _prefix = prefix; }
    private string Key(BingoType type) => _prefix + "session." + (int)type;

    public BingoModel Load(BingoType type)
    {
        if (!PlayerPrefs.HasKey(Key(type))) return null;
        try
        {
            var session = JsonUtility.FromJson<BingoSession>(PlayerPrefs.GetString(Key(type)));
            if (BingoModel.IsValid(session) && session.type == type) return new BingoModel(session);
        }
        catch (ArgumentException) { }
        Debug.LogWarning("Saved bingo is invalid; a new board will be created for " + type);
        return null;
    }

    public void Save(BingoModel model)
    {
        PlayerPrefs.SetString(Key(model.Session.type), JsonUtility.ToJson(model.Session));
        PlayerPrefs.SetInt(_prefix + "number", Math.Max(PlayerPrefs.GetInt(_prefix + "number", 0), model.Session.number));
        // Save on every change: closing a browser tab does not reliably invoke OnApplicationQuit.
        PlayerPrefs.Save();
    }

    public int NextNumber => PlayerPrefs.GetInt(_prefix + "number", 0) + 1;
    public void Finish(BingoType type)
    {
        PlayerPrefs.DeleteKey(Key(type));
        PlayerPrefs.Save();
    }
}
