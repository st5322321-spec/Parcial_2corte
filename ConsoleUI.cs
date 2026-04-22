namespace Parqueo.UI;

public static class ConsoleUI
{
    public const string Reset   = "\u001b[0m";
    public const string Bold    = "\u001b[1m";
    public const string Dim     = "\u001b[2m";

    public const string Black   = "\u001b[30m";
    public const string Red     = "\u001b[31m";
    public const string Green   = "\u001b[32m";
    public const string Yellow  = "\u001b[33m";
    public const string Blue    = "\u001b[34m";
    public const string Magenta = "\u001b[35m";
    public const string Cyan    = "\u001b[36m";
    public const string White   = "\u001b[37m";
    public const string BrightRed     = "\u001b[91m";
    public const string BrightGreen   = "\u001b[92m";
    public const string BrightYellow  = "\u001b[93m";
    public const string BrightBlue    = "\u001b[94m";
    public const string BrightMagenta = "\u001b[95m";
    public const string BrightCyan    = "\u001b[96m";
    public const string BrightWhite   = "\u001b[97m";

    public const string BgBlack   = "\u001b[40m";
    public const string BgRed     = "\u001b[41m";
    public const string BgGreen   = "\u001b[42m";
    public const string BgYellow  = "\u001b[43m";
    public const string BgBlue    = "\u001b[44m";
    public const string BgWhite   = "\u001b[47m";
    public const string BgBrightGreen = "\u001b[102m";
    public const string BgBrightRed   = "\u001b[101m";

    public static void Print(string text, string color = "")
        => Console.Write(color + text + Reset);

    public static void PrintLine(string text, string color = "")
        => Console.WriteLine(color + text + Reset);

    public static void PrintBold(string text, string color = "")
        => Console.Write(Bold + color + text + Reset);

    public static void PrintLineBold(string text, string color = "")
        => Console.WriteLine(Bold + color + text + Reset);

    public static void NewLine(int count = 1)
    {
        for (int i = 0; i < count; i++)
            Console.WriteLine();
    }

    public static void DrawBox(string title, string content, string color = "", int width = 60)
    {
        string top    = "╔" + new string('═', width - 2) + "╗";
        string bottom = "╚" + new string('═', width - 2) + "╝";
        string mid    = "╠" + new string('═', width - 2) + "╣";

        int titlePad = (width - 2 - title.Length) / 2;
        string titleLine = "║" + new string(' ', titlePad) + title +
                           new string(' ', width - 2 - titlePad - title.Length) + "║";

        PrintLine(top, color);
        PrintLine(titleLine, color);
        PrintLine(mid, color);

        foreach (string line in content.Split('\n'))
        {
            string safe = line.Length > width - 4 ? line[..(width - 4)] : line;
            string padded = "║ " + safe.PadRight(width - 4) + " ║";
            PrintLine(padded, color);
        }

        PrintLine(bottom, color);
    }

    public static void DrawLine(int width = 70, char ch = '─', string color = "")
        => PrintLine(new string(ch, width), color);

    public static void DrawSectionTitle(string title, string color = "")
    {
        NewLine();
        int pad = (68 - title.Length) / 2;
        string line = new string('═', pad) + $"[ {title} ]" + new string('═', pad);
        PrintLine(line, Bold + color);
        NewLine();
    }

    public static void DrawMainBanner()
    {
        Console.Clear();
        PrintLine("", Reset);
        PrintLine("  " + BrightGreen + "Sistema de Gestión de Parqueadero" + Reset);
        NewLine();
    }

    public static void PrintSuccess(string msg) =>
        PrintLine($"  ✔  {msg}", BrightGreen);

    public static void PrintError(string msg) =>
        PrintLine($"  ✘  {msg}", BrightRed);

    public static void PrintWarning(string msg) =>
        PrintLine($"  ⚠  {msg}", BrightYellow);

    public static void PrintInfo(string msg) =>
        PrintLine($"  ℹ  {msg}", BrightCyan);

    public static string ReadLine(string prompt, string color = "")
    {
        Print($"  {prompt}: ", BrightCyan);
        Console.ForegroundColor = ConsoleColor.White;
        string? input = Console.ReadLine();
        Console.ResetColor();
        return input?.Trim() ?? "";
    }

    public static string ReadLineUpper(string prompt)
        => ReadLine(prompt).ToUpper().Trim();

    public static void PressAnyKey(string msg = "Presiona cualquier tecla para continuar...")
    {
        NewLine();
        PrintLine($"  {msg}", Dim + White);
        Console.ReadKey(true);
    }
    public static void SpinnerAnimation(string message, int durationMs = 1200)
    {
        char[] spinner = { '|', '/', '─', '\\' };
        int    frames  = durationMs / 100;

        for (int i = 0; i < frames; i++)
        {
            Console.Write($"\r  {BrightYellow}{spinner[i % 4]}{Reset}  {message}  ");
            Thread.Sleep(100);
        }
        Console.Write("\r" + new string(' ', message.Length + 20) + "\r");
    }

    public static void VehicleEntryAnimation(string plate, int spotId)
    {
        string[] frames = {
            "  Entrando al parqueadero",
            "  Entrando al parqueadero",
            "  Entrando al parqueadero",
            "  Entrando al parqueadero",
            "  Entrando al parqueadero",
            "  Entrando al parqueadero",
        };

        foreach (string frame in frames)
        {
            Console.Write($"\r{BrightYellow}{frame}{Reset}");
            Thread.Sleep(160);
        }
        Console.WriteLine();
        PrintSuccess($"Vehículo {plate} estacionado en espacio #{spotId}");
        Thread.Sleep(300);
    }

    public static void PrintTableHeader(params (string label, int width)[] cols)
    {
        Print("  ", Reset);
        foreach (var (label, width) in cols)
            Print($"│ {label.PadRight(width)} ", BrightCyan);
        PrintLine("│", BrightCyan);

        Print("  ", Reset);
        foreach (var (_, width) in cols)
            Print($"├─{new string('─', width)}─", BrightCyan);
        PrintLine("┤", BrightCyan);
    }

    public static void PrintTableRow(string color = "", params (string value, int width)[] cols)
    {
        Print("  ", Reset);
        foreach (var (value, width) in cols)
        {
            string safe = value.Length > width ? value[..width] : value;
            Print($"│ {safe.PadRight(width)} ", color == "" ? White : color);
        }
        PrintLine("│", color == "" ? White : color);
    }

    public static void PrintTableSeparator(params int[] widths)
    {
        Print("  ", Reset);
        foreach (int w in widths)
            Print($"├─{new string('─', w)}─", Dim);
        PrintLine("┤", Dim);
    }
}
