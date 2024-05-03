using System;

namespace AspNetCore.Turnstile.Core.TagHelpers
{
    /// <summary>
    /// Recaptcha rendering options for the <see cref="TurnstileScriptTagHelper"/>.
    /// </summary>
    [Flags]
    public enum Render
    {
        /// <summary>
        /// The default rendering option. This will render your V2 reCAPTCHA elements automatically after the script has been loaded.
        /// </summary>
        Onload,

        /// <summary>
        /// When rendering your reCAPTCHA elements explicitly a given onloadCallback will be called after the script has been loaded.
        /// </summary>
        Explicit,

        /// <summary>
        /// Loads the reCAPTCHA V3 script.
        /// </summary>
        V3
    }
}
