using System;

namespace ResgateIO.Service
{
    /// <summary>
    /// Delegate for handling global errors. The delegate is called when an unhandled exception is thrown when executing a request.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="req">The executing request.</param>
    /// <returns>A RES service error</returns>
    public delegate ResError ErrorHandlerDelegate(Exception exception, IRequest req);
}
