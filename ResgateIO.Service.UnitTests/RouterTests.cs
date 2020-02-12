using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;

namespace ResgateIO.Service.UnitTests
{
    public class RouterTests
    {
        #region TestSets
        public static IEnumerable<object[]> GetValidPathTestSets()
        {
            yield return new object[] { "", "model" };
            yield return new object[] { "", "model.foo" };
            yield return new object[] { "", "model.$id" };
            yield return new object[] { "", "model.$id.foo" };
            yield return new object[] { "", "model.>" };
            yield return new object[] { "", "model.$id.>" };
            yield return new object[] { "test", "model" };
            yield return new object[] { "test", "model.foo" };
            yield return new object[] { "test", "model.$id" };
            yield return new object[] { "test", "model.$id.foo" };
            yield return new object[] { "test", "model.>" };
            yield return new object[] { "test", "model.$id.>" };
        }

        public static IEnumerable<object[]> GetInvalidPathTestSets()
        {
            yield return new object[] { "model.$id.type.$id" };
            yield return new object[] { "model..foo" };
            yield return new object[] { "model.$" };
            yield return new object[] { "model.$.foo" };
            yield return new object[] { "model.>.foo" };
            yield return new object[] { "model.foo.>bar" };
        }

        public static IEnumerable<object[]> GetMatchingPathTestSets()
        {
            yield return new object[] { "", "model", "model" };
            yield return new object[] { "", "model.foo", "model.foo" };
            yield return new object[] { "", "model.$id", "model.42" };
            yield return new object[] { "", "model.$id.foo", "model.42.foo" };
            yield return new object[] { "", "model.>", "model.foo" };
            yield return new object[] { "", "model.>", "model.foo.bar" };
            yield return new object[] { "", "model.$id.>", "model.42.foo" };
            yield return new object[] { "", "model.$id.>", "model.42.foo.bar" };
            yield return new object[] { "test", "model", "test.model" };
            yield return new object[] { "test", "model.foo", "test.model.foo" };
            yield return new object[] { "test", "model.$id", "test.model.42" };
            yield return new object[] { "test", "model.$id.foo", "test.model.42.foo" };
            yield return new object[] { "test", "model.>", "test.model.foo" };
            yield return new object[] { "test", "model.>", "test.model.foo.bar" };
            yield return new object[] { "test", "model.$id.>", "test.model.42.foo" };
            yield return new object[] { "test", "model.$id.>", "test.model.42.foo.bar" };
        }

        public static IEnumerable<object[]> GetMatchingMountedPathTestSets()
        {
            yield return new object[] { "", "model", "sub.model" };
            yield return new object[] { "", "model.foo", "sub.model.foo" };
            yield return new object[] { "", "model.$id", "sub.model.42" };
            yield return new object[] { "", "model.$id.foo", "sub.model.42.foo" };
            yield return new object[] { "", "model.>", "sub.model.foo" };
            yield return new object[] { "", "model.>", "sub.model.foo.bar" };
            yield return new object[] { "", "model.$id.>", "sub.model.42.foo" };
            yield return new object[] { "", "model.$id.>", "sub.model.42.foo.bar" };
            yield return new object[] { "test", "model", "test.sub.model" };
            yield return new object[] { "test", "model.foo", "test.sub.model.foo" };
            yield return new object[] { "test", "model.$id", "test.sub.model.42" };
            yield return new object[] { "test", "model.$id.foo", "test.sub.model.42.foo" };
            yield return new object[] { "test", "model.>", "test.sub.model.foo" };
            yield return new object[] { "test", "model.>", "test.sub.model.foo.bar" };
            yield return new object[] { "test", "model.$id.>", "test.sub.model.42.foo" };
            yield return new object[] { "test", "model.$id.>", "test.sub.model.42.foo.bar" };
        }

        public static IEnumerable<object[]> GetMismatchingPathTestSets()
        {
            yield return new object[] { "", "model", "model.foo" };
            yield return new object[] { "", "model.foo", "model" };
            yield return new object[] { "", "model.$id", "model.42.foo" };
            yield return new object[] { "", "model.$id.foo", "model.42" };
            yield return new object[] { "", "model.>", "model" };
            yield return new object[] { "", "model.$id.>", "model.42" };
            yield return new object[] { "test", "model", "test.model.foo" };
            yield return new object[] { "test", "model.foo", "test.model" };
            yield return new object[] { "test", "model.$id", "test.model.42.foo" };
            yield return new object[] { "test", "model.$id.foo", "test.model.42" };
            yield return new object[] { "test", "model.>", "test.model" };
            yield return new object[] { "test", "model.$id.>", "test.model.42" };
        }

