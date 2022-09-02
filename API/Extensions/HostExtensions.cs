using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class HostExtensions
    {
        public static async Task<IHost> SeedData(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try{

                var dataContext = services.GetRequiredService<DataContext>();

                await dataContext.Database.MigrateAsync();
                await Seeder.SeedUsers(dataContext);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occured during migratin");
            }

            return host;
        }
    }
}