using System;
using Unity.Netcode;
using UnityEngine;

public class ShieldController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Artık event, clientId parametresi gönderiyor olmalı
        PlayerSkillController.OnTimerFinished += OnTimerFinished;
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

    public override void OnNetworkDespawn()
    {
        PlayerSkillController.OnTimerFinished -= OnTimerFinished;
    }
}
