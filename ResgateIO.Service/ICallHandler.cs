namespace ResgateIO.Service
{
    public interface ICallHandler: IResourceHandler
    {
        void Call(ICallRequest request);
    }
}