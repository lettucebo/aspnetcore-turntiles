using AspNetCore.Turnstile.Core;
using AspNetCore.Turnstile.Core.Configuration;
using NUnit.Framework;

namespace Turnstile.Tests.Configuration
{
    [TestFixture]
    public class TurnstileOptionsTests
    {
        [Test]
        public void ValidationFailedAction_ShouldNeverReturn_ValidationFailedActionUnspecified()
        {
            // Arrange
            var optionsUnmodified = new TurnstileOptions();
            var optionsModified = new TurnstileOptions();

            // Act
            optionsModified.ValidationFailedAction = ValidationFailedAction.Unspecified;

            // Assert
            Assert.AreNotEqual(ValidationFailedAction.Unspecified, optionsUnmodified.ValidationFailedAction);
            Assert.AreNotEqual(ValidationFailedAction.Unspecified, optionsModified.ValidationFailedAction);
        }
    }
}
