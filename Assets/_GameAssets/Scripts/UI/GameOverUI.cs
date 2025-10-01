using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LeaderBoardUI _leaderBoardUI;
    [SerializeField] private ScoreTablePlayer _scoreTablePlayerPrefab;
    [SerializeField] private Transform _scoreTableParentTransform;
    [SerializeField] private Image _gameOverBackgrondImage;
    [SerializeField] private RectTransform _gameOverTransform;
    [SerializeField] private RectTransform _scoreTableTransform;
    [SerializeField] private TMP_Text _winnerText;
    [SerializeField] private Button _mainMenuButton;

    [Header("Settings")]
    [SerializeField] private float _animationDuration;
    [SerializeField] private float _scaleDuration;


    private RectTransform _mainMenuButtonTransform;
    private RectTransform _winnerTransform;

    private void Awake()
    {
        _mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);

        _mainMenuButtonTransform = _mainMenuButton.GetComponent<RectTransform>();
        _winnerTransform = _winnerText.GetComponent<RectTransform>();
    }

    private void OnMainMenuButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.HostGameManager.Shutdown();
        }

        ClientSingleton.Instance.ClientGameManager.Disconnect();
    }

    private void Start()
    {
        _scoreTableTransform.gameObject.SetActive(false);
        _scoreTableTransform.localScale = Vector3.zero;

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStatedChanged;
    }

    private void GameManager_OnGameStatedChanged(GameState gameState)
    {
        if (gameState == GameState.GameOver)
        {
            AnimateGameOver();
        }
    }

    private void AnimateGameOver()
    {
        _gameOverBackgrondImage.DOFade(0.8f, _animationDuration / 2);
        _gameOverTransform.DOAnchorPosY(0f, _animationDuration).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            _gameOverTransform.GetComponent<TMP_Text>().DOFade(0f, _animationDuration / 2).SetDelay(1f).OnComplete(() =>
            {
                AnimateLeaderboardAndButtons();
            });
        }
        );
    }

    private void AnimateLeaderboardAndButtons()
    {
        _scoreTableTransform.gameObject.SetActive(true);
        _scoreTableTransform.DOScale(0.8f, _scaleDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            _mainMenuButtonTransform.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _winnerTransform.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack);
            });
        });

        PopulateGameOverLeaderBoard();
    }

    private void PopulateGameOverLeaderBoard()
    {
        var leaderboardData = _leaderBoardUI.GetLeaderboardData().OrderByDescending(x => x.Score).ToList();

        HashSet<ulong> existingClientId = new HashSet<ulong>();

        for (int i = 0; i < leaderboardData.Count; i++)
        {
            var entry = leaderboardData[i];

            if (existingClientId.Contains(entry.ClientId))
            {
                continue;
            }

            ScoreTablePlayer scoreTablePlayerInstance = Instantiate(_scoreTablePlayerPrefab, _scoreTableParentTransform);
            bool isOwner = entry.ClientId == NetworkManager.Singleton.LocalClientId;
            int rank = i + 1;

            scoreTablePlayerInstance.SetScoreTableData(rank.ToString(), entry.PlayerName,
                entry.Score.ToString(), isOwner);

            existingClientId.Add(entry.ClientId);
        }
        SetWinnersName();
    }

    private void SetWinnersName()
    {
        string winnersName = _leaderBoardUI.GetWinnersName();
        _winnerText.text = winnersName + " SMASHED Y'ALL!";
    }
}
