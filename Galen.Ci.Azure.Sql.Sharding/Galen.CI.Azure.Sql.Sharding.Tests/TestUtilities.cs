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
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace Galen.CI.Azure.Sql.Sharding.Tests
{
    internal class TestUtilities
    {
        private static object ExecuteScalar(DbConnection connection, string sql)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            var result = command.ExecuteScalar();

            return result;
        }

        private static void ExecuteNonQuery(string connectionString, string sql)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = sql;
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void CreateDatabase(string serverName, string databaseName)
        {
            var sql = $"CREATE DATABASE {databaseName}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                IntegratedSecurity = true,
                ApplicationName = "Galen.CI.Azure.Sql.Sharding.Tests"
            };

            ExecuteNonQuery(connectionStringBuilder.ConnectionString, sql);
        }

        public static void CreateDummyTableWithNRows(string serverName, string databaseName, int numberOfRows)
        {
            const string createTableSql =
                "CREATE TABLE [dbo].[Dummy]( " +
                    "[ID][INT] NOT NULL, " +
                    "[Value] [NVARCHAR](50) NOT NULL " +
                ") ON[PRIMARY] ";

            var sqlStringBuilder = new StringBuilder(createTableSql);
            for (int row = 1; row <= numberOfRows; row++)
            {
                sqlStringBuilder.AppendFormat("INSERT INTO [dbo].[Dummy] VALUES ({0}, 'Test {0}')\n", row);
            }

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                ApplicationName = "Galen.CI.Azure.Sql.Sharding.Tests"
            };

            ExecuteNonQuery(connectionStringBuilder.ConnectionString, sqlStringBuilder.ToString());
        }

        public static void DropDatabase(string serverName, string databaseName)
        {
            var sql = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}];";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                IntegratedSecurity = true,
                ApplicationName = "Galen.CI.Azure.Sql.Sharding.Tests"
            };

            ExecuteNonQuery(connectionStringBuilder.ConnectionString, sql);
        }

        public static void InitializeShardMapManager(string connectionString)
        {
            ShardMapManagerFactory.CreateSqlShardMapManager(connectionString, ShardMapManagerCreateMode.KeepExisting);
        }

        public static bool ShardMapManagerExists(string connectionString)
        {
            ShardMapManager _ = null;
            return ShardMapManagerFactory.TryGetSqlShardMapManager(connectionString, ShardMapManagerLoadPolicy.Eager, out _);
        }

        public static bool ListShardMapExists<TKey>(string connectionString, string mapName)
        {
            var shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(
                connectionString,
                ShardMapManagerLoadPolicy.Lazy);

            ListShardMap<TKey> _ = null;
            return shardMapManager.TryGetListShardMap(mapName, out _);
        }

        public static bool RangeShardMapExists<TKey>(string connectionString, string mapName)
        {
            var shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(
                connectionString,
                ShardMapManagerLoadPolicy.Lazy);

            RangeShardMap<TKey> _ = null;
            return shardMapManager.TryGetRangeShardMap(mapName, out _);
        }

        public static void CreateListShardMap<TKey>(string connectionString, string mapName)
        {
            var shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(
                connectionString,
                ShardMapManagerLoadPolicy.Lazy);

            shardMapManager.CreateListShardMap<TKey>(mapName);
        }

        public static void CreateRangeShardMap<TKey>(string connectionString, string mapName)
        {
            var shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(
                connectionString,
                ShardMapManagerLoadPolicy.Lazy);

            shardMapManager.CreateRangeShardMap<TKey>(mapName);
        }

        public static ShardLocation GetShardLocationFromListMap<TKey>(string connectionString, string mapName, TKey key)
        {
            var shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(
                connectionString,
                ShardMapManagerLoadPolicy.Lazy);

            var listMap = shardMapManager.GetListShardMap<TKey>(mapName);
            var mapping = listMap.GetMappingForKey(key);
            return mapping.Shard.Location;
        }

        private static ShardLocation GetShardLocationFromRangeMap<TKey>(
            string connectionString, 
            string mapName,
            Range<TKey> range)
        {
            var shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(
                connectionString,
                ShardMapManagerLoadPolicy.Lazy);

            var rangeMap = shardMapManager.GetRangeShardMap<TKey>(mapName);
            var mapping = rangeMap.GetMappings(range).SingleOrDefault();        // expectation is we're looking for a single mapping
            return mapping?.Shard.Location;
        }

        public static ShardLocation GetShardLocationFromRangeMap<TKey>(
            string connectionString, 
            string mapName, 
            TKey lowValue, 
            TKey highValue)
        {
            var range = new Range<TKey>(lowValue, highValue);
            return GetShardLocationFromRangeMap(connectionString, mapName, range);
        }

        public static ShardLocation GetShardLocationFromRangeMap<TKey>(string connectionString, string mapName, TKey lowValue)
        {
            var range = new Range<TKey>(lowValue);
            return GetShardLocationFromRangeMap(connectionString, mapName, range);
        }

        public static void CreateShardInListMap<TKey>(
            string connectionString, 
            string mapName, 
            string serverName, 
            string databaseName)
        {
            var shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(
                connectionString,
                ShardMapManagerLoadPolicy.Lazy);

            var listMap = shardMapManager.GetListShardMap<TKey>(mapName);
            listMap.CreateShard(new ShardLocation(serverName, databaseName));
        }

        public static void CreateShardInRangeMap<TKey>(
            string connectionString,
            string mapName,
            string serverName,
            string databaseName)
        {
            var shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(
                connectionString,
                ShardMapManagerLoadPolicy.Lazy);

            var rangeMap = shardMapManager.GetRangeShardMap<TKey>(mapName);
            rangeMap.CreateShard(new ShardLocation(serverName, databaseName));
        }

        public static int? GetDummyRowCountUsingListMapShardConnection<TKey>(
            string shardManagerConnectionString, 
            string mapName, 
            TKey key, 
            string shardConnectionStringPartial)
        {
            var shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(
                shardManagerConnectionString,
                ShardMapManagerLoadPolicy.Lazy);

            var listMap = shardMapManager.GetListShardMap<TKey>(mapName);
            var shardConnection = listMap.OpenConnectionForKey<TKey>(key, shardConnectionStringPartial);
            using (shardConnection)
            {
                const string sql = "SELECT COUNT(*) FROM [dbo].[Dummy]";
                var result = (int?)ExecuteScalar(shardConnection, sql);
                return result;
            }
        }

        public static int? GetDummyRowCountUsingRangeMapShardConnection<TKey>(
            string shardManagerConnectionString,
            string mapName,
            TKey key,
            string shardConnectionStringPartial)
        {
            var shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(
                shardManagerConnectionString,
                ShardMapManagerLoadPolicy.Lazy);

            var rangeMap = shardMapManager.GetRangeShardMap<TKey>(mapName);
            var shardConnection = rangeMap.OpenConnectionForKey<TKey>(key, shardConnectionStringPartial);
            using (shardConnection)
            {
                const string sql = "SELECT COUNT(*) FROM [dbo].[Dummy]";
                var result = (int?)ExecuteScalar(shardConnection, sql);
                return result;
            }
        }
    }
}
