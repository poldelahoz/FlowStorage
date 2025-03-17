using FlowStorage.Abstractions.Factories;
using FlowStorage.Abstractions.Services;
using FlowStorage.Factories;
using FlowStorage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;
namespace FlowStorage
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFlowStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var (storageType, connectionString) = GetConfigurationValues(configuration);

            services.AddFactoryServices(storageType, connectionString);

            switch (storageType)
            {
                case StorageType.AzureBlobStorage:
                    services.AddAzureBlobStorageServices(connectionString);
                    break;

                case StorageType.LocalStorage:
                    services.AddLocalStorageServices(connectionString);
                    break;

                case StorageType.InMemoryStorage:
                    services.AddInMemoryStorageServices();
                    break;

                default:
                    throw new NotSupportedException($"FlowStorage type '{storageType}' is not supported.");
            }

            return services;
        }

        private static (StorageType storageType, string? connectionString) GetConfigurationValues(IConfiguration configuration)
        {
            string? storageTypeString = configuration["FlowStorage:type"]
                ?? Environment.GetEnvironmentVariable("FLOWSTORAGE_TYPE");

            string? connectionString = configuration["FlowStorage:connectionString"]
                ?? Environment.GetEnvironmentVariable("FLOWSTORAGE_CONNECTION_STRING");

            ArgumentException.ThrowIfNullOrEmpty(storageTypeString, nameof(storageTypeString));

            if (!Enum.TryParse(storageTypeString, ignoreCase: true, out StorageType storageType))
            {
                throw new NotSupportedException($"FlowStorage type '{storageTypeString}' is not supported.");
            }

            return (storageType, connectionString);
        }

        private static IServiceCollection AddFactoryServices(this IServiceCollection services, StorageType storageType, string? connectionString)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));

            services.AddSingleton<IFlowStorageFactory>(sp =>
            {
                return ActivatorUtilities.CreateInstance<FlowStorageFactory>(sp, storageType, connectionString);
            });

            return services;
        }

        private static IServiceCollection AddAzureBlobStorageServices(this IServiceCollection services, string? connectionString)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));

            services.AddSingleton<IBlobServiceClientFactory, BlobServiceClientFactory>();

            services.AddSingleton<IAzureBlobFlowStorage>(sp =>
            {
                return ActivatorUtilities.CreateInstance<AzureBlobFlowStorage>(sp, connectionString);
            });

            services.AddSingleton<IFlowStorage>(sp => sp.GetRequiredService<IAzureBlobFlowStorage>());

            return services;
        }

        private static IServiceCollection AddLocalStorageServices(this IServiceCollection services, string? connectionString)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));

            services.AddSingleton<IFileSystem, FileSystem>();

            services.AddSingleton<ILocalFlowStorage>(sp =>
            {
                return ActivatorUtilities.CreateInstance<LocalFlowStorage>(sp, connectionString);
            });

            services.AddSingleton<IFlowStorage>(sp => sp.GetRequiredService<ILocalFlowStorage>());

            return services;
        }

        private static IServiceCollection AddInMemoryStorageServices(this IServiceCollection services)
        {
            services.AddSingleton<IInMemoryFlowStorage, InMemoryFlowStorage>();

            services.AddSingleton<IFlowStorage>(sp => sp.GetRequiredService<IInMemoryFlowStorage>());

            return services;
        }
    }
}
