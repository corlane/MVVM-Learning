namespace CorlaneCabinetOrderFormV3.Services;

/// <summary>
/// Holds the actual adjusted thicknesses for each material species which will get passed thru the DXF pipeline.
/// </summary>

public static class MaterialSpeciesThickness
{
    public static double PrefinishedPly { get; set; } = MaterialDefaults.Thickness34;
    public static double MaplePly { get; set; } = MaterialDefaults.Thickness34;
    public static double RedOakPly { get; set; } = MaterialDefaults.Thickness34;
    public static double WhiteOakPly { get; set; } = MaterialDefaults.Thickness34;
    public static double CherryPly { get; set; } = MaterialDefaults.Thickness34;
    public static double AlderPly { get; set; } = MaterialDefaults.Thickness34;
    public static double MahoganyPly { get; set; } = MaterialDefaults.Thickness34;
    public static double WalnutPly { get; set; } = MaterialDefaults.Thickness34;
    public static double HickoryPly { get; set; } = MaterialDefaults.Thickness34;
    public static double MDF { get; set; } = MaterialDefaults.Thickness34;
    public static double WhiteMelamine { get; set; } = MaterialDefaults.Thickness34;
    public static double BlackMelamine { get; set; } = MaterialDefaults.Thickness34;
    public static double Custom { get; set; } = MaterialDefaults.Thickness34;


    public static double Thickness14Plywood { get; set; } = MaterialDefaults.Thickness14;
}
