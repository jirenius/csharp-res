namespace ResgateIO.Service
{
    public interface ICollectionHandler : IResourceHandler
    {
        void Get(ICollectionRequest request);
    }
}
