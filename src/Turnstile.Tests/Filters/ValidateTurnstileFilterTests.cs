﻿using System.Net;
using AspNetCore.Turnstile.Core;
using AspNetCore.Turnstile.Core.Configuration;
using AspNetCore.Turnstile.Core.Filters;
using AspNetCore.Turnstile.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;

namespace Turnstile.Tests.Filters
{
    [TestFixture]
    public class ValidateTurnstileFilterTests
    {
        private const string TokenValue = "test";

        private ILogger<ValidateTurnstileFilter> _logger;
        private Mock<IOptionsMonitor<TurnstileOptions>> _optionsMock;
        private Mock<ITurnstileService> _TurnstileServiceMock;
        private ActionContext _actionContext;
        private ActionExecutingContext _actionExecutingContext;
        private ActionExecutionDelegate _actionExecutionDelegate;
        private ValidateTurnstileFilter _filter;

        [SetUp]
        public void InitializeTest()
        {
            _logger = new LoggerFactory().CreateLogger<ValidateTurnstileFilter>();

            _optionsMock = new Mock<IOptionsMonitor<TurnstileOptions>>();
            _optionsMock.SetupGet(options => options.CurrentValue)
                .Returns(new TurnstileOptions());

            _TurnstileServiceMock = new Mock<ITurnstileService>();
            _TurnstileServiceMock.Setup(service => service.ValidateRecaptchaResponse(It.Is<string>(s => s == TokenValue), null))
                .ReturnsAsync(new ValidationResponse
                {
                    Success = true
                })
                .Verifiable();

            _actionContext = new ActionContext(
                new DefaultHttpContext(),
                Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>(),
                new ModelStateDictionary());

            _actionExecutingContext = new ActionExecutingContext(_actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), Mock.Of<Controller>())
            {
                Result = new OkResult() // It will return ok unless during code execution you change this when by condition
            };

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            _actionExecutionDelegate = async () => { return new ActionExecutedContext(_actionContext, new List<IFilterMetadata>(), Mock.Of<Controller>()); };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            _filter = new ValidateTurnstileFilter(_TurnstileServiceMock.Object, _optionsMock.Object, _logger);
        }

        [Test]
        public void Construction_IsSuccessful()
        {
            // Arrange


            // Act
            var instance = new ValidateTurnstileFilter(_TurnstileServiceMock.Object, _optionsMock.Object, _logger);

            // Assert
            Assert.NotNull(instance);
        }

        [Test(Description = "The default value for OnValidationFailedAction when creating a new instance of the filter " +
            "is Undefined. So we run the method and verify that the value was changed to something else.")]
        public async Task OnActionExecutionAsync_OnValidationFailedAction_IsNeverUndefined()
        {
            // Arrange


            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            Assert.AreEqual(ValidationFailedAction.BlockRequest, _filter.OnValidationFailedAction);
        }

        [Test]
        public async Task OnActionExecutionAsync_TokenIsExtracted_FromHeader()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add(TurnstileServiceConstants.TokenKeyName, TokenValue);

