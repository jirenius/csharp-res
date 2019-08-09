namespace ResgateIO.Service
{
    public interface IGetHandler : IResourceHandler
    {
        void Get(IGetRequest request);
    }
}
