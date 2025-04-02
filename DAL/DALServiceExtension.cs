using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public static class DALServiceExtensions
    {
        public static IServiceCollection AddDALServices(this IServiceCollection services, string connectionString)
        {
            /*
             * Adds Data Access Layer (DAL) services to the dependency injection container.
             * 
             * Parameters:
             *    services (IServiceCollection): The service collection to configure.
             *    connectionString (string): The database connection string.
             * 
             * Returns:
             *    IServiceCollection: The updated service collection with DAL services.
             */

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

            services.AddScoped<FileRepository>();
            services.AddScoped<UserRepository>();

            return services;
        }
    }
}
