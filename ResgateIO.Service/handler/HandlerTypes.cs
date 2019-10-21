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
        New         = 0x0010,
    }
}
