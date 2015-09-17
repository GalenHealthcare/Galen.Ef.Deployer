using System.Data.Entity;

namespace Galen.EntityFramework.Sharding
{
    public abstract class ShardKeyMapperBase<TType>
    {
        public abstract bool KeyMapsToShard(TType key, DbContext context);
    }
}