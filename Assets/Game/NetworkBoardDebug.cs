using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NetworkBoardDebug : NetworkBehaviour
{
    [SerializeField] private Tilemap myTilemap;
    [SerializeField] private Tilemap enemyTilemap;

    [SerializeField] private TileBase shipTile;
    [SerializeField] private TileBase emptyTile;

    [SerializeField] private bool hideEnemyShips = false;

    private const int BoardSize = 10;

    private NetworkList<byte> myShips;

    private void Awake()
    {
        myShips = new NetworkList<byte>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            for (int i = 0; i < BoardSize * BoardSize; i++)
                myShips.Add(0);
        }
    }

    private void Update()
    {
        if (!IsSpawned)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            TestShips();
        }
    }

    private void TestShips()
    {
        if (!IsOwner)
            return;

        // TESTOWE STATKI
        SetShipServerRpc(1, 1);
        SetShipServerRpc(2, 1);
        SetShipServerRpc(3, 1);

        SetShipServerRpc(5, 5);
        SetShipServerRpc(5, 6);
        SetShipServerRpc(5, 7);
    }

    [ServerRpc]
    private void SetShipServerRpc(
        int x,
        int y,
        ServerRpcParams rpcParams = default)
    {
        int index = y * BoardSize + x;

        if (index >= 0 && index < myShips.Count)
        {
            myShips[index] = 1;
        }
    }

    public void ShowEnemyShips()
    {
        if (enemyTilemap == null)
            return;

        enemyTilemap.ClearAllTiles();

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                int index = y * BoardSize + x;

                if (index >= myShips.Count)
                    continue;

                bool hasShip = myShips[index] == 1;

                if (hasShip && !hideEnemyShips)
                {
                    enemyTilemap.SetTile(
                        new Vector3Int(x, y, 0),
                        shipTile
                    );
                }
                else
                {
                    enemyTilemap.SetTile(
                        new Vector3Int(x, y, 0),
                        emptyTile
                    );
                }
            }
        }
    }
}