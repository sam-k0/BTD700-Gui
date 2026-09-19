using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Btd700Ctl;
using Btd700Ctl.Interop;

namespace Btd700Ctl.Gui.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private Btd700Driver? _driver;

    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand ApplyAudioCommand { get; }
    public ICommand StartBroadcastCommand { get; }
    public ICommand PauseBroadcastCommand { get; }
    public ICommand StopBroadcastCommand { get; }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set { _isConnected = value; OnPropertyChanged(); }
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

    private string? _selectedAudioMode;
    public string? SelectedAudioMode
    {
        get => _selectedAudioMode;
        set { _selectedAudioMode = value; OnPropertyChanged(); }
    }

    private string? _selectedTransportMode;
    public string? SelectedTransportMode
    {
        get => _selectedTransportMode;
        set { _selectedTransportMode = value; OnPropertyChanged(); }
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

    public ObservableCollection<string> Events { get; } = new();
    public string[] AudioModeNames { get; }
    public string[] TransportModeNames { get; }
    public string[] BroadcastQualityNames { get; }
    public string[] EncryptionModeNames { get; }

    public MainViewModel()
    {
        ConnectCommand = new Command(Connect);
        DisconnectCommand = new Command(Disconnect);
        ApplyAudioCommand = new Command(ApplyAudio);
        StartBroadcastCommand = new Command(StartBroadcast);
        PauseBroadcastCommand = new Command(PauseBroadcast);
        StopBroadcastCommand = new Command(StopBroadcast);

        AudioModeNames = Enum.GetNames<Btd700Interop.AudioMode>();
        TransportModeNames = Enum.GetNames<Btd700Interop.TransportMode>();
        BroadcastQualityNames = Enum.GetNames<Btd700Interop.BroadcastQuality>();
        EncryptionModeNames = Enum.GetNames<Btd700Interop.BroadcastEncryption>();
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
            _driver.Connected += (_, _) => { IsConnected = true; StatusText = "Connected"; RefreshDeviceInfo(); RefreshAudioConfig(); };
            _driver.Disconnected += (_, _) => { IsConnected = false; StatusText = "Disconnected"; };

            _driver.Connect();
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

    public void ApplyAudio()
    {
        if (_driver == null || SelectedAudioMode == null || SelectedTransportMode == null)
            return;

        try
        {
            var mode = (Btd700Interop.AudioMode)Enum.Parse(typeof(Btd700Interop.AudioMode), SelectedAudioMode);
            var transport = (Btd700Interop.TransportMode)Enum.Parse(typeof(Btd700Interop.TransportMode), SelectedTransportMode);
            _driver.SetAudioMode(mode, transport);
            AddEvent($"Audio: {SelectedAudioMode} / {SelectedTransportMode}");
        }
        catch (Btd700Exception ex)
        {
            AddEvent($"Audio error: {ex.Message}");
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
            SelectedAudioMode = config.Mode.ToString();
            SelectedTransportMode = config.Transport.ToString();
        }
        catch (Btd700Exception) { }
    }

    private void OnEventReceived(object? sender, Btd700EventArgs e) => AddEvent($"{e.Timestamp:HH:mm:ss} [{e.Type}]");
    private void OnDeviceInfoChanged(object? sender, DeviceInfoEventArgs e) => RefreshDeviceInfo();
    private void OnAudioConfigChanged(object? sender, AudioConfigEventArgs e) => RefreshAudioConfig();
    private void OnFirmwareChanged(object? sender, FirmwareVersionEventArgs e) => FirmwareVersion = e.Version;

    private void AddEvent(string message) => Events.Add(message);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
