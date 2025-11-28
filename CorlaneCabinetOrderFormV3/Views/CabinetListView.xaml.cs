using Microsoft.Extensions.DependencyInjection;
using CorlaneCabinetOrderFormV3.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3.Views;

public partial class CabinetListView : UserControl
{
    public CabinetListView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<CabinetListViewModel>();
    }

}