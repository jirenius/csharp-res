using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;

namespace ResgateIO.Service.UnitTests
{
    public class RouterTests
    {
        [Theory]
        [InlineData("", "model")]
        [InlineData("", "model.foo")]
        [InlineData("", "model.$id")]
        [InlineData("", "model.$id.foo")]
        [InlineData("", "model.>")]
        [InlineData("", "model.$id.>")]
        [InlineData("test", "model")]
        [InlineData("test", "model.foo")]
        [InlineData("test", "model.$id")]
        [InlineData("test", "model.$id.foo")]
        [InlineData("test", "model.>")]
        [InlineData("test", "model.$id.>")]
        public void AddHandler_ValidPath_NoException(string pattern, string path)
        {
            Router r = new Router(pattern);
            r.AddHandler(path, new DynamicHandler());
        }

        [Theory]
        [InlineData("model.$id.type.$id")]
        [InlineData("model..foo")]
        [InlineData("model.$")]
        [InlineData("model.$.foo")]
        [InlineData("model.>.foo")]
        [InlineData("model.foo.>bar")]
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
            Assert.Throws<ArgumentNullException>(() => r.AddHandler(pattern, group, null));
        }

        [Theory]
        [InlineData("test")]
        [InlineData("test.foo")]
        [InlineData("test.foo.bar")]
        public void Pattern_ValidPattern_GetsPattern(string pattern)
        {
            Router r = new Router(pattern);
            Assert.Equal(pattern, r.Pattern);
        }

        [Theory]
        [InlineData("", "model", "model")]
        [InlineData("", "model.foo", "model.foo")]
        [InlineData("", "model.$id", "model.42")]
        [InlineData("", "model.$id.foo", "model.42.foo")]
        [InlineData("", "model.>", "model.foo")]
        [InlineData("", "model.>", "model.foo.bar")]
        [InlineData("", "model.$id.>", "model.42.foo")]
        [InlineData("", "model.$id.>", "model.42.foo.bar")]
        [InlineData("test", "model", "test.model")]
        [InlineData("test", "model.foo", "test.model.foo")]
        [InlineData("test", "model.$id", "test.model.42")]
        [InlineData("test", "model.$id.foo", "test.model.42.foo")]
        [InlineData("test", "model.>", "test.model.foo")]
        [InlineData("test", "model.>", "test.model.foo.bar")]
        [InlineData("test", "model.$id.>", "test.model.42.foo")]
        [InlineData("test", "model.$id.>", "test.model.42.foo.bar")]
        public void GetHandler_MatchingPath_ReturnsHandler(string pattern, string path, string resourceName)
        {
            Router r = new Router(pattern);
            r.AddHandler(path, new DynamicHandler());
            Router.Match m = r.GetHandler(resourceName);
            Assert.NotNull(m);
            Assert.Equal(resourceName, m.Group);
        }

        [Theory]
        [InlineData("", "model", "model.foo")]
        [InlineData("", "model.foo", "model")]
        [InlineData("", "model.$id", "model.42.foo")]
        [InlineData("", "model.$id.foo", "model.42")]
        [InlineData("", "model.>", "model")]
        [InlineData("", "model.$id.>", "model.42")]
        [InlineData("test", "model", "test.model.foo")]
        [InlineData("test", "model.foo", "test.model")]
        [InlineData("test", "model.$id", "test.model.42.foo")]
        [InlineData("test", "model.$id.foo", "test.model.42")]
        [InlineData("test", "model.>", "test.model")]
        [InlineData("test", "model.$id.>", "test.model.42")]
        public void GetHandler_MismatchingPath_ReturnsNull(string pattern, string path, string resourceName)
        {
            Router r = new Router(pattern);
            r.AddHandler(path, new DynamicHandler());
            Router.Match m = r.GetHandler(resourceName);
            Assert.Null(m);
        }

