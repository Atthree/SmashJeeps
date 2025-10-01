using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharaterSelectPlayer : NetworkBehaviour
{
    [SerializeField] private int _playerIndex;
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private GameObject _readyGameObject;
    [SerializeField] private Button _kickButton;
    [SerializeField] private CharacterSelectVisual _characterSelectVisual;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    private void Awake()
    {
        _kickButton.onClick.AddListener(OnKickButtonClicked);
    }
    private void Start()
    {
        MultiplayerGameManager.Instance.OnPlayerDataNetworkListChanged
            += MultiplayerGameManager_OnPlayerDataNetworkListChanged;

        CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;

        UpdatePlayer();

        HandlePlayerNameChanged(string.Empty, PlayerName.Value);
        PlayerName.OnValueChanged += HandlePlayerNameChanged;
    }

    private void HandlePlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        _playerNameText.text = newName.ToString();
    }

    private void CharacterSelectReady_OnReadyChanged()
    {
        UpdatePlayer();
    }

    private void MultiplayerGameManager_OnPlayerDataNetworkListChanged()
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (MultiplayerGameManager.Instance.IsPlayerIndexConnected(_playerIndex))
        {
            gameObject.SetActive(true);

            PlayerDataSerializable playerdata = MultiplayerGameManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);

            _characterSelectVisual.SetPlayerColor(MultiplayerGameManager.Instance.GetPlayerColor(playerdata.ColorId));

            _readyGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerdata.ClientId));
            HideKickButton(playerdata);
            SetOwner(playerdata.ClientId);
            UpdatePlayerNameRpc();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void HideKickButton(PlayerDataSerializable playerData)
    {
        _kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer &&
            playerData.ClientId != NetworkManager.Singleton.LocalClientId);
    }

    private void OnKickButtonClicked()
    {
        PlayerDataSerializable playerData = MultiplayerGameManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);
        MultiplayerGameManager.Instance.KickPlayer(playerData.ClientId);
    }

    private void SetOwner(ulong clientId)
    {
        if (IsServer)
        {
            var networkObject = GetComponent<NetworkObject>();

            if (networkObject.OwnerClientId != clientId)
            {
                networkObject.ChangeOwnership(clientId);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdatePlayerNameRpc()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.UserName;
        }
    }

    public override void OnDestroy()
    {
        MultiplayerGameManager.Instance.OnPlayerDataNetworkListChanged
            -= MultiplayerGameManager_OnPlayerDataNetworkListChanged;

        CharacterSelectReady.Instance.OnReadyChanged -= CharacterSelectReady_OnReadyChanged;
        PlayerName.OnValueChanged -= HandlePlayerNameChanged;
    }
}
        

