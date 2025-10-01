using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }
    public event Action OnReadyChanged;
    public event Action OnUnreadyChaneged;
    public event Action OnAllPlayersReady;

    private Dictionary<ulong, bool> _playerReadyDictionery;

    private void Awake()
    {
        Instance = this;
        _playerReadyDictionery = new Dictionary<ulong, bool>();
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallBack;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallBack;
    }

    private void OnClientConnectedCallBack(ulong connectedClientId)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (IsPlayerReady(clientId))
            {
                SetPlayerReadyToAllRpc(clientId);
            }
        }
    }

    private void OnClientDisconnectCallBack(ulong clientId)
    {
        if (_playerReadyDictionery.ContainsKey(clientId))
        {
            _playerReadyDictionery.Remove(clientId);
            OnUnreadyChaneged?.Invoke();
        }
    }


    [Rpc(SendTo.Server)]
    private void SetPlayerReadyRpc(RpcParams rpcParams = default)
    {
        SetPlayerReadyToAllRpc(rpcParams.Receive.SenderClientId);
        _playerReadyDictionery[rpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_playerReadyDictionery.ContainsKey(clientId) || !_playerReadyDictionery[clientId])
            {
                allClientsReady = false;
                return;
            }
        }
        if (allClientsReady)
        {
            OnAllPlayersReady?.Invoke();
        }
    }
    [Rpc(SendTo.Server)]
    private void SetPlayerUnreadyRpc(RpcParams rpcParams = default)
    {
        SetPlayerUnreadyToAllRpc(rpcParams.Receive.SenderClientId);

        if (_playerReadyDictionery.ContainsKey(rpcParams.Receive.SenderClientId))
        {
            _playerReadyDictionery[rpcParams.Receive.SenderClientId] = false;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerReadyToAllRpc(ulong clientId)
    {
        _playerReadyDictionery[clientId] = true;
        OnReadyChanged?.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerUnreadyToAllRpc(ulong clientId)
    {
        _playerReadyDictionery[clientId] = false;
        OnReadyChanged?.Invoke();
        OnUnreadyChaneged?.Invoke();
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return _playerReadyDictionery.ContainsKey(clientId) && _playerReadyDictionery[clientId];
    }

    public bool AreAllPlayersReady()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_playerReadyDictionery.ContainsKey(clientId) || !_playerReadyDictionery[clientId])
            {
                return false;
            }
        }
        return true;
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyRpc();
    }

    public void SetPlayerUnready()
    {
        SetPlayerUnreadyRpc();
    }
}
