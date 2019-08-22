using System;
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
        public void SetAccess_NoHandler_AccessFlagNotSet()
        {
            var handler = new DynamicHandler().SetAccess(r => { }).SetAccess(null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAccess_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetAccess(r => called++);
            handler.Access(null);
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetGet_WithHandler_GetFlagSet()
        {
            var handler = new DynamicHandler().SetGet(r => { });
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
        }

        [Fact]
        public void SetGet_NoHandler_GetFlagNotSet()
        {
            var handler = new DynamicHandler().SetGet(r => { }).SetGet(null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        [Fact]
        public void SetGet_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetGet(r => called++);
            handler.Get(null);
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
        public void SetModelGet_NoHandler_ModelGetFlagNotSet()
        {
            var handler = new DynamicHandler().SetModelGet(r => { }).SetModelGet(null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
            Assert.Equal(ResourceType.Unknown, handler.Type);
        }

        [Fact]
        public void SetModelGet_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetModelGet(r => called++);
            handler.Get(null);
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
        public void SetCollectionGet_NoHandler_CollectionGetFlagNotSet()
        {
            var handler = new DynamicHandler().SetCollectionGet(r => { }).SetCollectionGet(null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
            Assert.Equal(ResourceType.Unknown, handler.Type);
        }

        [Fact]
        public void SetCollectionGet_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetCollectionGet(r => called++);
            handler.Get(null);
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetCall_WithHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().SetCall(r => { });
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void SetCall_NoHandler_CallFlagNotSetAndNoResponse()
        {
            var handler = new DynamicHandler().SetCall(r => { }).SetCall(null);
            var mock = new MockRequest { Method = "foo" };
            handler.Call(mock);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
            Assert.Empty(mock.Calls);
        }

        [Fact]
        public void SetCall_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetCall(r => called++);
            handler.Call(null);
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetCall_NoHandlerWithCallMethodHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().SetCallMethod("foo", r => { }).SetCall(r => { }).SetCall(null);
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void SetCallMethod_WithHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().SetCallMethod("foo", r => { });
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void SetCallMethod_NoHandlerWithoutCallHandler_CallFlagNotSet()
        {
            var handler = new DynamicHandler().SetCallMethod("foo", r => { }).SetCallMethod("foo", null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        [Fact]
        public void SetCallMethod_WithTwoHandlers_CallFlagSet()
        {
            var handler = new DynamicHandler().SetCallMethod("foo", r => { }).SetCallMethod("bar", r => { });
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void SetCallMethod_NoHandlerWithCallHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().SetCall(r => { }).SetCallMethod("foo", r => { }).SetCallMethod("foo", null);
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void SetCallMethod_NullOnUnregisteredHandlerWithoutCallHandler_CallFlagNotSet()
        {
            var handler = new DynamicHandler().SetCallMethod("foo", null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        [Fact]
        public void SetCallMethod_NullOnUnregisteredHandlerWithCallHandler_CallFlagSet()
        {
            var handler = new DynamicHandler().SetCall(r => { }).SetCallMethod("foo", null);
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        [Fact]
        public void SetCallMethod_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetCallMethod("foo", r => called++);
            handler.Call(new MockRequest { Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetCallMethod_MethodWithoutHandler_CallsMethodNotFound()
        {
            int called = 0;
            var handler = new DynamicHandler().SetCallMethod("foo", r => called++);
            var mock = new MockRequest { Method = "bar" };
            handler.Call(mock);
            Assert.Equal(0, called);
            Assert.Single(mock.Calls);
            Assert.Equal("MethodNotFound", mock.Calls[0].Method);
        }

        [Fact]
        public void SetAuth_WithHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().SetAuth(r => { });
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAuth_NoHandler_AuthFlagNotSetAndNoResponse()
        {
            var handler = new DynamicHandler().SetAuth(r => { }).SetAuth(null);
            var mock = new MockRequest { Method = "foo" };
            handler.Auth(mock);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
            Assert.Empty(mock.Calls);
        }

        [Fact]
        public void SetAuth_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetAuth(r => called++);
            handler.Auth(null);
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetAuth_NoHandlerWithAuthMethodHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().SetAuthMethod("foo", r => { }).SetAuth(r => { }).SetAuth(null);
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAuthMethod_WithHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().SetAuthMethod("foo", r => { });
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAuthMethod_NoHandlerWithoutAuthHandler_AuthFlagNotSet()
        {
            var handler = new DynamicHandler().SetAuthMethod("foo", r => { }).SetAuthMethod("foo", null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAuthMethod_WithTwoHandlers_AuthFlagSet()
        {
            var handler = new DynamicHandler().SetAuthMethod("foo", r => { }).SetAuthMethod("bar", r => { });
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAuthMethod_NoHandlerWithAuthHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().SetAuth(r => { }).SetAuthMethod("foo", r => { }).SetAuthMethod("foo", null);
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAuthMethod_NullOnUnregisteredHandlerWithoutAuthHandler_AuthFlagNotSet()
        {
            var handler = new DynamicHandler().SetAuthMethod("foo", null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAuthMethod_NullOnUnregisteredHandlerWithAuthHandler_AuthFlagSet()
        {
            var handler = new DynamicHandler().SetAuth(r => { }).SetAuthMethod("foo", null);
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        [Fact]
        public void SetAuthMethod_WithHandler_IsAuthed()
        {
            int called = 0;
            var handler = new DynamicHandler().SetAuthMethod("foo", r => called++);
            handler.Auth(new MockRequest { Method = "foo" });
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetAuthMethod_MethodWithoutHandler_CallsMethodNotFound()
        {
            int called = 0;
            var handler = new DynamicHandler().SetAuthMethod("foo", r => called++);
            var mock = new MockRequest { Method = "bar" };
            handler.Auth(mock);
            Assert.Equal(0, called);
            Assert.Single(mock.Calls);
            Assert.Equal("MethodNotFound", mock.Calls[0].Method);
        }

        [Fact]
        public void SetApplyChange_WithHandler_ApplyChangeFlagSet()
        {
            var handler = new DynamicHandler().SetApplyChange((r, c) => null);
            Assert.Equal(HandlerTypes.ApplyChange, handler.EnabledHandlers);
        }

        [Fact]
        public void SetApplyChange_NoHandler_ApplyChangeFlagNotSet()
        {
            var handler = new DynamicHandler().SetApplyChange((r, c) => null).SetApplyChange(null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        [Fact]
        public void SetApplyChange_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyChange((r, c) => { called++; return null; });
            handler.ApplyChange(null, null);
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetApplyAdd_WithHandler_ApplyAddFlagSet()
        {
            var handler = new DynamicHandler().SetApplyAdd((r, o, i) => { });
            Assert.Equal(HandlerTypes.ApplyAdd, handler.EnabledHandlers);
        }

        [Fact]
        public void SetApplyAdd_NoHandler_ApplyAddFlagNotSet()
        {
            var handler = new DynamicHandler().SetApplyAdd((r, o, i) => { }).SetApplyAdd(null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        [Fact]
        public void SetApplyAdd_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyAdd((r, o, i) => called++);
            handler.ApplyAdd(null, null, 0);
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetApplyRemove_WithHandler_ApplyRemoveFlagSet()
        {
            var handler = new DynamicHandler().SetApplyRemove((r, i) => null);
            Assert.Equal(HandlerTypes.ApplyRemove, handler.EnabledHandlers);
        }

        [Fact]
        public void SetApplyRemove_NoHandler_ApplyRemoveFlagNotSet()
        {
            var handler = new DynamicHandler().SetApplyRemove((r, i) => null).SetApplyRemove(null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        [Fact]
        public void SetApplyRemove_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyRemove((r, i) => { called++; return null; });
            handler.ApplyRemove(null, 0);
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetApplyCreate_WithHandler_ApplyCreateFlagSet()
        {
            var handler = new DynamicHandler().SetApplyCreate((r, o) => { });
            Assert.Equal(HandlerTypes.ApplyCreate, handler.EnabledHandlers);
        }

        [Fact]
        public void SetApplyCreate_NoHandler_ApplyCreateFlagNotSet()
        {
            var handler = new DynamicHandler().SetApplyCreate((r, o) => { }).SetApplyCreate(null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        [Fact]
        public void SetApplyCreate_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyCreate((r, o) => called++);
            handler.ApplyCreate(null, null);
            Assert.Equal(1, called);
        }

        [Fact]
        public void SetApplyDelete_WithHandler_ApplyDeleteFlagSet()
        {
            var handler = new DynamicHandler().SetApplyDelete(r => null);
            Assert.Equal(HandlerTypes.ApplyDelete, handler.EnabledHandlers);
        }

        [Fact]
        public void SetApplyDelete_NoHandler_ApplyDeleteFlagNotSet()
        {
            var handler = new DynamicHandler().SetApplyDelete(r => null).SetApplyDelete(null);
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        [Fact]
        public void SetApplyDelete_WithHandler_IsCalled()
        {
            int called = 0;
            var handler = new DynamicHandler().SetApplyDelete(r => { called++; return null; });
            handler.ApplyDelete(null);
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
