using System;
using Xunit;

namespace Celeste.Mod.FiniteLives.Test
{
    public class XMLParseUnitTests : IDisposable
    {
        private FiniteLivesModule module;

        public XMLParseUnitTests()
        {
            module = new FiniteLivesModule();
        }

        public void Dispose()
        {
            module = null;
        }

        [Fact]
        public void TestRead_SingleGoodFile()
        {
            module.ReadXMLFile("finitelives_test.xml");

            Assert.Equal(2, module.chapters.Count);
            Assert.True(module.chapters.ContainsKey("Chapter1Name"));
            Assert.True(module.chapters.ContainsKey("Chapter2Name"));

            Chapter c;
            Assert.True(module.chapters.TryGetValue("Chapter1Name", out c));
            Assert.Equal(5, c.GetLives("a-00"));
            Assert.Equal(0, c.GetLives("a-01"));
            Assert.Equal(0, c.GetLives("a-02"));
            Assert.Equal(0, c.GetLives("a-03"));
        }

        [Fact]
        public void TestRead_SingleBadFile()
        {
            module.ReadXMLFile("finitelives_test2.xml");

            Assert.Single(module.chapters);
            Assert.True(module.chapters.ContainsKey("Chapter1Name"));

            Chapter c;
            Assert.True(module.chapters.TryGetValue("Chapter1Name", out c));
            Assert.Equal(0, c.GetLives("a-00"));
            Assert.Equal(0, c.GetLives("a-01"));
            Assert.Equal(0, c.GetLives("a-02"));
            Assert.Equal(51, c.GetLives("a-99"));
        }
    }
}
