using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Screw pilot hole computation for mortise end panels.
/// 
/// Generates CNC pilot hole positions for screw fastening tenon-mortise joints on end panels.
/// Holes are positioned in gaps between tenons to reinforce the joint without hitting tenon protrusions.
/// Results are rendered to the MACHINING_SCREW_HOLES DXF layer for end panels.
/// 
/// Two Joint Orientations:
/// 
///   Depth-Direction Joints (horizontal tenons):
///     ComputeDepthDirectionScrewHoles()
///       Tenons run perpendicular to end panel face (left-to-right along panel depth).
///       Used for shelf/bottom support rails, drawer fronts, and similar depth-spanning parts.
///       
///       Inputs:
///         • partDepth          — length of the edge containing tenons (inches)
///         • mortiseBottomY     — vertical position of the mortise slot bottom on end panel (Y)
///         • flushFace          — how the joint sits: Top, Bottom, etc.
///         • xOffset            — horizontal offset for asymmetric joints (default: 0)
///         • forceTwoTenons     — override to exactly 2 tenons
///       
///       Hole Positioning:
///         1. Compute tenon ranges along partDepth (e.g., 0–2", gap 2–3", 3–5")
///         2. Place hole centers in gap midpoints: (tenons[i].EndY + tenons[i+1].StartY) / 2
///         3. Adjust vertical (Y) position based on flush face:
///            • TenonFlushFace.Top: slot is above mortise depth, account for 3/4" material thickness
///              slotBottomY = mortiseBottomY + (3/4" - TenonThickness)
///            • Other faces: slot aligns with mortise bottom
///         4. holeCenterY = slotBottomY + (MortiseSlotHeight / 2) — center of slot vertically
///         5. Apply xOffset to shift holes horizontally (for asymmetric mounting)
///       
///       Output: List of (gapCenterX, holeCenterY, diameter) tuples
///         X-coordinate: horizontal gap center (along panel depth, offset applied)
///         Y-coordinate: vertical center of mortise slot
///         Diameter: s.ScrewPilotHoleDiameter (typically 7/32")
///   
///   Height-Direction Joints (vertical tenons):
///     ComputeHeightDirectionScrewHoles()
///       Tenons run vertically along end panel edge (bottom-to-top).
///       Used for full-height vertical joinery (e.g., cabinet side to kick base, cabinets stacked vertically).
///       
///       Inputs:
///         • edgeLength         — vertical span of the edge containing tenons (inches)
///         • xPosition          — horizontal position of the tenon center on end panel (X)
///         • bottomY            — base Y-coordinate for the joint (0 for bottom, higher for stacked)
///         • flushFace          — how the joint sits: Back or InteriorFront
///         • forceTwoTenons     — override to exactly 2 tenons
///       
///       Hole Positioning:
///         1. Compute tenon ranges along edgeLength (e.g., 0–4", gap 4–5", 5–9")
///         2. Place hole centers in gap midpoints: bottomY + (tenons[i].EndY + tenons[i+1].StartY) / 2
///         3. Adjust horizontal (X) position based on flush face:
///            • TenonFlushFace.Back: hole offset left (negative) from tenon center
///              holeCenterX = xPosition - (TenonThickness / 2)
///            • TenonFlushFace.InteriorFront: hole offset into mortise slot
///              holeCenterX = xPosition + (MortiseSlotHeight / 2)
///            • Other faces: throw (only Back/InteriorFront valid for height joints)
///       
///       Output: List of (holeCenterX, gapCenterY, diameter) tuples
///         X-coordinate: horizontal position adjusted for flush face
///         Y-coordinate: vertical gap center (along edge, relative to bottomY)
///         Diameter: s.ScrewPilotHoleDiameter (typically 7/32")
/// 
/// Gap-Based Placement Strategy:
///   • Holes positioned in gaps between tenons (not in tenon protrusions)
///   • Number of holes = number of gaps = (number of tenons - 1)
///   • Ensures screw does not interfere with tenon geometry
///   • Reinforces each mortise-to-tenon connection point
/// 
/// Material Defaults:
///   • MaterialDefaults.Thickness34 = 0.75" (used to adjust slot position when flush face is Top)
///   • s.ScrewPilotHoleDiameter — typically 7/32" (for cabinet assembly screws)
///   • s.MortiseSlotHeight — slot depth for screw pilot (typically 0.375"–0.5")
///   • s.TenonThickness — protrusion depth (typically 0.75"–1.0")
/// 
/// Integration:
///   Used by MortiseSpecBuilder.BuildForBaseStandard() to populate MortiseSpec.ScrewHoles
///   for end panels. MortiseSpec is then passed to ExportEndPanel() for DXF rendering.
/// </summary>

internal static partial class PartOutlineBuilder
{
    internal static List<(double CenterX, double CenterY, double Diameter)> ComputeDepthDirectionScrewHoles(
        double partDepth, double mortiseBottomY, TenonFlushFace flushFace, LockDadoSettings s,
        double xOffset = 0, bool forceTwoTenons = false)
    {
        double mt34 = MaterialDefaults.Thickness34;
        double slotBottomY = flushFace switch
        {
            TenonFlushFace.Top => mortiseBottomY + s.TenonClearance + (mt34 / 2),
            TenonFlushFace.Bottom => mortiseBottomY + (mt34 / 2),
            _ => mortiseBottomY
        };
        double holeCenterY = slotBottomY;

        var tenons = ComputeTenonRanges(partDepth, s, forceTwoTenons, blindStart: null, blindStop: null);
        var holes = new List<(double, double, double)>();

        for (int i = 0; i < tenons.Count - 1; i++)
        {
            double gapCenterX = (tenons[i].EndY + tenons[i + 1].StartY) / 2.0 + xOffset;
            holes.Add((gapCenterX, holeCenterY, s.ScrewPilotHoleDiameter));
        }

        return holes;
    }

    internal static List<(double CenterX, double CenterY, double Diameter)> ComputeHeightDirectionScrewHoles(
        double edgeLength, double xPosition, double bottomY, TenonFlushFace flushFace, LockDadoSettings s,
        bool forceTwoTenons = false)
    {
        double mt34 = MaterialDefaults.Thickness34;

        double holeCenterX = flushFace switch
        {
            TenonFlushFace.Back => xPosition + s.TenonClearance + (mt34 / 2),
            TenonFlushFace.InteriorFront => xPosition - xPosition + (mt34 / 2),
            _ => throw new ArgumentOutOfRangeException(nameof(flushFace), flushFace, "Height-direction joints must use Back or InteriorFront.")
        };

        var tenons = ComputeTenonRanges(edgeLength, s, forceTwoTenons, blindStart: null, blindStop: null);
        var holes = new List<(double, double, double)>();

        for (int i = 0; i < tenons.Count - 1; i++)
        {
            double gapCenterY = bottomY + (tenons[i].EndY + tenons[i + 1].StartY) / 2.0;
            holes.Add((holeCenterX, gapCenterY, s.ScrewPilotHoleDiameter));
        }

        return holes;
    }
}