using Parqueo.Models;

namespace Parqueo.Core;

public class ParkingMap
{
    public const int ROWS = 18;
    public const int COLS = 20;

    private readonly ParkingSpot[,] _grid;

    private readonly List<ParkingSpot> _spots;

    private int _nextSpotId = 1;

    public ParkingMap()
    {
        _grid  = new ParkingSpot[ROWS, COLS];
        _spots = new List<ParkingSpot>();
        InitializeMap();
    }

    public ref ParkingSpot GetCell(int row, int col) => ref _grid[row, col];

    public ParkingSpot[,] Grid => _grid;

    public IReadOnlyList<ParkingSpot> Spots => _spots.AsReadOnly();

    public int TotalSpots  => _spots.Count;
    public int FreeSpots   => _spots.Count(s => s.Status == SpotStatus.Free);
    public int OccupiedSpots => _spots.Count(s => s.Status == SpotStatus.Occupied);

    public ParkingSpot? FindFirstFreeSpot()
        => _spots.FirstOrDefault(s => s.IsAvailable);

    public ParkingSpot? FindSpotById(int id)
        => _spots.FirstOrDefault(s => s.Id == id);

    public ParkingSpot? FindSpotByPlate(string plate)
        => _spots.FirstOrDefault(s => s.IsOccupied &&
               s.OccupiedBy!.Plate.Equals(plate, StringComparison.OrdinalIgnoreCase));
    public List<ParkingSpot> GetFreeSpots()
        => _spots.Where(s => s.IsAvailable).ToList();

    public List<ParkingSpot> GetOccupiedSpots()
        => _spots.Where(s => s.IsOccupied).ToList();

    private void InitializeMap()
    {
        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)
                _grid[r, c] = new ParkingSpot(r, c, CellType.Wall);
        SetRoad(0, 0, COLS - 1);

        _grid[0, 0] = new ParkingSpot(0, 0,  CellType.Entry);
        _grid[0, 19] = new ParkingSpot(0, 19, CellType.Exit);

        for (int r = 1; r < ROWS; r++)
        {
            _grid[r, 0]  = new ParkingSpot(r, 0,  CellType.Road);
            _grid[r, 19] = new ParkingSpot(r, 19, CellType.Road);
        }

        SetRoad(9, 0, COLS - 1);

        SetRoad(17, 0, COLS - 1);

        for (int r = 0; r < ROWS; r++)
            if (_grid[r, 10].CellType == CellType.Wall)
                _grid[r, 10] = new ParkingSpot(r, 10, CellType.Road);

        SetRoad(5, 1, 9);
        SetRoad(5, 11, 18);
        SetRoad(13, 1, 9);
        SetRoad(13, 11, 18);

        AddBlock('A', 1, 4, 1, 4);

        AddBlock('B', 1, 4, 6, 9);
        AddBlock('C', 6, 8, 1, 4);

        AddBlock('D', 6, 8, 6, 9);
        AddBlock('E', 1, 4, 12, 15);

        AddBlock('F', 10, 12, 1, 4);

        AddBlock('G', 10, 12, 6, 9);

        AddBlock('H', 10, 12, 12, 15);

        AddBlockWide('I', 14, 16, 1, 17);
    }

    private void SetRoad(int row, int col1, int col2)
    {
        for (int c = col1; c <= col2; c++)
            _grid[row, c] = new ParkingSpot(row, c, CellType.Road);
    }

    private void AddBlock(char blockName, int r1, int r2, int c1, int c2)
    {
        for (int r = r1; r <= r2; r++)
        {
            for (int c = c1; c <= c2; c++)
            {
                var spot = new ParkingSpot(r, c, CellType.Spot, _nextSpotId++, blockName);
                _grid[r, c] = spot;
                _spots.Add(spot);
            }
        }
    }

    private void AddBlockWide(char blockName, int r1, int r2, int c1, int c2)
    {
        for (int r = r1; r <= r2; r++)
        {
            for (int c = c1; c <= c2; c++)
            {
                var spot = new ParkingSpot(r, c, CellType.Spot, _nextSpotId++, blockName);
                _grid[r, c] = spot;
                _spots.Add(spot);
            }
        }
    }
}