        [Theory]
        [InlineData("", "model", "model", "foo", "foo")]
        [InlineData("", "model.foo", "model.foo", "bar", "bar")]
        [InlineData("", "model.$id", "model.42", "foo.bar", "foo.bar")]
        [InlineData("", "model.$id", "model.42", "${id}", "42")]
        [InlineData("", "model.$id", "model.42", "${id}foo", "42foo")]
        [InlineData("", "model.$id", "model.42", "foo${id}", "foo42")]
        [InlineData("", "model.$id", "model.42", "foo${id}bar", "foo42bar")]
        [InlineData("", "model.$id.$type", "model.42.foo", "foo.bar", "foo.bar")]
        [InlineData("", "model.$id.$type", "model.42.foo", "${id}", "42")]
        [InlineData("", "model.$id.$type", "model.42.foo", "${type}", "foo")]
        [InlineData("", "model.$id.$type", "model.42.foo", "${id}${type}", "42foo")]
        [InlineData("", "model.$id.$type", "model.42.foo", "${id}.${type}", "42.foo")]
        [InlineData("", "model.$id.$type", "model.42.foo", "${type}${id}", "foo42")]
        [InlineData("", "model.$id.$type", "model.42.foo", "bar.${type}.${id}.baz", "bar.foo.42.baz")]
        [InlineData("test", "model", "test.model", "foo", "foo")]
        [InlineData("test", "model.foo", "test.model.foo", "bar", "bar")]
        [InlineData("test", "model.$id", "test.model.42", "foo.bar", "foo.bar")]
        [InlineData("test", "model.$id", "test.model.42", "${id}", "42")]
        [InlineData("test", "model.$id", "test.model.42", "${id}foo", "42foo")]
        [InlineData("test", "model.$id", "test.model.42", "foo${id}", "foo42")]
        [InlineData("test", "model.$id", "test.model.42", "foo${id}bar", "foo42bar")]
        [InlineData("test", "model.$id.$type", "test.model.42.foo", "foo.bar", "foo.bar")]
        [InlineData("test", "model.$id.$type", "test.model.42.foo", "${id}", "42")]
        [InlineData("test", "model.$id.$type", "test.model.42.foo", "${type}", "foo")]
        [InlineData("test", "model.$id.$type", "test.model.42.foo", "${id}${type}", "42foo")]
        [InlineData("test", "model.$id.$type", "test.model.42.foo", "${id}.${type}", "42.foo")]
        [InlineData("test", "model.$id.$type", "test.model.42.foo", "${type}${id}", "foo42")]
        [InlineData("test", "model.$id.$type", "test.model.42.foo", "bar.${type}.${id}.baz", "bar.foo.42.baz")]
        public void GetHandler_MatchingPathWithGroup_ReturnsHandler(string pattern, string path, string resourceName, string group, string expectedGroup)
        {
            Router r = new Router(pattern);
            r.AddHandler(path, group, new DynamicHandler());
            Router.Match m = r.GetHandler(resourceName);
            Assert.NotNull(m);
            Assert.Equal(expectedGroup, m.Group);
        }

        [Theory]
        [InlineData("", "model", "sub.model", "foo", "foo")]
        [InlineData("", "model.foo", "sub.model.foo", "bar", "bar")]
        [InlineData("", "model.$id", "sub.model.42", "foo.bar", "foo.bar")]
        [InlineData("", "model.$id", "sub.model.42", "${id}", "42")]
        [InlineData("", "model.$id", "sub.model.42", "${id}foo", "42foo")]
        [InlineData("", "model.$id", "sub.model.42", "foo${id}", "foo42")]
        [InlineData("", "model.$id", "sub.model.42", "foo${id}bar", "foo42bar")]
        [InlineData("", "model.$id.$type", "sub.model.42.foo", "foo.bar", "foo.bar")]
        [InlineData("", "model.$id.$type", "sub.model.42.foo", "${id}", "42")]
        [InlineData("", "model.$id.$type", "sub.model.42.foo", "${type}", "foo")]
        [InlineData("", "model.$id.$type", "sub.model.42.foo", "${id}${type}", "42foo")]
        [InlineData("", "model.$id.$type", "sub.model.42.foo", "${id}.${type}", "42.foo")]
        [InlineData("", "model.$id.$type", "sub.model.42.foo", "${type}${id}", "foo42")]
        [InlineData("", "model.$id.$type", "sub.model.42.foo", "bar.${type}.${id}.baz", "bar.foo.42.baz")]
        [InlineData("test", "model", "test.sub.model", "foo", "foo")]
        [InlineData("test", "model.foo", "test.sub.model.foo", "bar", "bar")]
        [InlineData("test", "model.$id", "test.sub.model.42", "foo.bar", "foo.bar")]
        [InlineData("test", "model.$id", "test.sub.model.42", "${id}", "42")]
        [InlineData("test", "model.$id", "test.sub.model.42", "${id}foo", "42foo")]
        [InlineData("test", "model.$id", "test.sub.model.42", "foo${id}", "foo42")]
        [InlineData("test", "model.$id", "test.sub.model.42", "foo${id}bar", "foo42bar")]
        [InlineData("test", "model.$id.$type", "test.sub.model.42.foo", "foo.bar", "foo.bar")]
        [InlineData("test", "model.$id.$type", "test.sub.model.42.foo", "${id}", "42")]
        [InlineData("test", "model.$id.$type", "test.sub.model.42.foo", "${type}", "foo")]
        [InlineData("test", "model.$id.$type", "test.sub.model.42.foo", "${id}${type}", "42foo")]
        [InlineData("test", "model.$id.$type", "test.sub.model.42.foo", "${id}.${type}", "42.foo")]
        [InlineData("test", "model.$id.$type", "test.sub.model.42.foo", "${type}${id}", "foo42")]
        [InlineData("test", "model.$id.$type", "test.sub.model.42.foo", "bar.${type}.${id}.baz", "bar.foo.42.baz")]
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
        [InlineData("", "model", "$type", "model")]
        [InlineData("", "model.foo", "model.$id", "model.foo")]
        [InlineData("", "model.foo", "$type.foo", "model.foo")]
        [InlineData("", "model.$id", "model.>", "model.42")]
        [InlineData("", "model.$id.foo", "model.$id.$type", "model.42.foo")]
        [InlineData("", "model.$id.foo", "model.$id.>", "model.42.foo")]
        [InlineData("", "model.$id.foo", "model.>", "model.42.foo")]
        [InlineData("", "model.>", ">", "model.foo")]
        [InlineData("", "model.>", "$type.>", "model.foo")]
        [InlineData("", "model.$id.>", "model.>", "model.42.foo")]
        [InlineData("", "model.$id.>", "$type.>", "model.42.foo")]
        [InlineData("", "model.$id.>", ">", "model.42.foo")]
        [InlineData("test", "model", "$type", "test.model")]
        [InlineData("test", "model.foo", "model.$id", "test.model.foo")]
        [InlineData("test", "model.foo", "$type.foo", "test.model.foo")]
        [InlineData("test", "model.$id", "model.>", "test.model.42")]
        [InlineData("test", "model.$id.foo", "model.$id.$type", "test.model.42.foo")]
        [InlineData("test", "model.$id.foo", "model.$id.>", "test.model.42.foo")]
        [InlineData("test", "model.$id.foo", "model.>", "test.model.42.foo")]
        [InlineData("test", "model.>", ">", "test.model.foo")]
        [InlineData("test", "model.>", "$type.>", "test.model.foo")]
        [InlineData("test", "model.$id.>", "model.>", "test.model.42.foo")]
        [InlineData("test", "model.$id.>", "$type.>", "test.model.42.foo")]
        [InlineData("test", "model.$id.>", ">", "test.model.42.foo")]
        public void GetHandler_MoreSpecificPath_ReturnsMoreSpecificHandler(string pattern, string specificPath, string wildcardPath, string resourceName)
        {
            Router r = new Router(pattern);
            IResourceHandler specificHandler = new DynamicHandler().SetType(ResourceType.Model);
            IResourceHandler wildcardHandler = new DynamicHandler().SetType(ResourceType.Collection);
            r.AddHandler(specificPath, specificHandler);
            r.AddHandler(wildcardPath, wildcardHandler);
            Router.Match m = r.GetHandler(resourceName);
            Assert.Equal(specificHandler, m.Handler);
        }

