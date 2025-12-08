using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Models;
using System;
using System.Collections.Concurrent;

namespace CorlaneCabinetOrderFormV3.Services;

public class PreviewService : ObservableObject, IPreviewService
{
    private readonly object _sync = new();
    private object? _activeOwner;
    private CabinetModel? _current;

    // Per-owner cache so each tab/owner can keep its last requested model.
    // ConcurrentDictionary used for thread-safety as requests can come from UI thread(s).
    private readonly ConcurrentDictionary<object, CabinetModel?> _cache = new();

    public CabinetModel? CurrentPreviewCabinet
    {
        get => _current;
        private set
        {
            if (_current == value) return;
            _current = value;
            OnPropertyChanged(nameof(CurrentPreviewCabinet));
            PreviewChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? PreviewChanged;

    public void SetActiveOwner(object? owner)
    {
        lock (_sync)
        {
            _activeOwner = owner;

            // When active owner changes, if we have a cached model for the owner apply it.
            if (owner != null && _cache.TryGetValue(owner, out var cachedModel))
            {
                CurrentPreviewCabinet = cachedModel;
            }
            else
            {
                // No cached model for this owner — don't forcibly clear the preview.
                // Optionally you could clear: CurrentPreviewCabinet = null;
            }
        }
    }

    public void RequestPreview(object owner, CabinetModel model)
    {
        if (owner == null) return;

        // Always cache the latest request for the owner.
        _cache[owner] = model;

        lock (_sync)
        {
            // If the requester is the active owner, apply immediately.
            if (ReferenceEquals(owner, _activeOwner) || owner.Equals(_activeOwner))
            {
                CurrentPreviewCabinet = model;
            }
            else
            {
                // Not active — cached above so it will be applied when the owner becomes active.
            }
        }
    }

    public void ForcePreview(CabinetModel model)
    {
        lock (_sync)
        {
            CurrentPreviewCabinet = model;

            // Also update cache for the active owner if there is one (keeps per-owner last state sane)
            if (_activeOwner != null)
            {
                _cache[_activeOwner] = model;
            }
        }
    }

    public void ClearPreview()
    {
        lock (_sync)
        {
            CurrentPreviewCabinet = null;

            // Optionally clear cache for active owner only, or all owners — keep cache as-is by default.
            if (_activeOwner != null)
            {
                _cache[_activeOwner] = null;
            }
        }
    }
}