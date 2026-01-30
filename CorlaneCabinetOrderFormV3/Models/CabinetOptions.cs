namespace CorlaneCabinetOrderFormV3.Models;

public static class CabinetOptions
{
    public static class TopType
    {
        public const string Full = "Full";
        public const string Stretcher = "Stretcher";
    }

    public static class ShelfDepth
    {
        public const string HalfDepth = "Half Depth";
        public const string FullDepth = "Full Depth";
    }

    public static class BackThickness
    {
        public const string QuarterDecimal = "0.25";
        public const string QuarterFraction = "1/4";
        public const string ThreeQuarterDecimal = "0.75";
        public const string ThreeQuarterFraction = "3/4";
    }
}