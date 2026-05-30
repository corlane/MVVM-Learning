using CorlaneCabinetOrderFormV3.Converters;

namespace CorlaneCabinetOrderFormV3.Models;

/// <summary>
/// Extension methods for cabinet-type-agnostic style detection and dimension extraction.
/// </summary>
internal static class CabinetStyleExtensions
{
    /// <summary>
    /// Checks if the cabinet is a 90° Corner style (either Base or Upper).
    /// Returns false if cabinet is null.
    /// </summary>
    internal static bool IsCorner90(this CabinetModel? cabinet) =>
        cabinet != null && (cabinet.Style == CabinetStyles.Base.Corner90 || cabinet.Style == CabinetStyles.Upper.Corner90);

    /// <summary>
    /// Checks if the cabinet is an Angle Front style (either Base or Upper).
    /// Returns false if cabinet is null.
    /// </summary>
    internal static bool IsAngleFront(this CabinetModel? cabinet) =>
        cabinet != null && (cabinet.Style == CabinetStyles.Base.AngleFront || cabinet.Style == CabinetStyles.Upper.AngleFront);

    /// <summary>
    /// Extracts corner-specific dimensions from either a BaseCabinetModel or UpperCabinetModel.
    /// Both types have LeftBackWidth, RightBackWidth, LeftFrontWidth, RightFrontWidth, LeftDepth, RightDepth properties.
    /// </summary>
    internal static (double LeftBackWidth, double RightBackWidth, double LeftFrontWidth, double RightFrontWidth, double LeftDepth, double RightDepth) 
        GetCornerDimensions(this CabinetModel cabinet)
    {
        if (cabinet is BaseCabinetModel baseCab)
            return (ConvertDimension.FractionToDouble(baseCab.LeftBackWidth),
                    ConvertDimension.FractionToDouble(baseCab.RightBackWidth),
                    ConvertDimension.FractionToDouble(baseCab.LeftFrontWidth),
                    ConvertDimension.FractionToDouble(baseCab.RightFrontWidth),
                    ConvertDimension.FractionToDouble(baseCab.LeftDepth),
                    ConvertDimension.FractionToDouble(baseCab.RightDepth));
        if (cabinet is UpperCabinetModel upperCab)
            return (ConvertDimension.FractionToDouble(upperCab.LeftBackWidth),
                    ConvertDimension.FractionToDouble(upperCab.RightBackWidth),
                    ConvertDimension.FractionToDouble(upperCab.LeftFrontWidth),
                    ConvertDimension.FractionToDouble(upperCab.RightFrontWidth),
                    ConvertDimension.FractionToDouble(upperCab.LeftDepth),
                    ConvertDimension.FractionToDouble(upperCab.RightDepth));
        throw new InvalidOperationException("Corner dimensions require BaseCabinetModel or UpperCabinetModel");
    }

    /// <summary>
    /// Checks if the cabinet has a toe kick (only applies to BaseCabinetModel).
    /// Returns false if cabinet is null.
    /// </summary>
    internal static bool HasToeKick(this CabinetModel? cabinet) =>
        cabinet is BaseCabinetModel baseCab && baseCab.HasTK;
}
