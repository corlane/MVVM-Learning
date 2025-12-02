using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Services;
using HelixToolkit.Wpf;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class Cabinet3DViewModel : ObservableObject
{
    public Cabinet3DViewModel()
    {
        // empty constructor for design
    }

    private readonly ICabinetService _cabinetService;

    [ObservableProperty]
    public partial Model3DGroup? CurrentModel { get; set; }

    public Cabinet3DViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService;
        _cabinetService.Cabinets.CollectionChanged += OnCabinetsChanged;
        BuildCabinet(); // initial build
    }

    private void OnCabinetsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BuildCabinet();
    }

    public void BuildCabinet()
    {
        if (CabinetType is not null && CabinetType.Contains("Base"))
        {
            BuildBase();
        }

        //.... etc.


        // Add ambient light
        var ambColor = new Color
        {
            R = 60,
            G = 60,
            B = 60
        }; CurrentModel!.Children.Add(new AmbientLight(ambColor));
        //partModel.Children.Add(new DirectionalLight(Colors.White, new Vector3D(0, 0, -1)));

    }

    private void BuildBase()
    {
        CurrentModel!.Children.Clear();
        string? cabType = CabinetType;
        double width = ConvertDimension.FractionToDouble(WidthBaseCab!);
        double height = ConvertDimension.FractionToDouble(HeightBaseCab!);
        double depth = ConvertDimension.FractionToDouble(DepthBaseCab!);
        double backThickness = ConvertDimension.FractionToDouble(BaseCabBackThickness!);
        if (backThickness == 0.25) { depth -= backThickness; }
        double leftFrontWidth = ConvertDimension.FractionToDouble(LeftFrontWidthBase!);
        double rightFrontWidth = ConvertDimension.FractionToDouble(RightFrontWidthBase!);
        double leftDepth = ConvertDimension.FractionToDouble(LeftDepthBase!);
        double rightDepth = ConvertDimension.FractionToDouble(RightDepthBase!);
        double leftBackWidth = ConvertDimension.FractionToDouble(LeftDepthBase!);
        double rightBackWidth = ConvertDimension.FractionToDouble(RightBackWidthBase!);
        double tk_Height = ConvertDimension.FractionToDouble(TKHeight!);
        double tk_Depth = ConvertDimension.FractionToDouble(TKDepth!);
        double opening1Height = ConvertDimension.FractionToDouble(Opening1Height!);
        double opening2Height = ConvertDimension.FractionToDouble(Opening2Height!);
        double opening3Height = ConvertDimension.FractionToDouble(Opening3Height!);
        double opening4Height = ConvertDimension.FractionToDouble(Opening4Height!);
        double drwFront1Height = ConvertDimension.FractionToDouble(DrwFront1Height!);
        double drwFront2Height = ConvertDimension.FractionToDouble(DrwFront2Height!);
        double drwFront3Height = ConvertDimension.FractionToDouble(DrwFront3Height!);
        double drwFront4Height = ConvertDimension.FractionToDouble(DrwFront4Height!);

        double interiorWidth = width - (MaterialThickness34 * 2);
        double interiorDepth = depth - backThickness;
        double interiorHeight;
        double shelfDepth;
        if (ShelfDepth == "Half Depth") { shelfDepth = interiorDepth / 2; }
        else { shelfDepth = interiorDepth; }
        shelfDepth -= 0.125;
        double baseDoorGap = ConvertDimension.FractionToDouble(BaseDoorGaps!);
        double doorLeftReveal = ConvertDimension.FractionToDouble(BaseDoorLeftReveal!);
        double doorRightReveal = ConvertDimension.FractionToDouble(BaseDoorRightReveal!);
        double doorTopReveal = ConvertDimension.FractionToDouble(BaseDoorTopReveal!);
        double doorBottomReveal = ConvertDimension.FractionToDouble(BaseDoorBottomReveal!);
        double doorSideReveal = (doorLeftReveal + doorRightReveal) / 2; // this averages the potentially different left and right reveals so that the door creation calc can use just one variable instead of two.
        double halfMaterialThickness34 = MaterialThickness34 / 2; // This is to make door calcs etc. more straightforward
        double doubleMaterialThickness34 = MaterialThickness34 * 2; // This is to make door calcs etc. more straightforward
        double tripleMaterialThickness34 = MaterialThickness34 * 3; // This is to make door calcs etc. more straightforward
        double quadrupleMaterialThickness34 = MaterialThickness34 * 4; // This is to make door calcs etc. more straightforward

        Model3DGroup cabinet = new();

        Model3DGroup leftEnd;
        Model3DGroup rightEnd;
        Model3DGroup deck;
        Model3DGroup top = new();
        Model3DGroup topStretcherFront;
        Model3DGroup topStretcherBack;
        Model3DGroup stretcher;
        //Model3DGroup stretcher1 = new();
        //Model3DGroup stretcher2 = new();
        //Model3DGroup stretcher3 = new();
        //Model3DGroup stretcher4 = new();
        Model3DGroup shelf;
        Model3DGroup toekick = new();
        Model3DGroup back;
        Model3DGroup door1;
        Model3DGroup door2;
        Model3DGroup drwFront1;
        Model3DGroup drwFront2;
        Model3DGroup drwFront3;
        Model3DGroup drwFront4;

        List<Point3D> endPanelPoints;
        List<Point3D> deckPoints;
        List<Point3D> topPoints;
        List<Point3D> toekickPoints;
        List<Point3D> backPoints;
        List<Point3D> stretcherPoints;
        List<Point3D> shelfPoints;
        List<Point3D> doorPoints;
        List<Point3D> drwFrontPoints;

        if (HasTK)
        {
            endPanelPoints = new List<Point3D>
            {
                new (depth,tk_Height,0),
                new (depth,height,0),
                new (0,height,0),
                new (0,0,0),
                new (depth-tk_Depth,0,0),
                new (depth-tk_Depth,tk_Height,0)
            };

        }
        else
        {
            tk_Height = 0;
            tk_Depth = 0;

            endPanelPoints = new List<Point3D>
            {
                new (depth,0,0),
                new (depth,height,0),
                new (0,height,0),
                new (0,0,0)
            };
        }

        interiorHeight = height - (MaterialThickness34 * 2) - tk_Height;

        leftEnd = CreatePanel(endPanelPoints, MaterialThickness34, BaseSpecies!, BaseEdgebanding!, "Vertical");
        rightEnd = CreatePanel(endPanelPoints, MaterialThickness34, BaseSpecies!, BaseEdgebanding!, "Vertical");

        if (cabType == CabTypeStrings.CabTypeBase1 || cabType == CabTypeStrings.CabTypeBase2)
        {
            ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
            ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);
        }


        if (cabType == CabTypeStrings.CabTypeBase1 || cabType == CabTypeStrings.CabTypeBase2)
        {
            // Deck
            deckPoints = new List<Point3D>
            {
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,depth,0),
                new (0,depth,0)
            };
            deck = CreatePanel(deckPoints, MaterialThickness34, BaseSpecies!, BaseEdgebanding!, "Horizontal");
            ApplyTransform(deck, -(interiorWidth / 2), -depth, tk_Height, 270, 0, 0);

            // Full Top
            if (TopType == "Full")
            {
                topPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,depth,0),
                    new (0,depth,0)
                };
                top = CreatePanel(topPoints, MaterialThickness34, BaseSpecies!, BaseEdgebanding!, "Horizontal");
                ApplyTransform(top, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
            }

            // Stretcher Top
            if (TopType == "Stretcher")
            {
                topPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,StretcherWidth,0),
                    new (0,StretcherWidth,0)
                };
                topStretcherFront = CreatePanel(topPoints, MaterialThickness34, BaseSpecies!, BaseEdgebanding!, "Horizontal");
                topStretcherBack = CreatePanel(topPoints, MaterialThickness34, BaseSpecies!, "None", "Horizontal");

                ApplyTransform(topStretcherFront, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
                ApplyTransform(topStretcherBack, -(interiorWidth / 2), -StretcherWidth, height - MaterialThickness34, 270, 0, 0);
                top.Children.Add(topStretcherFront);
                top.Children.Add(topStretcherBack);
            }

            // Toekick
            if (HasTK)
            {
                toekickPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,tk_Height-.5,0),
                    new (0,tk_Height-.5,0)
                };
                toekick = CreatePanel(toekickPoints, MaterialThickness34, BaseSpecies!, "None", "Horizontal");
                ApplyTransform(toekick, -(interiorWidth / 2), 0.5, depth - tk_Depth - MaterialThickness34, 0, 0, 0); // The hardcoded 1/2" here is because the actual toekick board is 1/2" narrower than the specified toekick height
            }

            // Back
            if (backThickness == 0.75)
            {
                backPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,interiorHeight,0),
                    new (0,interiorHeight,0)
                };
                back = CreatePanel(backPoints, MaterialThickness34, BaseSpecies!, "None", "Vertical");
                ApplyTransform(back, -(interiorWidth / 2), MaterialThickness34 + tk_Height, 0, 0, 0, 0);
            }
            else
            {
                backPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (width,0,0),
                    new (width,height-tk_Height,0),
                    new (0,height-tk_Height,0)
                };
                back = CreatePanel(backPoints, MaterialThickness14, BaseSpecies!, "None", "Vertical");
                ApplyTransform(back, -(width / 2), tk_Height, -MaterialThickness14, 0, 0, 0);
            }

            // Drawer Stretchers
            if (CabinetType == CabTypeStrings.CabTypeBase1)
            {
                if (DrawerCount == 1)
                {
                    stretcherPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,StretcherWidth,0),
                        new (0,StretcherWidth,0)
                    };
                    stretcher = CreatePanel(stretcherPoints, MaterialThickness34, BaseSpecies!, BaseEdgebanding!, "Horizontal");
                    ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - doubleMaterialThickness34 - opening1Height, 270, 0, 0);
                    cabinet.Children.Add(stretcher);
                }
            }

            if (CabinetType == CabTypeStrings.CabTypeBase2)
            {
                if (DrawerCount > 1)
                {
                    stretcherPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,StretcherWidth,0),
                        new (0,StretcherWidth,0)
                    };
                    stretcher = CreatePanel(stretcherPoints, MaterialThickness34, BaseSpecies!, BaseEdgebanding!, "Horizontal");
                    ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - doubleMaterialThickness34 - opening1Height, 270, 0, 0);
                    cabinet.Children.Add(stretcher);
                }

                if (DrawerCount > 2)
                {
                    stretcherPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,StretcherWidth,0),
                        new (0,StretcherWidth,0)
                    };
                    stretcher = CreatePanel(stretcherPoints, MaterialThickness34, BaseSpecies!, BaseEdgebanding!, "Horizontal");
                    ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - doubleMaterialThickness34 - opening1Height - MaterialThickness34 - opening2Height, 270, 0, 0);
                    cabinet.Children.Add(stretcher);
                }

                if (DrawerCount > 3)
                {
                    stretcherPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,StretcherWidth,0),
                        new (0,StretcherWidth,0)
                    };
                    stretcher = CreatePanel(stretcherPoints, MaterialThickness34, BaseSpecies!, BaseEdgebanding!, "Horizontal");
                    ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - doubleMaterialThickness34 - opening1Height - MaterialThickness34 - opening2Height - MaterialThickness34 - opening3Height, 270, 0, 0);
                    cabinet.Children.Add(stretcher);
                }


            }

            // Shelves
            if (CabinetType != CabTypeStrings.CabTypeBase2)
            {
                double shelfSpacing = interiorHeight - opening1Height + MaterialThickness34; // This should be the space between the shelves
                if (HasTK) { shelfSpacing += tk_Height * 2; } // why the fuck does this work - oh well, it does.
                shelfSpacing /= (BaseShelfCount + 1);


                for (int i = 1; i < BaseShelfCount + 1; i++)
                {
                    shelfPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (interiorWidth-.125,0,0),
                        new (interiorWidth-.125,shelfDepth,0),
                        new (0,shelfDepth,0)
                    };
                    shelf = CreatePanel(shelfPoints, MaterialThickness34, BaseSpecies!, BaseEdgebanding!, "Horizontal");
                    ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -MaterialThickness34 - shelfDepth, i * shelfSpacing, 270, 0, 0);
                    cabinet.Children.Add(shelf);
                }
            }

            // Doors
            if (BaseDoorCount > 0 && BaseIncDoors)
            {
                double doorWidth = width - (doorSideReveal * 2);
                double doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height;

                if (CabinetType == CabTypeStrings.CabTypeBase1 && DrawerCount == 1)
                {
                    doorHeight = height - opening1Height - MaterialThickness34 - halfMaterialThickness34 - (baseDoorGap / 2) - doorBottomReveal - tk_Height;
                }


                if (BaseDoorCount == 1)
                {
                    doorPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (doorWidth,0,0),
                        new (doorWidth,doorHeight,0),
                        new (0,doorHeight,0)
                    };
                    door1 = CreatePanel(doorPoints, MaterialThickness34, BaseDoorSpecies!, "None", BaseDoorGrainDirection!);
                    if (!HasTK)
                    {
                        ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                    }
                    else
                    {
                        ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal + tk_Height, depth, 0, 0, 0);
                    }
                    cabinet.Children.Add(door1);
                }

                if (BaseDoorCount == 2)
                {
                    doorWidth = (doorWidth / 2) - .0625;

                    doorPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (doorWidth,0,0),
                        new (doorWidth, doorHeight, 0),
                        new (0,doorHeight,0)
                    };
                    door1 = CreatePanel(doorPoints, MaterialThickness34, BaseDoorSpecies!, "None", BaseDoorGrainDirection!);
                    door2 = CreatePanel(doorPoints, MaterialThickness34, BaseDoorSpecies!, "None", BaseDoorGrainDirection!);
                    if (!HasTK)
                    {
                        ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                        ApplyTransform(door2, (width / 2) - doorWidth - doorRightReveal, doorBottomReveal, depth, 0, 0, 0);
                    }
                    else
                    {
                        ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal + tk_Height, depth, 0, 0, 0);
                        ApplyTransform(door2, (width / 2) - doorWidth - doorRightReveal, doorBottomReveal + tk_Height, depth, 0, 0, 0);
                    }
                    cabinet.Children.Add(door1);
                    cabinet.Children.Add(door2);
                }

            }


            // Drawer Fronts
            double drwFrontWidth = width - (doorSideReveal * 2);

            if (BaseIncDrwFronts)
            {
                if (CabinetType == CabTypeStrings.CabTypeBase1 && DrawerCount == 1)
                {
                    drwFront1Height = opening1Height + (MaterialThickness34 - doorTopReveal) + (halfMaterialThickness34 - (baseDoorGap / 2));

                    drwFrontPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (drwFrontWidth,0,0),
                    new (drwFrontWidth,drwFront1Height,0),
                    new (0,drwFront1Height,0)
                };
                    drwFront1 = CreatePanel(drwFrontPoints, MaterialThickness34, BaseDoorSpecies!, "None", BaseDrwFrontGrainDirection!);
                    ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                    cabinet.Children.Add(drwFront1);

                }

                if (CabinetType == CabTypeStrings.CabTypeBase2)
                {
                    if (DrawerCount == 1)
                    {
                        drwFront1Height = height - doorTopReveal - doorBottomReveal - tk_Height;

                        drwFrontPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (drwFrontWidth,0,0),
                            new (drwFrontWidth,drwFront1Height,0),
                            new (0,drwFront1Height,0)
                        };
                        drwFront1 = CreatePanel(drwFrontPoints, MaterialThickness34, BaseDoorSpecies!, "None", BaseDrwFrontGrainDirection!);
                        ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                        cabinet.Children.Add(drwFront1);
                    }

                    if (DrawerCount > 1)
                    {
                        // Top Drawer

                        drwFront1Height = (opening1Height + doubleMaterialThickness34) - doorTopReveal - halfMaterialThickness34 - (baseDoorGap / 2);
                        drwFrontPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (drwFrontWidth,0,0),
                            new (drwFrontWidth,drwFront1Height,0),
                            new (0,drwFront1Height,0)
                        };
                        drwFront1 = CreatePanel(drwFrontPoints, MaterialThickness34, BaseDoorSpecies!, "None", BaseDrwFrontGrainDirection!);
                        ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                        cabinet.Children.Add(drwFront1);

                        // Second Drawer

                        if (DrawerCount == 2) // if true, this is the bottom drawer
                        {
                            drwFront2Height = opening2Height + (MaterialThickness34 * 1.5) - doorBottomReveal - (baseDoorGap / 2); // if the bottom drawer
                        }
                        else
                        {
                            drwFront2Height = opening2Height + MaterialThickness34 - baseDoorGap; // if NOT the bottom drawer
                        }

                        drwFrontPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (drwFrontWidth,0,0),
                            new (drwFrontWidth,drwFront2Height,0),
                            new (0,drwFront2Height,0)
                        };
                        drwFront2 = CreatePanel(drwFrontPoints, MaterialThickness34, BaseDoorSpecies!, "None", BaseDrwFrontGrainDirection!);
                        ApplyTransform(drwFront2,
                            -(width / 2) + doorLeftReveal,
                            height - drwFront2Height - opening1Height - (2 * MaterialThickness34) + halfMaterialThickness34 - (baseDoorGap / 2),
                            depth,
                            0, 0, 0);

                        cabinet.Children.Add(drwFront2);

                        if (DrawerCount > 2)
                        {
                            // Third Drawer

                            if (DrawerCount == 3) // if true, this is the bottom drawer
                            {
                                drwFront3Height = opening3Height + (MaterialThickness34 * 1.5) - doorBottomReveal - (baseDoorGap / 2); // if the bottom drawer
                            }
                            else
                            {
                                drwFront3Height = opening3Height + MaterialThickness34 - baseDoorGap; // if NOT the bottom drawer
                            }

                            drwFrontPoints = new List<Point3D>
                            {
                                new (0,0,0),
                                new (drwFrontWidth,0,0),
                                new (drwFrontWidth,drwFront3Height,0),
                                new (0,drwFront3Height,0)
                            };
                            drwFront3 = CreatePanel(drwFrontPoints, MaterialThickness34, BaseDoorSpecies!, "None", BaseDrwFrontGrainDirection!);
                            ApplyTransform(drwFront3,
                                -(width / 2) + doorLeftReveal,
                                height - drwFront3Height - opening1Height - opening2Height - (3 * MaterialThickness34) + halfMaterialThickness34 - (baseDoorGap / 2),
                                depth,
                                0, 0, 0);

                            cabinet.Children.Add(drwFront3);

                        }



                        // Fourth Drawer
                        if (DrawerCount > 3)
                        {
                            if (DrawerCount == 4) // if true, this is the bottom drawer
                            {
                                drwFront4Height = opening4Height + (MaterialThickness34 * 1.5) - doorBottomReveal - (baseDoorGap / 2); // if the bottom drawer
                            }
                            else
                            {
                                drwFront4Height = opening4Height + MaterialThickness34 - baseDoorGap; // if NOT the bottom drawer
                            }

                            drwFrontPoints = new List<Point3D>
                            {
                                new (0,0,0),
                                new (drwFrontWidth,0,0),
                                new (drwFrontWidth,drwFront4Height,0),
                                new (0,drwFront4Height,0)
                            };
                            drwFront4 = CreatePanel(drwFrontPoints, MaterialThickness34, BaseDoorSpecies!, "None", BaseDrwFrontGrainDirection!);
                            ApplyTransform(drwFront4,
                                -(width / 2) + doorLeftReveal,
                                height - drwFront4Height - opening1Height - opening2Height - opening3Height - (4 * MaterialThickness34) + halfMaterialThickness34 - (baseDoorGap / 2),
                                depth,
                                0, 0, 0);

                            cabinet.Children.Add(drwFront4);
                        }

                    }
                }
            }

            cabinet.Children.Add(leftEnd);
            cabinet.Children.Add(rightEnd);
            cabinet.Children.Add(deck);
            cabinet.Children.Add(top);
            cabinet.Children.Add(toekick);
            cabinet.Children.Add(back);

            CurrentModel = cabinet;
        }


    }



    private static Model3DGroup CreatePanel(List<Point3D> polygonPoints, double matlThickness, string panelSpecies, string edgebandingSpecies, string grainDirection)
    {
        panelSpecies ??= "Prefinished Ply";
        edgebandingSpecies ??= "Wood Maple";

        double thickness = matlThickness;

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

        // Add top positions at z=1 with same texture coords (or flipped if needed)
        int topOffset = polygonPoints.Count;
        foreach (var point in polygonPoints)
        {
            mainBuilder.Positions.Add(new Point3D(point.X, point.Y, thickness));
            double u = (point.X - minX) / (maxX - minX);
            double v = (point.Y - minY) / (maxY - minY);
            mainBuilder.TextureCoordinates.Add(new Point(u, v)); // Same as bottom for simple mapping
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


        // In the side faces loop (replace the existing loop):
        double cumulativeU = 0;
        for (int edgeFace = 0; edgeFace < polygonPoints.Count; edgeFace++)
        {
            int b0 = edgeFace; // bottom index
            int b1 = (edgeFace + 1) % polygonPoints.Count; // next bottom
            int t1 = b1 + topOffset; // next top
            int t0 = b0 + topOffset; // top

            // Texture coords for quad: unwrap horizontally (U cumulative perimeter, V height 0-1)
            double u0 = cumulativeU / perimeter;
            double u1 = (cumulativeU + sideLengths[edgeFace]) / perimeter;
            Point uvBottomLeft = new(u0, 0);  // bottom b0
            Point uvBottomRight = new(u1, 0); // bottom b1
            Point uvTopRight = new(u1, 1);    // top t1
            Point uvTopLeft = new(u0, 1);     // top t0

            // Add quad to appropriate builder (special for edgeFace==0, e.g., first side)
            if (edgeFace == 0) // Edge(s) to show edgeband texture
            {
                specialBuilder.AddQuad(
                mainBuilder.Positions[b0],  // Note: positions are shared or duplicate if needed; but since separate meshes, add to special
                mainBuilder.Positions[b1],
                mainBuilder.Positions[t1],
                mainBuilder.Positions[t0],
                uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
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

            cumulativeU += sideLengths[edgeFace];
        }

        // Compute normals for proper lighting
        mainBuilder.ComputeNormalsAndTangents(MeshFaces.Default);
        specialBuilder.ComputeNormalsAndTangents(MeshFaces.Default);

        // Convert to a MeshGeometry3D, freezing for performance
        var mesh = mainBuilder.ToMesh(true);
        var specialMesh = specialBuilder.ToMesh(true);


        // Apply material texture to panel
        BitmapImage panelSpeciesImage = new();
        panelSpeciesImage.BeginInit();
        panelSpeciesImage.UriSource = new Uri($"Images/Plywood/{panelSpecies} - {grainDirection}.png", UriKind.Relative);
        panelSpeciesImage.EndInit();
        var panelSpeciesBrush = new ImageBrush(panelSpeciesImage) { TileMode = TileMode.Tile, ViewportUnits = BrushMappingMode.Absolute, Viewport = new Rect(0, 0, 1, 1) };

        // Apply material texture to edgebanding
        ImageBrush edgebandingSpeciesBrush = new();
        if (edgebandingSpecies != "None")
        {
            BitmapImage edgebandingSpeciesImage = new();
            edgebandingSpeciesImage.BeginInit();
            edgebandingSpeciesImage.UriSource = new Uri($"Images/Edgebanding/{edgebandingSpecies}.png", UriKind.Relative);
            edgebandingSpeciesImage.EndInit();
            edgebandingSpeciesBrush = new ImageBrush(edgebandingSpeciesImage) { TileMode = TileMode.Tile, Viewport = new Rect(0, 0, 1, 1) };
        }
        else // This colors the edge with the selected plywood species if edgebandingSpecies is set to "None":
        {
            BitmapImage edgebandingSpeciesImage = new();
            edgebandingSpeciesImage.BeginInit();
            edgebandingSpeciesImage.UriSource = new Uri($"Images/Plywood/{panelSpecies} - {grainDirection}.png", UriKind.Relative);
            edgebandingSpeciesImage.EndInit();
            edgebandingSpeciesBrush = new ImageBrush(edgebandingSpeciesImage) { TileMode = TileMode.Tile, Viewport = new Rect(0, 0, 1, 1) };
        }

        // Create a material with texture
        var material = new DiffuseMaterial(panelSpeciesBrush);
        var specialMaterial = new DiffuseMaterial(edgebandingSpeciesBrush);

        // Create a GeometryModel3D
        var panelModel = new GeometryModel3D
        {
            Geometry = mesh,
            Material = material,
            BackMaterial = material // Visible from both sides
        };

        var edgebandingModel = new GeometryModel3D { Geometry = specialMesh, Material = specialMaterial, BackMaterial = specialMaterial };


        // Create a ModelVisual3D and add to the viewport
        var partModel = new Model3DGroup();
        partModel.Children.Add(panelModel);
        partModel.Children.Add(edgebandingModel);

        return partModel;

    }


    // Transform method. This allows x, y, and z translation, as well as x, y, and z rotation.
    private static void ApplyTransform(Model3DGroup geometryModel, double translateX, double translateY, double translateZ, double rotateXDegrees, double rotateYDegrees, double rotateZDegrees)
    {

        var transformGroup = new Transform3DGroup();

        // Apply translation
        transformGroup.Children.Add(new TranslateTransform3D(translateX, translateY, translateZ));

        // Apply rotations (around X, Y, Z axes in degrees)
        transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), rotateXDegrees)));
        transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotateYDegrees)));
        transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), rotateZDegrees)));

        // Assign the transform group to the model
        geometryModel.Transform = transformGroup;

    }

}