using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class BaseCabinetViewModel : ObservableValidator
{
    private void ResizeOpeningHeights()
    {
        if (_isResizing || _isMapping) return;

        try
        {
            _isResizing = true;

            var input = BuildLayoutInputs();
            var result = CabinetLayoutCalculator.ComputeFromOpenings(input);
            ApplyLayoutResult(result);

            // Style-specific disable flags
            if (Style == Style1)
                Opening1Disabled = DrwCount == 0;
            else if (Style == Style2)
            {
                Opening1Disabled = DrwCount == 1;
                if (DrwCount >= 2) { Opening1Disabled = false; Opening2Disabled = true; Opening3Disabled = true; }
                if (DrwCount >= 3) Opening2Disabled = false;
                if (DrwCount >= 4) Opening3Disabled = false;
            }

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
        OpeningHeight1 = r.Opening1.ToString();
        OpeningHeight2 = r.Opening2.ToString();
        OpeningHeight3 = r.Opening3.ToString();
        OpeningHeight4 = r.Opening4.ToString();
        DrwFrontHeight1 = r.DrwFront1.ToString();
        DrwFrontHeight2 = r.DrwFront2.ToString();
        DrwFrontHeight3 = r.DrwFront3.ToString();
        DrwFrontHeight4 = r.DrwFront4.ToString();
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

            // Style-specific disable flags
            if (Style == Style1 && DrwCount == 1)
            {
                Opening1Disabled = false;
                DrwFront1Disabled = false;
            }
            else if (Style == Style2)
            {
                if (DrwCount == 1) DrwFront1Disabled = true;
                if (DrwCount >= 2) { DrwFront1Disabled = false; DrwFront2Disabled = true; }
                if (DrwCount >= 3) { DrwFront2Disabled = false; DrwFront3Disabled = true; }
                if (DrwCount >= 4) DrwFront3Disabled = false;
            }

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

            UpdatePreview();
        }
        finally { _isResizing = false; }
    }

    private void ApplyDrawerFrontEqualization()
    {
        if (_isResizing || _isMapping) return;
        if (Style != Style2) return;
        if (DrwCount <= 0) return;
        if (!EqualizeAllDrwFronts && !EqualizeBottomDrwFronts) return;

        double tkHeight = ConvertDimension.FractionToDouble(TKHeight);
        if (!HasTK) tkHeight = 0;
        double height = ConvertDimension.FractionToDouble(Height) - tkHeight;

        double topReveal = ConvertDimension.FractionToDouble(TopReveal);
        double bottomReveal = ConvertDimension.FractionToDouble(BottomReveal);
        double gapWidth = ConvertDimension.FractionToDouble(GapWidth);

        try
        {
            _isResizing = false; // _isResizing = true breaks this, because the openings won't resize

            if (EqualizeAllDrwFronts)
            {
                if (DrwCount <= 0) return;

                double each = CabinetLayoutCalculator.EqualizeAll(height, topReveal, bottomReveal, gapWidth, DrwCount);

                DrwFrontHeight1 = each.ToString();
                DrwFrontHeight2 = each.ToString();
                DrwFrontHeight3 = each.ToString();
                if (DrwCount >= 4) DrwFrontHeight4 = each.ToString();
            }
            else if (EqualizeBottomDrwFronts)
            {
                if (DrwCount <= 1) return;

                double top = ConvertDimension.FractionToDouble(DrwFrontHeight1);
                double eachBottom = CabinetLayoutCalculator.EqualizeBottom(height, topReveal, bottomReveal, gapWidth, DrwCount, top);

                if (DrwCount >= 2) DrwFrontHeight2 = eachBottom.ToString();
                if (DrwCount >= 3) DrwFrontHeight3 = eachBottom.ToString();
                if (DrwCount >= 4) DrwFrontHeight4 = eachBottom.ToString();
            }
        }
        finally
        {
            _isResizing = false;
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

}
