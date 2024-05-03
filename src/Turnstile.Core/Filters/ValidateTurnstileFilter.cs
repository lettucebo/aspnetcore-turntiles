using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AspNetCore.Turnstile.Core.Configuration;
using AspNetCore.Turnstile.Core.Extensions;
using AspNetCore.Turnstile.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("Turnstile.Tests")]
namespace AspNetCore.Turnstile.Core.Filters
{
    internal class ValidateTurnstileFilter : IValidateTurnstileFilter
    {
        private readonly ITurnstileService _turnstileService;
        private readonly TurnstileOptions _options;
        private readonly ILogger<ValidateTurnstileFilter> _logger;

        public ValidateTurnstileFilter(ITurnstileService turnstileService, IOptionsMonitor<TurnstileOptions> options,
            ILogger<ValidateTurnstileFilter> logger)
        {
            _turnstileService = turnstileService;
            _logger = logger;
            _options = options.CurrentValue;
        }

        public ValidationFailedAction OnValidationFailedAction { get; set; } = ValidationFailedAction.Unspecified;

        public string? Action { get; set; }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (OnValidationFailedAction == ValidationFailedAction.Unspecified)
            {
                OnValidationFailedAction = _options.ValidationFailedAction;
            }

            ValidationResponse validationResponse;

            if (!TryGetRecaptchaToken(context.HttpContext.Request, out string? token))
            {
                _logger.RecaptchaResponseTokenMissing();

                validationResponse = new ValidationResponse()
                {
                    Success = false,
                    ErrorMessages = new List<string>()
                    {
                        "missing-input-response"
                    }
                };
            }
            else
            {
                validationResponse = await _turnstileService.ValidateRecaptchaResponse(token, GetRemoteIp(context)).ConfigureAwait(true);
            }

            TryAddResponseToActionAguments(context, validationResponse);

            if (!ShouldShortCircuit(context, validationResponse))
            {
                await next.Invoke().ConfigureAwait(true);
            }
        }

        private string? GetRemoteIp(ActionExecutingContext context)
        {
            return _options.UseRemoteIp ?
                context.HttpContext.Connection.RemoteIpAddress?.ToString() :
                null;
        }
        private bool ShouldShortCircuit(ActionExecutingContext context, ValidationResponse response)
        {
            if (!response.Success || Action != response.Action)
            {
                _logger.InvalidResponseToken();

                if (OnValidationFailedAction == ValidationFailedAction.BlockRequest)
                {
                    context.Result = new TurnstileValidationFailedResult();
                    return true;
                }
            }

            return false;
        }
        private static bool TryGetRecaptchaToken(HttpRequest request, out string? token)
        {
            if (request.Headers.TryGetValue(TurnstileServiceConstants.TokenKeyName, out var headerVal))
            {
                token = headerVal;
            }
            else if (request.HasFormContentType && request.Form.TryGetValue(TurnstileServiceConstants.TokenKeyNameLower, out var formVal))
            {
                token = formVal;
            }
            else if (request.Query.TryGetValue(TurnstileServiceConstants.TokenKeyNameLower, out var queryVal))
            {
                token = queryVal;
            }
            else
            {
                token = null;
            }

            return token != null;
        }
        private static void TryAddResponseToActionAguments(ActionExecutingContext context, ValidationResponse response)
        {
            if (context.ActionArguments.Any(pair => pair.Value is ValidationResponse))
            {
                context.ActionArguments[context.ActionArguments.First(pair => pair.Value is ValidationResponse).Key] = response;
            }
        }
    }
}
