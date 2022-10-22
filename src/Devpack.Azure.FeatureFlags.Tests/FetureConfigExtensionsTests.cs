using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Moq;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace Devpack.Azure.FeatureFlags.Tests
{
    public class FetureConfigExtensionsTests
    {
        private readonly Mock<IHostEnvironment> _hostEnvironmentMock;

        private readonly string _azureConenctionStringConfig;
            
        public FetureConfigExtensionsTests()
        {
            _hostEnvironmentMock = new Mock<IHostEnvironment>();

            _azureConenctionStringConfig = @"
                {
                    ""Azure"": { 
                        ""FeatureFlags"": {
                            ""ConnectionString"": ""Endpoint=https://mock.azconfig.io;Id=gYiH-le-s0:Yi3EP8VGk0mKRXDpYri;Secret=NGIxZmZhMzctMTQ3MC00Njk2LThlYWEtOGFkODMwNGUzOTBl""
                        }
                    }
                }";
        }

        [Fact(DisplayName = "Não deve implementar as configurações do azure quando o ambiente for de desenvolvimento - ConfigurationBuilder.")]
        [Trait("Category", "extensions")]
        public void AddSmartFeatureFlags_ConfigurationBuilder_WhenDevelopment()
        {
            _hostEnvironmentMock.SetupGet(m => m.EnvironmentName).Returns("Development");

            var builder = new ConfigurationBuilder();
            var response = builder.AddSmartFeatureFlags(_hostEnvironmentMock.Object);

            response.Sources.Should().BeEmpty();
        }

        [Theory(DisplayName = "Deve implementar as configurações do azure quando o ambiente for diferente de desenvolvimento - ConfigurationBuilder.")]
        [InlineData("Production")]
        [InlineData("Sandbox")]
        [Trait("Category", "extensions")]
        public void AddSmartFeatureFlags_ConfigurationBuilder_WhenNotDevelopment(string environmentkey)
        {
            _hostEnvironmentMock.SetupGet(m => m.EnvironmentName).Returns(environmentkey);

            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(_azureConenctionStringConfig)));

            var response = builder.AddSmartFeatureFlags(_hostEnvironmentMock.Object);

            builder.Invoking(c => c.Build()).Should().Throw<ArgumentException>();
            response.Sources.Should().HaveCount(2);
        }

        [Fact(DisplayName = "Não deve implementar as configurações do azure quando o ambiente for de desenvolvimento - ServiceCollection.")]
        [Trait("Category", "extensions")]
        public void AddSmartFeatureFlags_ServiceCollection_WhenDevelopment()
        {
            // Arrange
            _hostEnvironmentMock.SetupGet(m => m.EnvironmentName).Returns("Development");

            var services = new ServiceCollection();
            services.AddSingleton(_ => new Mock<IConfiguration>().Object);

            // act
            services.AddSmartFeatureFlags(_hostEnvironmentMock.Object);

            var serviceProvider = services.BuildServiceProvider();
            var featureProvider = serviceProvider.GetService<IFeatureDefinitionProvider>();
            var configurationProvider = serviceProvider.GetService<IConfigurationRefresherProvider>();

            // Asserts
            featureProvider.Should().NotBeNull();
            configurationProvider.Should().BeNull();
        }

        [Theory(DisplayName = "Deve implementar as configurações do azure quando o ambiente for diferente de desenvolvimento - ServiceCollection.")]
        [InlineData("Production")]
        [InlineData("Sandbox")]
        [Trait("Category", "extensions")]
        public void AddSmartFeatureFlags_ServiceCollection_WhenNotDevelopment(string environmentkey)
        {
            // Arrange
            _hostEnvironmentMock.SetupGet(m => m.EnvironmentName).Returns(environmentkey);

            var services = new ServiceCollection();
            services.AddSingleton(_ => new Mock<IConfiguration>().Object);

            // act
            services.AddSmartFeatureFlags(_hostEnvironmentMock.Object);

            var serviceProvider = services.BuildServiceProvider();
            var featureProvider = serviceProvider.GetService<IFeatureDefinitionProvider>();

            // Asserts
            featureProvider.Should().NotBeNull();

            serviceProvider.Invoking(s => s.GetService<IConfigurationRefresherProvider>()).Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Unable to access the Azure App Configuration provider. Please ensure that it has been configured correctly.");
        }

        [Fact(DisplayName = "Não deve implementar as configurações do azure quando o ambiente for de desenvolvimento - ApplicationBuilder.")]
        [Trait("Category", "extensions")]
        public void UseSmartFeatureFlags_WhenDevelopment()
        {
            _hostEnvironmentMock.SetupGet(m => m.EnvironmentName).Returns("Development");

            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var application = new ApplicationBuilder(serviceProvider);

            application.Invoking(a => a.UseSmartFeatureFlags(_hostEnvironmentMock.Object)).Should().NotThrow();
        }

        [Theory(DisplayName = "Deve implementar as configurações do azure quando o ambiente for diferente de desenvolvimento - ApplicationBuilder.")]
        [InlineData("Production")]
        [InlineData("Sandbox")]
        public void UseSmartFeatureFlags_WhenNotDevelopment(string environmentkey)
        {
            _hostEnvironmentMock.SetupGet(m => m.EnvironmentName).Returns(environmentkey);

            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var application = new ApplicationBuilder(serviceProvider);

            application.Invoking(a => a.UseSmartFeatureFlags(_hostEnvironmentMock.Object)).Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Unable to find the required services. Please add all the required services by calling 'IServiceCollection.AddAzureAppConfiguration' inside the call to 'ConfigureServices(...)' in the application startup code.");
        }
    }
}