        public static IEnumerable<object[]> GetMatchingPathWithGroupTestSets()
        {
            yield return new object[] { "", "model", "model", "foo", "foo" };
            yield return new object[] { "", "model.foo", "model.foo", "bar", "bar" };
            yield return new object[] { "", "model.$id", "model.42", "foo.bar", "foo.bar" };
            yield return new object[] { "", "model.$id", "model.42", "${id}", "42" };
            yield return new object[] { "", "model.$id", "model.42", "${id}foo", "42foo" };
            yield return new object[] { "", "model.$id", "model.42", "foo${id}", "foo42" };
            yield return new object[] { "", "model.$id", "model.42", "foo${id}bar", "foo42bar" };
            yield return new object[] { "", "model.$id.$type", "model.42.foo", "foo.bar", "foo.bar" };
            yield return new object[] { "", "model.$id.$type", "model.42.foo", "${id}", "42" };
            yield return new object[] { "", "model.$id.$type", "model.42.foo", "${type}", "foo" };
            yield return new object[] { "", "model.$id.$type", "model.42.foo", "${id}${type}", "42foo" };
            yield return new object[] { "", "model.$id.$type", "model.42.foo", "${id}.${type}", "42.foo" };
            yield return new object[] { "", "model.$id.$type", "model.42.foo", "${type}${id}", "foo42" };
            yield return new object[] { "", "model.$id.$type", "model.42.foo", "bar.${type}.${id}.baz", "bar.foo.42.baz" };
            yield return new object[] { "test", "model", "test.model", "foo", "foo" };
            yield return new object[] { "test", "model.foo", "test.model.foo", "bar", "bar" };
            yield return new object[] { "test", "model.$id", "test.model.42", "foo.bar", "foo.bar" };
            yield return new object[] { "test", "model.$id", "test.model.42", "${id}", "42" };
            yield return new object[] { "test", "model.$id", "test.model.42", "${id}foo", "42foo" };
            yield return new object[] { "test", "model.$id", "test.model.42", "foo${id}", "foo42" };
            yield return new object[] { "test", "model.$id", "test.model.42", "foo${id}bar", "foo42bar" };
            yield return new object[] { "test", "model.$id.$type", "test.model.42.foo", "foo.bar", "foo.bar" };
            yield return new object[] { "test", "model.$id.$type", "test.model.42.foo", "${id}", "42" };
            yield return new object[] { "test", "model.$id.$type", "test.model.42.foo", "${type}", "foo" };
            yield return new object[] { "test", "model.$id.$type", "test.model.42.foo", "${id}${type}", "42foo" };
            yield return new object[] { "test", "model.$id.$type", "test.model.42.foo", "${id}.${type}", "42.foo" };
            yield return new object[] { "test", "model.$id.$type", "test.model.42.foo", "${type}${id}", "foo42" };
            yield return new object[] { "test", "model.$id.$type", "test.model.42.foo", "bar.${type}.${id}.baz", "bar.foo.42.baz" };
        }

        public static IEnumerable<object[]> GetMatchingPathWithGroupOnMountedRouterTestSets()
        {
            yield return new object[] { "", "model", "sub.model", "foo", "foo" };
            yield return new object[] { "", "model.foo", "sub.model.foo", "bar", "bar" };
            yield return new object[] { "", "model.$id", "sub.model.42", "foo.bar", "foo.bar" };
            yield return new object[] { "", "model.$id", "sub.model.42", "${id}", "42" };
            yield return new object[] { "", "model.$id", "sub.model.42", "${id}foo", "42foo" };
            yield return new object[] { "", "model.$id", "sub.model.42", "foo${id}", "foo42" };
            yield return new object[] { "", "model.$id", "sub.model.42", "foo${id}bar", "foo42bar" };
            yield return new object[] { "", "model.$id.$type", "sub.model.42.foo", "foo.bar", "foo.bar" };
            yield return new object[] { "", "model.$id.$type", "sub.model.42.foo", "${id}", "42" };
            yield return new object[] { "", "model.$id.$type", "sub.model.42.foo", "${type}", "foo" };
            yield return new object[] { "", "model.$id.$type", "sub.model.42.foo", "${id}${type}", "42foo" };
            yield return new object[] { "", "model.$id.$type", "sub.model.42.foo", "${id}.${type}", "42.foo" };
            yield return new object[] { "", "model.$id.$type", "sub.model.42.foo", "${type}${id}", "foo42" };
            yield return new object[] { "", "model.$id.$type", "sub.model.42.foo", "bar.${type}.${id}.baz", "bar.foo.42.baz" };
            yield return new object[] { "test", "model", "test.sub.model", "foo", "foo" };
            yield return new object[] { "test", "model.foo", "test.sub.model.foo", "bar", "bar" };
            yield return new object[] { "test", "model.$id", "test.sub.model.42", "foo.bar", "foo.bar" };
            yield return new object[] { "test", "model.$id", "test.sub.model.42", "${id}", "42" };
            yield return new object[] { "test", "model.$id", "test.sub.model.42", "${id}foo", "42foo" };
            yield return new object[] { "test", "model.$id", "test.sub.model.42", "foo${id}", "foo42" };
            yield return new object[] { "test", "model.$id", "test.sub.model.42", "foo${id}bar", "foo42bar" };
            yield return new object[] { "test", "model.$id.$type", "test.sub.model.42.foo", "foo.bar", "foo.bar" };
            yield return new object[] { "test", "model.$id.$type", "test.sub.model.42.foo", "${id}", "42" };
            yield return new object[] { "test", "model.$id.$type", "test.sub.model.42.foo", "${type}", "foo" };
            yield return new object[] { "test", "model.$id.$type", "test.sub.model.42.foo", "${id}${type}", "42foo" };
            yield return new object[] { "test", "model.$id.$type", "test.sub.model.42.foo", "${id}.${type}", "42.foo" };
            yield return new object[] { "test", "model.$id.$type", "test.sub.model.42.foo", "${type}${id}", "foo42" };
            yield return new object[] { "test", "model.$id.$type", "test.sub.model.42.foo", "bar.${type}.${id}.baz", "bar.foo.42.baz" };
        }

