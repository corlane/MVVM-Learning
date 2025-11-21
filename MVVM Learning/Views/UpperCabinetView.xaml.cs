using Microsoft.Extensions.DependencyInjection;
using MVVM_Learning.ViewModels;
using System.Windows.Controls;

namespace MVVM_Learning.Views;

public partial class UpperCabinetView : UserControl
{
    public UpperCabinetView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<UpperCabinetViewModel>();
    }
}