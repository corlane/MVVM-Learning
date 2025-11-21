using Microsoft.Extensions.DependencyInjection;
using MVVM_Learning.ViewModels;
using System.Windows.Controls;

namespace MVVM_Learning.Views;

public partial class CabinetView : UserControl
{
    public CabinetView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<CabinetViewModel>();
    }
}