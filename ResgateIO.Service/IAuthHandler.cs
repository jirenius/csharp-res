namespace ResgateIO.Service
{
    public interface IAuthHandler: IResourceHandler
    {
        void Auth(IAuthRequest request);
    }
}