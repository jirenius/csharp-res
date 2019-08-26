using System;
using System.Collections.Generic;
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

        class EnabledHandlers_OverrideAccess_IsAccess_Class : BaseHandler
        {
            public override void Access(IAccessRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_OverrideAccess_IsAccess()
        {
            var handler = new EnabledHandlers_OverrideAccess_IsAccess_Class();
            Assert.Equal(HandlerTypes.Access, handler.EnabledHandlers);
        }

        class EnabledHandlers_OverrideGet_IsGet_Class : BaseHandler
        {
            public override void Get(IGetRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_OverrideGet_IsGet()
        {
            var handler = new EnabledHandlers_OverrideGet_IsGet_Class();
            Assert.Equal(HandlerTypes.Get, handler.EnabledHandlers);
        }

        class EnabledHandlers_OverrideCall_IsCall_Class : BaseHandler
        {
            public override void Call(ICallRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_OverrideCall_IsCall()
        {
            var handler = new EnabledHandlers_OverrideCall_IsCall_Class();
            Assert.Equal(HandlerTypes.Call, handler.EnabledHandlers);
        }

        class EnabledHandlers_OverrideAuth_IsAuth_Class : BaseHandler
        {
            public override void Auth(IAuthRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_OverrideAuth_IsAuth()
        {
            var handler = new EnabledHandlers_OverrideAuth_IsAuth_Class();
            Assert.Equal(HandlerTypes.Auth, handler.EnabledHandlers);
        }

        class EnabledHandlers_OverrideNew_IsNew_Class : BaseHandler
        {
            public override void New(INewRequest r) { }
        }
        [Fact]
        public void EnabledHandlers_OverrideNew_IsNew()
        {
            var handler = new EnabledHandlers_OverrideNew_IsNew_Class();
            Assert.Equal(HandlerTypes.New, handler.EnabledHandlers);
        }

        class EnabledHandlers_OverrideApplyChange_IsApplyChange_Class : BaseHandler
        {
            public override Dictionary<string, object> ApplyChange(IResourceContext rc, IDictionary<string, object> c) { return null; }
        }
        [Fact]
        public void EnabledHandlers_OverrideApplyChange_IsApplyChange()
        {
            var handler = new EnabledHandlers_OverrideApplyChange_IsApplyChange_Class();
            Assert.Equal(HandlerTypes.ApplyChange, handler.EnabledHandlers);
        }

        class EnabledHandlers_OverrideApplyAdd_IsApplyAdd_Class : BaseHandler
        {
            public override void ApplyAdd(IResourceContext rc, object v, int idx) { }
        }
        [Fact]
        public void EnabledHandlers_OverrideApplyAdd_IsApplyAdd()
        {
            var handler = new EnabledHandlers_OverrideApplyAdd_IsApplyAdd_Class();
            Assert.Equal(HandlerTypes.ApplyAdd, handler.EnabledHandlers);
        }

        class EnabledHandlers_OverrideApplyRemove_IsApplyRemove_Class : BaseHandler
        {
            public override object ApplyRemove(IResourceContext rc, int idx) { return null; }
        }
        [Fact]
        public void EnabledHandlers_OverrideApplyRemove_IsApplyRemove()
        {
            var handler = new EnabledHandlers_OverrideApplyRemove_IsApplyRemove_Class();
            Assert.Equal(HandlerTypes.ApplyRemove, handler.EnabledHandlers);
        }

        class EnabledHandlers_OverrideApplyCreate_IsApplyCreate_Class : BaseHandler
        {
            public override void ApplyCreate(IResourceContext rc, object data) { }
        }
        [Fact]
        public void EnabledHandlers_OverrideApplyCreate_IsApplyCreate()
        {
            var handler = new EnabledHandlers_OverrideApplyCreate_IsApplyCreate_Class();
            Assert.Equal(HandlerTypes.ApplyCreate, handler.EnabledHandlers);
        }

        class EnabledHandlers_OverrideApplyDelete_IsApplyDelete_Class : BaseHandler
        {
            public override object ApplyDelete(IResourceContext rc) { return null; }
        }
        [Fact]
        public void EnabledHandlers_OverrideApplyDelete_IsApplyDelete()
        {
            var handler = new EnabledHandlers_OverrideApplyDelete_IsApplyDelete_Class();
            Assert.Equal(HandlerTypes.ApplyDelete, handler.EnabledHandlers);
        }

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
        public void CallMethod_MethodWithNoAttribute_IsCalled()
        {
            var handler = new CallMethod_MethodWithNoAttribute_IsCalled_Class();
            handler.Call(new MockRequest { Method = "foo" });
            Assert.Equal(1, handler.Called);
        }

        class CallMethod_MethodWithAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            [CallMethod("bar")]
            public void Foo(ICallRequest r) { Called++; }
        }
        [Fact]
        public void CallMethod_MethodWithAttribute_IsCalled()
        {
            var handler = new CallMethod_MethodWithAttribute_IsCalled_Class();
            handler.Call(new MockRequest { Method = "bar" });
            Assert.Equal(1, handler.Called);
        }

        class CallMethod_MethodWithEmptyAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            [CallMethod]
            public void Foo(ICallRequest r) { Called++; }
        }
        [Fact]
        public void CallMethod_MethodWithEmptyAttribute_IsCalled()
        {
            var handler = new CallMethod_MethodWithEmptyAttribute_IsCalled_Class();
            handler.Call(new MockRequest { Method = "foo" });
            Assert.Equal(1, handler.Called);
        }

        class CallMethod_MethodWithIgnoreAttribute_IsNotCalled_Class : BaseHandler
        {
            public int Called = 0;
            [CallMethod(Ignore = true)]
            public void Foo(ICallRequest r) { Called++; }
        }
        [Fact]
        public void CallMethod_MethodWithIgnoreAttribute_IsNotCalled()
        {
            var handler = new CallMethod_MethodWithIgnoreAttribute_IsNotCalled_Class();
            handler.Call(new MockRequest { Method = "foo" });
            Assert.Equal(0, handler.Called);
        }

        class CallMethod_MethodWithNullAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            [CallMethod(null)]
            public void Foo(ICallRequest r) { Called++; }
        }
        [Fact]
        public void CallMethod_MethodWithNullAttribute_IsCalled()
        {
            var handler = new CallMethod_MethodWithNullAttribute_IsCalled_Class();
            handler.Call(new MockRequest { Method = "foo" });
            Assert.Equal(1, handler.Called);
        }

        class CallMethod_MethodWithInvalidAttribute_ThrowsInvalidOperationException_Class : BaseHandler
        {
            [CallMethod("invalid.foo")]
            public void Foo(ICallRequest r) { }
        }
        [Fact]
        public void CallMethod_MethodWithInvalidAttribute_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new CallMethod_MethodWithInvalidAttribute_ThrowsInvalidOperationException_Class());
        }

        class CallMethod_MethodWithEmptyStringAttribute_ThrowsInvalidOperationException_Class : BaseHandler
        {
            [CallMethod("")]
            public void Foo(ICallRequest r) { }
        }
        [Fact]
        public void CallMethod_MethodWithEmptyStringAttribute_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new CallMethod_MethodWithEmptyStringAttribute_ThrowsInvalidOperationException_Class());
        }

        class CallMethod_MethodWithInvalidClassMethodName_ThrowsInvalidOperationException_Class : BaseHandler
        {
            public void Foo‿Bar(ICallRequest r) { }
        }
        [Fact]
        public void CallMethod_MethodWithInvalidClassMethodName_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new CallMethod_MethodWithInvalidClassMethodName_ThrowsInvalidOperationException_Class());
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

        class CallMethod_AttributeOnCallHandler_ThrowsInvalidOperationException_Class : BaseHandler
        {
            [CallMethod]
            public override void Call(ICallRequest r) { }
        }
        [Fact]
        public void CallMethod_AttributeOnCallHandler_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new CallMethod_AttributeOnCallHandler_ThrowsInvalidOperationException_Class());
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
        public void AuthMethod_MethodWithNoAttribute_IsCalled()
        {
            var handler = new AuthMethod_MethodWithNoAttribute_IsCalled_Class();
            handler.Auth(new MockRequest { Method = "foo" });
            Assert.Equal(1, handler.Called);
        }

        class AuthMethod_MethodWithAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            [AuthMethod("bar")]
            public void Foo(IAuthRequest r) { Called++; }
        }
        [Fact]
        public void AuthMethod_MethodWithAttribute_IsCalled()
        {
            var handler = new AuthMethod_MethodWithAttribute_IsCalled_Class();
            handler.Auth(new MockRequest { Method = "bar" });
            Assert.Equal(1, handler.Called);
        }

        class AuthMethod_MethodWithEmptyAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            [AuthMethod]
            public void Foo(IAuthRequest r) { Called++; }
        }
        [Fact]
        public void AuthMethod_MethodWithEmptyAttribute_IsCalled()
        {
            var handler = new AuthMethod_MethodWithEmptyAttribute_IsCalled_Class();
            handler.Auth(new MockRequest { Method = "foo" });
            Assert.Equal(1, handler.Called);
        }

        class AuthMethod_MethodWithIgnoreAttribute_IsNotCalled_Class : BaseHandler
        {
            public int Called = 0;
            [AuthMethod(Ignore = true)]
            public void Foo(IAuthRequest r) { Called++; }
        }
        [Fact]
        public void AuthMethod_MethodWithIgnoreAttribute_IsNotCalled()
        {
            var handler = new AuthMethod_MethodWithIgnoreAttribute_IsNotCalled_Class();
            handler.Auth(new MockRequest { Method = "foo" });
            Assert.Equal(0, handler.Called);
        }

        class AuthMethod_MethodWithNullAttribute_IsCalled_Class : BaseHandler
        {
            public int Called = 0;
            [AuthMethod(null)]
            public void Foo(IAuthRequest r) { Called++; }
        }
        [Fact]
        public void AuthMethod_MethodWithNullAttribute_IsCalled()
        {
            var handler = new AuthMethod_MethodWithNullAttribute_IsCalled_Class();
            handler.Auth(new MockRequest { Method = "foo" });
            Assert.Equal(1, handler.Called);
        }

        class AuthMethod_MethodWithInvalidAttribute_ThrowsInvalidOperationException_Class : BaseHandler
        {
            [AuthMethod("invalid.foo")]
            public void Foo(IAuthRequest r) { }
        }
        [Fact]
        public void AuthMethod_MethodWithInvalidAttribute_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new AuthMethod_MethodWithInvalidAttribute_ThrowsInvalidOperationException_Class());
        }

        class AuthMethod_MethodWithEmptyStringAttribute_ThrowsInvalidOperationException_Class : BaseHandler
        {
            [AuthMethod("")]
            public void Foo(IAuthRequest r) { }
        }
        [Fact]
        public void AuthMethod_MethodWithEmptyStringAttribute_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new AuthMethod_MethodWithEmptyStringAttribute_ThrowsInvalidOperationException_Class());
        }

        class AuthMethod_MethodWithInvalidClassMethodName_ThrowsInvalidOperationException_Class : BaseHandler
        {
            public void Foo‿Bar(IAuthRequest r) { }
        }
        [Fact]
        public void AuthMethod_MethodWithInvalidClassMethodName_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new AuthMethod_MethodWithInvalidClassMethodName_ThrowsInvalidOperationException_Class());
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

        class AuthMethod_AttributeOnAuthHandler_ThrowsInvalidOperationException_Class : BaseHandler
        {
            [AuthMethod]
            public override void Auth(IAuthRequest r) { }
        }
        [Fact]
        public void AuthMethod_AttributeOnAuthHandler_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new AuthMethod_AttributeOnAuthHandler_ThrowsInvalidOperationException_Class());
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
    }
}
