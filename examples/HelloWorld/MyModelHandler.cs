using Newtonsoft.Json;
using ResgateIO.Service;
using System.Collections.Generic;

namespace HelloWorld
{
    class MyModelHandler : IModelHandler, IAccessHandler, ICallHandler
    {
        class MyModel
        {
            [JsonProperty(PropertyName = "message")]
            public string Message;
        }

        // The model we will serve as "example.mymodel" or GET /api/example/mymodel
        private readonly MyModel myModel = new MyModel { Message = "Hello, C# World!" };
        
        public void Get(IModelRequest request)
        {
            // Respond to get requests with the model
            request.Model(myModel);    
        }

        public void Access(IAccessRequest request)
        {
            // Allow everone to access this resource
            request.AccessGranted();
        }

        public void Call(ICallRequest request)
        {
            switch (request.Method)
            {
                // Handle setting of the message
                case "set":
                    var modelParams = request.ParseParams<MyModel>();

                    // Check if the Message property was changed
                    if (modelParams.Message != null && modelParams.Message != myModel.Message)
                    {
                        // Update the model
                        myModel.Message = modelParams.Message;
                        // Send a change event with updated fields
                        request.ChangeEvent(new Dictionary<string, object> { { "message", myModel.Message } });
                    }
                    request.Ok();
                    break;

                default:
                    request.MethodNotFound();
                    break;
            }
        }
    }
}