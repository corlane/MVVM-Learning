# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction
- Use concise, informal acknowledgments for simple bug identifications (e.g., 'classic copy-paste error').
- Prefer minimal, low-impact code changes; avoid adding/changing too much code when addressing issues.

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules
- When Door/Drawer size lists depend on CabinetModel accumulators, ensure that AccumulateMaterialAndEdgeTotals runs synchronously (using Dispatcher.Invoke) to avoid missing the last/only cabinet due to BeginInvoke timing.
- When investigating intermittent load hangs in CorlaneCabinetOrderFormV3, note that LoadAsync is fast (~288ms) and that commenting out ListViewItems.UpdateLayout() in CabinetListView.xaml.cs did not eliminate the UI freeze.
- Ensure drawer front height TextBox inputs maintain `UpdateSourceTrigger=PropertyChanged` while allowing in-progress fraction states (e.g., "1/", "1 1/") without being overwritten by recalculation logic.