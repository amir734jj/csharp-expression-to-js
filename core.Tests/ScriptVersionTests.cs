
using core.Enums;
using Xunit;

namespace core.Tests
{
    public class ScriptVersionTests
    {
        [Fact]
        public void Test__NonStd()
        {
            // Arrange, Act
            var scv = ScriptVersion.Es50.NonStandard();
            
            // Assert
            Assert.Equal(100005000, (int)scv);
        }

        [Fact]
        public void Test__MsJ()
        {
            // Arrange, Act
            var scv = ScriptVersion.Es50.MicrosoftJScript(90);
            
            // Assert
            Assert.Equal(309005000, (int)scv);
        }

        [Fact]
        public void Test__Js()
        {
            // Arrange, Act
            var scv = ScriptVersion.Es50.Javascript(181);
            
            // Assert
            Assert.Equal(218105000, (int)scv);
        }

        [Fact]
        public void Test__Proposals()
        {
            // Arrange, Act
            var scv = ScriptVersion.Es50.Proposals();
            
            // Assert
            Assert.Equal(5001, (int)scv);
        }

        [Fact]
        public void Test__Deprecated()
        {
            // Arrange, Act
            var scv = ScriptVersion.Es50.Deprecated();
            
            // Assert
            Assert.Equal(5002, (int)scv);
        }

        [Fact]
        public void Test__EsNext()
        {
            // Arrange, Act
            var scv = ScriptVersion.EsLatestStable.Proposals();
            
            // Assert
            Assert.Equal((int)ScriptVersion.EsNext, (int)scv);
        }
    }
}
