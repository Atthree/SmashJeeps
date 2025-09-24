using Unity.Netcode;
using UnityEngine;

public class MystreyBoxCollecible : NetworkBehaviour, ICollectible
{
    [Header("References")]
    [SerializeField] private Animator _boxAnimator;
    [SerializeField] private Collider _collider;

    [Header("Settings")]
    [SerializeField] private float _respwanTimer;

    public void Collect()
    {
        Debug.Log("Box Working !!");
        CollectRpc();
        
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void CollectRpc()
    {
        AnimateCollection();
        Invoke(nameof(Respawn), _respwanTimer);
    }

    private void AnimateCollection()
    {
        _collider.enabled = false;
        _boxAnimator.SetTrigger(Consts.BoxAnimations.IS_COLLECTED);
    }

    private void Respawn()
    {
        _boxAnimator.SetTrigger(Consts.BoxAnimations.IS_RESPAWNED);
        _collider.enabled = true;
    }
}