        public static IEnumerable<object[]> GetMoreSpecificPathTestSets()
        {
            yield return new object[] { "", "model", "$type", "model" };
            yield return new object[] { "", "model.foo", "model.$id", "model.foo" };
            yield return new object[] { "", "model.foo", "$type.foo", "model.foo" };
            yield return new object[] { "", "model.$id", "model.>", "model.42" };
            yield return new object[] { "", "model.$id.foo", "model.$id.$type", "model.42.foo" };
            yield return new object[] { "", "model.$id.foo", "model.$id.>", "model.42.foo" };
            yield return new object[] { "", "model.$id.foo", "model.>", "model.42.foo" };
            yield return new object[] { "", "model.>", ">", "model.foo" };
            yield return new object[] { "", "model.>", "$type.>", "model.foo" };
            yield return new object[] { "", "model.$id.>", "model.>", "model.42.foo" };
            yield return new object[] { "", "model.$id.>", "$type.>", "model.42.foo" };
            yield return new object[] { "", "model.$id.>", ">", "model.42.foo" };
            yield return new object[] { "test", "model", "$type", "test.model" };
            yield return new object[] { "test", "model.foo", "model.$id", "test.model.foo" };
            yield return new object[] { "test", "model.foo", "$type.foo", "test.model.foo" };
            yield return new object[] { "test", "model.$id", "model.>", "test.model.42" };
            yield return new object[] { "test", "model.$id.foo", "model.$id.$type", "test.model.42.foo" };
            yield return new object[] { "test", "model.$id.foo", "model.$id.>", "test.model.42.foo" };
            yield return new object[] { "test", "model.$id.foo", "model.>", "test.model.42.foo" };
            yield return new object[] { "test", "model.>", ">", "test.model.foo" };
            yield return new object[] { "test", "model.>", "$type.>", "test.model.foo" };
            yield return new object[] { "test", "model.$id.>", "model.>", "test.model.42.foo" };
            yield return new object[] { "test", "model.$id.>", "$type.>", "test.model.42.foo" };
            yield return new object[] { "test", "model.$id.>", ">", "test.model.42.foo" };
        }

        public static IEnumerable<object[]> GetMountedSubrouterTestData()
        {
            yield return new object[] { "", "model", "sub.model", "{}" };
            yield return new object[] { "", "model.foo", "sub.model.foo", "{}" };
            yield return new object[] { "", "model.$id", "sub.model.42", "{\"id\":\"42\"}" };
            yield return new object[] { "", "model.$id.foo", "sub.model.42.foo", "{\"id\":\"42\"}" };
            yield return new object[] { "", "model.>", "sub.model.foo", "{}" };
            yield return new object[] { "", "model.>", "sub.model.foo.bar", "{}" };
            yield return new object[] { "", "model.$id.>", "sub.model.42.foo", "{\"id\":\"42\"}" };
            yield return new object[] { "", "model.$id.>", "sub.model.42.foo.bar", "{\"id\":\"42\"}" };
            yield return new object[] { "test", "model", "test.sub.model", "{}" };
            yield return new object[] { "test", "model.foo", "test.sub.model.foo", "{}" };
            yield return new object[] { "test", "model.$id", "test.sub.model.42", "{\"id\":\"42\"}" };
            yield return new object[] { "test", "model.$id.foo", "test.sub.model.42.foo", "{\"id\":\"42\"}" };
            yield return new object[] { "test", "model.>", "test.sub.model.foo", "{}" };
            yield return new object[] { "test", "model.>", "test.sub.model.foo.bar", "{}" };
            yield return new object[] { "test", "model.$id.>", "test.sub.model.42.foo", "{\"id\":\"42\"}" };
            yield return new object[] { "test", "model.$id.>", "test.sub.model.42.foo.bar", "{\"id\":\"42\"}" };
        }

        public static IEnumerable<object[]> GetMountToSubpathTestData()
        {
            yield return new object[] { null, null, "sub", "model", "sub.model" };
            yield return new object[] { "", null, "sub", "model", "sub.model" };
            yield return new object[] { null, "", "sub", "model", "sub.model" };
            yield return new object[] { "", "", "sub", "model", "sub.model" };
            yield return new object[] { null, "sub", null, "model", "sub.model" };
            yield return new object[] { null, "sub", "", "model", "sub.model" };
            yield return new object[] { "test", null, "sub", "model", "test.sub.model" };
            yield return new object[] { "test", "sub", null, "model", "test.sub.model" };
            yield return new object[] { "test", null, "sub", "$id", "test.sub.foo" };
            yield return new object[] { "test", "sub", null, ">", "test.sub.foo.bar" };
        }
        #endregion

        #region AddHandler
        [Theory]
        [MemberData(nameof(GetValidPathTestSets))]
        public void AddHandler_ValidPath_NoException(string pattern, string path)
        {
            Router r = new Router(pattern);
            r.AddHandler(path, new DynamicHandler());
        }

        [Theory]
        [MemberData(nameof(GetInvalidPathTestSets))]
        public void AddHandler_InvalidPath_ThrowsException(string path)
        {
            Router r = new Router("test");
            Assert.Throws<ArgumentException>(() => r.AddHandler(path, new DynamicHandler()));
        }

        [Fact]
        public void AddHandler_DuplicatePattern_ThrowsException()
        {
            Router r = new Router();
            r.AddHandler("test.model", new DynamicHandler());
            Assert.Throws<ArgumentException>(() => r.AddHandler("test.model", new DynamicHandler()));
        }

        [Fact]
        public void AddHandler_NullHandler_ThrowsException()
        {
            Router r = new Router();
            Assert.Throws<ArgumentNullException>(() => r.AddHandler("test.model", null));
        }

        [Theory]
        [InlineData("", "test")]
        [InlineData("test", "test")]
        [InlineData("test", "test.$foo")]
        [InlineData("test.${foo}", "test.$foo")]
        [InlineData("${foo}", "test.$foo")]
        [InlineData("${foo}.test", "test.$foo")]
        [InlineData("${foo}${bar}", "test.$foo.$bar")]
        [InlineData("${bar}${foo}", "test.$foo.$bar")]
        [InlineData("${foo}.${bar}", "test.$foo.$bar.>")]
        [InlineData("${foo}${foo}", "test.$foo.$bar")]
        public void AddHandler_ValidGroup_NoException(string group, string pattern)
        {
            Router r = new Router();
            r.AddHandler(pattern, group, new DynamicHandler());
        }

