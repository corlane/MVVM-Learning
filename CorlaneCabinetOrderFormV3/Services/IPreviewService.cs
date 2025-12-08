using CorlaneCabinetOrderFormV3.Models;
using System;

namespace CorlaneCabinetOrderFormV3.Services;

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