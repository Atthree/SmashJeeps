using System;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public event Action<GameState> OnGameStateChanged;
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameDataSO _gameData;
    [SerializeField] private GameState _currentGameState;
    private NetworkVariable<int> _gameTimer = new NetworkVariable<int>(0);


    private void Awake()
    {
        Instance = this;
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _gameTimer.Value = _gameData.GameTimer;
            SetTimerRpc();
            InvokeRepeating(nameof(DecreaseTimer), 1f, 1f);
        }

        _gameTimer.OnValueChanged += OnTimerChange;
    }

    private void OnTimerChange(int previousValue, int newValue)
    {
        TimerUI.Instance.SetTimerUI(newValue);

        if (IsServer && newValue <= 0)
        {
            ChangeGameState(GameState.GameOver);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetTimerRpc()
    {
        TimerUI.Instance.SetTimerUI(_gameTimer.Value);
    }

    private void DecreaseTimer()
    {
        if (IsServer && _currentGameState == GameState.Playing)
        {
            _gameTimer.Value--;

            if (_gameTimer.Value <= 0)
            {
                CancelInvoke(nameof(DecreaseTimer));
            }
        }
    }

    public void ChangeGameState(GameState newGameState)
    {
        if (!IsServer) { return; }

        _currentGameState = newGameState;
        ChangeGameStateRpc(newGameState);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeGameStateRpc(GameState newGameState)
    {
        _currentGameState = newGameState;
        OnGameStateChanged?.Invoke(newGameState);
        Debug.Log($"Game State: {newGameState}");
    }

    public GameState GetGameState()
    {
        return _currentGameState;
    }
}
