namespace Galen.EntityFramework.Sharding
{
    public interface IShardedSeedingStrategy<TData, TType>
    {
        TType GetKey(TData data);
    }
}