        [Theory]
        [InlineData("$", "test.$foo")]
        [InlineData("${", "test.$foo")]
        [InlineData("${foo", "test.$foo")]
        [InlineData("${}", "test.$foo")]
        [InlineData("${$foo}", "test.$foo")]
        [InlineData("${bar}", "test.$foo")]
        public void AddHandler_InvalidValidGroup_ThrowsException(string group, string pattern)
        {
            Router r = new Router();
            Assert.Throws<ArgumentException>(() => r.AddHandler(pattern, group, new DynamicHandler()));
        }

        [Fact]
        public void AddHandler_WithPathWithoutSubpattern_NoException()
        {
            Router r = new Router("test");
            r.AddHandler(new DynamicHandler());
        }

        [ResourcePattern("model.$foo")]
        class AddHandler_ResourcePatternAttributeValidPath_NoException_Class : BaseHandler { }
        [Fact]
        public void AddHandler_ResourcePatternAttributeValidPath_NoException()
        {
            Router r = new Router("test");
            r.AddHandler(new AddHandler_ResourcePatternAttributeValidPath_NoException_Class());
        }

        [ResourcePattern("model..foo")]
        class AddHandler_ResourcePatternAttributeInvalidPath_ThrowsException_Class : BaseHandler { }
        [Fact]
        public void AddHandler_ResourcePatternAttributeInvalidPath_ThrowsException()
        {
            Router r = new Router("test");
            Assert.Throws<ArgumentException>(() => r.AddHandler(new AddHandler_ResourcePatternAttributeInvalidPath_ThrowsException_Class()));
        }
        
        [ResourcePattern("test.model")]
        class AddHandler_ResourcePatternAttributeDuplicatePattern_ThrowsException_Class : BaseHandler { }
        [Fact]
        public void AddHandler_ResourcePatternAttributeDuplicatePattern_ThrowsException()
        {
            Router r = new Router();
            r.AddHandler("test.model", new DynamicHandler());
            Assert.Throws<ArgumentException>(() => r.AddHandler(new AddHandler_ResourcePatternAttributeDuplicatePattern_ThrowsException_Class()));
        }

        [ResourceGroup("foo")]
        class AddHandler_ResourceGroupAttributeValidGroup_NoException_Class : BaseHandler { }
        [Fact]
        public void AddHandler_ResourceGroupAttributeValidGroup_NoException()
        {
            Router r = new Router("test");
            r.AddHandler("model", new AddHandler_ResourceGroupAttributeValidGroup_NoException_Class());
        }

        [ResourceGroup("$")]
        class AddHandler_ResourceGroupAttributeInvalidGroup_NoException_Class : BaseHandler { }
        [Fact]
        public void AddHandler_ResourceGroupAttributeInvalidGroup_NoException()
        {
            Router r = new Router();
            Assert.Throws<ArgumentException>(() => r.AddHandler("model", new AddHandler_ResourceGroupAttributeInvalidGroup_NoException_Class()));
        }

        [ResourcePattern("model.$foo")]
        [ResourceGroup("${foo}.bar")]
        class AddHandler_ResourcePatternAndResourceGroupAttributeValidGroup_NoException_Class : BaseHandler { }
        [Fact]
        public void AddHandler_ResourcePatternAndResourceGroupAttributeValidGroup_NoException()
        {
            Router r = new Router("test");
            r.AddHandler(new AddHandler_ResourceGroupAttributeValidGroup_NoException_Class());
        }
        #endregion

        #region AddEventListener
        [Theory]
        [MemberData(nameof(GetValidPathTestSets))]
        public void AddEventListener_ValidPathBeforeRegisteringHandler_NoException(string pattern, string path)
        {
            Router r = new Router(pattern);
            r.AddEventListener(path, (sender, ev) => { });
            r.AddHandler(path, new DynamicHandler());
        }

        [Theory]
        [MemberData(nameof(GetValidPathTestSets))]
        public void AddEventListener_ValidPathAfterRegisteringHandler_NoException(string pattern, string path)
        {
            Router r = new Router(pattern);
            r.AddHandler(path, new DynamicHandler());
            r.AddEventListener(path, (sender, ev) => { });
        }

        [Theory]
        [MemberData(nameof(GetInvalidPathTestSets))]
        public void AddEventListener_InvalidPath_ThrowsException(string path)
        {
            Router r = new Router("test");
            Assert.Throws<ArgumentException>(() => r.AddEventListener(path, (sender, ev) => { }));
        }

        [Fact]
        public void AddEventListener_DuplicatePattern_NoException()
        {
            Router r = new Router();
            r.AddEventListener("test.model", (sender, ev) => { });
            r.AddEventListener("test.model", (sender, ev) => { });
        }

        [Fact]
        public void AddEventListener_NullEventHandler_ThrowsException()
        {
            Router r = new Router();
            Assert.Throws<ArgumentNullException>(() => r.AddEventListener("test.model", null));
        }
        #endregion

        #region Pattern
        [Theory]
        [InlineData("test")]
        [InlineData("test.foo")]
        [InlineData("test.foo.bar")]
        public void Pattern_ValidPattern_GetsPattern(string pattern)
        {
            Router r = new Router(pattern);
            Assert.Equal(pattern, r.Pattern);
        }
        #endregion

