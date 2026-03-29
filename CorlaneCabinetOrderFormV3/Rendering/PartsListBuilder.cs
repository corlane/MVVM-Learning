using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class PartsListBuilder
{
    private const double Mt34 = MaterialDefaults.Thickness34;
    private const double Mt14 = MaterialDefaults.Thickness14;

    internal static List<PartListEntry> Build(IEnumerable<CabinetModel> cabinets)
    {
        var entries = new List<PartListEntry>();
        int index = 1;
        foreach (var cab in cabinets)
        {
            string label = FormatLabel(cab, index++);
            string species = ResolveSpecies(cab.Species, cab.CustomSpecies);

            switch (cab)
            {
                case BaseCabinetModel b: AddBaseParts(b, label, species, entries); break;
                case UpperCabinetModel u: AddUpperParts(u, label, species, entries); break;
                case FillerModel f: AddFillerParts(f, label, species, entries); break;
                case PanelModel p: AddPanelParts(p, label, species, entries); break;
            }
        }
        return entries;
    }

    // ── Formatting helpers ─────────────────────────────────────────────

    private static string Fmt(double inches) => ConvertDimension.DoubleToFraction(inches);

    private static string FormatLabel(CabinetModel cab, int index)
    {
        string name = !string.IsNullOrWhiteSpace(cab.Name) ? cab.Name : cab.CabinetType;
        string style = !string.IsNullOrWhiteSpace(cab.Style) ? $" ({cab.Style})" : "";
        int qty = Math.Max(1, cab.Qty);
        return $"#{index} — {name}{style} × {qty}";
    }

    private static string ResolveSpecies(string? species, string? customSpecies)
    {
        var s = (species ?? "").Trim();
        if (string.Equals(s, "Custom", StringComparison.OrdinalIgnoreCase))
        {
            var custom = (customSpecies ?? "").Trim();
            return string.IsNullOrWhiteSpace(custom) ? "Custom" : custom;
        }
        return string.IsNullOrWhiteSpace(s) ? "" : s;
    }

    private static void Add(List<PartListEntry> entries, string label, string name, int qty,
        string species, double lengthIn, double widthIn, double thicknessIn, string notes = "")
    {
        entries.Add(new PartListEntry
        {
            CabinetLabel = label,
            PartName = name,
            Qty = qty,
            Species = species,
            Length = Fmt(lengthIn),
            Width = Fmt(widthIn),
            Thickness = Fmt(thicknessIn),
            Notes = notes,
        });
    }

    /// <summary>For L-shaped / pentagonal parts where simple L×W doesn't apply.</summary>
    private static void AddComplex(List<PartListEntry> entries, string label, string name,
        int qty, string species, double thicknessIn, string notes)
    {
        entries.Add(new PartListEntry
        {
            CabinetLabel = label,
            PartName = name,
            Qty = qty,
            Species = species,
            Length = "—",
            Width = "—",
            Thickness = Fmt(thicknessIn),
            Notes = notes,
        });
    }

    /// <summary>Returns (lengthAlongGrain, widthAcrossGrain).</summary>
    private static (double length, double width) ByGrain(double heightDim, double widthDim, string grainDir)
    {
        return string.Equals(grainDir, "Horizontal", StringComparison.OrdinalIgnoreCase)
            ? (widthDim, heightDim)
            : (heightDim, widthDim);
    }

    // ────────────────────────────────────────────────────────────────────
    //  BASE CABINETS
    // ────────────────────────────────────────────────────────────────────

    private static void AddBaseParts(BaseCabinetModel b, string label, string species,
        List<PartListEntry> entries)
    {
        var dim = BaseCabinetDimensions.From(b);

        if (b.Style == CabinetStyles.Base.Standard || b.Style == CabinetStyles.Base.Drawer)
            AddBaseStandardParts(b, dim, label, species, entries);
        else if (b.Style == CabinetStyles.Base.Corner90)
            AddBaseCorner90Parts(b, dim, label, species, entries);
        else if (b.Style == CabinetStyles.Base.AngleFront)
            AddBaseAngleFrontParts(b, dim, label, species, entries);
    }

    private static void AddBaseStandardParts(BaseCabinetModel b, BaseCabinetDimensions dim,
        string label, string species, List<PartListEntry> entries)
    {
        double width = dim.Width, height = dim.Height, depth = dim.Depth;
        double backThk = dim.BackThickness;
        double iw = dim.InteriorWidth, id = dim.InteriorDepth, ih = dim.InteriorHeight;
        double tkH = dim.TKHeight, sd = dim.ShelfDepth;

        string tkNote = b.HasTK ? ", TK notch" : "";

        // End panels (vertical grain: length=height, width=depth)
        Add(entries, label, "Left End", 1, species, height, depth, Mt34, "Vertical" + tkNote);
        Add(entries, label, "Right End", 1, species, height, depth, Mt34, "Vertical" + tkNote);

        // Deck
        Add(entries, label, "Deck", 1, species, iw, depth, Mt34, "Horizontal");

        // Top
        if (string.Equals(b.TopType, CabinetOptions.TopType.Full, StringComparison.OrdinalIgnoreCase))
        {
            Add(entries, label, "Top", 1, species, iw, depth, Mt34, "Horizontal");
        }
        else
        {
            Add(entries, label, "Top Stretcher (Front)", 1, species, iw, 6, Mt34, "Horizontal");
            Add(entries, label, "Top Stretcher (Back)", 1, species, iw, 3, Mt34, "Horizontal");
        }

        // Toekick
        if (b.HasTK)
            Add(entries, label, "Toekick", 1, species, iw, tkH - 0.5, Mt34, "Horizontal");

        // Back
        if (backThk == 0.75)
        {
            Add(entries, label, "Back", 1, species, ih, iw, Mt34, "Vertical");
        }
        else
        {
            Add(entries, label, "Back", 1, "PFP 1/4", height - tkH, width, Mt14, "Vertical");
            Add(entries, label, "Nailer", 1, species, iw, 6, Mt34, "Horizontal");
        }

        // Shelves (standard style only)
        if (b.ShelfCount > 0 && b.Style != CabinetStyles.Base.Drawer)
            Add(entries, label, "Shelf", b.ShelfCount, species, iw - 0.125, sd, Mt34, "Horizontal");

        // Drawer stretchers
        int stretcherCount = 0;
        if (b.Style == CabinetStyles.Base.Standard && b.DrwCount == 1) stretcherCount = 1;
        else if (b.Style == CabinetStyles.Base.Drawer) stretcherCount = Math.Max(0, b.DrwCount - 1);

        if (stretcherCount > 0)
        {
            bool isTrash = b.TrashDrawer && b.Style == CabinetStyles.Base.Standard;
            double stretcherW = isTrash ? id : 6;
            Add(entries, label, "Drawer Stretcher", stretcherCount, species, iw, stretcherW, Mt34, "Horizontal");
        }

        // Sink stretcher
        if (b.SinkCabinet)
            Add(entries, label, "Sink Stretcher", 1, species, iw, dim.Opening1Height, Mt34, "Horizontal");

        // Doors
        if (b.DoorCount > 0 && b.Style != CabinetStyles.Base.Drawer)
        {
            string doorSp = CabinetBuildHelpers.ResolveDoorSpeciesForTotals(b.DoorSpecies, b.CustomDoorSpecies);
            string gd = b.DoorGrainDir ?? "Vertical";
            double doorSR = (dim.DoorLeftReveal + dim.DoorRightReveal) / 2;
            double doorW = width - (doorSR * 2);
            double doorH = height - dim.DoorTopReveal - dim.DoorBottomReveal - tkH;

            if (b.DrwCount == 1)
                doorH = height - dim.Opening1Height - Mt34 - (Mt34 / 2) - (dim.BaseDoorGap / 2) - dim.DoorBottomReveal - tkH;

            if (b.DoorCount == 2)
                doorW = (doorW / 2) - (dim.BaseDoorGap / 2);

            var (len, wid) = ByGrain(doorH, doorW, gd);
            Add(entries, label, "Door", b.DoorCount, doorSp, len, wid, Mt34, $"Grain: {gd}");
        }

        // Drawer fronts
        if (b.DrwCount > 0)
        {
            string doorSp = CabinetBuildHelpers.ResolveDoorSpeciesForTotals(b.DoorSpecies, b.CustomDoorSpecies);
            string gd = b.DrwFrontGrainDir ?? "Vertical";
            double doorSR = (dim.DoorLeftReveal + dim.DoorRightReveal) / 2;
            double frontW = width - (doorSR * 2);
            double[] fh = [dim.DrwFront1Height, dim.DrwFront2Height, dim.DrwFront3Height, dim.DrwFront4Height];

            for (int i = 0; i < Math.Min(4, b.DrwCount); i++)
            {
                if (fh[i] <= 0) continue;
                var (len, wid) = ByGrain(fh[i], frontW, gd);
                Add(entries, label, $"Drawer Front {i + 1}", 1, doorSp, len, wid, Mt34, $"Grain: {gd}");
            }
        }

        // Drawer boxes
        if (b.DrwCount > 0)
        {
            double dbxW = iw, dbxD = dim.DrawerBoxDepth;
            double topSp = 0, botSp = 0;
            if (b.DrwStyle?.Contains("Blum") == true) { dbxW -= 0.4; topSp = 0.375; botSp = 0.5906; }
            else if (b.DrwStyle?.Contains("Accuride") == true) { dbxW -= 1.0; topSp = 0.5; botSp = 0.5; }

            double[] oh = [dim.Opening1Height, dim.Opening2Height, dim.Opening3Height, dim.Opening4Height];
            for (int i = 0; i < Math.Min(4, b.DrwCount); i++)
            {
                double dbxH = oh[i] - topSp - botSp;
                if (dbxH <= 0) continue;
                AddDrawerBoxSubParts(entries, label, $"Drawer Box {i + 1}", dbxW, dbxH, dbxD);
            }
        }

        // Rollouts
        if (b.IncRollouts && b.RolloutCount > 0)
        {
            double roW = iw, roH = 4, roD = dim.DrawerBoxDepth;
            if (b.RolloutStyle?.Contains("Blum") == true) roW -= 0.4;
            else if (b.RolloutStyle?.Contains("Accuride") == true) roW -= 1.0;
            roW -= 1.0 * b.DoorCount; // mount bracket spacing

            for (int r = 0; r < b.RolloutCount; r++)
                AddDrawerBoxSubParts(entries, label, "Rollout", roW, roH, roD);
        }

        // Trash drawer
        if (b.TrashDrawer)
        {
            double tdW = iw;
            if (b.DrwStyle?.Contains("Blum") == true) tdW -= 0.4;
            else if (b.DrwStyle?.Contains("Accuride") == true) tdW -= 1.0;
            AddDrawerBoxSubParts(entries, label, "Trash Drawer", tdW, 12, dim.DrawerBoxDepth);
        }
    }

    // ── Corner 90° Base ──

    private static void AddBaseCorner90Parts(BaseCabinetModel b, BaseCabinetDimensions dim,
        string label, string species, List<PartListEntry> entries)
    {
        double height = dim.Height;
        double lf = dim.LeftFrontWidth, rf = dim.RightFrontWidth;
        double ld = dim.LeftDepth, rd = dim.RightDepth;
        double tkH = dim.TKHeight;
        string tkNote = b.HasTK ? ", TK notch" : "";

        // End panels
        Add(entries, label, "Left End", 1, species, height, ld, Mt34, "Vertical" + tkNote);
        Add(entries, label, "Right End", 1, species, height, rd, Mt34, "Vertical" + tkNote);

        // Deck & Top (L-shaped)
        string lNote = $"L-shape — LF: {Fmt(lf - Mt34)}, RF: {Fmt(rf - Mt34)}, LD: {Fmt(ld - 2 * Mt34)}, RD: {Fmt(rd - 2 * Mt34)}";
        AddComplex(entries, label, "Deck", 1, species, Mt34, lNote);
        AddComplex(entries, label, "Top", 1, species, Mt34, lNote);

        // Left back
        double lbW = lf + rd - 2 * Mt34;
        Add(entries, label, "Left Back", 1, species, height, lbW, Mt34, "Vertical" + (b.HasTK ? ", TK notch" : ""));

        // Right back
        double rbW = ld + rf - 3 * Mt34;
        Add(entries, label, "Right Back", 1, species, height - tkH, rbW, Mt34, "Vertical");

        // Toekicks
        if (b.HasTK)
        {
            double tkD = dim.TKDepth;
            Add(entries, label, "Toekick (Left)", 1, species, lf - Mt34 + tkD, tkH - 0.5, Mt34, "Horizontal");
            Add(entries, label, "Toekick (Right)", 1, species, rf + tkD, tkH - 0.5, Mt34, "Horizontal");
        }

        // Shelves (L-shaped)
        if (b.ShelfCount > 0)
        {
            string shelfNote = b.ShelfDepth == CabinetOptions.ShelfDepth.HalfDepth
                ? "L-shape (half depth)" : lNote;
            AddComplex(entries, label, "Shelf", b.ShelfCount, species, Mt34, shelfNote);
        }

        // Doors (always 2 for corner)
        if (b.DoorCount > 0)
        {
            string doorSp = CabinetBuildHelpers.ResolveDoorSpeciesForTotals(b.DoorSpecies, b.CustomDoorSpecies);
            string gd = b.DoorGrainDir ?? "Vertical";
            double cornerRev = 0.875;

            double d1W = lf - dim.DoorLeftReveal - cornerRev;
            double d2W = rf - dim.DoorRightReveal - cornerRev;
            double dH = height - dim.DoorTopReveal - dim.DoorBottomReveal - tkH;

            var (l1, w1) = ByGrain(dH, d1W, gd);
            var (l2, w2) = ByGrain(dH, d2W, gd);
            Add(entries, label, "Door (Left)", 1, doorSp, l1, w1, Mt34, $"Grain: {gd}");
            Add(entries, label, "Door (Right)", 1, doorSp, l2, w2, Mt34, $"Grain: {gd}");
        }
    }

    // ── Angle Front Base ──

    private static void AddBaseAngleFrontParts(BaseCabinetModel b, BaseCabinetDimensions dim,
        string label, string species, List<PartListEntry> entries)
    {
        double height = dim.Height;
        double ld = dim.LeftDepth, rd = dim.RightDepth;
        double lb = dim.LeftBackWidth, rb = dim.RightBackWidth;
        double tkH = dim.TKHeight;

        double vx = (rb - Mt34) - ld;
        double vy = (lb - rd) - Mt34;
        double frontW = Math.Sqrt(vx * vx + vy * vy);

        string tkNote = b.HasTK ? ", TK notch" : "";

        // End panels
        Add(entries, label, "Left End", 1, species, height, ld, Mt34, "Vertical" + tkNote);
        Add(entries, label, "Right End", 1, species, height, rd, Mt34, "Vertical" + tkNote);

        // Deck & Top (pentagonal)
        string aNote = $"Angle front — LD: {Fmt(ld)}, RD: {Fmt(rd)}, LBack: {Fmt(lb)}, RBack: {Fmt(rb)}, Front: {Fmt(frontW)}";
        AddComplex(entries, label, "Deck", 1, species, Mt34, aNote);
        AddComplex(entries, label, "Top", 1, species, Mt34, aNote);

        // Left back
        double lbkW = lb - Mt34 - 0.25;
        Add(entries, label, "Left Back", 1, species, height, lbkW, Mt34, "Vertical" + (b.HasTK ? ", TK notch" : ""));

        // Right back
        double rbkW = rb - 2 * Mt34 - 0.25;
        Add(entries, label, "Right Back", 1, species, height - tkH, rbkW, Mt34, "Vertical");

        // Toekick
        if (b.HasTK)
            Add(entries, label, "Toekick", 1, species, frontW + 2 * dim.TKDepth, tkH - 0.5, Mt34, "Horizontal");

        // Shelves (pentagonal)
        if (b.ShelfCount > 0)
            AddComplex(entries, label, "Shelf", b.ShelfCount, species, Mt34, aNote);

        // Doors
        if (b.DoorCount > 0)
        {
            string doorSp = CabinetBuildHelpers.ResolveDoorSpeciesForTotals(b.DoorSpecies, b.CustomDoorSpecies);
            string gd = b.DoorGrainDir ?? "Vertical";
            double dH = height - dim.DoorTopReveal - dim.DoorBottomReveal - tkH;

            if (b.DoorCount == 1)
            {
                double dW = frontW - dim.DoorLeftReveal - dim.DoorRightReveal;
                var (l, w) = ByGrain(dH, dW, gd);
                Add(entries, label, "Door", 1, doorSp, l, w, Mt34, $"Grain: {gd}");
            }
            else if (b.DoorCount == 2)
            {
                double d1W = (frontW / 2) - dim.DoorLeftReveal - (dim.BaseDoorGap / 2);
                double d2W = (frontW / 2) - dim.DoorRightReveal - (dim.BaseDoorGap / 2);
                var (l1, w1) = ByGrain(dH, d1W, gd);
                var (l2, w2) = ByGrain(dH, d2W, gd);
                Add(entries, label, "Door (Left)", 1, doorSp, l1, w1, Mt34, $"Grain: {gd}");
                Add(entries, label, "Door (Right)", 1, doorSp, l2, w2, Mt34, $"Grain: {gd}");
            }
        }
    }

    // ── Drawer box sub-parts (shared by all styles) ──

    private static void AddDrawerBoxSubParts(List<PartListEntry> entries, string label,
        string boxName, double dbxW, double dbxH, double dbxD)
    {
        const string pfp = "Prefinished Ply";

        Add(entries, label, $"{boxName} — Side", 2, pfp, dbxD, dbxH, Mt34, "Horizontal");
        double fbW = dbxW - 2 * Mt34;
        Add(entries, label, $"{boxName} — Front/Back", 2, pfp, fbW, dbxH, Mt34, "Horizontal");
        double botL = dbxD - 2 * Mt34;
        double botW = dbxW - 2 * Mt34;
        Add(entries, label, $"{boxName} — Bottom", 1, pfp, botL, botW, Mt34, "Vertical");
    }

    // ────────────────────────────────────────────────────────────────────
    //  UPPER CABINETS
    // ────────────────────────────────────────────────────────────────────

    private static void AddUpperParts(UpperCabinetModel u, string label, string species, List<PartListEntry> entries)
    {
        var dim = UpperCabinetDimensions.From(u);

        if (string.Equals(u.Style, CabinetStyles.Upper.Standard, StringComparison.OrdinalIgnoreCase))
            AddUpperStandardParts(u, dim, label, species, entries);
        else if (u.Style == CabinetStyles.Upper.Corner90)
            AddUpperCorner90Parts(u, dim, label, species, entries);
        else if (u.Style == CabinetStyles.Upper.AngleFront)
            AddUpperAngleFrontParts(u, dim, label, species, entries);
    }

    private static void AddUpperStandardParts(UpperCabinetModel u, UpperCabinetDimensions dim,
        string label, string species, List<PartListEntry> entries)
    {
        double width = dim.Width, height = dim.Height, depth = dim.Depth;
        double backThk = dim.BackThickness;
        double iw = dim.InteriorWidth, ih = dim.InteriorHeight, sd = dim.ShelfDepth;

        Add(entries, label, "Left End", 1, species, height, depth, Mt34, "Vertical");
        Add(entries, label, "Right End", 1, species, height, depth, Mt34, "Vertical");
        Add(entries, label, "Deck", 1, species, iw, depth, Mt34, "Horizontal");
        Add(entries, label, "Top", 1, species, iw, depth, Mt34, "Horizontal");

        if (backThk == 0.75)
        {
            Add(entries, label, "Back", 1, species, ih, iw, Mt34, "Vertical");
        }
        else
        {
            Add(entries, label, "Back", 1, "PFP 1/4", height, width, Mt14, "Vertical");
            Add(entries, label, "Nailer", 2, species, iw, 4, Mt34, "Horizontal");
        }

        if (u.ShelfCount > 0)
            Add(entries, label, "Shelf", u.ShelfCount, species, iw - 0.125, sd, Mt34, "Horizontal");

        if (u.DoorCount > 0)
        {
            string doorSp = CabinetBuildHelpers.ResolveDoorSpeciesForTotals(u.DoorSpecies, u.CustomDoorSpecies);
            string gd = u.DoorGrainDir ?? "Vertical";
            double doorSR = dim.DoorSideReveal;
            double doorW = width - (doorSR * 2);
            double doorH = height - dim.DoorTopReveal - dim.DoorBottomReveal;

            if (u.DoorCount == 2)
                doorW = (doorW / 2) - (dim.DoorGap / 2);

            var (len, wid) = ByGrain(doorH, doorW, gd);
            Add(entries, label, "Door", u.DoorCount, doorSp, len, wid, Mt34, $"Grain: {gd}");
        }
    }

    private static void AddUpperCorner90Parts(UpperCabinetModel u, UpperCabinetDimensions dim,
        string label, string species, List<PartListEntry> entries)
    {
        double height = dim.Height;
        double lf = dim.LeftFrontWidth, rf = dim.RightFrontWidth;
        double ld = dim.LeftDepth, rd = dim.RightDepth;

        Add(entries, label, "Left End", 1, species, height, ld, Mt34, "Vertical");
        Add(entries, label, "Right End", 1, species, height, rd, Mt34, "Vertical");

        string lNote = $"L-shape — LF: {Fmt(lf - Mt34)}, RF: {Fmt(rf - Mt34)}, LD: {Fmt(ld - 2 * Mt34)}, RD: {Fmt(rd - 2 * Mt34)}";
        AddComplex(entries, label, "Deck", 1, species, Mt34, lNote);
        AddComplex(entries, label, "Top", 1, species, Mt34, lNote);

        Add(entries, label, "Left Back", 1, species, height, lf + rd - 2 * Mt34, Mt34, "Vertical");
        Add(entries, label, "Right Back", 1, species, height, ld + rf - 3 * Mt34, Mt34, "Vertical");

        if (u.ShelfCount > 0)
            AddComplex(entries, label, "Shelf", u.ShelfCount, species, Mt34, lNote);

        if (u.DoorCount > 0)
        {
            string doorSp = CabinetBuildHelpers.ResolveDoorSpeciesForTotals(u.DoorSpecies, u.CustomDoorSpecies);
            string gd = u.DoorGrainDir ?? "Vertical";
            double cornerRev = 0.875;
            double d1W = lf - dim.DoorLeftReveal - cornerRev;
            double d2W = rf - dim.DoorRightReveal - cornerRev;
            double dH = height - dim.DoorTopReveal - dim.DoorBottomReveal;

            var (l1, w1) = ByGrain(dH, d1W, gd);
            var (l2, w2) = ByGrain(dH, d2W, gd);
            Add(entries, label, "Door (Left)", 1, doorSp, l1, w1, Mt34, $"Grain: {gd}");
            Add(entries, label, "Door (Right)", 1, doorSp, l2, w2, Mt34, $"Grain: {gd}");
        }
    }

    private static void AddUpperAngleFrontParts(UpperCabinetModel u, UpperCabinetDimensions dim,
        string label, string species, List<PartListEntry> entries)
    {
        double height = dim.Height;
        double ld = dim.LeftDepth, rd = dim.RightDepth;
        double lb = dim.LeftBackWidth, rb = dim.RightBackWidth;

        double vx = (rb - Mt34) - ld;
        double vy = (lb - rd) - Mt34;
        double frontW = Math.Sqrt(vx * vx + vy * vy);

        Add(entries, label, "Left End", 1, species, height, ld, Mt34, "Vertical");
        Add(entries, label, "Right End", 1, species, height, rd, Mt34, "Vertical");

        string aNote = $"Angle front — LD: {Fmt(ld)}, RD: {Fmt(rd)}, LBack: {Fmt(lb)}, RBack: {Fmt(rb)}, Front: {Fmt(frontW)}";
        AddComplex(entries, label, "Deck", 1, species, Mt34, aNote);
        AddComplex(entries, label, "Top", 1, species, Mt34, aNote);

        Add(entries, label, "Left Back", 1, species, height, lb - Mt34 - 0.25, Mt34, "Vertical");
        Add(entries, label, "Right Back", 1, species, height, rb - 2 * Mt34 - 0.25, Mt34, "Vertical");

        if (u.ShelfCount > 0)
            AddComplex(entries, label, "Shelf", u.ShelfCount, species, Mt34, aNote);

        if (u.DoorCount > 0)
        {
            string doorSp = CabinetBuildHelpers.ResolveDoorSpeciesForTotals(u.DoorSpecies, u.CustomDoorSpecies);
            string gd = u.DoorGrainDir ?? "Vertical";
            double dH = height - dim.DoorTopReveal - dim.DoorBottomReveal;

            if (u.DoorCount == 1)
            {
                double dW = frontW - dim.DoorLeftReveal - dim.DoorRightReveal;
                var (l, w) = ByGrain(dH, dW, gd);
                Add(entries, label, "Door", 1, doorSp, l, w, Mt34, $"Grain: {gd}");
            }
            else if (u.DoorCount == 2)
            {
                double d1W = (frontW / 2) - dim.DoorLeftReveal - (dim.DoorGap / 2);
                double d2W = (frontW / 2) - dim.DoorRightReveal - (dim.DoorGap / 2);
                var (l1, w1) = ByGrain(dH, d1W, gd);
                var (l2, w2) = ByGrain(dH, d2W, gd);
                Add(entries, label, "Door (Left)", 1, doorSp, l1, w1, Mt34, $"Grain: {gd}");
                Add(entries, label, "Door (Right)", 1, doorSp, l2, w2, Mt34, $"Grain: {gd}");
            }
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  FILLER & PANEL
    // ────────────────────────────────────────────────────────────────────

    private static void AddFillerParts(FillerModel f, string label, string species,
        List<PartListEntry> entries)
    {
        double width = ConvertDimension.FractionToDouble(f.Width);
        double height = ConvertDimension.FractionToDouble(f.Height);
        double depth = ConvertDimension.FractionToDouble(f.Depth);

        Add(entries, label, "End Panel", 1, species, height, depth, Mt34, "Vertical");
        Add(entries, label, "Back", 1, species, height, width, Mt34, "Vertical");
    }

    private static void AddPanelParts(PanelModel p, string label, string species,
        List<PartListEntry> entries)
    {
        double width = ConvertDimension.FractionToDouble(p.Width);
        double height = ConvertDimension.FractionToDouble(p.Height);
        double thickness = ConvertDimension.FractionToDouble(p.Depth); // Depth is used as thickness

        Add(entries, label, "Panel", 1, species, height, width, thickness, "Vertical");
    }
}