namespace Parqueo.Models;

public enum SpotStatus
{
    Free,
    Occupied
}

public enum CellType
{
    Wall,        // Muro/borde
    Road,        // Vía de acceso
    Spot,        // Espacio de parqueo
    Entry,       // Entrada al parqueadero
    Exit,        // Salida del parqueadero
    Divider      // Separador entre bloques
}

public class ParkingSpot
{
    public int Id { get; set; }           // -1 si no es espacio de parqueo
    public int Row { get; set; }
    public int Col { get; set; }
    public CellType CellType { get; set; }
    public SpotStatus Status { get; set; }
    public Vehicle? OccupiedBy { get; set; }

    public char Block { get; set; }

    public ParkingSpot(int row, int col, CellType cellType, int id = -1, char block = ' ')
    {
        Row = row;
        Col = col;
        CellType = cellType;
        Id = id;
        Block = block;
        Status = SpotStatus.Free;
        OccupiedBy = null;
    }

    public bool IsAvailable => CellType == CellType.Spot && Status == SpotStatus.Free;
    public bool IsOccupied  => CellType == CellType.Spot && Status == SpotStatus.Occupied;

    public void AssignVehicle(ref Vehicle vehicle)
    {
        Status = SpotStatus.Occupied;
        OccupiedBy = vehicle;
    }

    public Vehicle? FreeSpot()
    {
        Vehicle? v = OccupiedBy;
        Status = SpotStatus.Free;
        OccupiedBy = null;
        return v;
    }
}
