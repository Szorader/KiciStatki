using Unity.Netcode;
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
}