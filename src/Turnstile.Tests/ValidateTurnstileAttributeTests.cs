using AspNetCore.Turnstile.Core;
using AspNetCore.Turnstile.Core.Configuration;
using AspNetCore.Turnstile.Core.Filters;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Turnstile.Tests
{
    [TestFixture]
    public class ValidateTurnstileAttributeTests
    {
        [Test(Description = "CreateInstance(...) should throw InvalidOperationException if the library services are not registered.")]
        public void CreateInstance_ShouldThrowWhen_ServicesNotRegistered()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(provider => provider.GetService(typeof(ValidateTurnstileFilter)))
                .Returns(null);
            var attribute = new ValidateTurnstileAttribute();

            // Act


            // Assert
            Assert.Throws<InvalidOperationException>(() => attribute.CreateInstance(servicesMock.Object));
        }

        [Test(Description = "CreateInstance(...) should return a new instance of " +
            "ValidateTurnstileFilter with the default value for the OnValidationFailedAction property.")]
        public void CreateInstance_ShouldReturn_ValidateRecaptchaFilter_WithDefaultOnValidationFailedAction()
        {
            // Arrange
            var optionsMock = new Mock<IOptionsMonitor<TurnstileOptions>>();
            optionsMock.SetupGet(options => options.CurrentValue)
                .Returns(new TurnstileOptions());
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(provider => provider.GetService(typeof(IValidateTurnstileFilter)))
                .Returns(new ValidateTurnstileFilter(null, optionsMock.Object, null))
                .Verifiable();
            var attribute = new ValidateTurnstileAttribute();

            // Act
            var filterInstance = attribute.CreateInstance(servicesMock.Object);

            // Assert
            servicesMock.Verify();
            Assert.IsNotNull(filterInstance);
            Assert.IsInstanceOf<ValidateTurnstileFilter>(filterInstance);
            Assert.AreEqual(ValidationFailedAction.Unspecified, (filterInstance as ValidateTurnstileFilter).OnValidationFailedAction);
        }

        [Test(Description = "CreateInstance(...) should return a new instance of " +
            "ValidateTurnstileFilter with the user set value for the OnValidationFailedAction property.")]
        public void CreateInstance_ShouldReturn_ValidateRecaptchaFilter_WithUserSetOnValidationFailedAction()
        {
            // Arrange
            var optionsMock = new Mock<IOptionsMonitor<TurnstileOptions>>();
            optionsMock.SetupGet(options => options.CurrentValue)
                .Returns(new TurnstileOptions());
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(provider => provider.GetService(typeof(IValidateTurnstileFilter)))
                .Returns(new ValidateTurnstileFilter(null, optionsMock.Object, null))
                .Verifiable();
            var attribute = new ValidateTurnstileAttribute
            {
                ValidationFailedAction = ValidationFailedAction.ContinueRequest
            };

            // Act
            var filterInstance = attribute.CreateInstance(servicesMock.Object);

            // Assert
            servicesMock.Verify();
            Assert.IsNotNull(filterInstance);
            Assert.IsInstanceOf<ValidateTurnstileFilter>(filterInstance);
            Assert.AreEqual(ValidationFailedAction.ContinueRequest, (filterInstance as ValidateTurnstileFilter).OnValidationFailedAction);
        }

        [Test]
        public void CreateInstance_ShouldReturn_ValidateRecaptchaFilter_WithUserSetAction()
        {
            // Arrange
            var action = "submit";
            var optionsMock = new Mock<IOptionsMonitor<TurnstileOptions>>();
            optionsMock.SetupGet(options => options.CurrentValue)
                .Returns(new TurnstileOptions());
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(provider => provider.GetService(typeof(IValidateTurnstileFilter)))
                .Returns(new ValidateTurnstileFilter(null, optionsMock.Object, null))
                .Verifiable();
            var attribute = new ValidateTurnstileAttribute
            {
                Action = action
            };

            // Act
            var filterInstance = attribute.CreateInstance(servicesMock.Object);

            // Assert
            servicesMock.Verify();
            Assert.IsNotNull(filterInstance);
            Assert.IsInstanceOf<ValidateTurnstileFilter>(filterInstance);
            Assert.AreEqual(action, (filterInstance as ValidateTurnstileFilter).Action);
        }
    }
}
