using FlowStorage.Abstractions.IBlobWrappers;

namespace FlowStorage.Abstractions.Factories
{
    internal interface IBlobServiceClientFactory
    {
        IBlobServiceClientWrapper Create(string connectionString);
    }
}
