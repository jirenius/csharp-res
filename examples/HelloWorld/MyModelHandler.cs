using Newtonsoft.Json;
using ResgateIO.Service;

namespace HelloWorld
{
    class MyModelHandler : IModelHandler, IAccessHandler
    {
        class MyModel
        {
            [JsonProperty(PropertyName = "message")]
            public string Message;
        }

        private readonly MyModel myModel = new MyModel { Message = "Hello, C# World!" };
        
        public void Get(IModelRequest request)
        {
            request.Model(myModel);    
        }

        public void Access(IAccessRequest request)
        {
            request.AccessGranted();
        }
    }
}