using AspNetCore.Turnstile.Core.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace Turnstile.Tests.Filters
{
    [TestFixture]
    public class TurnstileValidationFailedResultTests
    {
        [Test]
        public void Construction_IsSuccessful()
        {
            // Arrange


            // Act
            var result = new TurnstileValidationFailedResult();

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ExecuteResultAsync_ShouldThrow_ArgmentNullException()
        {
            // Arrange
            var result = new TurnstileValidationFailedResult();

            // Act


            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => result.ExecuteResultAsync(null));
        }

        [Test]
        public async Task ExecuteResultAsync_ShouldSet_StatusCode_400()
        {
            // Arrange
            var context = new ActionContext(new DefaultHttpContext(), Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>(), new ModelStateDictionary());

            var result = new TurnstileValidationFailedResult();

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            Assert.AreEqual(400, context.HttpContext.Response.StatusCode);
        }

        [Test]
        public void ExecuteResultAsync_ShouldReturn_CompletedTask()
        {
            // Arrange
            var context = new ActionContext(new DefaultHttpContext(), Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>(), new ModelStateDictionary());

            var result = new TurnstileValidationFailedResult();

            // Act
            var response = result.ExecuteResultAsync(context);

            // Assert
            Assert.IsTrue(response.IsCompleted);
        }
    }
}
