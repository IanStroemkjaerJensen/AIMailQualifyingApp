using Care.Web.Application.Common.Services;
using Care.Web.Application.Common.Services.Interfaces;
using Care.Web.Application.Strategies;
using Care.Web.Infrastructure.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace Care.Web.Application
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IAiMailAnalyzer, AiMailAnalyzer>()
            .AddScoped<IDumbMailFactory, MailFactory>()
            .AddScoped<IContactMailService, ContactMailService>();

            return services;
        }
    }
}
