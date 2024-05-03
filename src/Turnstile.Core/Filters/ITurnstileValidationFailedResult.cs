using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Turnstile.Core.Filters
{
    /// <summary>
    /// Represents an <see cref="IActionResult"/> that is used when the reCAPTCHA validation failed. 
    /// This can be matched inside MVC result filters to process the validation failure.
    /// </summary>
    public interface ITurnstileValidationFailedResult : IActionResult
    {
    }
}
