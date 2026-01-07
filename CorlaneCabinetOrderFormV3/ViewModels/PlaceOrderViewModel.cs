using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels
{
    public partial class PlaceOrderViewModel : ObservableValidator
    {
        private readonly DefaultSettingsService? _defaults;
        private readonly ICabinetService _cabinetService;
        private readonly MainWindowViewModel _mainVm;
        private readonly IMaterialPricesService _materialPrices;

        private const int PlaceOrderTabIndex = 4;

        private static readonly HttpClient s_httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };

        private const string JobOrdersBaseUrl = "https://corlanecabinetry.com/joborders/";
        private static readonly Uri s_uploadJobUri = new(new Uri(JobOrdersBaseUrl), "upload-job.php");

        // NOTE: per your requirement, this is intentionally baked-in.
        // Replace with a strong random value and keep it consistent with the PHP script.
        private const string UploadApiKey = "corlanejobupload";

        private CancellationTokenSource? _networkCts;

        public PlaceOrderViewModel()
        {
            // empty constructor for design-time support
        }

        public PlaceOrderViewModel(
            ICabinetService cabinetService,
            MainWindowViewModel mainVm,
            DefaultSettingsService defaults,
            IMaterialPricesService materialPrices)
        {
            _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
            _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
            _defaults = defaults;
            _materialPrices = materialPrices ?? throw new ArgumentNullException(nameof(materialPrices));
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

        public ObservableCollection<MaterialTotal> MaterialTotals { get; } = new ObservableCollection<MaterialTotal>();

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? CompanyName { get; set; }
        partial void OnCompanyNameChanged(string? oldValue, string? newValue) => _defaults.CompanyName = newValue;

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? ContactName { get; set; }
        partial void OnContactNameChanged(string? oldValue, string? newValue) => _defaults.ContactName = newValue;

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? PhoneNumber { get; set; }
        partial void OnPhoneNumberChanged(string? oldValue, string? newValue) => _defaults.PhoneNumber = newValue;

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? EMail { get; set; }
        partial void OnEMailChanged(string? oldValue, string? newValue) => _defaults.EMail = newValue;

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? Street { get; set; }
        partial void OnStreetChanged(string? oldValue, string? newValue) => _defaults.Street = newValue;

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? City { get; set; }
        partial void OnCityChanged(string? oldValue, string? newValue) => _defaults.City = newValue;

        [ObservableProperty, Required, NotifyDataErrorInfo, MinLength(1)] public partial string? ZipCode { get; set; }
        partial void OnZipCodeChanged(string? oldValue, string? newValue) => _defaults.ZipCode = newValue;

        [RelayCommand]
        private async Task PlaceOrder()
        {
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

                await _cabinetService.SaveAsync(dialog.FileName, customer, TotalPrice).ConfigureAwait(false);

                try
                {
                    await UploadJobToWebsiteAsync(dialog.FileName).ConfigureAwait(false);
                    _mainVm.Notify2("Order placed. Job saved and sent to Corlane. Thank you!", Brushes.Green, 5000);

                    if (Application.Current?.Dispatcher == null)
                    {
                        OrderStatusBackground = new SolidColorBrush(Color.FromRgb(146, 250, 153));
                        OrderStatusText = $"Job ordered on {orderedAt:d}";
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            OrderStatusBackground = new SolidColorBrush(Color.FromRgb(146, 250, 153));
                            OrderStatusText = $"Job ordered on {orderedAt:d}";
                        });
                    }
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

        public void CalculatePrices()
        {
            if (Application.Current?.Dispatcher == null)
            {
                var totals = AggregateTotals();
                TotalPrice = UpdateMaterialTotalsAndReturnTotal(totals.materials, totals.edgebanding);
                OnPropertyChanged(nameof(FormattedTotal));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var totals = AggregateTotals();
                    TotalPrice = UpdateMaterialTotalsAndReturnTotal(totals.materials, totals.edgebanding);
                    OnPropertyChanged(nameof(FormattedTotal));
                });
            }
        }

        private (Dictionary<string, double> materials, Dictionary<string, double> edgebanding) AggregateTotals()
        {
            var aggMaterials = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var aggEdgebanding = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            foreach (var cab in _cabinetService.Cabinets)
            {
                try
                {
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
                }
                catch
                {
                    // best-effort
                }
            }

            return (aggMaterials, aggEdgebanding);
        }

        private decimal UpdateMaterialTotalsAndReturnTotal(Dictionary<string, double> materials, Dictionary<string, double> edgebanding)
        {
            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                return Application.Current.Dispatcher.Invoke(() => UpdateMaterialTotalsAndReturnTotal(materials, edgebanding));
            }

            MaterialTotals.Clear();

            decimal total = 0m;
            int totalSheetsTally = 0;

            foreach (var kv in materials.OrderBy(k => k.Key))
            {
                var species = kv.Key;
                var qtySqFt = kv.Value;

                var sheetAreaSqFt = GetSheetAreaSqFt(species);
                var yield = GetYield(species);

                int sheetQty = (int)Math.Ceiling((qtySqFt / yield) / sheetAreaSqFt);
                totalSheetsTally += sheetQty;

                var unitPricePerSheet = GetSheetPricePerSqFt(species) * (decimal)sheetAreaSqFt;

                var line = new MaterialTotal
                {
                    Species = species,
                    Quantity = sheetQty,
                    Unit = "Sheets",
                    UnitPrice = unitPricePerSheet,
                    SqFt = qtySqFt
                };

                MaterialTotals.Add(line);
                total += line.LineTotal;
            }

            foreach (var kv in edgebanding.OrderBy(k => k.Key))
            {
                var species = kv.Key;
                var qtyFt = kv.Value;

                var unitPrice = GetEdgeBandPricePerFt(species);

                var line = new MaterialTotal
                {
                    Species = species,
                    Quantity = qtyFt,
                    Unit = "ft",
                    UnitPrice = unitPrice
                };

                MaterialTotals.Add(line);
                total += line.LineTotal;
            }

            if (totalSheetsTally > 0)
            {
                var cnc = new MaterialTotal
                {
                    Species = "Sheets of CNC cutting",
                    Quantity = totalSheetsTally,
                    Unit = "Sheets",
                    UnitPrice = _materialPrices.CncPricePerSheet
                };

                MaterialTotals.Add(cnc);
                total += cnc.LineTotal;
            }

            return Math.Round(total, 2);
        }

        private decimal GetSheetPricePerSqFt(string? species)
        {
            if (string.IsNullOrWhiteSpace(species) || string.Equals(species, "None", StringComparison.OrdinalIgnoreCase))
            {
                return 0m;
            }

            if (_materialPrices.TryGetSheetMaterial(species, out var row))
            {
                return row.PricePerSqFt;
            }

            return 0m;
        }

        private decimal GetEdgeBandPricePerFt(string? species)
        {
            if (string.IsNullOrWhiteSpace(species) || string.Equals(species, "None", StringComparison.OrdinalIgnoreCase))
            {
                return 0m;
            }

            if (_materialPrices.TryGetEdgeBand(species, out var row))
            {
                return row.PricePerFt;
            }

            return 0m;
        }

        private double GetYield(string species)
        {
            if (_materialPrices.TryGetYield(species, out var y))
            {
                return y;
            }

            return _materialPrices.DefaultSheetYield;
        }

        private double GetSheetAreaSqFt(string species)
        {
            if (_materialPrices.TryGetSheetMaterial(species, out var row))
            {
                var areaSqIn = row.SheetWidthIn * row.SheetLengthIn;
                if (areaSqIn > 0)
                {
                    return areaSqIn / 144.0;
                }
            }

            return 32.0;
        }

        [ObservableProperty]
        public partial bool IsInternetConnected { get; set; }

        [ObservableProperty]
        private SolidColorBrush internetStatusBackground = new SolidColorBrush(Color.FromRgb(255, 88, 113));

        public string InternetStatusText => IsInternetConnected ? "CONNECTED" : "NOT CONNECTED";

        partial void OnIsInternetConnectedChanged(bool oldValue, bool newValue)
        {
            if (Application.Current?.Dispatcher == null)
            {
                InternetStatusBackground = newValue
                    ? new SolidColorBrush(Color.FromRgb(146, 250, 153))
                    : new SolidColorBrush(Color.FromRgb(255, 88, 113));
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

            if (Application.Current?.Dispatcher == null)
            {
                IsInternetConnected = connected;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => IsInternetConnected = connected);
            }
        }

        [ObservableProperty]
        private string orderStatusText = "NOT ORDERED";

        [ObservableProperty]
        private SolidColorBrush orderStatusBackground = new SolidColorBrush(Color.FromRgb(255, 88, 113));
    }
}