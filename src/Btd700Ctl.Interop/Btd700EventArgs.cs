using System;
using Btd700Ctl.Interop;

namespace Btd700Ctl;

public class Btd700EventArgs : EventArgs
{
    public Btd700Interop.EventType Type { get; }
    public byte[] Data { get; }
    public DateTime Timestamp { get; }

    public Btd700EventArgs(Btd700Interop.EventType type, byte[] data)
    {
        Type = type;
        Data = data;
        Timestamp = DateTime.UtcNow;
    }
}

public class DeviceInfoEventArgs : Btd700EventArgs
{
    public string Manufacturer { get; private set; } = "";
    public string Product { get; private set; } = "";
    public string Serial { get; private set; } = "";

    public DeviceInfoEventArgs(Btd700Interop.DeviceInfo info)
        : base(Btd700Interop.EventType.StateChanged, Array.Empty<byte>())
    {
        Manufacturer = info.Manufacturer;
        Product = info.Product;
        Serial = info.Serial;
    }
}

public class AudioConfigEventArgs : Btd700EventArgs
{
    public Btd700Interop.AudioMode Mode { get; }
    public Btd700Interop.TransportMode Transport { get; }

    public AudioConfigEventArgs(Btd700Interop.AudioConfig config)
        : base(Btd700Interop.EventType.AudioModeChanged, Array.Empty<byte>())
    {
        Mode = config.Mode;
        Transport = config.Transport;
    }
}

public class FirmwareVersionEventArgs : Btd700EventArgs
{
    public byte Major { get; }
    public byte Minor { get; }
    public ushort Build { get; }
    public string Version => $"{Major}.{Minor}.{Build}";

    public FirmwareVersionEventArgs(Btd700Interop.FirmwareVersion version)
        : base(Btd700Interop.EventType.AudioQualityChanged, Array.Empty<byte>())
    {
        Major = version.Major;
        Minor = version.Minor;
        Build = version.Build;
    }
}
