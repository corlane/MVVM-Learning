using CorlaneCabinetOrderFormV3.Models;
using HelixToolkit.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class CabinetPartFactory
{
    /// <summary>
    /// Creates a dark disc overlay that visually represents a drilled hole on a panel face.
    /// Place at <paramref name="rimZ"/> (the panel surface); <paramref name="bottomZ"/> indicates
    /// which side the viewer is on so the disc normal faces outward.
    /// </summary>
    internal static Model3DGroup CreateHole(
        double centerX,
        double centerY,
        double rimZ,
        double bottomZ,
        double diameter,
        int segments = 16)
    {
        double radius = diameter / 2.0;

        // normalDir: +1 when drilling –Z (viewer on +Z side), –1 when drilling +Z (viewer on –Z side)
        double normalDir = Math.Sign(rimZ - bottomZ);
        // Push the disc slightly proud of the surface so it renders in front of the panel face
        double discZ = rimZ + normalDir * 0.002;

        var builder = new MeshBuilder(false, false);
        var center = new Point3D(centerX, centerY, discZ);

        for (int i = 0; i < segments; i++)
        {
            double a0 = 2.0 * Math.PI * i / segments;
            double a1 = 2.0 * Math.PI * ((i + 1) % segments) / segments;

            var p0 = new Point3D(
                centerX + radius * Math.Cos(a0),
                centerY + radius * Math.Sin(a0),
                discZ);
            var p1 = new Point3D(
                centerX + radius * Math.Cos(a1),
                centerY + radius * Math.Sin(a1),
                discZ);

            if (normalDir >= 0)
                builder.AddTriangle(center, p1, p0);  // normal faces +Z
            else
                builder.AddTriangle(center, p0, p1);  // normal faces –Z
        }

        builder.ComputeNormalsAndTangents(MeshFaces.Default);

        var brush = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        brush.Freeze();
        var holeMaterial = new DiffuseMaterial(brush);
        holeMaterial.Freeze();

        var group = new Model3DGroup();
        group.Children.Add(new GeometryModel3D
        {
            Geometry = builder.ToMesh(true),
            Material = holeMaterial,
            BackMaterial = holeMaterial
        });

        return group;
    }

    /// <summary>
    /// Creates a dark rectangular overlay that visually represents a saw cut on a panel face.
    /// Same convention as <see cref="CreateHole"/>: <paramref name="rimZ"/> is the panel surface,
    /// <paramref name="bottomZ"/> indicates the viewer side so the quad normal faces outward.
    /// The rectangle is centered at (<paramref name="centerX"/>, <paramref name="centerY"/>).
    /// </summary>
    internal static Model3DGroup CreateRectangularCut(
        double centerX,
        double centerY,
        double rimZ,
        double bottomZ,
        double cutWidth,
        double cutLength)
    {
        double halfW = cutWidth / 2.0;
        double halfL = cutLength / 2.0;

        double normalDir = Math.Sign(rimZ - bottomZ);
        double discZ = rimZ + normalDir * 0.002;

        var builder = new MeshBuilder(false, false);

        var p0 = new Point3D(centerX - halfW, centerY - halfL, discZ);
        var p1 = new Point3D(centerX + halfW, centerY - halfL, discZ);
        var p2 = new Point3D(centerX + halfW, centerY + halfL, discZ);
        var p3 = new Point3D(centerX - halfW, centerY + halfL, discZ);

        if (normalDir >= 0)
        {
            builder.AddTriangle(p0, p2, p1);
            builder.AddTriangle(p0, p3, p2);
        }
        else
        {
            builder.AddTriangle(p0, p1, p2);
            builder.AddTriangle(p0, p2, p3);
        }

        builder.ComputeNormalsAndTangents(MeshFaces.Default);

        var brush = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        brush.Freeze();
        var cutMaterial = new DiffuseMaterial(brush);
        cutMaterial.Freeze();

        var group = new Model3DGroup();
        group.Children.Add(new GeometryModel3D
        {
            Geometry = builder.ToMesh(true),
            Material = cutMaterial,
            BackMaterial = cutMaterial
        });

        return group;
    }

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
        double plywoodTextureRotationDegrees = 0,
        CabinetPartKind partKind = CabinetPartKind.Unspecified)
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

        // Determine whether this part gets edgebanding (totals + special material on edges)
        bool hasEdgebanding = partKind is not CabinetPartKind.Toekick
            and not CabinetPartKind.TopStretcherBack
            and not CabinetPartKind.BackBase34
            and not CabinetPartKind.BackBase14
            and not CabinetPartKind.BackUpper14
            and not CabinetPartKind.SinkStretcher
            and not CabinetPartKind.DrawerBoxBottom;

        // Special handling for panels: panelEBEdges letters map to physical panel width/height.
        if (hasEdgebanding && isPanel && !string.IsNullOrEmpty(panelEBEdges))
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

            if (!hasEdgebanding)
            {
                // No edgebanding — all side faces use the main panel material
                mainBuilder.AddQuad(
                    mainBuilder.Positions[b0],
                    mainBuilder.Positions[b1],
                    mainBuilder.Positions[t1],
                    mainBuilder.Positions[t0],
                    uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
            }
            else if (!isPanel && !topDeck90)
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
            else if (isPanel)
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
            else if (topDeck90)
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
            if (edgeBandLengthInches > 0.0 && edgebandingSpecies != "None")
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

        Debug.WriteLine($"Created {partKind} with area {areaFt2:F2} ft^2 and edge banding {edgeBandLengthInches:F1} inches");

        return partModel;
    }
}

