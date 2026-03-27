using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
namespace CorlaneCabinetOrderFormV3.ViewModels;






// THIS IS ACTUALLY THE MATERIAL PRICES TAB VIEWMODEL. RENAMING IT WOULD BREAK A LOT OF THINGS SO WHATEVER.
// THE ACTUAL PROCESS ORDER VIEWMODEL IS REALLYProcessOrderViewModel.







public partial class ProcessOrderViewModel : ObservableValidator
{
    public ProcessOrderViewModel()
    {
        // empty constructor for design-time data in XAML.
    }



    private readonly MainWindowViewModel _mainVm;

    public ProcessOrderViewModel(MainWindowViewModel mainVm)
    {
        _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
    }

    private static readonly HttpClient s_httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private const string MaterialPricesBaseUrl = "https://corlanecabinetry.com/matprices/";
    private const string MaterialPricesFileName = "material-prices.json";

    private static readonly Uri s_pricesUri = new(new Uri(MaterialPricesBaseUrl), MaterialPricesFileName);
    private static readonly Uri s_uploadUri = new(new Uri(MaterialPricesBaseUrl), "upload-material-prices.php");

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public ObservableCollection<MaterialPriceRow> SheetMaterials { get; } = new();
    public ObservableCollection<EdgeBandPriceRow> EdgeBanding { get; } = new();

    [ObservableProperty] public partial decimal CncPricePerSheet { get; set; } = MaterialDefaults.DefaultCncPricePerSheet;
    [ObservableProperty] public partial double DefaultSheetYield { get; set; } = MaterialDefaults.DefaultYield;

    [ObservableProperty]
    public partial string YieldBySpeciesJson { get; set; } = "{\n  \"PFP 1/4\": 0.65\n}";

    [ObservableProperty]
    public partial string StatusText { get; set; } = "";

    [RelayCommand]
    private async Task DownloadPrices()
    {
        _mainVm.Notify2("Downloading...", Brushes.MediumBlue);

        try
        {
            var json = await s_httpClient.GetStringAsync(s_pricesUri).ConfigureAwait(false);

            var dto = JsonSerializer.Deserialize<MaterialPricesDto>(json, s_jsonOptions);
            if (dto == null)
            {
                _mainVm.Notify2("Failed to parse JSON.", Brushes.Red);
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                SheetMaterials.Clear();
                if (dto.SheetMaterials != null)
                {
                    foreach (var s in dto.SheetMaterials)
                    {
                        SheetMaterials.Add(new MaterialPriceRow
                        {
                            Species = s.Species ?? "",
                            PricePerSqFt = s.PricePerSqFt,
                            SheetWidthIn = s.SheetWidthIn,
                            SheetLengthIn = s.SheetLengthIn
                        });
                    }
                }

                EdgeBanding.Clear();
                if (dto.EdgeBanding != null)
                {
                    foreach (var e in dto.EdgeBanding)
                    {
                        EdgeBanding.Add(new EdgeBandPriceRow
                        {
                            Species = e.Species ?? "",
                            PricePerFt = e.PricePerFt
                        });
                    }
                }

                CncPricePerSheet = dto.CncCutting?.PricePerSheet ?? MaterialDefaults.DefaultCncPricePerSheet;
                DefaultSheetYield = dto.Yields?.DefaultSheetYield ?? MaterialDefaults.DefaultYield;

                var y = dto.Yields?.YieldBySpecies ?? new Dictionary<string, double>();
                YieldBySpeciesJson = JsonSerializer.Serialize(y, s_jsonOptions);
            });

            _mainVm.Notify2("Download complete.", Brushes.Green);
        }
        catch (Exception ex)
        {
            _mainVm.Notify2($"Download failed: {ex.Message}", Brushes.Red);
        }
    }

    [RelayCommand]
    private async Task UploadPrices()
    {
        var userName = Interaction.InputBox("Admin username:", "Upload Prices", "");
        if (string.IsNullOrWhiteSpace(userName))
        {
            _mainVm.Notify2("Upload canceled.", Brushes.Red);
            return;
        }

        var password = Interaction.InputBox("Admin password:", "Upload Prices", "");
        if (string.IsNullOrWhiteSpace(password))
        {
            _mainVm.Notify2("Upload canceled.", Brushes.Red);
            return;
        }

        _mainVm.Notify2("Uploading...", Brushes.MediumBlue);

        try
        {
            Dictionary<string, double> yieldBySpecies;
            try
            {
                yieldBySpecies = JsonSerializer.Deserialize<Dictionary<string, double>>(YieldBySpeciesJson, s_jsonOptions)
                                 ?? new Dictionary<string, double>();
            }
            catch (Exception ex)
            {
                _mainVm.Notify2($"Yield JSON is invalid: {ex.Message}", Brushes.Red);
                return;
            }

            var dto = new MaterialPricesDto
            {
                SheetMaterials = SheetMaterials
                    .Select(s => new SheetMaterialPriceDto
                    {
                        Species = s.Species,
                        PricePerSqFt = s.PricePerSqFt,
                        SheetWidthIn = s.SheetWidthIn,
                        SheetLengthIn = s.SheetLengthIn
                    })
                    .ToList(),
                EdgeBanding = EdgeBanding
                    .Select(e => new EdgeBandingPriceDto
                    {
                        Species = e.Species,
                        PricePerFt = e.PricePerFt
                    })
                    .ToList(),
                CncCutting = new CncCuttingDto { PricePerSheet = CncPricePerSheet },
                Yields = new YieldsDto
                {
                    DefaultSheetYield = DefaultSheetYield,
                    YieldBySpecies = yieldBySpecies
                }
            };

            var json = JsonSerializer.Serialize(dto, s_jsonOptions);

            using var request = new HttpRequestMessage(HttpMethod.Post, s_uploadUri);
            request.Headers.Authorization = CreateBasicAuthHeader(userName, password);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await s_httpClient.SendAsync(request).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _mainVm.Notify2($"Upload failed ({(int)response.StatusCode}): {body}", Brushes.Red);
                return;
            }

            _mainVm.Notify2("Upload succeeded. material-prices.json updated.", Brushes.Green);
        }
        catch (Exception ex)
        {
            _mainVm.Notify2($"Upload failed: {ex.Message}", Brushes.Red);
        }
    }

    private static AuthenticationHeaderValue CreateBasicAuthHeader(string userName, string password)
    {
        var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));
        return new AuthenticationHeaderValue("Basic", basic);
    }

    private void SetStatusUi(string text)
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => StatusText = text);
            return;
        }

        StatusText = text;
    }
}