using FlowStorage.Abstractions.Factories;
using FlowStorage.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FlowStorage.Factories
{
    internal class FlowStorageFactory(
        IServiceProvider serviceProvider,
        StorageType storageType,
        string connectionString) : IFlowStorageFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly StorageType _storageType = storageType;
        private readonly string _connectionString = connectionString;

        public IFlowStorage Create()
        {
            return _storageType switch
            {
                StorageType.AzureBlobStorage => ActivatorUtilities.CreateInstance<AzureBlobFlowStorage>(
                                        _serviceProvider, _connectionString),

                StorageType.LocalStorage => ActivatorUtilities.CreateInstance<LocalFlowStorage>(
                                        _serviceProvider, _connectionString),

                StorageType.InMemoryStorage => ActivatorUtilities.CreateInstance<InMemoryFlowStorage>(
                                        _serviceProvider),

                _ => throw new NotSupportedException($"FlowStorage type '{_storageType}' is not supported."),
            };
        }
    }
}