
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
        StateChanged?.Invoke();

        if (newValue)
        {
            Debug.Log("ROZPOCZYNAMY GRĘ!");
        }
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

        foreach (NetworkObject obj in
                 NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            PlayerReady player = obj.GetComponent<PlayerReady>();

            if (player == null)
                continue;

            playerCount++;

            if (player.IsReady.Value)
                readyCount++;
        }

        // Musi być dokładnie dwóch graczy
        if (playerCount == 2 && readyCount == 2)
        {
            foreach (NetworkObject obj in
                     NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
            {
                PlayerReady player = obj.GetComponent<PlayerReady>();

                if (player != null)
                    player.GameStarted.Value = true;
            }

            Debug.Log("OBYDWOJE GRACZE GOTOWI - START GRY!");
        }
    }
}
