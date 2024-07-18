namespace Fanex.DepositPlayerAPI.WebApi.Configurations;

using Microsoft.OpenApi.Models;

public class SwaggerServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Deposit API"
            });
        });
    }
}