/*using Unity.Netcode;
using UnityEngine;

public class NetworkBoardSync : NetworkBehaviour
{
    [SerializeField] private BoardController boardController;
    [SerializeField] private NetworkPlayerBoard networkBoard;

    private void Start()
    {
        if (boardController == null)
            boardController = GetComponent<BoardController>();

        if (networkBoard == null)
            networkBoard = GetComponent<NetworkPlayerBoard>();
    }

    public void SendShipPosition(int x, int y)
    {
        if (!IsOwner)
            return;

        networkBoard.SetCellServerRpc(x, y);
    }
}*/
/*using Unity.Netcode;
using UnityEngine;

public class NetworkBoardSync : NetworkBehaviour
{
    [SerializeField] private NetworkPlayerBoard networkBoard;

    private void Awake()
    {
        if (networkBoard == null)
        {
            networkBoard = GetComponent<NetworkPlayerBoard>();
        }
    }

    public void SendShipPosition(int x, int y)
    {
        if (!IsOwner)
        {
            Debug.LogWarning("Nie jesteś właścicielem tej planszy.");
            return;
        }

        if (networkBoard == null)
        {
            Debug.LogError("Brak NetworkPlayerBoard!");
            return;
        }

        networkBoard.SetCellServerRpc(x, y);
    }
}*/
/*using Unity.Netcode;
using UnityEngine;

public class NetworkBoardSync : NetworkBehaviour
{
    [SerializeField] private NetworkPlayerBoard networkBoard;

    private void Awake()
    {
        if (networkBoard == null)
            networkBoard = GetComponent<NetworkPlayerBoard>();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            TestShips();
        }
    }

    private void TestShips()
    {
        networkBoard.SetCellServerRpc(1, 1);
        networkBoard.SetCellServerRpc(2, 1);
        networkBoard.SetCellServerRpc(3, 1);

        networkBoard.SetCellServerRpc(5, 5);
        networkBoard.SetCellServerRpc(5, 6);
        networkBoard.SetCellServerRpc(5, 7);

        Debug.Log("Wysłano testowe statki.");
    }
}*/
/*using Unity.Netcode;
using UnityEngine;

public class NetworkBoardSync : NetworkBehaviour
{
    [SerializeField] private NetworkPlayerBoard networkBoard;

    private void Awake()
    {
        if (networkBoard == null)
            networkBoard = GetComponent<NetworkPlayerBoard>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log(
            $"NetworkPlayer SPAWNED | " +
            $"ClientId: {OwnerClientId} | " +
            $"IsOwner: {IsOwner} | " +
            $"IsServer: {IsServer} | " +
            $"IsClient: {IsClient}"
        );
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(
                $"T KLIKNIĘTE | Mój ClientId: {OwnerClientId}"
            );

            TestShips();
        }
    }

    private void TestShips()
    {
        if (networkBoard == null)
        {
            Debug.LogError("BRAK NetworkPlayerBoard!");
            return;
        }

        networkBoard.SetCellServerRpc(1, 1);
        networkBoard.SetCellServerRpc(2, 1);
        networkBoard.SetCellServerRpc(3, 1);

        networkBoard.SetCellServerRpc(5, 5);
        networkBoard.SetCellServerRpc(5, 6);
        networkBoard.SetCellServerRpc(5, 7);

        Debug.Log("Wysłano statki do serwera.");
    }
}*/
using Unity.Netcode;
using UnityEngine;

public class NetworkBoardSync : NetworkBehaviour
{
    public static NetworkBoardSync LocalInstance { get; private set; }

    [SerializeField] private NetworkPlayerBoard networkBoard;

    private void Awake()
    {
        if (networkBoard == null)
        {
            networkBoard = GetComponent<NetworkPlayerBoard>();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        Debug.Log(
            $"NetworkBoardSync Spawn | " +
            $"Owner: {OwnerClientId} | " +
            $"IsOwner: {IsOwner}"
        );
    }

    public override void OnNetworkDespawn()
    {
        if (LocalInstance == this)
        {
            LocalInstance = null;
        }
    }

    public void SendShipPosition(int x, int y)
    {
        if (!IsOwner)
            return;

        if (networkBoard == null)
        {
            Debug.LogError("Brak NetworkPlayerBoard!");
            return;
        }

        networkBoard.SetCellServerRpc(x, y);
    }
}


