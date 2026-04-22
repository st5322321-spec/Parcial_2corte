using Parqueo.Core;
using Parqueo.Models;
using Parqueo.Utils;

namespace Parqueo.UI;

public class MenuHandler
{
    private readonly ParkingManager _manager;
    private readonly Statistics     _stats;

    public MenuHandler(ParkingManager manager, Statistics stats)
    {
        _manager = manager;
        _stats   = stats;
    }

    public void Run()
    {
        bool running = true;

        while (running)
        {
            ShowMainMenu();
            string option = ConsoleUI.ReadLine("Selecciona una opción");

            switch (option.Trim())
            {
                case "1": ShowMapFull();              break;
                case "2": RegisterEntryFlow();        break;
                case "3": RegisterExitFlow();         break;
                case "4": ShowVehicleListFlow();      break;
                case "5": SearchVehicleFlow();        break;
                case "6": ShowStatisticsFlow();       break;
                case "7": ShowHistoryFlow();          break;
                case "8": running = ExitFlow();       break;
                default:
                    ConsoleUI.PrintWarning("Opción inválida. Selecciona un número del 1 al 8.");
                    ConsoleUI.PressAnyKey();
                    break;
            }
        }
    }

    private void ShowMainMenu()
    {
        Console.Clear();
        ConsoleUI.DrawMainBanner();

        ConsoleUI.Print("  Estado: ", ConsoleUI.Bold + ConsoleUI.BrightWhite);
        ConsoleUI.Print($"  {ConsoleUI.BgGreen}{ConsoleUI.Black} {_stats.FreeSpots} LIBRES {ConsoleUI.Reset}  ");
        ConsoleUI.Print($"  {ConsoleUI.BgBrightRed}{ConsoleUI.White} {_stats.OccupiedSpots} OCUPADOS {ConsoleUI.Reset}  ");
        ConsoleUI.PrintLine($"  Ocupación: {_stats.OccupancyPercent}%  |  Recaudado hoy: ${_stats.TotalCollected:N0} COP",
            ConsoleUI.BrightCyan);

        ConsoleUI.NewLine();
        ConsoleUI.DrawLine(68, '─', ConsoleUI.Dim);
        ConsoleUI.NewLine();

        var options = new[]
        {
            ("1", "Ver Mapa del Parqueadero"),
            ("2", "Registrar Entrada de Vehículo"),
            ("3", "Registrar Salida de Vehículo"),
            ("4", "Ver Vehículos en Parqueadero"),
            ("5", "Buscar Vehículo por Placa"),
            ("6", "Ver Estadísticas"),
            ("7", "Ver Historial de Cobros"),
            ("8", "Salir del Sistema"),
        };

        for (int i = 0; i < options.Length; i++)
        {
            var (num, desc) = options[i];
            ConsoleUI.Print($"    [{num}] ", ConsoleUI.BrightCyan);
            ConsoleUI.PrintLine(desc, ConsoleUI.BrightWhite);
        }

        ConsoleUI.NewLine();
        ConsoleUI.DrawLine(68, '─', ConsoleUI.Dim);
        ConsoleUI.NewLine();
    }

    private void ShowMapFull()
    {
        Console.Clear();
        MapRenderer.RenderMap(_manager.Map);
        ConsoleUI.PressAnyKey();
    }

