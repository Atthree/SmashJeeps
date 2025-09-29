using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager _networkManager;
    private Dictionary<ulong, string> _clientIdAuthDictionery = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> _authIdToUserDataDictionery = new Dictionary<string, UserData>();

    public NetworkServer(NetworkManager networkManager)
    {
        _networkManager = networkManager;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnServerReady;
    }

    private void OnServerReady()
    {
        _networkManager.OnClientDisconnectCallback += OnClientDisconnectCallBack;
    }

    private void OnClientDisconnectCallBack(ulong clientId)
    {
        if (_clientIdAuthDictionery.TryGetValue(clientId, out string authId))
        {
            _clientIdAuthDictionery.Remove(clientId);
            _authIdToUserDataDictionery.Remove(authId);
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        _clientIdAuthDictionery[request.ClientNetworkId] = userData.UserAuthId;
        _authIdToUserDataDictionery[userData.UserAuthId] = userData;

        response.Approved = true;
        response.CreatePlayerObject = true;
    }

    public void Dispose()
    {
        if (_networkManager == null) { return; }

        _networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        _networkManager.OnServerStarted -= OnServerReady;
        _networkManager.OnClientDisconnectCallback -= OnClientDisconnectCallBack;

        if (_networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }
    }
}

[Serializable]
public class UserData
{
    public string UserName;
    public string UserAuthId;
}
