using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Btd700Ctl.Interop;

public static class Btd700Interop
{
    #region Enums

    public enum Error
    {
        Ok = 0,
        DeviceNotFound = -1,
        DeviceNotOpen = -2,
        Hid = -3,
        InvalidArg = -4
    }

    public enum AudioMode
    {
        HighQuality = 0,
        Gaming = 1,
        Broadcast = 2
    }

    public enum TransportMode
    {
        Disconnected = 0,
        Classic = 1,
        LeAudio = 2,
        Multipoint = 3
    }

    public enum AudioFrequency
    {
        Freq44100 = 1,
        Freq48000 = 2,
        Freq96000 = 3
    }

    public enum AudioResolution
    {
        Res16Bit = 1,
        Res24Bit = 2
    }

    public enum Codec
    {
        Sbc = 0,
        AptX = 1,
        AptXAdaptive = 2,
        AptXLossless = 3,
        AptXLite = 4,
        Lc3 = 5
    }

    public enum DongleState
    {
        None = 0,
        Disconnected = 1,
        Connected = 2,
        StreamingAudio = 3,
        StreamingVoice = 4
    }

    public enum LeAudioState
    {
        None = 0,
        Disconnected = 1,
        Connected = 2,
        StreamingUnicast = 3,
        StreamingBroadcast = 4
    }

    public enum SinkMode
    {
        NotAvailable = 0,
        Classic = 1,
        LeAudio = 2,
        Dual = 3
    }

    public enum BroadcastState
    {
        OffPrivate = 0,
        OnPublic = 1
    }

    public enum BroadcastEncryption
    {
        Off = 0,
        On = 1
    }

    public enum BroadcastQuality
    {
        Standard16K = 0,
        Standard24K = 1,
        High = 2
    }

    public enum EventType
    {
        StateChanged = 0,
        AudioModeChanged = 1,
        CodecChanged = 2,
        LeAudioStateChanged = 3,
        AudioQualityChanged = 4,
        SinkTransportChanged = 5,
        GamingAvailabilityChanged = 6
    }

    #endregion

