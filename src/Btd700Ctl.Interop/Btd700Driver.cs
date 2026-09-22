using System;
using System.Runtime.InteropServices;
using Btd700Ctl.Interop;

namespace Btd700Ctl;

public class Btd700Driver : IDisposable
{
    private readonly IntPtr _handle;
    private readonly GCHandle _selfHandle;
    private readonly Btd700Interop.EventCallback _eventCallback;

    public event EventHandler? Connected;
    public event EventHandler? Disconnected;
    public event EventHandler<Btd700EventArgs>? EventReceived;
    public event EventHandler<DeviceInfoEventArgs>? DeviceInfoChanged;
    public event EventHandler<AudioConfigEventArgs>? AudioConfigChanged;
    public event EventHandler<FirmwareVersionEventArgs>? FirmwareVersionChanged;

    private Btd700Driver(IntPtr handle)
    {
        _handle = handle;
        _selfHandle = GCHandle.Alloc(this);
        _eventCallback = OnNativeEvent;
        var err = Btd700Interop.DriverSetEventCallback(_handle, _eventCallback, GCHandle.ToIntPtr(_selfHandle));
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to register event callback");
    }

    public static Btd700Driver Create()
    {
        var err = Btd700Interop.DriverCreate(out var handle);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to create driver");
        return new Btd700Driver(handle);
    }

    public void Connect()
    {
        var err = Btd700Interop.DriverConnect(_handle);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to connect");
        Connected?.Invoke(this, EventArgs.Empty);
    }

