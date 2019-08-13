namespace ResgateIO.Service
{
    /// <summary>
    /// Provides a base class for model based resource handler classes.
    /// </summary>
    public abstract class ModelHandler : ResourceHandler
    {   
        /// <summary>
        /// Initializes a new instance of the ModelHandler class.
        /// </summary>
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
