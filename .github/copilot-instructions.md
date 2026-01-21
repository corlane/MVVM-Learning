# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules
- When Door/Drawer size lists depend on CabinetModel accumulators, ensure that AccumulateMaterialAndEdgeTotals runs synchronously (using Dispatcher.Invoke) to avoid missing the last/only cabinet due to BeginInvoke timing.