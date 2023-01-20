using CommandService.Models;
using CommandService.SyncDataServices.Grpc;

namespace CommandService.Data
{
    public class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

                var logger = serviceScope.ServiceProvider.GetService<ILogger<PrepDb>>();

                var platforms = grpcClient.ReturnAllPlatforms();

                SeedData(serviceScope.ServiceProvider.GetService<ICommandRepository>(), platforms, logger);
            };
        }

        private static void SeedData(ICommandRepository repository, IEnumerable<Platform> platforms, ILogger<PrepDb> logger)
        {
            logger.LogInformation("Seeding new platforms...");

            foreach (var platform in platforms)
            {
                if(!repository.ExternalPlatformExists(platform.ExternalId))
                {
                    repository.CreatePlatform(platform);
                }
                
                repository.SaveChanges();
            }
        }
    }
}