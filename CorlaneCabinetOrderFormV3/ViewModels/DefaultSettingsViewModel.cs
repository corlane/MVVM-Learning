using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class DefaultSettingsViewModel : ObservableObject
{

    public DefaultSettingsViewModel()
    {
        // empty constructor for design-time support
    }

    private readonly DefaultSettingsService? _defaults;

    // Mirror every default as a bindable property
    public string DefaultSpecies { get => _defaults!.DefaultSpecies; set => _defaults!.DefaultSpecies = value; }
    public string DefaultEBSpecies { get => _defaults!.DefaultEBSpecies; set => _defaults!.DefaultEBSpecies = value; }

    public DefaultSettingsViewModel(DefaultSettingsService defaults)
    {
        _defaults = defaults;
    }

    

    [RelayCommand]
    private async Task SaveDefaults()
    {
        await _defaults!.SaveAsync();
        // Optional: show toast/message
    }

    public List<string> ListCabSpecies { get; } =
    [
        "Prefinished Ply",
            "Maple Ply",
            "Red Oak Ply",
            "White Oak Ply",
            "Cherry Ply",
            "Alder Ply",
            "Mahogany Ply",
            "Walnut Ply",
            "Hickory Ply",
            "MDF",
            "Melamine",
            "Custom"
    ];
        public List<string> ListEBSpecies { get; } =
        [
            "None",
            "PVC White",
            "PVC Black",
            "PVC Hardrock Maple",
            "PVC Paint Grade",
            "Wood Prefinished Maple",
            "Wood Maple",
            "Wood Red Oak",
            "Wood White Oak",
            "Wood Walnut",
            "Wood Cherry",
            "Wood Alder",
            "Wood Hickory",
            "Wood Mahogany",
            "Custom"
        ];


}