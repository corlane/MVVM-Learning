//using CorlaneCabinetOrderFormV3.ViewModels;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

//namespace CorlaneCabinetOrderFormV3.Views
//{
//    public partial class PlaceOrderView : UserControl
//    {
//        public PlaceOrderView()
//        {
//            InitializeComponent();
//            //DataContext = App.ServiceProvider.GetRequiredService<PlaceOrderViewModel>();
//        }

//        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
//        {
//            if (sender is TextBox textBox)
//            {
//                Dispatcher.BeginInvoke(() => textBox.SelectAll());
//            }

//            e.Handled = true;
//        }
//    }
//}

// Original code above was modified to add a playful behavior to the "Bad" radio button, making it move away from the mouse cursor when hovered over. This is done by handling the MouseEnter event and randomly repositioning the radio button within the bounds of its parent canvas, while ensuring it stays a certain distance away from the cursor.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CorlaneCabinetOrderFormV3.Views
{
    public partial class PlaceOrderView : UserControl
    {
        private readonly Random _rand = new();

        public PlaceOrderView()
        {
            InitializeComponent();
        }


        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                Dispatcher.BeginInvoke(() => textBox.SelectAll());
            }

            e.Handled = true;
        }

        private void RadioBad_MouseEnter(object? sender, MouseEventArgs e)
        {
            if (ratingCanvas == null || radioBad == null)
                return;

            // Get mouse position relative to the canvas
            var mousePos = e.GetPosition(ratingCanvas);

            // Ensure we have usable canvas size
            double canvasW = ratingCanvas.ActualWidth;
            double canvasH = ratingCanvas.ActualHeight;
            if (canvasW <= 0 || canvasH <= 0)
            {
                // Fallback to the UserControl size if canvas hasn't been measured yet
                canvasW = Math.Max(200, this.ActualWidth);
                canvasH = Math.Max(80, this.ActualHeight);
            }

            // Measure radio button size if needed
            double rbW = radioBad.ActualWidth;
            double rbH = radioBad.ActualHeight;
            if (rbW <= 0 || rbH <= 0)
            {
                radioBad.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var d = radioBad.DesiredSize;
                rbW = d.Width;
                rbH = d.Height;
            }

            // Safety padding: how far away from the mouse we should attempt to place the control
            const double safeDistance = 80.0;

            // Try a few times to find a position that's at least safeDistance away from the pointer
            for (int i = 0; i < 30; i++)
            {
                double x = _rand.NextDouble() * Math.Max(0, canvasW - rbW);
                double y = _rand.NextDouble() * Math.Max(0, canvasH - rbH);

                double dx = x - mousePos.X;
                double dy = y - mousePos.Y;
                if (Math.Sqrt(dx * dx + dy * dy) >= safeDistance)
                {
                    Canvas.SetLeft(radioBad, x);
                    Canvas.SetTop(radioBad, y);
                    return;
                }
            }

            // If we couldn't find a suitable spot, move to the far corner as a fallback
            Canvas.SetLeft(radioBad, Math.Max(0, canvasW - rbW));
            Canvas.SetTop(radioBad, Math.Max(0, canvasH - rbH));
        }
    }
}