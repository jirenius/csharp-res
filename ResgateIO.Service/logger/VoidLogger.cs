namespace ResgateIO.Service
{
    internal class VoidLogger: ILogger
    {
        public void Info(string message) { }
        public void Error(string message) { }
        public void Trace(string message) { }
    }
}
