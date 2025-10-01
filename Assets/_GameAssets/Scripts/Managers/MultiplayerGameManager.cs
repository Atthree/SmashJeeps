using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class MultiplayerGameManager : NetworkBehaviour
{
    public static MultiplayerGameManager Instance { get; private set; }
    public event Action OnPlayerDataNetworkListChanged;
    [SerializeField] private List<Color> _playerColorList;
    private NetworkList<PlayerDataSerializable> _playerDataNetworkList = new NetworkList<PlayerDataSerializable>();

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
        _playerDataNetworkList.OnListChanged += PlayerDataNetworklist_OnListChanged;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _playerDataNetworkList.Clear();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallBack;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallBack;

        }
    }

    private void OnClientConnectedCallBack(ulong clientId)
    {
        for (int i = 0; i < _playerDataNetworkList.Count; i++)
        {
            if (_playerDataNetworkList[i].ClientId == clientId)
            {
                _playerDataNetworkList.RemoveAt(i);
            }
        }

        _playerDataNetworkList.Add(new PlayerDataSerializable
        {
            ClientId = clientId,
            ColorId = GetFirstUnusedColorId()
        });
    }

    private void OnClientDisconnectedCallBack(ulong clientId)
    {
        for (int i = 0; i < _playerDataNetworkList.Count; i++)
        {
            PlayerDataSerializable playerData = _playerDataNetworkList[i];

            if (playerData.ClientId == clientId)
            {
                _playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void PlayerDataNetworklist_OnListChanged(NetworkListEvent<PlayerDataSerializable> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke();
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < _playerDataNetworkList.Count;
    }

    public PlayerDataSerializable GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return _playerDataNetworkList[playerIndex];
    }

    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorRpc(colorId);
    }


    [Rpc(SendTo.Server)]
    private void ChangePlayerColorRpc(int colorId, RpcParams rpcParams = default)
    {
        if (!IsColorAvailable(colorId))
        {
            //COLOR NOT AVAILABLE
            return;
        }

        int playerDataIndex = GetPlayerDataIndexFromClientId(rpcParams.Receive.SenderClientId);
        PlayerDataSerializable playerData = _playerDataNetworkList[playerDataIndex];
        playerData.ColorId = colorId;
        _playerDataNetworkList[playerDataIndex] = playerData;
    }

    private int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < _playerDataNetworkList.Count; i++)
        {
            if (_playerDataNetworkList[i].ClientId == clientId)
            {
                return i;
            }
        }

        return -1;
    }

    public Color GetPlayerColor(int colorId)
    {
        return _playerColorList[colorId];
    }

    private int GetFirstUnusedColorId()
    {
        for (int i = 0; i < _playerColorList.Count; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }
        return -1;
    }

    private bool IsColorAvailable(int colorId)
    {
        foreach (PlayerDataSerializable playerData in _playerDataNetworkList)
        {
            if (playerData.ColorId == colorId)
            {
                // ALREADY USING
                return false;
            }
        }
        return true;
    }

    public PlayerDataSerializable GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerDataSerializable playerData in _playerDataNetworkList)
        {
            if (playerData.ClientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    public PlayerDataSerializable GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        OnClientDisconnectedCallBack(clientId);
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallBack;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallBack;
    }
}
