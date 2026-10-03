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

    private const int BoardSize = 10;

    private void Awake()
    {
        Debug.Log(
            $"NetworkBoardSync Awake | " +
            $"GameObject: {gameObject.name}"
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
            $"Object: {gameObject.name} | " +
            $"OwnerClientId: {OwnerClientId} | " +
            $"LocalClientId: {NetworkManager.Singleton.LocalClientId} | " +
            $"IsOwner: {IsOwner} | " +
            $"IsServer: {IsServer} | " +
            $"IsClient: {IsClient}"
        );

        if (!IsOwner)
        {
            Debug.Log(
                $"NetworkBoardSync: to nie jest mój Player Object. " +
                $"Owner = {OwnerClientId}"
            );

            return;
        }

        // To jest najważniejsze.
        LocalInstance = this;

        Debug.Log(
            "NetworkBoardSync: LocalInstance ZOSTAŁ ustawiony."
        );

        FindBoardController();

        // Dajemy chwilę na uruchomienie BoardController.
        Invoke(nameof(SyncExistingBoard), 0.5f);
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log(
            $"NetworkBoardSync OnNetworkDespawn | " +
            $"OwnerClientId: {OwnerClientId}"
        );

        if (LocalInstance == this)
        {
            LocalInstance = null;
        }
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
        else
        {
            Debug.Log(
                $"NetworkBoardSync: znaleziono BoardController: " +
                $"{boardController.gameObject.name}"
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
        {
            networkBoard = GetComponent<NetworkPlayerBoard>();

            if (networkBoard == null)
            {
                Debug.LogError(
                    "NetworkBoardSync: nadal brak NetworkPlayerBoard!"
                );

                return;
            }
        }

        Debug.Log(
            "NetworkBoardSync: synchronizuję istniejącą planszę..."
        );

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
            $"NetworkBoardSync: synchronizacja zakończona. " +
            $"Wysłano {count} pól."
        );
    }

    public void SendShipPosition(int x, int y)
    {
        if (!IsOwner)
        {
            Debug.LogWarning(
                "NetworkBoardSync.SendShipPosition: nie jestem właścicielem."
            );

            return;
        }

        if (networkBoard == null)
        {
            networkBoard = GetComponent<NetworkPlayerBoard>();

            if (networkBoard == null)
            {
                Debug.LogError(
                    "NetworkBoardSync.SendShipPosition: brak NetworkPlayerBoard!"
                );

                return;
            }
        }

        if (!networkBoard.IsSpawned)
        {
            Debug.LogWarning(
                "NetworkBoardSync.SendShipPosition: " +
                "NetworkPlayerBoard nie jest jeszcze Spawned!"
            );

            return;
        }

        networkBoard.SetCellServerRpc(x, y);
    }
}

