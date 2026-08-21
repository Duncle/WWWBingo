using System;
using UnityEngine;

[Serializable]
public sealed class BingoPhrase
{
    [SerializeField]
    private string text;

    [SerializeField]
    private BingoCategory category;

    public string Text => text;
    public BingoCategory Category => category;
}