        #region GetHandler
        [Theory]
        [MemberData(nameof(GetMatchingPathTestSets))]
        public void GetHandler_MatchingPath_ReturnsHandler(string pattern, string path, string resourceName)
        {
            Router r = new Router(pattern);
            r.AddHandler(path, new DynamicHandler());
            Router.Match m = r.GetHandler(resourceName);
            Assert.NotNull(m);
            Assert.Equal(resourceName, m.Group);
        }

        [Theory]
        [MemberData(nameof(GetMatchingMountedPathTestSets))]
        public void GetHandler_MatchingPathAddedBeforeMount_ReturnsHandler(string pattern, string path, string resourceName)
        {
            Router r = new Router(pattern);
            Router sub = new Router();
            sub.AddHandler(path, new DynamicHandler());
            r.Mount("sub", sub);
            Router.Match m = r.GetHandler(resourceName);
            Assert.NotNull(m);
            Assert.Equal(resourceName, m.Group);
        }

        [Theory]
        [MemberData(nameof(GetMatchingMountedPathTestSets))]
        public void GetHandler_MatchingPathAddedAfterMount_ReturnsHandler(string pattern, string path, string resourceName)
        {
            Router r = new Router(pattern);
            Router sub = new Router();
            r.Mount("sub", sub);
            r.AddHandler("sub."+path, new DynamicHandler());
            Router.Match m = r.GetHandler(resourceName);
            Assert.NotNull(m);
            Assert.Equal(resourceName, m.Group);
        }

        [Theory]
        [MemberData(nameof(GetMatchingPathTestSets))]
        public void GetHandler_EventListenersOnMatchingPath_ReturnsEventHandler(string pattern, string path, string resourceName)
        {
            int called1 = 0;
            int called2 = 0;
            Router r = new Router(pattern);
            r.AddHandler(path, new DynamicHandler());
            r.AddEventListener(path, (sender, ev) => called1++);
            r.AddEventListener(path, (sender, ev) => called2++);
            Router.Match m = r.GetHandler(resourceName);
            m.EventHandler.Invoke(null, null);
            Assert.Equal(1, called1);
            Assert.Equal(1, called2);
        }

        [Theory]
        [MemberData(nameof(GetMatchingMountedPathTestSets))]
        public void GetHandler_EventListenersMatchingPathAddedBeforeMount_ReturnsEventHandler(string pattern, string path, string resourceName)
        {
            int called1 = 0;
            int called2 = 0;
            Router r = new Router(pattern);
            Router sub = new Router();
            sub.AddEventListener(path, (sender, ev) => called1++);
            sub.AddEventListener(path, (sender, ev) => called2++);
            r.Mount("sub", sub);
            r.AddHandler("sub."+path, new DynamicHandler());
            Router.Match m = r.GetHandler(resourceName);
            m.EventHandler.Invoke(null, null);
            Assert.Equal(1, called1);
            Assert.Equal(1, called2);
        }

        [Theory]
        [MemberData(nameof(GetMatchingMountedPathTestSets))]
        public void GetHandler_EventListenersMatchingPathAddedAfterMount_ReturnsEventHandler(string pattern, string path, string resourceName)
        {
            int called1 = 0;
            int called2 = 0;
            Router r = new Router(pattern);
            Router sub = new Router();
            sub.AddHandler(path, new DynamicHandler());
            r.Mount("sub", sub);
            r.AddEventListener("sub."+path, (sender, ev) => called1++);
            r.AddEventListener("sub."+path, (sender, ev) => called2++);
            Router.Match m = r.GetHandler(resourceName);
            m.EventHandler.Invoke(null, null);
            Assert.Equal(1, called1);
            Assert.Equal(1, called2);
        }

        [Theory]
        [MemberData(nameof(GetMismatchingPathTestSets))]
        public void GetHandler_MismatchingPath_ReturnsNull(string pattern, string path, string resourceName)
        {
            Router r = new Router(pattern);
            r.AddHandler(path, new DynamicHandler());
            Router.Match m = r.GetHandler(resourceName);
            Assert.Null(m);
        }

        [Theory]
        [MemberData(nameof(GetMismatchingPathTestSets))]
        public void GetHandler_EventListenerOnMismatchingPath_ReturnsNull(string pattern, string path, string resourceName)
        {
            Router r = new Router(pattern);
            r.AddHandler(path, new DynamicHandler());
            r.AddEventListener(path, (sender, ev) => { });
            Router.Match m = r.GetHandler(resourceName);
            Assert.Null(m);
        }

        [Theory]
        [MemberData(nameof(GetMatchingPathWithGroupTestSets))]
        public void GetHandler_MatchingPathWithGroup_ReturnsHandler(string pattern, string path, string resourceName, string group, string expectedGroup)
        {
            Router r = new Router(pattern);
            r.AddHandler(path, group, new DynamicHandler());
            Router.Match m = r.GetHandler(resourceName);
            Assert.NotNull(m);
            Assert.Equal(expectedGroup, m.Group);
        }

        [Theory]
        [MemberData(nameof(GetMatchingPathWithGroupOnMountedRouterTestSets))]
        public void GetHandler_MatchingPathWithGroupOnMountedRouter_ReturnsHandler(string pattern, string path, string resourceName, string group, string expectedGroup)
        {
            Router r = new Router(pattern);
            Router sub = new Router();
            sub.AddHandler(path, group, new DynamicHandler());
            r.Mount("sub", sub);
            Router.Match m = r.GetHandler(resourceName);
            Assert.NotNull(m);
            Assert.Equal(expectedGroup, m.Group);
        }
        
