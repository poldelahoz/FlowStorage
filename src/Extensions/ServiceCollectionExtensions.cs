using FlowStorage.Abstractions;
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
            string storageTypeString = configuration["FlowStorage:type"]
                ?? Environment.GetEnvironmentVariable("FLOWSTORAGE_TYPE")
                ?? throw new ArgumentNullException("FLOWSTORAGE_TYPE");

            if (!Enum.TryParse(storageTypeString, ignoreCase: true, out StorageType storageType))
            {
                throw new NotSupportedException($"FlowStorage type '{storageTypeString}' is not supported.");
            }

            services.AddSingleton<IFlowStorageFactory, FlowStorageFactory>();

            switch (storageType)
            {
                case StorageType.AzureBlobStorage:
                    services.AddSingleton<IBlobServiceClientFactory, BlobServiceClientFactory>();
                    services.AddSingleton<IAzureBlobFlowStorage>(sp =>
                    {
                        var config = sp.GetRequiredService<IConfiguration>();
                        string connectionString = config["FlowStorage:connectionString"]
                            ?? Environment.GetEnvironmentVariable("FLOWSTORAGE_CONNECTION_STRING")
                            ?? throw new ArgumentNullException("FLOWSTORAGE_CONNECTION_STRING");
                        return ActivatorUtilities.CreateInstance<AzureBlobFlowStorage>(sp, connectionString);
                    });
                    services.AddSingleton<IFlowStorage>(sp => sp.GetRequiredService<IAzureBlobFlowStorage>());
                    break;
                case StorageType.LocalStorage:
                    services.AddSingleton<IFileSystem, FileSystem>();
                    services.AddSingleton<ILocalFlowStorage>(sp =>
                    {
                        var config = sp.GetRequiredService<IConfiguration>();
                        string connectionString = config["FlowStorage:connectionString"]
                            ?? Environment.GetEnvironmentVariable("FLOWSTORAGE_CONNECTION_STRING")
                            ?? throw new ArgumentNullException("FLOWSTORAGE_CONNECTION_STRING");
                        return ActivatorUtilities.CreateInstance<LocalFlowStorage>(sp, connectionString);
                    });
                    services.AddSingleton<IFlowStorage>(sp => sp.GetRequiredService<ILocalFlowStorage>());
                    break;
                default:
                    throw new NotSupportedException($"FlowStorage type '{storageType}' is not supported.");
            }

            return services;
        }
    }
}