    private void RegisterEntryFlow()
    {
        Console.Clear();
        ConsoleUI.DrawSectionTitle("REGISTRO DE ENTRADA", ConsoleUI.BrightGreen);
        if (_stats.FreeSpots == 0)
        {
            ConsoleUI.PrintError("¡El parqueadero está LLENO! No hay espacios disponibles.");
            ConsoleUI.PressAnyKey();
            return;
        }

        ConsoleUI.PrintInfo($"Espacios disponibles: {_stats.FreeSpots} de {_stats.TotalSpots}");
        ConsoleUI.NewLine();

        string plate = "";
        while (true)
        {
            plate = ConsoleUI.ReadLineUpper("Ingresa la PLACA del vehículo (Ej: ABC123 o ABC12)");

            if (!PlateValidator.IsValid(plate))
            {
                ConsoleUI.PrintError(PlateValidator.GetValidationError(plate));
                continue;
            }

            plate = PlateValidator.Normalize(plate);
            break;
        }

        ConsoleUI.NewLine();
        ConsoleUI.PrintLine("  Tipo de vehículo:", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine("    [1] Moto     → $1,500/hora", ConsoleUI.Cyan);
        ConsoleUI.PrintLine("    [2] Carro    → $3,000/hora", ConsoleUI.BrightCyan);
        ConsoleUI.PrintLine("    [3] Camión   → $5,000/hora", ConsoleUI.BrightMagenta);
        ConsoleUI.NewLine();

        VehicleType vehicleType;
        while (true)
        {
            string typeOpt = ConsoleUI.ReadLine("Selecciona el tipo");
            vehicleType = typeOpt switch
            {
                "1" => VehicleType.Moto,
                "2" => VehicleType.Carro,
                "3" => VehicleType.Camion,
                _   => (VehicleType)(-1)
            };

            if ((int)vehicleType == -1)
            {
                ConsoleUI.PrintWarning("Opción inválida. Selecciona 1, 2 o 3.");
                continue;
            }
            break;
        }

        ConsoleUI.NewLine();
        ConsoleUI.VehicleEntryAnimation(plate, 0);

        var (success, message, spot) = _manager.RegisterEntry(plate, vehicleType);

        ConsoleUI.NewLine();
        if (success && spot != null)
        {
            ConsoleUI.SpinnerAnimation("Asignando espacio...", 800);
            ConsoleUI.PrintSuccess(message);
            ConsoleUI.NewLine();

            PrintEntryTicket(plate, vehicleType, spot);
        }
        else
        {
            ConsoleUI.PrintError(message);
        }

        ConsoleUI.PressAnyKey();
    }

    private void PrintEntryTicket(string plate, VehicleType type, ParkingSpot spot)
    {
        string typeStr = type switch
        {
            VehicleType.Moto   => "Moto",
            VehicleType.Carro  => "Carro",
            VehicleType.Camion => "Camión",
            _ => "?"
        };

        string rate = type switch
        {
            VehicleType.Moto   => "$1,500 COP/hora",
            VehicleType.Carro  => "$3,000 COP/hora",
            VehicleType.Camion => "$5,000 COP/hora",
            _ => "─"
        };

        ConsoleUI.PrintLine("  TICKET DE ENTRADA", ConsoleUI.BrightGreen);
        ConsoleUI.PrintLine($"  Placa      : {PlateValidator.Format(plate),-22} │", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"  Tipo       : {typeStr,-22} │", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"  Espacio #  : {spot.Id,-22} │", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"  Bloque     : {spot.Block,-22} │", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"  Hora entrada: {DateTime.Now:HH:mm:ss  dd/MM/yyyy,-14} │", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"  Tarifa     : {rate,-22} │", ConsoleUI.BrightYellow);
        ConsoleUI.PrintLine("  ", ConsoleUI.BrightGreen);
    }

    private void RegisterExitFlow()
    {
        Console.Clear();
        ConsoleUI.DrawSectionTitle("REGISTRO DE SALIDA", ConsoleUI.BrightRed);

        if (_stats.OccupiedSpots == 0)
        {
            ConsoleUI.PrintWarning("No hay vehículos en el parqueadero.");
            ConsoleUI.PressAnyKey();
            return;
        }

        ShowParkedVehiclesSummary();

        ConsoleUI.NewLine();
        string plate = ConsoleUI.ReadLineUpper("Ingresa la PLACA del vehículo a retirar");
        plate = PlateValidator.Normalize(plate);

        decimal preview = _manager.PreviewFee(plate);
        if (preview >= 0)
        {
            var spot = _manager.FindByPlate(plate);
            if (spot?.OccupiedBy != null)
            {
                ConsoleUI.NewLine();
                ConsoleUI.PrintLine("  VISTA PREVIA DE COBRO", ConsoleUI.BrightYellow);
                ConsoleUI.PrintLine($"  Placa    : {plate,-24} │", ConsoleUI.White);
                ConsoleUI.PrintLine($"  Tiempo   : {spot.OccupiedBy.GetElapsedTime(),-24} │", ConsoleUI.White);
                ConsoleUI.PrintLine($"  A pagar  : ${preview:N0} COP{new string(' ', Math.Max(0, 17 - $"{preview:N0}".Length))} │",
                    ConsoleUI.BrightYellow);
                ConsoleUI.PrintLine(" ", ConsoleUI.BrightYellow);
                ConsoleUI.NewLine();

                string confirm = ConsoleUI.ReadLine("¿Confirmar salida? (S/N)").ToUpper();
                if (confirm != "S")
                {
                    ConsoleUI.PrintWarning("Operación cancelada.");
                    ConsoleUI.PressAnyKey();
                    return;
                }
            }
        }

        // Registrar salida
        var (success, transaction, message) = _manager.RegisterExit(plate);

        ConsoleUI.NewLine();
        if (success && transaction != null)
        {
            ConsoleUI.SpinnerAnimation("Procesando cobro...", 800);
            ConsoleUI.PrintSuccess(message);
            ConsoleUI.NewLine();
            PrintExitTicket(transaction);

            // Guardar en archivo
            FileManager.AppendTransaction(transaction);
        }
        else
        {
            ConsoleUI.PrintError(message);
        }

        ConsoleUI.PressAnyKey();
    }

    private void PrintExitTicket(Transaction t)
    {
        ConsoleUI.PrintLine("  TICKET DE SALIDA", ConsoleUI.BrightRed);
        ConsoleUI.PrintLine($"  Placa      : {PlateValidator.Format(t.Plate),-22} │", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"  Tipo       : {t.VehicleType,-22} │", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"  Espacio #  : {t.SpotId,-22} │", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"  Entrada    : {t.EntryTime:HH:mm:ss  dd/MM/yyyy,-14} │", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"  Salida     : {t.ExitTime:HH:mm:ss  dd/MM/yyyy,-14} │", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"  Duración   : {t.GetDurationString(),-22} │", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"  TOTAL A PAGAR: ${t.AmountPaid:N0} COP{new string(' ', Math.Max(0, 14 - $"{t.AmountPaid:N0}".Length))} │",
            ConsoleUI.BrightYellow);
    }

    private void ShowVehicleListFlow()
    {
        Console.Clear();
        ConsoleUI.DrawSectionTitle("VEHÍCULOS EN PARQUEADERO", ConsoleUI.BrightCyan);

        var parked = _manager.GetAllParked();

        if (parked.Count == 0)
        {
            ConsoleUI.PrintWarning("No hay vehículos en el parqueadero actualmente.");
            ConsoleUI.PressAnyKey();
            return;
        }

        ConsoleUI.PrintInfo($"Total de vehículos: {parked.Count}");
        ConsoleUI.NewLine();

        ConsoleUI.PrintTableHeader(
            ("Espacio", 7), ("Placa", 8), ("Tipo", 7), ("Bloque", 6),
            ("Hora Entrada", 12), ("Tiempo", 12), ("Est. Cobro", 12));
        for (int i = 0; i < parked.Count; i++)
        {
            var (spot, vehicle) = parked[i];
            decimal preview = _manager.PreviewFee(vehicle.Plate);

            string rowColor = vehicle.Type switch
            {
                VehicleType.Moto   => ConsoleUI.Cyan,
                VehicleType.Carro  => ConsoleUI.BrightWhite,
                VehicleType.Camion => ConsoleUI.BrightMagenta,
                _                  => ConsoleUI.White
            };

            ConsoleUI.PrintTableRow(rowColor,
                ($"#{spot.Id}", 7),
                (PlateValidator.Format(vehicle.Plate), 8),
                (vehicle.Type.ToString(), 7),
                (spot.Block.ToString(), 6),
                (vehicle.EntryTime.ToString("HH:mm:ss"), 12),
                (vehicle.GetElapsedTime(), 12),
                ($"${preview:N0}", 12));
        }

        ConsoleUI.NewLine();
        ConsoleUI.PressAnyKey();
    }

    private void ShowParkedVehiclesSummary()
    {
        var parked = _manager.GetAllParked();
        ConsoleUI.PrintLine("  Vehículos actuales:", ConsoleUI.BrightWhite);

        for (int i = 0; i < parked.Count; i++)
        {
            var (spot, vehicle) = parked[i];
            ConsoleUI.PrintLine(
                $"    #{spot.Id}  {PlateValidator.Format(vehicle.Plate)}  [{vehicle.Type}]  {vehicle.GetElapsedTime()}",
                ConsoleUI.Dim + ConsoleUI.White);
        }
    }

    private void SearchVehicleFlow()
    {
        Console.Clear();
        ConsoleUI.DrawSectionTitle("BÚSQUEDA DE VEHÍCULO", ConsoleUI.BrightMagenta);

        string plate = ConsoleUI.ReadLineUpper("Ingresa la PLACA a buscar");
        plate = PlateValidator.Normalize(plate);

        ConsoleUI.NewLine();
        ConsoleUI.SpinnerAnimation("Buscando en el parqueadero...", 600);

        var spot = _manager.FindByPlate(plate);

        if (spot == null)
        {
            ConsoleUI.PrintWarning($"No se encontró ningún vehículo con placa {PlateValidator.Format(plate)} en el parqueadero.");
            ConsoleUI.PressAnyKey();
            return;
        }

        var vehicle = spot.OccupiedBy!;
        decimal estimatedFee = _manager.PreviewFee(plate);

        ConsoleUI.PrintSuccess($"¡Vehículo encontrado!");
        ConsoleUI.NewLine();

        ConsoleUI.PrintLine("  INFORMACIÓN DEL VEHÍCULO", ConsoleUI.BrightMagenta);
        ConsoleUI.PrintLine($"  Placa      : {PlateValidator.Format(vehicle.Plate),-22} │", ConsoleUI.White);
        ConsoleUI.PrintLine($"  Tipo       : {vehicle.Type,-22} │", ConsoleUI.White);
        ConsoleUI.PrintLine($"  Espacio #  : {spot.Id,-22} │", ConsoleUI.White);
        ConsoleUI.PrintLine($"  Bloque     : {spot.Block,-22} │", ConsoleUI.White);
        ConsoleUI.PrintLine($"  Fila/Col   : ({spot.Row}, {spot.Col})".PadRight(40) + "│", ConsoleUI.White);
        ConsoleUI.PrintLine($"  Entrada    : {vehicle.EntryTime:HH:mm:ss  dd/MM/yyyy,-14} │", ConsoleUI.White);
        ConsoleUI.PrintLine($"  Tiempo     : {vehicle.GetElapsedTime(),-22} │", ConsoleUI.White);
        ConsoleUI.PrintLine($"  Est. cobro : ${estimatedFee:N0} COP{new string(' ', Math.Max(0, 17 - $"{estimatedFee:N0}".Length))} │",
            ConsoleUI.BrightYellow);

        ConsoleUI.NewLine();

        ConsoleUI.PrintInfo($"Ubicación en el mapa: Fila {spot.Row}, Columna {spot.Col}");
        ConsoleUI.NewLine();
        MapRenderer.RenderMiniMap(_manager.Map);

        ConsoleUI.PressAnyKey();
    }

    private void ShowStatisticsFlow()
    {
        Console.Clear();
        ConsoleUI.DrawSectionTitle("ESTADÍSTICAS EN TIEMPO REAL", ConsoleUI.BrightYellow);
        PrintOccupancyBar();

        ConsoleUI.NewLine();

        ConsoleUI.PrintLine("  OCUPACIÓN ACTUAL:", ConsoleUI.Bold + ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"    Total de espacios   : {_stats.TotalSpots}", ConsoleUI.White);
        ConsoleUI.PrintLine($"    Espacios libres     : {_stats.FreeSpots}", ConsoleUI.BrightGreen);
        ConsoleUI.PrintLine($"    Espacios ocupados   : {_stats.OccupiedSpots}", ConsoleUI.BrightRed);
        ConsoleUI.PrintLine($"    % Ocupación         : {_stats.OccupancyPercent}%",
            _stats.OccupancyPercent > 80 ? ConsoleUI.BrightRed : ConsoleUI.BrightYellow);

        ConsoleUI.NewLine();

        ConsoleUI.PrintLine("  VEHÍCULOS POR TIPO (actualmente en parqueo):", ConsoleUI.Bold + ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"    Motos   : {_stats.CountByType(VehicleType.Moto)}", ConsoleUI.Cyan);
        ConsoleUI.PrintLine($"    Carros  : {_stats.CountByType(VehicleType.Carro)}", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"    Camiones: {_stats.CountByType(VehicleType.Camion)}", ConsoleUI.BrightMagenta);

        ConsoleUI.NewLine();

        ConsoleUI.PrintLine("  FINANCIERO (transacciones completadas):", ConsoleUI.Bold + ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"    Total transacciones : {_stats.TotalTransactions}", ConsoleUI.White);
        ConsoleUI.PrintLine($"    Total recaudado     : ${_stats.TotalCollected:N0} COP", ConsoleUI.BrightGreen);
        ConsoleUI.PrintLine($"    Promedio por cobro  : ${_stats.AverageTransaction:N0} COP", ConsoleUI.BrightGreen);
        ConsoleUI.PrintLine($"    Cobros por Motos    : ${_stats.CollectedByType(VehicleType.Moto):N0} COP", ConsoleUI.Cyan);
        ConsoleUI.PrintLine($"    Cobros por Carros   : ${_stats.CollectedByType(VehicleType.Carro):N0} COP", ConsoleUI.BrightWhite);
        ConsoleUI.PrintLine($"    Cobros por Camiones : ${_stats.CollectedByType(VehicleType.Camion):N0} COP", ConsoleUI.BrightMagenta);

        ConsoleUI.NewLine();
        var longest = _stats.GetLongestStay();
        if (longest.HasValue)
        {
            ConsoleUI.PrintLine("  MAYOR PERMANENCIA:", ConsoleUI.Bold + ConsoleUI.BrightWhite);
            ConsoleUI.PrintLine($"    Placa: {longest.Value.plate}   Tiempo: {longest.Value.time}", ConsoleUI.BrightYellow);
        }

        ConsoleUI.NewLine();
        ConsoleUI.PressAnyKey();
    }

    private void PrintOccupancyBar()
    {
        int total    = _stats.TotalSpots;
        int occupied = _stats.OccupiedSpots;
        int barWidth = 50;
        int filled   = total == 0 ? 0 : (int)Math.Round((double)occupied / total * barWidth);

        ConsoleUI.Print("  Ocupación: [", ConsoleUI.BrightWhite);
        ConsoleUI.Print(new string('█', filled), ConsoleUI.BrightRed);
        ConsoleUI.Print(new string('░', barWidth - filled), ConsoleUI.BrightGreen);
        ConsoleUI.PrintLine($"] {_stats.OccupancyPercent}%", ConsoleUI.BrightWhite);
    }

    private void ShowHistoryFlow()
    {
        Console.Clear();
        ConsoleUI.DrawSectionTitle("HISTORIAL DE COBROS", ConsoleUI.BrightCyan);

        var history = _manager.History;

        if (history.Count == 0)
        {
            ConsoleUI.PrintWarning("No hay transacciones registradas aún.");
            ConsoleUI.PressAnyKey();
            return;
        }

        ConsoleUI.PrintInfo($"Total de transacciones: {history.Count}");
        ConsoleUI.NewLine();

        ConsoleUI.PrintTableHeader(
            ("#", 3), ("Placa", 8), ("Tipo", 7), ("Entrada", 10),
            ("Salida", 10), ("Duración", 10), ("Valor", 12));

        for (int i = 0; i < history.Count; i++)
        {
            var t = history[i];
            ConsoleUI.PrintTableRow("",
                (t.Id.ToString(), 3),
                (PlateValidator.Format(t.Plate), 8),
                (t.VehicleType.ToString(), 7),
                (t.EntryTime.ToString("HH:mm:ss"), 10),
                (t.ExitTime.ToString("HH:mm:ss"), 10),
                (t.GetDurationString(), 10),
                ($"${t.AmountPaid:N0}", 12));
        }

        ConsoleUI.NewLine();
        ConsoleUI.PrintLine($"  TOTAL RECAUDADO: ${_stats.TotalCollected:N0} COP",
            ConsoleUI.Bold + ConsoleUI.BrightGreen);

        ConsoleUI.NewLine();
        ConsoleUI.PressAnyKey();
    }

    private bool ExitFlow()
    {
        Console.Clear();
        ConsoleUI.DrawSectionTitle("CIERRE DEL SISTEMA", ConsoleUI.BrightYellow);

        string[] report = _stats.GenerateDayClosingReport();
        ConsoleUI.PrintLine("  REPORTE DE CIERRE DEL DÍA:", ConsoleUI.Bold + ConsoleUI.BrightWhite);
        ConsoleUI.NewLine();

        for (int i = 0; i < report.Length; i++)
            ConsoleUI.PrintLine("    " + report[i], ConsoleUI.White);

        ConsoleUI.NewLine();

        ConsoleUI.SpinnerAnimation("Guardando datos...", 800);

        if (_manager.History.Count > 0)
        {
            var (csvOk, csvPath)       = FileManager.ExportTransactions(_manager.History);
            var (reportOk, reportPath) = FileManager.SaveDayReport(report);

            if (csvOk)    ConsoleUI.PrintSuccess($"Historial exportado: {csvPath}");
            if (reportOk) ConsoleUI.PrintSuccess($"Reporte guardado  : {reportPath}");
        }
        else
        {
            ConsoleUI.PrintInfo("No hay transacciones que exportar.");
        }

        ConsoleUI.NewLine();
        string confirm = ConsoleUI.ReadLine("¿Confirmar salida del sistema? (S/N)").ToUpper();

        if (confirm == "S")
        {
            Console.Clear();
            ConsoleUI.PrintLine("", ConsoleUI.Reset);
            ConsoleUI.PrintLine("  Gracias por usar el sistema!           ", ConsoleUI.BrightCyan);
            ConsoleUI.PrintLine("  Sistema de Parqueadero                  ", ConsoleUI.BrightCyan);
            ConsoleUI.NewLine(2);
            return false;
        }

        return true;
    }
}
