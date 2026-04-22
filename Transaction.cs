namespace Parqueo.Models;

public class Transaction
{
    public int    Id        { get; set; }
    public string Plate     { get; set; }
    public VehicleType VehicleType { get; set; }
    public DateTime EntryTime  { get; set; }
    public DateTime ExitTime   { get; set; }
    public TimeSpan Duration   { get; set; }
    public decimal  AmountPaid { get; set; }
    public int      SpotId     { get; set; }

    public Transaction(int id, Vehicle vehicle, decimal amountPaid)
    {
        Id          = id;
        Plate       = vehicle.Plate;
        VehicleType = vehicle.Type;
        EntryTime   = vehicle.EntryTime;
        ExitTime    = DateTime.Now;
        Duration    = ExitTime - EntryTime;
        AmountPaid  = amountPaid;
        SpotId      = vehicle.SpotId;
    }

    public string GetDurationString()
    {
        if (Duration.TotalHours >= 1)
            return $"{(int)Duration.TotalHours}h {Duration.Minutes}min";
        if (Duration.TotalMinutes >= 1)
            return $"{Duration.Minutes}min {Duration.Seconds}s";
        return $"{Duration.Seconds}s";
    }

    public string ToCsvLine()
        => $"{Id},{Plate},{VehicleType},{EntryTime:yyyy-MM-dd HH:mm:ss}," +
           $"{ExitTime:yyyy-MM-dd HH:mm:ss},{GetDurationString()},{AmountPaid},{SpotId}";

    public static string CsvHeader()
        => "Id,Placa,TipoVehiculo,Entrada,Salida,Duracion,ValorPagado,Espacio";
}
