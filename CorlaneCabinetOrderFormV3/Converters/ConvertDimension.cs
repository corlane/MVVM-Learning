//namespace CorlaneCabinetOrderFormV3.Converters;

//public static class ConvertDimension
//{
//    public static double FractionToDouble(string fraction)
//    {
//        double result;
//        if (fraction is not null)
//        {
//            if (double.TryParse(fraction, out result))
//            {
//                return result;
//            }
//            string[] split = fraction.Split(new char[] { ' ', '/' });
//            if (split.Length == 2 || split.Length == 3)
//            {
//                int a, b;
//                if (int.TryParse(split[0], out a) && int.TryParse(split[1], out b))
//                {
//                    if (split.Length == 2)
//                    {
//                        return (double)a / b;
//                    }
//                    int c;
//                    if (int.TryParse(split[2], out c))
//                    {
//                        return a + (double)b / c;
//                    }
//                }
//            }
//        }
//        result = 0;

//        return result;
//    }

//    public static string DoubleToFraction(double dimension)
//    {
//        // This will round DOWN to the nearest 32nd
//        string fraction = "";
//        int intPart = (int)dimension;
//        double decimalPart = (dimension - intPart);
//        if (decimalPart == 0)
//        {
//            fraction = intPart.ToString();
//            return fraction;
//        }
//        double denominator = 32;
//        double numerator = decimalPart * denominator;

//        numerator = (int)numerator;
//        if (numerator % 2 == 0)
//        {
//            do
//            {
//                numerator /= 2;
//                denominator /= 2;
//            }
//            while (numerator % 2 == 0);
//        }
//        if (intPart == 0)
//        {
//            fraction = numerator + "/" + denominator;
//        }
//        else
//        {
//            fraction = intPart + " " + numerator + "/" + denominator;
//        }
//        return fraction;
//    }

//}









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
        // This will round DOWN to the nearest 32nd
        string fraction = "";
        int intPart = (int)dimension;
        double decimalPart = (dimension - intPart);
        if (decimalPart == 0)
        {
            fraction = intPart.ToString();
            return fraction;
        }
        double denominator = 32;
        double numerator = decimalPart * denominator;

        numerator = (int)numerator;

        // If it rounds down to 0/32, there is no fractional component to display.
        if (numerator == 0)
        {
            return intPart.ToString();
        }

        if (numerator % 2 == 0)
        {
            do
            {
                numerator /= 2;
                denominator /= 2;
            }
            while (numerator % 2 == 0);
        }
        if (intPart == 0)
        {
            fraction = numerator + "/" + denominator;
        }
        else
        {
            fraction = intPart + " " + numerator + "/" + denominator;
        }
        return fraction;
    }
}