namespace Parqueo.Models;

public enum VehicleType
{
    Moto,
    Carro,
    Camion
}
public class Vehicle
{
    public string Plate { get; set; }
    public VehicleType Type { get; set; }
    public DateTime EntryTime { get; set; }
    public int SpotId { get; set; }

    public Vehicle(string plate, VehicleType type, int spotId)
    {
        Plate = plate.ToUpper().Trim();
        Type = type;
        EntryTime = DateTime.Now;
        SpotId = spotId;
    }

    public string GetElapsedTime()
    {
        TimeSpan elapsed = DateTime.Now - EntryTime;
        if (elapsed.TotalHours >= 1)
            return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}min";
        return $"{elapsed.Minutes}min {elapsed.Seconds}s";
    }

    public string GetIcon()
    {
        return Type switch
        {
            VehicleType.Moto   => "M",
            VehicleType.Carro  => "C",
            VehicleType.Camion => "T",
            _                  => "?"
        };
    }

    public override string ToString()
        => $"[{Type}] {Plate} — Entrada: {EntryTime:HH:mm:ss} — Espacio #{SpotId}";
}
