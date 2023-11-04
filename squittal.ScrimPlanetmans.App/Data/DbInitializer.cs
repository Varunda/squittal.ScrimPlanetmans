using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace squittal.ScrimPlanetmans.Data
{
    public class DbInitializer {

        public static void Initialize(IServiceProvider serviceProvider) {
            ILogger<DbInitializer> logger = serviceProvider.GetRequiredService<ILogger<DbInitializer>>();

            using (var context = new PlanetmansDbContext(
                serviceProvider.GetRequiredService<
                DbContextOptions<PlanetmansDbContext>>()))
            {

                logger.LogInformation($"connStr: {context.Database.GetConnectionString()}");
                context.Database.Migrate();
                return;
            }
        }
    }
}
