using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Tests for lock dado geometry: tenon ranges, mortise pockets, screw holes,
/// and tenon thinning pockets. No WPF types involved — no STA thread needed.
/// </summary>
public class LockDado_PartOutlineBuilder_Tests
{
    private static readonly LockDadoSettings S = LockDadoSettings.Default;

    // ── ComputeTenonRanges ────────────────────────────────────────────────────

    [Fact]
    public void TenonRanges_ShortPart_ProducesOneTenon()
    {
        // Usable zone = 8 - 1.5 - 1.5 = 5". floor(5/10) = 0 gaps → 1 tenon
        var ranges = PartOutlineBuilder.ComputeTenonRanges(edgeLength: 8.0, S);

        Assert.Single(ranges);
        Assert.Equal(S.BlindStart,          ranges[0].StartY, precision: 5);
        Assert.Equal(8.0 - S.BlindStop,     ranges[0].EndY,   precision: 5);
    }

    [Fact]
    public void TenonRanges_24InchPart_ProducesTwoTenons()
    {
        // Usable = 24 - 1.5 - 1.5 = 21". floor(21/10) = 2 gaps → 3 tenons
        var ranges = PartOutlineBuilder.ComputeTenonRanges(edgeLength: 24.0, S);

        Assert.Equal(3, ranges.Count);

        // Tenons must start at BlindStart and end at depth - BlindStop
        Assert.Equal(S.BlindStart,       ranges[0].StartY, precision: 5);
        Assert.Equal(24.0 - S.BlindStop, ranges[^1].EndY,  precision: 5);
    }

    [Fact]
    public void TenonRanges_NoGaps_BetweenTenons()
    {
        // Gaps must exactly bridge between consecutive tenons
        var ranges = PartOutlineBuilder.ComputeTenonRanges(edgeLength: 24.0, S);

        for (int i = 0; i < ranges.Count - 1; i++)
        {
            double gap = ranges[i + 1].StartY - ranges[i].EndY;
            Assert.Equal(S.GapWidth, gap, precision: 5);
        }
    }

    [Fact]
    public void TenonRanges_AllTenonsEqualWidth()
    {
        var ranges = PartOutlineBuilder.ComputeTenonRanges(edgeLength: 24.0, S);
        double firstWidth = ranges[0].EndY - ranges[0].StartY;

        foreach (var (start, end) in ranges)
            Assert.Equal(firstWidth, end - start, precision: 5);
    }

    [Fact]
    public void TenonRanges_TotalLengthEqualsUsableZone()
    {
        double edgeLen    = 24.0;
        var    ranges     = PartOutlineBuilder.ComputeTenonRanges(edgeLen, S);
        int    gapCount   = ranges.Count - 1;

        double totalTenon = ranges.Sum(r => r.EndY - r.StartY);
        double expected   = S.UsableZone(edgeLen) - gapCount * S.GapWidth;

        Assert.Equal(expected, totalTenon, precision: 5);
    }

    // ── ComputeDepthDirectionMortisePockets ───────────────────────────────────

    [Fact]
    public void MortisePockets_Deck_SlotBottomY_OffsetByFlushFaceCorrectly()
    {
        // Deck: flush Top → slot starts at mortiseBottomY + (Mt34 - TenonThickness)
        double mt34          = Services.MaterialDefaults.Thickness34;
        double mortiseBottomY = 4.5;   // typical tkHeight
        double expectedSlotY  = mortiseBottomY + (mt34 - S.TenonThickness);

        var pockets = PartOutlineBuilder.ComputeDepthDirectionMortisePockets(
            partDepth     : 24.0,
            mortiseBottomY: mortiseBottomY,
            flushFace     : TenonFlushFace.Top,
            s             : S);

        Assert.NotEmpty(pockets);
        foreach (var (_, _, y1, _) in pockets)
            Assert.Equal(expectedSlotY, y1, precision: 5);
    }

    [Fact]
    public void MortisePockets_Top_SlotBottomY_AtMortiseBottomY()
    {
        // Top: flush Bottom → slot starts exactly at mortiseBottomY
        double mortiseBottomY = 34.5 - Services.MaterialDefaults.Thickness34;

        var pockets = PartOutlineBuilder.ComputeDepthDirectionMortisePockets(
            partDepth     : 24.0,
            mortiseBottomY: mortiseBottomY,
            flushFace     : TenonFlushFace.Bottom,
            s             : S);

        Assert.NotEmpty(pockets);
        foreach (var (_, _, y1, _) in pockets)
            Assert.Equal(mortiseBottomY, y1, precision: 5);
    }

    [Fact]
    public void MortisePockets_SlotHeight_EqualsMortiseSlotHeight()
    {
        var pockets = PartOutlineBuilder.ComputeDepthDirectionMortisePockets(
            partDepth     : 24.0,
            mortiseBottomY: 4.5,
            flushFace     : TenonFlushFace.Bottom,
            s             : S);

        foreach (var (_, _, y1, y2) in pockets)
            Assert.Equal(S.MortiseSlotHeight, y2 - y1, precision: 5);
    }

    [Fact]
    public void MortisePockets_XRanges_OversizeTenonByMortiseOversize()
    {
        double depth  = 24.0;
        var tenons    = PartOutlineBuilder.ComputeTenonRanges(depth, S);
        var pockets   = PartOutlineBuilder.ComputeDepthDirectionMortisePockets(
            partDepth     : depth,
            mortiseBottomY: 4.5,
            flushFace     : TenonFlushFace.Bottom,
            s             : S);

        Assert.Equal(tenons.Count, pockets.Count);

        // Interior tenons (not clamped) should be exactly MortiseOversize wider each side
        for (int i = 1; i < tenons.Count - 1; i++)
        {
            var (tStart, tEnd)    = tenons[i];
            var (px1, px2, _, _)  = pockets[i];
            Assert.Equal(tStart - S.MortiseOversize, px1, precision: 5);
            Assert.Equal(tEnd   + S.MortiseOversize, px2, precision: 5);
        }
    }

