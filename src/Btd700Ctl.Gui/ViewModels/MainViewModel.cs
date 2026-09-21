using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Threading;
using Btd700Ctl;
using Btd700Ctl.Interop;
using CommunityToolkit.Mvvm.Input;
using static Btd700Ctl.Interop.Btd700Interop;

namespace Btd700Ctl.Gui.ViewModels;

public partial class MainViewModel : INotifyPropertyChanged
{
    private Btd700Driver? _driver;
    private bool _isApplyingAudioConfig;
    private bool _isSyncingFromDevice;
    public ICommand ToggleConnectionCommand { get; }
    public ICommand ToggleDeviceInfoPopupCommand { get; }
    public ICommand ToggleEventLogCommand { get; }
    public ICommand StartBroadcastCommand { get; }
    public ICommand PauseBroadcastCommand { get; }
    public ICommand StopBroadcastCommand { get; }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ConnectionBrush));
            OnPropertyChanged(nameof(ToggleConnectionButtonText));
        }
    }

    private bool _isDeviceInfoPopupOpen = true;
    public bool IsDeviceInfoPopupOpen
    {
        get => _isDeviceInfoPopupOpen;
        set
        {
            _isDeviceInfoPopupOpen = value;
            OnPropertyChanged();
        }
    }

    private bool _isEventLogOpen = true;
    public bool IsEventLogOpen
    {
        get => _isEventLogOpen;
        set
        {
            _isEventLogOpen = value;
            OnPropertyChanged();
        }
    }

    public IBrush ConnectionBrush => IsConnected ? Brushes.LimeGreen : Brushes.IndianRed;
    public string ToggleConnectionButtonText => IsConnected ? "Disconnect" : "Connect";
    public string DeviceInfoToggleText => IsDeviceInfoPopupOpen ? "Hide" : "Show";
    public string EventLogToggleText => IsEventLogOpen ? "Hide" : "Show";

    public void ToggleDeviceInfoPopup()
    {
        IsDeviceInfoPopupOpen = !IsDeviceInfoPopupOpen;
        OnPropertyChanged(nameof(DeviceInfoToggleText));
    }

    public void ToggleEventLog()
    {
        IsEventLogOpen = !IsEventLogOpen;
        OnPropertyChanged(nameof(EventLogToggleText));
    }

    private string _statusText = "Disconnected";
    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    private string _manufacturer = "---";
    public string Manufacturer
    {
        get => _manufacturer;
        set { _manufacturer = value; OnPropertyChanged(); }
    }

    private string _product = "---";
    public string Product
    {
        get => _product;
        set { _product = value; OnPropertyChanged(); }
    }

    private string _serial = "---";
    public string Serial
    {
        get => _serial;
        set { _serial = value; OnPropertyChanged(); }
    }

    private string _firmwareVersion = "---";
    public string FirmwareVersion
    {
        get => _firmwareVersion;
        set { _firmwareVersion = value; OnPropertyChanged(); }
    }

    private string _broadcastName = "---";
    public string BroadcastName
    {
        get => _broadcastName;
        set { _broadcastName = value; OnPropertyChanged(); }
    }

    private AudioMode _selectedAudioMode = AudioMode.HighQuality;
    public AudioMode SelectedAudioMode
    {
        get => _selectedAudioMode;
        set
        {
            if (_selectedAudioMode == value)
                return;

            _selectedAudioMode = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsStandardMode));
            OnPropertyChanged(nameof(IsGamingMode));
            OnPropertyChanged(nameof(IsBroadcastMode));
            OnPropertyChanged(nameof(IsNotBroadcastMode));

            if (_driver != null && !_isSyncingFromDevice && !_isApplyingAudioConfig)
            {
                ApplyCurrentAudioConfiguration();
            }
        }
    }

    public bool IsStandardMode =>
        SelectedAudioMode == AudioMode.HighQuality;

    public bool IsGamingMode =>
        SelectedAudioMode == AudioMode.Gaming;

    public bool IsBroadcastMode =>
        SelectedAudioMode == AudioMode.Broadcast;

    public bool IsNotBroadcastMode =>
        SelectedAudioMode != AudioMode.Broadcast;

    [RelayCommand]
    private void SelectAudioMode(string mode)
    {
        var normalizedMode = mode switch
        {
            "Standard" => nameof(AudioMode.HighQuality),
            _ => mode
        };

        if (Enum.TryParse<AudioMode>(normalizedMode, out var audioMode))
            SelectedAudioMode = audioMode;
    }

    private string? _selectedTransportMode;
    public string? SelectedTransportMode
    {
        get => _selectedTransportMode;
        set
        {
            if (_selectedTransportMode == value)
                return;

            _selectedTransportMode = value;
            OnPropertyChanged();

            if (_driver != null && !_isSyncingFromDevice && !_isApplyingAudioConfig)
            {
                ApplyCurrentAudioConfiguration();
            }
        }
    }

    private string? _selectedCodec;
    public string? SelectedCodec
    {
        get => _selectedCodec;
        set
        {
            if (_selectedCodec == value)
                return;

            _selectedCodec = value;
            OnPropertyChanged();

            if (_driver != null && !_isSyncingFromDevice && !_isApplyingAudioConfig)
            {
                ApplyCurrentAudioConfiguration();
            }
        }
    }

    private string? _selectedQuality;
    public string? SelectedQuality
    {
        get => _selectedQuality;
        set { _selectedQuality = value; OnPropertyChanged(); }
    }

    private string? _selectedEncryption;
    public string? SelectedEncryption
    {
        get => _selectedEncryption;
        set { _selectedEncryption = value; OnPropertyChanged(); }
    }

    private string? _activeCodec;
    public string? ActiveCodec
    {
        get => _activeCodec;
        private set { _activeCodec = value; OnPropertyChanged(); }
    }



    public ObservableCollection<string> Events { get; } = new();
    public string[] AudioModeNames { get; }
    public string[] TransportModeNames { get; }
    public string[] BroadcastQualityNames { get; }
    public string[] EncryptionModeNames { get; }

    private string[] _codecNames = Array.Empty<string>();
    public string[] CodecNames
    {
        get => _codecNames;
        private set
        {
            if (_codecNames.SequenceEqual(value))
                return;

            _codecNames = value;
            OnPropertyChanged();
        }
    }

    public MainViewModel()
    {
        ToggleConnectionCommand = new Command(ToggleConnection);
        ToggleDeviceInfoPopupCommand = new Command(ToggleDeviceInfoPopup);
        ToggleEventLogCommand = new Command(ToggleEventLog);
        StartBroadcastCommand = new Command(StartBroadcast);
        PauseBroadcastCommand = new Command(PauseBroadcast);
        StopBroadcastCommand = new Command(StopBroadcast);

        AudioModeNames = Enum.GetNames<Btd700Interop.AudioMode>();
        TransportModeNames = Enum.GetNames<Btd700Interop.TransportMode>();
        BroadcastQualityNames = Enum.GetNames<Btd700Interop.BroadcastQuality>();
        EncryptionModeNames = Enum.GetNames<Btd700Interop.BroadcastEncryption>();
        CodecNames = Array.Empty<string>();
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() =>{
            if(_driver == null)
            {
                Connect();
            }
        });
    }

    public void Connect()
    {
        try
        {
            _driver = Btd700Driver.Create();
            _driver.EventReceived += OnEventReceived;
            _driver.DeviceInfoChanged += OnDeviceInfoChanged;
            _driver.AudioConfigChanged += OnAudioConfigChanged;
            _driver.FirmwareVersionChanged += OnFirmwareChanged;
            _driver.Connected += (_, _) => {
                    IsConnected = true; 
                    StatusText = "Connected";
                    RefreshDeviceInfo();
                    RefreshAudioConfig(); 
                };
            _driver.Disconnected += (_, _) => { IsConnected = false; StatusText = "Disconnected"; };

            _driver.Connect();
            AddEvent("Connected");
        }
        catch (Btd700Exception ex)
        {
            StatusText = $"Error: {ex.Message} ({ex.ErrorCode})";
            AddEvent($"Connection failed: {ex.Message} ({ex.ErrorCode})");
        }
    }

    public void Disconnect()
    {
        if (_driver != null)
        {
            _driver.EventReceived -= OnEventReceived;
            _driver.DeviceInfoChanged -= OnDeviceInfoChanged;
            _driver.AudioConfigChanged -= OnAudioConfigChanged;
            _driver.FirmwareVersionChanged -= OnFirmwareChanged;
            _driver.Connected -= (_, _) => { };
            _driver.Disconnected -= (_, _) => { };
            _driver.Dispose();
            _driver = null;
        }
        IsConnected = false;
        StatusText = "Disconnected";
        AddEvent("Disconnected");
    }

    public void ToggleConnection()
    {
        if(IsConnected && _driver != null)
        {
            Disconnect();
            return;
        }
        Connect();
    }

    private void ApplyCurrentAudioConfiguration()
    {
        if (_driver == null || SelectedTransportMode == null || _isApplyingAudioConfig)
            return;

        _isApplyingAudioConfig = true;

        try
        {
            var mode = SelectedAudioMode;
            var transport = (Btd700Interop.TransportMode)Enum.Parse(typeof(Btd700Interop.TransportMode), SelectedTransportMode);
            _driver.SetAudioMode(mode, transport);

            RefreshCodecOptions();

            if (SelectedAudioMode == AudioMode.Gaming &&
                CodecNames.Contains(nameof(Btd700Interop.Codec.AptXAdaptive), StringComparer.OrdinalIgnoreCase))
            {
                SelectedCodec = nameof(Btd700Interop.Codec.AptXAdaptive);
            }

            if (!string.IsNullOrWhiteSpace(SelectedCodec) &&
                Enum.TryParse<Btd700Interop.Codec>(SelectedCodec, out var codec))
            {
                _driver.SetCodec(codec);
            }

            RefreshCodecInfo();
            AddEvent($"Audio: {SelectedAudioMode} / {SelectedTransportMode} / {SelectedCodec}");
        }
        catch (Btd700Exception ex)
        {
            AddEvent($"Audio error: {ex.Message}");
        }
        finally
        {
            _isApplyingAudioConfig = false;
        }
    }

    public void StartBroadcast()
    {
        if (_driver == null) return;
        try
        {
            var encryption = SelectedEncryption != null ?
                (Btd700Interop.BroadcastEncryption)Enum.Parse(typeof(Btd700Interop.BroadcastEncryption), SelectedEncryption) :
                Btd700Interop.BroadcastEncryption.Off;
            var quality = SelectedQuality != null ?
                (Btd700Interop.BroadcastQuality)Enum.Parse(typeof(Btd700Interop.BroadcastQuality), SelectedQuality) :
                Btd700Interop.BroadcastQuality.High;
            _driver.StartBroadcast(encryption, quality);
            AddEvent($"Broadcast started (enc:{encryption}, q:{quality})");
        }
        catch (Btd700Exception ex)
        {
            AddEvent($"Broadcast error: {ex.Message}");
        }
    }

    public void PauseBroadcast()
    {
        AddEvent("Broadcast pause not supported by hardware");
    }

    public void StopBroadcast()
    {
        if (_driver == null) return;
        try
        {
            _driver.StopBroadcast();
            AddEvent("Broadcast stopped");
        }
        catch (Btd700Exception ex)
        {
            AddEvent($"Broadcast error: {ex.Message}");
        }
    }

    private void RefreshDeviceInfo()
    {
        if (_driver == null) return;
        try
        {
            var info = _driver.QueryDeviceInfo();
            Manufacturer = info.Manufacturer;
            Product = info.Product;
            Serial = info.Serial;
            FirmwareVersion = _driver.GetFirmwareVersion();
        }
        catch (Btd700Exception) { }
    }

    private void RefreshAudioConfig()
    {
        if (_driver == null) return;
        try
        {
            var config = _driver.QueryAudioConfig();

            _isSyncingFromDevice = true;
            try
            {
                SelectedAudioMode = config.Mode;
                SelectedTransportMode = config.Transport.ToString();
            }
            finally
            {
                _isSyncingFromDevice = false;
            }

            RefreshCodecOptions();
            RefreshCodecInfo();
        }
        catch (Btd700Exception) { }
    }

    private void RefreshCodecOptions()
    {
        if (_driver == null)
        {
            CodecNames = Array.Empty<string>();
            SelectedCodec = null;
            ActiveCodec = "Unknown";
            return;
        }

        try
        {
            var supportedMask = _driver.QuerySupportedCodecs();
            var activeMask = _driver.QueryActiveCodec();

            var supported = Enum.GetValues<Btd700Interop.Codec>()
                .Where(codec => (supportedMask & (1 << (int)codec)) != 0)
                .Select(codec => codec.ToString())
                .ToArray();

            CodecNames = supported;

            var activeName = Enum.GetValues<Btd700Interop.Codec>()
                .Where(codec => (activeMask & (1 << (int)codec)) != 0)
                .Select(codec => codec.ToString())
                .FirstOrDefault();

            _isSyncingFromDevice = true;
            try
            {
                SelectedCodec = activeName ?? supported.FirstOrDefault();
            }
            finally
            {
                _isSyncingFromDevice = false;
            }

            ActiveCodec = Btd700Interop.CodecToString(activeMask);
        }
        catch (Btd700Exception)
        {
            CodecNames = Array.Empty<string>();
            SelectedCodec = null;
            ActiveCodec = "Unknown";
        }
    }

    private void RefreshCodecInfo()
    {
        if (_driver == null) return;

        try
        {
            var codecMask = _driver.QueryActiveCodec();
            ActiveCodec = Btd700Interop.CodecToString(codecMask);

            var activeName = Enum.GetValues<Btd700Interop.Codec>()
                .Where(codec => (codecMask & (1 << (int)codec)) != 0)
                .Select(codec => codec.ToString())
                .FirstOrDefault();

            if (activeName != null)
            {
                _isSyncingFromDevice = true;
                try
                {
                    SelectedCodec = activeName;
                }
                finally
                {
                    _isSyncingFromDevice = false;
                }
            }
        }
        catch (Btd700Exception)
        {
            ActiveCodec = "Unknown";
        }
    }

    private void OnEventReceived(object? sender, Btd700EventArgs e)
    {
        AddEvent($"{e.Timestamp:HH:mm:ss} [{e.Type}]");

        if (e.Type == Btd700Interop.EventType.AudioModeChanged ||
            e.Type == Btd700Interop.EventType.SinkTransportChanged ||
            e.Type == Btd700Interop.EventType.CodecChanged ||
            e.Type == Btd700Interop.EventType.GamingAvailabilityChanged)
        {
            Dispatcher.UIThread.Post(() =>
            {
                RefreshAudioConfig();
                RefreshCodecInfo();
            });
        }
    }

    private void OnDeviceInfoChanged(object? sender, DeviceInfoEventArgs e) => RefreshDeviceInfo();
    private void OnAudioConfigChanged(object? sender, AudioConfigEventArgs e) => RefreshAudioConfig();
    private void OnFirmwareChanged(object? sender, FirmwareVersionEventArgs e) => FirmwareVersion = e.Version;

    private void AddEvent(string message) => Dispatcher.UIThread.Invoke(() => Events.Add(message));

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {   
        // dispatch change to ui thread
        if (Dispatcher.UIThread.CheckAccess())
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        else
            Dispatcher.UIThread.Post(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }
}

internal class Command : ICommand
{
    private readonly Action _execute;
    public Command(Action execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }
}
