using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVM_Learning.Models;
using System.Collections.ObjectModel;

namespace MVVM_Learning.ViewModels;

public partial class CabinetViewModel : ObservableValidator
{
    public CabinetViewModel()
    {

    }


    [ObservableProperty] public partial string Width { get; set; } = "";
    [ObservableProperty] public partial string Height { get; set; } = "";
    [ObservableProperty] public partial string Depth { get; set; } = "";

    [RelayCommand]
    public void AddCabinet()
    {
        CabinetList.Add(new CabinetModel()
        {
            Width = this.Width,
            Height = this.Height,
            Depth = this.Depth
        });

        // Clear input fields after adding
        // This uses the generated OnWidthChanged etc.
        Width = Height = Depth = string.Empty;
    }

}
