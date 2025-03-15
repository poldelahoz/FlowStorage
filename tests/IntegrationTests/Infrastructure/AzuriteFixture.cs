using Testcontainers.Azurite;

namespace FlowStorageTests.IntegrationTests.Infrastructure
{
    public class AzuriteFixture : IAsyncLifetime
    {
        private readonly AzuriteContainer _azuriteContainer;

        public string ConnectionString { get; private set; } = string.Empty;

        public AzuriteFixture()
        {
            _azuriteContainer = new AzuriteBuilder()
                .WithImage("mcr.microsoft.com/azure-storage/azurite")
                .WithName($"azurite_{Guid.NewGuid()}")
                .WithPortBinding(10000, 10000)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _azuriteContainer.StartAsync();
            ConnectionString = _azuriteContainer.GetConnectionString();
        }

        public async Task DisposeAsync()
        {
            await _azuriteContainer.StopAsync();
            await _azuriteContainer.DisposeAsync();
        }
    }
}
