using Unity.Netcode;
using UnityEngine;

public class MystreyBoxCollecible : NetworkBehaviour, ICollectible
{
    [Header("References")]
    [SerializeField] private MysteryBoxSkillsSO[] _mysteryBoxSkill;
    [SerializeField] private Animator _boxAnimator;
    [SerializeField] private Collider _collider;

    [Header("Settings")]
    [SerializeField] private float _respwanTimer;

    public void Collect(PlayerSkillController playerSkillController)
    {
        if(playerSkillController.HasSkillAlready()) { return; }

        MysteryBoxSkillsSO skill = GetRandomSkill();
        SkillsUI.Instance.SetSkill(skill.SkillName, skill.SkillIcon,skill.SkillUsageType, skill.SkillData.SpawnAmountOrTimer);
        playerSkillController.SetupSkill(skill);

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
    private MysteryBoxSkillsSO GetRandomSkill()
    {
        int randomIndex = Random.Range(0, _mysteryBoxSkill.Length);
        return _mysteryBoxSkill[randomIndex];
    }
}
