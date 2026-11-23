/*using Unity.Netcode;
using UnityEngine;

public class NetworkBoardSync : NetworkBehaviour
{
    public static NetworkBoardSync LocalInstance { get; private set; }

    private NetworkPlayerBoard networkBoard;

    private BoardController boardController;

    private const int BoardSize = 10;

    private void Awake()
    {
        networkBoard = GetComponent<NetworkPlayerBoard>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log(
            $"NetworkBoardSync | " +
            $"Owner: {OwnerClientId} | " +
            $"Local: {NetworkManager.Singleton.LocalClientId} | " +
            $"IsOwner: {IsOwner}"
        );

        if (!IsOwner)
            return;

        LocalInstance = this;

        FindBoardController();

        // Jeżeli statki zostały ustawione
        // PRZED uruchomieniem sieci,
        // wysyłamy je teraz.
        Invoke(nameof(SyncExistingBoard), 0.2f);
    }

    public override void OnNetworkDespawn()
    {
        if (LocalInstance == this)
        {
            LocalInstance = null;
        }
    }

    private void FindBoardController()
    {
        boardController =
            FindFirstObjectByType<BoardController>();

        if (boardController == null)
        {
            Debug.LogError(
                "NetworkBoardSync: nie znaleziono BoardController!"
            );
        }
    }

    private void SyncExistingBoard()
    {
        if (!IsOwner)
            return;

        if (boardController == null)
        {
            FindBoardController();
        }

        if (boardController == null)
            return;

        Debug.Log("Synchronizuję istniejącą planszę...");

        int count = 0;

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                if (boardController.grid[x, y] == CellState.Ship)
                {
                    SendShipPosition(x, y);
                    count++;
                }
            }
        }

        Debug.Log(
            $"Synchronizacja zakończona. Wysłano {count} pól."
        );
    }

    public void SendShipPosition(int x, int y)
    {
        if (!IsOwner)
            return;

        if (networkBoard == null)
        {
            Debug.LogError(
                "NetworkBoardSync: brak NetworkPlayerBoard!"
            );

            return;
        }

        networkBoard.SetCellServerRpc(x, y);
    }
}*/

using Unity.Netcode;
using UnityEngine;

public class NetworkBoardSync : NetworkBehaviour
{
    public static NetworkBoardSync LocalInstance { get; private set; }

    private NetworkPlayerBoard networkBoard;
    private BoardController boardController;

    private void Awake()
    {
        Debug.Log(
            $"NetworkBoardSync Awake | GameObject: {gameObject.name}"
        );

        networkBoard = GetComponent<NetworkPlayerBoard>();

        if (networkBoard == null)
        {
            Debug.LogError(
                "NetworkBoardSync: BRAK NetworkPlayerBoard na tym samym GameObject!"
            );
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log(
            $"NetworkBoardSync OnNetworkSpawn | " +
            $"OwnerClientId: {OwnerClientId} | " +
            $"IsOwner: {IsOwner}"
        );

        if (!IsOwner)
            return;

        LocalInstance = this;

        FindBoardController();

        // Synchronizujemy planszę po krótkim opóźnieniu.
        Invoke(nameof(SyncExistingBoard), 0.5f);
    }

    public override void OnNetworkDespawn()
    {
        if (LocalInstance == this)
            LocalInstance = null;
    }

    private void FindBoardController()
    {
        boardController = FindFirstObjectByType<BoardController>();

        if (boardController == null)
        {
            Debug.LogError(
                "NetworkBoardSync: NIE ZNALEZIONO BoardController!"
            );
        }
    }

    private void SyncExistingBoard()
    {
        if (!IsOwner)
            return;

        if (boardController == null)
            FindBoardController();

        if (boardController == null)
            return;

        if (networkBoard == null)
            networkBoard = GetComponent<NetworkPlayerBoard>();

        if (networkBoard == null || !networkBoard.IsSpawned)
        {
            Debug.LogError(
                "NetworkBoardSync: NetworkPlayerBoard nie jest gotowy!"
            );
            return;
        }

        Debug.Log(
            "NetworkBoardSync: synchronizuję istniejącą planszę..."
        );

        int count = 0;

        for (int x = 0; x < BoardController.BoardSize; x++)
        {
            for (int y = 0; y < BoardController.BoardSize; y++)
            {
                if (boardController.grid[x, y] != CellState.Ship)
                    continue;

                byte shipId = boardController.GetShipId(x, y);

                if (shipId == 0)
                {
                    Debug.LogError(
                        $"Brak shipId dla pola {x}, {y}!"
                    );
                    continue;
                }

                SendShipPosition(x, y, shipId);
                count++;
            }
        }

        Debug.Log(
            $"NetworkBoardSync: synchronizacja zakończona. " +
            $"Wysłano {count} pól."
        );
    }

    public void SendShipPosition(int x, int y, byte shipId)
    {
        if (!IsOwner)
            return;

        if (networkBoard == null || !networkBoard.IsSpawned)
            return;

        if (shipId == 0)
            return;

        networkBoard.SetCellServerRpc(x, y, shipId);
    }
}


