using System;
using System.Threading.Tasks;
using Xunit;

namespace ResgateIO.Service.UnitTests
{
    public class DynamicHandlerTests
    {
        [Fact]
        public void Access_WithHandler_AccessFlagSet()
        {
            var handler = new DynamicHandler().Access(r => { });
            Assert.Equal(HandlerTypes.Access, handler.EnabledHandlers);
        }

        [Fact]
        public async Task Access_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().Access(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Access });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task Access_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().Access(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Access });
            Assert.Equal(1, called);
        }

        [Fact]
        public void Get_WithHandler_GetFlagSet()
        {
            var handler = new DynamicHandler().Get(r => { });
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
        }

        [Fact]
        public void Get_WithAsyncHandler_GetFlagSet()
        {
            var handler = new DynamicHandler().Get(async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
        }

        [Fact]
        public async Task Get_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().Get(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Get });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task Get_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().Get(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Get });
            Assert.Equal(1, called);
        }

        [Fact]
        public void ModelGet_WithHandler_ModelGetFlagSet()
        {
            var handler = new DynamicHandler().ModelGet(r => { });
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
            Assert.Equal(ResourceType.Model, handler.Type);
        }

        [Fact]
        public void ModelGet_WithAsyncHandler_ModelGetFlagSet()
        {
            var handler = new DynamicHandler().ModelGet(async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
            Assert.Equal(ResourceType.Model, handler.Type);
        }

        [Fact]
        public async Task ModelGet_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ModelGet(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Get });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task ModelGet_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ModelGet(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Get });
            Assert.Equal(1, called);
        }

        [Fact]
        public void CollectionGet_WithHandler_CollectionGetFlagSet()
        {
            var handler = new DynamicHandler().CollectionGet(r => { });
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
            Assert.Equal(ResourceType.Collection, handler.Type);
        }

        [Fact]
        public void CollectionGet_WithAsyncHandler_CollectionGetFlagSet()
        {
            var handler = new DynamicHandler().CollectionGet(async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
            Assert.Equal(ResourceType.Collection, handler.Type);
        }

        [Fact]
        public async Task CollectionGet_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().CollectionGet(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Get });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task CollectionGet_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().CollectionGet(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Get });
            Assert.Equal(1, called);
        }

        [Fact]
        public void Call_WithHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().Call(r => { });
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void Call_WithAsyncHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().Call(async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public async Task Call_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().Call(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task Call_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().Call(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public void CallMethod_WithHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().CallMethod("foo", r => { });
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void CallMethod_WithAsyncHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().CallMethod("foo", async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void CallMethod_WithTwoHandlers_CallFlagSet()
        {
            var handler = new DynamicHandler().CallMethod("foo", r => { }).CallMethod("bar", r => { });
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void CallMethod_WithTwoAsyncHandlers_CallFlagSet()
        {
            var handler = new DynamicHandler().CallMethod("foo", async r => await Task.Yield()).CallMethod("bar", async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public async Task CallMethod_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().CallMethod("foo", r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task CallMethod_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().CallMethod("foo", async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task CallMethod_MethodWithoutHandler_CallsMethodNotFound()
        {
            int called = 0;
            var handler = new DynamicHandler().CallMethod("foo", r => called++);
            var mock = new MockRequest { Type = RequestType.Call, Method = "bar" };
            await handler.Handle(mock);
            Assert.Equal(0, called);
            Assert.Single(mock.Calls);
            Assert.Equal("MethodNotFound", mock.Calls[0].Method);
        }

        [Fact]
        public async Task CallMethod_WithMethodWithCallHandler_MethodIsCalled()
        {
            int methodCalled = 0;
            int callCalled = 0;
            var handler = new DynamicHandler()
                .CallMethod("foo", r => methodCalled++)
                .Call(r => callCalled++);
            var mock = new MockRequest { Type = RequestType.Call, Method = "foo" };
            await handler.Handle(mock);
            Assert.Equal(1, methodCalled);
            Assert.Equal(0, callCalled);
        }

        [Fact]
        public async Task CallMethod_WithoutMethodWithCallHandler_CallHandlerIsCalled()
        {
            int methodCalled = 0;
            int callCalled = 0;
            var handler = new DynamicHandler()
                .CallMethod("bar", r => methodCalled++)
                .Call(r => callCalled++);
            var mock = new MockRequest { Type = RequestType.Call, Method = "foo" };
            await handler.Handle(mock);
            Assert.Equal(0, methodCalled);
            Assert.Equal(1, callCalled);
        }

        [Fact]
        public void Auth_WithHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().Auth(r => { });
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void Auth_WithAsyncHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().Auth(async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public async Task Auth_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().Auth(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task Auth_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().Auth(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public void AuthMethod_WithHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().AuthMethod("foo", r => { });
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void AuthMethod_WithAsyncHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().AuthMethod("foo", async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void AuthMethod_WithTwoHandlers_AuthFlagSet()
        {
            var handler = new DynamicHandler().AuthMethod("foo", r => { }).AuthMethod("bar", r => { });
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void AuthMethod_WithTwoAsyncHandlers_AuthFlagSet()
        {
            var handler = new DynamicHandler().AuthMethod("foo", async r => await Task.Yield()).AuthMethod("bar", async r => await Task.Yield());
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public async Task AuthMethod_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().AuthMethod("foo", r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task AuthMethod_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().AuthMethod("foo", async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task AuthMethod_MethodWithoutHandler_CallsMethodNotFound()
        {
            int called = 0;
            var handler = new DynamicHandler().AuthMethod("foo", r => called++);
            var mock = new MockRequest { Type = RequestType.Auth, Method = "bar" };
            await handler.Handle(mock);
            Assert.Equal(0, called);
            Assert.Single(mock.Calls);
            Assert.Equal("MethodNotFound", mock.Calls[0].Method);
        }

        [Fact]
        public async Task New_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().New(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "new" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task New_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().New(async r => await Task.Run(() => called++));
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "new" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task New_CallHandlerWithoutNewHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().Call(r => called++);
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "new" });
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task New_CallHandler_IsNotCalled()
        {
            int newCalled = 0;
            int callCalled = 0;
            var handler = new DynamicHandler()
                .Call(r => callCalled++)
                .New(r => newCalled++);
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "new" });
            Assert.Equal(1, newCalled);
            Assert.Equal(0, callCalled);
        }

        [Fact]
        public async Task ApplyChange_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ApplyChange((r, ev) => called++);
            await handler.Apply(null, new ChangeEventArgs(null));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task ApplyChange_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ApplyChange(async (r, ev) => await Task.Run(() => called++));
            await handler.Apply(null, new ChangeEventArgs(null));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task ApplyAdd_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ApplyAdd((r, ev) => called++);
            await handler.Apply(null, new AddEventArgs(null, 0));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task ApplyAdd_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ApplyAdd(async (r, ev) => await Task.Run(() => called++));
            await handler.Apply(null, new AddEventArgs(null, 0));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task ApplyRemove_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ApplyRemove((r, ev) => called++);
            await handler.Apply(null, new RemoveEventArgs(0));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task ApplyRemove_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ApplyRemove(async (r, ev) => await Task.Run(() => called++));
            await handler.Apply(null, new RemoveEventArgs(0));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task ApplyCreate_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ApplyCreate((r, ev) => called++);
            await handler.Apply(null, new CreateEventArgs(null));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task ApplyCreate_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ApplyCreate(async (r, ev) => await Task.Run(() => called++));
            await handler.Apply(null, new CreateEventArgs(null));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task ApplyDelete_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ApplyDelete((r, ev) => called++);
            await handler.Apply(null, new DeleteEventArgs());
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task ApplyDelete_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ApplyDelete(async (r, ev) => await Task.Run(() => called++));
            await handler.Apply(null, new DeleteEventArgs());
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task ApplyCustom_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ApplyCustom((r, ev) => called++);
            await handler.Apply(null, new CustomEventArgs("foo", null));
            Assert.Equal(1, called);
        }

        [Fact]
        public async Task ApplyCustom_WithAsyncHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().ApplyCustom(async (r, ev) => await Task.Run(() => called++));
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
                .Access(r => { })
                .Get(r => { });
            Assert.Equal(HandlerTypes.Access | HandlerTypes.Get, handler.EnabledHandlers);
        }
    }
}