        [Theory]
        [InlineData("", "model")]
        [InlineData("", "model.foo")]
        [InlineData("", "model.$id")]
        [InlineData("", "model.$id.foo")]
        [InlineData("", "model.>")]
        [InlineData("", "model.$id.>")]
        [InlineData("test", "model")]
        [InlineData("test", "model.foo")]
        [InlineData("test", "model.$id")]
        [InlineData("test", "model.$id.foo")]
        [InlineData("test", "model.>")]
        [InlineData("test", "model.$id.>")]
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
            IResourceHandler specificHandler = new DynamicHandler().SetType(ResourceType.Model);
            IResourceHandler wildcardHandler = new DynamicHandler().SetType(ResourceType.Collection);
            r.AddHandler(specificPath, specificHandler);
            r.AddHandler(wildcardPath, wildcardHandler);
            Assert.True(r.Contains(rh => rh.Type == ResourceType.Model));
            Assert.True(r.Contains(rh => rh.Type == ResourceType.Collection));
            Assert.False(r.Contains(rh => rh.Type == ResourceType.Unknown));
        }

        [Theory]
        [InlineData(null, null, "sub", "model", "sub.model")]
        [InlineData("", null, "sub", "model", "sub.model")]
        [InlineData(null, "", "sub", "model", "sub.model")]
        [InlineData("", "", "sub", "model", "sub.model")]
        [InlineData(null, "sub", null, "model", "sub.model")]
        [InlineData(null, "sub", "", "model", "sub.model")]
        [InlineData("test", null, "sub", "model", "test.sub.model")]
        [InlineData("test", "sub", null, "model", "test.sub.model")]
        [InlineData("test", null, "sub", "$id", "test.sub.foo")]
        [InlineData("test", "sub", null, ">", "test.sub.foo.bar")]
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
        [InlineData("model")]
        [InlineData("model.$id")]
        [InlineData("model.>")]
        public void Mount_ToExistingPattern_ThrowsInvalidOperationException(string pattern)
        {
            Router r = new Router("test");
            r.AddHandler(pattern, new DynamicHandler());
            Router sub = new Router();
            Assert.Throws<InvalidOperationException>(() => r.Mount(pattern, sub));
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
    }
}
