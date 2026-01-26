using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private readonly IPriceBreakdownService _priceBreakdownService;

        private const int PlaceOrderTabIndex = 4;

        private static readonly HttpClient s_httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };

        private const string JobOrdersBaseUrl = "https://corlanecabinetry.com/joborders/";
        private static readonly Uri s_uploadJobUri = new(new Uri(JobOrdersBaseUrl), "upload-job.php");

        // NOTE: per your requirement, this is intentionally baked-in.
        // Replace with a strong random value and keep it consistent with the PHP script.
        private const string UploadApiKey = "corlanejobupload";

        private const string CustomPricingMessage = "CUSTOM MATERIAL - AUTOMATIC PRICING NOT AVAILABLE";

        private CancellationTokenSource? _networkCts;

        public PlaceOrderViewModel()
        {
            // empty constructor for design-time support
        }

        public PlaceOrderViewModel(
            ICabinetService cabinetService,
            MainWindowViewModel mainVm,
            DefaultSettingsService defaults,
            IPriceBreakdownService priceBreakdownService)
        {
            _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
            _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
            _defaults = defaults;
            _priceBreakdownService = priceBreakdownService ?? throw new ArgumentNullException(nameof(priceBreakdownService));

            OrderedAtLocal = _cabinetService.OrderedAtLocal;
            CompanyName = _defaults.CompanyName;
            ContactName = _defaults.ContactName;
            PhoneNumber = _defaults.PhoneNumber;
            EMail = _defaults.EMail;
            Street = _defaults.Street;
            City = _defaults.City;
            ZipCode = _defaults.ZipCode;

            ValidateAllProperties();

            if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
            {
                cc.CollectionChanged += Cabinets_CollectionChanged;
            }

            _mainVm.PropertyChanged += MainVm_PropertyChanged;

            InitializeNetworkMonitoring();

            if (_mainVm.SelectedTabIndex == PlaceOrderTabIndex)
            {
                CalculatePrices();
            }
        }

        // --- Pricing display helpers ---

        public string QuotedPriceText => HasCustomOrUnknownMaterialsInJob()
            ? CustomPricingMessage
            : FormattedTotal;

        private bool HasCustomOrUnknownMaterialsInJob()
        {
            if (_cabinetService?.Cabinets == null)
            {
                return false;
            }

            // Use the same “available species” lists the UI uses.
            // (They’re duplicated across VMs, but values are consistent.)
            var cabSpecies = new HashSet<string>(new BaseCabinetViewModel().ListCabSpecies, StringComparer.OrdinalIgnoreCase);
            var ebSpecies = new HashSet<string>(new BaseCabinetViewModel().ListEBSpecies, StringComparer.OrdinalIgnoreCase);

            foreach (var cab in _cabinetService.Cabinets)
            {
                // Cabinet species
                if (IsCustomOrUnknown(cab.Species, cabSpecies))
                    return true;

                // Edgebanding species
                if (IsCustomOrUnknown(cab.EBSpecies, ebSpecies))
                    return true;

                // Door species (only exists on base/upper)
                switch (cab)
                {
                    case BaseCabinetModel b:
                        if (IsCustomOrUnknown(b.DoorSpecies, cabSpecies))
                            return true;
                        break;

                    case UpperCabinetModel u:
                        if (IsCustomOrUnknown(u.DoorSpecies, cabSpecies))
                            return true;
                        break;
                }
            }

            return false;

            static bool IsCustomOrUnknown(string? value, HashSet<string> allowed)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    // Treat blank as unknown (per requirement: “not equal to any available species”)
                    return true;
                }

                var v = value.Trim();

                if (string.Equals(v, "Custom", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return !allowed.Contains(v);
            }
        }

        private void TrySaveDefaults()
        {
            if (_defaults == null)
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await _defaults.SaveAsync().ConfigureAwait(false);
                }
                catch
                {
                    // best-effort: defaults saving must never break the UI
                }
            });
        }

        public ObservableCollection<MaterialTotal> MaterialTotals { get; } = new ObservableCollection<MaterialTotal>();

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? CompanyName { get; set; }
        partial void OnCompanyNameChanged(string? oldValue, string? newValue)
        {
            if (_defaults != null)
            {
                _defaults.CompanyName = newValue;
                TrySaveDefaults();
            }

            if (CompanyName == "Corlane!")
            {
                _mainVm.Notify2("😊 Hello, Corlane team!", Brushes.BlueViolet, 4000);
                _mainVm.IsAdmin = true;
            }
            else
            {
                _mainVm.IsAdmin = false;
            }
        }

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? ContactName { get; set; }
        partial void OnContactNameChanged(string? oldValue, string? newValue)
        {
            if (_defaults != null)
            {
                _defaults.ContactName = newValue;
                TrySaveDefaults();
            }
        }

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? PhoneNumber { get; set; }
        partial void OnPhoneNumberChanged(string? oldValue, string? newValue)
        {
            if (_defaults != null)
            {
                _defaults.PhoneNumber = newValue;
                TrySaveDefaults();
            }
        }

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? EMail { get; set; }
        partial void OnEMailChanged(string? oldValue, string? newValue)
        {
            if (_defaults != null)
            {
                _defaults.EMail = newValue;
                TrySaveDefaults();
            }
        }

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? Street { get; set; }
        partial void OnStreetChanged(string? oldValue, string? newValue)
        {
            if (_defaults != null)
            {
                _defaults.Street = newValue;
                TrySaveDefaults();
            }
        }

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? City { get; set; }
        partial void OnCityChanged(string? oldValue, string? newValue)
        {
            if (_defaults != null)
            {
                _defaults.City = newValue;
                TrySaveDefaults();
            }
        }

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? ZipCode { get; set; }
        partial void OnZipCodeChanged(string? oldValue, string? newValue)
        {
            if (_defaults != null)
            {
                _defaults.ZipCode = newValue;
                TrySaveDefaults();
            }
        }

        [RelayCommand]
        private async Task PlaceOrder()
        {
            var conf = MessageBox.Show("At this point, you will be prompted to save the job, then the order will be sent to Corlane.\n\nPlease ensure all information is correct before proceeding.\n\nDo you wish to proceed?", "Place Order", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (conf != MessageBoxResult.Yes) return;

            // Snapshot a current quote at the moment the user clicks Place Order.
            CalculatePrices();

            var dialog = new SaveFileDialog
            {
                Filter = "Corlane Cabinet Order Form Files (*.cor)|*.cor",
                DefaultExt = "cor",
                FileName = string.IsNullOrWhiteSpace(_mainVm.CurrentJobName) ? "Untitled Job" : _mainVm.CurrentJobName
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                if (_defaults != null)
                {
                    try
                    {
                        await _defaults.SaveAsync();
                    }
                    catch
                    {
                        // ignore
                    }
                }

                var customer = new JobCustomerInfo
                {
                    CompanyName = CompanyName,
                    ContactName = ContactName,
                    PhoneNumber = PhoneNumber,
                    EMail = EMail,
                    Street = Street,
                    City = City,
                    ZipCode = ZipCode
                };

                var orderedAt = DateTime.Now;

                _cabinetService.OrderedAtLocal = orderedAt;
                OrderedAtLocal = orderedAt;

                await _cabinetService.SaveAsync(dialog.FileName, customer, TotalPrice);

                try
                {
                    await UploadJobToWebsiteAsync(dialog.FileName);
                    _mainVm.Notify2("Order placed. Job saved and sent to Corlane. Thank you!", Brushes.Green, 5000);

                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        OrderStatusBackground = new SolidColorBrush(Color.FromRgb(146, 250, 153));
                        OrderStatusText = $"Job ordered on {orderedAt:d}";
                    });
                }
                catch (Exception ex)
                {
                    _mainVm.Notify2($"Order saved, but upload failed: {ex.Message}", Brushes.OrangeRed, 6000);
                }

                _mainVm.CurrentJobName = Path.GetFileNameWithoutExtension(dialog.FileName);
                _mainVm.IsModified = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error placing order: {ex.Message}", "Error");
            }
        }

        [ObservableProperty]
        private DateTime? orderedAtLocal;

        partial void OnOrderedAtLocalChanged(DateTime? oldValue, DateTime? newValue)
        {
            UpdateOrderStatusUi(newValue);
        }

        private void UpdateOrderStatusUi(DateTime? orderedAt)
        {
            if (orderedAt.HasValue)
            {
                OrderStatusBackground = new SolidColorBrush(Color.FromRgb(146, 250, 153));
                OrderStatusText = $"Job ordered on {orderedAt.Value:d}";
                return;
            }

            OrderStatusBackground = new SolidColorBrush(Color.FromRgb(255, 88, 113));
            OrderStatusText = "NOT ORDERED";
        }

        private static async Task UploadJobToWebsiteAsync(string jobFilePath)
        {
            if (!File.Exists(jobFilePath))
            {
                throw new FileNotFoundException("Job file not found.", jobFilePath);
            }

            using var form = new MultipartFormDataContent();

            var fileBytes = await File.ReadAllBytesAsync(jobFilePath).ConfigureAwait(false);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            form.Add(fileContent, "jobFile", Path.GetFileName(jobFilePath));
            form.Add(new StringContent(Path.GetFileName(jobFilePath)), "originalFileName");

            using var request = new HttpRequestMessage(HttpMethod.Post, s_uploadJobUri)
            {
                Content = form
            };

            request.Headers.Add("X-Api-Key", UploadApiKey);

            using var response = await s_httpClient.SendAsync(request).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Upload failed ({(int)response.StatusCode}): {body}");
            }
        }

        [ObservableProperty]
        public partial decimal TotalPrice { get; set; }

        public string FormattedTotal => TotalPrice.ToString("C2");

        private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_mainVm.SelectedTabIndex == PlaceOrderTabIndex)
                CalculatePrices();

            OnPropertyChanged(nameof(QuotedPriceText));
        }

        private void MainVm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.SelectedTabIndex))
            {
                if (_mainVm.SelectedTabIndex == PlaceOrderTabIndex)
                {
                    CalculatePrices();
                    OnPropertyChanged(nameof(QuotedPriceText));
                }
            }
        }

        public void CalculatePrices()
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                var totals = AggregateTotals();
                TotalPrice = UpdateMaterialTotalsAndReturnTotal(totals.materials, totals.edgebanding);
                OnPropertyChanged(nameof(FormattedTotal));
                OnPropertyChanged(nameof(QuotedPriceText));
            });
        }

        private (Dictionary<string, double> materials, Dictionary<string, double> edgebanding) AggregateTotals()
        {
            var aggMaterials = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var aggEdgebanding = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            // Configure this however you want (settings, UI, default, etc.)

            foreach (var cab in _cabinetService.Cabinets)
            {
                try
                {
                    string upperCabExtraEbSpecies = GetMatchingEdgebandingSpecies(cab.Species); // sets species extra banding on bottom of upper cabinet end panels

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

                    // Pricing-only adjustment: + (2 * Depth) per Upper Cabinet (goes to a chosen EB species)
                    if (cab is UpperCabinetModel)
                    {
                        var depthIn = ConvertDimension.FractionToDouble(cab.Depth);
                        var extraFeetTimesQty = ((2.0 * depthIn) / 12.0) * cab.Qty;

                        if (extraFeetTimesQty > 0 && !string.IsNullOrWhiteSpace(upperCabExtraEbSpecies) &&
                            !string.Equals(upperCabExtraEbSpecies, "None", StringComparison.OrdinalIgnoreCase))
                        {
                            if (aggEdgebanding.ContainsKey(upperCabExtraEbSpecies))
                                aggEdgebanding[upperCabExtraEbSpecies] += extraFeetTimesQty;
                            else
                                aggEdgebanding[upperCabExtraEbSpecies] = extraFeetTimesQty;
                        }
                    }
                }
                catch
                {
                    // best-effort
                }
            }

            return (aggMaterials, aggEdgebanding);
        }

        private static string GetMatchingEdgebandingSpecies(string? species) // Helper to map common species/material names to edgebanding names
        {
            return species switch
            {
                null or "" => "None",

                // Match common species/material names -> edgebanding names
                string s when s.Contains("Alder", StringComparison.OrdinalIgnoreCase) => "Wood Alder",
                string s when s.Contains("Cherry", StringComparison.OrdinalIgnoreCase) => "Wood Cherry",
                string s when s.Contains("Hickory", StringComparison.OrdinalIgnoreCase) => "Wood Hickory",
                string s when s.Contains("Mahogany", StringComparison.OrdinalIgnoreCase) => "Wood Mahogany",
                string s when s.Contains("Maple", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
                string s when s.Contains("Maply Ply", StringComparison.OrdinalIgnoreCase) => "Wood Maple", // your example
                string s when s.Contains("MDF", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
                string s when s.Contains("Melamine", StringComparison.OrdinalIgnoreCase) => "PVC Custom",
                string s when s.Contains("Prefinished Ply", StringComparison.OrdinalIgnoreCase) => "PVC Hardrock Maple",
                string s when s.Contains("PFP 1/4", StringComparison.OrdinalIgnoreCase) => "None",
                string s when s.Contains("Red Oak", StringComparison.OrdinalIgnoreCase) => "Wood Red Oak",
                string s when s.Contains("Walnut", StringComparison.OrdinalIgnoreCase) => "Wood Walnut",
                string s when s.Contains("White Oak", StringComparison.OrdinalIgnoreCase) => "Wood White Oak",

                _ => "None"
            };
        }

        private decimal UpdateMaterialTotalsAndReturnTotal(Dictionary<string, double> materials, Dictionary<string, double> edgebanding)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                return Application.Current.Dispatcher.Invoke(() => UpdateMaterialTotalsAndReturnTotal(materials, edgebanding));
            }

            MaterialTotals.Clear();

            var breakdown = _priceBreakdownService.Build(materials, edgebanding);

            foreach (var line in breakdown.Lines)
            {
                MaterialTotals.Add(line);
            }

            return breakdown.Total;
        }

        [ObservableProperty]
        public partial bool IsInternetConnected { get; set; }

        [ObservableProperty]
        public partial SolidColorBrush InternetStatusBackground { get; set; } = new SolidColorBrush(Color.FromRgb(255, 88, 113));

        public string InternetStatusText => IsInternetConnected ? "CONNECTED" : "NOT CONNECTED";

        partial void OnIsInternetConnectedChanged(bool oldValue, bool newValue)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                InternetStatusBackground = newValue
                    ? new SolidColorBrush(Color.FromRgb(146, 250, 153))
                    : new SolidColorBrush(Color.FromRgb(255, 88, 113));
                OnPropertyChanged(nameof(InternetStatusText));
            });
        }

        private void InitializeNetworkMonitoring()
        {
            try
            {
                _ = ProbeInternetAsync();
                NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
                NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

                _networkCts = new CancellationTokenSource();
                _ = Task.Run(() => ProbeLoopAsync(_networkCts.Token), _networkCts.Token);
            }
            catch
            {
                // ignore
            }
        }

        private void NetworkChange_NetworkAddressChanged(object? sender, EventArgs e) => _ = ProbeInternetAsync();

        private void NetworkChange_NetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e) => _ = ProbeInternetAsync();

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
            catch { }
        }

        private async Task ProbeInternetAsync()
        {
            bool connected;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://clients3.google.com/generate_204");
                request.Headers.Add("Cache-Control", "no-cache");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await s_httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false);

                connected = response.StatusCode == HttpStatusCode.NoContent || response.IsSuccessStatusCode;
            }
            catch
            {
                connected = false;
            }

            Application.Current.Dispatcher.BeginInvoke(() => IsInternetConnected = connected);
        }

        [ObservableProperty]
        public partial string OrderStatusText { get; set; } = "NOT ORDERED";

        [ObservableProperty]
        public partial SolidColorBrush OrderStatusBackground { get; set; } = new SolidColorBrush(Color.FromRgb(255, 88, 113));
    }
}