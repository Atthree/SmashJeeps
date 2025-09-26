using System.Threading;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillsUI : MonoBehaviour
{
    public static SkillsUI Instance { get; private set; }

    [Header("Skill References")]
    [SerializeField] private Image _skillImage;
    [SerializeField] private TMP_Text _skillNameText;
    [SerializeField] private TMP_Text _timerCounterText;
    [SerializeField] private Transform _timerCounterParentTransform;

    [Header("Settings")]
    [SerializeField] private float _scaleDuration;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetSkillToNone();

        _timerCounterParentTransform.localScale = Vector3.zero;
        _timerCounterParentTransform.gameObject.SetActive(false);
    }
    public void SetSkill(string skillname, Sprite skillSprite, SkillUsageType skillUsageType, int timerCounter)
    {
        _skillImage.gameObject.SetActive(true);
        _skillNameText.text = skillname;
        _skillImage.sprite = skillSprite;

        if (skillUsageType == SkillUsageType.Amount || skillUsageType == SkillUsageType.Timer)
        {
            SetTimerCounterAnimation(timerCounter);
        }
    }

    public void SetTimerCounterAnimation(int timerCounter)
    {
        if (_timerCounterParentTransform.gameObject.activeInHierarchy) { return; }

        _timerCounterParentTransform.gameObject.SetActive(true);
        _timerCounterParentTransform.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack);
        _timerCounterText.text = timerCounter.ToString();
    }

    public void SetSkillToNone()
    {
        _skillImage.gameObject.SetActive(false);
        _skillNameText.text = string.Empty;

        if (_timerCounterParentTransform.gameObject.activeInHierarchy)
        {
            _timerCounterParentTransform.gameObject.SetActive(false);
        }
    }
    public void SetTimerCounterText(int timerCounter)
    {
        _timerCounterText.text = timerCounter.ToString();
    }
}
