using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Populated by the cabinet builders as they compute values.
/// Single source of truth — no duplicated arithmetic.
/// </summary>
internal sealed class CabinetBuildResult
{
    // ── Core interior dimensions ──
    public double InteriorWidth { get; set; }
    public double InteriorDepth { get; set; }
    public double InteriorHeight { get; set; }
    public double ShelfDepth { get; set; }

    // ── Drawer boxes ──
    public double DrawerBoxWidth { get; set; }
    public double DrawerBoxDepth { get; set; }
    public List<double> DrawerBoxHeights { get; } = [];

    // ── Rollouts ──
    public double RolloutWidth { get; set; }
    public double RolloutHeight { get; set; }
    public double RolloutDepth { get; set; }

    // ── Doors ──
    public double DoorWidth { get; set; }
    public double DoorHeight { get; set; }

    // ── Drawer fronts ──
    public double DrawerFrontWidth { get; set; }
    public List<double> DrawerFrontHeights { get; } = [];
}