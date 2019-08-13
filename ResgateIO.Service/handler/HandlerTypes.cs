using System;

namespace ResgateIO.Service
{
    [Flags]
    public enum HandlerTypes
    {
        None        =      0,
        Access      = 0x0001,
        Get         = 0x0002,
        Call        = 0x0004,
        Auth        = 0x0008,
        ApplyChange = 0x0010,
        ApplyAdd    = 0x0020,
        ApplyRemove = 0x0040,
        ApplyCreate = 0x0080,
        ApplyDelete = 0x0100,
    }
}
