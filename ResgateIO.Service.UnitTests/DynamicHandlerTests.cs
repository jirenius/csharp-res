using System;
using System.Threading.Tasks;
using Xunit;

namespace ResgateIO.Service.UnitTests
{
    public class DynamicHandlerTests
    {
        [Fact]
        public void SetAccess_WithHandler_AccessFlagSet()
        {
            var handler = new DynamicHandler().SetAccess(r => { });
            Assert.Equal(HandlerTypes.Access, handler.EnabledHandlers);
        }

        [Fact]
        public async Task SetAccess_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetAccess(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Access });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetAccess_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetAccess(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Access });
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetGet_WithHandler_GetFlagSet()
        {
            var handler = new DynamicHandler().SetGet(r => { });
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
        }

        [Fact]
        public void SetGet_WithAsyncHandler_GetFlagSet()
        {
            var handler = new DynamicHandler().SetGet(async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
        }

        [Fact]
        public async Task SetGet_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetGet(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Get });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetGet_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetGet(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Get });
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetModelGet_WithHandler_ModelGetFlagSet()
        {
            var handler = new DynamicHandler().SetModelGet(r => { });
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
            Assert.Equal(ResourceType.Model, handler.Type);
        }

        [Fact]
        public void SetModelGet_WithAsyncHandler_ModelGetFlagSet()
        {
            var handler = new DynamicHandler().SetModelGet(async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
            Assert.Equal(ResourceType.Model, handler.Type);
        }

        [Fact]
        public async Task SetModelGet_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetModelGet(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Get });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetModelGet_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetModelGet(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Get });
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetCollectionGet_WithHandler_CollectionGetFlagSet()
        {
            var handler = new DynamicHandler().SetCollectionGet(r => { });
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
            Assert.Equal(ResourceType.Collection, handler.Type);
        }

        [Fact]
        public void SetCollectionGet_WithAsyncHandler_CollectionGetFlagSet()
        {
            var handler = new DynamicHandler().SetCollectionGet(async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
            Assert.Equal(ResourceType.Collection, handler.Type);
        }

        [Fact]
        public async Task SetCollectionGet_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetCollectionGet(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Get });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetCollectionGet_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetCollectionGet(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Get });
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetCall_WithHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().SetCall(r => { });
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void SetCall_WithAsyncHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().SetCall(async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public async Task SetCall_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetCall(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetCall_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetCall(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetCallMethod_WithHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().SetCallMethod("foo", r => { });
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void SetCallMethod_WithAsyncHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().SetCallMethod("foo", async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void SetCallMethod_WithTwoHandlers_CallFlagSet()
        {
            var handler = new DynamicHandler().SetCallMethod("foo", r => { }).SetCallMethod("bar", r => { });
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void SetCallMethod_WithTwoAsyncHandlers_CallFlagSet()
        {
            var handler = new DynamicHandler().SetCallMethod("foo", async r => await Task.Yield()).SetCallMethod("bar", async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public async Task SetCallMethod_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetCallMethod("foo", r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetCallMethod_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetCallMethod("foo", async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetCallMethod_MethodWithoutHandler_CallsMethodNotFound()
        {
            int called = 0;
            var handler = new DynamicHandler().SetCallMethod("foo", r => called++);
            var mock = new MockRequest { Type = RequestType.Call, Method = "bar" };
            await handler.Handle(mock);
            Assert.Equal(0, called);
            Assert.Single(mock.Calls);
            Assert.Equal("MethodNotFound", mock.Calls[0].Method);
        }

        [Fact]
        public async Task SetCallMethod_WithMethodWithCallHandler_MethodIsCalled()
        {
            int methodCalled = 0;
            int callCalled = 0;
            var handler = new DynamicHandler()
                .SetCallMethod("foo", r => methodCalled++)
                .SetCall(r => callCalled++);
            var mock = new MockRequest { Type = RequestType.Call, Method = "foo" };
            await handler.Handle(mock);
            Assert.Equal(1, methodCalled);
            Assert.Equal(0, callCalled);
        }

        [Fact]
        public async Task SetCallMethod_WithoutMethodWithCallHandler_CallHandlerIsCalled()
        {
            int methodCalled = 0;
            int callCalled = 0;
            var handler = new DynamicHandler()
                .SetCallMethod("bar", r => methodCalled++)
                .SetCall(r => callCalled++);
            var mock = new MockRequest { Type = RequestType.Call, Method = "foo" };
            await handler.Handle(mock);
            Assert.Equal(0, methodCalled);
            Assert.Equal(1, callCalled);
        }

        [Fact]
        public void SetAuth_WithHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().SetAuth(r => { });
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAuth_WithAsyncHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().SetAuth(async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public async Task SetAuth_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetAuth(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetAuth_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetAuth(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetAuthMethod_WithHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().SetAuthMethod("foo", r => { });
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAuthMethod_WithAsyncHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().SetAuthMethod("foo", async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAuthMethod_WithTwoHandlers_AuthFlagSet()
        {
            var handler = new DynamicHandler().SetAuthMethod("foo", r => { }).SetAuthMethod("bar", r => { });
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAuthMethod_WithTwoAsyncHandlers_AuthFlagSet()
        {
            var handler = new DynamicHandler().SetAuthMethod("foo", async r => await Task.Yield()).SetAuthMethod("bar", async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public async Task SetAuthMethod_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetAuthMethod("foo", r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetAuthMethod_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetAuthMethod("foo", async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetAuthMethod_MethodWithoutHandler_CallsMethodNotFound()
        {
            int called = 0;
            var handler = new DynamicHandler().SetAuthMethod("foo", r => called++);
            var mock = new MockRequest { Type = RequestType.Auth, Method = "bar" };
            await handler.Handle(mock);
            Assert.Equal(0, called);
            Assert.Single(mock.Calls);
            Assert.Equal("MethodNotFound", mock.Calls[0].Method);
        }

        [Fact]
        public async Task SetNew_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetNew(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "new" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetNew_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetNew(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "new" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetNew_CallHandlerWithoutNewHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetCall(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "new" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetNew_CallHandler_IsNotCalled()
        {
            int newCalled = 0;
            int callCalled = 0;
            var handler = new DynamicHandler()
                .SetCall(r => callCalled++)
                .SetNew(r => newCalled++);
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "new" });
            Assert.Equal(1, newCalled);
            Assert.Equal(0, callCalled);
        }

        [Fact]
        public async Task SetApplyChange_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyChange((r, ev) => called++);
            await handler.Apply(null, new ChangeEventArgs(null));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetApplyChange_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyChange(async (r, ev) => await Task.Run(() => called++));
            await handler.Apply(null, new ChangeEventArgs(null));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetApplyAdd_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyAdd((r, ev) => called++);
            await handler.Apply(null, new AddEventArgs(null, 0));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetApplyAdd_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyAdd(async (r, ev) => await Task.Run(() => called++));
            await handler.Apply(null, new AddEventArgs(null, 0));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetApplyRemove_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyRemove((r, ev) => called++);
            await handler.Apply(null, new RemoveEventArgs(0));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetApplyRemove_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyRemove(async (r, ev) => await Task.Run(() => called++));
            await handler.Apply(null, new RemoveEventArgs(0));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetApplyCreate_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyCreate((r, ev) => called++);
            await handler.Apply(null, new CreateEventArgs(null));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetApplyCreate_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyCreate(async (r, ev) => await Task.Run(() => called++));
            await handler.Apply(null, new CreateEventArgs(null));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetApplyDelete_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyDelete((r, ev) => called++);
            await handler.Apply(null, new DeleteEventArgs());
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetApplyDelete_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyDelete(async (r, ev) => await Task.Run(() => called++));
            await handler.Apply(null, new DeleteEventArgs());
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetApplyCustom_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyCustom((r, ev) => called++);
            await handler.Apply(null, new CustomEventArgs("foo", null));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task SetApplyCustom_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyCustom(async (r, ev) => await Task.Run(() => called++));
            await handler.Apply(null, new CustomEventArgs("foo", null));
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetType_WithModel_TypeIsSet()
        {
            var handler = new DynamicHandler().SetType(ResourceType.Model);
            Assert.Equal(ResourceType.Model, handler.Type);
        }

        [Fact]
        public void EnabledHandlers_WithMultipleHandlers_FlagsAreSet()
        {
            var handler = new DynamicHandler()
                .SetAccess(r => { })
                .SetGet(r => { });
            Assert.Equal(HandlerTypes.Access | HandlerTypes.Get, handler.EnabledHandlers);
        }
    }
}
