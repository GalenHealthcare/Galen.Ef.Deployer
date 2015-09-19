#region License
// /*
//         The MIT License
// 
//         Copyright (c) 2015 Galen Healthcare Solutions
// 
//         Permission is hereby granted, free of charge, to any person obtaining a copy
//         of this software and associated documentation files (the "Software"), to deal
//         in the Software without restriction, including without limitation the rights
//         to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//         copies of the Software, and to permit persons to whom the Software is
//         furnished to do so, subject to the following conditions:
// 
//         The above copyright notice and this permission notice shall be included in
//         all copies or substantial portions of the Software.
// 
//         THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//         IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//         FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//         AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//         LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//         OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//         THE SOFTWARE.
//  */
#endregion
using System;
using System.Linq;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace Galen.CI.Azure.Sql.Sharding
{
    public class ShardMapManagementService
    {
        private readonly string m_ConnectionString;

        public ShardMapManagementService(string shardMapManagerConnectionString)
        {
            m_ConnectionString = shardMapManagerConnectionString;
        }

        private ShardMapManager m_ShardMapManager;
        private ShardMapManager ShardMapManager
        {
            get
            {
                if (m_ShardMapManager == null)
                {
                    ShardMapManagerFactory.TryGetSqlShardMapManager(
                        m_ConnectionString,
                        ShardMapManagerLoadPolicy.Lazy,
                        out m_ShardMapManager);
                }

                return m_ShardMapManager;
            }
            set
            {
                m_ShardMapManager = value;
            }
        }

        public void Deploy()
        {
            if (ShardMapManager != null)
            {
                // shard map manager has already been deployed
                return;
            }

            ShardMapManager = ShardMapManagerFactory.CreateSqlShardMapManager(
                m_ConnectionString,
                ShardMapManagerCreateMode.KeepExisting);
        }

        private ListShardMap<TKey> GetListShardMap<TKey>(string mapName)
        {
            ListShardMap<TKey> listShardMap = null;
            ShardMapManager.TryGetListShardMap(mapName, out listShardMap);
            return listShardMap;
        }

        private RangeShardMap<TKey> GetRangeShardMap<TKey>(string mapName)
        {
            RangeShardMap<TKey> rangeShardMap = null;
            ShardMapManager.TryGetRangeShardMap(mapName, out rangeShardMap);
            return rangeShardMap;
        }

        public void CreateListShardMap<TKey>(string mapName)
        {
            var listShardMap = GetListShardMap<TKey>(mapName);
            if (listShardMap != null)
            {
                // shard map already exists
                return;
            }

            ShardMapManager.CreateListShardMap<TKey>(mapName);
        }

        public void CreateRangeShardMap<TKey>(string mapName)
        {
            var rangeShardMap = GetRangeShardMap<TKey>(mapName);
            if (rangeShardMap != null)
            {
                // shard map already exists
                return;
            }

            ShardMapManager.CreateRangeShardMap<TKey>(mapName);
        }

        public void AddListMapShard<TKey>(string mapName, TKey key, string serverName, string databaseName)
        {
            var listShardMap = GetListShardMap<TKey>(mapName);            

            PointMapping<TKey> mapping = null;
            var isMappingExists = listShardMap.TryGetMappingForKey(key, out mapping);

            var location = new ShardLocation(serverName, databaseName);

            Shard shard = null;
            var isShardExists = listShardMap.TryGetShard(location, out shard);

            var isMappedToDifferentShard = (isMappingExists && !isShardExists) ||
                                           (isMappingExists && !mapping.Shard.Equals(shard));
            if (isMappedToDifferentShard)
            {
                var message = string.Format(
                    "Key {0} in list shard map {1} is already mapped to database {2} on server {3}!",
                    key,
                    mapName,
                    mapping.Shard.Location.Database,
                    mapping.Shard.Location.Server);
                throw new Exception(message);
            }

            if (isMappingExists)
            {
                // mapping already exists and points to the correct shard location
                return;
            }

            if (!isShardExists)
            {
                shard = listShardMap.CreateShard(location);
            }

            listShardMap.CreatePointMapping(key, shard);
        }

        public void AddRangeMapShard<TKey>(
            string mapName, 
            TKey lowValue, 
            TKey highValue, 
            string serverName, 
            string databaseName)
        {
            var range = new Range<TKey>(lowValue, highValue);
            AddRangeMapShard(mapName, range, serverName, databaseName);
        }

        public void AddRangeMapShard<TKey>(string mapName, TKey lowValue, string serverName, string databaseName)
        {
            var range = new Range<TKey>(lowValue);
            AddRangeMapShard(mapName, range, serverName, databaseName);
        }

        private void AddRangeMapShard<TKey>(string mapName, Range<TKey> range, string serverName, string databaseName)
        {
            var rangeShardMap = GetRangeShardMap<TKey>(mapName);

            var existingMappings = rangeShardMap.GetMappings(range);
            if (existingMappings.Count > 1)
            {
                throw new Exception($"Range ({range.Low},{range.GetHighString()}) in range shard map {mapName} is already mapped across multiple shards!");
            }

            var mapping = existingMappings.SingleOrDefault();
            var isMappingExists = (mapping != null);

            var location = new ShardLocation(serverName, databaseName);

            Shard shard = null;
            var isShardExists = rangeShardMap.TryGetShard(location, out shard);

            var isMappedToDifferentShard = (isMappingExists && !isShardExists) ||
                                           (isMappingExists && !mapping.Shard.Equals(shard));
            if (isMappedToDifferentShard)
            {
                var message = string.Format(
                    "All or part of range ({0},{1}) in range shard map {2} is already mapped to database {3} on server {4}!",
                    range.Low,
                    range.GetHighString(),
                    mapName,
                    mapping.Shard.Location.Database,
                    mapping.Shard.Location.Server);
                throw new Exception(message);
            }

            var isRangeChange = (isMappingExists && !mapping.Value.Equals(range));
            if (isRangeChange)
            {
                // mapping already exists, but not for the exact range specified
                // we don't support this
                var message = string.Format(
                    "Range can not be changed to ({0},{1}) for shard database {2} on server {3} as it " +
                    "already exists in range shard map {4} but is mapped to range ({5},{6})!  " +
                    "Changing an existing shard mapping range is not supported by this service.",
                    range.Low,
                    range.GetHighString(),
                    databaseName,
                    serverName,
                    mapName,
                    mapping.Value.Low,
                    mapping.Value.GetHighString());
                throw new Exception(message);
            }

            if (isMappingExists)
            {
                // mapping already exists and points to the correct shard location
                return;
            }

            if (!isShardExists)
            {
                shard = rangeShardMap.CreateShard(location);
            }

            rangeShardMap.CreateRangeMapping(range, shard);
        }
    }
}
