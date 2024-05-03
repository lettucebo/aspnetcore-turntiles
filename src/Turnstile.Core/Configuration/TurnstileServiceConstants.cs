namespace AspNetCore.Turnstile.Core.Configuration
{
    /// <summary>
    /// Constant values for this service.
    /// </summary>
    public class TurnstileServiceConstants
    {
        /// <summary>
        /// The validation endpoint.
        /// </summary>
        public const string GoogleRecaptchaEndpoint = "https://www.google.com/recaptcha/api/siteverify";

        /// <summary>
        /// The header key name under which the token is stored.
        /// </summary>
        public const string TokenKeyName = "G-Recaptcha-Response";

        /// <summary>
        /// The header key name under which the token is stored in lower case.
        /// </summary>
        public const string TokenKeyNameLower = "g-recaptcha-response";

        /// <summary>
        /// The section name in the appsettings.json from which the settings are read.
        /// </summary>
        public const string SettingsSectionKey = "TurnstileSettings";

        /// <summary>
        /// The named HttpClient name that we use in the IRecpatchaService.
        /// </summary>
        public const string TurnstileServiceHttpClientName = "ReCaptchaValidationClient";
    }
}
