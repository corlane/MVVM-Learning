using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class PODrwStretcherWidthsViewModel : ObservableObject
{
	private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
	private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

	private const double DepthThresholdIn = 7.0;
	private const double ReferenceStretcherWidthIn = 6.0;

	private readonly ICabinetService? _cabinetService;

	public PODrwStretcherWidthsViewModel()
	{
		// design-time support
		UpdateTabHeaderBrush();
	}

	public PODrwStretcherWidthsViewModel(ICabinetService cabinetService)
	{
		_cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

		if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
		{
			cc.CollectionChanged += (_, __) => Refresh();
		}

		Refresh();
	}

	public ObservableCollection<DrwStretcherWidthExceptionRow> Exceptions { get; } = new();

	[ObservableProperty]
	public partial int TotalCabsNeedingChange { get; set; }

	[ObservableProperty]
	public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

	public void Refresh()
	{
		if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
		{
			Application.Current.Dispatcher.Invoke(Refresh);
			return;
		}

		Exceptions.Clear();
		TotalCabsNeedingChange = 0;

		if (_cabinetService is null)
		{
			UpdateTabHeaderBrush();
			return;
		}

		int cabNumber = 0;

		foreach (var cab in _cabinetService.Cabinets)
		{
			cabNumber++;

			if (cab is not BaseCabinetModel baseCab)
			{
				continue;
			}

			if (baseCab.DrwCount <= 0)
			{
				continue;
			}

			double depthIn = ConvertDimension.FractionToDouble(baseCab.Depth ?? "");
			if (depthIn <= 0)
			{
				continue;
			}

			if (depthIn >= DepthThresholdIn)
			{
				continue;
			}

			var row = new DrwStretcherWidthExceptionRow
			{
				CabinetNumber = cabNumber,
				CabinetName = baseCab.Name ?? "",
				Depth = baseCab.Depth ?? "",
				DrwCount = baseCab.DrwCount,
				ReferenceStretcherWidth = ReferenceStretcherWidthIn.ToString("0.##"),
				Instruction = $"Depth < {DepthThresholdIn:0.##}\" with drawers: change drawer stretcher widths to {ReferenceStretcherWidthIn:0.##}\" (ref).",
				IsDone = false
			};

			row.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(DrwStretcherWidthExceptionRow.IsDone))
				{
					UpdateTabHeaderBrush();
				}
			};

			Exceptions.Add(row);
			TotalCabsNeedingChange += Math.Max(1, baseCab.Qty);
		}

		UpdateTabHeaderBrush();
	}

	private void UpdateTabHeaderBrush()
	{
		if (Exceptions.Count == 0)
		{
			TabHeaderBrush = s_okGreen;
			return;
		}

		bool allDone = Exceptions.All(r => r.IsDone);
		TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
	}

	public sealed partial class DrwStretcherWidthExceptionRow : ObservableObject
	{
		[ObservableProperty] public partial bool IsDone { get; set; }

		[ObservableProperty] public partial int CabinetNumber { get; set; }
		[ObservableProperty] public partial string CabinetName { get; set; } = "";

		[ObservableProperty] public partial string Depth { get; set; } = "";
		[ObservableProperty] public partial int DrwCount { get; set; }

		[ObservableProperty] public partial string ReferenceStretcherWidth { get; set; } = "";
		[ObservableProperty] public partial string Instruction { get; set; } = "";
	}
}