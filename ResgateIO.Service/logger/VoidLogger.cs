namespace ResgateIO.Service
{
    internal class VoidLogger: ILogger
    {
        public void Info(string format, params object[] args) { }
        public void Debug(string format, params object[] args) { }
        public void Error(string format, params object[] args) { }
        public void Trace(string format, params object[] args) { }
    }
}