    [Fact]
    public void MortisePockets_Count_MatchesTenonCount()
    {
        var pockets = PartOutlineBuilder.ComputeDepthDirectionMortisePockets(
            partDepth: 24.0, mortiseBottomY: 4.5, flushFace: TenonFlushFace.Bottom, s: S);
        var tenons  = PartOutlineBuilder.ComputeTenonRanges(24.0, S);

        Assert.Equal(tenons.Count, pockets.Count);
    }

    // ── ComputeDepthDirectionScrewHoles ───────────────────────────────────────

    [Fact]
    public void ScrewHoles_Count_IsOneFewerThanTenons()
    {
        var tenons = PartOutlineBuilder.ComputeTenonRanges(24.0, S);
        var holes  = PartOutlineBuilder.ComputeDepthDirectionScrewHoles(
            partDepth: 24.0, mortiseBottomY: 4.5, flushFace: TenonFlushFace.Bottom, s: S);

        Assert.Equal(tenons.Count - 1, holes.Count);
    }

    [Fact]
    public void ScrewHoles_CenterX_IsAtGapCenter()
    {
        double depth  = 24.0;
        var    tenons = PartOutlineBuilder.ComputeTenonRanges(depth, S);
        var    holes  = PartOutlineBuilder.ComputeDepthDirectionScrewHoles(
            partDepth: depth, mortiseBottomY: 4.5, flushFace: TenonFlushFace.Bottom, s: S);

        for (int i = 0; i < holes.Count; i++)
        {
            double expectedCenterX = (tenons[i].EndY + tenons[i + 1].StartY) / 2.0;
            Assert.Equal(expectedCenterX, holes[i].CenterX, precision: 5);
        }
    }

    [Fact]
    public void ScrewHoles_Diameter_MatchesSetting()
    {
        var holes = PartOutlineBuilder.ComputeDepthDirectionScrewHoles(
            partDepth: 24.0, mortiseBottomY: 4.5, flushFace: TenonFlushFace.Bottom, s: S);

        foreach (var (_, _, dia) in holes)
            Assert.Equal(S.ScrewPilotHoleDiameter, dia, precision: 5);
    }

    // ── ComputeTenonThinningPockets ───────────────────────────────────────────

    [Fact]
    public void ThinningPockets_AlwaysReturnsTwoPockets()
    {
        var pockets = PartOutlineBuilder.ComputeTenonThinningPockets(18.0, 24.0, S);
        Assert.Equal(2, pockets.Length);
    }

    [Fact]
    public void ThinningPockets_LeftPocket_StartsAtZero()
    {
        var pockets = PartOutlineBuilder.ComputeTenonThinningPockets(18.0, 24.0, S);
        Assert.Equal(0.0, pockets[0].X1, precision: 5);
    }

    [Fact]
    public void ThinningPockets_RightPocket_EndsAtLength()
    {
        double length = 18.0;
        var pockets   = PartOutlineBuilder.ComputeTenonThinningPockets(length, 24.0, S);
        Assert.Equal(length, pockets[1].X2, precision: 5);
    }

    [Fact]
    public void ThinningPockets_XWidth_EqualsDadoDepth()
    {
        var pockets = PartOutlineBuilder.ComputeTenonThinningPockets(18.0, 24.0, S);
        Assert.Equal(S.DadoDepth, pockets[0].X2 - pockets[0].X1, precision: 5);
        Assert.Equal(S.DadoDepth, pockets[1].X2 - pockets[1].X1, precision: 5);
    }

    [Fact]
    public void ThinningPockets_YSpan_CoversUsableZonePlusOversize()
    {
        double depth    = 24.0;
        var    pockets  = PartOutlineBuilder.ComputeTenonThinningPockets(18.0, depth, S);
        double expectedY1 = S.BlindStart - S.TenonPocketOversize;
        double expectedY2 = depth - S.BlindStop + S.TenonPocketOversize;

        foreach (var (_, _, y1, y2) in pockets)
        {
            Assert.Equal(expectedY1, y1, precision: 5);
            Assert.Equal(expectedY2, y2, precision: 5);
        }
    }

    // ── TenonBothEnds outline sanity ──────────────────────────────────────────

    [Fact]
    public void TenonBothEnds_VertexCount_CorrectForKnownGapCount()
    {
        // 24" depth: 2 gaps → 3 tenons per edge
        // Per edge: 2 fixed corners + 2 blind zone steps + (3 tenons × 4 verts) = 16
        // Both edges + 2 straight edges (front/back, 1 vert each side) = 16×2 + 4 = 36
        double length = 18.0;
        double depth  = 24.0;
        int    gaps   = S.GapCount(depth);        // 2
        int    tenons = gaps + 1;                 // 3
        int    expectedVerts = 4                  // 4 corners (front-L, front-R, back-R, back-L)
                             + 4                  // 2 blind zone steps per edge × 2 edges
                             + tenons * 4 * 2;    // 4 verts per tenon × 2 edges

        var verts = PartOutlineBuilder.TenonBothEnds(length, depth, S);
        Assert.Equal(expectedVerts, verts.Count);
    }
}