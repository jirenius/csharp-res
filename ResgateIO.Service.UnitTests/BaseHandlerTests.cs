using System;
using System.Threading.Tasks;
using Xunit;

namespace ResgateIO.Service.UnitTests
{
    public class BaseHandlerTests
    {
        #region EnabledHandlers
        class EnabledHandlers_EmptyClass_IsNone_Class : BaseHandler { }
        [Fact]
        public void EnabledHandlers_EmptyClass_IsNone()
        {
            var handler = new EnabledHandlers_EmptyClass_IsNone_Class();
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        class EnabledHandlers_AccessHandler_IsAccess_Class : BaseHandler
        {
            public void Access(IAccessRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_AccessHandler_IsAccess()
        {
            var handler = new EnabledHandlers_AccessHandler_IsAccess_Class();
            Assert.Equal(HandlerTypes.Access, handler.EnabledHandlers);
        }

        class EnabledHandlers_GetHandler_IsGet_Class : BaseHandler
        {
            public void Get(IGetRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_GetHandler_IsGet()
        {
            var handler = new EnabledHandlers_GetHandler_IsGet_Class();
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
        }

        class EnabledHandlers_CallHandler_IsCall_Class : BaseHandler
        {
            public void Call(ICallRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_CallHandler_IsCall()
        {
            var handler = new EnabledHandlers_CallHandler_IsCall_Class();
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        class EnabledHandlers_AuthHandler_IsAuth_Class : BaseHandler
        {
            public void Auth(IAuthRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_AuthHandler_IsAuth()
        {
            var handler = new EnabledHandlers_AuthHandler_IsAuth_Class();
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

#pragma warning disable 0618
        class EnabledHandlers_NewHandler_IsNew_Class : BaseHandler
        {
            public void New(INewRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_NewHandler_IsNew()
        {
            var handler = new EnabledHandlers_NewHandler_IsNew_Class();
            Assert.Equal(HandlerTypes.New, handler.EnabledHandlers);
        }
#pragma warning restore 0618

        #region ApplyHandler
        class ApplyHandler_ApplyChangeHandler_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public void ApplyChange(IResourceContext rc, ChangeEventArgs ev) { Called++; }
        }
        [Fact]
        public async Task ApplyHandler_ApplyChangeHandler_IsCalled()
        {
            var handler = new ApplyHandler_ApplyChangeHandler_IsCalled_Class();
            await handler.Apply(null, new ChangeEventArgs(null));
            Assert.Equal(1, handler.Called);
        }

        class ApplyHandler_ApplyChangeAsyncHandler_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public async Task ApplyChange(IResourceContext rc, ChangeEventArgs ev) { await Task.Run(() => Called++); }
        }
        [Fact]
        public async Task ApplyHandler_ApplyChangeAsyncHandler_IsCalled()
        {
            var handler = new ApplyHandler_ApplyChangeAsyncHandler_IsCalled_Class();
            await handler.Apply(null, new ChangeEventArgs(null));
            Assert.Equal(1, handler.Called);
        }

        class ApplyHandler_ApplyAddHandler_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public void ApplyAdd(IResourceContext rc, AddEventArgs ev) { Called++; }
        }
        [Fact]
        public async Task ApplyHandler_ApplyAddHandler_IsCalled()
        {
            var handler = new ApplyHandler_ApplyAddHandler_IsCalled_Class();
            await handler.Apply(null, new AddEventArgs(null, 0));
            Assert.Equal(1, handler.Called);
        }

        class ApplyHandler_ApplyAddAsyncHandler_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public async Task ApplyAdd(IResourceContext rc, AddEventArgs ev) { await Task.Run(() => Called++); }
        }
        [Fact]
        public async Task ApplyHandler_ApplyAddAsyncHandler_IsCalled()
        {
            var handler = new ApplyHandler_ApplyAddAsyncHandler_IsCalled_Class();
            await handler.Apply(null, new AddEventArgs(null, 0));
            Assert.Equal(1, handler.Called);
        }

        class ApplyHandler_ApplyRemoveHandler_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public void ApplyRemove(IResourceContext rc, RemoveEventArgs ev) { Called++; }
        }
        [Fact]
        public async Task ApplyHandler_ApplyRemoveHandler_IsCalled()
        {
            var handler = new ApplyHandler_ApplyRemoveHandler_IsCalled_Class();
            await handler.Apply(null, new RemoveEventArgs(0));
            Assert.Equal(1, handler.Called);
        }

        class ApplyHandler_ApplyRemoveAsyncHandler_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public async Task ApplyRemove(IResourceContext rc, RemoveEventArgs ev) { await Task.Run(() => Called++); }
        }
        [Fact]
        public async Task ApplyHandler_ApplyRemoveAsyncHandler_IsCalled()
        {
            var handler = new ApplyHandler_ApplyRemoveAsyncHandler_IsCalled_Class();
            await handler.Apply(null, new RemoveEventArgs(0));
            Assert.Equal(1, handler.Called);
        }

        class ApplyHandler_ApplyCreateHandler_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public void ApplyCreate(IResourceContext rc, CreateEventArgs ev) { Called++; }
        }
        [Fact]
        public async Task ApplyHandler_ApplyCreateHandler_IsCalled()
        {
            var handler = new ApplyHandler_ApplyCreateHandler_IsCalled_Class();
            await handler.Apply(null, new CreateEventArgs(null));
            Assert.Equal(1, handler.Called);
        }

        class ApplyHandler_ApplyCreateAsyncHandler_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public async Task ApplyCreate(IResourceContext rc, CreateEventArgs ev) { await Task.Run(() => Called++); }
        }
        [Fact]
        public async Task ApplyHandler_ApplyCreateAsyncHandler_IsCalled()
        {
            var handler = new ApplyHandler_ApplyCreateAsyncHandler_IsCalled_Class();
            await handler.Apply(null, new CreateEventArgs(null));
            Assert.Equal(1, handler.Called);
        }

        class ApplyHandler_ApplyDeleteHandler_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public void ApplyDelete(IResourceContext rc, DeleteEventArgs ev) { Called++; }
        }
        [Fact]
        public async Task ApplyHandler_ApplyDeleteHandler_IsCalled()
        {
            var handler = new ApplyHandler_ApplyDeleteHandler_IsCalled_Class();
            await handler.Apply(null, new DeleteEventArgs());
            Assert.Equal(1, handler.Called);
        }

        class ApplyHandler_ApplyDeleteAsyncHandler_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public async Task ApplyDelete(IResourceContext rc, DeleteEventArgs ev) { await Task.Run(() => Called++); }
        }
        [Fact]
        public async Task ApplyHandler_ApplyDeleteAsyncHandler_IsCalled()
        {
            var handler = new ApplyHandler_ApplyDeleteAsyncHandler_IsCalled_Class();
            await handler.Apply(null, new DeleteEventArgs());
            Assert.Equal(1, handler.Called);
        }

        class ApplyHandler_ApplyCustomHandler_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public void ApplyCustom(IResourceContext rc, CustomEventArgs ev) { Called++; }
        }
        [Fact]
        public async Task ApplyHandler_ApplyCustomHandler_IsCalled()
        {
            var handler = new ApplyHandler_ApplyCustomHandler_IsCalled_Class();
            await handler.Apply(null, new CustomEventArgs("foo", null));
            Assert.Equal(1, handler.Called);
        }

        class ApplyHandler_ApplyCustomAsyncHandler_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public async Task ApplyCustom(IResourceContext rc, CustomEventArgs ev) { await Task.Run(() => Called++); }
        }
        [Fact]
        public async Task ApplyHandler_ApplyCustomAsyncHandler_IsCalled()
        {
            var handler = new ApplyHandler_ApplyCustomAsyncHandler_IsCalled_Class();
            await handler.Apply(null, new CustomEventArgs("foo", null));
            Assert.Equal(1, handler.Called);
        }
        #endregion

        class EnabledHandlers_CallMethodWithoutAttribute_IsCall_Class : BaseHandler
        {
            public void Foo(ICallRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_CallMethodWithoutAttribute_IsCall()
        {
            var handler = new EnabledHandlers_CallMethodWithoutAttribute_IsCall_Class();
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        class EnabledHandlers_CallNewMethodWithoutAttribute_IsCall_Class : BaseHandler
        {
            public void New(ICallRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_CallNewMethodWithoutAttribute_IsCall()
        {
            var handler = new EnabledHandlers_CallNewMethodWithoutAttribute_IsCall_Class();
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        class EnabledHandlers_CallMethodWithAttribute_IsCall_Class : BaseHandler
        {
            [CallMethod("foo")]
            public void Foo(ICallRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_CallMethodWithAttribute_IsCall()
        {
            var handler = new EnabledHandlers_CallMethodWithAttribute_IsCall_Class();
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        class EnabledHandlers_CallNewMethodWithAttribute_IsCall_Class : BaseHandler
        {
            [CallMethod("new")]
            public void New(ICallRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_CallNewMethodWithAttribute_IsCall()
        {
            var handler = new EnabledHandlers_CallNewMethodWithAttribute_IsCall_Class();
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        class EnabledHandlers_CallMethodWithEmptyAttribute_IsCall_Class : BaseHandler
        {
            [CallMethod()]
            public void Foo(ICallRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_CallMethodWithEmptyAttribute_IsCall()
        {
            var handler = new EnabledHandlers_CallMethodWithEmptyAttribute_IsCall_Class();
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        class EnabledHandlers_CallMethodWithIgnoreAttribute_IsNone_Class : BaseHandler
        {
            [CallMethod(Ignore = true)]
            public void Foo(ICallRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_CallMethodWithIgnoreAttribute_IsNone()
        {
            var handler = new EnabledHandlers_CallMethodWithIgnoreAttribute_IsNone_Class();
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }

        class EnabledHandlers_AuthMethodWithoutAttribute_IsAuth_Class : BaseHandler
        {
            public void Foo(IAuthRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_AuthMethodWithoutAttribute_IsAuth()
        {
            var handler = new EnabledHandlers_AuthMethodWithoutAttribute_IsAuth_Class();
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        class EnabledHandlers_AuthMethodWithAttribute_IsAuth_Class : BaseHandler
        {
            [AuthMethod("foo")]
            public void Foo(IAuthRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_AuthMethodWithAttribute_IsAuth()
        {
            var handler = new EnabledHandlers_AuthMethodWithAttribute_IsAuth_Class();
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        class EnabledHandlers_AuthMethodWithEmptyAttribute_IsAuth_Class : BaseHandler
        {
            [AuthMethod()]
            public void Foo(IAuthRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_AuthMethodWithEmptyAttribute_IsAuth()
        {
            var handler = new EnabledHandlers_AuthMethodWithEmptyAttribute_IsAuth_Class();
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        class EnabledHandlers_AuthMethodWithIgnoreAttribute_IsNone_Class : BaseHandler
        {
            [AuthMethod(Ignore = true)]
            public void Foo(IAuthRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_AuthMethodWithIgnoreAttribute_IsNone()
        {
            var handler = new EnabledHandlers_AuthMethodWithIgnoreAttribute_IsNone_Class();
            Assert.Equal(HandlerTypes.None, handler.EnabledHandlers);
        }
        #endregion

        #region CallMethod
        class CallMethod_MethodWithNoAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public void Foo(ICallRequest r) { Called++; }
        }
        [Fact]
        public async Task CallMethod_MethodWithNoAttribute_IsCalled()
        {
            var handler = new CallMethod_MethodWithNoAttribute_IsCalled_Class();
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "foo" });
            Assert.Equal(1, handler.Called);
        }

        class CallMethod_MethodWithAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            [CallMethod("bar")]
            public void Foo(ICallRequest r) { Called++; }
        }
        [Fact]
        public async Task CallMethod_MethodWithAttribute_IsCalled()
        {
            var handler = new CallMethod_MethodWithAttribute_IsCalled_Class();
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "bar" });
            Assert.Equal(1, handler.Called);
        }

        class CallMethod_MethodWithEmptyAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            [CallMethod]
            public void Foo(ICallRequest r) { Called++; }
        }
        [Fact]
        public async Task CallMethod_MethodWithEmptyAttribute_IsCalled()
        {
            var handler = new CallMethod_MethodWithEmptyAttribute_IsCalled_Class();
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "foo" });
            Assert.Equal(1, handler.Called);
        }

        class CallMethod_MethodWithIgnoreAttribute_IsNotCalled_Class : BaseHandler
        {
            public int Called = 0;
            [CallMethod(Ignore = true)]
            public void Foo(ICallRequest r) { Called++; }
        }
        [Fact]
        public async Task CallMethod_MethodWithIgnoreAttribute_IsNotCalled()
        {
            var handler = new CallMethod_MethodWithIgnoreAttribute_IsNotCalled_Class();
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "foo" });
            Assert.Equal(0, handler.Called);
        }

        class CallMethod_MethodWithNullAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            [CallMethod(null)]
            public void Foo(ICallRequest r) { Called++; }
        }
        [Fact]
        public async Task CallMethod_MethodWithNullAttribute_IsCalled()
        {
            var handler = new CallMethod_MethodWithNullAttribute_IsCalled_Class();
            await handler.Handle(new MockRequest { Type = RequestType.Call, Method = "foo" });
            Assert.Equal(1, handler.Called);
        }

        class CallMethod_MethodWithInvalidAttribute_ThrowsArgumentException_Class : BaseHandler
        {
            [CallMethod("invalid.foo")]
            public void Foo(ICallRequest r) { }
        }
        [Fact]
        public void CallMethod_MethodWithInvalidAttribute_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new CallMethod_MethodWithInvalidAttribute_ThrowsArgumentException_Class());
        }

        class CallMethod_MethodWithEmptyStringAttribute_ThrowsArgumentException_Class : BaseHandler
        {
            [CallMethod("")]
            public void Foo(ICallRequest r) { }
        }
        [Fact]
        public void CallMethod_MethodWithEmptyStringAttribute_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new CallMethod_MethodWithEmptyStringAttribute_ThrowsArgumentException_Class());
        }

        class CallMethod_MethodWithInvalidClassMethodName_ThrowsArgumentException_Class : BaseHandler
        {
            public void Foo‿Bar(ICallRequest r) { }
        }
        [Fact]
        public void CallMethod_MethodWithInvalidClassMethodName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new CallMethod_MethodWithInvalidClassMethodName_ThrowsArgumentException_Class());
        }

        class CallMethod_MethodWithMismatchingSignature_ThrowsInvalidOperationException_Class : BaseHandler
        {
            [CallMethod]
            public void Foo(IGetRequest r) { }
        }
        [Fact]
        public void CallMethod_MethodWithMismatchingSignature_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new CallMethod_MethodWithMismatchingSignature_ThrowsInvalidOperationException_Class());
        }

        class CallMethod_DuplicateMethods_ThrowsInvalidOperationException_Class : BaseHandler
        {
            public void Foo(ICallRequest r) { }
            [CallMethod("foo")]
            public void Bar(ICallRequest r) { }
        }
        [Fact]
        public void CallMethod_DuplicateMethods_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new CallMethod_DuplicateMethods_ThrowsInvalidOperationException_Class());
        }
        #endregion

        #region AuthMethod
        class AuthMethod_MethodWithNoAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            public void Foo(IAuthRequest r) { Called++; }
        }
        [Fact]
        public async Task AuthMethod_MethodWithNoAttribute_IsCalled()
        {
            var handler = new AuthMethod_MethodWithNoAttribute_IsCalled_Class();
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "foo" });
            Assert.Equal(1, handler.Called);
        }

        class AuthMethod_MethodWithAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            [AuthMethod("bar")]
            public void Foo(IAuthRequest r) { Called++; }
        }
        [Fact]
        public async Task AuthMethod_MethodWithAttribute_IsCalled()
        {
            var handler = new AuthMethod_MethodWithAttribute_IsCalled_Class();
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "bar" });
            Assert.Equal(1, handler.Called);
        }

        class AuthMethod_MethodWithEmptyAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            [AuthMethod]
            public void Foo(IAuthRequest r) { Called++; }
        }
        [Fact]
        public async Task AuthMethod_MethodWithEmptyAttribute_IsCalled()
        {
            var handler = new AuthMethod_MethodWithEmptyAttribute_IsCalled_Class();
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "foo" });
            Assert.Equal(1, handler.Called);
        }

        class AuthMethod_MethodWithIgnoreAttribute_IsNotCalled_Class : BaseHandler
        {
            public int Called = 0;
            [AuthMethod(Ignore = true)]
            public void Foo(IAuthRequest r) { Called++; }
        }
        [Fact]
        public async Task AuthMethod_MethodWithIgnoreAttribute_IsNotCalled()
        {
            var handler = new AuthMethod_MethodWithIgnoreAttribute_IsNotCalled_Class();
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "foo" });
            Assert.Equal(0, handler.Called);
        }

        class AuthMethod_MethodWithNullAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            [AuthMethod(null)]
            public void Foo(IAuthRequest r) { Called++; }
        }
        [Fact]
        public async Task AuthMethod_MethodWithNullAttribute_IsCalled()
        {
            var handler = new AuthMethod_MethodWithNullAttribute_IsCalled_Class();
            await handler.Handle(new MockRequest { Type = RequestType.Auth, Method = "foo" });
            Assert.Equal(1, handler.Called);
        }

        class AuthMethod_MethodWithInvalidAttribute_ThrowsArgumentException_Class : BaseHandler
        {
            [AuthMethod("invalid.foo")]
            public void Foo(IAuthRequest r) { }
        }
        [Fact]
        public void AuthMethod_MethodWithInvalidAttribute_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new AuthMethod_MethodWithInvalidAttribute_ThrowsArgumentException_Class());
        }

        class AuthMethod_MethodWithEmptyStringAttribute_ThrowsArgumentException_Class : BaseHandler
        {
            [AuthMethod("")]
            public void Foo(IAuthRequest r) { }
        }
        [Fact]
        public void AuthMethod_MethodWithEmptyStringAttribute_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new AuthMethod_MethodWithEmptyStringAttribute_ThrowsArgumentException_Class());
        }

        class AuthMethod_MethodWithInvalidClassMethodName_ThrowsArgumentException_Class : BaseHandler
        {
            public void Foo‿Bar(IAuthRequest r) { }
        }
        [Fact]
        public void AuthMethod_MethodWithInvalidClassMethodName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new AuthMethod_MethodWithInvalidClassMethodName_ThrowsArgumentException_Class());
        }

        class AuthMethod_MethodWithMismatchingSignature_ThrowsInvalidOperationException_Class : BaseHandler
        {
            [AuthMethod]
            public void Foo(IGetRequest r) { }
        }
        [Fact]
        public void AuthMethod_MethodWithMismatchingSignature_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new AuthMethod_MethodWithMismatchingSignature_ThrowsInvalidOperationException_Class());
        }

        class AuthMethod_DuplicateMethods_ThrowsInvalidOperationException_Class : BaseHandler
        {
            public void Foo(IAuthRequest r) { }
            [AuthMethod("foo")]
            public void Bar(IAuthRequest r) { }
        }
        [Fact]
        public void AuthMethod_DuplicateMethods_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new AuthMethod_DuplicateMethods_ThrowsInvalidOperationException_Class());
        }
        #endregion

        #region OnRegister
        class OnRegister_ServiceAndPattern_IsExposed_Class : BaseHandler { }
        [Fact]
        public void OnRegister_ServiceAndPattern_IsExposed()
        {
            var handler = new OnRegister_ServiceAndPattern_IsExposed_Class();
            var service = new ResService("");
            Assert.Null(handler.Service);
            Assert.Null(handler.FullPattern);
            handler.OnRegister(service, "test.model");
            Assert.Equal(service, handler.Service);
            Assert.Equal("test.model", handler.FullPattern);
        }
        #endregion
    }
}
