using Parqueo.Core;
using Parqueo.Models;

namespace Parqueo.UI;

public static class MapRenderer
{

    public static void RenderMap(ParkingMap map)
    {
        ConsoleUI.DrawSectionTitle("MAPA DEL PARQUEADERO", ConsoleUI.BrightCyan);

        for (int row = 0; row < ParkingMap.ROWS; row++)
        {
            ConsoleUI.Print($"  {row,2} ", ConsoleUI.Dim + ConsoleUI.White);

            for (int col = 0; col < ParkingMap.COLS; col++)
            {
                ref ParkingSpot cell = ref map.GetCell(row, col);
                RenderCell(ref cell);
            }

            Console.WriteLine();
        }

        Console.WriteLine();
        PrintLegend(map);
    }

    private static void RenderCell(ref ParkingSpot cell)
    {
        switch (cell.CellType)
        {
            case CellType.Entry:
                ConsoleUI.Print($"{ConsoleUI.Bold}{ConsoleUI.BgGreen}{ConsoleUI.Black} EN {ConsoleUI.Reset}");
                break;
            case CellType.Exit:
                ConsoleUI.Print($"{ConsoleUI.Bold}{ConsoleUI.BgRed}{ConsoleUI.White} SA {ConsoleUI.Reset}");
                break;
            case CellType.Road:
                ConsoleUI.Print($"{ConsoleUI.BrightYellow}░░░{ConsoleUI.Reset}");
                break;
            case CellType.Wall:
                ConsoleUI.Print($"{ConsoleUI.Dim}███{ConsoleUI.Reset}");
                break;
            case CellType.Spot:
                RenderSpot(ref cell);
                break;
            case CellType.Divider:
                ConsoleUI.Print($"{ConsoleUI.Dim}▒▒{ConsoleUI.Reset}");
                break;
            default:
                ConsoleUI.Print("  ", ConsoleUI.Reset);
                break;
        }
    }

    private static void RenderSpot(ref ParkingSpot spot)
    {
        if (spot.Status == SpotStatus.Free)
        {
            ConsoleUI.Print($"{ConsoleUI.BgGreen}{ConsoleUI.Black} D {ConsoleUI.Reset}");
        }
        else
        {
            string icon = spot.OccupiedBy?.GetIcon() ?? " X ";
            ConsoleUI.Print($"{ConsoleUI.BgBrightRed}{ConsoleUI.White} {icon} {ConsoleUI.Reset}");
        }
    }

    private static void PrintLegend(ParkingMap map)
    {
        ConsoleUI.PrintLine("  LEYENDA:", ConsoleUI.Bold + ConsoleUI.BrightWhite);
        ConsoleUI.Print($"  {ConsoleUI.BgGreen}{ConsoleUI.Black} D {ConsoleUI.Reset}", ConsoleUI.Reset);
        ConsoleUI.Print(" Espacio libre   ", ConsoleUI.BrightGreen);
        ConsoleUI.Print($"  {ConsoleUI.BgBrightRed}{ConsoleUI.White} C {ConsoleUI.Reset}", ConsoleUI.Reset);
        ConsoleUI.Print(" Carro           ", ConsoleUI.BrightRed);
        ConsoleUI.Print($"  {ConsoleUI.BgBrightRed}{ConsoleUI.White} M {ConsoleUI.Reset}", ConsoleUI.Reset);
        ConsoleUI.Print(" Moto            ", ConsoleUI.BrightRed);
        ConsoleUI.Print($"  {ConsoleUI.BgBrightRed}{ConsoleUI.White} T {ConsoleUI.Reset}", ConsoleUI.Reset);
        ConsoleUI.PrintLine(" Camion/Truck", ConsoleUI.BrightRed);

        ConsoleUI.Print($"  {ConsoleUI.BrightYellow}░░{ConsoleUI.Reset}");
        ConsoleUI.Print(" Via/Carretera   ", ConsoleUI.BrightYellow);
        ConsoleUI.Print($"  {ConsoleUI.BgGreen}{ConsoleUI.Black} EN {ConsoleUI.Reset}");
        ConsoleUI.Print(" Entrada         ", ConsoleUI.BrightGreen);
        ConsoleUI.Print($"  {ConsoleUI.BgRed}{ConsoleUI.White} SA {ConsoleUI.Reset}");
        ConsoleUI.PrintLine(" Salida", ConsoleUI.BrightRed);

        Console.WriteLine();

        int    free     = map.FreeSpots;
        int    occupied = map.OccupiedSpots;
        int    total    = map.TotalSpots;
        double pct      = total == 0 ? 0 : Math.Round((double)occupied / total * 100, 1);

        ConsoleUI.Print($"  Disponibilidad: ", ConsoleUI.BrightWhite);
        ConsoleUI.Print($"{free} libres ", ConsoleUI.BrightGreen);
        ConsoleUI.Print($"/ {occupied} ocupados ", ConsoleUI.BrightRed);
        ConsoleUI.Print($"/ {total} total  ", ConsoleUI.White);
        ConsoleUI.PrintLine($"[{pct}% ocupado]",
            pct > 80 ? ConsoleUI.BrightRed : pct > 50 ? ConsoleUI.BrightYellow : ConsoleUI.BrightGreen);
    }

    public static void RenderMiniMap(ParkingMap map)
    {
        ConsoleUI.PrintLine("  ┌─ MAPA RAPIDO ──────────────────────────────┐", ConsoleUI.BrightCyan);

        for (int row = 0; row < ParkingMap.ROWS; row++)
        {
            ConsoleUI.Print($"  │", ConsoleUI.BrightCyan);

            for (int col = 0; col < ParkingMap.COLS; col++)
            {
                ref ParkingSpot cell = ref map.GetCell(row, col);
                PrintMiniCell(ref cell);
            }

            ConsoleUI.PrintLine("│", ConsoleUI.BrightCyan);
        }

        ConsoleUI.PrintLine("  └────────────────────────────────────────────┘", ConsoleUI.BrightCyan);
    }

    private static void PrintMiniCell(ref ParkingSpot cell)
    {
        switch (cell.CellType)
        {
            case CellType.Entry:   ConsoleUI.Print(" E ", ConsoleUI.BrightGreen);  break;
            case CellType.Exit:    ConsoleUI.Print(" S ", ConsoleUI.BrightRed);    break;
            case CellType.Road:    ConsoleUI.Print(" . ", ConsoleUI.BrightYellow); break;
            case CellType.Wall:    ConsoleUI.Print(" # ", ConsoleUI.Dim);          break;
            case CellType.Spot:
                if (cell.Status == SpotStatus.Free)
                    ConsoleUI.Print(" o ", ConsoleUI.BrightGreen);
                else
                    ConsoleUI.Print(" X ", ConsoleUI.BrightRed);
                break;
            default: ConsoleUI.Print(" ", ConsoleUI.Reset); break;
        }
    }
}
