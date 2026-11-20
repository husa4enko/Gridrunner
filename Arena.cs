namespace Gridrunner;

public enum TileType
{
    Floor,
    Wall
}

public class Arena
{
    public const int Columns = 20;
    public const int Rows = 12;

    private readonly TileType[,] _tiles = new TileType[Columns, Rows];

    public Arena()
    {
        // Start with every tile as floor.
        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                _tiles[x, y] = TileType.Floor;
            }
        }

        // Temporary test walls.
        _tiles[8, 5] = TileType.Wall;
        _tiles[8, 6] = TileType.Wall;
        _tiles[8, 7] = TileType.Wall;
    }

    public TileType GetTile(int x, int y)
    {
        if (x < 0 || x >= Columns || y < 0 || y >= Rows)
            throw new ArgumentOutOfRangeException();

        return _tiles[x, y];
    }
}
