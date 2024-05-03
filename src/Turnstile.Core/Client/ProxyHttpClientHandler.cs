using System;
using System.Net;
using System.Net.Http;
using AspNetCore.Turnstile.Core.Configuration;
using Microsoft.Extensions.Options;

namespace AspNetCore.Turnstile.Core.Client
{

    /// <summary>
    /// HttpClientHandler utilizing proxy server if such configuration was provided
    /// </summary>
    public class ProxyHttpClientHandler : HttpClientHandler
    {
        /// <summary>
        /// Create HttpHandler configured by TurnstileSettings
        /// </summary>
        /// <param name="settings">Recaptcha Settings sepcifying proxy configuration</param>
        public ProxyHttpClientHandler(IOptions<TurnstileSettings> settings)
        {
            var currentSettings = settings?.Value;

            if (currentSettings != null && currentSettings.UseProxy == true && !String.IsNullOrEmpty(currentSettings.ProxyAddress))
            {
                this.UseProxy = true;
                this.Proxy = new WebProxy(currentSettings.ProxyAddress, currentSettings.BypassOnLocal);
            }
            else
            {
                this.UseProxy = false;
            }
        }
    }
}
