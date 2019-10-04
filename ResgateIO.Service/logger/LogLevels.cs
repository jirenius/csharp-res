using System;

namespace ResgateIO.Service
{
    [Flags]
    public enum LogLevels
    {
        None    = 0,
        Info    = 0x0001,
        Debug   = 0x0002,
        Error   = 0x0004,
        Trace   = 0x0008,
        Default = 0x000D,
        All     = 0x000F,
    }
}
