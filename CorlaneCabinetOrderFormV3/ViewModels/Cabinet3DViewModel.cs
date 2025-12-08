using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using HelixToolkit.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class Cabinet3DViewModel : ObservableObject
{
    public Cabinet3DViewModel()
    {
        // Blank constructor for design
    }

    private readonly MainWindowViewModel? _mainVm;
    private readonly IPreviewService _previewSvc;

    // Constructor used by DI - MainWindowViewModel and IPreviewService injected
    public Cabinet3DViewModel(MainWindowViewModel mainVm, IPreviewService previewSvc)
    {
        _mainVm = mainVm;
        _previewSvc = previewSvc;

        // Subscribe to preview changes from the centralized service.
        // Keep the handler light — dispatch to UI thread and rebuild.
        _previewSvc.PreviewChanged += PreviewSvc_PreviewChanged;

        // Initial build if a preview is already present
        if (_previewSvc.CurrentPreviewCabinet != null)
        {
            RebuildPreview();
        }
    }

    private void PreviewSvc_PreviewChanged(object? sender, EventArgs e)
    {
        // Ensure RebuildPreview runs on UI thread and with Render priority.
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
        {
            RebuildPreview();
        }));
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
        var group = new Model3DGroup();

        // Read preview cabinet from the centralized preview service
        var cab = _previewSvc.CurrentPreviewCabinet;

        if (cab is CabinetModel cabinetModel)
        {
            var built = BuildCabinet(cabinetModel);
            group.Children.Add(built);
        }
        else
        {
            // No preview to show - group will contain only lighting.
        }

        // Lights
        group.Children.Add(new DirectionalLight(Colors.DarkGray, new Vector3D(-1, -1, -1)));

        PreviewModel = group;
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
        Model3DGroup leftBack;
        Model3DGroup rightBack;
        Model3DGroup door1;
        Model3DGroup door2;
        Model3DGroup drwFront1;
        Model3DGroup drwFront2;
        Model3DGroup drwFront3;
        Model3DGroup drwFront4;

        List<Point3D> endPanelPoints = new();
        List<Point3D> leftEndPanelPoints = new();
        List<Point3D> rightEndPanelPoints = new();
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
        double halfMaterialThickness34 = MaterialThickness34 / 2; // This is to make door calcs etc. more straightforward
        double doubleMaterialThickness34 = MaterialThickness34 * 2; // This is to make door calcs etc. more straightforward
        double tripleMaterialThickness34 = MaterialThickness34 * 3; // This is to make door calcs etc. more straightforward
        double quadrupleMaterialThickness34 = MaterialThickness34 * 4; // This is to make door calcs etc. more straightforward

        double StretcherWidth = 6;

        if (cab is BaseCabinetModel baseCab)
        {
            string? cabType = baseCab.Style;
            string style1 = BaseCabinetViewModel.Style1;
            string style2 = BaseCabinetViewModel.Style2;
            string style3 = BaseCabinetViewModel.Style3;
            string style4 = BaseCabinetViewModel.Style4;

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
            bool topDeck90 = false; // This is sent to the panel creator to let it know if it is a top or deck at 90 degrees so it cab have 2 edgebanded edges
            bool isPanel = false; // This is sent to the panel creator to let it know if it is a panel (true) or a shelf/deck/top/toekick (false) so it can have edgebanding applied correctly.
            int shelfCount = baseCab.ShelfCount;

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

            interiorHeight = height - doubleMaterialThickness34 - tk_Height;


            leftEnd = CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", cab, topDeck90, isPanel);
            rightEnd = CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", cab, topDeck90, isPanel);

            if (cabType == style1 || cabType == style2)
            {
                ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
                ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);
            }

            if (cabType == style1 || cabType == style2)
            {
                // Deck
                deckPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,depth,0),
                    new (0,depth,0)
                };
                deck = CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab, topDeck90, isPanel);
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
                    top = CreatePanel(topPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab, topDeck90, isPanel);
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
                    topStretcherFront = CreatePanel(topPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab, topDeck90, isPanel);
                    topStretcherBack = CreatePanel(topPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", cab, topDeck90, isPanel);

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
                    toekick = CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", cab, topDeck90, isPanel);
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
                    back = CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", cab, topDeck90, isPanel);
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
                    back = CreatePanel(backPoints, MaterialThickness14, baseCab.Species, "None", "Vertical", cab, topDeck90, isPanel);
                    ApplyTransform(back, -(width / 2), tk_Height, -MaterialThickness14, 0, 0, 0);
                }

                // Drawer Stretchers
                if (cabType == style1)
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
                        stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab, topDeck90, isPanel);
                        ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - doubleMaterialThickness34 - opening1Height, 270, 0, 0);
                        cabinet.Children.Add(stretcher);
                    }
                }

                if (cabType == style2)
                {
                    stretcherPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,StretcherWidth,0),
                        new (0,StretcherWidth,0)
                    };

                    if (baseCab.DrwCount > 1)
                    {
                        stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab, topDeck90, isPanel);
                        ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - doubleMaterialThickness34 - opening1Height, 270, 0, 0);
                        cabinet.Children.Add(stretcher);
                    }

                    if (baseCab.DrwCount > 2)
                    {
                        stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab, topDeck90, isPanel);
                        ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - doubleMaterialThickness34 - opening1Height - MaterialThickness34 - opening2Height, 270, 0, 0);
                        cabinet.Children.Add(stretcher);
                    }

                    if (baseCab.DrwCount > 3)
                    {
                        stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab, topDeck90, isPanel);
                        ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - doubleMaterialThickness34 - opening1Height - MaterialThickness34 - opening2Height - MaterialThickness34 - opening3Height, 270, 0, 0);
                        cabinet.Children.Add(stretcher);
                    }
                }

                // Shelves
                if (cabType != style2)
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
                        shelf = CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", cab, topDeck90, isPanel);
                        ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -MaterialThickness34 - shelfDepth, i * shelfSpacing, 270, 0, 0);
                        cabinet.Children.Add(shelf);
                    }
                }

                // Doors
                if (baseCab.DoorCount > 0 && baseCab.IncDoors && cabType != style2)
                {
                    double doorWidth = width - (doorSideReveal * 2);
                    double doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height;

                    if (cabType == style1 && baseCab.DrwCount == 1)
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
                        door1 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DoorGrainDir, cab, topDeck90, isPanel);
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
                        doorWidth = (doorWidth / 2) - (baseDoorGap / 2);

                        doorPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (doorWidth,0,0),
                            new (doorWidth, doorHeight, 0),
                            new (0,doorHeight,0)
                        };
                        door1 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DoorGrainDir, cab, topDeck90, isPanel);
                        door2 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DoorGrainDir, cab, topDeck90, isPanel);
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
                    if (cabType == style1 && baseCab.DrwCount == 1 && baseCab.IncDrwFront1)
                    {
                        drwFront1Height = opening1Height + (MaterialThickness34 - doorTopReveal) + (halfMaterialThickness34 - (baseDoorGap / 2));

                        drwFrontPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (drwFrontWidth,0,0),
                        new (drwFrontWidth,drwFront1Height,0),
                        new (0,drwFront1Height,0)
                    };
                        drwFront1 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DrwFrontGrainDir, cab, topDeck90, isPanel);
                        ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                        cabinet.Children.Add(drwFront1);

                    }

                    if (cabType == "Drawer")
                    {
                        if (baseCab.DrwCount == 1 && baseCab.IncDrwFront1)
                        {
                            drwFront1Height = height - doorTopReveal - doorBottomReveal - tk_Height;

                            drwFrontPoints = new List<Point3D>
                            {
                                new (0,0,0),
                                new (drwFrontWidth,0,0),
                                new (drwFrontWidth,drwFront1Height,0),
                                new (0,drwFront1Height,0)
                            };
                            drwFront1 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DrwFrontGrainDir, cab, topDeck90, isPanel);
                            ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                            cabinet.Children.Add(drwFront1);
                        }

                        if (baseCab.DrwCount > 1)
                        {
                            // Top Drawer
                            if (baseCab.IncDrwFront1)
                            {
                                drwFront1Height = (opening1Height + doubleMaterialThickness34) - doorTopReveal - halfMaterialThickness34 - (baseDoorGap / 2);
                                drwFrontPoints = new List<Point3D>
                                {
                                    new (0,0,0),
                                    new (drwFrontWidth,0,0),
                                    new (drwFrontWidth,drwFront1Height,0),
                                    new (0,drwFront1Height,0)
                                };
                                drwFront1 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DrwFrontGrainDir, cab, topDeck90, isPanel);
                                ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                                cabinet.Children.Add(drwFront1);
                            }

                            // Second Drawer

                            if (baseCab.IncDrwFront2)
                            {
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
                                drwFront2 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DrwFrontGrainDir, cab, topDeck90, isPanel);
                                ApplyTransform(drwFront2,
                                    -(width / 2) + doorLeftReveal,
                                    height - drwFront2Height - opening1Height - (2 * MaterialThickness34) + halfMaterialThickness34 - (baseDoorGap / 2),
                                    depth,
                                    0, 0, 0);

                                cabinet.Children.Add(drwFront2);
                            }

                            if (baseCab.DrwCount > 2)
                            {
                                // Third Drawer
                                if (baseCab.IncDrwFront3)
                                {
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
                                    drwFront3 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DrwFrontGrainDir, cab, topDeck90, isPanel);
                                    ApplyTransform(drwFront3,
                                        -(width / 2) + doorLeftReveal,
                                        height - drwFront3Height - opening1Height - opening2Height - (3 * MaterialThickness34) + halfMaterialThickness34 - (baseDoorGap / 2),
                                        depth,
                                        0, 0, 0);

                                    cabinet.Children.Add(drwFront3);
                                }

                            }

                            // Fourth Drawer
                            if (baseCab.DrwCount > 3)
                            {
                                if (baseCab.IncDrwFront4)
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
                                    drwFront4 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, "None", baseCab.DrwFrontGrainDir, cab, topDeck90, isPanel);
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
                }
                cabinet.Children.Add(leftEnd);
                cabinet.Children.Add(rightEnd);
                cabinet.Children.Add(deck);
                cabinet.Children.Add(top);
                cabinet.Children.Add(toekick);
                cabinet.Children.Add(back);
            }


            // 90 deg. Corner Cabinets Style 3
            if (cabType == style3)
            {
                // End Panels
                if (baseCab.HasTK)
                {
                    leftEndPanelPoints = new List<Point3D>
                    {
                        new (leftDepth,tk_Height,0),
                        new (leftDepth,height,0),
                        new (0,height,0),
                        new (0,0,0),
                        new (leftDepth-tk_Depth,0,0),
                        new (leftDepth-tk_Depth,tk_Height,0)
                    };

                    rightEndPanelPoints = new List<Point3D>
                    {
                        new (rightDepth,tk_Height,0),
                        new (rightDepth,height,0),
                        new (0,height,0),
                        new (0,0,0),
                        new (rightDepth-tk_Depth,0,0),
                        new (rightDepth-tk_Depth,tk_Height,0)
                    };

                }
                else
                {
                    tk_Height = 0;
                    tk_Depth = 0;

                    leftEndPanelPoints = new List<Point3D>
                    {
                        new (leftDepth,0,0),
                        new (leftDepth,height,0),
                        new (0,height,0),
                        new (0,0,0)
                    };

                    rightEndPanelPoints = new List<Point3D>
                    {
                        new (rightDepth,0,0),
                        new (rightDepth,height,0),
                        new (0,height,0),
                        new (0,0,0)
                    };
                }
                leftEnd = CreatePanel(leftEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", cab, topDeck90, isPanel);
                rightEnd = CreatePanel(rightEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", cab, topDeck90, isPanel);

                ApplyTransform(leftEnd, 0, 0, 0, 0, 270, 0);
                ApplyTransform(rightEnd, -(rightDepth - MaterialThickness34) - leftFrontWidth, 0, -leftDepth - rightFrontWidth, 0, 180, 0);

                // Deck & top
                deckPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (leftFrontWidth-MaterialThickness34,0,0),
                    new (leftFrontWidth-MaterialThickness34, rightFrontWidth-MaterialThickness34,0),
                    new ((leftFrontWidth - MaterialThickness34) + rightDepth,rightFrontWidth - MaterialThickness34,0),
                    new ((leftFrontWidth - MaterialThickness34) + rightDepth,-leftDepth,0),
                    new (0,-leftDepth,0),

                };
                deck = CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab, true, isPanel);
                top = CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab, true, isPanel);

                ApplyTransform(deck, 0, leftDepth, -height, 90, 0, 0);
                ApplyTransform(top, 0, leftDepth, -tk_Height - MaterialThickness34, 90, 0, 0);

                // Backs

                // Left Back
                backPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (leftFrontWidth+rightDepth - MaterialThickness34,0,0),
                    new (leftFrontWidth+rightDepth - MaterialThickness34,height-tk_Height-doubleMaterialThickness34,0),
                    new (0,height-tk_Height-doubleMaterialThickness34,0),
                };
                leftBack = CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", cab, topDeck90, isPanel);
                ApplyTransform(leftBack, 0, tk_Height + MaterialThickness34, 0, 0, 0, 0);

                // Right Back
                backPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (leftDepth+rightFrontWidth - MaterialThickness34,0,0),
                    new (leftDepth+rightFrontWidth - MaterialThickness34,height-tk_Height-doubleMaterialThickness34,0),
                    new (0,height-tk_Height-doubleMaterialThickness34,0),
                };
                rightBack = CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", cab, topDeck90, isPanel);
                ApplyTransform(rightBack, -leftDepth - rightFrontWidth + MaterialThickness34, tk_Height + MaterialThickness34, leftFrontWidth + rightDepth - doubleMaterialThickness34, 0, 90, 0);

                // Shelves
                if (shelfCount > 0)
                {
                    double shelfSpacing = (height - tk_Height - doubleMaterialThickness34) / (shelfCount + 1);
                    for (int i = 1; i < shelfCount + 1; i++)
                    {
                        shelfPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (leftFrontWidth-MaterialThickness34-.125,0,0),
                            new (leftFrontWidth-MaterialThickness34-.125, rightFrontWidth-MaterialThickness34-.125,0),
                            new ((leftFrontWidth - MaterialThickness34-.125) + rightDepth,rightFrontWidth - MaterialThickness34-.125,0),
                            new ((leftFrontWidth - MaterialThickness34-.125) + rightDepth,-leftDepth-.125,0),
                            new (0,-leftDepth,0),
                        };
                        shelf = CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", cab, true, isPanel);
                        ApplyTransform(shelf, 0 + .0625, leftDepth, -i * shelfSpacing - tk_Height, 90, 0, 0);
                        cabinet.Children.Add(shelf);
                    }
                }

                cabinet.Children.Add(leftEnd);
                cabinet.Children.Add(rightEnd);
                cabinet.Children.Add(deck);
                cabinet.Children.Add(top);
                cabinet.Children.Add(leftBack);
                cabinet.Children.Add(rightBack);
                ApplyTransform(cabinet, 0, 0, 0, 0, 45, 0);
            }

            // 45 degree corner cabinets - style 4
            if (cabType == style4)
            {
                // End Panels
                if (baseCab.HasTK)
                {
                    leftEndPanelPoints = new List<Point3D>
                    {
                        new (leftDepth,tk_Height,0),
                        new (leftDepth,height,0),
                        new (0,height,0),
                        new (0,0,0),
                        new (leftDepth-tk_Depth,0,0),
                        new (leftDepth-tk_Depth,tk_Height,0)
                    };

                    rightEndPanelPoints = new List<Point3D>
                    {
                        new (rightDepth,tk_Height,0),
                        new (rightDepth,height,0),
                        new (0,height,0),
                        new (0,0,0),
                        new (rightDepth-tk_Depth,0,0),
                        new (rightDepth-tk_Depth,tk_Height,0)
                    };

                }
                else
                {
                    tk_Height = 0;
                    tk_Depth = 0;

                    leftEndPanelPoints = new List<Point3D>
                    {
                        new (leftDepth,0,0),
                        new (leftDepth,height,0),
                        new (0,height,0),
                        new (0,0,0)
                    };

                    rightEndPanelPoints = new List<Point3D>
                    {
                        new (rightDepth,0,0),
                        new (rightDepth,height,0),
                        new (0,height,0),
                        new (0,0,0)
                    };
                }
                leftEnd = CreatePanel(leftEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", cab, topDeck90, isPanel);
                rightEnd = CreatePanel(rightEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", cab, topDeck90, isPanel);

                ApplyTransform(leftEnd, 0, 0, 0, 0, 270, 0);
                ApplyTransform(rightEnd, -(rightDepth - MaterialThickness34) - leftFrontWidth, 0, -leftDepth - rightFrontWidth, 0, 180, 0);

                // Deck & top
                deckPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (leftFrontWidth-MaterialThickness34,0,0),
                    new (leftFrontWidth-MaterialThickness34, rightFrontWidth-MaterialThickness34,0),
                    new ((leftFrontWidth - MaterialThickness34) + rightDepth,rightFrontWidth - MaterialThickness34,0),
                    new ((leftFrontWidth - MaterialThickness34) + rightDepth,-leftDepth,0),
                    new (0,-leftDepth,0),

                };
                deck = CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab, true, isPanel);
                top = CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab, true, isPanel);

                ApplyTransform(deck, 0, leftDepth, -height, 90, 0, 0);
                ApplyTransform(top, 0, leftDepth, -tk_Height - MaterialThickness34, 90, 0, 0);

                // Backs

                // Left Back
                backPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (leftFrontWidth+rightDepth - MaterialThickness34,0,0),
                    new (leftFrontWidth+rightDepth - MaterialThickness34,height-tk_Height-doubleMaterialThickness34,0),
                    new (0,height-tk_Height-doubleMaterialThickness34,0),
                };
                leftBack = CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", cab, topDeck90, isPanel);
                ApplyTransform(leftBack, 0, tk_Height + MaterialThickness34, 0, 0, 0, 0);

                // Right Back
                backPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (leftDepth+rightFrontWidth - MaterialThickness34,0,0),
                    new (leftDepth+rightFrontWidth - MaterialThickness34,height-tk_Height-doubleMaterialThickness34,0),
                    new (0,height-tk_Height-doubleMaterialThickness34,0),
                };
                rightBack = CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", cab, topDeck90, isPanel);
                ApplyTransform(rightBack, -leftDepth - rightFrontWidth + MaterialThickness34, tk_Height + MaterialThickness34, leftFrontWidth + rightDepth - doubleMaterialThickness34, 0, 90, 0);

                // Shelves
                if (shelfCount > 0)
                {
                    double shelfSpacing = (height - tk_Height - doubleMaterialThickness34) / (shelfCount + 1);
                    for (int i = 1; i < shelfCount + 1; i++)
                    {
                        shelfPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (leftFrontWidth-MaterialThickness34-.125,0,0),
                            new (leftFrontWidth-MaterialThickness34-.125, rightFrontWidth-MaterialThickness34-.125,0),
                            new ((leftFrontWidth - MaterialThickness34-.125) + rightDepth,rightFrontWidth - MaterialThickness34-.125,0),
                            new ((leftFrontWidth - MaterialThickness34-.125) + rightDepth,-leftDepth-.125,0),
                            new (0,-leftDepth,0),
                        };
                        shelf = CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", cab, true, isPanel);
                        ApplyTransform(shelf, 0 + .0625, leftDepth, -i * shelfSpacing - tk_Height, 90, 0, 0);
                        cabinet.Children.Add(shelf);
                    }
                }

                cabinet.Children.Add(leftEnd);
                cabinet.Children.Add(rightEnd);
                cabinet.Children.Add(deck);
                cabinet.Children.Add(top);
                cabinet.Children.Add(leftBack);
                cabinet.Children.Add(rightBack);
            }
        }

        if (cab is UpperCabinetModel upperCab)
        {
            string? cabType = upperCab.Style;
            string style1 = UpperCabinetViewModel.Style1;
            string style2 = UpperCabinetViewModel.Style2;
            string style3 = UpperCabinetViewModel.Style3;
            double width = ConvertDimension.FractionToDouble(upperCab.Width);
            double height = ConvertDimension.FractionToDouble(upperCab.Height);
            double depth = ConvertDimension.FractionToDouble(upperCab.Depth);
            double backThickness = ConvertDimension.FractionToDouble(upperCab.BackThickness);
            if (backThickness == 0.25) { depth -= backThickness; }
            double leftFrontWidth = ConvertDimension.FractionToDouble(upperCab.LeftFrontWidth);
            double rightFrontWidth = ConvertDimension.FractionToDouble(upperCab.RightFrontWidth);
            double leftDepth = ConvertDimension.FractionToDouble(upperCab.LeftDepth);
            double rightDepth = ConvertDimension.FractionToDouble(upperCab.RightDepth);
            double leftBackWidth = ConvertDimension.FractionToDouble(upperCab.LeftBackWidth);
            double rightBackWidth = ConvertDimension.FractionToDouble(upperCab.RightBackWidth);
            double interiorWidth = width - (MaterialThickness34 * 2);
            double interiorDepth = depth - backThickness;
            double interiorHeight = height - doubleMaterialThickness34;
            double shelfDepth = interiorDepth;
            shelfDepth -= 0.125;
            double upperDoorGap = ConvertDimension.FractionToDouble(upperCab.GapWidth);
            double doorLeftReveal = ConvertDimension.FractionToDouble(upperCab.LeftReveal);
            double doorRightReveal = ConvertDimension.FractionToDouble(upperCab.RightReveal);
            double doorTopReveal = ConvertDimension.FractionToDouble(upperCab.TopReveal);
            double doorBottomReveal = ConvertDimension.FractionToDouble(upperCab.BottomReveal);
            double doorSideReveal = (doorLeftReveal + doorRightReveal) / 2; // this averages the potentially different left and right reveals so that the door creation calc can use just one variable instead of two.
            bool topDeck90 = false; // This is sent to the panel creator to let it know if it is a top or deck at 90 degrees so it cab have 2 edgebanded edges
            bool isPanel = false; // This is sent to the panel creator to let it know if it is a panel (true) or a shelf/deck/top/toekick (false) so it can have edgebanding applied correctly.

            endPanelPoints = new List<Point3D>
            {
                new (depth,0,0),
                new (depth,height,0),
                new (0,height,0),
                new (0,0,0)
            };

            leftEnd = CreatePanel(endPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", cab, topDeck90, isPanel);
            rightEnd = CreatePanel(endPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", cab, topDeck90, isPanel);

            if (cabType.Contains(style1))
            {
                ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
                ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);
            }


            if (cabType.Contains(style1))
            {
                // Deck
                deckPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,depth,0),
                    new (0,depth,0)
                };
                deck = CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", cab, topDeck90, isPanel);
                ApplyTransform(deck, -(interiorWidth / 2), -depth, 0, 270, 0, 0);

                // Full Top
                topPoints = new List<Point3D>
                {
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,depth,0),
                    new (0,depth,0)
                };
                top = CreatePanel(topPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", cab, topDeck90, isPanel);
                ApplyTransform(top, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);


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
                    back = CreatePanel(backPoints, MaterialThickness34, upperCab.Species, "None", "Vertical", cab, topDeck90, isPanel);
                    ApplyTransform(back, -(interiorWidth / 2), MaterialThickness34, 0, 0, 0, 0);
                }
                else
                {
                    backPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (width,0,0),
                        new (width,height,0),
                        new (0,height,0)
                    };
                    back = CreatePanel(backPoints, MaterialThickness14, upperCab.Species, "None", "Vertical", cab, topDeck90, isPanel);
                    ApplyTransform(back, -(width / 2), 0, -MaterialThickness14, 0, 0, 0);
                }


                // Shelves
                double shelfSpacing = interiorHeight + MaterialThickness34; // This should be the space between the shelves
                shelfSpacing /= (upperCab.ShelfCount + 1);


                for (int i = 1; i < upperCab.ShelfCount + 1; i++)
                {
                    shelfPoints = new List<Point3D>
                    {
                        new (0,0,0),
                        new (interiorWidth-.125,0,0),
                        new (interiorWidth-.125,shelfDepth,0),
                        new (0,shelfDepth,0)
                    };
                    shelf = CreatePanel(shelfPoints, MaterialThickness34, upperCab.Species, "None", "Horizontal", cab, topDeck90, isPanel);
                    ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -MaterialThickness34 - shelfDepth, i * shelfSpacing, 270, 0, 0);
                    cabinet.Children.Add(shelf);
                }


                // Doors
                if (upperCab.DoorCount > 0 && upperCab.IncDoors)
                {
                    double doorWidth = width - (doorSideReveal * 2);
                    double doorHeight = height - doorTopReveal - doorBottomReveal;

                    if (upperCab.DoorCount == 1)
                    {
                        doorPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (doorWidth,0,0),
                            new (doorWidth,doorHeight,0),
                            new (0,doorHeight,0)
                        };
                        door1 = CreatePanel(doorPoints, MaterialThickness34, upperCab.DoorSpecies, "None", upperCab.DoorGrainDir, cab, topDeck90, isPanel);
                        ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);

                        cabinet.Children.Add(door1);
                    }

                    if (upperCab.DoorCount == 2)
                    {
                        doorWidth = (doorWidth / 2) - (upperDoorGap / 2);

                        doorPoints = new List<Point3D>
                        {
                            new (0,0,0),
                            new (doorWidth,0,0),
                            new (doorWidth, doorHeight, 0),
                            new (0,doorHeight,0)
                        };
                        door1 = CreatePanel(doorPoints, MaterialThickness34, upperCab.DoorSpecies, "None", upperCab.DoorGrainDir, cab, topDeck90, isPanel);
                        door2 = CreatePanel(doorPoints, MaterialThickness34, upperCab.DoorSpecies, "None", upperCab.DoorGrainDir, cab, topDeck90, isPanel);
                        ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                        ApplyTransform(door2, (width / 2) - doorWidth - doorRightReveal, doorBottomReveal, depth, 0, 0, 0);

                        cabinet.Children.Add(door1);
                        cabinet.Children.Add(door2);
                    }

                }

                cabinet.Children.Add(leftEnd);
                cabinet.Children.Add(rightEnd);
                cabinet.Children.Add(deck);
                cabinet.Children.Add(top);
                cabinet.Children.Add(back);

            }
        }

        if (cab is FillerModel filler)
        {
            double width = ConvertDimension.FractionToDouble(filler.Width);
            double height = ConvertDimension.FractionToDouble(filler.Height);
            double depth = ConvertDimension.FractionToDouble(filler.Depth);
            bool topDeck90 = false; // This is sent to the panel creator to let it know if it is a top or deck at 90 degrees so it cab have 2 edgebanded edges
            bool isPanel = false; // This is sent to the panel creator to let it know if it is a panel (true) or a shelf/deck/top/toekick (false) so it can have edgebanding applied correctly.

            endPanelPoints = new List<Point3D>
            {
                new (depth,0,0),
                new (depth,height,0),
                new (0,height,0),
                new (0,0,0)
            };

            leftEnd = CreatePanel(endPanelPoints, MaterialThickness34, filler.Species, "None", "Vertical", cab, topDeck90, isPanel);
            ApplyTransform(leftEnd, 0, 0, -MaterialThickness34, 0, 270, 0);

            backPoints = new List<Point3D>
            {
                new (0,0,0),
                new (width,0,0),
                new (width,height,0),
                new (0,height,0)
            };

            back = CreatePanel(backPoints, MaterialThickness34, filler.Species, "None", "Vertical", cab, topDeck90, isPanel);
            ApplyTransform(back, 0, 0, depth, 0, 0, 0);

            cabinet.Children.Add(leftEnd);
            cabinet.Children.Add(back);
        }

        if (cab is PanelModel panel)
        {
            double width = ConvertDimension.FractionToDouble(panel.Width);
            double height = ConvertDimension.FractionToDouble(panel.Height);
            double depth = ConvertDimension.FractionToDouble(panel.Depth);
            bool topDeck90 = false; // This is sent to the panel creator to let it know if it is a top or deck at 90 degrees so it cab have 2 edgebanded edges
            bool isPanel = true; // This is sent to the panel creater to let it know it is just a panel so it can edgeband all 4 edges

            backPoints = new List<Point3D>
            {
                new (0,0,0),
                new (width,0,0),
                new (width,height,0),
                new (0,height,0)
            };

            back = CreatePanel(backPoints, depth, panel.Species, panel.EBSpecies, "Vertical", cab, topDeck90, isPanel);
            ApplyTransform(back, 0, 0, depth / 2, 0, 0, 0);

            cabinet.Children.Add(back);
        }

        return cabinet;
    }



    // This lovely bit of kit will create a panel of any size, shape, thickness, material species, etc. and edgeband it
    private static Model3DGroup CreatePanel(List<Point3D> polygonPoints, double matlThickness, string panelSpecies, string edgebandingSpecies, string grainDirection, CabinetModel cab, bool topDeck90, bool isPanel)
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
            if (!isPanel && !topDeck90)
            {
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
            }

            if (isPanel)
            {
                specialBuilder.AddQuad(
                mainBuilder.Positions[b0],  // Note: positions are shared or duplicate if needed; but since separate meshes, add to special
                mainBuilder.Positions[b1],
                mainBuilder.Positions[t1],
                mainBuilder.Positions[t0],
                uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
            }

            if (topDeck90)
            {
                if (edgeFace == 0 || edgeFace == 1) // Edge(s) to show edgeband texture
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

        string resourcePath = $"pack://application:,,,/Images/Plywood/{panelSpecies} - {grainDirection}.png";

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

            return new DiffuseMaterial(brush);
        }
        catch (Exception ex)
        {
            // Fallback to solid color
            return new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(200, 200, 200)));
        }
    }

    private static Material GetEdgeBandingSpecies(string? species)
    {
        // Handle "None" or null
        if (string.IsNullOrWhiteSpace(species) || species == "None")
        {
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
            // Fallback to solid color
            return new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(139, 69, 19)));
        }
    }

}

