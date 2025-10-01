using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    private NetworkManager _networkManager;

    public NetworkClient(NetworkManager networkManager)
    {
        _networkManager = networkManager;

        networkManager.OnClientDisconnectCallback += OnClientDisconnectCallBack;
    }

    private void OnClientDisconnectCallBack(ulong clientId)
    {
        if(clientId != 0 && clientId != _networkManager.LocalClientId) { return; }

        Disconnect();
    }

    public void Disconnect()
    {
        if (SceneManager.GetActiveScene().name != Consts.SceneNames.MENU_SCENE)
        {
            SceneManager.LoadScene(Consts.SceneNames.MENU_SCENE);
        }

        if (_networkManager.IsConnectedClient)
        {
            _networkManager.Shutdown();
        }
    }

    public void Dispose()
    {
        if (_networkManager == null) { return; }

        _networkManager.OnClientDisconnectCallback -= OnClientDisconnectCallBack;

        if (_networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }
    }
}
