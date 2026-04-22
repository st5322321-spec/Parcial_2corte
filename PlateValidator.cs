using System.Text.RegularExpressions;

namespace Parqueo.Utils;

public static class PlateValidator
{
    private static readonly Regex _carPattern  =
        new(@"^[A-Z]{3}[-]?[0-9]{3}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex _motoPattern =
        new(@"^[A-Z]{3}[-]?[0-9]{2}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static bool IsValid(string plate)
    {
        string normalized = Normalize(plate);
        return _carPattern.IsMatch(normalized) || _motoPattern.IsMatch(normalized);
    }

    public static bool IsCarOrTruck(string plate)
        => _carPattern.IsMatch(Normalize(plate));
    public static bool IsMoto(string plate)
        => _motoPattern.IsMatch(Normalize(plate));
    public static string Normalize(string plate)
        => plate.Trim().ToUpper().Replace(" ", "").Replace("-", "");
    public static string Format(string plate)
    {
        string n = Normalize(plate);
        if (n.Length == 6) return $"{n[..3]}-{n[3..]}";
        if (n.Length == 5) return $"{n[..3]}-{n[3..]}";
        return n;
    }

    public static string GetValidationError(string plate)
    {
        if (string.IsNullOrWhiteSpace(plate))
            return "La placa no puede estar vacía.";

        string normalized = Normalize(plate);

        if (normalized.Length < 5 || normalized.Length > 6)
            return $"Longitud inválida ({normalized.Length} caracteres). Debe ser 5 (motos) o 6 (carros/camiones).";

        if (!Regex.IsMatch(normalized[..3], @"^[A-Z]{3}$"))
            return "Los primeros 3 caracteres deben ser letras (ej: ABC).";

        if (!Regex.IsMatch(normalized[3..], @"^[0-9]+$"))
            return "Los últimos caracteres deben ser dígitos (ej: 123 o 12).";

        return "Formato inválido. Use: ABC123 (carro) o ABC12 (moto).";
    }
}
