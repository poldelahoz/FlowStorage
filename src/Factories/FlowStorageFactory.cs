using Azure.Storage.Blobs;
using FlowStorage.Abstractions;
using FlowStorage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowStorage.Factories
{
    internal class FlowStorageFactory : IFlowStorageFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly StorageType _storageType;
        private readonly string _connectionString;

        public FlowStorageFactory(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // Utilizamos GetValue<T> y fallback manual para las variables de entorno
            string storageTypeString = configuration["FlowStorage:type"]
                ?? Environment.GetEnvironmentVariable("FLOWSTORAGE_TYPE")
                ?? throw new ArgumentNullException("FLOWSTORAGE_TYPE");

            if (!Enum.TryParse(storageTypeString, ignoreCase: true, out StorageType storageType))
            {
                throw new NotSupportedException($"FlowStorage type '{storageTypeString}' is not supported.");
            }
            _storageType = storageType;

            _connectionString = configuration["FlowStorage:connectionString"]
                ?? Environment.GetEnvironmentVariable("FLOWSTORAGE_CONNECTION_STRING")
                ?? throw new ArgumentNullException("FLOWSTORAGE_CONNECTION_STRING");
        }

        public IFlowStorage Create()
        {
            return _storageType switch
            {
                StorageType.AzureBlobStorage => ActivatorUtilities.CreateInstance<AzureBlobFlowStorage>(
                                        _serviceProvider, _connectionString),

                StorageType.LocalStorage => ActivatorUtilities.CreateInstance<LocalFlowStorage>(
                                        _serviceProvider, _connectionString),

                _ => throw new NotSupportedException($"FlowStorage type '{_storageType}' is not supported."),
            };
        }
    }
}