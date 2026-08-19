using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public abstract class BingoRandomBase : MonoBehaviour
{
    [SerializeField] private GameObject bingoParent;
    [SerializedDictionary] private SerializedDictionary<string, string> _baseBingoDescriptions;
    
    private Dictionary<string, string> _mergedDictionary;
    
    protected abstract IReadOnlyDictionary<string, string> AdditionalBingoDescription { get; }
    
    public IReadOnlyDictionary<string, string> MergedDictionary => _mergedDictionary;

    private void Awake() => BuildMergedDictionary();

    private void BuildMergedDictionary()
    {
        _mergedDictionary = new Dictionary<string, string>();

        foreach (var description in _baseBingoDescriptions)
            _mergedDictionary.TryAdd(description.Key, description.Value);

        foreach (var description in AdditionalBingoDescription)
            _mergedDictionary.TryAdd(description.Key, description.Value);
    }
}
