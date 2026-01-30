# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction
- Use concise, informal acknowledgments for simple bug identifications (e.g., 'classic copy-paste error').
- Prefer minimal, low-impact code changes; avoid adding/changing too much code when addressing issues.
- When generating patches, keep changes minimal and avoid accidental deletions of unrelated members (e.g., LoadDefaults/LoadSelectedIfMine). Aim for targeted diffs that only touch the requested areas.
- Use a golden commit plus a refactor branch to safely perform potentially risky refactors with easy rollback.

## Code Style
- Use specific formatting rules
- Follow naming conventions
- Refactor helper/rendering code into internal static classes under the CorlaneCabinetOrderFormV3.Rendering namespace. User confirmed preference to place rendering/geometry helpers in these classes.

## Project-Specific Rules
- When Door/Drawer size lists depend on CabinetModel accumulators, ensure that AccumulateMaterialAndEdgeTotals runs synchronously (using Dispatcher.Invoke) to avoid missing the last/only cabinet due to BeginInvoke timing.
- When investigating intermittent load hangs in CorlaneCabinetOrderFormV3, note that LoadAsync is fast (~288ms) and that commenting out ListViewItems.UpdateLayout() in CabinetListView.xaml.cs did not eliminate the UI freeze.
- Ensure drawer front height TextBox inputs maintain `UpdateSourceTrigger=PropertyChanged` while allowing in-progress fraction states (e.g., "1/", "1 1/") without being overwritten by recalculation logic.
- When refactoring rendering/geometry logic, place it in internal static classes under the CorlaneCabinetOrderFormV3.Rendering namespace.
- Hidden cabinet parts are visual-only; however, they must still contribute to material and edge totals. When building material/edge totals, always include all cabinet parts; BOM calculations should ignore preview hide flags, and totals must always be computed for the full cabinet regardless of hidden parts. User confirmed requirement: preview hide toggles are visualization-only; material/edge totals must always include hidden parts.