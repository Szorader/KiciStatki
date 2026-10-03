using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Netcode;

public enum CellState
{
    Empty,
    Ship,
    Hit,
    Miss
}

public enum ShipPart
{
    None,
    Single,
    Start,
    Middle1,
    Middle2,
    Middle3,
    End
}

public class BoardController : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase emptyTile;

    [Header("Ship 1")]
    [SerializeField] private TileBase ship1Tile;

    [Header("Ship 2")]
    [SerializeField] private TileBase ship2StartTile;
    [SerializeField] private TileBase ship2EndTile;

    [Header("Ship 3")]
    [SerializeField] private TileBase ship3StartTile;
    [SerializeField] private TileBase ship3MiddleTile;
    [SerializeField] private TileBase ship3EndTile;

    [Header("Ship 4")]
    [SerializeField] private TileBase ship4StartTile;
    [SerializeField] private TileBase ship4Middle1Tile;
    [SerializeField] private TileBase ship4Middle2Tile;
    [SerializeField] private TileBase ship4EndTile;

    [Header("Ship 5")]
    [SerializeField] private TileBase ship5StartTile;
    [SerializeField] private TileBase ship5Middle1Tile;
    [SerializeField] private TileBase ship5Middle2Tile;
    [SerializeField] private TileBase ship5Middle3Tile;
    [SerializeField] private TileBase ship5EndTile;

    [Header("Ship Buttons")]
    [SerializeField] private Button[] shipButtons = new Button[5];

    private const int BoardSize = 10;
    private const int MaxShipSize = 5;

    private readonly CellState[,] grid =
        new CellState[BoardSize, BoardSize];

    private readonly ShipPart[,] shipParts =
        new ShipPart[BoardSize, BoardSize];

    private readonly int[,] shipSizes =
        new int[BoardSize, BoardSize];

    private readonly int[,] shipRotations =
        new int[BoardSize, BoardSize];

    private readonly bool[] shipsPlaced =
        new bool[MaxShipSize];

    private int currentShipSize;

    private bool placingShip;

    // 0   = 0°
    // 1   = 90°
    // 2   = 180°
    // 3   = 270°
    private int currentRotation;

    private Vector2Int currentPosition;

    private Button currentShipButton;

    private void Start()
    {
        GenerateBoard();
    }

    private void Update()
    {
        if (!placingShip)
            return;

        UpdateShipPosition();
        HandleRotation();

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    // =========================================================
    // INPUT
    // =========================================================

    private void HandleRotation()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0)
        {
            RotateShipClockwise();
        }
        else if (scroll < 0)
        {
            RotateShipCounterClockwise();
        }
    }

    private void RotateShipClockwise()
    {
        currentRotation++;

        if (currentRotation >= 4)
            currentRotation = 0;

        ShowShipPreview();
    }

    private void RotateShipCounterClockwise()
    {
        currentRotation--;

        if (currentRotation < 0)
            currentRotation = 3;

        ShowShipPreview();
    }

    private void HandleMouseClick()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        PlaceShip();
    }

    // =========================================================
    // BOARD
    // =========================================================

    private void GenerateBoard()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                grid[x, y] = CellState.Empty;
                shipParts[x, y] = ShipPart.None;
                shipSizes[x, y] = 0;
                shipRotations[x, y] = 0;

                Vector3Int tilePosition =
                    new Vector3Int(x, y, 0);

                tilemap.SetTile(
                    tilePosition,
                    emptyTile
                );

                tilemap.SetTransformMatrix(
                    tilePosition,
                    Matrix4x4.identity
                );
            }
        }
    }

    // =========================================================
    // SHIP BUTTONS
    // =========================================================

    public void StartPlacingShip1()
    {
        StartPlacingShip(1);
    }

    public void StartPlacingShip2()
    {
        StartPlacingShip(2);
    }

    public void StartPlacingShip3()
    {
        StartPlacingShip(3);
    }

    public void StartPlacingShip4()
    {
        StartPlacingShip(4);
    }

    public void StartPlacingShip5()
    {
        StartPlacingShip(5);
    }

    private void StartPlacingShip(int size)
    {
        if (size < 1 || size > MaxShipSize)
            return;

        if (shipsPlaced[size - 1])
            return;

        currentShipSize = size;

        currentShipButton =
            shipButtons[size - 1];

        placingShip = true;

        // Zawsze zaczynamy od 0°
        currentRotation = 0;

        currentPosition = new Vector2Int(
            0,
            0
        );

        ShowShipPreview();
    }

    // =========================================================
    // POSITION
    // =========================================================

    private void UpdateShipPosition()
    {
        if (Camera.main == null)
            return;

        Vector3 mouseWorldPosition =
            Camera.main.ScreenToWorldPoint(
                Input.mousePosition
            );

        Vector3Int cellPosition =
            tilemap.WorldToCell(
                mouseWorldPosition
            );

        Vector2Int newPosition =
            new Vector2Int(
                cellPosition.x,
                cellPosition.y
            );

        if (newPosition == currentPosition)
            return;

        currentPosition = newPosition;

        ShowShipPreview();
    }

    // =========================================================
    // PREVIEW
    // =========================================================

    private void ShowShipPreview()
    {
        RefreshBoard();

        if (!CanPlaceShip(
            currentPosition,
            currentShipSize,
            currentRotation))
        {
            return;
        }

        for (int i = 0; i < currentShipSize; i++)
        {
            Vector2Int position =
                GetShipCellPosition(i);

            Vector3Int tilePosition =
                new Vector3Int(
                    position.x,
                    position.y,
                    0
                );

            TileBase tile =
                GetCurrentShipTile(i);

            tilemap.SetTile(
                tilePosition,
                tile
            );

            SetTileRotation(
                tilePosition,
                currentRotation
            );
        }
    }

    // =========================================================
    // SHIP POSITION
    // =========================================================

    private Vector2Int GetShipCellPosition(int index)
    {
        switch (currentRotation)
        {
            // 0°
            // START -> END
            case 0:
                return new Vector2Int(
                    currentPosition.x + index,
                    currentPosition.y
                );

            // 90°
            // START
            //   |
            //   |
            // END
            case 1:
                return new Vector2Int(
                    currentPosition.x,
                    currentPosition.y + index
                );

            // 180°
            // END <- START
            case 2:
                return new Vector2Int(
                    currentPosition.x - index,
                    currentPosition.y
                );

            // 270°
            // END
            //  |
            //  |
            // START
            case 3:
                return new Vector2Int(
                    currentPosition.x,
                    currentPosition.y - index
                );

            default:
                return currentPosition;
        }
    }

    // =========================================================
    // VALIDATION
    // =========================================================

    private bool CanPlaceShip(
        Vector2Int position,
        int size,
        int rotation)
    {
        for (int i = 0; i < size; i++)
        {
            Vector2Int shipPosition =
                GetShipCellPositionForRotation(
                    position,
                    i,
                    rotation
                );

            int x = shipPosition.x;
            int y = shipPosition.y;

            // Poza planszą
            if (x < 0 ||
                x >= BoardSize ||
                y < 0 ||
                y >= BoardSize)
            {
                return false;
            }

            // Zajęte pole
            if (grid[x, y] != CellState.Empty)
            {
                return false;
            }
        }

        return true;
    }

    private Vector2Int GetShipCellPositionForRotation(
        Vector2Int position,
        int index,
        int rotation)
    {
        switch (rotation)
        {
            // 0°
            case 0:
                return new Vector2Int(
                    position.x + index,
                    position.y
                );

            // 90°
            case 1:
                return new Vector2Int(
                    position.x,
                    position.y + index
                );

            // 180°
            case 2:
                return new Vector2Int(
                    position.x - index,
                    position.y
                );

            // 270°
            case 3:
                return new Vector2Int(
                    position.x,
                    position.y - index
                );

            default:
                return position;
        }
    }

    // =========================================================
    // PLACE SHIP
    // =========================================================

    private void PlaceShip()
    {
        if (!CanPlaceShip(
            currentPosition,
            currentShipSize,
            currentRotation))
        {
            return;
        }

        for (int i = 0; i < currentShipSize; i++)
        {
            Vector2Int position =
                GetShipCellPosition(i);

            grid[position.x, position.y] =
                CellState.Ship;

            shipParts[position.x, position.y] =
                GetShipPart(i);

            shipSizes[position.x, position.y] =
                currentShipSize;

            shipRotations[position.x, position.y] =
                currentRotation;

            // Synchronizacja sieciowa
            if (NetworkBoardSync.LocalInstance != null)
            {
                NetworkBoardSync.LocalInstance
                    .SendShipPosition(
                        position.x,
                        position.y
                    );
            }
            else
            {
                Debug.LogWarning(
                    "NetworkBoardSync.LocalInstance jest null!"
                );
            }
        }

        shipsPlaced[currentShipSize - 1] = true;

        placingShip = false;

        if (currentShipButton != null)
        {
            currentShipButton.interactable = false;
            currentShipButton = null;
        }

        RefreshBoard();
    }

    // =========================================================
    // SHIP PART
    // =========================================================

    private ShipPart GetShipPart(int index)
    {
        if (currentShipSize == 1)
            return ShipPart.Single;

        if (index == 0)
            return ShipPart.Start;

        if (index == currentShipSize - 1)
            return ShipPart.End;

        switch (index)
        {
            case 1:
                return ShipPart.Middle1;

            case 2:
                return ShipPart.Middle2;

            case 3:
                return ShipPart.Middle3;

            default:
                return ShipPart.None;
        }
    }

    // =========================================================
    // CURRENT SHIP TILE
    // =========================================================

    private TileBase GetCurrentShipTile(int index)
    {
        switch (currentShipSize)
        {
            // -------------------------------------------------
            // STATEK 1
            // -------------------------------------------------

            case 1:
                return ship1Tile;

            // -------------------------------------------------
            // STATEK 2
            // -------------------------------------------------

            case 2:

                if (index == 0)
                    return ship2StartTile;

                return ship2EndTile;

            // -------------------------------------------------
            // STATEK 3
            // -------------------------------------------------

            case 3:

                if (index == 0)
                    return ship3StartTile;

                if (index == 1)
                    return ship3MiddleTile;

                return ship3EndTile;

            // -------------------------------------------------
            // STATEK 4
            // -------------------------------------------------

            case 4:

                if (index == 0)
                    return ship4StartTile;

                if (index == 1)
                    return ship4Middle1Tile;

                if (index == 2)
                    return ship4Middle2Tile;

                return ship4EndTile;

            // -------------------------------------------------
            // STATEK 5
            // -------------------------------------------------

            case 5:

                if (index == 0)
                    return ship5StartTile;

                if (index == 1)
                    return ship5Middle1Tile;

                if (index == 2)
                    return ship5Middle2Tile;

                if (index == 3)
                    return ship5Middle3Tile;

                return ship5EndTile;

            default:
                return null;
        }
    }

    // =========================================================
    // STORED SHIP TILE
    // =========================================================

    private TileBase GetStoredShipTile(
        int x,
        int y)
    {
        int size =
            shipSizes[x, y];

        ShipPart part =
            shipParts[x, y];

        switch (size)
        {
            case 1:
                return ship1Tile;

            case 2:

                if (part == ShipPart.Start)
                    return ship2StartTile;

                return ship2EndTile;

            case 3:

                if (part == ShipPart.Start)
                    return ship3StartTile;

                if (part == ShipPart.Middle1)
                    return ship3MiddleTile;

                return ship3EndTile;

            case 4:

                if (part == ShipPart.Start)
                    return ship4StartTile;

                if (part == ShipPart.Middle1)
                    return ship4Middle1Tile;

                if (part == ShipPart.Middle2)
                    return ship4Middle2Tile;

                return ship4EndTile;

            case 5:

                if (part == ShipPart.Start)
                    return ship5StartTile;

                if (part == ShipPart.Middle1)
                    return ship5Middle1Tile;

                if (part == ShipPart.Middle2)
                    return ship5Middle2Tile;

                if (part == ShipPart.Middle3)
                    return ship5Middle3Tile;

                return ship5EndTile;

            default:
                return emptyTile;
        }
    }

    // =========================================================
    // REFRESH BOARD
    // =========================================================

    private void RefreshBoard()
    {
        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                Vector3Int tilePosition =
                    new Vector3Int(x, y, 0);

                if (grid[x, y] ==
                    CellState.Ship)
                {
                    TileBase tile =
                        GetStoredShipTile(
                            x,
                            y
                        );

                    tilemap.SetTile(
                        tilePosition,
                        tile
                    );

                    SetTileRotation(
                        tilePosition,
                        shipRotations[x, y]
                    );
                }
                else
                {
                    tilemap.SetTile(
                        tilePosition,
                        emptyTile
                    );

                    tilemap.SetTransformMatrix(
                        tilePosition,
                        Matrix4x4.identity
                    );
                }
            }
        }
    }

    // =========================================================
    // TILE ROTATION
    // =========================================================

    private void SetTileRotation(
        Vector3Int position,
        int rotation)
    {
        float angle = rotation * 90f;

        Quaternion quaternion =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );

        tilemap.SetTransformMatrix(
            position,
            Matrix4x4.TRS(
                Vector3.zero,
                quaternion,
                Vector3.one
            )
        );
    }
}