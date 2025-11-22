using System.Windows;

namespace MVVM_Learning;

public partial class MainWindow : Window
{
    public MainWindow()
    {
#pragma warning disable WPF0001
        ThemeMode = ThemeMode.System;
#pragma warning restore WPF0001
        InitializeComponent();
    }
}