    #region Structs

    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Manufacturer;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Product;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Serial;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FirmwareVersion
    {
        public byte Major;
        public byte Minor;
        public ushort Build;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AudioConfig
    {
        public AudioMode Mode;
        public TransportMode Transport;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AudioQuality
    {
        public AudioFrequency Frequency;
        public AudioResolution Resolution;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BroadcastInfo
    {
        public BroadcastState State;
        public BroadcastEncryption Encryption;
        public BroadcastQuality Quality;
    }

    [StructLayout(LayoutKind.Sequential)]
     public struct EventData 
     { 
        public EventType Type;
        public IntPtr Data;
        public nuint DataLen; 
    }
    
    #endregion

    #region Callbacks

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
    public delegate void EventCallback( IntPtr eventPtr, IntPtr userData);

    #endregion

    #region P/Invoke

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_create")]
    public static extern Error DriverCreate(out IntPtr driver);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_destroy")]
    public static extern void DriverDestroy(IntPtr driver);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_connect")]
    public static extern Error DriverConnect(IntPtr driver);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_disconnect")]
    public static extern Error DriverDisconnect(IntPtr driver);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_is_connected")]
    public static extern int DriverIsConnected(IntPtr driver);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_device_info")]
    public static extern Error DriverDeviceInfo(IntPtr driver, out DeviceInfo info);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_firmware_version")]
    public static extern Error DriverFirmwareVersion(IntPtr driver, out FirmwareVersion version);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_state")]
    public static extern Error DriverState(IntPtr driver, out DongleState state);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_audio_config")]
    public static extern Error DriverAudioConfig(IntPtr driver, out AudioConfig config);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_supported_codecs")]
    public static extern Error DriverSupportedCodecs(IntPtr driver, out ushort codecs);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_active_codec")]
    public static extern Error DriverActiveCodec(IntPtr driver, out ushort codec);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_le_audio_state")]
    public static extern Error DriverLeAudioState(IntPtr driver, out LeAudioState state);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_audio_quality")]
    public static extern Error DriverAudioQuality(IntPtr driver, out AudioQuality quality);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_sink_transport")]
    public static extern Error DriverSinkTransport(IntPtr driver, out SinkMode mode);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_is_gaming_available")]
    public static extern Error DriverIsGamingAvailable(IntPtr driver, out int available);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_broadcast_info")]
    public static extern Error DriverBroadcastInfo(IntPtr driver, out BroadcastInfo info);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_broadcast_name")]
    public static extern Error DriverBroadcastName(IntPtr driver, StringBuilder name, nuint nameLen);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_broadcast_key")]
    public static extern Error DriverBroadcastKey(IntPtr driver, byte[] buf, nuint bufSize, out nuint outLen);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_set_audio_mode")]
    public static extern Error DriverSetAudioMode(IntPtr driver, AudioMode mode, TransportMode transport);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_set_codec_mask")]
    public static extern Error DriverSetCodecMask(IntPtr driver, ushort codecMask);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_set_codec")]
    public static extern Error DriverSetCodec(IntPtr driver, Codec codec);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_set_broadcast_info")]
    public static extern Error DriverSetBroadcastInfo(IntPtr driver, BroadcastState state, BroadcastEncryption encryption, BroadcastQuality quality);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_set_broadcast_name")]
    public static extern Error DriverSetBroadcastName(IntPtr driver, string name);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_set_broadcast_key")]
    public static extern Error DriverSetBroadcastKey(IntPtr driver, byte[] key, nuint keyLen);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_trigger_connect")]
    public static extern Error DriverTriggerConnect(IntPtr driver);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_trigger_disconnect")]
    public static extern Error DriverTriggerDisconnect(IntPtr driver);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_factory_reset")]
    public static extern Error DriverFactoryReset(IntPtr driver);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_set_event_callback")]
    public static extern Error DriverSetEventCallback(IntPtr driver, EventCallback callback, IntPtr user_data);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_poll_events")]
    public static extern Error DriverPollEvents(IntPtr driver, int timeout_ms);

    [DllImport("btd700ctl", EntryPoint = "btd700_driver_send_command")]
    public static extern Error DriverSendCommand(IntPtr driver, byte cmd, byte[] args, nuint argsLen, byte[] outBuf, nuint outBufSize, out nuint outLen);

    #endregion

    #region Helpers

    public static string GetErrorMessage(Error error) => error switch
    {
        Error.Ok => "Success",
        Error.DeviceNotFound => "Device not found",
        Error.DeviceNotOpen => "Device not open",
        Error.Hid => "HID operation failed",
        Error.InvalidArg => "Invalid argument",
        _ => $"Unknown error ({error})",
    };

    public static string BytesToString(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return "";
        int len = 0;
        for (; len < bytes.Length; len++)
            if (bytes[len] == 0) break;
        return Encoding.ASCII.GetString(bytes, 0, len);
    }

    public static string FrequencyToString(AudioFrequency freq) => freq switch
    {
        AudioFrequency.Freq44100 => "44.1 kHz",
        AudioFrequency.Freq48000 => "48 kHz",
        AudioFrequency.Freq96000 => "96 kHz",
        _ => freq.ToString(),
    };

    public static string ResolutionToString(AudioResolution res) => res switch
    {
        AudioResolution.Res16Bit => "16-bit",
        AudioResolution.Res24Bit => "24-bit",
        _ => res.ToString(),
    };

    public static string CodecToString(ushort codecMask)
    {
        var sb = new StringBuilder();
        foreach (Codec codec in Enum.GetValues<Codec>())
        {
            if ((codecMask & (1 << (int)codec)) != 0)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(codec.ToString());
            }
        }
        return sb.ToString();
    }

    #endregion
}
