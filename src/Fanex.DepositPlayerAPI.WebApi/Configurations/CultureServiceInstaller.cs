namespace Fanex.DepositPlayerAPI.WebApi.Configurations;

using Microsoft.AspNetCore.Localization;

public class CultureServiceInstaller : IServiceInstaller
{
    private static readonly string[] SupportedCultures = ["en-US", "zh-CN"];

    public static readonly IReadOnlyDictionary<string, string> LanguageCodeMappers =
        new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "en", "en-US" },
            { "cs", "zh-CN" }
        };

    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var defaultCulture = SupportedCultures[0];

            options
                .SetDefaultCulture(defaultCulture)
                .AddSupportedCultures(SupportedCultures)
                .AddSupportedUICultures(SupportedCultures);

            options.RequestCultureProviders =
            [
                new CustomRequestCultureProvider(context =>
                {
                    var query = context.Request.Query;
                    var culture = defaultCulture;

                    if (query != null && query.ContainsKey("lang"))
                    {
                        culture = LanguageCodeMappers.GetValueOrDefault(query["lang"], defaultCulture);
                    }

                    return Task.FromResult(new ProviderCultureResult(defaultCulture, culture));
                })
            ];
        });
    }
}