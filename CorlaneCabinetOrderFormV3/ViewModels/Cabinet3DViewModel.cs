using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using HelixToolkit.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class Cabinet3DViewModel : ObservableObject
{
    private bool _isRebuilding = false; // Add this field

    public Cabinet3DViewModel()
    {
        // Blank constructor for design
    }



    private readonly MainWindowViewModel? _mainVm;
    public Cabinet3DViewModel(MainWindowViewModel mainVm)
    {
        _mainVm = mainVm;
        _mainVm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.CurrentPreviewCabinet))
            {
                if (!_isRebuilding) // Prevent re-entry
                {
                    RebuildPreview();
                }
            }

        };

    }



    [ObservableProperty]
    public partial Model3DGroup? PreviewModel { get; set; }
    partial void OnPreviewModelChanged(Model3DGroup? value)
    {
        // This triggers the attached property every time the model changes
        PreviewBounds = value?.Bounds ?? Rect3D.Empty;
    }



    [ObservableProperty]
    public partial Rect3D PreviewBounds { get; set; }




    [RelayCommand]
    private void LoadInitialModel()
    {
        RebuildPreview();
    }


    public void RebuildPreview()
    {
        if (_isRebuilding)
        {
            Debug.WriteLine($"[Cabinet3DViewModel] Skipping RebuildPreview - already rebuilding");
            return;
        }

        try
        {
            _isRebuilding = true;

            Debug.WriteLine($"[Cabinet3DViewModel] RebuildPreview called. CurrentPreviewCabinet: {_mainVm?.CurrentPreviewCabinet?.Name ?? "null"} (Type: {_mainVm?.CurrentPreviewCabinet?.GetType().Name ?? "null"})");

            var group = new Model3DGroup();

            if (_mainVm?.CurrentPreviewCabinet is CabinetModel cab)
            {
                Debug.WriteLine($"[Cabinet3DViewModel] Building cabinet. Type: {cab.GetType().Name}, BaseCabType: {(cab as BaseCabinetModel)?.BaseCabType ?? "N/A"}");
                var built = BuildCabinet(cab);
                Debug.WriteLine($"[Cabinet3DViewModel] Built cabinet has {built.Children.Count} children");
                group.Children.Add(built);
            }
            else
            {
                Debug.WriteLine($"[Cabinet3DViewModel] WARNING: CurrentPreviewCabinet is null or not a CabinetModel!");
            }

            // Lights
            group.Children.Add(new DirectionalLight(Colors.DarkGray, new Vector3D(-1, -1, -1)));

            Debug.WriteLine($"[Cabinet3DViewModel] Setting PreviewModel with {group.Children.Count} total children");
            PreviewModel = group;
        }
        finally
        {
            _isRebuilding = false;
        }
    }



    private Model3DGroup BuildCabinet(CabinetModel cab)
    {
        Model3DGroup cabinet = new();

        Model3DGroup leftEnd;
        Model3DGroup rightEnd;
        Model3DGroup deck;
        Model3DGroup top = new();
        Model3DGroup topStretcherFront;
        Model3DGroup topStretcherBack;
        Model3DGroup stretcher;
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


        double MaterialThickness34 = 0.75;
        double MaterialThickness14 = 0.25;
        double StretcherWidth = 6;
        // Your full pasted code here, but with 'cab' instead of local variables
        // Use 'if (cab is BaseCabinetModel b)' etc.
        // It will work perfectly
        if (cab is BaseCabinetModel baseCab)
        {
            if (string.IsNullOrWhiteSpace(baseCab.BaseCabType)) { baseCab.BaseCabType = "Standard"; }
            if (string.IsNullOrWhiteSpace(baseCab.Width)) { baseCab.Width = "18"; }
            if (string.IsNullOrWhiteSpace(baseCab.Height)) { baseCab.Height = "34.5"; }
            if (string.IsNullOrWhiteSpace(baseCab.Depth)) { baseCab.Depth = "24"; }

            string? cabType = baseCab.BaseCabType;
            double width = ConvertDimension.FractionToDouble(baseCab.Width);
            double height = ConvertDimension.FractionToDouble(baseCab.Height);
            double depth = ConvertDimension.FractionToDouble(baseCab.Depth);
            double backThickness = ConvertDimension.FractionToDouble(baseCab.BackThickness);
            if (backThickness == 0.25) { depth -= backThickness; }
            double leftFrontWidth = ConvertDimension.FractionToDouble(baseCab.LeftFrontWidth);
            double rightFrontWidth = ConvertDimension.FractionToDouble(baseCab.RightFrontWidth);
            double leftDepth = ConvertDimension.FractionToDouble(baseCab.LeftDepth);
            double rightDepth = ConvertDimension.FractionToDouble(baseCab.RightDepth);
            double leftBackWidth = ConvertDimension.FractionToDouble(baseCab.LeftBackWidth);
            double rightBackWidth = ConvertDimension.FractionToDouble(baseCab.RightBackWidth);
            double tk_Height = ConvertDimension.FractionToDouble(baseCab.TKHeight ?? "4");
            double tk_Depth = ConvertDimension.FractionToDouble(baseCab.TKDepth ?? "3");
            double opening1Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1);
            double opening2Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2);
            double opening3Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3);
            double opening4Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight4);
            double drwFront1Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight1);
            double drwFront2Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight2);
            double drwFront3Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight3);
            double drwFront4Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight4);
            double interiorWidth = width - (MaterialThickness34 * 2);
            double interiorDepth = depth - backThickness;
            double interiorHeight;
            double shelfDepth;
            if (baseCab.ShelfDepth == "Half Depth") { shelfDepth = interiorDepth / 2; }
            else { shelfDepth = interiorDepth; }
            shelfDepth -= 0.125;
            double baseDoorGap = ConvertDimension.FractionToDouble(baseCab.GapWidth);
            double doorLeftReveal = ConvertDimension.FractionToDouble(baseCab.LeftReveal);
            double doorRightReveal = ConvertDimension.FractionToDouble(baseCab.RightReveal);
            double doorTopReveal = ConvertDimension.FractionToDouble(baseCab.TopReveal);
            double doorBottomReveal = ConvertDimension.FractionToDouble(baseCab.BottomReveal);
            double doorSideReveal = (doorLeftReveal + doorRightReveal) / 2; // this averages the potentially different left and right reveals so that the door creation calc can use just one variable instead of two.
            double halfMaterialThickness34 = MaterialThickness34 / 2; // This is to make door calcs etc. more straightforward
            double doubleMaterialThickness34 = MaterialThickness34 * 2; // This is to make door calcs etc. more straightforward
            double tripleMaterialThickness34 = MaterialThickness34 * 3; // This is to make door calcs etc. more straightforward
            double quadrupleMaterialThickness34 = MaterialThickness34 * 4; // This is to make door calcs etc. more straightforward



            //Apparently these are null on app launch. Uncommenting these causes a cabinet to appear. 
            //cabType = "Standard";
            //height = 34;
            //width = 18;
            //depth = 24;
            //backThickness = .75;
            //tk_Depth = 3;
            //tk_Height = 4;






            if (baseCab.HasTK)
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

            leftEnd = CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", cab);
            rightEnd = CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", cab);

            if (cabType.Contains("Standard") || cabType.Contains("Drawer"))
            {
                ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
                ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);
            }


            if (cabType.Contains("Standard") || cabType.Contains("Drawer"))
            {
                // Deck
                deckPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,depth,0),
                    new (0,depth,0)
                };
                deck = CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab);
                ApplyTransform(deck, -(interiorWidth / 2), -depth, tk_Height, 270, 0, 0);

                // Full Top
                if (baseCab.TopType == "Full")
                {
                    topPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,depth,0),
                        new (0,depth,0)
                    };
                    top = CreatePanel(topPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab);
                    ApplyTransform(top, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
                }

                else
                {
                    topPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,StretcherWidth,0),
                        new (0,StretcherWidth,0)
                    };
                    topStretcherFront = CreatePanel(topPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab);
                    topStretcherBack = CreatePanel(topPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", cab);

                    ApplyTransform(topStretcherFront, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
                    ApplyTransform(topStretcherBack, -(interiorWidth / 2), -StretcherWidth, height - MaterialThickness34, 270, 0, 0);
                    top.Children.Add(topStretcherFront);
                    top.Children.Add(topStretcherBack);
                }

                // Toekick
                if (baseCab.HasTK)
                {
                    toekickPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,tk_Height-.5,0),
                        new (0,tk_Height-.5,0)
                    };
                    toekick = CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", cab);
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
                    back = CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", cab);
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
                    back = CreatePanel(backPoints, MaterialThickness14, baseCab.Species, "None", "Vertical", cab);
                    ApplyTransform(back, -(width / 2), tk_Height, -MaterialThickness14, 0, 0, 0);
                }

                // Drawer Stretchers
                if (cabType == "Standard")
                {
                    if (baseCab.DrwCount == 1)
                    {
                        stretcherPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (interiorWidth,0,0),
                            new (interiorWidth,StretcherWidth,0),
                            new (0,StretcherWidth,0)
                        };
                        stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab);
                        ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - doubleMaterialThickness34 - opening1Height, 270, 0, 0);
                        cabinet.Children.Add(stretcher);
                    }
                }

                if (cabType == "Drawer")
                {
                    if (baseCab.DrwCount > 1)
                    {
                        stretcherPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (interiorWidth,0,0),
                            new (interiorWidth,StretcherWidth,0),
                            new (0,StretcherWidth,0)
                        };
                        stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab);
                        ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - doubleMaterialThickness34 - opening1Height, 270, 0, 0);
                        cabinet.Children.Add(stretcher);
                    }

                    if (baseCab.DrwCount > 2)
                    {
                        stretcherPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (interiorWidth,0,0),
                            new (interiorWidth,StretcherWidth,0),
                            new (0,StretcherWidth,0)
                        };
                        stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab);
                        ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - doubleMaterialThickness34 - opening1Height - MaterialThickness34 - opening2Height, 270, 0, 0);
                        cabinet.Children.Add(stretcher);
                    }

                    if (baseCab.DrwCount > 3)
                    {
                        stretcherPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (interiorWidth,0,0),
                            new (interiorWidth,StretcherWidth,0),
                            new (0,StretcherWidth,0)
                        };
                        stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab);
                        ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - doubleMaterialThickness34 - opening1Height - MaterialThickness34 - opening2Height - MaterialThickness34 - opening3Height, 270, 0, 0);
                        cabinet.Children.Add(stretcher);
                    }

                }

                // Shelves
                if (cabType != "Drawer")
                {
                    double shelfSpacing = interiorHeight - opening1Height + MaterialThickness34; // This should be the space between the shelves
                    if (baseCab.HasTK) { shelfSpacing += tk_Height * 2; } // why the fuck does this work - oh well, it does.
                    shelfSpacing /= (baseCab.ShelfCount + 1);


                    for (int i = 1; i < baseCab.ShelfCount + 1; i++)
                    {
                        shelfPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (interiorWidth-.125,0,0),
                            new (interiorWidth-.125,shelfDepth,0),
                            new (0,shelfDepth,0)
                        };
                        shelf = CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab);
                        ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -MaterialThickness34 - shelfDepth, i * shelfSpacing, 270, 0, 0);
                        cabinet.Children.Add(shelf);
                    }
                }

                // Doors
                if (baseCab.DoorCount > 0 && baseCab.IncDoors && cabType is not "Drawer")
                {
                    double doorWidth = width - (doorSideReveal * 2);
                    double doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height;

                    if (cabType == "Standard" && baseCab.DrwCount == 1)
                    {
                        doorHeight = height - opening1Height - MaterialThickness34 - halfMaterialThickness34 - (baseDoorGap / 2) - doorBottomReveal - tk_Height;
                    }


                    if (baseCab.DoorCount == 1)
                    {
                        doorPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (doorWidth,0,0),
                            new (doorWidth,doorHeight,0),
                            new (0,doorHeight,0)
                        };
                        door1 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DoorGrainDir, cab);
                        if (!baseCab.HasTK)
                        {
                            ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                        }
                        else
                        {
                            ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal + tk_Height, depth, 0, 0, 0);
                        }
                        cabinet.Children.Add(door1);
                    }

                    if (baseCab.DoorCount == 2)
                    {
                        doorWidth = (doorWidth / 2) - (baseDoorGap/2);

                        doorPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (doorWidth,0,0),
                            new (doorWidth, doorHeight, 0),
                            new (0,doorHeight,0)
                        };
                        door1 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DoorGrainDir, cab);
                        door2 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DoorGrainDir, cab);
                        if (!baseCab.HasTK)
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

                if (baseCab.IncDrwFronts)
                {
                    if (cabType == "Standard" && baseCab.DrwCount == 1)
                    {
                        drwFront1Height = opening1Height + (MaterialThickness34 - doorTopReveal) + (halfMaterialThickness34 - (baseDoorGap / 2));

                        drwFrontPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (drwFrontWidth,0,0),
                        new (drwFrontWidth,drwFront1Height,0),
                        new (0,drwFront1Height,0)
                    };
                        drwFront1 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DrwFrontGrainDir, cab);
                        ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                        cabinet.Children.Add(drwFront1);

                    }

                    if (cabType == "Drawer")
                    {
                        if (baseCab.DrwCount == 1)
                        {
                            drwFront1Height = height - doorTopReveal - doorBottomReveal - tk_Height;

                            drwFrontPoints = new List<Point3D>
                            {
                                new (0,0,0),
                                new (drwFrontWidth,0,0),
                                new (drwFrontWidth,drwFront1Height,0),
                                new (0,drwFront1Height,0)
                            };
                            drwFront1 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DrwFrontGrainDir, cab);
                            ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                            cabinet.Children.Add(drwFront1);
                        }

                        if (baseCab.DrwCount > 1)
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
                            drwFront1 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DrwFrontGrainDir, cab);
                            ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                            cabinet.Children.Add(drwFront1);

                            // Second Drawer

                            if (baseCab.DrwCount == 2) // if true, this is the bottom drawer
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
                            drwFront2 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DrwFrontGrainDir, cab);
                            ApplyTransform(drwFront2,
                                -(width / 2) + doorLeftReveal,
                                height - drwFront2Height - opening1Height - (2 * MaterialThickness34) + halfMaterialThickness34 - (baseDoorGap / 2),
                                depth,
                                0, 0, 0);

                            cabinet.Children.Add(drwFront2);

                            if (baseCab.DrwCount > 2)
                            {
                                // Third Drawer

                                if (baseCab.DrwCount == 3) // if true, this is the bottom drawer
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
                                drwFront3 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DrwFrontGrainDir, cab);
                                ApplyTransform(drwFront3,
                                    -(width / 2) + doorLeftReveal,
                                    height - drwFront3Height - opening1Height - opening2Height - (3 * MaterialThickness34) + halfMaterialThickness34 - (baseDoorGap / 2),
                                    depth,
                                    0, 0, 0);

                                cabinet.Children.Add(drwFront3);

                            }



                            // Fourth Drawer
                            if (baseCab.DrwCount > 3)
                            {
                                if (baseCab.DrwCount == 4) // if true, this is the bottom drawer
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
                                drwFront4 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DrwFrontGrainDir, cab);
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

            }

        }

        // other cabinet types here

        return cabinet;
    }



    // This lovely bit of kit will create a panel of any size, shape, thickness, material species, etc. and edgeband it
    private static Model3DGroup CreatePanel(List<Point3D> polygonPoints, double matlThickness, string panelSpecies, string edgebandingSpecies, string grainDirection, CabinetModel cab)
    {
        //panelSpecies ??= "Prefinished Ply";
        //edgebandingSpecies ??= "Wood Maple";

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
 



        // Create a material with texture
        var material = GetPlywoodSpecies(panelSpecies, grainDirection);
        var specialMaterial = GetEdgeBandingSpecies(edgebandingSpecies);
        if (edgebandingSpecies == "None")
        {
            specialMaterial = GetPlywoodSpecies(panelSpecies, grainDirection);
        }

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

    private static Material GetPlywoodSpecies(string? panelSpecies, string? grainDirection)
    {
        // Provide defaults if null or empty
        panelSpecies ??= "Prefinished Ply";
        grainDirection ??= "Horizontal";

        if (string.IsNullOrWhiteSpace(panelSpecies))
            panelSpecies = "Prefinished Ply";
        if (string.IsNullOrWhiteSpace(grainDirection))
            grainDirection = "Horizontal";

        Debug.WriteLine($"[GetPlywoodSpecies] Attempting to load: species='{panelSpecies}', grain='{grainDirection}'");

        string resourcePath = $"pack://application:,,,/Images/Plywood/{panelSpecies} - {grainDirection}.png";

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(resourcePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            Debug.WriteLine($"[GetPlywoodSpecies] SUCCESS loading: {resourcePath}");

            var brush = new ImageBrush(bitmap)
            {
                TileMode = TileMode.Tile,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, 1, 1)
            };

            return new DiffuseMaterial(brush);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GetPlywoodSpecies] ERROR loading texture '{resourcePath}': {ex.GetType().Name} - {ex.Message}");
            // Fallback to solid color
            return new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(200, 200, 200)));
        }
    }

    private static Material GetEdgeBandingSpecies(string? species)
    {
        Debug.WriteLine($"[GetEdgeBandingSpecies] Attempting to load: species='{species ?? "null"}'");

        // Handle "None" or null
        if (string.IsNullOrWhiteSpace(species) || species == "None")
        {
            Debug.WriteLine($"[GetEdgeBandingSpecies] Using solid color for 'None' or null");
            return new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(139, 69, 19))); // Wood brown
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

            Debug.WriteLine($"[GetEdgeBandingSpecies] SUCCESS loading: {resourcePath}");

            var brush = new ImageBrush(bitmap)
            {
                TileMode = TileMode.Tile,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, 1, 1)
            };

            return new DiffuseMaterial(brush);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GetEdgeBandingSpecies] ERROR loading edgebanding texture '{resourcePath}': {ex.GetType().Name} - {ex.Message}");
            // Fallback to solid color
            return new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(139, 69, 19)));
        }
    }

}

