# What does it do?

Library that makes it easy to configure and use feature flags in Microsoft Azure.

# How to use it?

To use the library, just perform the following configuration:

**Program.cs**

```csharp
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureWebHostDefaults(webBuilder => webBuilder.ConfigureAppConfiguration((host, builder) =>
        {
            builder.AddSmartFeatureFlags(host.HostEnvironment);
        }).UseStartup<Startup>());

        builder.Build().Run();
    }
```

**Startup.cs**

```csharp
    private readonly IHostEnvironment _hostEnvironment;

    public Startup(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        ..

        services.AddSmartFeatureFlags(_hostEnvironment);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseSmartFeatureFlags(_hostEnvironment);
    }
```

**AppSettions.json**

```json
    {
        "Azure": { 
            "FeatureFlags": {
                "ConnectionString": "YourFeatureFlagsConnectionString"
            }
        }
    }
```

Then:

- When the application is running in the development environment, the invoice flags must be enabled via 'appSettiong.json', according to the attached documentation.
[Feature Flags](https://learn.microsoft.com/en-us/azure/azure-app-configuration/use-feature-flags-dotnet-core?tabs=core5x)

- When the application is running in sandbox or production, the feature flags must be created and manipulated in the following Azure address.
[Azure FeatureFlags](https://portal.azure.com/#@sitemercado.com.br/resource/subscriptions/d65f9ce3-254f-47f0-8451-ab7a531130cf/resourceGroups/rg-app-configuration/providers/Microsoft.AppConfiguration/configurationStores/sm-app-config-operacao/ff)

*OBS:* The creation of the feature flag in azure must be done manually by the developer.