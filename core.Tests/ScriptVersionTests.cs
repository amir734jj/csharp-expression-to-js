﻿
using Xunit;

namespace core.Tests
{
    public class ScriptVersionTests
    {
        [Fact]
        public void NonStd()
        {
            var scv = ScriptVersion.Es50.NonStandard();
            Assert.Equal(100005000, (int)scv);
        }

        [Fact]
        public void MsJ()
        {
            var scv = ScriptVersion.Es50.MicrosoftJScript(90);
            Assert.Equal(309005000, (int)scv);
        }

        [Fact]
        public void Js()
        {
            var scv = ScriptVersion.Es50.Javascript(181);
            Assert.Equal(218105000, (int)scv);
        }

        [Fact]
        public void Proposals()
        {
            var scv = ScriptVersion.Es50.Proposals();
            Assert.Equal(5001, (int)scv);
        }

        [Fact]
        public void Deprecated()
        {
            var scv = ScriptVersion.Es50.Deprecated();
            Assert.Equal(5002, (int)scv);
        }

        [Fact]
        public void EsNext()
        {
            var scv = ScriptVersion.EsLatestStable.Proposals();
            Assert.Equal((int)ScriptVersion.EsNext, (int)scv);
        }
    }
}