        [Theory]
        [MemberData(nameof(GetMoreSpecificPathTestSets))]
        public void GetHandler_MoreSpecificPath_ReturnsMoreSpecificHandler(string pattern, string specificPath, string wildcardPath, string resourceName)
        {
            Router r = new Router(pattern);
            var specificHandler = new DynamicHandler().SetType(ResourceType.Model);
            var wildcardHandler = new DynamicHandler().SetType(ResourceType.Collection);
            r.AddHandler(specificPath, specificHandler);
            r.AddHandler(wildcardPath, wildcardHandler);
            Router.Match m = r.GetHandler(resourceName);
            Assert.Equal(specificHandler, m.Handler);
        }

        [Theory]
        [MemberData(nameof(GetMoreSpecificPathTestSets))]
        public void GetHandler_EventListenersOnMoreSpecificPath_ReturnsMoreSpecificHandler(string pattern, string specificPath, string wildcardPath, string resourceName)
        {
            int specificCalled = 0;
            int wildcardCalled = 0;
            Router r = new Router(pattern);
            var specificHandler = new DynamicHandler().SetType(ResourceType.Model);
            var wildcardHandler = new DynamicHandler().SetType(ResourceType.Collection);
            r.AddHandler(specificPath, specificHandler);
            r.AddHandler(wildcardPath, wildcardHandler);
            r.AddEventListener(specificPath, (sender, ev) => specificCalled++);
            r.AddEventListener(wildcardPath, (sender, ev) => wildcardCalled++);
            Router.Match m = r.GetHandler(resourceName);
            m.EventHandler.Invoke(null, null);
            Assert.Equal(1, specificCalled);
            Assert.Equal(0, wildcardCalled);
        }

        [Theory]
        [MemberData(nameof(GetMountedSubrouterTestData))]
        public void GetHandler_FromMountedSubRouter_ReturnsHandler(string rootPattern, string handlerPattern, string resourceName, string expectedParams)
        {
            var handler = new DynamicHandler();
            Router r = rootPattern == null ? new Router() : new Router(rootPattern);
            Router sub = new Router();
            sub.AddHandler(handlerPattern, handler);
            r.Mount("sub", sub);
            Router.Match m = r.GetHandler(resourceName);
            Assert.NotNull(m);
            Assert.Equal(handler, m.Handler);
            Test.AssertJsonEqual(JObject.Parse(expectedParams), m.Params);
        }

        [Theory]
        [MemberData(nameof(GetMountedSubrouterTestData))]
        public void GetHandler_HandlerAddedAfterToMountedSubRouter_ReturnsHandler(string rootPattern, string handlerPattern, string resourceName, string expectedParams)
        {
            var handler = new DynamicHandler();
            Router r = rootPattern == null ? new Router() : new Router(rootPattern);
            Router sub = new Router();
            r.Mount("sub", sub);
            r.AddHandler("sub." + handlerPattern, handler);
            Router.Match m = r.GetHandler(resourceName);
            Assert.NotNull(m);
            Assert.Equal(handler, m.Handler);
            Test.AssertJsonEqual(JObject.Parse(expectedParams), m.Params);
        }

        [Theory]
        [MemberData(nameof(GetMountedSubrouterTestData))]
        public void GetHandler_EventListenerWhenFromMountedSubRouter_ReturnsHandler(string rootPattern, string handlerPattern, string resourceName, string expectedParams)
        {
            var called = 0;
            Router r = rootPattern == null ? new Router() : new Router(rootPattern);
            Router sub = new Router();
            sub.AddHandler(handlerPattern, new DynamicHandler());
            r.Mount("sub", sub);
            r.AddEventListener("sub." + handlerPattern, (sender, ev) => called++);
            Router.Match m = r.GetHandler(resourceName);
            m.EventHandler.Invoke(null, null);
            Test.AssertJsonEqual(JObject.Parse(expectedParams), m.Params);
            Assert.Equal(1, called);
        }

        [Theory]
        [MemberData(nameof(GetMountedSubrouterTestData))]
        public void GetHandler_EventListenerWhenHandlerAddedAfterToMountedSubRouter_ReturnsHandler(string rootPattern, string handlerPattern, string resourceName, string expectedParams)
        {
            var called = 0;
            Router r = rootPattern == null ? new Router() : new Router(rootPattern);
            Router sub = new Router();
            sub.AddEventListener(handlerPattern, (sender, ev) => called++);
            r.Mount("sub", sub);
            r.AddHandler("sub." + handlerPattern, new DynamicHandler());
            Router.Match m = r.GetHandler(resourceName);
            m.EventHandler.Invoke(null, null);
            Test.AssertJsonEqual(JObject.Parse(expectedParams), m.Params);
            Assert.Equal(1, called);
        }

        [Fact]
        public void GetHandler_WithPathWithoutPatternMatchingPath_ExpectedMatch()
        {
            Router r = new Router("test");
            var handler = new DynamicHandler();
            r.AddHandler(handler);
            Router.Match m = r.GetHandler("test");
            Assert.NotNull(m);
            Assert.Equal(handler, m.Handler);
        }

        [ResourcePattern("model.$foo")]
        class GetHandler_ResourcePatternAttributeMatchingPath_ExpectedMatch_Class : BaseHandler { }
        [Fact]
        public void GetHandler_ResourcePatternAttributeMatchingPath_ExpectedMatch()
        {
            Router r = new Router("test");
            var handler = new GetHandler_ResourcePatternAttributeMatchingPath_ExpectedMatch_Class();
            r.AddHandler(handler);
            Router.Match m = r.GetHandler("test.model.bar");
            Assert.NotNull(m);
            Assert.Equal(handler, m.Handler);
            Test.AssertJsonEqual(new { foo = "bar" }, m.Params);
        }

