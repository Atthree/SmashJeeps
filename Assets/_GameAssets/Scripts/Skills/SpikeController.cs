using Unity.Netcode;
using UnityEngine;

public class SpikeController : NetworkBehaviour
{
    [SerializeField] private Collider _spikeCollider;
    public override void OnNetworkSpawn()
    {
        PlayerSkillController.OnTimerFinished += OnTimerFinished;
        if (IsOwner)
        {
            SetOwnerVisualRpc();
        }
    }

    private void OnTimerFinished(ulong clientId)
    {
        if (clientId != OwnerClientId) return;

        DestroyRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }

    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualRpc()
    {
        _spikeCollider.enabled = false;
    }

    public override void OnNetworkDespawn()
    {
        PlayerSkillController.OnTimerFinished -= OnTimerFinished;
    }
}
