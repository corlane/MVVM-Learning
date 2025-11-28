using System.Windows;

namespace MVVM_Learning;

public partial class MainWindow : Window
{
    public MainWindow()
    {
#pragma warning disable WPF0001

        Application.Current.ThemeMode = ThemeMode.System;
#pragma warning restore WPF0001

        InitializeComponent();

    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
#pragma warning disable WPF0001
        
        Application.Current.ThemeMode = ThemeMode.Dark;
        
#pragma warning restore WPF0001

    }


    private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
#pragma warning disable WPF0001
        Application.Current.ThemeMode = ThemeMode.Light;

#pragma warning restore WPF0001

    }
}