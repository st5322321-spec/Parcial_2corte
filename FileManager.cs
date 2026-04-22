using Parqueo.Models;

namespace Parqueo.Utils;

public static class FileManager
{
    private static readonly string _dataDir =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

    private static string TransactionsFile =>
        Path.Combine(_dataDir, $"historial_{DateTime.Now:yyyy-MM-dd}.csv");

    private static string DayReportFile =>
        Path.Combine(_dataDir, $"reporte_{DateTime.Now:yyyy-MM-dd}.txt");

    public static (bool success, string path) ExportTransactions(IReadOnlyList<Transaction> transactions)
    {
        try
        {
            EnsureDataDir();

            using StreamWriter writer = new(TransactionsFile, append: false);
            writer.WriteLine(Transaction.CsvHeader());

            for (int i = 0; i < transactions.Count; i++)
                writer.WriteLine(transactions[i].ToCsvLine());

            return (true, TransactionsFile);
        }
        catch (Exception ex)
        {
            return (false, $"Error al exportar: {ex.Message}");
        }
    }

    public static bool AppendTransaction(Transaction transaction)
    {
        try
        {
            EnsureDataDir();
            bool fileExists = File.Exists(TransactionsFile);

            using StreamWriter writer = new(TransactionsFile, append: true);
            if (!fileExists)
                writer.WriteLine(Transaction.CsvHeader());

            writer.WriteLine(transaction.ToCsvLine());
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static (bool success, string path) SaveDayReport(string[] reportLines)
    {
        try
        {
            EnsureDataDir();

            using StreamWriter writer = new(DayReportFile, append: false);
            writer.WriteLine($"REPORTE DE CIERRE — PARQUEADERO");
            writer.WriteLine(new string('=', 50));

            for (int i = 0; i < reportLines.Length; i++)
                writer.WriteLine(reportLines[i]);

            writer.WriteLine(new string('=', 50));
            writer.WriteLine($"Generado: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            return (true, DayReportFile);
        }
        catch (Exception ex)
        {
            return (false, $"Error al guardar reporte: {ex.Message}");
        }
    }

    private static void EnsureDataDir()
    {
        if (!Directory.Exists(_dataDir))
            Directory.CreateDirectory(_dataDir);
    }

    public static string GetDataDirectory() => _dataDir;
}
