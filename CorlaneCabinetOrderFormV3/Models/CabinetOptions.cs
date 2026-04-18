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

        /// <summary>
        /// Build the back thickness list formatted for the user's preferred dimension format.
        /// </summary>
        public static List<string> GetList(string dimensionFormat)
        {
            bool useFraction = string.Equals(dimensionFormat, "Fraction", StringComparison.OrdinalIgnoreCase);
            string thin = useFraction ? QuarterFraction : QuarterDecimal;
            string thick = useFraction ? ThreeQuarterFraction : ThreeQuarterDecimal;
            return [thin, thick];
        }
    }

    // ── Shared combobox option lists ──────────────────────────────────────

    public static readonly IReadOnlyList<string> GrainDirections = ["Horizontal", "Vertical"];

    public static readonly IReadOnlyList<int> DoorCounts = [0, 1, 2];

    public static readonly IReadOnlyList<int> Corner90DoorCounts = [0, 2];

    public static readonly IReadOnlyList<int> ShelfCounts = [0, 1, 2, 3, 4, 5, 6, 7, 8];

    public static readonly IReadOnlyList<string> DrawerStyles =
    [
        "Blum Tandem H/Equivalent Undermount",
        "Accuride/Equivalent Sidemount"
    ];

    public static readonly IReadOnlyList<string> ShelfDepths =
    [
        ShelfDepth.HalfDepth,
        ShelfDepth.FullDepth
    ];

    public static readonly IReadOnlyList<string> TopTypes =
    [
        TopType.Stretcher,
        TopType.Full
    ];
}