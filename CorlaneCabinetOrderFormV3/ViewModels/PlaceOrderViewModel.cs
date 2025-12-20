using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
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

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? CompanyName { get; set; } partial void OnCompanyNameChanged(string? oldValue, string? newValue)
        {
            _defaults.CompanyName = newValue;
        }
        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? ContactName { get; set; } partial void OnContactNameChanged(string? oldValue, string? newValue)
        {
            _defaults.ContactName = newValue;
        }
        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? PhoneNumber { get; set; } partial void OnPhoneNumberChanged(string? oldValue, string? newValue)
        {
            _defaults.PhoneNumber = newValue;
        }
        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? EMail { get; set; } partial void OnEMailChanged(string? oldValue, string? newValue)
        {
            _defaults.EMail = newValue;
        }
        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? Street { get; set; } partial void OnStreetChanged(string? oldValue, string? newValue)
        {
            _defaults.Street = newValue;
        }
        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? City { get; set; } partial void OnCityChanged(string? oldValue, string? newValue)
        {
            _defaults.City = newValue;
        }
        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? ZipCode { get; set; } partial void OnZipCodeChanged(string? oldValue, string? newValue)
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
            // Ensure run on UI thread (we touch observable properties)
            if (Application.Current?.Dispatcher == null)
            {
                // no dispatcher — fallback to direct run
                TotalPrice = ComputeTotal();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TotalPrice = ComputeTotal();
                    // Notify that formatted value changed as well
                    OnPropertyChanged(nameof(FormattedTotal));
                });
            }
        }

        private decimal ComputeTotal()
        {
            decimal total = 0m;
            double totalArea= 0;
            double totalEdgeBandingLength = 0;
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

            foreach (var cab in _cabinetService.Cabinets)
            {
                try
                {
                    // Prefer accumulated per-cabinet material totals if present
                    if (cab.MaterialAreaBySpecies != null && cab.MaterialAreaBySpecies.Count > 0)
                    {
                        foreach (var kv in cab.MaterialAreaBySpecies)
                        {
                            var species = kv.Key;
                            var areaFt2 = kv.Value;
                            decimal rate = GetRateForSpecies(species);
                            total += rate * (decimal)areaFt2;
                            total = total * cab.Qty;
                            totalArea += areaFt2 * cab.Qty;
                        }

                        // Add edgebanding cost if present
                        if (cab.EdgeBandingLengthBySpecies != null && cab.EdgeBandingLengthBySpecies.Count > 0)
                        {
                            foreach (var kv in cab.EdgeBandingLengthBySpecies)
                            {
                                var ebSpecies = kv.Key;
                                var feet = kv.Value;
                                decimal ebRate = GetEdgeBandRateForSpecies(ebSpecies);
                                total += ebRate * (decimal)feet;
                                totalEdgeBandingLength += feet * cab.Qty;
                            }
                        }
                    }

                }
                catch
                {
                    // ignore individual cabinet errors — continue best-effort
                }
            }
            int sheetCount = (int)Math.Ceiling(totalArea / 32.0);
            decimal sheetCost = sheetCount * 55; // price to cut a 4x8 sheet of plywood
            total += sheetCost;

            Debug.WriteLine($"[Pricing] Total area: {totalArea} ft², Total edge length: {totalEdgeBandingLength} ft, Sheet count: {sheetCount}");

            return Math.Round(total, 2);
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
            return 0m;
        }

        private static decimal GetEdgeBandRateForSpecies(string? species)
        {
            if (string.IsNullOrWhiteSpace(species)) return 0;
            if (_edgebandRateBySpecies.TryGetValue(species, out var r)) return r;
            return 0m; // default per-foot
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