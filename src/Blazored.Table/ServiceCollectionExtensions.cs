using Blazored.Table.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Blazored.Table
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazoredTable(this IServiceCollection services)
        {
            return services.AddScoped<ITableService, TableService>();
        }
    }
}
