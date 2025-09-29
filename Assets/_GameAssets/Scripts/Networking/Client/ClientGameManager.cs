using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : IDisposable
{
    private JoinAllocation _joinAllocation;
    private NetworkClient _networkClient;
    public async UniTask<bool> InitAsync()
    {
        // Authenticate player
        await UnityServices.InitializeAsync();

        _networkClient = new NetworkClient(NetworkManager.Singleton);

        AuthenticationState authenticationState = await AuthenticationHandler.DoAuth();

        if (authenticationState == AuthenticationState.Authenticated)
        {
            return true;
        }
        return false;
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(Consts.SceneNames.MENU_SCENE);
    }

    public async UniTask StartClientAsync(string joinJode)
    {
        try
        {
            _joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinJode);
        }
        catch (Exception exepcion)
        {
            Debug.LogError(exepcion);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(_joinAllocation, "dtls"));

        UserData userData = new UserData
        {
            UserName = PlayerPrefs.GetString(Consts.PlayerData.PLAYER_NAME, "Noname"),
            UserAuthId = AuthenticationService.Instance.PlayerId
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
 
        NetworkManager.Singleton.StartClient();
    }

    public void Dispose()
    {
        _networkClient?.Dispose();
    }
}
