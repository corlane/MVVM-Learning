using Microsoft.Extensions.DependencyInjection;
using MVVM_Learning.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MVVM_Learning.Views;

public partial class CabinetListView : UserControl
{
    public CabinetListView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<CabinetListViewModel>();
    }

}