namespace ResgateIO.Service
{
    public abstract class ModelHandler : ResourceHandler
    {   
        public ModelHandler()
            : base(ResourceType.Model)
        {
        }

        /// <summary>
        /// Method called on a model get request.
        /// </summary>
        /// <param name="request">Model get request context.</param>
        public abstract void Get(IModelRequest request);


        /// <summary>
        /// Method called on a get request.
        /// </summary>
        /// <param name="request">Get request context.</param>
        public override void Get(IGetRequest request)
        {
            Get((IModelRequest)request);
        }
    }
}
