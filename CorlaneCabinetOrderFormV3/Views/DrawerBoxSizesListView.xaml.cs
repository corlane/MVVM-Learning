using CorlaneCabinetOrderFormV3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3.Views;

public partial class DrawerBoxSizesListView : UserControl
{
    public DrawerBoxSizesListView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<DrawerBoxSizesListViewModel>();
    }
}