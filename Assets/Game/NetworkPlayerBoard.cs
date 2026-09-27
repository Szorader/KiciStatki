/*using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerBoard : NetworkBehaviour
{
    private const int BoardSize = 10;

    private NetworkList<byte> cells;

    private void Awake()
    {
        cells = new NetworkList<byte>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                cells.Add(0);
            }
        }
    }

    [ServerRpc]
    public void SetCellServerRpc(int x, int y, ServerRpcParams rpcParams = default)
    {
        if (x < 0 || x >= BoardSize ||
            y < 0 || y >= BoardSize)
            return;

        int index = y * BoardSize + x;

        cells[index] = 1;
    }

    public bool HasShip(int x, int y)
    {
        int index = y * BoardSize + x;

        if (index < 0 || index >= cells.Count)
            return false;

        return cells[index] == 1;
    }
}*/
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
        if (IsServer)
        {
            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                cells.Add(0);
            }

            Debug.Log($"Plansza utworzona dla gracza {OwnerClientId}");
        }
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

        int index = y * BoardSize + x;

        cells[index] = 1;

        Debug.Log(
            $"Gracz {OwnerClientId} ustawił statek: {x}, {y}"
        );
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
        Debug.Log(
            $"BOARD SPAWNED | " +
            $"OwnerClientId: {OwnerClientId} | " +
            $"LocalClientId: {NetworkManager.Singleton.LocalClientId} | " +
            $"IsOwner: {IsOwner} | " +
            $"IsServer: {IsServer} | " +
            $"IsClient: {IsClient}"
        );

        if (IsServer)
        {
            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                cells.Add(0);
            }

            Debug.Log(
                $"Utworzono 100 pól dla gracza {OwnerClientId}"
            );
        }
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

        int index = y * BoardSize + x;

        cells[index] = 1;

        Debug.Log(
            $"SERWER: ustawiono statek gracza {OwnerClientId} " +
            $"na {x},{y}"
        );
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

