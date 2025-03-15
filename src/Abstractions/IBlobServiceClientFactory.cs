using FlowStorage.Abstractions.IBlobWrappers;

namespace FlowStorage.Abstractions
{
    internal interface IBlobServiceClientFactory
    {
        IBlobServiceClientWrapper Create(string connectionString);
    }
}
