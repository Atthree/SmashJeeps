using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct LeaderboardEntitesSerializable : INetworkSerializeByMemcpy, IEquatable<LeaderboardEntitesSerializable>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public int Score;

    public LeaderboardEntitesSerializable(ulong clientId, FixedString32Bytes playerName, int score)
    {
        ClientId = clientId;
        PlayerName = playerName;
        Score = score;
    }
    public bool Equals(LeaderboardEntitesSerializable other)
    {
        return ClientId == other.ClientId
            && PlayerName.Equals(other.PlayerName)
            && Score == other.Score;
    }
}
