using CorlaneCabinetOrderFormV3.Models;
using System;

namespace CorlaneCabinetOrderFormV3.Services;

// IPreviewService.cs
// Defines the contract for the 3D preview service that coordinates which cabinet model
// is currently displayed in the HelixViewport3D preview window.
//
// The app has multiple input tabs (Base, Upper, Filler, Panel) each with their own
// ViewModel, all sharing a single 3D preview panel. Without coordination, whichever
// tab last called UpdatePreview() would overwrite the preview — even if that tab is
// not currently visible.
//
// IPreviewService solves this with an active-owner token pattern:
//   - Each tab ViewModel registers itself as the active owner when it gains focus
//     (SetActiveOwner). Only the active owner's RequestPreview calls are applied to
//     the live preview immediately.
//   - RequestPreview calls from inactive owners are cached per-owner so that when
//     the user switches back to that tab, its last-built model is restored instantly
//     via SetActiveOwner without needing a rebuild.
//   - ForcePreview bypasses the owner check entirely — used when the user clicks a
//     cabinet in the cabinet list to preview it regardless of which tab is active.
//   - PreviewChanged event lets the 3D viewport (Cabinet3DViewModel) observe model
//     changes without being directly coupled to any input ViewModel.
//
// The concrete implementation (PreviewService) is thread-safe; all owner comparisons
// and model swaps are guarded by a lock and a ConcurrentDictionary cache.

public interface IPreviewService
{
    CabinetModel? CurrentPreviewCabinet { get; }
    event EventHandler? PreviewChanged;

    // Set owner (e.g., tab index, viewmodel instance id, or any token)
    void SetActiveOwner(object? owner);

    // Request a preview from a VM (accepted only if owner == active)
    void RequestPreview(object owner, CabinetModel model);

    // Force immediate preview regardless of active owner (list selection)
    void ForcePreview(CabinetModel model);

    // Clear preview (optional)
    void ClearPreview();
}