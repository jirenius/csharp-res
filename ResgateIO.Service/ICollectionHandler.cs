using System;
using System.Collections.Generic;
using System.Text;

namespace ResgateIO.Service
{
    public interface ICollectionHandler : IResourceHandler
    {
        void Get(ICollectionRequest request);
    }
}
