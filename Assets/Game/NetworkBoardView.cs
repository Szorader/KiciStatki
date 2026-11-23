/*using Unity.Netcode;
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

    private const int BoardSize = 8;

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
}*/


using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NetworkBoardView : MonoBehaviour
{
    [Header("Plansza przeciwnika")]
    [SerializeField] private Tilemap enemyTilemap;

    [Header("Moja plansza - nakladka")]
    [SerializeField] private Tilemap playerHitsTilemap;

    [Header("Kafelki planszy przeciwnika")]
    [SerializeField] private TileBase emptyTile;
    [SerializeField] private TileBase missTile;
    [SerializeField] private TileBase hitTile;
    [SerializeField] private TileBase sunkTile;

    [Header("Kafelki mojej planszy")]
    [SerializeField] private TileBase playerMissTile;
    [SerializeField] private TileBase playerHitTile;
    [SerializeField] private TileBase playerSunkTile;

    private NetworkPlayerBoard enemyBoard;
    private NetworkPlayerBoard ownBoard;

    private Camera mainCamera;

    private const int BoardSize = 8;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsClient)
            return;

        FindBoards();

        if (enemyBoard != null)
            DrawEnemyBoard();

        if (ownBoard != null)
            DrawOwnHits();

        if (Input.GetMouseButtonDown(0))
            ShootFromMousePosition();
    }

    // =====================================================
    // ZNAJDOWANIE PLANSZ
    // =====================================================

    private void FindBoards()
    {
        if (ownBoard != null && enemyBoard != null &&
            ownBoard.IsSpawned && enemyBoard.IsSpawned)
            return;

        ownBoard = null;
        enemyBoard = null;

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
                ownBoard = board;
            else
                enemyBoard = board;
        }
    }

    // =====================================================
    // PLANSZA PRZECIWNIKA
    // =====================================================

    private void DrawEnemyBoard()
    {
        if (enemyTilemap == null)
            return;

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                byte state = enemyBoard.GetCell(x, y);

                TileBase tile = emptyTile;

                switch (state)
                {
                    case 2:
                        tile = missTile;
                        break;

                    case 3:
                        tile = hitTile;
                        break;

                    case 4:
                        tile = sunkTile;
                        break;
                }

                Vector3Int position =
                    new Vector3Int(x, y, 0);

                if (enemyTilemap.GetTile(position) != tile)
                    enemyTilemap.SetTile(position, tile);
            }
        }
    }

    // =====================================================
    // MOJA PLANSZA - TYLKO NAKLADKA
    // =====================================================

    private void DrawOwnHits()
    {
        if (playerHitsTilemap == null)
            return;

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                byte state = ownBoard.GetCell(x, y);

                TileBase overlay = null;

                switch (state)
                {
                    case 2:
                        overlay = playerMissTile;
                        break;

                    case 3:
                        overlay = playerHitTile;
                        break;

                    case 4:
                        overlay = playerSunkTile;
                        break;
                }

                Vector3Int position =
                    new Vector3Int(x, y, 0);

                if (playerHitsTilemap.GetTile(position) != overlay)
                {
                    playerHitsTilemap.SetTile(
                        position,
                        overlay
                    );
                }
            }
        }
    }

    // =====================================================
    // KLIKANIE I STRZELANIE
    // =====================================================

    private void ShootFromMousePosition()
    {
        if (mainCamera == null ||
            enemyTilemap == null ||
            enemyBoard == null)
            return;

        Vector3 mousePosition =
            mainCamera.ScreenToWorldPoint(
                Input.mousePosition
            );

        Vector3Int cellPosition =
            enemyTilemap.WorldToCell(mousePosition);

        if (cellPosition.x < 0 ||
            cellPosition.x >= BoardSize ||
            cellPosition.y < 0 ||
            cellPosition.y >= BoardSize)
            return;

        Shoot(cellPosition.x, cellPosition.y);
    }

    public void Shoot(int x, int y)
    {
        if (enemyBoard == null ||
            !enemyBoard.IsSpawned)
            return;

        if (PlayerReady.GetCurrentTurnOwnerId() !=
            NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("To nie Twoja kolej!");
            return;
        }

        byte state = enemyBoard.GetCell(x, y);

        if (state == 2 || state == 3 || state == 4)
            return;

        enemyBoard.ShootServerRpc(x, y);
    }
}

