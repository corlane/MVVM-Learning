using CorlaneCabinetOrderFormV3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3.Views;

public partial class DoorSizesListView : UserControl
{
    public DoorSizesListView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<DoorSizesListViewModel>();
    }
}