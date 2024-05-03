using System;
using AspNetCore.Turnstile.Core.Client;
using AspNetCore.Turnstile.Core.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Turnstile.Tests.Client
{
    [TestFixture]
    public class ProxyHttpClientHandlerTests
    {
        [Test]
        public void Initialize_WithProxy()
        {
            // Arrange
            var settingsMock = new Mock<IOptions<TurnstileSettings>>();
            settingsMock.SetupGet(instance => instance.Value)
                .Returns(new TurnstileSettings()
                {
                    UseProxy = true,
                    ProxyAddress = "http://10.1.2.3:80"
                });

            // Act
            var handler = new ProxyHttpClientHandler(settingsMock.Object);

            // Assert
            Assert.IsTrue(handler.UseProxy);
            Assert.IsNotNull(handler.Proxy);
            Assert.IsFalse(handler.Proxy.IsBypassed(new Uri("http://127.0.0.1:8080")));

        }

        [Test]
        public void Initialize_WithProxyBypassed()
        {
            // Arrange
            var settingsMock = new Mock<IOptions<TurnstileSettings>>();
            settingsMock.SetupGet(instance => instance.Value)
                .Returns(new TurnstileSettings()
                {
                    UseProxy = true,
                    ProxyAddress = "http://10.1.2.3:80",
                    BypassOnLocal = true
                });

            // Act
            var handler = new ProxyHttpClientHandler(settingsMock.Object);

            // Assert
            Assert.IsTrue(handler.UseProxy);
            Assert.IsNotNull(handler.Proxy);
            Assert.IsTrue(handler.Proxy.IsBypassed(new Uri("http://127.0.0.1:8080")));
        }

        [Test]
        public void Initialize_NoProxy()
        {
            // Arrange
            var settingsMock = new Mock<IOptions<TurnstileSettings>>();
            settingsMock.SetupGet(instance => instance.Value)
                .Returns(new TurnstileSettings()
                {
                    UseProxy = false
                });

            // Act
            var handler = new ProxyHttpClientHandler(settingsMock.Object);

            // Assert
            Assert.IsFalse(handler.UseProxy);
            Assert.IsNull(handler.Proxy);
        }

        [Test]
        public void Initialize_NotSpecified()
        {
            // Arrange
            var settingsMock = new Mock<IOptions<TurnstileSettings>>();
            settingsMock.SetupGet(instance => instance.Value)
                .Returns(new TurnstileSettings()
                {
                });

            // Act
            var handler = new ProxyHttpClientHandler(settingsMock.Object);

            // Assert
            Assert.IsFalse(handler.UseProxy);
            Assert.IsNull(handler.Proxy);
        }
    }
}
