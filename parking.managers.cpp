using Parqueo.Models;

namespace Parqueo.Core;

public static class Rates
{
    public const decimal MotoHourly   = 1_500m;
    public const decimal CarroHourly  = 3_000m;
    public const decimal CamionHourly = 5_000m;

    public const decimal MotoFraction   = 750m;
    public const decimal CarroFraction  = 1_500m;
    public const decimal CamionFraction = 2_500m;

    public static decimal GetHourlyRate(VehicleType type) => type switch
    {
        VehicleType.Moto   => MotoHourly,
        VehicleType.Carro  => CarroHourly,
        VehicleType.Camion => CamionHourly,
        _ => 0m
    };

    public static decimal GetFractionRate(VehicleType type) => type switch
    {
        VehicleType.Moto   => MotoFraction,
        VehicleType.Carro  => CarroFraction,
        VehicleType.Camion => CamionFraction,
        _ => 0m
    };
}

public class ParkingManager
{
    private readonly ParkingMap         _map;
    private readonly List<Transaction>  _history;
    private int                         _transactionCounter;

    public ParkingManager(ParkingMap map)
    {
        _map                = map;
        _history            = new List<Transaction>();
        _transactionCounter = 1;
    }

    public IReadOnlyList<Transaction> History => _history.AsReadOnly();
    public ParkingMap Map => _map;

    public (bool success, string message, ParkingSpot? spot) RegisterEntry(
        string plate, VehicleType type)
    {
        var existing = _map.FindSpotByPlate(plate);
        if (existing != null)
            return (false, $"La placa {plate} ya está registrada en el espacio #{existing.Id}.", null);

        ParkingSpot? spot = _map.FindFirstFreeSpot();
        if (spot == null)
            return (false, "No hay espacios disponibles en este momento.", null);

        var vehicle = new Vehicle(plate, type, spot.Id);
        spot.AssignVehicle(ref vehicle);

        return (true, $"Vehículo {plate} registrado en espacio #{spot.Id} (Bloque {spot.Block}).", spot);
    }

    public (bool success, Transaction? transaction, string message) RegisterExit(string plate)
    {
        ParkingSpot? spot = _map.FindSpotByPlate(plate);
        if (spot == null)
            return (false, null, $"No se encontró ningún vehículo con placa {plate} en el parqueadero.");

        Vehicle vehicle = spot.OccupiedBy!;
        decimal amount  = CalculateFee(ref vehicle);

        spot.FreeSpot();

        var transaction = new Transaction(_transactionCounter++, vehicle, amount);
        _history.Add(transaction);

        return (true, transaction, "Salida registrada exitosamente.");
    }

    public decimal CalculateFee(ref Vehicle vehicle)
    {
        TimeSpan elapsed      = DateTime.Now - vehicle.EntryTime;
        double   totalMinutes = elapsed.TotalMinutes;

        if (totalMinutes < 1)
            return 0m;

        decimal hourlyRate   = Rates.GetHourlyRate(vehicle.Type);
        decimal fractionRate = Rates.GetFractionRate(vehicle.Type);

        int     fullHours    = (int)Math.Floor(elapsed.TotalHours);
        int     remainMin    = (int)(elapsed.TotalMinutes % 60);

        decimal total = fullHours * hourlyRate;

        if (remainMin > 0)
            total += fractionRate;

        return total;
    }

    public decimal PreviewFee(string plate)
    {
        ParkingSpot? spot = _map.FindSpotByPlate(plate);
        if (spot?.OccupiedBy == null) return 0m;
        var v = spot.OccupiedBy;
        return CalculateFee(ref v);
    }

    public ParkingSpot? FindByPlate(string plate) => _map.FindSpotByPlate(plate);
    public List<(ParkingSpot spot, Vehicle vehicle)> GetAllParked()
    {
        var result = new List<(ParkingSpot, Vehicle)>();
        foreach (var spot in _map.GetOccupiedSpots())
            if (spot.OccupiedBy != null)
                result.Add((spot, spot.OccupiedBy));
        return result;
    }

    public decimal TotalCollected() => _history.Sum(t => t.AmountPaid);
}
