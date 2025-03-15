namespace FlowStorage.Abstractions
{
    internal interface IFlowStorageFactory
    {
        IFlowStorage Create();
    }
}
