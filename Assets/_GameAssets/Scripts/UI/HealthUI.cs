using DG.Tweening;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance { get; private set; }


    [Header("References")]
    [SerializeField] private RectTransform _healtBarTransform;

    [Header("Settings")]
    [SerializeField] private float _animationDuration;

    private void Awake()
    {
        Instance = this;
    }

    public void SetHealth(int health, int maxHealth)
    {
        _healtBarTransform.DOScaleX(health / (float)maxHealth, _animationDuration).SetEase(Ease.Linear);
    }

}
