namespace ResgateIO.Service
{
    public interface IAccessHandler: IResourceHandler
    {
        void Access(IAccessRequest request);
    }
}