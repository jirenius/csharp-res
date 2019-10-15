//namespace ResgateIO.Service
//{
//    /// <summary>
//    /// Provides a base class for collection based resource handler classes.
//    /// </summary>
//    public abstract class CollectionHandler : BaseHandler
//    {
//        /// <summary>
//        /// Initializes a new instance of the CollectionHandler class.
//        /// </summary>
//        public CollectionHandler()
//            : base(ResourceType.Collection)
//        {
//        }

//        /// <summary>
//        /// Method called on a collection get request.
//        /// </summary>
//        /// <param name="request">Collection get request context.</param>
//        public abstract void Get(ICollectionRequest request);


//        /// <summary>
//        /// Method called on a get request.
//        /// </summary>
//        /// <param name="request">Get request context.</param>
//        public override void Get(IGetRequest request)
//        {
//            Get((ICollectionRequest)request);
//        }
//    }
//}
