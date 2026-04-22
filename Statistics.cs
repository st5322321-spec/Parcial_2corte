using Parqueo.Models;

namespace Parqueo.Core;

public class Statistics
{
    private readonly ParkingMap        _map;
    private readonly ParkingManager    _manager;

    public Statistics(ParkingMap map, ParkingManager manager)
    {
        _map     = map;
        _manager = manager;
    }

    public int   TotalSpots    => _map.TotalSpots;
    public int   FreeSpots     => _map.FreeSpots;
    public int   OccupiedSpots => _map.OccupiedSpots;

    public double OccupancyPercent =>
        TotalSpots == 0 ? 0 : Math.Round((double)OccupiedSpots / TotalSpots * 100, 1);

    public (string plate, string time)? GetLongestStay()
    {
        var parked = _manager.GetAllParked();
        if (parked.Count == 0) return null;

        var longest = parked.OrderByDescending(p => DateTime.Now - p.vehicle.EntryTime).First();
        return (longest.vehicle.Plate, longest.vehicle.GetElapsedTime());
    }

    public decimal TotalCollected     => _manager.TotalCollected();
    public int     TotalTransactions  => _manager.History.Count;

    public decimal AverageTransaction =>
        TotalTransactions == 0 ? 0 : TotalCollected / TotalTransactions;

    public int CountByType(VehicleType type)
        => _manager.GetAllParked().Count(p => p.vehicle.Type == type);

    public int TransactionsByType(VehicleType type)
        => _manager.History.Count(t => t.VehicleType == type);

    public decimal CollectedByType(VehicleType type)
        => _manager.History.Where(t => t.VehicleType == type).Sum(t => t.AmountPaid);

    public string[] GenerateDayClosingReport()
    {
        var lines = new List<string>
        {
            $"Fecha: {DateTime.Now:yyyy-MM-dd}",
            $"Hora de cierre: {DateTime.Now:HH:mm:ss}",
            "",
            $"Total vehículos atendidos : {TotalTransactions}",
            $"  Motos   : {TransactionsByType(VehicleType.Moto)}",
            $"  Carros  : {TransactionsByType(VehicleType.Carro)}",
            $"  Camiones: {TransactionsByType(VehicleType.Camion)}",
            "",
            $"Ingresos totales del día  : ${TotalCollected:N0} COP",
            $"  Motos   : ${CollectedByType(VehicleType.Moto):N0}",
            $"  Carros  : ${CollectedByType(VehicleType.Carro):N0}",
            $"  Camiones: ${CollectedByType(VehicleType.Camion):N0}",
            "",
            $"Ingreso promedio por cobro: ${AverageTransaction:N0} COP",
            $"Vehículos aún en parqueo  : {OccupiedSpots}",
            $"Espacios libres al cierre : {FreeSpots} / {TotalSpots}",
        };

        var longest = GetLongestStay();
        if (longest.HasValue)
            lines.Add($"Mayor permanencia registrada: {longest.Value.plate} ({longest.Value.time})");

        return lines.ToArray();
    }
}
