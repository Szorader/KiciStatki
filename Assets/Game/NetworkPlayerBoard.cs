/*using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerBoard : NetworkBehaviour
{
    public const int BoardSize = 10;

    private NetworkList<byte> cells;

    private void Awake()
    {
        cells = new NetworkList<byte>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        // 100 pól
        for (int i = 0; i < BoardSize * BoardSize; i++)
        {
            cells.Add(0);
        }

        Debug.Log(
            $"NetworkPlayerBoard utworzony dla gracza {OwnerClientId}"
        );
    }

    [ServerRpc]
    public void SetCellServerRpc(
        int x,
        int y,
        ServerRpcParams rpcParams = default)
    {
        if (x < 0 || x >= BoardSize ||
            y < 0 || y >= BoardSize)
        {
            return;
        }

        // Dodatkowe zabezpieczenie:
        // gracz może zmieniać tylko swoją planszę
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
        {
            Debug.LogWarning(
                $"Gracz {rpcParams.Receive.SenderClientId} próbował zmienić planszę gracza {OwnerClientId}"
            );

            return;
        }

        int index = y * BoardSize + x;

        if (index >= cells.Count)
            return;

        cells[index] = 1;
    }

    public bool HasShip(int x, int y)
    {
        if (x < 0 || x >= BoardSize ||
            y < 0 || y >= BoardSize)
        {
            return false;
        }

        int index = y * BoardSize + x;

        if (index >= cells.Count)
            return false;

        return cells[index] == 1;
    }
}*/



using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerBoard : NetworkBehaviour
{
    public const int BoardSize = 8;

    // 0 - puste
    // 1 - statek
    // 2 - pudło
    // 3 - trafiony
    // 4 - zatopiony

    private NetworkList<byte> cells;
    private NetworkList<byte> shipIds;

    private void Awake()
    {
        cells = new NetworkList<byte>();
        shipIds = new NetworkList<byte>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        // Nie dodajemy ponownie danych, jeśli lista już istnieje.
        if (cells.Count == 0)
        {
            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                cells.Add(0);
                shipIds.Add(0);
            }
        }
    }

    // =====================================================
    // UMIESZCZANIE STATKÓW
    // =====================================================

    [ServerRpc]
    public void SetCellServerRpc(
        int x,
        int y,
        byte shipId,
        ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        if (!IsValidPosition(x, y) || shipId == 0)
            return;

        int index = GetIndex(x, y);

        if (index >= cells.Count || index >= shipIds.Count)
            return;

        // Nie nadpisujemy już zajętego pola.
        if (cells[index] != 0)
            return;

        cells[index] = 1;
        shipIds[index] = shipId;

        Debug.Log(
            $"Dodano statek ID {shipId} na polu ({x},{y})"
        );
    }

    // =====================================================
    // STRZELANIE
    // =====================================================

    [ServerRpc(RequireOwnership = false)]
    public void ShootServerRpc(
        int x,
        int y,
        ServerRpcParams rpcParams = default)
    {
        ulong shooterId = rpcParams.Receive.SenderClientId;

        if (!IsServer)
            return;
        
        if (PlayerReady.HasGameEnded())
            return;

        if (!IsValidPosition(x, y))
            return;

        if (!IsGameStarted())
            return;

        // Sprawdzamy, czyja jest kolej.
        if (PlayerReady.GetCurrentTurnOwnerId() != shooterId)
            return;

        // Nie można strzelać do własnej planszy.
        if (shooterId == OwnerClientId)
            return;

        int index = GetIndex(x, y);

        if (index >= cells.Count)
            return;

        byte state = cells[index];

        // Pole było już ostrzelane.
        if (state == 2 || state == 3 || state == 4)
            return;

        // PUDŁO
        if (state == 0)
        {
            cells[index] = 2;

            Debug.Log(
                $"Gracz {shooterId}: PUDŁO ({x},{y})"
            );

            // Po pudle kolej przechodzi na przeciwnika.
            PlayerReady.SetTurnForAll(OwnerClientId);

            return;
        }

        // TRAFIENIE
        if (state == 1)
        {
            cells[index] = 3;

            Debug.Log(
                $"Gracz {shooterId}: TRAFIENIE ({x},{y})"
            );

            byte shipId = shipIds[index];

            CheckSunkShip(shipId);

// Sprawdzamy, czy wszystkie statki przeciwnika zostały zatopione.
            if (AreAllShipsSunk())
            {
                PlayerReady.SetGameOverForAll(shooterId);
            }
        }
    }

    private bool AreAllShipsSunk()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            // Jeśli istnieje jeszcze nietrafiony statek
            // albo trafiony, ale niezatopiony, gra trwa.
            if (cells[i] == 1 || cells[i] == 3)
                return false;
        }

        return true;
    }
    // =====================================================
    // SPRAWDZANIE ZATOPIENIA
    // =====================================================

    private void CheckSunkShip(byte shipId)
    {
        if (shipId == 0)
            return;

        bool allHit = true;

        List<int> shipIndexes = new List<int>();

        for (int i = 0; i < shipIds.Count; i++)
        {
            if (shipIds[i] != shipId)
                continue;

            shipIndexes.Add(i);

            if (cells[i] == 1)
                allHit = false;
        }

        if (!allHit || shipIndexes.Count == 0)
            return;

        // Wszystkie pola tego statku zmieniamy na zatopione.
        foreach (int index in shipIndexes)
        {
            cells[index] = 4;
        }

        Debug.Log(
            $"ZATOPIONO STATEK ID: {shipId}"
        );
    }

    // =====================================================
    // ODCZYT
    // =====================================================

    public byte GetCell(int x, int y)
    {
        if (!IsValidPosition(x, y))
            return 0;

        int index = GetIndex(x, y);

        if (index >= cells.Count)
            return 0;

        return cells[index];
    }

    public byte GetShipId(int x, int y)
    {
        if (!IsValidPosition(x, y))
            return 0;

        int index = GetIndex(x, y);

        if (index >= shipIds.Count)
            return 0;

        return shipIds[index];
    }

    public bool HasShip(int x, int y)
    {
        byte state = GetCell(x, y);

        return state == 1 || state == 3 || state == 4;
    }

    // =====================================================
    // POMOCNICZE
    // =====================================================

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < BoardSize &&
               y >= 0 && y < BoardSize;
    }

    private int GetIndex(int x, int y)
    {
        return y * BoardSize + x;
    }

    private bool IsGameStarted()
    {
        PlayerReady[] players =
            FindObjectsByType<PlayerReady>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        int activePlayers = 0;

        foreach (PlayerReady player in players)
        {
            if (!player.IsSpawned)
                continue;

            activePlayers++;

            if (!player.GameStarted.Value)
                return false;
        }

        // Wymagamy dokładnie dwóch graczy.
        return activePlayers == 2;
    }
}