    public void Disconnect()
    {
        var err = Btd700Interop.DriverDisconnect(_handle);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to disconnect");
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    public bool IsConnected() => Btd700Interop.DriverIsConnected(_handle) != 0;

    public Btd700Interop.DeviceInfo QueryDeviceInfo()
    {
        var err = Btd700Interop.DriverDeviceInfo(_handle, out var info);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query device info");
        return info;
    }

    public Btd700Interop.FirmwareVersion QueryFirmwareVersion()
    {
        var err = Btd700Interop.DriverFirmwareVersion(_handle, out var version);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query firmware version");
        return version;
    }

    public string GetFirmwareVersion()
    {
        var ver = QueryFirmwareVersion();
        return $"{ver.Major}.{ver.Minor}.{ver.Build}";
    }

    public Btd700Interop.DongleState QueryState()
    {
        var err = Btd700Interop.DriverState(_handle, out var state);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query dongle state");
        return state;
    }

    public Btd700Interop.AudioConfig QueryAudioConfig()
    {
        var err = Btd700Interop.DriverAudioConfig(_handle, out var config);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query audio config");
        return config;
    }

    public Btd700Interop.AudioQuality QueryAudioQuality()
    {
        var err = Btd700Interop.DriverAudioQuality(_handle, out var quality);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query audio quality");
        return quality;
    }

    public ushort QuerySupportedCodecs()
    {
        var err = Btd700Interop.DriverSupportedCodecs(_handle, out var codecs);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query supported codecs");
        return codecs;
    }

    public ushort QueryActiveCodec()
    {
        var err = Btd700Interop.DriverActiveCodec(_handle, out var codec);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query active codec");
        return codec;
    }

    public Btd700Interop.LeAudioState QueryLeAudioState()
    {
        var err = Btd700Interop.DriverLeAudioState(_handle, out var state);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query LE Audio state");
        return state;
    }

    public Btd700Interop.SinkMode QuerySinkTransport()
    {
        var err = Btd700Interop.DriverSinkTransport(_handle, out var mode);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query sink transport");
        return mode;
    }

    public bool IsGamingAvailable()
    {
        var err = Btd700Interop.DriverIsGamingAvailable(_handle, out var available);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query gaming availability");
        return available != 0;
    }

    public Btd700Interop.BroadcastInfo QueryBroadcastInfo()
    {
        var err = Btd700Interop.DriverBroadcastInfo(_handle, out var info);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query broadcast info");
        return info;
    }

    public string QueryBroadcastName()
    {
        var sb = new System.Text.StringBuilder(256);
        var err = Btd700Interop.DriverBroadcastName(_handle, sb, (nuint)sb.Capacity);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query broadcast name");
        return sb.ToString();
    }

    public byte[] QueryBroadcastKey()
    {
        var buf = new byte[256];
        var err = Btd700Interop.DriverBroadcastKey(_handle, buf, (nuint)buf.Length, out var outLen);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to query broadcast key");

        var resultLen = Math.Min((int)outLen, buf.Length);
        var result = new byte[resultLen];
        if (resultLen > 0)
            Array.Copy(buf, result, resultLen);
        return result;
    }

    public void SetAudioMode(Btd700Interop.AudioMode mode, Btd700Interop.TransportMode transport)
    {
        var err = Btd700Interop.DriverSetAudioMode(_handle, mode, transport);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to set audio mode");
    }

    public void SetCodecMask(ushort codecMask)
    {
        var err = Btd700Interop.DriverSetCodecMask(_handle, codecMask);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to set codec mask");
    }

    public void SetCodec(Btd700Interop.Codec codec)
    {
        var err = Btd700Interop.DriverSetCodec(_handle, codec);
        System.Console.WriteLine("Set codec to "+codec.ToString());
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to set codec");
    }

    public void SetBroadcastInfo(Btd700Interop.BroadcastState state, Btd700Interop.BroadcastEncryption encryption, Btd700Interop.BroadcastQuality quality)
    {
        var err = Btd700Interop.DriverSetBroadcastInfo(_handle, state, encryption, quality);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to set broadcast info");
    }

    public void StartBroadcast(Btd700Interop.BroadcastEncryption encryption = Btd700Interop.BroadcastEncryption.Off, Btd700Interop.BroadcastQuality quality = Btd700Interop.BroadcastQuality.High)
    {
        SetBroadcastInfo(Btd700Interop.BroadcastState.OnPublic, encryption, quality);
    }

    public void StopBroadcast()
    {
        SetBroadcastInfo(Btd700Interop.BroadcastState.OffPrivate, Btd700Interop.BroadcastEncryption.Off, Btd700Interop.BroadcastQuality.Standard16K);
    }

    public void SetBroadcastName(string name)
    {
        var err = Btd700Interop.DriverSetBroadcastName(_handle, name);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to set broadcast name");
    }

    public void SetBroadcastKey(byte[] key)
    {
        var err = Btd700Interop.DriverSetBroadcastKey(_handle, key, (nuint)key.Length);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to set broadcast key");
    }

    public void TriggerConnect()
    {
        var err = Btd700Interop.DriverTriggerConnect(_handle);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to trigger connect");
    }

    public void TriggerDisconnect()
    {
        var err = Btd700Interop.DriverTriggerDisconnect(_handle);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to trigger disconnect");
    }

    public void FactoryReset()
    {
        var err = Btd700Interop.DriverFactoryReset(_handle);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to factory reset");
    }

    public byte[] SendCommand(byte cmd, byte[] args)
    {
        var outBuf = new byte[256];
        var err = Btd700Interop.DriverSendCommand(_handle, cmd, args, (nuint)args.Length, outBuf, (nuint)outBuf.Length, out var outLen);
        if (err != Btd700Interop.Error.Ok)
            throw new Btd700Exception(err, "Failed to send command");
        var result = new byte[(int)outLen];
        Array.Copy(outBuf, result, result.Length);
        return result;
    }

    private void OnNativeEvent(IntPtr eventPtr, IntPtr userData)
    {
        try
        {
            if (eventPtr == IntPtr.Zero)
                return;

            var evt = Marshal.PtrToStructure<Btd700Interop.EventData>(eventPtr);

            Console.WriteLine(
                $"Native event: type={evt.Type}, data=0x{evt.Data.ToInt64():X}, len={evt.DataLen}");

            var handler = EventReceived;

            if (handler != null)
            {
                const nuint MaxEventSize = 64 * 1024;

                if (evt.DataLen <= MaxEventSize)
                {
                    var length = checked((int)evt.DataLen);
                    var bytes = new byte[length];

                    if (length > 0 && evt.Data != IntPtr.Zero)
                    {
                        Marshal.Copy(evt.Data, bytes, 0, length);
                    }

                    handler(
                        this,
                        new Btd700EventArgs(evt.Type, bytes));
                }
            }

            switch (evt.Type)
            {
                case Btd700Interop.EventType.StateChanged:
                    Connected?.Invoke(this, EventArgs.Empty);
                    break;

                case Btd700Interop.EventType.AudioModeChanged:
                case Btd700Interop.EventType.AudioQualityChanged:
                case Btd700Interop.EventType.CodecChanged:
                case Btd700Interop.EventType.LeAudioStateChanged:
                case Btd700Interop.EventType.SinkTransportChanged:
                case Btd700Interop.EventType.GamingAvailabilityChanged:
                    // Refresh the device state on the UI thread after a native change event.
                    break;
            }
        }
        catch (Exception ex)
        {
            // Never allow managed exceptions to escape into native code.
            Console.WriteLine(
                $"Exception handling native event: {ex}");
        }
    }





    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            Btd700Interop.DriverDestroy(_handle);
        }
        if (_selfHandle.IsAllocated)
        {
            _selfHandle.Free();
        }
    }
}
