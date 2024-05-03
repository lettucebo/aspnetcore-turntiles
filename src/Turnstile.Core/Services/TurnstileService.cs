using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AspNetCore.Turnstile.Core;
using AspNetCore.Turnstile.Core.Configuration;
using AspNetCore.Turnstile.Core.Extensions;
using AspNetCore.Turnstile.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Turnstile.Tests")]
namespace AspNetCore.Turnstile.Core.Services
{
    /// <inheritdoc />
    internal class TurnstileService : ITurnstileService
    {
        private readonly TurnstileSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TurnstileService> _logger;

        public TurnstileService(IOptionsMonitor<TurnstileSettings> settings,
            IHttpClientFactory httpClientFactory, ILogger<TurnstileService> logger)
        {
            _settings = settings.CurrentValue;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <inheritdoc />
        public ValidationResponse? ValidationResponse { get; private set; }

        /// <inheritdoc />
        public async Task<ValidationResponse> ValidateRecaptchaResponse(string token, string? remoteIp = null)
        {
            _ = token ?? throw new ArgumentNullException(nameof(token));

            try
            {
                var httpClient = _httpClientFactory.CreateClient(TurnstileServiceConstants.TurnstileServiceHttpClientName);
                var response = await httpClient.PostAsync($"?secret={_settings.SecretKey}&response={token}{(remoteIp != null ? $"&remoteip={remoteIp}" : "")}", null!)
                    .ConfigureAwait(true);

                response.EnsureSuccessStatusCode();

                ValidationResponse = JsonConvert.DeserializeObject<ValidationResponse>(
                    await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(true))
                    ?? new ValidationResponse()
                    {
                        Success = false,
                        ErrorMessages = new List<string>()
                        {
                            "response-deserialization-failed"
                        }
                    };
            }
            catch (HttpRequestException)
            {
                _logger.ValidationRequestFailed();
                ValidationResponse = new ValidationResponse()
                {
                    Success = false,
                    ErrorMessages = new List<string>()
                    {
                        "request-failed"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.ValidationRequestUnexpectedException(ex);
                throw;
            }

            return ValidationResponse;
        }
    }
}

