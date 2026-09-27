using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NetworkBoardView : MonoBehaviour
{
    [Header("Plansze")]
    [SerializeField] private Tilemap myTilemap;
    [SerializeField] private Tilemap enemyTilemap;

    [Header("Kafelki")]
    [SerializeField] private TileBase emptyTile;
    [SerializeField] private TileBase shipTile;

    private NetworkPlayerBoard myBoard;
    private NetworkPlayerBoard enemyBoard;

    private const int BoardSize = 10;

    private void Update()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsClient)
            return;

        FindBoards();

        if (myBoard == null || enemyBoard == null)
            return;

        DrawMyBoard();
        DrawEnemyBoard();
    }

    private void FindBoards()
    {
        NetworkPlayerBoard[] boards =
            FindObjectsByType<NetworkPlayerBoard>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (NetworkPlayerBoard board in boards)
        {
            if (!board.IsSpawned)
                continue;

            if (board.IsOwner)
            {
                myBoard = board;
            }
            else
            {
                enemyBoard = board;
            }
        }
    }

    private void DrawMyBoard()
    {
        if (myTilemap == null)
            return;

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                TileBase tile = emptyTile;

                if (myBoard.HasShip(x, y))
                {
                    tile = shipTile;
                }

                myTilemap.SetTile(
                    new Vector3Int(x, y, 0),
                    tile
                );
            }
        }
    }

    private void DrawEnemyBoard()
    {
        if (enemyTilemap == null)
            return;

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                TileBase tile = emptyTile;

                if (enemyBoard.HasShip(x, y))
                {
                    tile = shipTile;
                }

                enemyTilemap.SetTile(
                    new Vector3Int(x, y, 0),
                    tile
                );
            }
        }
    }
}
