using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteractionController : NetworkBehaviour
{
    [SerializeField] private CameraShake _cameraShake;
    private PlayerSkillController _playerSkillController;
    private PlayerVehicleController _playerVehicleController;
    private PlayerHealthController _playerHealthController;
    private PlayerNetworkController _playerNetworkController;
    private bool _isCrashed;
    private bool _isShieldActive;
    private bool _isSpikeActive;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        _playerSkillController = GetComponent<PlayerSkillController>();
        _playerVehicleController = GetComponent<PlayerVehicleController>();
        _playerHealthController = GetComponent<PlayerHealthController>();
        _playerNetworkController = GetComponent<PlayerNetworkController>();

        _playerVehicleController.OnVehicleCrashed += PlayerVehicleController_OnVehicleCrashed;
    }

    private void PlayerVehicleController_OnVehicleCrashed()
    {
        enabled = false;
        _isCrashed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckCollision(other);
    }
    private void OnTriggerStay(Collider other)
    {
        CheckCollision(other);
    }
    private void CheckCollision(Collider other)
    {
        if (!IsOwner) { return; }
        if (_isCrashed) { return; }
        if(GameManager.Instance.GetGameState() != GameState.Playing) { return; }

        CheckCollectibleCollision(other);
        CheckDamageableCollision(other);

    }

    private void CheckCollectibleCollision(Collider other)
    {
        if (other.gameObject.TryGetComponent(out ICollectible collectible))
        {
            collectible.Collect(_playerSkillController,_cameraShake);
        }
    }

    private void CheckDamageableCollision(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IDamageable damagable))
        {
            if (_isShieldActive)
            {
                Debug.Log("Shiled Active: Damage Blocked");
                return;
            }
            CrashTheVehicle(damagable);
        }
    }

    private void CrashTheVehicle(IDamageable damageable)
    {
        var playerName = _playerNetworkController.PlayerName.Value;

        _cameraShake.ShakeCamera(3f, 0.8f);
        damageable.Damage(_playerVehicleController,damageable.GetKillerName());
        _playerHealthController.TakeDamage(damageable.GetDamageAmount());
        SetKillerUIRpc(damageable.GetKillerClientId(), playerName.ToString(),
            RpcTarget.Single(damageable.GetKillerClientId(), RpcTargetUse.Temp));

        SpawnerManager.Instance.RespawnPlayer(damageable.GetRespawnTimer(),OwnerClientId);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SetKillerUIRpc(ulong killerClientId,FixedString32Bytes playerName ,RpcParams rpcParams)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(killerClientId, out var killerClient))
        {
            KillScreenUI.Instance.SetSmashUI(playerName.ToString());
            killerClient.PlayerObject.GetComponent<PlayerScoreController>().AddScore(1);
        }
    }

    public void OnPlayerRespawned()
    {
        enabled = true;
        _isCrashed = false;
        _playerHealthController.RestartHealth();
    }

    public void SetShieldActive(bool active) => _isShieldActive = active;
    public void SetSpikeActive(bool active) => _isSpikeActive = active;
}
