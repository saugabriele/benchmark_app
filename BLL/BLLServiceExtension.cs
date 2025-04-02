using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DAL;

namespace BLL
{
    public static class BLLServiceExtension
    {
        public static IServiceCollection AddBLLServices(this IServiceCollection services, string connectionString)
        {
            /*
             * Adds Business Logic Layer (BLL) services to the dependency injection container.
             * 
             * Parameters:
             *    services (IServiceCollection): The service collection to configure.
             *    connectionString (string): The database connection string.
             * 
             * Returns:
             *    IServiceCollection: The updated service collection with DAL and BLL services.
             */
            services.AddDALServices(connectionString);

            services.AddScoped<UserManager>();
            services.AddScoped<FileManager>();

            return services;
        }

        public static AppDbContext GetDbContext(IServiceProvider serviceProvider)
        {
            /* 
             * This method retrieves an instance of AppDbContext from the service provider.
             * 
             * Parameters:
             *     serviceProvider (IServiceProvide): The service provider used to resolve services.
             * 
             * Returns:
             *      AppDbContext: The instance of AppDbContext retrieved from the service provider.
             */
            return serviceProvider.GetRequiredService<AppDbContext>();
        }
    }
}
