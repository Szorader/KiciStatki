
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerReady : NetworkBehaviour
{
    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> GameStarted = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    
    public NetworkVariable<ulong> CurrentTurnOwnerId =
        new NetworkVariable<ulong>(
            ulong.MaxValue,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<bool> GameOver =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<ulong> WinnerClientId =
        new NetworkVariable<ulong>(
            ulong.MaxValue,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public static event Action StateChanged;

    public override void OnNetworkSpawn()
    {
        IsReady.OnValueChanged += OnStateChanged;
        GameStarted.OnValueChanged += OnGameStartedChanged;

        StateChanged?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        IsReady.OnValueChanged -= OnStateChanged;
        GameStarted.OnValueChanged -= OnGameStartedChanged;

        StateChanged?.Invoke();
    }

    private void OnStateChanged(bool oldValue, bool newValue)
    {
        StateChanged?.Invoke();
    }

    private void OnGameStartedChanged(bool oldValue, bool newValue)
    {
        Debug.Log(
            $"GAME STARTED CHANGED | Client: {NetworkManager.Singleton.LocalClientId} | " +
            $"Owner: {OwnerClientId} | Old: {oldValue} | New: {newValue}"
        );

        StateChanged?.Invoke();
    }

    // Wywoływane po kliknięciu przycisku READY
    public void SetReady()
    {
        if (!IsOwner || IsReady.Value || GameStarted.Value)
            return;

        SetReadyServerRpc();
    }
    
    [ServerRpc]
    private void SetReadyServerRpc()
    {
        Debug.Log($"Gracz {OwnerClientId} kliknął READY!");

        if (IsReady.Value || GameStarted.Value)
            return;

        IsReady.Value = true;

        CheckPlayersReady();
    }

    // Sprawdzenie, czy obaj gracze są gotowi
    private void CheckPlayersReady()
    {
        if (!IsServer)
            return;

        int playerCount = 0;
        int readyCount = 0;

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerReady player = client.PlayerObject.GetComponent<PlayerReady>();

            if (player == null)
            {
                Debug.LogWarning($"Gracz {client.ClientId} nie ma komponentu PlayerReady!");
                continue;
            }

            playerCount++;

            if (player.IsReady.Value)
                readyCount++;

            Debug.Log(
                $"Gracz ID: {client.ClientId} | Ready: {player.IsReady.Value}"
            );
        }

        Debug.Log($"LICZNIK GOTOWOŚCI: {readyCount}/{playerCount}");

        if (playerCount == 2 && readyCount == 2)
        {
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject == null)
                    continue;

                PlayerReady player = client.PlayerObject.GetComponent<PlayerReady>();

                if (player != null)
                    player.GameStarted.Value = true;
            }

            NetworkClient firstClient =
                NetworkManager.Singleton.ConnectedClientsList[0];

            SetTurnForAll(firstClient.ClientId);

            Debug.Log("OBYDWOJE GRACZE GOTOWI - START GRY!");
        }
    }
    
    
    public static ulong GetCurrentTurnOwnerId()
    {
        PlayerReady[] players =
            FindObjectsByType<PlayerReady>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (PlayerReady player in players)
        {
            if (player.IsSpawned)
                return player.CurrentTurnOwnerId.Value;
        }

        return ulong.MaxValue;
    }

    public static void SetTurnForAll(ulong clientId)
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
            return;

        PlayerReady[] players =
            FindObjectsByType<PlayerReady>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (PlayerReady player in players)
        {
            if (player.IsSpawned)
                player.CurrentTurnOwnerId.Value = clientId;
        }

        Debug.Log($"Aktualna tura gracza: {clientId}");
    }
    // =====================================================
// KONIEC GRY
// =====================================================

    public static void SetGameOverForAll(ulong winnerId)
    {
        PlayerReady[] players =
            FindObjectsByType<PlayerReady>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (PlayerReady player in players)
        {
            if (!player.IsSpawned || !player.IsServer)
                continue;

            player.WinnerClientId.Value = winnerId;
            player.GameOver.Value = true;
        }

        Debug.Log($"KONIEC GRY! Zwycięzca: {winnerId}");
    }

    public static bool HasGameEnded()
    {
        PlayerReady[] players =
            FindObjectsByType<PlayerReady>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (PlayerReady player in players)
        {
            if (player.IsSpawned && player.GameOver.Value)
                return true;
        }

        return false;
    }

}
