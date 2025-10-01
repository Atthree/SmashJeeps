using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectSingleUI : MonoBehaviour
{
    [SerializeField] private int _colorId;
    [SerializeField] private Image _colorImage;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _selectGameObject;

    private void Awake()
    {
        _button.onClick.AddListener(() =>
        {
            MultiplayerGameManager.Instance.ChangePlayerColor(_colorId);
        });
    }
    private void Start()
    {
        MultiplayerGameManager.Instance.OnPlayerDataNetworkListChanged += MultiplayerGameManager_OnPlayerDataNetworkListChanged;

        _colorImage.color = MultiplayerGameManager.Instance.GetPlayerColor(_colorId);
        UpdateIsSelected();
    }

    private void MultiplayerGameManager_OnPlayerDataNetworkListChanged()
    {
        UpdateIsSelected();
    }

    private void UpdateIsSelected()
    {
        if (MultiplayerGameManager.Instance.GetPlayerData().ColorId == _colorId)
        {
            _selectGameObject.SetActive(true);
        }
        else
        {
            _selectGameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        MultiplayerGameManager.Instance.OnPlayerDataNetworkListChanged -= MultiplayerGameManager_OnPlayerDataNetworkListChanged;
    }
}
