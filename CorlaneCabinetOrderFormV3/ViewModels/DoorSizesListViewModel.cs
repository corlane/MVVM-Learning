using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CorlaneCabinetOrderFormV3.Converters;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class DoorSizesListViewModel : ObservableObject
{
    private readonly ICabinetService? _cabinetService;
    private readonly DefaultSettingsService? _defaults;

    public DoorSizesListViewModel()
    {
        // design-time ctor
        Items = new ObservableCollection<DoorSizeItem>();
    }

    public DoorSizesListViewModel(ICabinetService cabinetService, DefaultSettingsService defaults)
    {
        _cabinetService = cabinetService;
        _defaults = defaults;

        Items = new ObservableCollection<DoorSizeItem>();

        // initial population
        RebuildItems();

        // react to collection changes
        if (_cabinetService?.Cabinets is INotifyCollectionChanged notif)
        {
            notif.CollectionChanged += Cabinets_CollectionChanged;
        }

        // react to format changes so displayed dimensions update
        if (_defaults != null)
        {
            _defaults.PropertyChanged += Defaults_PropertyChanged;
        }
    }

    public ObservableCollection<DoorSizeItem> Items { get; }

    private void Defaults_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DefaultSettingsService.DefaultDimensionFormat))
        {
            RebuildItems();
        }
    }

    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // easiest: rebuild on any change
        RebuildItems();
    }

    private void RebuildItems()
    {
        Items.Clear();

        if (_cabinetService == null) return;

        var dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";
        var cabinets = _cabinetService.Cabinets.ToList();

        for (int idx = 0; idx < cabinets.Count; idx++)
        {
            var cab = cabinets[idx];
            int cabNumber = idx + 1;
            string cabName = cab.Name ?? "";
            int qty = cab.Qty;

            // Handle doors
            if (cab is BaseCabinetModel baseCab && baseCab.IncDoorsInList && baseCab.DoorCount > 0)
            {
                // compute door height and width roughly consistent with other code
                double cabWidth = ConvertDimension.FractionToDouble(baseCab.Width);
                double cabHeight = ConvertDimension.FractionToDouble(baseCab.Height);
                double tkHeight = baseCab.HasTK ? ConvertDimension.FractionToDouble(baseCab.TKHeight ?? "0") : 0.0;
                double topReveal = ConvertDimension.FractionToDouble(baseCab.TopReveal ?? "0");
                double bottomReveal = ConvertDimension.FractionToDouble(baseCab.BottomReveal ?? "0");
                double gap = ConvertDimension.FractionToDouble(baseCab.GapWidth ?? "0");

                // door height: rough calculation
                double doorHeightDouble = cabHeight - tkHeight - topReveal - bottomReveal;
                // door width per door
                double perDoorWidthDouble = cabWidth;
                if (baseCab.DoorCount == 2)
                {
                    perDoorWidthDouble = (cabWidth / 2.0) - (gap / 2.0);
                }

                // formatting helper
                static string FormatDimension(double value, string format)
                {
                    if (string.Equals(format, "Fraction", System.StringComparison.OrdinalIgnoreCase))
                        return ConvertDimension.DoubleToFraction(value);
                    return value.ToString("0.###");
                }

                for (int d = 0; d < baseCab.DoorCount; d++)
                {
                    Items.Add(new DoorSizeItem
                    {
                        CabinetNumber = cabNumber,
                        CabinetName = cabName,
                        Qty = qty,
                        ItemType = "Door",
                        Width = FormatDimension(perDoorWidthDouble, dimFormat),
                        Height = FormatDimension(doorHeightDouble, dimFormat),
                        Species = string.IsNullOrWhiteSpace(baseCab.DoorSpecies) ? baseCab.Species ?? "" : baseCab.DoorSpecies,
                        GrainDirection = baseCab.DoorGrainDir ?? ""
                    });
                }
            }
            else if (cab is UpperCabinetModel upperCab && upperCab.IncDoorsInList && upperCab.DoorCount > 0)
            {
                double cabWidth = ConvertDimension.FractionToDouble(upperCab.Width);
                double cabHeight = ConvertDimension.FractionToDouble(upperCab.Height);
                double topReveal = ConvertDimension.FractionToDouble(upperCab.TopReveal ?? "0");
                double bottomReveal = ConvertDimension.FractionToDouble(upperCab.BottomReveal ?? "0");
                double gap = ConvertDimension.FractionToDouble(upperCab.GapWidth ?? "0");

                double doorHeightDouble = cabHeight - topReveal - bottomReveal;
                double perDoorWidthDouble = cabWidth;
                if (upperCab.DoorCount == 2)
                {
                    perDoorWidthDouble = (cabWidth / 2.0) - (gap / 2.0);
                }

                static string FormatDimension(double value, string format)
                {
                    if (string.Equals(format, "Fraction", System.StringComparison.OrdinalIgnoreCase))
                        return ConvertDimension.DoubleToFraction(value);
                    return value.ToString("0.###");
                }

                for (int d = 0; d < upperCab.DoorCount; d++)
                {
                    Items.Add(new DoorSizeItem
                    {
                        CabinetNumber = cabNumber,
                        CabinetName = cabName,
                        Qty = qty,
                        ItemType = "Door",
                        Width = FormatDimension(perDoorWidthDouble, dimFormat),
                        Height = FormatDimension(doorHeightDouble, dimFormat),
                        Species = string.IsNullOrWhiteSpace(upperCab.DoorSpecies) ? upperCab.Species ?? "" : upperCab.DoorSpecies,
                        GrainDirection = upperCab.DoorGrainDir ?? ""
                    });
                }
            }

            // Drawer fronts (openings 1..4)
            if (cab is BaseCabinetModel bCab)
            {
                AddDrawerFrontIfIncluded(bCab, cabNumber, cabName, qty, 1, bCab.IncDrwFrontInList1, bCab.DrwFrontHeight1, bCab);
                AddDrawerFrontIfIncluded(bCab, cabNumber, cabName, qty, 2, bCab.IncDrwFrontInList2, bCab.DrwFrontHeight2, bCab);
                AddDrawerFrontIfIncluded(bCab, cabNumber, cabName, qty, 3, bCab.IncDrwFrontInList3, bCab.DrwFrontHeight3, bCab);
                AddDrawerFrontIfIncluded(bCab, cabNumber, cabName, qty, 4, bCab.IncDrwFrontInList4, bCab.DrwFrontHeight4, bCab);
            }
            else if (cab is UpperCabinetModel uCab)
            {
                // Upper cabinet does not have drawer-front properties in the model, skip unless extended in the future
            }
        }
    }

    private void AddDrawerFrontIfIncluded(BaseCabinetModel bCab, int cabNumber, string cabName, int qty, int openingIndex, bool includedFlag, string heightString, CabinetModel cabForSpecies)
    {
        if (!includedFlag) return;
        if (bCab.DrwCount < openingIndex) return;

        double cabWidth = ConvertDimension.FractionToDouble(bCab.Width);
        double leftReveal = ConvertDimension.FractionToDouble(bCab.LeftReveal ?? "0");
        double rightReveal = ConvertDimension.FractionToDouble(bCab.RightReveal ?? "0");
        double doorSideReveal = (leftReveal + rightReveal) / 2.0;
        double widthDouble = cabWidth - (doorSideReveal * 2.0);

        double heightDouble = ConvertDimension.FractionToDouble(heightString ?? "0");

        var dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";
        string widthFormatted = string.Equals(dimFormat, "Fraction", System.StringComparison.OrdinalIgnoreCase)
            ? ConvertDimension.DoubleToFraction(widthDouble)
            : widthDouble.ToString("0.###");

        string heightFormatted = string.Equals(dimFormat, "Fraction", System.StringComparison.OrdinalIgnoreCase)
            ? ConvertDimension.DoubleToFraction(heightDouble)
            : heightDouble.ToString("0.###");

        Items.Add(new DoorSizeItem
        {
            CabinetNumber = cabNumber,
            CabinetName = cabName,
            Qty = bCab.Qty,
            ItemType = $"Drawer Front {openingIndex}",
            Width = widthFormatted,
            Height = heightFormatted,
            Species = string.IsNullOrWhiteSpace(bCab.DoorSpecies) ? cabForSpecies.Species ?? "" : bCab.DoorSpecies,
            GrainDirection = bCab.DrwFrontGrainDir ?? ""
        });
    }

    public sealed class DoorSizeItem
    {
        public int CabinetNumber { get; init; }
        public string CabinetName { get; init; } = "";
        public int Qty { get; init; }
        public string ItemType { get; init; } = "";
        public string Width { get; init; } = "";
        public string Height { get; init; } = "";
        public string Species { get; init; } = "";
        public string GrainDirection { get; init; } = "";
    }
}