using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardController : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase tile;

    private const int BoardSize = 10;

    private void Start()
    {
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                tilemap.SetTile(position, tile);
            }
        }
    }
}