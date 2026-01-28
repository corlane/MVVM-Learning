using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CorlaneCabinetOrderFormV3.Views
{
    /// <summary>
    /// Interaction logic for Cabinet3DView.xaml
    /// </summary>
    public partial class Cabinet3DView : UserControl
    {
        public Cabinet3DView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<Cabinet3DViewModel>();
        }

        private void ResetToFrontView_Click(object sender, RoutedEventArgs e)
        {

            ResetView();
            //// Perfect straight-on front view — tested and gorgeous
            //viewport.Camera.Position = new Point3D(0, 0, 160);
            //viewport.Camera.LookDirection = new Vector3D(0, 0, -160);
            //viewport.Camera.UpDirection = new Vector3D(0, 1, 0);

            //// Frame it perfectly every time
            //viewport.ZoomExtents();
        }

        [RelayCommand]
        private void ResetView()
        {
            // Perfect straight-on front view — tested and gorgeous
            viewport.Camera.Position = new Point3D(0, 0, 160);
            viewport.Camera.LookDirection = new Vector3D(0, 0, -160);
            viewport.Camera.UpDirection = new Vector3D(0, 1, 0);
            // Frame it perfectly every time
            viewport.ZoomExtents();
        }


    }
}
