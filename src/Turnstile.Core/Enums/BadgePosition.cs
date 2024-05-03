namespace AspNetCore.Turnstile.Core.TagHelpers
{
    /// <summary>
    /// Recaptcha badge options for the <see cref="TurnstileInvisibleTagHelper"/>.
    /// </summary>
    public enum BadgePosition
    {
        /// <summary>
        /// Position the badge in the bottom left of your page.
        /// </summary>
        BottomLeft,

        /// <summary>
        /// Position the badge in the bottom right of your page.
        /// </summary>
        BottomRight,

        /// <summary>
        /// Use this if you want to customize the position with CSS yourself.
        /// </summary>
        Inline
    }
}
