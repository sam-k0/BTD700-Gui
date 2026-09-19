using System;
using Btd700Ctl.Interop;

namespace Btd700Ctl;

public class Btd700Exception : Exception
{
    public Btd700Interop.Error ErrorCode { get; }

    public Btd700Exception(Btd700Interop.Error error, string? message = null)
        : base(message ?? Btd700Interop.GetErrorMessage(error))
    {
        ErrorCode = error;
    }

    public Btd700Exception(Btd700Interop.Error error, string message, Exception inner)
        : base(message, inner)
    {
        ErrorCode = error;
    }
}
