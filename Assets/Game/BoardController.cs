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

public class BoardController : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase emptyTile;
    [SerializeField] private TileBase shipTile;

    [SerializeField] private Button[] shipButtons = new Button[5];

    private const int BoardSize = 10;
    private const int MaxShipSize = 5;

    private readonly CellState[,] grid =
        new CellState[BoardSize, BoardSize];

    private int currentShipSize;
    private bool placingShip;
    private bool horizontal = true;
    private Vector2Int currentPosition;

    private readonly bool[] shipsPlaced =
        new bool[MaxShipSize];

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

    private void HandleRotation()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0)
        {
            horizontal = false;
            ShowShipPreview();
        }
        else if (scroll < 0)
        {
            horizontal = true;
            ShowShipPreview();
        }
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

    private void GenerateBoard()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                grid[x, y] = CellState.Empty;

                tilemap.SetTile(
                    new Vector3Int(x, y, 0),
                    emptyTile
                );
            }
        }
    }

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
        currentShipButton = shipButtons[size - 1];

        placingShip = true;
        horizontal = true;

        ShowShipPreview();
    }

    private void UpdateShipPosition()
    {
        if (Camera.main == null)
            return;

        Vector3 mouseWorldPosition =
            Camera.main.ScreenToWorldPoint(
                Input.mousePosition
            );

        Vector3Int cellPosition =
            tilemap.WorldToCell(mouseWorldPosition);

        Vector2Int newPosition = new Vector2Int(
            cellPosition.x,
            cellPosition.y
        );

        if (newPosition == currentPosition)
            return;

        currentPosition = newPosition;

        ShowShipPreview();
    }

    private void ShowShipPreview()
    {
        RefreshBoard();

        if (!CanPlaceShip(
            currentPosition,
            currentShipSize,
            horizontal))
        {
            return;
        }

        for (int i = 0; i < currentShipSize; i++)
        {
            Vector2Int position =
                GetShipCellPosition(i);

            tilemap.SetTile(
                new Vector3Int(
                    position.x,
                    position.y,
                    0
                ),
                shipTile
            );
        }
    }

    private Vector2Int GetShipCellPosition(int index)
    {
        if (horizontal)
        {
            return new Vector2Int(
                currentPosition.x + index,
                currentPosition.y
            );
        }

        return new Vector2Int(
            currentPosition.x,
            currentPosition.y + index
        );
    }

    private bool CanPlaceShip(
        Vector2Int position,
        int size,
        bool isHorizontal)
    {
        for (int i = 0; i < size; i++)
        {
            int x = position.x +
                    (isHorizontal ? i : 0);

            int y = position.y +
                    (isHorizontal ? 0 : i);

            if (x < 0 || x >= BoardSize ||
                y < 0 || y >= BoardSize)
            {
                return false;
            }

            if (grid[x, y] != CellState.Empty)
            {
                return false;
            }
        }

        return true;
    }

  /*  private void PlaceShip()
    {
        if (!CanPlaceShip(
            currentPosition,
            currentShipSize,
            horizontal))
        {
            return;
        }

        for (int i = 0; i < currentShipSize; i++)
        {
            Vector2Int position =
                GetShipCellPosition(i);

            grid[position.x, position.y] =
                CellState.Ship;
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
*/
  private void PlaceShip()
  {
      if (!CanPlaceShip(
              currentPosition,
              currentShipSize,
              horizontal))
      {
          return;
      }

      for (int i = 0; i < currentShipSize; i++)
      {
          Vector2Int position =
              GetShipCellPosition(i);

          // Lokalna plansza
          grid[position.x, position.y] =
              CellState.Ship;

          // Synchronizacja sieciowa
          if (NetworkBoardSync.LocalInstance != null)
          {
              NetworkBoardSync.LocalInstance.SendShipPosition(
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
    private void RefreshBoard()
    {
        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                TileBase tile =
                    grid[x, y] == CellState.Ship
                        ? shipTile
                        : emptyTile;

                tilemap.SetTile(
                    new Vector3Int(x, y, 0),
                    tile
                );
            }
        }
    }
}