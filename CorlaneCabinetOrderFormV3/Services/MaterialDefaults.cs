namespace CorlaneCabinetOrderFormV3.Services;

/// <summary>
/// Single source of truth for material-related fallback constants.
/// Every service and VM should reference these instead of hardcoding values.
/// </summary>
public static class MaterialDefaults
{
    /// <summary>4×8 sheet = 32 sq ft.</summary>
    public const double DefaultSheetAreaSqFt = 32.0;

    /// <summary>Offline fallback yield (fraction of sheet that becomes usable parts).</summary>
    public const double DefaultYield = 0.78;

    /// <summary>Offline fallback CNC cutting price per sheet.</summary>
    public const decimal DefaultCncPricePerSheet = 60m;

    /// <summary>3/4″ plywood nominal thickness.</summary>
    public const double Thickness34 = 0.75;

    /// <summary>1/4″ plywood nominal thickness.</summary>
    public const double Thickness14 = 0.25;
}