        [ResourceGroup("foo")]
        class GetHandler_ResourceGroupAttributeMatchingPath_ExpectedMatch_Class : BaseHandler { }
        [Fact]
        public void GetHandler_ResourceGroupAttributeMatchingPath_ExpectedMatch()
        {
            Router r = new Router("test");
            var handler = new GetHandler_ResourceGroupAttributeMatchingPath_ExpectedMatch_Class();
            r.AddHandler("model", handler);
            Router.Match m = r.GetHandler("test.model");
            Assert.NotNull(m);
            Assert.Equal(handler, m.Handler);
            Assert.Equal("foo", m.Group);
        }

        [ResourcePattern("model.$foo")]
        [ResourceGroup("${foo}.bar")]
        class GetHandler_ResourcePatternAndResourceGroupAttributeMatchingPath_ExpectedMatch_Class : BaseHandler { }
        [Fact]
        public void GetHandler_ResourcePatternAndResourceGroupAttributeValidGroup_NoException()
        {
            Router r = new Router("test");
            var handler = new GetHandler_ResourcePatternAndResourceGroupAttributeMatchingPath_ExpectedMatch_Class();
            r.AddHandler(handler);
            Router.Match m = r.GetHandler("test.model.42");
            Assert.NotNull(m);
            Assert.Equal(handler, m.Handler);
            Test.AssertJsonEqual(new { foo = "42" }, m.Params);
            Assert.Equal("42.bar", m.Group);
        }
        #endregion

        #region Contains
        [Theory]
        [MemberData(nameof(GetValidPathTestSets))]
        public void Contains_SinglePath_ReturnsTrue(string pattern, string path)
        {
            Router r = new Router(pattern);
            r.AddHandler(path, new DynamicHandler());
            Assert.True(r.Contains(rh => true));
        }

        [Theory]
        [InlineData("", "model", "$type")]
        [InlineData("", "model.foo", "model.$id")]
        [InlineData("", "model.foo", "$type.foo")]
        [InlineData("", "model.$id", "model.>")]
        [InlineData("", "model.$id.foo", "model.$id.$type")]
        [InlineData("", "model.$id.foo", "model.$id.>")]
        [InlineData("", "model.$id.foo", "model.>")]
        [InlineData("", "model.>", ">")]
        [InlineData("", "model.>", "$type.>")]
        [InlineData("", "model.$id.>", "model.>")]
        [InlineData("", "model.$id.>", "$type.>")]
        [InlineData("", "model.$id.>", ">")]
        [InlineData("test", "model", "$type")]
        [InlineData("test", "model.foo", "model.$id")]
        [InlineData("test", "model.foo", "$type.foo")]
        [InlineData("test", "model.$id", "model.>")]
        [InlineData("test", "model.$id.foo", "model.$id.$type")]
        [InlineData("test", "model.$id.foo", "model.$id.>")]
        [InlineData("test", "model.$id.foo", "model.>")]
        [InlineData("test", "model.>", ">")]
        [InlineData("test", "model.>", "$type.>")]
        [InlineData("test", "model.$id.>", "model.>")]
        [InlineData("test", "model.$id.>", "$type.>")]
        [InlineData("test", "model.$id.>", ">")]
        public void Contains_OverlappingPaths_ReturnsCorrectValue(string pattern, string specificPath, string wildcardPath)
        {
            Router r = new Router(pattern);
            var specificHandler = new DynamicHandler().SetType(ResourceType.Model);
            var wildcardHandler = new DynamicHandler().SetType(ResourceType.Collection);
            r.AddHandler(specificPath, specificHandler);
            r.AddHandler(wildcardPath, wildcardHandler);
            Assert.True(r.Contains(rh => rh.Type == ResourceType.Model));
            Assert.True(r.Contains(rh => rh.Type == ResourceType.Collection));
            Assert.False(r.Contains(rh => rh.Type == ResourceType.Unknown));
        }
        #endregion

        #region Mount
        [Theory]
        [MemberData(nameof(GetMountToSubpathTestData))]
        public void Mount_ToSubpath_MountsRouter(string rootPattern, string subPattern, string mountPattern, string handlerPattern, string resourceName)
        {
            var handler = new DynamicHandler();
            Router r = rootPattern == null ? new Router() : new Router(rootPattern);
            Router sub = subPattern == null ? new Router() : new Router(subPattern);
            sub.AddHandler(handlerPattern, handler);
            if (mountPattern == null)
                r.Mount(sub);
            else
                r.Mount(mountPattern, sub);
            Router.Match m = r.GetHandler(resourceName);
            Assert.NotNull(m);
            Assert.Equal(handler, m.Handler);
        }

        [Fact]
        public void Mount_ToRoot_ThrowsInvalidOperationException()
        {
            Router r = new Router("test");
            Router sub = new Router();
            Assert.Throws<InvalidOperationException>(() => r.Mount(sub));
        }

        [Fact]
        public void Mount_MountedRouter_ThrowsInvalidOperationException()
        {
            Router r1 = new Router("test1");
            Router r2 = new Router("test2");
            Router sub = new Router("sub");
            r1.Mount(sub);
            Assert.Throws<InvalidOperationException>(() => r2.Mount(sub));
        }

        [Theory]
        [MemberData(nameof(GetValidPathTestSets))]
        public void Mount_ToExistingPattern_ThrowsInvalidOperationException(string path, string pattern)
        {
            Router r = new Router(path);
            r.AddHandler(pattern, new DynamicHandler());
            Router sub = new Router();
            Assert.Throws<InvalidOperationException>(() => r.Mount(pattern, sub));
        }
        #endregion

        #region ValidateEventListeners
        [Theory]
        [MemberData(nameof(GetValidPathTestSets))]
        public void ValidateEventListeners_AddEventListenerBeforeAddHandler_NoException(string pattern, string path)
        {
            Router r = new Router(pattern);
            r.AddEventListener(path, (sender, ev) => { });
            r.AddHandler(path, new DynamicHandler());
            r.ValidateEventListeners();
        }

