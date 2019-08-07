namespace ResgateIO.Service
{
    public interface IModelHandler : IResourceHandler
    {
        void Get(IModelRequest request);
    }
}
