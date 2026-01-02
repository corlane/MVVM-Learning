using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels
{
    public partial class PlaceOrderViewModel : ObservableValidator
    {
        private readonly DefaultSettingsService? _defaults;
        private readonly ICabinetService _cabinetService;
        private readonly MainWindowViewModel _mainVm;

        // Adjust this to the tab index of your Place Order tab
        private const int PlaceOrderTabIndex = 4;

        // Http client used to probe reachability
        private static readonly HttpClient s_httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };

        // Cancellation for background probe loop
        private CancellationTokenSource? _networkCts;


        public PlaceOrderViewModel()
        {
            // empty constructor for design-time support
        }

        public PlaceOrderViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm, DefaultSettingsService defaults)
        {
            _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
            _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
            _defaults = defaults;

            CompanyName = _defaults.CompanyName;
            ContactName = _defaults.ContactName;
            PhoneNumber = _defaults.PhoneNumber;
            EMail = _defaults.EMail;
            Street = _defaults.Street;
            City = _defaults.City;
            ZipCode = _defaults.ZipCode;

            ValidateAllProperties();

            // React when the set of cabinets changes
            if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
            {
                cc.CollectionChanged += Cabinets_CollectionChanged;
            }

            // React when tab changes
            _mainVm.PropertyChanged += MainVm_PropertyChanged;

            // Initialize network monitoring for Internet status
            InitializeNetworkMonitoring();

            // Optionally compute on startup if already on that tab
            if (_mainVm.SelectedTabIndex == PlaceOrderTabIndex)
            {
                CalculatePrices();
            }
        }

        // Material totals collection for binding to a ListView/DataGrid
        public ObservableCollection<MaterialTotal> MaterialTotals { get; } = new ObservableCollection<MaterialTotal>();

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? CompanyName { get; set; }
        partial void OnCompanyNameChanged(string? oldValue, string? newValue)
        {
            _defaults.CompanyName = newValue;
        }
        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? ContactName { get; set; }
        partial void OnContactNameChanged(string? oldValue, string? newValue)
        {
            _defaults.ContactName = newValue;
        }
        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? PhoneNumber { get; set; }
        partial void OnPhoneNumberChanged(string? oldValue, string? newValue)
        {
            _defaults.PhoneNumber = newValue;
        }
        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? EMail { get; set; }
        partial void OnEMailChanged(string? oldValue, string? newValue)
        {
            _defaults.EMail = newValue;
        }
        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? Street { get; set; }
        partial void OnStreetChanged(string? oldValue, string? newValue)
        {
            _defaults.Street = newValue;
        }
        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? City { get; set; }
        partial void OnCityChanged(string? oldValue, string? newValue)
        {
            _defaults.City = newValue;
        }
        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? ZipCode { get; set; }
        partial void OnZipCodeChanged(string? oldValue, string? newValue)
        {
            _defaults.ZipCode = newValue;
        }


        [RelayCommand]
        private void PlaceOrder()
        {
            MessageBox.Show("Not implemented yet!", "Order", MessageBoxButton.OK, MessageBoxImage.Information);
        }



        // Total numeric price
        [ObservableProperty]
        public partial decimal TotalPrice { get; set; }

        // Human friendly formatted price for binding
        public string FormattedTotal => TotalPrice.ToString("C2");

        private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // If user is on Place Order tab, recalc immediately.
            if (_mainVm.SelectedTabIndex == PlaceOrderTabIndex)
                CalculatePrices();
        }

        private void MainVm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.SelectedTabIndex))
            {
                if (_mainVm.SelectedTabIndex == PlaceOrderTabIndex)
                {
                    CalculatePrices();
                }
            }
        }



        // Public helper — call this from UI or tests if needed.
        public void CalculatePrices()
        {
            // Ensure run on UI thread (we touch observable properties and update MaterialTotals)
            if (Application.Current?.Dispatcher == null)
            {
                // no dispatcher — fallback to direct run
                TotalPrice = ComputeTotal();
                var totals = AggregateTotals();
                UpdateMaterialTotals(totals.materials, totals.edgebanding);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TotalPrice = ComputeTotal();
                    // Notify that formatted value changed as well
                    OnPropertyChanged(nameof(FormattedTotal));

                    // Update material totals collection for UI binding
                    var totals = AggregateTotals();
                    UpdateMaterialTotals(totals.materials, totals.edgebanding);
                });
            }
        }

        private decimal ComputeTotal()
        {
            decimal total = 0m;

            // Ensure accumulators exist: run the 3D builder accumulation for each saved cabinet.
            // This is necessary because the preview flow builds and accumulates into the preview model
            // instances (often different object instances than the saved cabinets).
            try
            {
                var builder = App.ServiceProvider.GetRequiredService<Cabinet3DViewModel>();
                foreach (var c in _cabinetService.Cabinets)
                {
                    try { builder.AccumulateMaterialAndEdgeTotals(c); } catch { /* best-effort */ }
                }
            }
            catch
            {
                // ignore DI / builder errors — fallback behavior below may still run
            }

            // Use aggregated totals (safest and single-pass) to compute price
            var (materials, edgebanding) = AggregateTotals();

            // Material area pricing
            foreach (var kv in materials)
            {
                var species = kv.Key;
                var areaFt2 = kv.Value; // already multiplied by cabinet qty in aggregation
                Debug.WriteLine($"[Pricing] Species '{species}' Area: {areaFt2} ft²");
                decimal rate = GetRateForSpecies(species);
                total += rate * (decimal)areaFt2;
            }

            // Edgebanding pricing
            foreach (var kv in edgebanding)
            {
                var ebSpecies = kv.Key;
                var feet = kv.Value; // already multiplied by cabinet qty
                Debug.WriteLine($"[Pricing] Edgeband Species '{ebSpecies}' Length: {feet} ft");
                decimal ebRate = GetEdgeBandRateForSpecies(ebSpecies);
                total += ebRate * (decimal)feet;
            }

            // sheet cost based on total area
            double totalArea = materials.Values.Sum();
            int sheetCount = (int)Math.Ceiling(totalArea / 32.0);
            decimal sheetCost = sheetCount * 55; // price to cut a 4x8 sheet of plywood
            total += sheetCost;

            return Math.Round(total, 2);
        }

        // Aggregate across all cabinets (multiplying by Qty). Returns material area (ft²) and edgebanding (ft).
        private (Dictionary<string, double> materials, Dictionary<string, double> edgebanding) AggregateTotals()
        {
            var aggMaterials = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var aggEdgebanding = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            foreach (var cab in _cabinetService.Cabinets)
            {
                try
                {
                    // Materials
                    if (cab.MaterialAreaBySpecies != null)
                    {
                        foreach (var kv in cab.MaterialAreaBySpecies)
                        {
                            var species = string.IsNullOrWhiteSpace(kv.Key) ? "None" : kv.Key;
                            var areaFt2TimesQty = kv.Value * cab.Qty;
                            if (aggMaterials.ContainsKey(species))
                                aggMaterials[species] += areaFt2TimesQty;
                            else
                                aggMaterials[species] = areaFt2TimesQty;
                        }
                    }

                    // Edgebanding
                    if (cab.EdgeBandingLengthBySpecies != null)
                    {
                        foreach (var kv in cab.EdgeBandingLengthBySpecies)
                        {
                            var eb = string.IsNullOrWhiteSpace(kv.Key) ? "None" : kv.Key;
                            var feetTimesQty = kv.Value * cab.Qty;
                            if (aggEdgebanding.ContainsKey(eb))
                                aggEdgebanding[eb] += feetTimesQty;
                            else
                                aggEdgebanding[eb] = feetTimesQty;
                        }
                    }
                }
                catch
                {
                    // continue best-effort
                }
            }

            return (aggMaterials, aggEdgebanding);
        }

        // Update the ObservableCollection used for UI binding.
        // Clear + add is simple and preserves consumers that bind to the same collection instance.
        private void UpdateMaterialTotals(Dictionary<string, double> materials, Dictionary<string, double> edgebanding)
        {
            // Must run on UI thread. Caller ensures this, but double-check for safety.
            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => UpdateMaterialTotals(materials, edgebanding));
                return;
            }

            MaterialTotals.Clear();

            // Track the total sheets as reported by the per-species sheetQty calculation
            int totalSheetsTally = 0;

            // Add material area lines (converted to sheet counts)
            foreach (var kv in materials.OrderBy(k => k.Key))
            {
                var species = kv.Key;
                var qty = kv.Value; // ft²
                int sheetQty = (int)Math.Ceiling(qty / 32.0); // number of 4x8 sheets required for this species
                totalSheetsTally += sheetQty;

                var unitPricePerSheet = GetRateForSpecies(species) * 32m; // price per 4x8 sheet for that species
                MaterialTotals.Add(new MaterialTotal
                {
                    Species = species,
                    Quantity = sheetQty,
                    Unit = "Sheets (4x8)",
                    UnitPrice = unitPricePerSheet
                });
            }

            // Add edgebanding lines (ft)
            foreach (var kv in edgebanding.OrderBy(k => k.Key))
            {
                var species = kv.Key;
                var qty = kv.Value; // feet
                var unitPrice = GetEdgeBandRateForSpecies(species);
                MaterialTotals.Add(new MaterialTotal
                {
                    Species = species,
                    Quantity = qty,
                    Unit = "ft",
                    UnitPrice = unitPrice
                });
            }

            // Use the tallied sheet count (sum of sheetQty values) for the summary line
            if (totalSheetsTally > 0)
            {
                MaterialTotals.Add(new MaterialTotal
                {
                    Species = $"Sheets (4x8) x{totalSheetsTally}",
                    Quantity = totalSheetsTally,
                    Unit = "sheets",
                    UnitPrice = 55m
                });
            }
        }

        // Example price table ($ per ft^2). Replace with real rates or wire to settings/service.
        private static readonly Dictionary<string, decimal> _priceBySpecies = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Prefinished Ply", 1.40625m },
            { "Maple Ply", 2.5m },
            { "Red Oak Ply", 32m },
            { "White Oak Ply", 32m },
            { "Cherry Ply", 34m },
            { "Alder Ply", 28m },
            { "Mahogany Ply", 4.6875m },
            { "Walnut Ply", 45m },
            { "Hickory Ply", 35m },
            { "MDF", 18m },
            { "Melamine", 15m },
            { "None", 0m }
        };

        // Edgebanding rates (per linear foot). Tune these values to your pricing.
        private static readonly Dictionary<string, decimal> _edgebandRateBySpecies = new(StringComparer.OrdinalIgnoreCase)
        {
            { "PVC White", 0.65m },
            { "PVC Black", 0.65m },
            { "PVC Paint Grade", 0.65m },
            { "Wood Prefinished Maple", .65m },
            { "Wood Maple", .65m },
            { "None", 0m }
        };

        private static decimal GetRateForSpecies(string? species)
        {
            if (string.IsNullOrWhiteSpace(species)) return 0m;
            if (_priceBySpecies.TryGetValue(species, out var r)) return r;
            // fallback
            return 2.5m;
        }

        private static decimal GetEdgeBandRateForSpecies(string? species)
        {
            if (string.IsNullOrWhiteSpace(species)) return 0;
            if (_edgebandRateBySpecies.TryGetValue(species, out var r)) return r;
            return .65m; // default per-foot
        }


        // Internet connection state properties
        [ObservableProperty]
        public partial bool IsInternetConnected { get; set; }
        // Background brush for the status TextBlock (bind to this in XAML)
        [ObservableProperty]
        private SolidColorBrush internetStatusBackground = new SolidColorBrush(Color.FromRgb(255, 88, 113));
        // Computed text bound to the TextBlock
        public string InternetStatusText => IsInternetConnected ? "CONNECTED" : "NOT CONNECTED";
        partial void OnIsInternetConnectedChanged(bool oldValue, bool newValue)
        {
            // Update background and notify computed text property changed on UI thread
            if (Application.Current?.Dispatcher == null)
            {
                InternetStatusBackground = newValue
                    ? new SolidColorBrush(Color.FromRgb(146, 250, 153)) // light green when connected
                    : new SolidColorBrush(Color.FromRgb(255, 88, 113)); // red when not connected
                OnPropertyChanged(nameof(InternetStatusText));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InternetStatusBackground = newValue
                        ? new SolidColorBrush(Color.FromRgb(146, 250, 153))
                        : new SolidColorBrush(Color.FromRgb(255, 88, 113));
                    OnPropertyChanged(nameof(InternetStatusText));
                });
            }
        }
        private void InitializeNetworkMonitoring()
        {
            try
            {
                // initial quick check (use probing for real Internet reachability)
                _ = ProbeInternetAsync();

                // Subscribe to static network events to get notified of changes
                NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
                NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

                // Start a periodic background probe to catch cases where network stack reports available but no Internet
                _networkCts = new CancellationTokenSource();
                _ = Task.Run(() => ProbeLoopAsync(_networkCts.Token), _networkCts.Token);
            }
            catch
            {
                // ignore if network APIs aren't available in some environments
            }
        }
        private void NetworkChange_NetworkAddressChanged(object? sender, EventArgs e)
        {
            // do an immediate probe when network addresses change
            _ = ProbeInternetAsync();
        }
        private void NetworkChange_NetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
        {
            // do an immediate probe when availability changes
            _ = ProbeInternetAsync();
        }
        // Periodic loop that re-checks reachability in the background
        private async Task ProbeLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await ProbeInternetAsync().ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(15), token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
            catch { /* swallow background exceptions */ }
        }
        // A reliable probe: make a short HTTP GET to a lightweight URL that returns 204 when reachable.
        // Returns true only when a successful response is received.
        private async Task ProbeInternetAsync()
        {
            bool connected = false;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://clients3.google.com/generate_204");
                // Add a small header to reduce chance of redirection to captive portals, still tolerant to success statuses
                request.Headers.Add("Cache-Control", "no-cache");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await s_httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false);

                connected = response.StatusCode == HttpStatusCode.NoContent || response.IsSuccessStatusCode;
            }
            catch
            {
                connected = false;
            }

            // Marshal to UI thread to update bound property
            if (Application.Current?.Dispatcher == null)
            {
                IsInternetConnected = connected;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => IsInternetConnected = connected);
            }
        }

    }
}