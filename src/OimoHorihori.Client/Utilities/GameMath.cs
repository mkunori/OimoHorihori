namespace OimoHorihori.Utilities;

public static class GameMath
{
    public static double Add(double left, double right)
    {
        if (!double.IsFinite(left) || !double.IsFinite(right))
        {
            return double.MaxValue;
        }

        double result = left + right;
        return double.IsFinite(result)
            ? result : double.MaxValue;
    }

    public static double Multiply(double left, double right)
    {
        if (!double.IsFinite(left) || !double.IsFinite(right))
        {
            return double.MaxValue;
        }

        double result = left * right;

        return double.IsFinite(result) ? result : double.MaxValue;
    }

    public static double Pow(double value, int exponent)
    {
        if (!double.IsFinite(value) || value < 0 || exponent < 0)
        {
            return double.MaxValue;
        }

        double result = Math.Pow(value, exponent);

        return double.IsFinite(result) ? result : double.MaxValue;
    }
}