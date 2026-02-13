using CorlaneCabinetOrderFormV3.Models;
using HelixToolkit.Wpf;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class CabinetPartFactory
{
    internal static Model3DGroup CreatePanel(
        List<Point3D> polygonPoints,
        double matlThickness,
        string panelSpecies,
        string edgebandingSpecies,
        string grainDirection,
        CabinetModel cab,
        bool topDeck90,
        bool isPanel,
        string panelEBEdges,
        bool isFaceUp,
        double plywoodTextureRotationDegrees = 0)
    {
        double thickness = matlThickness;

        // --- compute polygon area (shoelace) in square inches and accumulate into cabinet totals ---
        double areaSqIn = 0.0;
        for (int i = 0, j = polygonPoints.Count - 1; i < polygonPoints.Count; j = i++)
        {
            var pi = polygonPoints[i];
            var pj = polygonPoints[j];
            areaSqIn += (pj.X * pi.Y) - (pi.X * pj.Y);
        }

        areaSqIn = Math.Abs(areaSqIn) * 0.5;
        double areaFt2 = areaSqIn / 144.0; // in^2 -> ft^2

        static string BuildFaceKey(string species, bool faceUp)
        {
            var baseKey = string.IsNullOrWhiteSpace(species) ? "None" : species.Trim();
            if (string.Equals(baseKey, "None", StringComparison.OrdinalIgnoreCase))
            {
                return "None";
            }

            // Materials where face orientation should be ignored (legacy behavior)
            if (string.Equals(baseKey, "Prefinished Ply", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(baseKey, "PFP 1/4", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(baseKey, "MDF", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(baseKey, "Melamine", StringComparison.OrdinalIgnoreCase))
            {
                return baseKey;
            }

            return faceUp ? $"{baseKey} UP" : $"{baseKey} DOWN";
        }

        try
        {
            var speciesKey = BuildFaceKey(panelSpecies, isFaceUp);

            if (cab.MaterialAreaBySpecies.ContainsKey(speciesKey))
            {
                cab.MaterialAreaBySpecies[speciesKey] += areaFt2;
            }
            else
            {
                cab.MaterialAreaBySpecies[speciesKey] = areaFt2;
            }
        }
        catch
        {
            // Keep CreatePanel robust - don't let accumulation failures break the preview
        }

        // Create a MeshBuilder with textures enabled (second param true)
        var mainBuilder = new MeshBuilder(false, true);
        var specialBuilder = new MeshBuilder(false, true); // For edgebanded side

        // Find min/max for UV normalization (project XY to 0-1)
        double minX = polygonPoints.Min(p => p.X);
        double maxX = polygonPoints.Max(p => p.X);
        double minY = polygonPoints.Min(p => p.Y);
        double maxY = polygonPoints.Max(p => p.Y);

        // Add bottom positions and texture coords
        foreach (var point in polygonPoints)
        {
            mainBuilder.Positions.Add(point);
            double u = (point.X - minX) / (maxX - minX);
            double v = (point.Y - minY) / (maxY - minY);
            mainBuilder.TextureCoordinates.Add(new Point(u, v));
        }

        // Add bottom face using triangulation
        var bottomIndices = Enumerable.Range(0, polygonPoints.Count).ToList();
        mainBuilder.AddPolygonByTriangulation(bottomIndices);

        // Add top positions at z=thickness with same texture coords
        int topOffset = polygonPoints.Count;
        foreach (var point in polygonPoints)
        {
            mainBuilder.Positions.Add(new Point3D(point.X, point.Y, thickness));
            double u = (point.X - minX) / (maxX - minX);
            double v = (point.Y - minY) / (maxY - minY);
            mainBuilder.TextureCoordinates.Add(new Point(u, v));
        }

        // Add top face (reverse indices for correct winding/normal direction)
        var topIndices = Enumerable.Range(topOffset, polygonPoints.Count).Reverse().ToList();
        mainBuilder.AddPolygonByTriangulation(topIndices);

        // Add side faces as quads with texture coords (unwrap sides: U around perimeter, V height)
        double perimeter = 0;
        var sideLengths = new List<double>();
        for (int i = 0; i < polygonPoints.Count; i++)
        {
            int next = (i + 1) % polygonPoints.Count;
            double dx = polygonPoints[next].X - polygonPoints[i].X;
            double dy = polygonPoints[next].Y - polygonPoints[i].Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            sideLengths.Add(len);
            perimeter += len;
        }

        // Track edgebanding total length (in inches) for this panel
        double edgeBandLengthInches = 0.0;

        // Special handling for panels: panelEBEdges letters map to physical panel width/height.
        if (isPanel && !string.IsNullOrEmpty(panelEBEdges))
        {
            double panelWidthInches = maxX - minX;
            double panelHeightInches = maxY - minY;

            if (panelEBEdges.Contains('T')) edgeBandLengthInches += panelWidthInches;
            if (panelEBEdges.Contains('B')) edgeBandLengthInches += panelWidthInches;
            if (panelEBEdges.Contains('L')) edgeBandLengthInches += panelHeightInches;
            if (panelEBEdges.Contains('R')) edgeBandLengthInches += panelHeightInches;
        }

        double cumulativeU = 0;
        for (int edgeFace = 0; edgeFace < polygonPoints.Count; edgeFace++)
        {
            int b0 = edgeFace;
            int b1 = (edgeFace + 1) % polygonPoints.Count;
            int t1 = b1 + topOffset;
            int t0 = b0 + topOffset;

            double u0 = cumulativeU / perimeter;
            double u1 = (cumulativeU + sideLengths[edgeFace]) / perimeter;
            Point uvBottomLeft = new(u0, 0);
            Point uvBottomRight = new(u1, 0);
            Point uvTopRight = new(u1, 1);
            Point uvTopLeft = new(u0, 1);

            if (!isPanel && !topDeck90)
            {
                if (edgeFace == 0)
                {
                    specialBuilder.AddQuad(
                        mainBuilder.Positions[b0],
                        mainBuilder.Positions[b1],
                        mainBuilder.Positions[t1],
                        mainBuilder.Positions[t0],
                        uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);

                    edgeBandLengthInches += sideLengths[edgeFace];
                }
                else
                {
                    mainBuilder.AddQuad(
                        mainBuilder.Positions[b0],
                        mainBuilder.Positions[b1],
                        mainBuilder.Positions[t1],
                        mainBuilder.Positions[t0],
                        uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
                }
            }

            if (isPanel)
            {
                mainBuilder.AddQuad(
                    mainBuilder.Positions[b0],
                    mainBuilder.Positions[b1],
                    mainBuilder.Positions[t1],
                    mainBuilder.Positions[t0],
                    uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);

                if (panelEBEdges.Contains('B') && edgeFace == 0)
                {
                    specialBuilder.AddQuad(
                        mainBuilder.Positions[b0],
                        mainBuilder.Positions[b1],
                        mainBuilder.Positions[t1],
                        mainBuilder.Positions[t0],
                        uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
                }
                else if (panelEBEdges.Contains('R') && edgeFace == 1)
                {
                    specialBuilder.AddQuad(
                        mainBuilder.Positions[b0],
                        mainBuilder.Positions[b1],
                        mainBuilder.Positions[t1],
                        mainBuilder.Positions[t0],
                        uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
                }
                else if (panelEBEdges.Contains('T') && edgeFace == 2)
                {
                    specialBuilder.AddQuad(
                        mainBuilder.Positions[b0],
                        mainBuilder.Positions[b1],
                        mainBuilder.Positions[t1],
                        mainBuilder.Positions[t0],
                        uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
                }
                else if (panelEBEdges.Contains('L') && edgeFace == 3)
                {
                    specialBuilder.AddQuad(
                        mainBuilder.Positions[b0],
                        mainBuilder.Positions[b1],
                        mainBuilder.Positions[t1],
                        mainBuilder.Positions[t0],
                        uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
                }
            }

            if (topDeck90)
            {
                if (edgeFace == 0 || edgeFace == 1)
                {
                    specialBuilder.AddQuad(
                        mainBuilder.Positions[b0],
                        mainBuilder.Positions[b1],
                        mainBuilder.Positions[t1],
                        mainBuilder.Positions[t0],
                        uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);

                    edgeBandLengthInches += sideLengths[edgeFace];
                }
                else
                {
                    mainBuilder.AddQuad(
                        mainBuilder.Positions[b0],
                        mainBuilder.Positions[b1],
                        mainBuilder.Positions[t1],
                        mainBuilder.Positions[t0],
                        uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
                }
            }

            cumulativeU += sideLengths[edgeFace];
        }

        // If edgebanding was applied, accumulate into the cabinet's edge-banding totals
        try
        {
            if (edgeBandLengthInches > 0.0)
            {
                var ebSpeciesKey = string.IsNullOrWhiteSpace(edgebandingSpecies) ? "None" : edgebandingSpecies;
                double feet = edgeBandLengthInches / 12.0;

                if (cab.EdgeBandingLengthBySpecies.ContainsKey(ebSpeciesKey))
                    cab.EdgeBandingLengthBySpecies[ebSpeciesKey] += feet;
                else
                    cab.EdgeBandingLengthBySpecies[ebSpeciesKey] = feet;
            }
        }
        catch
        {
            // swallow accumulation errors to keep preview resilient
        }

        mainBuilder.ComputeNormalsAndTangents(MeshFaces.Default);
        specialBuilder.ComputeNormalsAndTangents(MeshFaces.Default);

        var mesh = mainBuilder.ToMesh(true);
        var specialMesh = specialBuilder.ToMesh(true);

        var material = CabinetMaterials.GetPlywoodSpecies(panelSpecies, grainDirection, plywoodTextureRotationDegrees);
        var specialMaterial = CabinetMaterials.GetEdgeBandingSpecies(edgebandingSpecies);

        if (edgebandingSpecies == "None")
        {
            specialMaterial = CabinetMaterials.GetPlywoodSpecies(panelSpecies, grainDirection, plywoodTextureRotationDegrees);
        }

        var panelModel = new GeometryModel3D
        {
            Geometry = mesh,
            Material = material,
            BackMaterial = material
        };

        var edgebandingModel = new GeometryModel3D
        {
            Geometry = specialMesh,
            Material = specialMaterial,
            BackMaterial = specialMaterial
        };

        var partModel = new Model3DGroup();
        partModel.Children.Add(panelModel);
        partModel.Children.Add(edgebandingModel);

        return partModel;
    }
}