        [Theory]
        [MemberData(nameof(GetValidPathTestSets))]
        public void ValidateEventListeners_AddEventListenerAfterAddHandler_NoException(string pattern, string path)
        {
            Router r = new Router(pattern);
            r.AddHandler(path, new DynamicHandler());
            r.AddEventListener(path, (sender, ev) => { });
            r.ValidateEventListeners();
        }

        [Theory]
        [MemberData(nameof(GetValidPathTestSets))]
        public void ValidateEventListeners_AddEventListenerWithoutAddHandler_ThrowsInvalidOperationException(string pattern, string path)
        {
            Router r = new Router(pattern);
            r.AddEventListener(path, (sender, ev) => { });
            Assert.Throws<InvalidOperationException>(() => r.ValidateEventListeners());
        }

        [Theory]
        [MemberData(nameof(GetValidPathTestSets))]
        public void ValidateEventListeners_AddEventListenerWithFewerAddHandler_ThrowsInvalidOperationException(string pattern, string path)
        {
            Router r = new Router(pattern);
            r.AddEventListener(path, (sender, ev) => { });
            r.AddHandler("completely.different", new DynamicHandler());
            r.AddEventListener("completely.different", (sender, ev) => { });
            Assert.Throws<InvalidOperationException>(() => r.ValidateEventListeners());
        }
        #endregion

        #region EventListenerAttribute
        class EventListenerAttribute_PublicMethod_IsAdded_Class : BaseHandler {
            public int Called = 0;
            [EventListener("model")]
            public void OnModelEvent(object sender, EventArgs ev)
            {
                Called++;
            }
        }
        [Fact]
        public void EventListenerAttribute_PublicMethod_IsAdded()
        {
            var r = new Router("test");
            var h = new EventListenerAttribute_PublicMethod_IsAdded_Class();
            r.AddHandler("model", new DynamicHandler());
            r.AddHandler("foo", h);
            Router.Match m = r.GetHandler("test.model");
            m.EventHandler?.Invoke(null, null);
            Assert.Equal(1, h.Called);
        }

        [Fact]
        public void EventListenerAttribute_PublicMethodForOwnClass_IsAdded()
        {
            var r = new Router("test");
            var h = new EventListenerAttribute_PublicMethod_IsAdded_Class();
            r.AddHandler("model", h);
            Router.Match m = r.GetHandler("test.model");
            m.EventHandler?.Invoke(null, null);
            Assert.Equal(1, h.Called);
        }

        class EventListenerAttribute_PrivateMethod_IsAdded_Class : BaseHandler
        {
            public int Called = 0;
            [EventListener("model")]
            private void onModelEvent(object sender, EventArgs ev)
            {
                Called++;
            }
        }
        [Fact]
        public void EventListenerAttribute_PrivateMethod_IsAdded_IsAdded()
        {
            var r = new Router("test");
            var h = new EventListenerAttribute_PrivateMethod_IsAdded_Class();
            r.AddHandler("model", new DynamicHandler());
            r.AddHandler("foo", h);
            Router.Match m = r.GetHandler("test.model");
            m.EventHandler?.Invoke(null, null);
            Assert.Equal(1, h.Called);
        }

        class EventListenerAttribute_InheritedMethod_IsAdded_BaseClass : BaseHandler
        {
            public int Called = 0;
            [EventListener("model")]
            public void OnModelEvent(object sender, EventArgs ev)
            {
                Called++;
            }
        }
        class EventListenerAttribute_InheritedMethod_IsAdded_Class : EventListenerAttribute_InheritedMethod_IsAdded_BaseClass { }
        [Fact]
        public void EventListenerAttribute_InheritedMethod_IsAdded()
        {
            var r = new Router("test");
            var h = new EventListenerAttribute_InheritedMethod_IsAdded_Class();
            r.AddHandler("model", new DynamicHandler());
            r.AddHandler("foo", h);
            Router.Match m = r.GetHandler("test.model");
            m.EventHandler?.Invoke(null, null);
            Assert.Equal(1, h.Called);
        }

        class EventListenerAttribute_InvalidMethodSignature_ThrowsArgumentException_Class : BaseHandler
        {
            [EventListener("model")]
            public void OnModelEvent(EventArgs ev) { }
        }
        [Fact]
        public void EventListenerAttribute_InvalidMethodSignature_ThrowsArgumentException()
        {
            var r = new Router("test");
            var h = new EventListenerAttribute_InvalidMethodSignature_ThrowsArgumentException_Class();
            Assert.Throws<ArgumentException>(() => r.AddHandler("model", h));
        }

        class EventListenerAttribute_MultiplePublicMethods_AreAdded_Class : BaseHandler
        {
            public int Called1 = 0;
            public int Called2 = 0;
            [EventListener("model")]
            public void OnModelEvent1(object sender, EventArgs ev)
            {
                Called1++;
            }
            [EventListener("model")]
            public void OnModelEvent2(object sender, EventArgs ev)
            {
                Called2++;
            }
        }
        [Fact]
        public void EventListenerAttribute_MultiplePublicMethods_AreAdded()
        {
            var r = new Router("test");
            var h = new EventListenerAttribute_MultiplePublicMethods_AreAdded_Class();
            r.AddHandler("model", new DynamicHandler());
            r.AddHandler("foo", h);
            Router.Match m = r.GetHandler("test.model");
            m.EventHandler?.Invoke(null, null);
            Assert.Equal(1, h.Called1);
            Assert.Equal(1, h.Called2);
        }
        #endregion
    }
}
