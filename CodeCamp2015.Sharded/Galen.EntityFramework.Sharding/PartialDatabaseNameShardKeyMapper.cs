using System.Data.Entity;

namespace Galen.EntityFramework.Sharding
{
    public class PartialDatabaseNameShardKeyMapper : ShardKeyMapperBase<string>
    {
        public override bool KeyMapsToShard(string key, DbContext context)
        {
            var databaseName = context.Database.Connection.Database;
            if (databaseName.ToLower().Contains(key.ToLower()))
            {
                return true;
            }
            return false;
        }
    }
}