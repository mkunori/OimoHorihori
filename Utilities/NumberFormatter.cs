using System.Globalization;

namespace OimoHorihori.Utilities;

public static class NumberFormatter
{
    public static string Format(
        double value)
    {
        if (!double.IsFinite(value))
        {
            return "---";
        }

        return value.ToString(
            "0.000e+0",
            CultureInfo.InvariantCulture);
    }
}