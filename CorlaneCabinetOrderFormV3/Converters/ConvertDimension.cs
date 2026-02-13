namespace CorlaneCabinetOrderFormV3.Converters;

public static class ConvertDimension
{
    public static double FractionToDouble(string fraction)
    {
        double result;
        if (fraction is not null)
        {
            if (double.TryParse(fraction, out result))
            {
                return result;
            }
            string[] split = fraction.Split(new char[] { ' ', '/' });
            if (split.Length == 2 || split.Length == 3)
            {
                int a, b;
                if (int.TryParse(split[0], out a) && int.TryParse(split[1], out b))
                {
                    if (split.Length == 2)
                    {
                        return (double)a / b;
                    }
                    int c;
                    if (int.TryParse(split[2], out c))
                    {
                        // Handle mixed numbers with a possibly negative whole part correctly.
                        // e.g. "-1 1/2" should be -1.5, not -0.5.
                        if (a < 0)
                            return a - (double)b / c;
                        return a + (double)b / c;
                    }
                }
            }
        }
        result = 0;

        return result;
    }

    public static string DoubleToFraction(double dimension)
    {
        // This will round DOWN to the nearest 32nd.
        // Handle sign separately so the fractional component is always positive.
        bool isNegative = dimension < 0;
        double absDim = Math.Abs(dimension);

        int intPart = (int)absDim;
        double decimalPart = (absDim - intPart);
        if (decimalPart == 0)
        {
            return (isNegative ? "-" : "") + intPart.ToString();
        }

        int denominator = 32;
        int numerator = (int)(decimalPart * denominator);

        // If it rounds down to 0/32, there is no fractional component to display.
        if (numerator == 0)
        {
            return (isNegative ? "-" : "") + intPart.ToString();
        }

        // Reduce fraction by dividing out factors of 2.
        while (numerator % 2 == 0 && denominator % 2 == 0)
        {
            numerator /= 2;
            denominator /= 2;
        }

        string fraction;
        if (intPart == 0)
        {
            fraction = numerator + "/" + denominator;
        }
        else
        {
            fraction = intPart + " " + numerator + "/" + denominator;
        }

        if (isNegative)
            return "-" + fraction;

        return fraction;
    }
}