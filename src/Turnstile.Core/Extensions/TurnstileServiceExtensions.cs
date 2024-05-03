using System;
using AspNetCore.Turnstile.Core.Services;
using AspNetCore.Turnstile.Core.Client;
using AspNetCore.Turnstile.Core.Configuration;
using AspNetCore.Turnstile.Core.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Turnstile.Core.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods for easy service registration in the StartUp.cs.
    /// </summary>
    public static class TurnstileServiceExtensions
    {
        /// <summary>
        /// Register the <see cref="TurnstileService"/> to the web project and all it's dependencies.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options">Specify global options for the service.</param>
        /// <returns></returns>
        public static IServiceCollection AddTurnstileService(this IServiceCollection services, Action<TurnstileOptions>? options = null)
        {
            services.AddOptions<TurnstileSettings>()
                    .Configure<IConfiguration>((settings, config) =>
                     config.GetSection(TurnstileServiceConstants.SettingsSectionKey)
                    .Bind(settings, (op) => op.BindNonPublicProperties = true));

            services.Configure(options ??= opt => { });

            services.AddScoped<ProxyHttpClientHandler, ProxyHttpClientHandler>();

            services.AddHttpClient(TurnstileServiceConstants.TurnstileServiceHttpClientName, client =>
            {
                client.BaseAddress = new Uri(TurnstileServiceConstants.GoogleRecaptchaEndpoint);
            })
            .ConfigurePrimaryHttpMessageHandler<ProxyHttpClientHandler>();


            services.AddScoped<ITurnstileService, TurnstileService>();

            services.AddTransient<IValidateTurnstileFilter, ValidateTurnstileFilter>();

            return services;
        }
    }
}