            _actionExecutingContext.HttpContext = httpContext;

            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            _TurnstileServiceMock.Verify();
            Assert.IsInstanceOf<OkResult>(_actionExecutingContext.Result);
        }

        [Test]
        public async Task OnActionExecutionAsync_TokenIsExtracted_FromQueryParameter()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(
                new Dictionary<string, StringValues>() 
                { 
                    { TurnstileServiceConstants.TokenKeyName.ToLowerInvariant(), new StringValues(TokenValue) }
                } 
            );

            _actionExecutingContext.HttpContext = httpContext;

            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            _TurnstileServiceMock.Verify();
            Assert.IsInstanceOf<OkResult>(_actionExecutingContext.Result);
        }

        [Test]
        public async Task OnActionExecutionAsync_TokenIsExtracted_FromFormData()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Form = new FormCollection(
                new Dictionary<string, StringValues>()
                {
                    { TurnstileServiceConstants.TokenKeyName.ToLowerInvariant(), new StringValues(TokenValue) }
                }
            );

            _actionExecutingContext.HttpContext = httpContext;

            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            _TurnstileServiceMock.Verify();
            Assert.IsInstanceOf<OkResult>(_actionExecutingContext.Result);
        }

        [Test]
        public async Task OnActionExecutionAsync_WhenTokenNotFound_BlocksAndReturns_RecaptchaValidationFailedResult()
        {
            // Arrange


            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            _TurnstileServiceMock.VerifyNoOtherCalls();
            Assert.IsInstanceOf<ITurnstileValidationFailedResult>(_actionExecutingContext.Result);
        }

        [Test]
        public async Task OnActionExecutionAsync_WhenTokenNotFound_ContinuesAndAddsResponseToArguments()
        {
            // Arrange
            _filter.OnValidationFailedAction = ValidationFailedAction.ContinueRequest;
            _actionExecutingContext.ActionArguments.Add("argumentName", new ValidationResponse { Success = true });

            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            _TurnstileServiceMock.VerifyNoOtherCalls();
            Assert.IsInstanceOf<OkResult>(_actionExecutingContext.Result);
            Assert.IsFalse((_actionExecutingContext.ActionArguments["argumentName"] as ValidationResponse).Success);
            Assert.GreaterOrEqual((_actionExecutingContext.ActionArguments["argumentName"] as ValidationResponse).Errors.Count(), 1);
            Assert.AreEqual(ValidationError.MissingInputResponse, (_actionExecutingContext.ActionArguments["argumentName"] as ValidationResponse).Errors.First());
        }

        [Test]
        public async Task OnActionExecutionAsync_WhenValidationFailed_BlocksAndReturns_RecaptchaValidationFailedResult()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add(TurnstileServiceConstants.TokenKeyName, TokenValue);

            _actionExecutingContext.HttpContext = httpContext;

            _TurnstileServiceMock = new Mock<ITurnstileService>();
            _TurnstileServiceMock.Setup(service => service.ValidateRecaptchaResponse(It.Is<string>(s => s == TokenValue), null))
                .ReturnsAsync(new ValidationResponse
                {
                    Success = false
                })
                .Verifiable();

            _filter = new ValidateTurnstileFilter(_TurnstileServiceMock.Object, _optionsMock.Object, _logger);

            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            _TurnstileServiceMock.Verify();
            Assert.IsInstanceOf<ITurnstileValidationFailedResult>(_actionExecutingContext.Result);
        }

        [Test]
        public async Task OnActionExecutionAsync_WhenValidationFailed_ContinuesAndAddsResponseToArguments()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add(TurnstileServiceConstants.TokenKeyName, TokenValue);

            _actionExecutingContext.HttpContext = httpContext;

            _TurnstileServiceMock = new Mock<ITurnstileService>();
            _TurnstileServiceMock.Setup(service => service.ValidateRecaptchaResponse(It.Is<string>(s => s == TokenValue), null))
                .ReturnsAsync(new ValidationResponse
                {
                    Success = false,
                    ErrorMessages = new List<string> { "invalid-input-response" }
                })
                .Verifiable();

            _filter = new ValidateTurnstileFilter(_TurnstileServiceMock.Object, _optionsMock.Object, _logger)
            {
                OnValidationFailedAction = ValidationFailedAction.ContinueRequest
            };

            _actionExecutingContext.ActionArguments.Add("argumentName", new ValidationResponse { Success = true });

            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            _TurnstileServiceMock.Verify();
            Assert.IsInstanceOf<OkResult>(_actionExecutingContext.Result);
            Assert.IsFalse((_actionExecutingContext.ActionArguments["argumentName"] as ValidationResponse).Success);
            Assert.GreaterOrEqual((_actionExecutingContext.ActionArguments["argumentName"] as ValidationResponse).Errors.Count(), 1);
            Assert.AreEqual(ValidationError.InvalidInputResponse, (_actionExecutingContext.ActionArguments["argumentName"] as ValidationResponse).Errors.First());
        }

        [Test]
        public async Task OnActionExecutionAsync_WhenActionDoesNotMatch_BlocksAndReturns_RecaptchaValidationFailedResult()
        {
            // Arrange
            var action = "submit";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add(TurnstileServiceConstants.TokenKeyName, TokenValue);

            _actionExecutingContext.HttpContext = httpContext;

            _TurnstileServiceMock = new Mock<ITurnstileService>();
            _TurnstileServiceMock.Setup(service => service.ValidateRecaptchaResponse(It.Is<string>(s => s == TokenValue), null))
                .ReturnsAsync(new ValidationResponse
                {
                    Success = true
                })
                .Verifiable();

            _filter = new ValidateTurnstileFilter(_TurnstileServiceMock.Object, _optionsMock.Object, _logger);
            _filter.Action = action;

            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            _TurnstileServiceMock.Verify();
            Assert.IsInstanceOf<ITurnstileValidationFailedResult>(_actionExecutingContext.Result);
        }

        [Test]
        public async Task OnActionExecutionAsync_WhenActionDoesNotMatch_ContinuesAndAddsResponseToArguments()
        {
            // Arrange
            var action = "submit";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add(TurnstileServiceConstants.TokenKeyName, TokenValue);

            _actionExecutingContext.HttpContext = httpContext;

            _TurnstileServiceMock = new Mock<ITurnstileService>();
            _TurnstileServiceMock.Setup(service => service.ValidateRecaptchaResponse(It.Is<string>(s => s == TokenValue), null))
                .ReturnsAsync(new ValidationResponse
                {
                    Success = true,
                    ErrorMessages = new List<string> { "invalid-input-response" }
                })
                .Verifiable();

            _filter = new ValidateTurnstileFilter(_TurnstileServiceMock.Object, _optionsMock.Object, _logger)
            {
                OnValidationFailedAction = ValidationFailedAction.ContinueRequest,
                Action = action
            };

            _actionExecutingContext.ActionArguments.Add("argumentName", new ValidationResponse { Success = true });

            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            _TurnstileServiceMock.Verify();
            Assert.IsInstanceOf<OkResult>(_actionExecutingContext.Result);
            Assert.IsTrue((_actionExecutingContext.ActionArguments["argumentName"] as ValidationResponse).Success);
            Assert.GreaterOrEqual((_actionExecutingContext.ActionArguments["argumentName"] as ValidationResponse).Errors.Count(), 1);
            Assert.AreEqual(ValidationError.InvalidInputResponse, (_actionExecutingContext.ActionArguments["argumentName"] as ValidationResponse).Errors.First());
        }

        [Test]
        public async Task OnActionExecutionAsync_WhenValidationSuccess_ContinuesAndAddsResponseToArguments()
        {
            // Arrange
            var action = "submit";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add(TurnstileServiceConstants.TokenKeyName, TokenValue);

            _actionExecutingContext.HttpContext = httpContext;

            _TurnstileServiceMock = new Mock<ITurnstileService>();
            _TurnstileServiceMock.Setup(service => service.ValidateRecaptchaResponse(It.Is<string>(s => s == TokenValue), null))
                .ReturnsAsync(new ValidationResponse
                {
                    Success = true,
                    Action = action
                })
                .Verifiable();

            _filter = new ValidateTurnstileFilter(_TurnstileServiceMock.Object, _optionsMock.Object, _logger)
            {
                Action = action
            };

            _actionExecutingContext.ActionArguments.Add("argumentName", new ValidationResponse { Success = false, Action = string.Empty });

            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            _TurnstileServiceMock.Verify();
            Assert.IsInstanceOf<OkResult>(_actionExecutingContext.Result);
            Assert.IsTrue((_actionExecutingContext.ActionArguments["argumentName"] as ValidationResponse).Success);
            Assert.AreEqual(action, (_actionExecutingContext.ActionArguments["argumentName"] as ValidationResponse).Action);
            Assert.AreEqual((_actionExecutingContext.ActionArguments["argumentName"] as ValidationResponse).Errors.Count(), 0);
        }

        [Test]
        public async Task OnActionExecutionAsync_WhenValidationSuccess_ReturnsOkResult_WithoutAddingResponseToArguments()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add(TurnstileServiceConstants.TokenKeyName, TokenValue);

            _actionExecutingContext.HttpContext = httpContext;

            _TurnstileServiceMock = new Mock<ITurnstileService>();
            _TurnstileServiceMock.Setup(service => service.ValidateRecaptchaResponse(It.Is<string>(s => s == TokenValue), null))
                .ReturnsAsync(new ValidationResponse
                {
                    Success = true
                })
                .Verifiable();

            _filter = new ValidateTurnstileFilter(_TurnstileServiceMock.Object, _optionsMock.Object, _logger);

            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            _TurnstileServiceMock.Verify();
            Assert.IsInstanceOf<OkResult>(_actionExecutingContext.Result);
            Assert.AreEqual(0, _actionExecutingContext.ActionArguments.Count);
        }

        [Test]
        public async Task OnActionExecutionAsync_ShouldAddRemoteIp()
        {
            // Arrange
            var ip = "144.22.5.213";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add(TurnstileServiceConstants.TokenKeyName, TokenValue);
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse(ip);

            _actionExecutingContext.HttpContext = httpContext;

            _optionsMock.SetupGet(options => options.CurrentValue)
                .Returns(new TurnstileOptions { UseRemoteIp = true });

            _TurnstileServiceMock = new Mock<ITurnstileService>();
            _TurnstileServiceMock.Setup(service => service.ValidateRecaptchaResponse(It.Is<string>(s => s == TokenValue), It.Is<string>(s => s == ip)))
                .ReturnsAsync(new ValidationResponse
                {
                    Success = true
                })
                .Verifiable();

            _filter = new ValidateTurnstileFilter(_TurnstileServiceMock.Object, _optionsMock.Object, _logger);

            // Act
            await _filter.OnActionExecutionAsync(_actionExecutingContext, _actionExecutionDelegate);

            // Assert
            _TurnstileServiceMock.Verify();
            Assert.IsInstanceOf<OkResult>(_actionExecutingContext.Result);
            Assert.AreEqual(0, _actionExecutingContext.ActionArguments.Count);
        }
    }
}
