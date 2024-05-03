using AspNetCore.Turnstile.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Turnstile.Tests.Extensions
{
    [TestFixture]
    public class TurnstileServiceExtensionsTests
    {
        [Test]
        public void AddTurnstileService_ShouldAddAllRequired_WithDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTurnstileService();

            // Assert
            Assert.IsTrue(services.Any(service => service.ServiceType.FullName == "Griesoft.AspNetCore.ReCaptcha.Services.ITurnstileService" && service.Lifetime == ServiceLifetime.Scoped));
            Assert.IsTrue(services.Any(service => service.ServiceType.FullName == "Griesoft.AspNetCore.ReCaptcha.Filters.IValidateTurnstileFilter" && service.Lifetime == ServiceLifetime.Transient));
            Assert.IsTrue(services.Any(service => service.ServiceType.FullName == "Microsoft.Extensions.Options.IConfigureOptions`1[[Griesoft.AspNetCore.ReCaptcha.Configuration.TurnstileOptions, Griesoft.AspNetCore.ReCaptcha, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"));
            Assert.IsTrue(services.Any(service => service.ServiceType.FullName == "Microsoft.Extensions.Options.IConfigureOptions`1[[Griesoft.AspNetCore.ReCaptcha.Configuration.TurnstileSettings, Griesoft.AspNetCore.ReCaptcha, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"));
        }
    }
}
