using EventSourcing.API.BackgroundServices;
using EventStore.ClientAPI;
using MediatR;
using System.Reflection;

namespace EventSourcing.API.EventStores
{
    public static class EventStoreExtensions
    {
        public static void AddEventStore(this IServiceCollection services, IConfiguration configuration)
        {
            var connection = EventStoreConnection.Create(connectionString: configuration.GetConnectionString("EventStore"));
            connection.ConnectAsync().Wait();
            services.AddSingleton(connection);
            services.AddSingleton<ProductStream>();
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddHostedService<ProductReadModelEventStore>();

            using var logFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole();
            });
            var logger = logFactory.CreateLogger("Startup-Program");
            connection.Connected += (sender, args) =>
             {
                 logger.LogInformation("Event Store connection establised");

             };
            connection.ErrorOccurred += (sender, args) =>
            {
                logger.LogInformation(args.Exception.Message);
            };
        }
    }
}
