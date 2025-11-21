using Microsoft.Extensions.DependencyInjection;
using MVVM_Learning.ViewModels;
using System.Windows.Controls;

namespace MVVM_Learning.Views;

public partial class BaseCabinetView : UserControl
{
    public BaseCabinetView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<BaseCabinetViewModel>();
    }
}