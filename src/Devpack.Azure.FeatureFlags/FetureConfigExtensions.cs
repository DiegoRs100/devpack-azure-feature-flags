using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;

namespace Devpack.Azure.FeatureFlags
{
    public static class FetureConfigExtensions
    {
        public static IConfigurationBuilder AddSmartFeatureFlags(this IConfigurationBuilder builder, IHostEnvironment environment)
        {
            if (environment.IsDevelopment())
                return builder;

            var configuration = builder.Build();

            return builder.AddAzureAppConfiguration(options => options
                .Connect(configuration["Azure:FeatureFlags:ConnectionString"])
                .UseFeatureFlags(ff => ff.Label = environment.EnvironmentName));
        }

        public static IServiceCollection AddSmartFeatureFlags(this IServiceCollection services, IHostEnvironment environment)
        {
            services.AddFeatureManagement();

            if (!environment.IsDevelopment())
                services.AddAzureAppConfiguration();

            return services;
        }

        public static IApplicationBuilder UseSmartFeatureFlags(this IApplicationBuilder application, IHostEnvironment environment)
        {
            if (environment.IsDevelopment())
                return application;

            return application.UseAzureAppConfiguration();
        }
    }
}