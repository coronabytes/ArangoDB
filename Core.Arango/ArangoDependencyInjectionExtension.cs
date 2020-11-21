using System;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Arango
{
    public static class ArangoDependencyInjectionExtension
    {
        public static IServiceCollection AddArango(this IServiceCollection collection,
            Func<IServiceProvider, string> configurator)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));

            return collection.AddSingleton<IArangoContext>(serviceProvider =>
            {
                var connectionString = configurator(serviceProvider);

                return new ArangoContext(connectionString);
            });
        }

        public static IServiceCollection AddArango(this IServiceCollection collection,
            Action<IServiceProvider, IArangoConfiguration> configurator)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));

            return collection.AddSingleton<IArangoContext>(serviceProvider =>
            {
                var config = new ArangoConfiguration();
                configurator(serviceProvider, config);
                return new ArangoContext(config);
            });
        }

        public static IServiceCollection AddArango(this IServiceCollection collection, string connectionString,
            IArangoConfiguration settings = null)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            return collection.AddSingleton<IArangoContext>(serviceProvider =>
                new ArangoContext(connectionString, settings));
        }
    }
}