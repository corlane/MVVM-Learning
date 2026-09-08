using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class BaseCabinetViewModel : ObservableValidator
{
    private string? _activeInputProperty;

    private void ResizeOpeningHeights()
    {
        if (_isResizing || _isMapping) return;

        try
        {
            _isResizing = true;

            var input = BuildLayoutInputs();
            var result = CabinetLayoutCalculator.ComputeFromOpenings(input);
            ApplyLayoutResult(result);

            ApplyDrawerFrontEqualization();
            UpdateDisabledFlags();
            UpdatePreview();
        }
        finally { _isResizing = false; }
    }

    private CabinetLayoutCalculator.LayoutInputs BuildLayoutInputs() => new(
        Style, DrwCount,
        ConvertDimension.FractionToDouble(Height),
        ConvertDimension.FractionToDouble(TKHeight),
        HasTK,
        ConvertDimension.FractionToDouble(TopReveal),
        ConvertDimension.FractionToDouble(BottomReveal),
        ConvertDimension.FractionToDouble(GapWidth),
        ConvertDimension.FractionToDouble(OpeningHeight1),
        ConvertDimension.FractionToDouble(OpeningHeight2),
        ConvertDimension.FractionToDouble(OpeningHeight3),
        ConvertDimension.FractionToDouble(OpeningHeight4),
        ConvertDimension.FractionToDouble(DrwFrontHeight1),
        ConvertDimension.FractionToDouble(DrwFrontHeight2),
        ConvertDimension.FractionToDouble(DrwFrontHeight3),
        ConvertDimension.FractionToDouble(DrwFrontHeight4));

    private void ApplyLayoutResult(CabinetLayoutCalculator.LayoutResult r)
    {
        var active = _activeInputProperty;
        if (active != nameof(OpeningHeight1)) OpeningHeight1 = FormatDimension(r.Opening1);
        if (active != nameof(OpeningHeight2)) OpeningHeight2 = FormatDimension(r.Opening2);
        if (active != nameof(OpeningHeight3)) OpeningHeight3 = FormatDimension(r.Opening3);
        if (active != nameof(OpeningHeight4)) OpeningHeight4 = FormatDimension(r.Opening4);
        if (active != nameof(DrwFrontHeight1)) DrwFrontHeight1 = FormatDimension(r.DrwFront1);
        if (active != nameof(DrwFrontHeight2)) DrwFrontHeight2 = FormatDimension(r.DrwFront2);
        if (active != nameof(DrwFrontHeight3)) DrwFrontHeight3 = FormatDimension(r.DrwFront3);
        if (active != nameof(DrwFrontHeight4)) DrwFrontHeight4 = FormatDimension(r.DrwFront4);
    }

    private void ResizeDrwFrontHeights()
    {
        if (_isResizing || _isMapping) return;

        try
        {
            _isResizing = true;

            var input = BuildLayoutInputs();
            var result = CabinetLayoutCalculator.ComputeFromDrawerFronts(input);
            ApplyLayoutResult(result);

            if (EqualizeBottomDrwFronts)
            {
                DrwFront2Disabled = true;
                DrwFront3Disabled = true;
            }
            if (EqualizeAllDrwFronts)
            {
                DrwFront1Disabled = true;
                DrwFront2Disabled = true;
                DrwFront3Disabled = true;
            }

            UpdateDisabledFlags();
            UpdatePreview();
        }
        finally { _isResizing = false; }
    }


    private void ApplyDrawerFrontEqualization()
    {
        if (_isMapping) return;
        if (Style != Style2) return;
        if (DrwCount <= 0) return;
        if (!EqualizeAllDrwFronts && !EqualizeBottomDrwFronts) return;

        double tkHeight = ConvertDimension.FractionToDouble(TKHeight);
        if (!HasTK) tkHeight = 0;
        double height = ConvertDimension.FractionToDouble(Height) - tkHeight;
        double topReveal = ConvertDimension.FractionToDouble(TopReveal);
        double bottomReveal = ConvertDimension.FractionToDouble(BottomReveal);
        double gapWidth = ConvertDimension.FractionToDouble(GapWidth);

        var active = _activeInputProperty;
        bool acquired = !_isResizing;
        _isResizing = true;

        try
        {
            if (EqualizeAllDrwFronts)
            {
                double each = CabinetLayoutCalculator.EqualizeAll(
                    height, topReveal, bottomReveal, gapWidth, DrwCount);

                if (active != nameof(DrwFrontHeight1)) DrwFrontHeight1 = each.ToString();
                if (active != nameof(DrwFrontHeight2)) DrwFrontHeight2 = each.ToString();
                if (active != nameof(DrwFrontHeight3)) DrwFrontHeight3 = each.ToString();
                if (DrwCount >= 4 && active != nameof(DrwFrontHeight4))
                    DrwFrontHeight4 = each.ToString();
            }
            else if (EqualizeBottomDrwFronts)
            {
                if (DrwCount <= 1) return;

                double top = ConvertDimension.FractionToDouble(DrwFrontHeight1);
                double eachBottom = CabinetLayoutCalculator.EqualizeBottom(
                    height, topReveal, bottomReveal, gapWidth, DrwCount, top);

                if (DrwCount >= 2) DrwFrontHeight2 = eachBottom.ToString();
                if (DrwCount >= 3) DrwFrontHeight3 = eachBottom.ToString();
                if (DrwCount >= 4) DrwFrontHeight4 = eachBottom.ToString();
            }

            // Openings used to update via OnChanged → ResizeDrwFrontHeights.
            // That is now blocked, so do it here while still guarded.
            ApplyLayoutResult(CabinetLayoutCalculator.ComputeFromDrawerFronts(BuildLayoutInputs()));
        }
        finally
        {
            if (acquired) _isResizing = false;
        }
    }

    private void RecalculateDrawerLayout()
    {
        if (EqualizeAllDrwFronts || EqualizeBottomDrwFronts)
        {
            ApplyDrawerFrontEqualization();
        }
        else
        {
            ResizeOpeningHeights();
        }
        ResizeDrwFrontHeights();
    }

    private void RecalculateFrontWidth()
    {
        if (_isResizing || _isMapping)
            return;

        if (!string.Equals(Style, Style4, StringComparison.Ordinal))
        {
            FrontWidth = string.Empty;
            return;
        }

        try
        {
            double frontWidth = CabinetLayoutCalculator.ComputeAngleFrontWidth(
                ConvertDimension.FractionToDouble(LeftDepth),
                ConvertDimension.FractionToDouble(RightDepth),
                ConvertDimension.FractionToDouble(LeftBackWidth),
                ConvertDimension.FractionToDouble(RightBackWidth));

            string dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";
            FrontWidth = string.Equals(dimFormat, "Fraction", StringComparison.OrdinalIgnoreCase)
                ? ConvertDimension.DoubleToFraction(frontWidth)
                : frontWidth.ToString("0.####");
        }
        catch
        {
            FrontWidth = string.Empty;
        }
    }

    private void RecalculateBackWidths90()
    {
        double leftBack = ConvertDimension.FractionToDouble(LeftFrontWidth) + ConvertDimension.FractionToDouble(RightDepth);
        double rightBack = ConvertDimension.FractionToDouble(RightFrontWidth) + ConvertDimension.FractionToDouble(LeftDepth);

        bool useFraction = string.Equals(_defaults?.DefaultDimensionFormat, "Fraction", StringComparison.OrdinalIgnoreCase);

        LeftBackWidth90 = useFraction
            ? ConvertDimension.DoubleToFraction(leftBack)
            : leftBack.ToString();

        RightBackWidth90 = useFraction
            ? ConvertDimension.DoubleToFraction(rightBack)
            : rightBack.ToString();
    }


    private void UpdateDisabledFlags()
    {
        if (Style == Style1)
        {
            Opening1Disabled = DrwCount == 0;
            Opening2Disabled = true;
            Opening3Disabled = true;

            DrwFront1Disabled = false;
            DrwFront2Disabled = true;
            DrwFront3Disabled = true;

            if (DrwCount == 1)
            {
                Opening1Disabled = false;
                DrwFront1Disabled = false;
            }
        }
        else if (Style == Style2)
        {
            Opening1Disabled = DrwCount <= 1;
            Opening2Disabled = DrwCount < 3;
            Opening3Disabled = DrwCount < 4;

            DrwFront1Disabled = DrwCount <= 1;
            DrwFront2Disabled = DrwCount < 3;
            DrwFront3Disabled = DrwCount < 4;
        }

        if (EqualizeBottomDrwFronts)
        {
            Opening2Disabled = true;
            Opening3Disabled = true;
            DrwFront2Disabled = true;
            DrwFront3Disabled = true;
            // Opening1 + DrwFront1 stay as set above
        }

        if (EqualizeAllDrwFronts)
        {
            Opening1Disabled = true;
            Opening2Disabled = true;
            Opening3Disabled = true;
            DrwFront1Disabled = true;
            DrwFront2Disabled = true;
            DrwFront3Disabled = true;
        }
    }
    private string FormatDimension(double value)
    {
        string dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";
        return string.Equals(dimFormat, "Fraction", StringComparison.OrdinalIgnoreCase)
            ? ConvertDimension.DoubleToFraction(value)
            : value.ToString("0.####");
    }
}
