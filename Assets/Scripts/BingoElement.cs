using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BingoElement : MonoBehaviour
{
    [SerializeField] private Button _elementButton;
    [SerializeField] private GameObject _crossImage;

    private void Awake() => _elementButton.onClick.AddListener(ToggleCrossImage);

    private void ToggleCrossImage() => _crossImage.SetActive(!_crossImage.activeInHierarchy);
}
