using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "BingoPreset", menuName = "Bingo/BingoPreset")]
public class BingoPreset : ScriptableObject
{
    [SerializeField] private BingoType bingoType;
    [SerializeField] private int gridSize;
    [SerializeField] private string centerText;
    [SerializeField] private List<BingoPhrase> bingoPhrases;
    
    public BingoType BingoType => bingoType;
    public int GridSize => gridSize;
    public string CenterText => centerText;
    public List<BingoPhrase> BingoPhrases => bingoPhrases;
}
