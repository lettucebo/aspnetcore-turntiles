using System.Net;
using AspNetCore.Turnstile.Core;
using AspNetCore.Turnstile.Core.Configuration;
using AspNetCore.Turnstile.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Turnstile.Tests.Services
{
    [TestFixture]
    public class TurnstileServiceTests
    {
        private const string SiteKey = "sitekey";
        private const string SecretKey = "verysecretkey";
        private const string Token = "testtoken";

        private ILogger<TurnstileService> _logger;
        private Mock<IOptionsMonitor<TurnstileSettings>> _settingsMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private Mock<IHttpClientFactory> _httpClientFactory;

        [SetUp]
        public void Initialize()
        {
            _logger = new LoggerFactory().CreateLogger<TurnstileService>();

            _settingsMock = new Mock<IOptionsMonitor<TurnstileSettings>>();
            _settingsMock.SetupGet(instance => instance.CurrentValue)
                .Returns(new TurnstileSettings()
                {
                    SiteKey = SiteKey,
                    SecretKey = SecretKey
                })
                .Verifiable();

            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpMessageHandlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{'success': true, 'challenge_ts': '" + DateTime.UtcNow.ToString("o") + "', 'hostname': 'https://test.com', 'error-codes': []}")
               })
               .Verifiable();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            _httpClientFactory = new Mock<IHttpClientFactory>();
            _httpClientFactory.Setup(instance => instance.CreateClient(It.Is<string>(val => val == TurnstileServiceConstants.TurnstileServiceHttpClientName)))
                .Returns(_httpClient)
                .Verifiable();
        }

        [Test]
        public void Construction_IsSuccessful()
        {
            // Arrange


            // Act
            var instance = new TurnstileService(_settingsMock.Object, _httpClientFactory.Object, _logger);

            // Assert
            Assert.NotNull(instance);
            _settingsMock.Verify(settings => settings.CurrentValue, Times.Once);
        }

        [Test]
        public void ValidateRecaptchaResponse_ShouldThrow_ArgumentNullException()
        {
            // Arrange
            var service = new TurnstileService(_settingsMock.Object, _httpClientFactory.Object, _logger);

            // Act


            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => service.ValidateRecaptchaResponse(null));
        }

        [Test]
        public async Task ValidateRecaptchaResponse_Should_CreateNamedHttpClient()
        {
            // Arrange
            var service = new TurnstileService(_settingsMock.Object, _httpClientFactory.Object, _logger);

            // Act
            var response = await service.ValidateRecaptchaResponse(Token);

            // Assert
            _httpClientFactory.Verify();
        }

        [Test]
        public async Task ValidateRecaptchaResponse_ShouldReturn_HttpRequestError()
        {
            // Arrange
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpMessageHandlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.BadRequest
               })
               .Verifiable();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            _httpClientFactory = new Mock<IHttpClientFactory>();
            _httpClientFactory.Setup(instance => instance.CreateClient(It.Is<string>(val => val == TurnstileServiceConstants.TurnstileServiceHttpClientName)))
                .Returns(_httpClient);

            var service = new TurnstileService(_settingsMock.Object, _httpClientFactory.Object, _logger);

            // Act
            var response = await service.ValidateRecaptchaResponse(Token);

            // Assert
            _httpMessageHandlerMock.Verify();
            Assert.GreaterOrEqual(response.Errors.Count(), 1);
            Assert.AreEqual(ValidationError.HttpRequestFailed, response.Errors.First());
        }

        [Test]
        public void ValidateRecaptchaResponse_ShouldThrowAnyOtherThan_HttpRequestException()
        {
            // Arrange
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpMessageHandlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ThrowsAsync(new Exception())
               .Verifiable();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            _httpClientFactory = new Mock<IHttpClientFactory>();
            _httpClientFactory.Setup(instance => instance.CreateClient(It.Is<string>(val => val == TurnstileServiceConstants.TurnstileServiceHttpClientName)))
                .Returns(_httpClient);

            var service = new TurnstileService(_settingsMock.Object, _httpClientFactory.Object, _logger);

            // Act


            // Assert
            Assert.ThrowsAsync<Exception>(() => service.ValidateRecaptchaResponse(Token));
            _httpMessageHandlerMock.Verify();
        }

        [Test]
        public async Task ValidateRecaptchaResponse_ShouldReturn_DeserializedResponse()
        {
            // Arrange
            var service = new TurnstileService(_settingsMock.Object, _httpClientFactory.Object, _logger);

            // Act
            var response = await service.ValidateRecaptchaResponse(Token);

            // Assert
            _httpMessageHandlerMock.Verify();
            Assert.IsTrue(response.Success);
            Assert.AreEqual(0, response.Errors.Count());
        }

        [Test]
        public async Task ValidateRecaptchaResponse_Should_DeserializedResponse_AndSet_ValidationResponseProperty()
        {
            // Arrange
            var service = new TurnstileService(_settingsMock.Object, _httpClientFactory.Object, _logger);

            // Act
            var response = await service.ValidateRecaptchaResponse(Token);

            // Assert
            _httpMessageHandlerMock.Verify();
            Assert.IsTrue(response.Success);
            Assert.AreEqual(0, response.Errors.Count());
            Assert.NotNull(service.ValidationResponse);
        }
    }
}
