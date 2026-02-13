using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class CabinetMaterials
{
    internal static DiffuseMaterial GetPlywoodSpecies(string? panelSpecies, string? grainDirection, double rotationDegrees = 0)
    {
        panelSpecies ??= "Prefinished Ply";
        grainDirection ??= "Horizontal";

        if (string.IsNullOrWhiteSpace(panelSpecies))
            panelSpecies = "Prefinished Ply";
        if (string.IsNullOrWhiteSpace(grainDirection))
            grainDirection = "Horizontal";
        if (panelSpecies == "PFP 1/4") panelSpecies = "Prefinished Ply";

        //string resourcePath = $"pack://application:,,,/Images/Plywood/{panelSpecies} - {grainDirection}.png";

        string resourcePath = $"pack://application:,,,/Images/Plywood/{panelSpecies}.png";
        if (grainDirection == "Horizontal") { rotationDegrees = 90; }
        if (grainDirection == "Vertical") { rotationDegrees = 0; }

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(resourcePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            var brush = new ImageBrush(bitmap)
            {
                TileMode = TileMode.Tile,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, .5, 1)
            };

            if (Math.Abs(rotationDegrees) > 1e-6)
            {
                var rt = new RotateTransform(rotationDegrees, 0.5, 0.5);
                rt.Freeze();
                brush.RelativeTransform = rt;
            }

            brush.Freeze();

            var material = new DiffuseMaterial(brush);
            material.Freeze();

            return material;
        }
        catch
        {
            var fallbackBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            fallbackBrush.Freeze();

            var fallbackMaterial = new DiffuseMaterial(fallbackBrush);
            fallbackMaterial.Freeze();

            return fallbackMaterial;
        }
    }

    internal static DiffuseMaterial GetEdgeBandingSpecies(string? species)
    {
        if (string.IsNullOrWhiteSpace(species) || species == "None")
        {
            var solid = new SolidColorBrush(Color.FromRgb(139, 69, 19));
            solid.Freeze();

            var mat = new DiffuseMaterial(solid);
            mat.Freeze();

            return mat;
        }

        // Normalize species name for image lookup: remove the word "Prefinished" if present
        // so entries like "Wood Prefinished Maple" or "Prefinished Maple" map to the image name
        // without the redundant word.
        species = species.Trim();
        if (species.Contains(" Prefinished", StringComparison.OrdinalIgnoreCase))
        {
            species = species.Replace(" Prefinished", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
            //MessageBox.Show($"Normalized edge banding species to '{species}' for image lookup.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        string resourcePath = $"pack://application:,,,/Images/Edgebanding/{species}.png";

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(resourcePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            var brush = new ImageBrush(bitmap)
            {
                TileMode = TileMode.Tile,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, 1, 1)
            };

            brush.Freeze();

            var material = new DiffuseMaterial(brush);
            material.Freeze();

            return material;
        }
        catch
        {
            var solid = new SolidColorBrush(Color.FromRgb(139, 69, 19));
            solid.Freeze();

            var mat = new DiffuseMaterial(solid);
            mat.Freeze();

            return mat;
        }
    }
}