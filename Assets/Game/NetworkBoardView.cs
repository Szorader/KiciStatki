using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NetworkBoardView : MonoBehaviour
{
    [Header("Plansza przeciwnika")]
    [SerializeField] private Tilemap enemyTilemap;

    [Header("Kafelki")]
    [SerializeField] private TileBase emptyTile;
    [SerializeField] private TileBase shipTile;

    [Header("Ustawienia")]
    [SerializeField] private bool showEnemyShips = true;

    private NetworkPlayerBoard enemyBoard;

    private const int BoardSize = 10;

    private void Update()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsClient)
            return;

        FindEnemyBoard();

        if (enemyBoard == null)
            return;

        DrawEnemyBoard();
    }

    private void FindEnemyBoard()
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

            // Szukamy planszy, która NIE należy do nas
            if (!board.IsOwner)
            {
                enemyBoard = board;
                return;
            }
        }

        enemyBoard = null;
    }

    private void DrawEnemyBoard()
    {
        if (enemyTilemap == null)
            return;

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                Vector3Int position =
                    new Vector3Int(x, y, 0);

                TileBase tile = emptyTile;

                if (showEnemyShips &&
                    enemyBoard.HasShip(x, y))
                {
                    tile = shipTile;
                }

                enemyTilemap.SetTile(
                    position,
                    tile
                );
            }
        }
    }
}