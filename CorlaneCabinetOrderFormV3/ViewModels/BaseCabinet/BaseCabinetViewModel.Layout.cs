using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.ViewModels;

/*
 * =============================================================================
 * DRAWER / OPENING LAYOUT ENGINE
 * =============================================================================
 *
 * This class keeps opening heights and drawer-front heights in lockstep with
 * cabinet Height, toe-kick, reveals, gaps, Style, and DrwCount.
 *
 * There are TWO sources of truth, depending on what the user last edited:
 *
 *   Openings  -> CabinetLayoutCalculator.ComputeFromOpenings
 *               (user typed an opening; derive fronts, then remaining openings)
 *
 *   Fronts    -> CabinetLayoutCalculator.ComputeFromDrawerFronts
 *               (user typed a drawer front; derive openings)
 *
 * Equalization (Style2 only) is a third overlay that rewrites fronts, then
 * re-derives openings from those fronts.
 *
 *
 * -----------------------------------------------------------------------------
 * WHY THE GUARDS EXIST
 * -----------------------------------------------------------------------------
 *
 * Every OpeningHeight* / DrwFrontHeight* is a TwoWay string with
 * UpdateSourceTrigger=PropertyChanged. Each keystroke:
 *
 *   TextBox -> VM property -> OnXxxChanged -> resize -> write OTHER properties
 *           -> those OnXxxChanged fire too
 *
 * Without guards that loop (a) overwrites the field being typed ("12." becomes
 * "12", "1 1/" becomes "0") and (b) re-enters resize forever.
 *
 * Three flags, three jobs — do not collapse them:
 *
 *   _isMapping
 *     Bulk load from a model / defaults. Every OnChanged returns immediately.
 *     Set true around the whole mapping block, false in finally.
 *
 *   _isResizing
 *     Nestable lock for "we are applying a calculated layout." OnChanged
 *     handlers MUST return when this is true so assigning Opening2 does not
 *     start a second resize that clears _activeInputProperty.
 *
 *     Take it with:
 *         bool acquired = !_isResizing;
 *         _isResizing = true;
 *         try { ... }
 *         finally { if (acquired) _isResizing = false; }
 *
 *     Outer caller keeps the lock; inner helper (equalization) must NOT clear
 *     it. Historically `_isResizing = false` inside equalization was used so
 *     that assigning fronts would chain into ResizeDrwFrontHeights via
 *     OnChanged. That chain is what ate in-progress text. Call the calculator
 *     explicitly instead.
 *
 *   _activeInputProperty
 *     nameof() of the field the user is currently typing. ApplyLayoutResult
 *     MUST skip that field or the TextBox loses partial input.
 *
 *     ALWAYS snapshot before assigning:
 *         var active = _activeInputProperty;
 *         if (active != nameof(OpeningHeight1)) OpeningHeight1 = ...
 *
 *     Reading _activeInputProperty on each line is wrong: setting Opening1
 *     used to fire OnOpeningHeight1Changed, which set then CLEARED
 *     _activeInputProperty, so later lines (fronts) overwrote the field
 *     being typed. Opening1 appeared fine only because it was assigned first.
 *
 *
 * -----------------------------------------------------------------------------
 * DATA FLOW (happy path)
 * -----------------------------------------------------------------------------
 *
 * A) User edits an OPENING (OpeningHeight1..4)
 *
 *    OnOpeningHeightNChanged
 *      if (_isMapping || _isResizing) return;
 *      _activeInputProperty = nameof(OpeningHeightN);
 *      ResizeOpeningHeights();
 *      _activeInputProperty = null;
 *
 *    ResizeOpeningHeights
 *      take _isResizing
 *      input  = BuildLayoutInputs()          // current strings -> doubles
 *      result = ComputeFromOpenings(input)
 *      ApplyLayoutResult(result)             // skip active opening
 *      ApplyDrawerFrontEqualization()        // no-op if flags off / not Style2
 *      UpdateDisabledFlags()
 *      UpdatePreview()
 *
 * B) User edits a DRAWER FRONT (DrwFrontHeight1..4)
 *
 *    OnDrwFrontHeightNChanged
 *      if (_isMapping || _isResizing) return;
 *      _activeInputProperty = nameof(DrwFrontHeightN);
 *      ApplyDrawerFrontEqualization();       // Front1 also equalizes bottoms
 *      ResizeDrwFrontHeights();
 *      _activeInputProperty = null;
 *
 *    ResizeDrwFrontHeights
 *      take _isResizing
 *      result = ComputeFromDrawerFronts(BuildLayoutInputs())
 *      ApplyLayoutResult(result)             // skip active front
 *      UpdateDisabledFlags()
 *      UpdatePreview()
 *
 * C) Height / TK / reveals / gaps / Style / DrwCount change
 *
 *    RecalculateDrawerLayout()
 *      if EqualizeAll or EqualizeBottom -> ApplyDrawerFrontEqualization()
 *      else                             -> ResizeOpeningHeights()
 *      ResizeDrwFrontHeights()           // disable flags + preview live here
 *
 *
 * -----------------------------------------------------------------------------
 * EQUALIZATION (Style2 only)
 * -----------------------------------------------------------------------------
 *
 * ApplyDrawerFrontEqualization
 *   abort if _isMapping, Style != Style2, DrwCount <= 0, or both flags false
 *   nestable _isResizing lock (do NOT return early on _isResizing)
 *
 *   EqualizeAllDrwFronts
 *     each front = EqualizeAll(height, reveals, gap, count)
 *     write every front EXCEPT _activeInputProperty
 *     (if the user is typing Opening1, active is Opening1, so ALL fronts
 *      get written — that is intended)
 *
 *   EqualizeBottomDrwFronts
 *     Front1 is the master; Fronts 2..N = EqualizeBottom(..., top: Front1)
 *     NEVER write Front1 here (that is the field the user edits)
 *     abort if DrwCount <= 1
 *
 *   Then ALWAYS:
 *     ApplyLayoutResult(ComputeFromDrawerFronts(...))
 *     so openings catch up without going through OnChanged.
 *
 * Do not re-enable _isResizing=false to "let openings resize." That was the
 * overwrite bug. The explicit ComputeFromDrawerFronts call replaces the chain.
 *
 *
 * -----------------------------------------------------------------------------
 * DISABLE FLAGS  (UpdateDisabledFlags — assign EVERY flag EVERY time)
 * -----------------------------------------------------------------------------
 *
 * Call from BOTH ResizeOpeningHeights and ResizeDrwFrontHeights, AND from
 * OnEqualize*Changed / OnStyleChanged / OnDrwCountChanged / end of mapping.
 * Stale flags are what made Opening2 look "stuck enabled."
 *
 * Style1 (base, no equalize):
 *   Opening1 disabled only when DrwCount == 0
 *   Opening2, Opening3 always disabled
 *   Front1 enabled; Front2, Front3 disabled
 *   DrwCount == 1: Opening1 and Front1 enabled
 *
 * Style2 (last used opening/front is computed remainder):
 *   count 1: all openings + Front1 locked
 *   count 2: Opening1 / Front1 open; 2 and 3 locked
 *   count 3: Opening1–2 / Front1–2 open; 3 locked
 *   count 4: Opening1–3 / Front1–3 open (4 is always IsReadOnly in XAML)
 *
 * Overlay (after the count table, never re-enables):
 *   EqualizeBottomDrwFronts
 *     only Opening1 (and Front1, if count >= 2) stay editable
 *     Opening2, Opening3, Front2, Front3 forced disabled
 *   EqualizeAllDrwFronts   (wins if both are on — apply last)
 *     ALL openings and ALL fronts disabled; everything is computed
 *
 * XAML uses IsReadOnly="{Binding Opening1Disabled}" (NOT IsEnabled).
 * true = cannot type. No invert converter. Opening4 / Front4 are
 * IsReadOnly="True" in XAML permanently.
 *
 *
 * -----------------------------------------------------------------------------
 * DIMENSION FORMAT (decimal vs fraction)
 * -----------------------------------------------------------------------------
 *
 * VM properties are STRINGS. Calculator talks DOUBLES.
 *
 *   in  : ConvertDimension.FractionToDouble  ("12 1/2" | "12.5" | "1/2")
 *   out : FormatDimension(double)
 *           Fraction -> ConvertDimension.DoubleToFraction (32nds, round down)
 *           Decimal  -> ToString("0.####")
 *         NEVER assign raw double.ToString() — that is how fractions vanished.
 *
 * Two writers, two jobs:
 *
 *   1. ApplyLayoutResult / equalization / mapping
 *        FormatDimension on every field EXCEPT _activeInputProperty.
 *
 *   2. Behaviors.DimensionAutoFormat (XAML attached, LostFocus)
 *        Formats THE box that lost focus (the skipped active field) and
 *        UpdateSource()s the VM. That UpdateSource triggers OnChanged ->
 *        resize, which is fine: active is skipped, siblings get
 *        FormatDimension so they do not snap back to decimals.
 *
 * While typing we must NOT format (would kill "12." and "1 1/").
 * Incomplete strings that parse to 0 (and are not "0") are left alone by
 * DimensionAutoFormat. FractionToDouble does not Trim(); leading spaces
 * on mixed numbers can fail parse.
 *
 * Format fallbacks must match:
 *   VM FormatDimension            : _defaults?.DefaultDimensionFormat ?? "Decimal"
 *   DimensionAutoFormat           : service value, else "Fraction"
 * Align these if settings fail to resolve.
 *
 *
 * -----------------------------------------------------------------------------
 * BUILDING INPUTS
 * -----------------------------------------------------------------------------
 *
 * BuildLayoutInputs() reads the CURRENT property strings every time.
 * After equalization writes fronts, it must be called AGAIN before
 * ComputeFromDrawerFronts — do not reuse a pre-equalize snapshot.
 *
 *
 * -----------------------------------------------------------------------------
 * TROUBLESHOOTING CHEAT SHEET
 * -----------------------------------------------------------------------------
 *
 * Cannot type "." or " " or "/" in a box
 *   Something is writing that property on every keystroke.
 *   Check: _activeInputProperty snapshot, OnChanged missing `_isResizing`
 *   return, equalization writing the active front, .ToString() instead of skip.
 *
 * Opening change does not equalize fronts
 *   ApplyDrawerFrontEqualization must be called EXPLICITLY from
 *   ResizeOpeningHeights. OnDrwFrontHeight1Changed will NOT run during
 *   _isResizing (by design).
 *
 * Equalize flag on but openings do not move
 *   Equalization must end with ComputeFromDrawerFronts + ApplyLayoutResult,
 *   still inside the _isResizing lock. Do not set _isResizing = false to
 *   "force" OnChanged.
 *
 * Disable flags ignored / Opening2 still editable
 *   1. Count table ENABLES Opening2 at DrwCount >= 3. Overlay equalize
 *      flags after that table.
 *   2. UpdateDisabledFlags must run on flag/style/count change, not only
 *      on height resize.
 *   3. XAML is IsReadOnly, not IsEnabled. Property must be [ObservableProperty].
 *
 * Left a box, format did not stick / siblings went decimal
 *   ApplyLayoutResult is using .ToString() instead of FormatDimension.
 *   DimensionAutoFormat only touches the box that lost focus.
 *
 * Recursion / stack overflow on one keystroke
 *   OnChanged is missing `if (_isMapping || _isResizing) return`.
 *   Equalization is clearing _isResizing before assigning fronts.
 *
 * First load shows raw doubles
 *   Mapping path must FormatDimension too; _isMapping must wrap the
 *   whole load so OnChanged does not fight it. Then UpdateDisabledFlags().
 *
 * =============================================================================
 */


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
