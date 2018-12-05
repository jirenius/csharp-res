using System;
using System.Collections.Generic;
using System.Text;

namespace ResgateIO.Service
{
    public interface IModelHandler : IResourceHandler
    {
        void Get(IModelRequest request);
    }
}
