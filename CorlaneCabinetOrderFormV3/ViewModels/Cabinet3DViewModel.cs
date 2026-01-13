using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using HelixToolkit.Wpf;
using System.Diagnostics;
using System.Globalization;
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

    //private readonly MainWindowViewModel? _mainVm;
    private readonly IPreviewService? _previewSvc;

    // Constructor used by DI - MainWindowViewModel and IPreviewService injected
    public Cabinet3DViewModel(IPreviewService previewSvc)
    {
        //_mainVm = mainVm;
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

    // Visibility Properties
    [ObservableProperty] public partial bool LeftEndHidden { get; set; } = false; partial void OnLeftEndHiddenChanged(bool value)
    {
        if (value) HideLeftEndText = "Show Left End";
        else HideLeftEndText = "Hide Left End";

        RebuildPreview();
    }
    [ObservableProperty] public partial bool RightEndHidden { get; set; } = false; partial void OnRightEndHiddenChanged(bool value)
    {
        if (value) HideRightEndText = "Show Right End";
        else HideRightEndText = "Hide Right End";

        RebuildPreview();
    }
    [ObservableProperty] public partial bool DeckHidden { get; set; } = false; partial void OnDeckHiddenChanged(bool value)
    {
        if (value) HideDeckText = "Show Deck";
        else HideDeckText = "Hide Deck";

        RebuildPreview();
    }
    [ObservableProperty] public partial bool TopHidden { get; set; } = false; partial void OnTopHiddenChanged(bool value)
    {
        if (value) HideTopText = "Show Top";
        else HideTopText = "Hide Top";

        RebuildPreview();
    }
    [ObservableProperty] public partial string HideTopText { get; set; } = "Hide Top";
    [ObservableProperty] public partial string HideDeckText { get; set; } = "Hide Deck";
    [ObservableProperty] public partial string HideLeftEndText { get; set; } = "Hide Left End";
    [ObservableProperty] public partial string HideRightEndText { get; set; } = "Hide Right End";



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
        var cab = _previewSvc!.CurrentPreviewCabinet;

        if (cab is CabinetModel cabinetModel)
        {
            // Reset accumulators so rebuild produces fresh material/edge totals
            cabinetModel.ResetAllMaterialAndEdgeTotals();

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


    // Public helper: force the 3D builder to (re)compute material / edgeband accumulators for a CabinetModel instance.
    // This does NOT change the preview; it simply runs the same BuildCabinet pipeline that populates the
    // CabinetModel.MaterialAreaBySpecies and CabinetModel.EdgeBandingLengthBySpecies dictionaries.
    public void AccumulateMaterialAndEdgeTotals(CabinetModel? cab)
    {
        if (cab == null) return;

        // Clear previous totals
        cab.ResetAllMaterialAndEdgeTotals();

        // BuildCabinet will call CreatePanel(...) which accumulates into the provided cab.
        // Run on the UI thread because some HelixToolkit operations expect a Dispatcher (and to be safe).
        if (Application.Current?.Dispatcher == null)
        {
            try { _ = BuildCabinet(cab); } catch { /* best-effort */ }
        }
        else
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try { _ = BuildCabinet(cab); } catch { /* swallow */ }
                }, DispatcherPriority.Render);
            }
            catch
            {
                // swallow - best-effort
            }
        }
    }


    private Model3DGroup BuildCabinet(CabinetModel cab)
    {
        var cabinet = new Model3DGroup();

        // Dispatch to small focused builders to keep this method readable.
        if (cab is BaseCabinetModel baseCab)
        {
            BuildBase(cabinet, baseCab);
        }
        else if (cab is UpperCabinetModel upperCab)
        {
            BuildUpper(cabinet, upperCab);
        }
        else if (cab is FillerModel filler)
        {
            BuildFiller(cabinet, filler);
        }
        else if (cab is PanelModel panel)
        {
            BuildPanel(cabinet, panel);
        }

        return cabinet;
    }

    private void BuildBase(Model3DGroup cabinet, BaseCabinetModel baseCab)
    {
        Model3DGroup leftEnd;
        Model3DGroup rightEnd;
        Model3DGroup deck;
        Model3DGroup top = new();
        Model3DGroup topStretcherFront;
        Model3DGroup topStretcherBack;
        Model3DGroup stretcher;
        Model3DGroup shelf;
        Model3DGroup toekick = new();
        Model3DGroup toekick1;
        Model3DGroup toekick2;
        Model3DGroup back;
        Model3DGroup leftBack;
        Model3DGroup rightBack;
        Model3DGroup door1;
        Model3DGroup door2;
        Model3DGroup drwFront1;
        Model3DGroup drwFront2;
        Model3DGroup drwFront3;
        Model3DGroup drwFront4;
        Model3DGroup dbxLeftSide;
        Model3DGroup dbxRightSide;
        Model3DGroup dbxFront;
        Model3DGroup dbxBack;
        Model3DGroup dbxBottom;
        Model3DGroup nailer;



        List<Point3D> endPanelPoints;
        List<Point3D> leftEndPanelPoints;
        List<Point3D> rightEndPanelPoints;
        List<Point3D> deckPoints;
        List<Point3D> topPoints;
        List<Point3D> topStretcherFrontPoints;
        List<Point3D> topStretcherBackPoints;
        List<Point3D> toekickPoints;
        List<Point3D> backPoints;
        List<Point3D> stretcherPoints;
        List<Point3D> sinkStretcherPoints;
        List<Point3D> shelfPoints;
        List<Point3D> doorPoints;
        List<Point3D> drwFrontPoints;
        List<Point3D> dbxSidePoints;
        List<Point3D> dbxFrontAndBackPoints;
        List<Point3D> dbxBottomPoints;
        List<Point3D> nailerPoints;



        double MaterialThickness34 = 0.75;
        double MaterialThickness14 = 0.25;
        double halfMaterialThickness34 = MaterialThickness34 / 2; // This is to make door calcs etc. more straightforward
        double doubleMaterialThickness34 = MaterialThickness34 * 2; // This is to make door calcs etc. more straightforward

        double backLegWidth = 3;
        double StretcherWidth = 6;
        double topStretcherBackWidth = 3;

        string? cabType = baseCab.Style;
        string style1 = BaseCabinetViewModel.Style1;
        string style2 = BaseCabinetViewModel.Style2;
        string style3 = BaseCabinetViewModel.Style3;
        string style4 = BaseCabinetViewModel.Style4;

        string doorEdgebandingSpecies;

        doorEdgebandingSpecies = baseCab.DoorSpecies switch
        {
            null => "None",
            string s when s.Contains("Alder", StringComparison.OrdinalIgnoreCase) => "Wood Alder",
            string s when s.Contains("Cherry", StringComparison.OrdinalIgnoreCase) => "Wood Cherry",
            string s when s.Contains("Hickory", StringComparison.OrdinalIgnoreCase) => "Wood Hickory",
            string s when s.Contains("Mahogany", StringComparison.OrdinalIgnoreCase) => "Wood Mahogany",
            string s when s.Contains("Maple", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
            string s when s.Contains("MDF", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
            string s when s.Contains("Melamine", StringComparison.OrdinalIgnoreCase) => "PVC Custom",
            string s when s.Contains("Prefinished Ply", StringComparison.OrdinalIgnoreCase) => "Wood Prefinished Maple",
            string s when s.Contains("Red Oak", StringComparison.OrdinalIgnoreCase) => "Wood Red Oak",
            string s when s.Contains("Walnut", StringComparison.OrdinalIgnoreCase) => "Wood Walnut",
            string s when s.Contains("White Oak", StringComparison.OrdinalIgnoreCase) => "Wood Oak",
            _ => "None"
        };

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
        //shelfDepth -= 0.125;
        double baseDoorGap = ConvertDimension.FractionToDouble(baseCab.GapWidth);
        double doorLeftReveal = ConvertDimension.FractionToDouble(baseCab.LeftReveal);
        double doorRightReveal = ConvertDimension.FractionToDouble(baseCab.RightReveal);
        double doorTopReveal = ConvertDimension.FractionToDouble(baseCab.TopReveal);
        double doorBottomReveal = ConvertDimension.FractionToDouble(baseCab.BottomReveal);
        double doorSideReveal = (doorLeftReveal + doorRightReveal) / 2; // this averages the potentially different left and right reveals so that the door creation calc can use just one variable instead of two.
        bool topDeck90 = false; // This is sent to the panel creator to let it know if it is a top or deck at 90 degrees so it cab have 2 edgebanded edges
        bool isPanel = false; // This is sent to the panel creator to let it know if it is a panel (true) so it can have edgebanding applied correctly. Also using it for doors and drawer fronts so they are banded on all 4 edges.
        int shelfCount = baseCab.ShelfCount;
        double dbxWidth = interiorWidth;
        double dbxHeight;
        double dbxDepth = interiorDepth - 1;
        if (depth >= 10.625 && depth < 13.625) dbxDepth = 9;
        if (depth >= 13.625 && depth < 16.625) dbxDepth = 12;
        if (depth >= 16.625 && depth < 19.625) dbxDepth = 15;
        if (depth >= 19.625 && depth < 22.625) dbxDepth = 18;
        if (depth >= 22.625) dbxDepth = 21;
        double tandemSideSpacing = .4; // TODO confirm and set to proper value
        double tandemTopSpacing = .375;
        double tandemBottomSpacing = .5906; // This is for top & bottom drws. Middle drws will have .375 additional (.9656) bottom spacing 
        double tandemMidDrwBottomSpacingAdjustment = .375; // see above
        double accurideSideSpacing = 1; // TODO confirm and set to proper value
        double accurideTopSpacing = .5; // TODO confirm and set to proper value
        double accurideBottomSpacing = .5; // TODO confirm and set to proper value
        double rolloutHeight = 4;

        string panelEBEdges = "";

        stretcherPoints =
        [
            new (0,0,0),
            new (interiorWidth,0,0),
            new (interiorWidth,StretcherWidth,0),
            new (0,StretcherWidth,0)
        ];

        shelfPoints =
        [
            new (0,0,0),
            new (interiorWidth-.125,0,0),
            new (interiorWidth-.125,shelfDepth,0),
            new (0,shelfDepth,0)
        ];


        if (baseCab.HasTK)
        {
            endPanelPoints =
            [
                new (depth,tk_Height,0),
                new (depth,height,0),
                new (0,height,0),
                new (0,0,0),
                new (3,0,0),
                new (3,.5,0),
                new (depth-tk_Depth-3,.5,0),
                new (depth-tk_Depth-3,0,0),
                new (depth-tk_Depth,0,0),
                new (depth-tk_Depth,tk_Height,0)
            ];
        }
        else
        {
            tk_Height = 0;
            tk_Depth = 0;

            endPanelPoints =
            [
                new (depth,0,0),
                new (depth,height,0),
                new (0,height,0),
                new (0,0,0)
            ];
        }

        interiorHeight = height - doubleMaterialThickness34 - tk_Height;

        //Debug.WriteLine($"End Panels:");

        leftEnd = CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
        rightEnd = CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

        if (cabType == style1 || cabType == style2)
        {
            ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
            ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);
        }

        if (cabType == style1 || cabType == style2)
        {
            //Debug.WriteLine("Deck:");

            // Deck
            deckPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,depth,0),
                new (0,depth,0)
            ];
            deck = CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
            ApplyTransform(deck, -(interiorWidth / 2), -depth, tk_Height, 270, 0, 0);

            // Full Top
            if (baseCab.TopType == "Full")
            {
                //Debug.WriteLine("Full Top");

                topPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,depth,0),
                    new (0,depth,0)
                ];
                top = CreatePanel(topPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(top, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
            }

            else
            {
                //Debug.WriteLine("Stretcher Top");

                topStretcherFrontPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,StretcherWidth,0),
                    new (0,StretcherWidth,0)
                ];

                topStretcherBackPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,topStretcherBackWidth,0),
                    new (0,topStretcherBackWidth,0)
                ];


                topStretcherFront = CreatePanel(topStretcherFrontPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                topStretcherBack = CreatePanel(topStretcherBackPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);

                ApplyTransform(topStretcherFront, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
                ApplyTransform(topStretcherBack, -(interiorWidth / 2), -topStretcherBackWidth, height - MaterialThickness34, 270, 0, 0);
                top.Children.Add(topStretcherFront);
                top.Children.Add(topStretcherBack);
            }

            // Toekick
            if (baseCab.HasTK)
            {
                //Debug.WriteLine("Toekick");

                toekickPoints =
                    [
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,tk_Height-.5,0),
                        new (0,tk_Height-.5,0)
                    ];
                toekick = CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(toekick, -(interiorWidth / 2), 0.5, depth - tk_Depth - MaterialThickness34, 0, 0, 0); // The hardcoded 1/2" here is because the actual toekick board is 1/2" narrower than the specified toekick height
            }

            // Back
            if (backThickness == 0.75)
            {
                //Debug.WriteLine("Back");

                backPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,interiorHeight,0),
                    new (0,interiorHeight,0)
                ];
                back = CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(back, -(interiorWidth / 2), MaterialThickness34 + tk_Height, 0, 0, 0, 0);
            }
            else
            {
                //Debug.WriteLine("Back");

                backPoints =
                [
                    new (0,0,0),
                    new (width,0,0),
                    new (width,height-tk_Height,0),
                    new (0,height-tk_Height,0)
                ];
                back = CreatePanel(backPoints, MaterialThickness14, "PFP 1/4", "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(back, -(width / 2), tk_Height, -MaterialThickness14, 0, 0, 0);

                nailerPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,StretcherWidth,0),
                    new (0,StretcherWidth,0)
                ];

                nailer = CreatePanel(nailerPoints, MaterialThickness34, baseCab.Species, "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(nailer, -(interiorWidth / 2), height - StretcherWidth - MaterialThickness34, 0, 0, 0, 0);
                cabinet.Children.Add(nailer);
            }

            // Drawer Stretchers
            if (cabType == style1 && baseCab.DrwCount == 1)
            {
                //Debug.WriteLine("Drw Stretcher");

                double topDeckAndStretcherThickness = (baseCab.DrwCount + 1) * MaterialThickness34;

                stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - topDeckAndStretcherThickness - opening1Height, 270, 0, 0);
                cabinet.Children.Add(stretcher);

                if (baseCab.SinkCabinet)
                {
                    //Debug.WriteLine("Sink Stretcher");

                    sinkStretcherPoints =
                    [
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,opening1Height,0),
                        new (0,opening1Height,0)
                    ];

                    stretcher = CreatePanel(sinkStretcherPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ApplyTransform(stretcher, -(interiorWidth / 2), -height + MaterialThickness34, -depth, 180, 0, 0);
                    cabinet.Children.Add(stretcher);
                }
            }

            if (cabType == style2)
            {
                double opening1HeightAdjusted = opening1Height;
                double opening2HeightAdjusted = opening2Height;
                double opening3HeightAdjusted = opening3Height;

                if (baseCab.DrwCount == 2)
                {
                    //Debug.WriteLine("Drw Stretcher");

                    opening1HeightAdjusted += doubleMaterialThickness34; // moves the reference to the bottom of the stretcher
                    stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted, 270, 0, 0);
                    cabinet.Children.Add(stretcher);
                }

                if (baseCab.DrwCount == 3)
                {
                    //Debug.WriteLine("Drw Stretcher");

                    opening1HeightAdjusted += doubleMaterialThickness34; // moves the reference to the bottom of the stretcher
                    stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted, 270, 0, 0);
                    cabinet.Children.Add(stretcher);

                    //Debug.WriteLine("Drw Stretcher");

                    opening2HeightAdjusted += MaterialThickness34; // moves the reference to the bottom of the stretcher
                    stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted - opening2HeightAdjusted, 270, 0, 0);
                    cabinet.Children.Add(stretcher);
                }

                if (baseCab.DrwCount == 4)
                {
                    //Debug.WriteLine("Drw Stretcher");

                    opening1HeightAdjusted += doubleMaterialThickness34; // moves the reference to the bottom of the stretcher
                    stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted, 270, 0, 0);
                    cabinet.Children.Add(stretcher);

                    //Debug.WriteLine("Drw Stretcher");

                    opening2HeightAdjusted += MaterialThickness34; // moves the reference to the bottom of the stretcher
                    stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted - opening2HeightAdjusted, 270, 0, 0);
                    cabinet.Children.Add(stretcher);

                    //Debug.WriteLine("Drw Stretcher");

                    opening3HeightAdjusted += MaterialThickness34; // moves the reference to the bottom of the stretcher
                    stretcher = CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted - opening2HeightAdjusted - opening3HeightAdjusted, 270, 0, 0);
                    cabinet.Children.Add(stretcher);
                }

                if (baseCab.SinkCabinet)
                {
                    //Debug.WriteLine("Sink Stretcher");

                    sinkStretcherPoints =
                    [
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,opening1Height,0),
                        new (0,opening1Height,0)
                    ];

                    stretcher = CreatePanel(sinkStretcherPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ApplyTransform(stretcher, -(interiorWidth / 2), -height + MaterialThickness34, -depth, 180, 0, 0);
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
                    //Debug.WriteLine("Shelf");

                    shelf = CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -backThickness - shelfDepth, i * shelfSpacing, 270, 0, 0);
                    cabinet.Children.Add(shelf);
                }
            }


            // Doors
            if (baseCab.DoorCount > 0 && baseCab.IncDoors && cabType != style2 || baseCab.DoorCount > 0 && baseCab.IncDoorsInList && cabType != style2)
            {
                double doorWidth = width - (doorSideReveal * 2);
                double doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height;

                if (cabType == style1 && baseCab.DrwCount == 1)
                {
                    doorHeight = height - opening1Height - MaterialThickness34 - halfMaterialThickness34 - (baseDoorGap / 2) - doorBottomReveal - tk_Height;
                }

                if (baseCab.DoorCount == 1)
                {
                    doorPoints =
                    [
                        new (0,0,0),
                        new (doorWidth,0,0),
                        new (doorWidth,doorHeight,0),
                        new (0,doorHeight,0)
                    ];

                    //Debug.WriteLine("Door");

                    if (baseCab.IncDoorsInList)
                    {
                        AddFrontPartRow(baseCab, "Door", doorHeight, doorWidth, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                    }

                    if (baseCab.IncDoors)
                    {
                        door1 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
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
                }

                if (baseCab.DoorCount == 2)
                {
                    doorWidth = (doorWidth / 2) - (baseDoorGap / 2);

                    doorPoints =
                    [
                    new (0,0,0),
                    new (doorWidth,0,0),
                    new (doorWidth, doorHeight, 0),
                    new (0,doorHeight,0)
                    ];

                    //Debug.WriteLine("Door");

                    if (baseCab.IncDoorsInList)
                    {
                        AddFrontPartRow(baseCab, "Door", doorHeight, doorWidth, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                        AddFrontPartRow(baseCab, "Door", doorHeight, doorWidth, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                    }

                    if (baseCab.IncDoors)
                    {
                        door1 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);

                        //Debug.WriteLine("Door");

                        door2 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
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

            }


            // Drawer Fronts
            double drwFrontWidth = width - (doorSideReveal * 2);

            if (baseCab.IncDrwFront1 || baseCab.IncDrwFront2 || baseCab.IncDrwFront3 || baseCab.IncDrwFront4 || baseCab.IncDrwFrontInList1 || baseCab.IncDrwFrontInList2 || baseCab.IncDrwFrontInList3 || baseCab.IncDrwFrontInList4)
            {
                if (cabType == style1 && baseCab.DrwCount == 1 && baseCab.IncDrwFront1 || cabType == style1 && baseCab.DrwCount == 1 && baseCab.IncDrwFrontInList1)
                {
                    drwFront1Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight1);

                    drwFrontPoints =
                    [
                        new (0,0,0),
                        new (drwFrontWidth,0,0),
                        new (drwFrontWidth,drwFront1Height,0),
                        new (0,drwFront1Height,0)
                    ];

                    //Debug.WriteLine("Drw Front");
                    if (baseCab.IncDrwFrontInList1)
                    {
                        AddFrontPartRow(baseCab, "Drawer Front 1", drwFront1Height, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
                    }

                    if (baseCab.IncDrwFront1)
                    {
                        drwFront1 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                        cabinet.Children.Add(drwFront1);
                    }
                }

                if (cabType == "Drawer")
                {
                    if (baseCab.DrwCount == 1 && baseCab.IncDrwFront1 || baseCab.DrwCount == 1 && baseCab.IncDrwFrontInList1)
                    {
                        drwFront1Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight1);

                        drwFrontPoints =
                        [
                            new (0,0,0),
                            new (drwFrontWidth,0,0),
                            new (drwFrontWidth,drwFront1Height,0),
                            new (0,drwFront1Height,0)
                        ];
                        //Debug.WriteLine("Drw Front");

                        if (baseCab.IncDrwFrontInList1)
                        {
                            AddFrontPartRow(baseCab, "Drawer Front 1", drwFront1Height, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
                        }

                        if (baseCab.IncDrwFront1)
                        {
                            drwFront1 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                            ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                            cabinet.Children.Add(drwFront1);
                        }
                    }

                    if (baseCab.DrwCount > 1)
                    {
                        // Top Drawer
                        if (baseCab.IncDrwFront1 || baseCab.IncDrwFrontInList1)
                        {
                            drwFront1Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight1);

                            drwFrontPoints =
                            [
                                new (0,0,0),
                                new (drwFrontWidth,0,0),
                                new (drwFrontWidth,drwFront1Height,0),
                                new (0,drwFront1Height,0)
                            ];
                            //Debug.WriteLine("Drw Front");

                            if (baseCab.IncDrwFrontInList1)
                            {
                                AddFrontPartRow(baseCab, "Drawer Front 1", drwFront1Height, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
                            }

                            if (baseCab.IncDrwFront1)
                            {
                                drwFront1 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                                ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                                cabinet.Children.Add(drwFront1);
                            }
                        }

                        // Second Drawer

                        if (baseCab.IncDrwFront2 || baseCab.IncDrwFrontInList2)
                        {
                            if (baseCab.DrwCount == 2) // if true, this is the bottom drawer
                            {
                                drwFront2Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight2);
                            }

                            drwFrontPoints =
                            [
                                new (0,0,0),
                                new (drwFrontWidth,0,0),
                                new (drwFrontWidth,drwFront2Height,0),
                                new (0,drwFront2Height,0)
                            ];
                            //Debug.WriteLine("Drw Front");

                            if (baseCab.IncDrwFrontInList2)
                            {
                                AddFrontPartRow(baseCab, "Drawer Front 2", drwFront2Height, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
                            }

                            if (baseCab.IncDrwFront2)
                            {
                                drwFront2 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                                ApplyTransform(drwFront2,
                                    -(width / 2) + doorLeftReveal,
                                    height - doorTopReveal - drwFront1Height - baseDoorGap - drwFront2Height,
                                    depth,
                                    0, 0, 0);

                                cabinet.Children.Add(drwFront2);
                            }
                        }

                        if (baseCab.DrwCount > 2)
                        {
                            // Third Drawer
                            if (baseCab.IncDrwFront3 || baseCab.IncDrwFrontInList3)
                            {
                                if (baseCab.DrwCount == 3) // if true, this is the bottom drawer
                                {
                                    drwFront3Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight3);
                                }

                                drwFrontPoints =
                                [
                                    new (0,0,0),
                                    new (drwFrontWidth,0,0),
                                    new (drwFrontWidth,drwFront3Height,0),
                                    new (0,drwFront3Height,0)
                                ];
                                //Debug.WriteLine("Drw Front");

                                if(baseCab.IncDrwFrontInList3)
{
                                    AddFrontPartRow(baseCab, "Drawer Front 3", drwFront3Height, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
                                }

                                if (baseCab.IncDrwFront3)
                                {
                                    drwFront3 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                                    ApplyTransform(drwFront3,
                                        -(width / 2) + doorLeftReveal,
                                        height - doorTopReveal - drwFront1Height - baseDoorGap - drwFront2Height - baseDoorGap - drwFront3Height,
                                        depth,
                                        0, 0, 0);

                                    cabinet.Children.Add(drwFront3);
                                }
                            }

                        }

                        // Fourth Drawer
                        if (baseCab.DrwCount > 3)
                        {
                            if (baseCab.IncDrwFront4 || baseCab.IncDrwFrontInList4)
                            {
                                if (baseCab.DrwCount == 4) // if true, this is the bottom drawer
                                {
                                    drwFront4Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight4);
                                }

                                drwFrontPoints =
                                [
                                    new (0,0,0),
                                    new (drwFrontWidth,0,0),
                                    new (drwFrontWidth,drwFront4Height,0),
                                    new (0,drwFront4Height,0)
                                ];
                                //Debug.WriteLine("Drw Front");

                                if (baseCab.IncDrwFrontInList4)
                                {
                                    AddFrontPartRow(baseCab, "Drawer Front 4", drwFront4Height, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
                                }

                                if (baseCab.IncDrwFront4)
                                {
                                    drwFront4 = CreatePanel(drwFrontPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                                    ApplyTransform(drwFront4,
                                        -(width / 2) + doorLeftReveal,
                                        height - doorTopReveal - drwFront1Height - baseDoorGap - drwFront2Height - baseDoorGap - drwFront3Height - baseDoorGap - drwFront4Height,
                                        depth,
                                        0, 0, 0);

                                    cabinet.Children.Add(drwFront4);
                                }
                            }
                        }
                    }
                }
            }

            // Drawer Boxes
            if (baseCab.DrwCount > 0)
            {
                if (baseCab.IncDrwBoxOpening1 || baseCab.IncDrwBoxOpening2 || baseCab.IncDrwBoxOpening3 || baseCab.IncDrwBoxOpening4 || baseCab.IncDrwBoxInListOpening1 || baseCab.IncDrwBoxInListOpening2 || baseCab.IncDrwBoxInListOpening3 || baseCab.IncDrwBoxInListOpening4)
                {
                    //double sideSpacing;
                    double topSpacing = 0;
                    double bottomSpacing = 0;

                    if (baseCab.DrwStyle is not null)
                    {
                        if (baseCab.DrwStyle.Contains("Blum"))
                        {
                            dbxWidth -= tandemSideSpacing;
                            //sideSpacing = tandemSideSpacing;
                            topSpacing = tandemTopSpacing;
                            bottomSpacing = tandemBottomSpacing;
                        }
                        else if (baseCab.DrwStyle.Contains("Accuride"))
                        {
                            dbxWidth -= accurideSideSpacing;
                            //sideSpacing = accurideSideSpacing;
                            topSpacing = accurideTopSpacing;
                            bottomSpacing = accurideBottomSpacing;
                        }
                    }

                    double dbxFrontAndBackWidth = dbxWidth - (MaterialThickness34 * 2);
                    double dbxBottomWidth = dbxWidth - (MaterialThickness34 * 2);
                    double dbxBottomLength = dbxDepth - (MaterialThickness34 * 2);

                    if (baseCab.IncDrwBoxInListOpening1 && baseCab.DrwCount > 0)
                    {
                        dbxHeight = opening1Height - topSpacing - bottomSpacing;
                        AddDrawerBoxRow(baseCab, "Drawer Box 1", dbxHeight, dbxWidth, dbxDepth);
                    }

                    if (baseCab.IncDrwBoxOpening1)
                    {
                        dbxHeight = opening1Height - topSpacing - bottomSpacing;

                        dbxSidePoints =
                        [
                            new (dbxDepth,dbxHeight,0),
                            new (0,dbxHeight,0),
                            new (0,0,0),
                            new (dbxDepth,0,0)
                        ];
                        //Debug.WriteLine("Drw Box Sides");

                        dbxLeftSide = CreatePanel(dbxSidePoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
                        dbxRightSide = CreatePanel(dbxSidePoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

                        dbxFrontAndBackPoints =
                        [
                            new (dbxFrontAndBackWidth,dbxHeight,0),
                            new (0,dbxHeight,0),
                            new (0,0,0),
                            new (dbxFrontAndBackWidth,0,0)
                        ];
                        //Debug.WriteLine("Drw Box Front and Back");

                        dbxFront = CreatePanel(dbxFrontAndBackPoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
                        dbxBack = CreatePanel(dbxFrontAndBackPoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

                        dbxBottomPoints =
                        [
                            new (0,0,0),
                            new (dbxBottomWidth,0,0),
                            new (dbxBottomWidth,dbxBottomLength,0),
                            new (0,dbxBottomLength,0)
                        ];
                        //Debug.WriteLine("Drw Box Bottom");

                        dbxBottom = CreatePanel(dbxBottomPoints, MaterialThickness34, "Prefinished Ply", "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);

                        // Build box:
                        ApplyTransform(dbxLeftSide, 0, 0, -(dbxWidth - MaterialThickness34), 0, 0, 0);
                        ApplyTransform(dbxFront, 0, 0, 0, 0, 90, 0);
                        ApplyTransform(dbxBack, 0, 0, dbxDepth - MaterialThickness34, 0, 90, 0);
                        ApplyTransform(dbxBottom, 0, MaterialThickness34, -MaterialThickness34 - .5, 90, 90, 0);

                        // Rotate Box:
                        Model3DGroup dbx1rotate = new();
                        dbx1rotate.Children.Add(dbxLeftSide);
                        dbx1rotate.Children.Add(dbxRightSide);
                        dbx1rotate.Children.Add(dbxFront);
                        dbx1rotate.Children.Add(dbxBack);
                        dbx1rotate.Children.Add(dbxBottom);
                        ApplyTransform(dbx1rotate, 0, 0, 0, 0, 90, 0);

                        // Position Box in Cabinet:
                        Model3DGroup dbx1 = new();
                        dbx1.Children.Add(dbx1rotate);
                        ApplyTransform(dbx1, (dbxWidth / 2) - MaterialThickness34, height - dbxHeight - MaterialThickness34 - topSpacing, interiorDepth + backThickness, 0, 0, 0);
                        cabinet.Children.Add(dbx1);
                    }

                    //if (baseCab.IncDrwBoxInListOpening2 && baseCab.DrwCount > 1)
                    //{
                    //    //dbxHeight = opening2Height - topSpacing - bottomSpacing;
                    //    AddDrawerBoxRow(baseCab, "Drawer Box 2", dbxHeight, dbxWidth, dbxDepth);
                    //}


                    if (baseCab.IncDrwBoxOpening2 && baseCab.DrwCount > 1)
                    {
                        dbxHeight = opening2Height - topSpacing - bottomSpacing;
                        if (baseCab.DrwCount > 2) // if true this is a middle drawer and needs additional bottom spacing
                        {
                            dbxHeight -= tandemMidDrwBottomSpacingAdjustment;
                        }

                        dbxSidePoints =
                        [
                            new (dbxDepth,dbxHeight,0),
                            new (0,dbxHeight,0),
                            new (0,0,0),
                            new (dbxDepth,0,0)
                        ];
                        //Debug.WriteLine("Drw Box Sides");

                        dbxLeftSide = CreatePanel(dbxSidePoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
                        dbxRightSide = CreatePanel(dbxSidePoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

                        dbxFrontAndBackPoints =
                        [
                            new (dbxFrontAndBackWidth,dbxHeight,0),
                            new (0,dbxHeight,0),
                            new (0,0,0),
                            new (dbxFrontAndBackWidth,0,0)
                        ];
                        dbxFront = CreatePanel(dbxFrontAndBackPoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
                        dbxBack = CreatePanel(dbxFrontAndBackPoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

                        dbxBottomPoints =
                        [
                            new (0,0,0),
                            new (dbxBottomWidth,0,0),
                            new (dbxBottomWidth,dbxBottomLength,0),
                            new (0,dbxBottomLength,0)
                        ];
                        //Debug.WriteLine("Drw Box Bottom");

                        dbxBottom = CreatePanel(dbxBottomPoints, MaterialThickness34, "Prefinished Ply", "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);

                        ApplyTransform(dbxLeftSide, 0, 0, -(dbxWidth - MaterialThickness34), 0, 0, 0);
                        ApplyTransform(dbxFront, 0, 0, 0, 0, 90, 0);
                        ApplyTransform(dbxBack, 0, 0, dbxDepth - MaterialThickness34, 0, 90, 0);
                        ApplyTransform(dbxBottom, 0, MaterialThickness34, -MaterialThickness34 - .5, 90, 90, 0);

                        Model3DGroup dbx1rotate = new();
                        dbx1rotate.Children.Add(dbxLeftSide);
                        dbx1rotate.Children.Add(dbxRightSide);
                        dbx1rotate.Children.Add(dbxFront);
                        dbx1rotate.Children.Add(dbxBack);
                        dbx1rotate.Children.Add(dbxBottom);

                        ApplyTransform(dbx1rotate, 0, 0, 0, 0, 90, 0);

                        Model3DGroup dbx2 = new();
                        dbx2.Children.Add(dbx1rotate);
                        ApplyTransform(dbx2, (dbxWidth / 2) - MaterialThickness34, height - dbxHeight - MaterialThickness34 - opening1Height - MaterialThickness34 - topSpacing, interiorDepth + backThickness, 0, 0, 0);
                        cabinet.Children.Add(dbx2);

                        if (baseCab.IncDrwBoxInListOpening2 && baseCab.DrwCount > 1)
                        {
                            //dbxHeight = opening2Height - topSpacing - bottomSpacing;
                            AddDrawerBoxRow(baseCab, "Drawer Box 2", dbxHeight, dbxWidth, dbxDepth);
                        }

                    }


                    if (baseCab.IncDrwBoxOpening3 && baseCab.DrwCount > 2)
                    {
                        dbxHeight = opening3Height - topSpacing - bottomSpacing;
                        if (baseCab.DrwCount > 3) // if true this is a middle drawer and needs additional bottom spacing
                        {
                            dbxHeight -= tandemMidDrwBottomSpacingAdjustment;
                        }
                        dbxSidePoints =
                        [
                            new (dbxDepth,dbxHeight,0),
                            new (0,dbxHeight,0),
                            new (0,0,0),
                            new (dbxDepth,0,0)
                        ];
                        //Debug.WriteLine("Drw Box Sides");

                        dbxLeftSide = CreatePanel(dbxSidePoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
                        dbxRightSide = CreatePanel(dbxSidePoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

                        dbxFrontAndBackPoints =
                        [
                            new (dbxFrontAndBackWidth,dbxHeight,0),
                            new (0,dbxHeight,0),
                            new (0,0,0),
                            new (dbxFrontAndBackWidth,0,0)
                        ];
                        //Debug.WriteLine("Drw Front and Back");

                        dbxFront = CreatePanel(dbxFrontAndBackPoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
                        dbxBack = CreatePanel(dbxFrontAndBackPoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

                        dbxBottomPoints =
                        [
                            new (0,0,0),
                            new (dbxBottomWidth,0,0),
                            new (dbxBottomWidth,dbxBottomLength,0),
                            new (0,dbxBottomLength,0)
                        ];
                        //Debug.WriteLine("Drw Box Bottom");

                        dbxBottom = CreatePanel(dbxBottomPoints, MaterialThickness34, "Prefinished Ply", "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);

                        ApplyTransform(dbxLeftSide, 0, 0, -(dbxWidth - MaterialThickness34), 0, 0, 0);
                        ApplyTransform(dbxFront, 0, 0, 0, 0, 90, 0);
                        ApplyTransform(dbxBack, 0, 0, dbxDepth - MaterialThickness34, 0, 90, 0);
                        ApplyTransform(dbxBottom, 0, MaterialThickness34, -MaterialThickness34 - .5, 90, 90, 0);

                        Model3DGroup dbx1rotate = new();
                        dbx1rotate.Children.Add(dbxLeftSide);
                        dbx1rotate.Children.Add(dbxRightSide);
                        dbx1rotate.Children.Add(dbxFront);
                        dbx1rotate.Children.Add(dbxBack);
                        dbx1rotate.Children.Add(dbxBottom);

                        ApplyTransform(dbx1rotate, 0, 0, 0, 0, 90, 0);

                        Model3DGroup dbx3 = new();
                        dbx3.Children.Add(dbx1rotate);
                        ApplyTransform(dbx3, (dbxWidth / 2) - MaterialThickness34, height - dbxHeight - MaterialThickness34 - opening1Height - opening2Height - MaterialThickness34 - MaterialThickness34 - topSpacing, interiorDepth + backThickness, 0, 0, 0);
                        cabinet.Children.Add(dbx3);

                        if (baseCab.IncDrwBoxInListOpening3 && baseCab.DrwCount > 2)
                        {
                            //dbxHeight = opening2Height - topSpacing - bottomSpacing;
                            AddDrawerBoxRow(baseCab, "Drawer Box 2", dbxHeight, dbxWidth, dbxDepth);
                        }

                    }

                    if (baseCab.IncDrwBoxInListOpening4 && baseCab.DrwCount > 3)
                    {
                        dbxHeight = opening4Height - topSpacing - bottomSpacing;
                        AddDrawerBoxRow(baseCab, "Drawer Box 4", dbxHeight, dbxWidth, dbxDepth);
                    }

                    if (baseCab.IncDrwBoxOpening4 && baseCab.DrwCount > 3)
                    {
                        dbxHeight = opening4Height - topSpacing - bottomSpacing;

                        dbxSidePoints =
                        [
                            new (dbxDepth,dbxHeight,0),
                            new (0,dbxHeight,0),
                            new (0,0,0),
                            new (dbxDepth,0,0)
                        ];
                        //Debug.WriteLine("Drw Box Sides");

                        dbxLeftSide = CreatePanel(dbxSidePoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
                        dbxRightSide = CreatePanel(dbxSidePoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

                        dbxFrontAndBackPoints =
                        [
                            new (dbxFrontAndBackWidth,dbxHeight,0),
                            new (0,dbxHeight,0),
                            new (0,0,0),
                            new (dbxFrontAndBackWidth,0,0)
                        ];
                        //Debug.WriteLine("Drw Box Front and Back");

                        dbxFront = CreatePanel(dbxFrontAndBackPoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
                        dbxBack = CreatePanel(dbxFrontAndBackPoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

                        dbxBottomPoints =
                        [
                            new (0,0,0),
                            new (dbxBottomWidth,0,0),
                            new (dbxBottomWidth,dbxBottomLength,0),
                            new (0,dbxBottomLength,0)
                        ];
                        //Debug.WriteLine("Drw Bottom");

                        dbxBottom = CreatePanel(dbxBottomPoints, MaterialThickness34, "Prefinished Ply", "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);

                        ApplyTransform(dbxLeftSide, 0, 0, -(dbxWidth - MaterialThickness34), 0, 0, 0);
                        ApplyTransform(dbxFront, 0, 0, 0, 0, 90, 0);
                        ApplyTransform(dbxBack, 0, 0, dbxDepth - MaterialThickness34, 0, 90, 0);
                        ApplyTransform(dbxBottom, 0, MaterialThickness34, -MaterialThickness34 - .5, 90, 90, 0);

                        Model3DGroup dbx1rotate = new();
                        dbx1rotate.Children.Add(dbxLeftSide);
                        dbx1rotate.Children.Add(dbxRightSide);
                        dbx1rotate.Children.Add(dbxFront);
                        dbx1rotate.Children.Add(dbxBack);
                        dbx1rotate.Children.Add(dbxBottom);

                        ApplyTransform(dbx1rotate, 0, 0, 0, 0, 90, 0);

                        Model3DGroup dbx4 = new();
                        dbx4.Children.Add(dbx1rotate);
                        ApplyTransform(dbx4, (dbxWidth / 2) - MaterialThickness34, height - dbxHeight - MaterialThickness34 - opening1Height - opening2Height - opening3Height - MaterialThickness34 - MaterialThickness34 - MaterialThickness34 - topSpacing, interiorDepth + backThickness, 0, 0, 0);
                        cabinet.Children.Add(dbx4);
                    }


                    // Rollouts or Trash Drawer
                    if (baseCab.IncRollouts || baseCab.IncRolloutsInList || baseCab.TrashDrawer)
                    {
                        //double sideSpacing;
                        //topSpacing = 0;
                        //bottomSpacing = 0;
                        dbxHeight = rolloutHeight;

                        if (baseCab.DrwStyle is not null)
                        {
                            if (baseCab.DrwStyle.Contains("Blum"))
                            {
                                dbxWidth -= tandemSideSpacing;
                                //sideSpacing = tandemSideSpacing;
                                //topSpacing = tandemTopSpacing;
                                //bottomSpacing = tandemBottomSpacing;
                            }
                            else if (baseCab.DrwStyle.Contains("Accuride"))
                            {
                                dbxWidth -= accurideSideSpacing;
                                //sideSpacing = accurideSideSpacing;
                                //topSpacing = accurideTopSpacing;
                                //bottomSpacing = accurideBottomSpacing;
                            }
                        }

                        dbxFrontAndBackWidth = dbxWidth - (MaterialThickness34 * 2);
                        dbxBottomWidth = dbxWidth - (MaterialThickness34 * 2);
                        dbxBottomLength = dbxDepth - (MaterialThickness34 * 2);

                        if (baseCab.RolloutCount >= 1 || baseCab.TrashDrawer)
                        {
                            if (baseCab.TrashDrawer)
                            {
                                dbxHeight = 12;
                            }

                            dbxSidePoints =
                            [
                                new (dbxDepth,dbxHeight,0),
                                new (0,dbxHeight,0),
                                new (0,0,0),
                                new (dbxDepth,0,0)
                            ];

                            dbxLeftSide = CreatePanel(dbxSidePoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
                            dbxRightSide = CreatePanel(dbxSidePoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

                            dbxFrontAndBackPoints =
                            [
                                new (dbxFrontAndBackWidth,dbxHeight,0),
                                new (0,dbxHeight,0),
                                new (0,0,0),
                                new (dbxFrontAndBackWidth,0,0)
                            ];

                            dbxFront = CreatePanel(dbxFrontAndBackPoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
                            dbxBack = CreatePanel(dbxFrontAndBackPoints, MaterialThickness34, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

                            dbxBottomPoints =
                            [
                                new (0,0,0),
                                new (dbxBottomWidth,0,0),
                                new (dbxBottomWidth,dbxBottomLength,0),
                                new (0,dbxBottomLength,0)
                            ];

                            dbxBottom = CreatePanel(dbxBottomPoints, MaterialThickness34, "Prefinished Ply", "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);

                            // Build box:
                            ApplyTransform(dbxLeftSide, 0, 0, -(dbxWidth - MaterialThickness34), 0, 0, 0);
                            ApplyTransform(dbxFront, 0, 0, 0, 0, 90, 0);
                            ApplyTransform(dbxBack, 0, 0, dbxDepth - MaterialThickness34, 0, 90, 0);
                            ApplyTransform(dbxBottom, 0, MaterialThickness34, -MaterialThickness34 - .5, 90, 90, 0);

                            // Rotate Box:
                            Model3DGroup dbx1rotate = new();
                            dbx1rotate.Children.Add(dbxLeftSide);
                            dbx1rotate.Children.Add(dbxRightSide);
                            dbx1rotate.Children.Add(dbxFront);
                            dbx1rotate.Children.Add(dbxBack);
                            dbx1rotate.Children.Add(dbxBottom);
                            ApplyTransform(dbx1rotate, 0, 0, 0, 0, 90, 0);

                            if (baseCab.IncRollouts)
                            {
                                for (int r = 0; r < baseCab.RolloutCount; r++)
                                {
                                    if (baseCab.IncRolloutsInList)
                                    {
                                        AddDrawerBoxRow(baseCab, "Rollout", dbxHeight, dbxWidth, dbxDepth);
                                    }

                                    // Position Box in Cabinet:
                                    Model3DGroup dbx1 = new();
                                    dbx1.Children.Add(dbx1rotate);
                                    ApplyTransform(dbx1, (dbxWidth / 2) - MaterialThickness34, MaterialThickness34 + tk_Height + 0.5906 + (r * 6), interiorDepth + backThickness - .25, 0, 0, 0); // set rollout .25" back from front of cabinet
                                    cabinet.Children.Add(dbx1);
                                }
                            }
                            if (baseCab.TrashDrawer)
                            {
                                if (baseCab.IncDrwBoxesInList)
                                {
                                    AddDrawerBoxRow(baseCab, "Trash Drawer", dbxHeight, dbxWidth, dbxDepth);
                                }

                                if (baseCab.IncDrwBoxes)
                                {
                                    // Position Box in Cabinet:
                                    Model3DGroup trashDrawer = new();
                                    trashDrawer.Children.Add(dbx1rotate);
                                    ApplyTransform(trashDrawer, (dbxWidth / 2) - MaterialThickness34, MaterialThickness34 + tk_Height + 0.5906, interiorDepth + backThickness, 0, 0, 0); // set trash drawer .25" back from front of cabinet
                                    cabinet.Children.Add(trashDrawer);
                                }
                            }
                        }
                    }
                }
            }


            if (!LeftEndHidden) cabinet.Children.Add(leftEnd);
            if (!RightEndHidden) cabinet.Children.Add(rightEnd);
            if (!DeckHidden) cabinet.Children.Add(deck);
            if (!TopHidden) cabinet.Children.Add(top);
            cabinet.Children.Add(back);
            cabinet.Children.Add(toekick);
        }



        // 90 deg. Corner Cabinets Style 3
        if (cabType == style3)
        {
            // End Panels
            if (baseCab.HasTK)
            {
                leftEndPanelPoints =
                    [
                        new (leftDepth,tk_Height,0),
                        new (leftDepth,height,0),
                        new (0,height,0),
                        new (0,0,0),
                        new (3,0,0),
                        new (3,.5,0),
                        new (leftDepth-tk_Depth-3,.5,0),
                        new (leftDepth-tk_Depth-3,0,0),
                        new (leftDepth-tk_Depth,0,0),
                        new (leftDepth-tk_Depth,tk_Height,0)
                    ];

                rightEndPanelPoints =
                    [
                        new (rightDepth,tk_Height,0),
                        new (rightDepth,height,0),
                        new (0,height,0),
                        new (0,0,0),
                        new (3,0,0),
                        new (3,.5,0),
                        new (rightDepth-tk_Depth-3,.5,0),
                        new (rightDepth-tk_Depth-3,0,0),
                        new (rightDepth-tk_Depth,0,0),
                        new (rightDepth-tk_Depth,tk_Height,0)
                    ];
            }
            else
            {
                tk_Height = 0;
                tk_Depth = 0;

                leftEndPanelPoints =
                    [
                        new (leftDepth,0,0),
                        new (leftDepth,height,0),
                        new (0,height,0),
                        new (0,0,0)
                    ];

                rightEndPanelPoints =
                    [
                        new (rightDepth,0,0),
                        new (rightDepth,height,0),
                        new (0,height,0),
                        new (0,0,0)
                    ];
            }
            leftEnd = CreatePanel(leftEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            rightEnd = CreatePanel(rightEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

            ApplyTransform(leftEnd, 0, 0, 0, 0, 270, 0);
            ApplyTransform(rightEnd, -(rightDepth - MaterialThickness34) - leftFrontWidth, 0, -leftDepth - rightFrontWidth, 0, 180, 0);

            // Deck & top
            deckPoints =
                [
                    new (0,0,0),
                    new (leftFrontWidth-MaterialThickness34,0,0),
                    new (leftFrontWidth-MaterialThickness34, rightFrontWidth-MaterialThickness34,0),
                    new ((leftFrontWidth - MaterialThickness34) + rightDepth - (doubleMaterialThickness34),rightFrontWidth - MaterialThickness34,0),
                    new ((leftFrontWidth - MaterialThickness34) + rightDepth - (doubleMaterialThickness34),-leftDepth + doubleMaterialThickness34,0),
                    new (0,-leftDepth + doubleMaterialThickness34,0),
                ];
            deck = CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, true, isPanel, panelEBEdges, isFaceUp: false);
            top = CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, true, isPanel, panelEBEdges, isFaceUp: false);

            ApplyTransform(top, 0, leftDepth, -height, 90, 0, 0);
            ApplyTransform(deck, 0, leftDepth, -tk_Height - MaterialThickness34, 90, 0, 0);

            // Backs

            // Left Back
            if (baseCab.HasTK)
            {
                backPoints =
                    [
                        new (0,0,0),
                        new (leftFrontWidth + rightDepth - MaterialThickness34 - backLegWidth - MaterialThickness34,0,0),
                        new (leftFrontWidth + rightDepth - MaterialThickness34 - backLegWidth - MaterialThickness34,-tk_Height,0),
                        new (leftFrontWidth + rightDepth - MaterialThickness34 - MaterialThickness34,-tk_Height,0),
                        new (leftFrontWidth + rightDepth - MaterialThickness34 - MaterialThickness34,height-tk_Height,0),
                        new (0,height-tk_Height,0)
                    ];
            }
            else
            {
                backPoints =
                    [
                        new (0,0,0),
                        new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,0,0),
                        new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,height,0),
                        new (0,height,0)
                    ];
            }
            leftBack = CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ApplyTransform(leftBack, 0, tk_Height, MaterialThickness34, 0, 0, 0);

            // Right Back
            backPoints =
                [
                    new (0,0,0),
                    new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,0,0),
                    new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,height-tk_Height,0),
                    new (0,height-tk_Height,0),
                ];
            rightBack = CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ApplyTransform(rightBack, -leftDepth - rightFrontWidth + MaterialThickness34, tk_Height, leftFrontWidth + rightDepth - doubleMaterialThickness34 - .75, 0, 90, 0);


            // Toekick
            if (baseCab.HasTK)
            {
                toekickPoints =
                    [
                        new (0,0,0),
                        new (leftFrontWidth - MaterialThickness34 + tk_Depth,0,0),
                        new (leftFrontWidth - MaterialThickness34 + tk_Depth,tk_Height-.5,0),
                        new (0,tk_Height-.5,0)
                    ];
                toekick1 = CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(toekick1, 0, 0.5, leftDepth - tk_Depth - MaterialThickness34, 0, 0, 0); // The hardcoded 1/2" here is because the actual toekick board is 1/2" narrower than the specified toekick height
                cabinet.Children.Add(toekick1);

                toekickPoints =
                    [
                        new (0,0,0),
                        new (rightFrontWidth + tk_Depth,0,0),
                        new (rightFrontWidth + tk_Depth,tk_Height-.5,0),
                        new (0,tk_Height-.5,0)
                    ];
                toekick2 = CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(toekick2, -leftDepth - rightFrontWidth + MaterialThickness34, 0.5, leftFrontWidth + tk_Depth - MaterialThickness34, 0, 90, 0); // The hardcoded 1/2" here is because the actual toekick board is 1/2" narrower than the specified toekick height
                cabinet.Children.Add(toekick2);
            }


            // Shelves
            if (shelfCount > 0)
            {
                double gap = .125;

                double shelfSpacing = (height - tk_Height - doubleMaterialThickness34) / (shelfCount + 1);
                for (int i = 1; i < shelfCount + 1; i++)
                {
                    shelfPoints =
                        [
                            new (0,0,0),
                            new (leftFrontWidth-MaterialThickness34-gap,0,0),
                            new (leftFrontWidth-MaterialThickness34-gap, rightFrontWidth-MaterialThickness34-gap,0),
                            new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,rightFrontWidth - MaterialThickness34-gap,0),
                            new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,-leftDepth + doubleMaterialThickness34 + gap,0),
                            new (0,-leftDepth + doubleMaterialThickness34 + gap,0),
                        ];
                    shelf = CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, "PVC Hardrock Maple", "Horizontal", baseCab, true, isPanel, panelEBEdges, isFaceUp: false);
                    ApplyTransform(shelf, 0 + .0625, leftDepth, -i * shelfSpacing - tk_Height, 90, 0, 0);
                    cabinet.Children.Add(shelf);
                }
            }

            // Doors
            double cornerCabDoorOpenSideReveal = 0.875;

            if (baseCab.DoorCount > 0 && baseCab.IncDoors || baseCab.DoorCount > 0 && baseCab.IncDoorsInList)
            {
                double door1Width = leftFrontWidth - doorLeftReveal - cornerCabDoorOpenSideReveal;
                double door2Width = rightFrontWidth - doorRightReveal - cornerCabDoorOpenSideReveal;

                double doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height;

                if (baseCab.IncDoorsInList)
                {
                    AddFrontPartRow(baseCab, "Door", doorHeight, door1Width, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                    AddFrontPartRow(baseCab, "Door", doorHeight, door2Width, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                }

                if (baseCab.IncDoors)
                {
                    doorPoints =
                    [
                        new (0,0,0),
                        new (door1Width,0,0),
                        new (door1Width,doorHeight,0),
                        new (0,doorHeight,0)
                    ];

                    door1 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);

                    doorPoints =
                        [
                            new (0,0,0),
                            new (door2Width,0,0),
                            new (door2Width,doorHeight,0),
                            new (0,doorHeight,0)
                        ];
                    door2 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);


                    if (!baseCab.HasTK)
                    {
                        ApplyTransform(door1, -MaterialThickness34 + doorLeftReveal, doorBottomReveal, leftDepth, 0, 0, 0);
                        ApplyTransform(door2, -leftDepth - door2Width - cornerCabDoorOpenSideReveal, doorBottomReveal, leftFrontWidth - (doubleMaterialThickness34), 0, 90, 0);
                    }
                    else
                    {
                        ApplyTransform(door1, -MaterialThickness34 + doorLeftReveal, doorBottomReveal + tk_Height, leftDepth, 0, 0, 0);
                        ApplyTransform(door2, -leftDepth - door2Width - cornerCabDoorOpenSideReveal, doorBottomReveal + tk_Height, leftFrontWidth - (doubleMaterialThickness34), 0, 90, 0);
                    }
                    cabinet.Children.Add(door1);
                    cabinet.Children.Add(door2);
                }
            }

            if (!LeftEndHidden) cabinet.Children.Add(leftEnd);
            if (!RightEndHidden) cabinet.Children.Add(rightEnd);
            if (!DeckHidden) cabinet.Children.Add(deck);
            if (!TopHidden) cabinet.Children.Add(top);
            cabinet.Children.Add(leftBack);
            cabinet.Children.Add(rightBack);
            ApplyTransform(cabinet, 0, 0, 0, 0, 45, 0);
        }


        // Angle Front - style 4
        if (cabType == style4)
        {
            // End Panels
            if (baseCab.HasTK)
            {
                leftEndPanelPoints =
                    [
                        new (leftDepth,tk_Height,0),
                        new (leftDepth,height,0),
                        new (0,height,0),
                        new (0,0,0),
                        new (3,0,0),
                        new (3,.5,0),
                        new (leftDepth-tk_Depth-3,.5,0),
                        new (leftDepth-tk_Depth-3,0,0),
                        new (leftDepth-tk_Depth,0,0),
                        new (leftDepth-tk_Depth,tk_Height,0)
                    ];

                rightEndPanelPoints =
                    [
                        new (rightDepth,tk_Height,0),
                        new (rightDepth,height,0),
                        new (0,height,0),
                        new (0,0,0),
                        new (3,0,0),
                        new (3,.5,0),
                        new (rightDepth-tk_Depth-3,.5,0),
                        new (rightDepth-tk_Depth-3,0,0),
                        new (rightDepth-tk_Depth,0,0),
                        new (rightDepth-tk_Depth,tk_Height,0)
                    ];
            }
            else
            {
                tk_Height = 0;
                tk_Depth = 0;

                leftEndPanelPoints =
                    [
                        new (leftDepth,0,0),
                        new (leftDepth,height,0),
                        new (0,height,0),
                        new (0,0,0)
                    ];

                rightEndPanelPoints =
                    [
                        new (rightDepth,0,0),
                        new (rightDepth,height,0),
                        new (0,height,0),
                        new (0,0,0)
                    ];
            }

            leftEnd = CreatePanel(leftEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            rightEnd = CreatePanel(rightEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

            ApplyTransform(leftEnd, 0, 0, -MaterialThickness34, 0, 90, 0);
            ApplyTransform(rightEnd, -leftBackWidth, 0, -rightBackWidth, 0, 0, 0);


            // Deck & top - build original polygon then normalize so edge[0] is at origin along +X
            var originalDeck = new List<Point3D>
            {
                new (leftDepth,MaterialThickness34,0),
                new (rightBackWidth - MaterialThickness34, leftBackWidth - rightDepth,0),
                new (rightBackWidth - MaterialThickness34, leftBackWidth - MaterialThickness34 - .25,0),
                new (MaterialThickness34 + .25, leftBackWidth - MaterialThickness34 - .25,0),
                new (MaterialThickness34 + .25, MaterialThickness34,0),
            };

            // Pick p0,p1 as the "front" edge we want to align
            var p0 = originalDeck[0];
            var p1 = originalDeck[1];

            // Vector from p0->p1 and its angle
            double vx = p1.X - p0.X;
            double vy = p1.Y - p0.Y;
            double frontWidth = Math.Sqrt(vx * vx + vy * vy);
            double angle = Math.Atan2(vy, vx); // radians

            // Precompute cos/sin for -angle (rotate points so edge lies on +X)
            double ca = Math.Cos(-angle);
            double sa = Math.Sin(-angle);

            // Translate so p0 -> origin, then rotate by -angle
            deckPoints = new List<Point3D>(originalDeck.Count);
            foreach (var q in originalDeck)
            {
                double tx = q.X - p0.X;
                double ty = q.Y - p0.Y;
                double rz = q.Z - p0.Z; // keep relative Z

                double rx = tx * ca - ty * sa;
                double ry = tx * sa + ty * ca;

                deckPoints.Add(new Point3D(rx, ry, rz));
            }

            // Create deck/top from normalized polygon (edge[0] now runs from (0,0,0) -> (edgeLen,0,0))
            deck = CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, isPanel, panelEBEdges, isFaceUp: false, ((angle * 180) / Math.PI) - 45);
            top = CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, isPanel, panelEBEdges, isFaceUp: false, ((angle * 180) / Math.PI) - 45);

            // Apply the same world transforms as before
            ApplyTransform(top, 0, 0, 0, -90, ((angle * 180) / Math.PI) + 90, 0);
            ApplyTransform(deck, 0, 0, 0, -90, ((angle * 180) / Math.PI) + 90, 0); //rads to degs ((angle * 180) / Math.PI) + 90
            var deckRotated = new Model3DGroup();
            var topRotated = new Model3DGroup();

            deckRotated.Children.Add(deck);
            topRotated.Children.Add(top);

            ApplyTransform(deckRotated, -MaterialThickness34, tk_Height, -leftDepth, 0, 0, 0);
            ApplyTransform(topRotated, -MaterialThickness34, height - MaterialThickness34, -leftDepth, 0, 0, 0);


            // Toekick
            if (baseCab.HasTK)
            {
                toekickPoints =
                [
                    new (-tk_Depth,0,0),
                    new (frontWidth + tk_Depth,0,0),
                    new (frontWidth + tk_Depth,tk_Height-.5,0),
                    new (-tk_Depth,tk_Height-.5,0)
                ];
                toekick = CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(toekick, 0, 0, -tk_Depth, 0, ((angle * 180) / Math.PI) + 90, 0);
                var toekickRotated = new Model3DGroup();
                toekickRotated.Children.Add(toekick);
                ApplyTransform(toekickRotated, -MaterialThickness34, .5, -leftDepth, 0, 0, 0);
                cabinet.Children.Add(toekickRotated);
            }


            // Backs

            // Left Back

            if (baseCab.HasTK)
            {
                backPoints =
                [
                    new (0,0,0),
                    new (backLegWidth,0,0),
                    new (backLegWidth,tk_Height,0),
                    new (leftBackWidth - MaterialThickness34 - .25,tk_Height,0),
                    new (leftBackWidth - MaterialThickness34 - .25,height,0),
                    new (0,height,0)
                ];

            }

            else
            {
                backPoints =
                [
                    new (0,0,0),
                    new (leftBackWidth - MaterialThickness34 - .25,0,0),
                    new (leftBackWidth - MaterialThickness34 - .25,height - tk_Height,0),
                    new (0,height - tk_Height,0)
                ];
            }

            leftBack = CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ApplyTransform(leftBack, -leftBackWidth + .25, 0, -MaterialThickness34 - .25, 0, 0, 0);

            // Right Back
            backPoints =
            [
                new (0,tk_Height,0),
                new (rightBackWidth - doubleMaterialThickness34 - .25,tk_Height,0),
                new (rightBackWidth - doubleMaterialThickness34 - .25,height,0),
                new (0,height,0)
            ];
            rightBack = CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ApplyTransform(rightBack, MaterialThickness34 + .25, 0, -leftBackWidth + .25, 0, 90, 0);


            // Shelves
            if (shelfCount > 0)
            {
                double gap = .125;

                double shelfSpacing = (height - doubleMaterialThickness34) / (shelfCount + 1);
                for (int i = 1; i < shelfCount + 1; i++)
                {
                    shelfPoints =
                        [
                            new (leftDepth,MaterialThickness34 + gap,0),
                            new (rightBackWidth - MaterialThickness34 - gap, leftBackWidth - rightDepth,0),
                            new (rightBackWidth - MaterialThickness34 - gap, leftBackWidth - MaterialThickness34 - .25 - gap,0),
                            new (MaterialThickness34 + .25 + gap, leftBackWidth - MaterialThickness34 - .25 - gap,0),
                            new (MaterialThickness34 + .25 + gap, MaterialThickness34 + gap,0),
                        ];
                    shelf = CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, "PVC Hardrock Maple", "Horizontal", baseCab, true, isPanel, panelEBEdges, isFaceUp: false, 45);
                    ApplyTransform(shelf, 0, gap / 2, +i * shelfSpacing, 90, 90, 180);
                    cabinet.Children.Add(shelf);
                }
            }

            // Doors
            if (baseCab.DoorCount > 0 && baseCab.IncDoors || baseCab.DoorCount > 0 && baseCab.IncDoorsInList)
            {
                double door1Width = frontWidth - doorLeftReveal - doorRightReveal;

                double doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height;

                if (baseCab.DoorCount == 1)
                {
                    if (baseCab.IncDoorsInList)
                    {
                        AddFrontPartRow(baseCab, "Door", doorHeight, door1Width, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                    }

                    if (baseCab.IncDoors)
                    {
                        doorPoints =
                        [
                            new (0,0,0),
                        new (door1Width,0,0),
                        new (door1Width,doorHeight,0),
                        new (0,doorHeight,0)
                        ];
                        door1 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ApplyTransform(door1, doorLeftReveal, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);
                        var door1Rotated = new Model3DGroup();
                        door1Rotated.Children.Add(door1);
                        ApplyTransform(door1Rotated, -MaterialThickness34, tk_Height, -leftDepth, 0, 0, 0);
                        cabinet.Children.Add(door1Rotated);
                    }
                }
                if (baseCab.DoorCount == 2)
                {
                    door1Width = (frontWidth / 2) - doorLeftReveal - (baseDoorGap / 2);
                    double door2Width = (frontWidth / 2) - doorRightReveal - (baseDoorGap / 2);

                    if (baseCab.IncDoorsInList)
                    {
                        AddFrontPartRow(baseCab, "Door", doorHeight, door1Width, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                        AddFrontPartRow(baseCab, "Door", doorHeight, door2Width, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                    }

                    if (baseCab.IncDoors)
                    {
                        doorPoints =
                        [
                            new (0,0,0),
                        new (door1Width,0,0),
                        new (door1Width,doorHeight,0),
                        new (0,doorHeight,0)
                        ];
                        door1 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ApplyTransform(door1, doorLeftReveal, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);
                        var door1Rotated = new Model3DGroup();
                        door1Rotated.Children.Add(door1);
                        ApplyTransform(door1Rotated, -MaterialThickness34, tk_Height, -leftDepth, 0, 0, 0);
                        cabinet.Children.Add(door1Rotated);

                        doorPoints =
                        [
                            new (0,0,0),
                        new (door2Width,0,0),
                        new (door2Width,doorHeight,0),
                        new (0,doorHeight,0)
                        ];
                        door2 = CreatePanel(doorPoints, MaterialThickness34, baseCab.DoorSpecies, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ApplyTransform(door2, door1Width + doorLeftReveal + baseDoorGap, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);
                        var door2Rotated = new Model3DGroup();
                        door2Rotated.Children.Add(door2);
                        ApplyTransform(door2Rotated, -MaterialThickness34, tk_Height, -leftDepth, 0, 0, 0);
                        cabinet.Children.Add(door2Rotated);
                    }
                }
            }

            if (!LeftEndHidden) cabinet.Children.Add(leftEnd);
            if (!RightEndHidden) cabinet.Children.Add(rightEnd);
            if (!DeckHidden) cabinet.Children.Add(deckRotated);
            if (!TopHidden) cabinet.Children.Add(topRotated);
            cabinet.Children.Add(leftBack);
            cabinet.Children.Add(rightBack);
            ApplyTransform(cabinet, 0, 0, 0, 0, -135, 0);
        }

    }

    private void BuildUpper(Model3DGroup cabinet, UpperCabinetModel upperCab)
    {
        Model3DGroup leftEnd;
        Model3DGroup rightEnd;
        Model3DGroup deck;
        Model3DGroup top;
        Model3DGroup shelf;
        Model3DGroup back;
        Model3DGroup leftBack;
        Model3DGroup rightBack;
        Model3DGroup door1;
        Model3DGroup door2;
        Model3DGroup nailer;

        List<Point3D> endPanelPoints;
        List<Point3D> leftEndPanelPoints;
        List<Point3D> rightEndPanelPoints;
        List<Point3D> deckPoints;
        List<Point3D> topPoints;
        List<Point3D> backPoints;
        List<Point3D> shelfPoints;
        List<Point3D> doorPoints;
        List<Point3D> nailerPoints;

        double MaterialThickness34 = 0.75;
        double MaterialThickness14 = 0.25;
        //double halfMaterialThickness34 = MaterialThickness34 / 2; // This is to make door calcs etc. more straightforward
        double doubleMaterialThickness34 = MaterialThickness34 * 2; // This is to make door calcs etc. more straightforward
        //double tripleMaterialThickness34 = MaterialThickness34 * 3; // This is to make door calcs etc. more straightforward
        //double quadrupleMaterialThickness34 = MaterialThickness34 * 4; // This is to make door calcs etc. more straightforward

        //double StretcherWidth = 6;

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
        int shelfCount = upperCab.ShelfCount;
        double shelfDepth = interiorDepth;
        shelfDepth -= 0.125;
        double upperDoorGap = ConvertDimension.FractionToDouble(upperCab.GapWidth);
        double doorLeftReveal = ConvertDimension.FractionToDouble(upperCab.LeftReveal);
        double doorRightReveal = ConvertDimension.FractionToDouble(upperCab.RightReveal);
        double doorTopReveal = ConvertDimension.FractionToDouble(upperCab.TopReveal);
        double doorBottomReveal = ConvertDimension.FractionToDouble(upperCab.BottomReveal);
        double doorSideReveal = (doorLeftReveal + doorRightReveal) / 2; // this averages the potentially different left and right reveals so that the door creation calc can use just one variable instead of two.
        double StretcherWidth = 6;
        bool topDeck90 = false; // This is sent to the panel creator to let it know if it is a top or deck at 90 degrees so it cab have 2 edgebanded edges
        bool isPanel = false; // This is sent to the panel creator to let it know if it is a panel (true) or a shelf/deck/top/toekick (false) so it can have edgebanding applied correctly.
        string panelEBEdges = "";

        string doorEdgebandingSpecies;

        doorEdgebandingSpecies = upperCab.DoorSpecies switch
        {
            null => "None",
            string s when s.Contains("Alder", StringComparison.OrdinalIgnoreCase) => "Wood Alder",
            string s when s.Contains("Cherry", StringComparison.OrdinalIgnoreCase) => "Wood Cherry",
            string s when s.Contains("Hickory", StringComparison.OrdinalIgnoreCase) => "Wood Hickory",
            string s when s.Contains("Mahogany", StringComparison.OrdinalIgnoreCase) => "Wood Mahogany",
            string s when s.Contains("Maple", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
            string s when s.Contains("MDF", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
            string s when s.Contains("Melamine", StringComparison.OrdinalIgnoreCase) => "PVC Custom",
            string s when s.Contains("Prefinished Ply", StringComparison.OrdinalIgnoreCase) => "Wood Prefinished Maple",
            string s when s.Contains("Red Oak", StringComparison.OrdinalIgnoreCase) => "Wood Red Oak",
            string s when s.Contains("Walnut", StringComparison.OrdinalIgnoreCase) => "Wood Walnut",
            string s when s.Contains("White Oak", StringComparison.OrdinalIgnoreCase) => "Wood Oak",
            _ => "None"
        };

        endPanelPoints =
        [
            new (depth,0,0),
            new (depth,height,0),
            new (0,height,0),
            new (0,0,0)
        ];

        leftEnd = CreatePanel(endPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
        rightEnd = CreatePanel(endPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

        if (cabType.Contains(style1))
        {
            ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
            ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);
        }


        if (cabType.Contains(style1))
        {
            // Deck
            deckPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,depth,0),
                    new (0,depth,0)
                ];
            deck = CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
            ApplyTransform(deck, -(interiorWidth / 2), -depth, 0, 270, 0, 0);

            // Full Top
            topPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,depth,0),
                    new (0,depth,0)
                ];
            top = CreatePanel(topPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
            ApplyTransform(top, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);


            // Back
            if (backThickness == 0.75)
            {
                backPoints =
                    [
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,interiorHeight,0),
                        new (0,interiorHeight,0)
                    ];
                back = CreatePanel(backPoints, MaterialThickness34, upperCab.Species, "None", "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(back, -(interiorWidth / 2), MaterialThickness34, 0, 0, 0, 0);
            }
            else
            {
                backPoints =
                    [
                        new (0,0,0),
                        new (width,0,0),
                        new (width,height,0),
                        new (0,height,0)
                    ];
                back = CreatePanel(backPoints, MaterialThickness14, "PFP 1/4", "None", "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(back, -(width / 2), 0, -MaterialThickness14, 0, 0, 0);


                nailerPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,StretcherWidth,0),
                    new (0,StretcherWidth,0)
                ];

                nailer = CreatePanel(nailerPoints, MaterialThickness34, upperCab.Species, "PVC Hardrock Maple", "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(nailer, -(interiorWidth / 2), height - StretcherWidth - MaterialThickness34, 0, 0, 0, 0);
                cabinet.Children.Add(nailer);

                nailer = CreatePanel(nailerPoints, MaterialThickness34, upperCab.Species, "PVC Hardrock Maple", "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(nailer, -(interiorWidth / 2), 0 + MaterialThickness34, 0, 0, 0, 0);
                cabinet.Children.Add(nailer);

            }


            // Shelves
            double shelfSpacing = interiorHeight + MaterialThickness34; // This should be the space between the shelves
            shelfSpacing /= (upperCab.ShelfCount + 1);


            for (int i = 1; i < upperCab.ShelfCount + 1; i++)
            {
                shelfPoints =
                    [
                        new (0,0,0),
                        new (interiorWidth-.125,0,0),
                        new (interiorWidth-.125,shelfDepth,0),
                        new (0,shelfDepth,0)
                    ];
                shelf = CreatePanel(shelfPoints, MaterialThickness34, upperCab.Species, "PVC Hardrock Maple", "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -MaterialThickness34 - shelfDepth, i * shelfSpacing, 270, 0, 0);
                cabinet.Children.Add(shelf);
            }


            // Doors
            if (upperCab.DoorCount > 0 && upperCab.IncDoors || upperCab.DoorCount > 0 && upperCab.IncDoorsInList)
            {
                double doorWidth = width - (doorSideReveal * 2);
                double doorHeight = height - doorTopReveal - doorBottomReveal;

                if (upperCab.DoorCount == 1)
                {

                    if (upperCab.IncDoorsInList)
                    {
                        AddFrontPartRow(upperCab, "Door", doorHeight, doorWidth, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                    }

                    if (upperCab.IncDoors)
                    {
                        doorPoints =
                        [
                            new (0,0,0),
                            new (doorWidth,0,0),
                            new (doorWidth,doorHeight,0),
                            new (0,doorHeight,0)
                        ];
                        door1 = CreatePanel(doorPoints, MaterialThickness34, upperCab.DoorSpecies, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);

                        cabinet.Children.Add(door1);
                    }
                }

                if (upperCab.DoorCount == 2)
                {
                    doorWidth = (doorWidth / 2) - (upperDoorGap / 2);


                    if (upperCab.IncDoorsInList)
                    {
                        AddFrontPartRow(upperCab, "Door", doorHeight, doorWidth, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                        AddFrontPartRow(upperCab, "Door", doorHeight, doorWidth, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                    }

                    if (upperCab.IncDoors)
                    {
                        doorPoints =
                        [
                            new (0,0,0),
                            new (doorWidth,0,0),
                            new (doorWidth, doorHeight, 0),
                            new (0,doorHeight,0)
                        ];
                        door1 = CreatePanel(doorPoints, MaterialThickness34, upperCab.DoorSpecies, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);
                        door2 = CreatePanel(doorPoints, MaterialThickness34, upperCab.DoorSpecies, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                        ApplyTransform(door2, (width / 2) - doorWidth - doorRightReveal, doorBottomReveal, depth, 0, 0, 0);

                        cabinet.Children.Add(door1);
                        cabinet.Children.Add(door2);
                    }
                }
            }

            if (!LeftEndHidden) cabinet.Children.Add(leftEnd);
            if (!RightEndHidden) cabinet.Children.Add(rightEnd);
            if (!DeckHidden) cabinet.Children.Add(deck);
            if (!TopHidden) cabinet.Children.Add(top);
            cabinet.Children.Add(back);

        }


        // 90 deg. Corner Cabinet Style 2
        if (cabType == style2)
        {
            // End Panels

            leftEndPanelPoints =
                [
                    new (leftDepth,0,0),
                    new (leftDepth,height,0),
                    new (0,height,0),
                    new (0,0,0)
                ];

            rightEndPanelPoints =
                [
                    new (rightDepth,0,0),
                    new (rightDepth,height,0),
                    new (0,height,0),
                    new (0,0,0)
                ];

            leftEnd = CreatePanel(leftEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            rightEnd = CreatePanel(rightEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

            ApplyTransform(leftEnd, 0, 0, 0, 0, 270, 0);
            ApplyTransform(rightEnd, -(rightDepth - MaterialThickness34) - leftFrontWidth, 0, -leftDepth - rightFrontWidth, 0, 180, 0);

            // Deck & top
            deckPoints =
                [
                    new (0,0,0),
                    new (leftFrontWidth-MaterialThickness34,0,0),
                    new (leftFrontWidth-MaterialThickness34, rightFrontWidth-MaterialThickness34,0),
                    new ((leftFrontWidth - MaterialThickness34) + rightDepth - (doubleMaterialThickness34),rightFrontWidth - MaterialThickness34,0),
                    new ((leftFrontWidth - MaterialThickness34) + rightDepth - (doubleMaterialThickness34),-leftDepth + doubleMaterialThickness34,0),
                    new (0,-leftDepth + doubleMaterialThickness34,0),
                ];
            deck = CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, true, isPanel, panelEBEdges, isFaceUp: false);
            top = CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, true, isPanel, panelEBEdges, isFaceUp: false);

            ApplyTransform(top, 0, leftDepth, -height, 90, 0, 0);
            ApplyTransform(deck, 0, leftDepth, -MaterialThickness34, 90, 0, 0);

            // Backs

            // Left Back

            backPoints =
                [
                    new (0,0,0),
                    new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,0,0),
                    new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,height,0),
                    new (0,height,0)
                ];
            leftBack = CreatePanel(backPoints, MaterialThickness34, upperCab.Species, "None", "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ApplyTransform(leftBack, 0, 0, MaterialThickness34, 0, 0, 0);

            // Right Back
            backPoints =
                [
                    new (0,0,0),
                    new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,0,0),
                    new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,height,0),
                    new (0,height,0),
                ];
            rightBack = CreatePanel(backPoints, MaterialThickness34, upperCab.Species, "None", "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ApplyTransform(rightBack, -leftDepth - rightFrontWidth + MaterialThickness34, 0, leftFrontWidth + rightDepth - doubleMaterialThickness34 - .75, 0, 90, 0);


            // Shelves
            if (shelfCount > 0)
            {
                double gap = .125;

                double shelfSpacing = (height - doubleMaterialThickness34) / (shelfCount + 1);
                for (int i = 1; i < shelfCount + 1; i++)
                {
                    shelfPoints =
                        [
                            new (0,0,0),
                            new (leftFrontWidth-MaterialThickness34-gap,0,0),
                            new (leftFrontWidth-MaterialThickness34-gap, rightFrontWidth-MaterialThickness34-gap,0),
                            new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,rightFrontWidth - MaterialThickness34-gap,0),
                            new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,-leftDepth + doubleMaterialThickness34 + gap,0),
                            new (0,-leftDepth + doubleMaterialThickness34 + gap,0),
                        ];
                    shelf = CreatePanel(shelfPoints, MaterialThickness34, upperCab.Species, "PVC Hardrock Maple", "Horizontal", upperCab, true, isPanel, panelEBEdges, isFaceUp: false);
                    ApplyTransform(shelf, 0 + .0625, leftDepth, -i * shelfSpacing, 90, 0, 0);
                    cabinet.Children.Add(shelf);
                }
            }

            // Doors
            double cornerCabDoorOpenSideReveal = 0.875;

            if (upperCab.DoorCount > 0 && upperCab.IncDoors || upperCab.DoorCount > 0 && upperCab.IncDoorsInList)
            {
                double door1Width = leftFrontWidth - doorLeftReveal - cornerCabDoorOpenSideReveal;
                double door2Width = rightFrontWidth - doorRightReveal - cornerCabDoorOpenSideReveal;

                double doorHeight = height - doorTopReveal - doorBottomReveal;

                if (upperCab.IncDoorsInList)
                {
                    AddFrontPartRow(upperCab, "Door", doorHeight, door1Width, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                    AddFrontPartRow(upperCab, "Door", doorHeight, door2Width, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                }

                if (upperCab.IncDoors)
                {
                    doorPoints =
                    [
                        new (0,0,0),
                        new (door1Width,0,0),
                        new (door1Width,doorHeight,0),
                        new (0,doorHeight,0)
                    ];
                    door1 = CreatePanel(doorPoints, MaterialThickness34, upperCab.DoorSpecies, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);

                    doorPoints =
                        [
                            new (0,0,0),
                        new (door2Width,0,0),
                        new (door2Width,doorHeight,0),
                        new (0,doorHeight,0)
                        ];
                    door2 = CreatePanel(doorPoints, MaterialThickness34, upperCab.DoorSpecies, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);

                    ApplyTransform(door1, -MaterialThickness34 + doorLeftReveal, doorBottomReveal, leftDepth, 0, 0, 0);
                    ApplyTransform(door2, -leftDepth - door2Width - cornerCabDoorOpenSideReveal, doorBottomReveal, leftFrontWidth - (doubleMaterialThickness34), 0, 90, 0);
                    cabinet.Children.Add(door1);
                    cabinet.Children.Add(door2);
                }
            }

            if (!LeftEndHidden) cabinet.Children.Add(leftEnd);
            if (!RightEndHidden) cabinet.Children.Add(rightEnd);
            if (!DeckHidden) cabinet.Children.Add(deck);
            if (!TopHidden) cabinet.Children.Add(top);
            cabinet.Children.Add(leftBack);
            cabinet.Children.Add(rightBack);
            ApplyTransform(cabinet, 0, 0, 0, 0, 45, 0);
        }



        // Angle Front Corner Cabinet Style 3

        if (cabType == style3)
        {
            // End Panels

            leftEndPanelPoints =
            [
                new (leftDepth,0,0),
                new (leftDepth,height,0),
                new (0,height,0),
                new (0,0,0)
            ];

            rightEndPanelPoints =
            [
                new (rightDepth,0,0),
                new (rightDepth,height,0),
                new (0,height,0),
                new (0,0,0)
            ];

            leftEnd = CreatePanel(leftEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            rightEnd = CreatePanel(rightEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

            ApplyTransform(leftEnd, 0, 0, -MaterialThickness34, 0, 90, 0);
            ApplyTransform(rightEnd, -leftBackWidth, 0, -rightBackWidth, 0, 0, 0);


            // Deck & top - build original polygon then normalize so edge[0] is at origin along +X
            var originalDeck = new List<Point3D>
            {
                new (leftDepth,MaterialThickness34,0),
                new (rightBackWidth - MaterialThickness34, leftBackWidth - rightDepth,0),
                new (rightBackWidth - MaterialThickness34, leftBackWidth - MaterialThickness34 - .25,0),
                new (MaterialThickness34 + .25, leftBackWidth - MaterialThickness34 - .25,0),
                new (MaterialThickness34 + .25, MaterialThickness34,0),
            };

            // Pick p0,p1 as the "front" edge we want to align
            var p0 = originalDeck[0];
            var p1 = originalDeck[1];

            // Vector from p0->p1 and its angle
            double vx = p1.X - p0.X;
            double vy = p1.Y - p0.Y;
            double frontWidth = Math.Sqrt(vx * vx + vy * vy);
            double angle = Math.Atan2(vy, vx); // radians

            // Precompute cos/sin for -angle (rotate points so edge lies on +X)
            double ca = Math.Cos(-angle);
            double sa = Math.Sin(-angle);

            // Translate so p0 -> origin, then rotate by -angle
            deckPoints = new List<Point3D>(originalDeck.Count);
            foreach (var q in originalDeck)
            {
                double tx = q.X - p0.X;
                double ty = q.Y - p0.Y;
                double rz = q.Z - p0.Z; // keep relative Z

                double rx = tx * ca - ty * sa;
                double ry = tx * sa + ty * ca;

                deckPoints.Add(new Point3D(rx, ry, rz));
            }

            // Create deck/top from normalized polygon (edge[0] now runs from (0,0,0) -> (edgeLen,0,0))
            deck = CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, false, isPanel, panelEBEdges, isFaceUp: false, ((angle * 180) / Math.PI) - 45);
            top = CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, false, isPanel, panelEBEdges, isFaceUp: false, ((angle * 180) / Math.PI) - 45);

            // Apply the same world transforms as before
            ApplyTransform(top, 0, 0, 0, -90, ((angle * 180) / Math.PI) + 90, 0);
            ApplyTransform(deck, 0, 0, 0, -90, ((angle * 180) / Math.PI) + 90, 0); //rads to degs ((angle * 180) / Math.PI) + 90
            var deckRotated = new Model3DGroup();
            var topRotated = new Model3DGroup();

            deckRotated.Children.Add(deck);
            topRotated.Children.Add(top);

            ApplyTransform(deckRotated, -MaterialThickness34, 0, -leftDepth, 0, 0, 0);
            ApplyTransform(topRotated, -MaterialThickness34, height - MaterialThickness34, -leftDepth, 0, 0, 0);


            // Backs

            // Left Back

            backPoints =
            [
                new (0,0,0),
                new (leftBackWidth - MaterialThickness34 - .25,0,0),
                new (leftBackWidth - MaterialThickness34 - .25,height,0),
                new (0,height,0)
            ];
            leftBack = CreatePanel(backPoints, MaterialThickness34, upperCab.Species, "None", "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ApplyTransform(leftBack, -leftBackWidth + .25, 0, -MaterialThickness34 - .25, 0, 0, 0);

            // Right Back
            backPoints =
            [
                new (0,0,0),
                new (rightBackWidth - doubleMaterialThickness34 - .25,0,0),
                new (rightBackWidth - doubleMaterialThickness34 - .25,height,0),
                new (0,height,0)
            ];
            rightBack = CreatePanel(backPoints, MaterialThickness34, upperCab.Species, "None", "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ApplyTransform(rightBack, MaterialThickness34 + .25, 0, -leftBackWidth + .25, 0, 90, 0);


            // Shelves
            if (shelfCount > 0)
            {
                double gap = .125;

                double shelfSpacing = (height - doubleMaterialThickness34) / (shelfCount + 1);
                for (int i = 1; i < shelfCount + 1; i++)
                {
                    shelfPoints =
                        [
                            new (leftDepth,MaterialThickness34 + gap,0),
                            new (rightBackWidth - MaterialThickness34 - gap, leftBackWidth - rightDepth,0),
                            new (rightBackWidth - MaterialThickness34 - gap, leftBackWidth - MaterialThickness34 - .25 - gap,0),
                            new (MaterialThickness34 + .25 + gap, leftBackWidth - MaterialThickness34 - .25 - gap,0),
                            new (MaterialThickness34 + .25 + gap, MaterialThickness34 + gap,0),
                        ];
                    shelf = CreatePanel(shelfPoints, MaterialThickness34, upperCab.Species, "PVC Hardrock Maple", "Horizontal", upperCab, true, isPanel, panelEBEdges, isFaceUp: false, 45);
                    ApplyTransform(shelf, 0, gap / 2, +i * shelfSpacing, 90, 90, 180);
                    cabinet.Children.Add(shelf);
                }
            }

            // Doors
            if (upperCab.DoorCount > 0 && upperCab.IncDoors || upperCab.DoorCount > 0 && upperCab.IncDoorsInList)
            {
                double door1Width = frontWidth - doorLeftReveal - doorRightReveal;
                double doorHeight = height - doorTopReveal - doorBottomReveal;

                if (upperCab.DoorCount == 1)
                {
                    if (upperCab.IncDoorsInList)
                    {
                        AddFrontPartRow(upperCab, "Door", doorHeight, door1Width, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                    }

                    if (upperCab.IncDoors)
                    {
                        doorPoints =
                        [
                            new (0,0,0),
                            new (door1Width,0,0),
                            new (door1Width,doorHeight,0),
                            new (0,doorHeight,0)
                        ];
                        door1 = CreatePanel(doorPoints, MaterialThickness34, upperCab.DoorSpecies, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ApplyTransform(door1, doorLeftReveal, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);
                        var door1Rotated = new Model3DGroup();
                        door1Rotated.Children.Add(door1);
                        ApplyTransform(door1Rotated, -MaterialThickness34, 0, -leftDepth, 0, 0, 0);
                        cabinet.Children.Add(door1Rotated);
                    }
                }
                if (upperCab.DoorCount == 2)
                {
                    door1Width = (frontWidth / 2) - doorLeftReveal - (upperDoorGap / 2);
                    double door2Width = (frontWidth / 2) - doorRightReveal - (upperDoorGap / 2);

                    if (upperCab.IncDoorsInList)
                    {
                        AddFrontPartRow(upperCab, "Door", doorHeight, door1Width, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                        AddFrontPartRow(upperCab, "Door", doorHeight, door2Width, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                    }

                    if (upperCab.IncDoors)
                    {
                        doorPoints =
                        [
                            new (0,0,0),
                            new (door1Width,0,0),
                            new (door1Width,doorHeight,0),
                            new (0,doorHeight,0)
                        ];
                        door1 = CreatePanel(doorPoints, MaterialThickness34, upperCab.DoorSpecies, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ApplyTransform(door1, doorLeftReveal, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);
                        var door1Rotated = new Model3DGroup();
                        door1Rotated.Children.Add(door1);
                        ApplyTransform(door1Rotated, -MaterialThickness34, 0, -leftDepth, 0, 0, 0);
                        cabinet.Children.Add(door1Rotated);

                        doorPoints =
                        [
                            new (0,0,0),
                            new (door2Width,0,0),
                            new (door2Width,doorHeight,0),
                            new (0,doorHeight,0)
                        ];
                        door2 = CreatePanel(doorPoints, MaterialThickness34, upperCab.DoorSpecies, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ApplyTransform(door2, door1Width + doorLeftReveal + upperDoorGap, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);
                        var door2Rotated = new Model3DGroup();
                        door2Rotated.Children.Add(door2);
                        ApplyTransform(door2Rotated, -MaterialThickness34, 0, -leftDepth, 0, 0, 0);
                        cabinet.Children.Add(door2Rotated);
                    }
                }
            }

            if (!LeftEndHidden) cabinet.Children.Add(leftEnd);
            if (!RightEndHidden) cabinet.Children.Add(rightEnd);
            if (!DeckHidden) cabinet.Children.Add(deckRotated);
            if (!TopHidden) cabinet.Children.Add(topRotated);
            cabinet.Children.Add(leftBack);
            cabinet.Children.Add(rightBack);
            ApplyTransform(cabinet, 0, 0, 0, 0, -135, 0);
        }

    }

    private static void BuildFiller(Model3DGroup cabinet, FillerModel filler)
    {
        Model3DGroup leftEnd;
        //Model3DGroup top = new();
        //Model3DGroup toekick = new();
        //Model3DGroup toekick1 = new();
        //Model3DGroup toekick2 = new();
        Model3DGroup back;

        List<Point3D> endPanelPoints;
        //List<Point3D> leftEndPanelPoints = [];
        //List<Point3D> rightEndPanelPoints = [];
        List<Point3D> backPoints;

        double MaterialThickness34 = 0.75;
        //double halfMaterialThickness34 = MaterialThickness34 / 2; // This is to make door calcs etc. more straightforward
        //double doubleMaterialThickness34 = MaterialThickness34 * 2; // This is to make door calcs etc. more straightforward
        //double tripleMaterialThickness34 = MaterialThickness34 * 3; // This is to make door calcs etc. more straightforward
        //double quadrupleMaterialThickness34 = MaterialThickness34 * 4; // This is to make door calcs etc. more straightforward

        double width = ConvertDimension.FractionToDouble(filler.Width);
        double height = ConvertDimension.FractionToDouble(filler.Height);
        double depth = ConvertDimension.FractionToDouble(filler.Depth);
        bool topDeck90 = false; // This is sent to the panel creator to let it know if it is a top or deck at 90 degrees so it cab have 2 edgebanded edges
        bool isPanel = false; // This is sent to the panel creator to let it know if it is a panel (true) or a shelf/deck/top/toekick (false) so it can have edgebanding applied correctly.
        string panelEBEdges = "";

        endPanelPoints =
        [
            new (depth,0,0),
            new (depth,height,0),
            new (0,height,0),
            new (0,0,0)
        ];

        leftEnd = CreatePanel(endPanelPoints, MaterialThickness34, filler.Species, "None", "Vertical", filler, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
        ApplyTransform(leftEnd, 0, 0, -MaterialThickness34, 0, 270, 0);

        backPoints =
        [
            new (0,0,0),
            new (width,0,0),
            new (width,height,0),
            new (0,height,0)
        ];

        back = CreatePanel(backPoints, MaterialThickness34, filler.Species, "None", "Vertical", filler, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
        ApplyTransform(back, 0, 0, depth, 0, 0, 0);

        cabinet.Children.Add(leftEnd);
        cabinet.Children.Add(back);

    }

    private static void BuildPanel(Model3DGroup cabinet, PanelModel panel)
    {
        //Model3DGroup leftEnd;
        //Model3DGroup top = new();
        //Model3DGroup toekick = new();
        //Model3DGroup toekick1 = new();
        //Model3DGroup toekick2 = new();
        Model3DGroup back;

        //List<Point3D> endPanelPoints = [];
        //List<Point3D> leftEndPanelPoints = [];
        //List<Point3D> rightEndPanelPoints = [];
        List<Point3D> backPoints;

        double width = ConvertDimension.FractionToDouble(panel.Width);
        double height = ConvertDimension.FractionToDouble(panel.Height);
        double depth = ConvertDimension.FractionToDouble(panel.Depth);
        bool topDeck90 = false; // This is sent to the panel creator to let it know if it is a top or deck at 90 degrees so it cab have 2 edgebanded edges
        bool isPanel = true; // This is sent to the panel creater to let it know it is just a panel so it can edgeband all 4 edges
        string panelEBEdges = "";
        if (panel.PanelEBTop) { panelEBEdges += "T"; }
        if (panel.PanelEBBottom) { panelEBEdges += "B"; }
        if (panel.PanelEBLeft) { panelEBEdges += "L"; }
        if (panel.PanelEBRight) { panelEBEdges += "R"; }

        backPoints =
        [
            new (0,0,0),
            new (width,0,0),
            new (width,height,0),
            new (0,height,0)
        ];

        back = CreatePanel(backPoints, depth, panel.Species, panel.EBSpecies, "Vertical", panel, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
        ApplyTransform(back, 0, 0, depth / 2, 0, 0, 0);

        cabinet.Children.Add(back);
    }


    private static Model3DGroup CreatePanel(
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
        double areaFt2 = areaSqIn / 144.0; // convert in^2 -> ft^2

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
            if (cab != null)
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

        // Track edgebanding total length (in inches) for this panel
        double edgeBandLengthInches = 0.0;

        // Special handling for panels: panelEBEdges letters map to physical panel width/height,
        // so compute explicitly rather than relying on polygon winding/order (which can vary).
        if (isPanel && !string.IsNullOrEmpty(panelEBEdges))
        {
            double panelWidthInches = maxX - minX;  // X spans width
            double panelHeightInches = maxY - minY; // Y spans height

            if (panelEBEdges.Contains('T')) edgeBandLengthInches += panelWidthInches;
            if (panelEBEdges.Contains('B')) edgeBandLengthInches += panelWidthInches;
            if (panelEBEdges.Contains('L')) edgeBandLengthInches += panelHeightInches;
            if (panelEBEdges.Contains('R')) edgeBandLengthInches += panelHeightInches;

            // We still create the specialBuilder quads below for visual edgebanding, but
            // we deliberately do not add per-edge sideLengths again to avoid double-counting.
        }

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
                    mainBuilder.Positions[b0],
                    mainBuilder.Positions[b1],
                    mainBuilder.Positions[t1],
                    mainBuilder.Positions[t0],
                    uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);

                    // accumulate edgeband length for this edge (non-panel path)
                    edgeBandLengthInches += sideLengths[edgeFace];
                }
                //################################################################################## Add check for upper cabinet here and add edgebanding to edgeFace == 1. This works but species can't be changed per edge.
                //else if (cab.CabinetType == "Upper Cabinet" && edgeFace == 3)
                //{
                //    specialBuilder.AddQuad(
                //    mainBuilder.Positions[b0],
                //    mainBuilder.Positions[b1],
                //    mainBuilder.Positions[t1],
                //    mainBuilder.Positions[t0],
                //    uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
                //    edgeBandLengthInches += sideLengths[edgeFace];
                //}

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
                // Always add main quad for panel side
                mainBuilder.AddQuad(
                mainBuilder.Positions[b0],
                mainBuilder.Positions[b1],
                mainBuilder.Positions[t1],
                mainBuilder.Positions[t0],
                uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);

                // For visual edgebanding, add special quad when requested.
                // Do NOT add to edgeBandLengthInches here because panel totals were computed above from width/height.
                if (panelEBEdges.Contains('B') && edgeFace == 0) // Bottom edge
                {
                    specialBuilder.AddQuad(
                    mainBuilder.Positions[b0],
                    mainBuilder.Positions[b1],
                    mainBuilder.Positions[t1],
                    mainBuilder.Positions[t0],
                    uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
                }
                else if (panelEBEdges.Contains('R') && edgeFace == 1) // Right edge
                {
                    specialBuilder.AddQuad(
                    mainBuilder.Positions[b0],
                    mainBuilder.Positions[b1],
                    mainBuilder.Positions[t1],
                    mainBuilder.Positions[t0],
                    uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
                }
                else if (panelEBEdges.Contains('T') && edgeFace == 2) // Top edge
                {
                    specialBuilder.AddQuad(
                    mainBuilder.Positions[b0],
                    mainBuilder.Positions[b1],
                    mainBuilder.Positions[t1],
                    mainBuilder.Positions[t0],
                    uvBottomLeft, uvBottomRight, uvTopRight, uvTopLeft);
                }
                else if (panelEBEdges.Contains('L') && edgeFace == 3) // Left edge
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
                if (edgeFace == 0 || edgeFace == 1) // Edge(s) to show edgeband texture
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
            if (cab != null && edgeBandLengthInches > 0.0)
            {
                var ebSpeciesKey = string.IsNullOrWhiteSpace(edgebandingSpecies) ? "None" : edgebandingSpecies;
                double feet = edgeBandLengthInches / 12.0;
                if (cab.EdgeBandingLengthBySpecies.ContainsKey(ebSpeciesKey))
                    cab.EdgeBandingLengthBySpecies[ebSpeciesKey] += feet;
                else
                    cab.EdgeBandingLengthBySpecies[ebSpeciesKey] = feet;
            }
            //Debug.WriteLine($"Edgebanding for panel: {cab.Name} {edgeBandLengthInches} inches ({edgeBandLengthInches / 12.0} feet) - Material {edgebandingSpecies}");
        }
        catch
        {
            // swallow accumulation errors to keep preview resilient
        }

        // Compute normals for proper lighting
        mainBuilder.ComputeNormalsAndTangents(MeshFaces.Default);
        specialBuilder.ComputeNormalsAndTangents(MeshFaces.Default);

        // Convert to a MeshGeometry3D, freezing for performance
        var mesh = mainBuilder.ToMesh(true);
        var specialMesh = specialBuilder.ToMesh(true);

        // Create a material with texture: pass plywood rotation through to GetPlywoodSpecies
        // IMPORTANT: still use base species for textures so you don't need separate images for UP/DOWN.
        var material = GetPlywoodSpecies(panelSpecies, grainDirection, plywoodTextureRotationDegrees);
        var specialMaterial = GetEdgeBandingSpecies(edgebandingSpecies);
        if (edgebandingSpecies == "None")
        {
            specialMaterial = GetPlywoodSpecies(panelSpecies, grainDirection, plywoodTextureRotationDegrees);
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


    private static void ApplyTransform(
    Model3DGroup geometryModel,
    double translateX,
    double translateY,
    double translateZ,
    double rotateXDegrees,
    double rotateYDegrees,
    double rotateZDegrees,
    double? centerX = null,
    double? centerY = null,
    double? centerZ = null)
    {
        // Build transform sequence explicitly so callers can choose a rotation center.
        var transformGroup = new Transform3DGroup();

        if (centerX.HasValue && centerY.HasValue && centerZ.HasValue)
        {
            // 1) Translate pivot to origin
            transformGroup.Children.Add(new TranslateTransform3D(-centerX.Value, -centerY.Value, -centerZ.Value));

            // 2) Apply rotations around origin (pivot is at origin now)
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), rotateXDegrees)));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotateYDegrees)));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), rotateZDegrees)));

            // 3) Translate back to pivot and apply the requested world translation
            transformGroup.Children.Add(new TranslateTransform3D(centerX.Value + translateX, centerY.Value + translateY, centerZ.Value + translateZ));
        }
        else
        {
            // Backwards-compatible: previous behavior was translate then rotate around origin.
            transformGroup.Children.Add(new TranslateTransform3D(translateX, translateY, translateZ));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), rotateXDegrees)));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotateYDegrees)));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), rotateZDegrees)));
        }

        geometryModel.Transform = transformGroup;
    }


    private static DiffuseMaterial GetPlywoodSpecies(string? panelSpecies, string? grainDirection, double rotationDegrees = 0)
    {
        // Provide defaults if null or empty
        panelSpecies ??= "Prefinished Ply";
        grainDirection ??= "Horizontal";

        if (string.IsNullOrWhiteSpace(panelSpecies))
            panelSpecies = "Prefinished Ply";
        if (string.IsNullOrWhiteSpace(grainDirection))
            grainDirection = "Horizontal";
        if (panelSpecies == "PFP 1/4") panelSpecies = "Prefinished Ply"; // Use the same texture for 1/4" and 3/4" prefinished ply

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

            // Apply rotation about the brush center using RelativeTransform.
            // RelativeTransform uses 0..1 coordinates, so center = (0.5, 0.5).
            if (Math.Abs(rotationDegrees) > 1e-6)
            {
                brush.RelativeTransform = new RotateTransform(rotationDegrees, 0.5, 0.5);
            }

            return new DiffuseMaterial(brush);
        }
        catch
        {
            // Fallback to solid color
            return new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(200, 200, 200)));
        }
    }

    private static DiffuseMaterial GetEdgeBandingSpecies(string? species)
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
        catch
        {
            // Fallback to solid color
            return new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(139, 69, 19)));
        }
    }


    private static void AddFrontPartRow(
        BaseCabinetModel cab,
        string type,
        double height,
        double width,
        string? species,
        string? grainDirection)
    {
        // CabinetNumber/CabinetName are assigned later by the list view-model.
        cab.FrontParts.Add(new FrontPartRow(
            CabinetNumber: 0,
            CabinetName: "",
            Type: type,
            Height: height,
            Width: width,
            Species: species ?? "",
            GrainDirection: grainDirection ?? ""));
    }

    private static void AddFrontPartRow(
        UpperCabinetModel cab,
        string type,
        double height,
        double width,
        string? species,
        string? grainDirection)
    {
        cab.FrontParts.Add(new FrontPartRow(
            CabinetNumber: 0,
            CabinetName: "",
            Type: type,
            Height: height,
            Width: width,
            Species: species ?? "",
            GrainDirection: grainDirection ?? ""));
    }


    private static void AddDrawerBoxRow(
        BaseCabinetModel cab, 
        string type, 
        double height, 
        double width, 
        double length)
    {
        cab.DrawerBoxes.Add(new DrawerBoxRow(
            CabinetNumber: 0,
            CabinetName: "",
            Type: type,
            Height: height,
            Width: width,
            Length: length));
    }




    // Add this helper below the other private static helpers (e.g. below ApplyTransform) in the same file.
    private static List<Point3D> FilletPolygon(List<Point3D> polygonPoints, double radius, int segments)
    {
        // No-op for trivial requests
        if (radius <= double.Epsilon || segments < 1 || polygonPoints == null || polygonPoints.Count < 3)
            return [.. polygonPoints!];

        var result = new List<Point3D>();
        int n = polygonPoints.Count;

        for (int i = 0; i < n; i++)
        {
            Point3D prev3 = polygonPoints[(i - 1 + n) % n];
            Point3D curr3 = polygonPoints[i];
            Point3D next3 = polygonPoints[(i + 1) % n];

            // Work in 2D (XY plane). Use Z of current for produced points.
            var currZ = curr3.Z;
            var prev = new Vector(prev3.X - curr3.X, prev3.Y - curr3.Y); // from corner toward prev
            var next = new Vector(next3.X - curr3.X, next3.Y - curr3.Y); // from corner toward next

            double lenPrev = prev.Length;
            double lenNext = next.Length;

            // Degenerate edges -> keep corner
            if (lenPrev < 1e-8 || lenNext < 1e-8)
            {
                result.Add(curr3);
                continue;
            }

            prev.Normalize();
            next.Normalize();

            // Angle between the two edge directions (in radians)
            double dot = Math.Max(-1.0, Math.Min(1.0, (prev.X * next.X + prev.Y * next.Y)));
            double angle = Math.Acos(dot);

            // If angle ~ 0 (collinear) or ~pi (straight/180), nothing to fillet
            if (angle < 1e-4 || Math.PI - angle < 1e-4)
            {
                result.Add(curr3);
                continue;
            }

            // distance along each edge to tangent point
            double tangentDist = radius / Math.Tan(angle / 2.0);

            // cannot exceed available edge segment length
            double maxAllowed = Math.Min(lenPrev, lenNext) - 1e-6;
            if (tangentDist > maxAllowed) tangentDist = Math.Max(0.0, maxAllowed);

            if (tangentDist <= 1e-6)
            {
                result.Add(curr3);
                continue;
            }

            // Tangent points (in XY)
            var t1 = new Point(curr3.X + prev.X * tangentDist, curr3.Y + prev.Y * tangentDist); // along prev edge
            var t2 = new Point(curr3.X + next.X * tangentDist, curr3.Y + next.Y * tangentDist); // along next edge

            // bisector direction (prev + next). When opposite directions (straight line) bisector is zero and already handled above.
            var bis = new Vector(prev.X + next.X, prev.Y + next.Y);
            double bisLen = bis.Length;
            if (bisLen < 1e-8)
            {
                // fallback: just emit corner
                result.Add(curr3);
                continue;
            }
            bis.Normalize();

            // center distance along bisector from corner
            double centerDist = radius / Math.Sin(angle / 2.0);
            var center = new Point(curr3.X + bis.X * centerDist, curr3.Y + bis.Y * centerDist);

            // start/end angles for arc
            double startAng = Math.Atan2(t1.Y - center.Y, t1.X - center.X);
            double endAng = Math.Atan2(t2.Y - center.Y, t2.X - center.X);

            // Determine sweep direction so arc goes inside the corner.
            // cross < 0 => clockwise turn from prev->next (concave vs convex depends on polygon orientation),
            // we adapt sweep to follow the smaller arc that lies between t1 and t2 toward the polygon interior.
            double cross = prev.X * next.Y - prev.Y * next.X;
            double sweep = endAng - startAng;

            if (cross < 0)
            {
                // prefer clockwise sweep
                if (sweep > 0) sweep -= 2.0 * Math.PI;
            }
            else
            {
                // prefer counter-clockwise sweep
                if (sweep < 0) sweep += 2.0 * Math.PI;
            }

            // Add tangent start
            result.Add(new Point3D(t1.X, t1.Y, currZ));

            // Add intermediate arc points (evenly spaced by angle)
            for (int s = 1; s <= segments; s++)
            {
                double t = (double)s / (segments + 1);
                double ang = startAng + sweep * t;
                double x = center.X + radius * Math.Cos(ang);
                double y = center.Y + radius * Math.Sin(ang);
                result.Add(new Point3D(x, y, currZ));
            }

            // Add tangent end
            result.Add(new Point3D(t2.X, t2.Y, currZ));
        }

        return result;
    }

}
