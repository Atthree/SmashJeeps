using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using Unity.Collections;
using TMPro;
using System;

public class PlayerNetworkController : NetworkBehaviour
{
    public static event Action<PlayerNetworkController> OnPlayerSpawned;
    public static event Action<PlayerNetworkController> OnPlayerDespawned;


    [SerializeField] private CinemachineCamera _playerCamera;
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private PlayerScoreController _playerScoreController;
    
    private PlayerVehicleController _playerVehicleController;
    private PlayerSkillController _playerSkillController;
    private PlayerInteractionController _playerInteractionController;
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        _playerCamera.gameObject.SetActive(IsOwner);

        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.UserName;
            SetPlayerNameRpc();

            OnPlayerSpawned?.Invoke(this);
        }

        if (!IsOwner) { return; }

        _playerVehicleController = GetComponent<PlayerVehicleController>();
        _playerSkillController = GetComponent<PlayerSkillController>();
        _playerInteractionController = GetComponent<PlayerInteractionController>();
    }

    public void OnPlayerRespawned()
    {
        _playerVehicleController.OnPlayerRespawned();
        _playerSkillController.OnPlayerRespawned();
        _playerInteractionController.OnPlayerRespawned();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerNameRpc()
    {
        _playerNameText.text = PlayerName.Value.ToString();
    }

    public PlayerScoreController GetPlayerScoreController()
    {
        return _playerScoreController;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}