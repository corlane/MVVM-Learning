using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace CorlaneCabinetOrderFormV3.ViewModels
{
    public partial class ProcessOrderViewModel : ObservableValidator
    {
        private readonly PlaceOrderViewModel? _placeOrderVm;
        private readonly ReadOnlyObservableCollection<MaterialTotal> _materialTotalsReadOnly;

        // Parameterless ctor for design-time support
        public ProcessOrderViewModel()
        {
            // provide a small sample collection for design-time
            //var sample = new ObservableCollection<MaterialTotal>()
            //{
            //    new MaterialTotal { Species = "Prefinished Ply", Quantity = 12.5, Unit = "ft²", UnitPrice = 1.40625m },
            //    new MaterialTotal { Species = "PVC White", Quantity = 25.0, Unit = "ft", UnitPrice = 0.65m }
            //};
            //_materialTotalsReadOnly = new ReadOnlyObservableCollection<MaterialTotal>(sample);
        }

        // DI constructor used at runtime. PlaceOrderViewModel is the source of the MaterialTotals collection.
        public ProcessOrderViewModel(PlaceOrderViewModel placeOrderVm)
        {
            _placeOrderVm = placeOrderVm ?? throw new System.ArgumentNullException(nameof(placeOrderVm));
            _materialTotalsReadOnly = new ReadOnlyObservableCollection<MaterialTotal>(_placeOrderVm.MaterialTotals);
        }

        // Expose read-only collection for binding in the view
        public ReadOnlyObservableCollection<MaterialTotal> MaterialTotals => _materialTotalsReadOnly;

        // Example command if you later want to refresh from the PlaceOrderViewModel
        [RelayCommand]
        private void RefreshFromPlaceOrder()
        {
            // If you add logic later (for example re-aggregate), you can call into the PlaceOrderViewModel.
            // Currently the MaterialTotals collection is shared, so UI updates as PlaceOrderViewModel mutates it.
        }
    }
}