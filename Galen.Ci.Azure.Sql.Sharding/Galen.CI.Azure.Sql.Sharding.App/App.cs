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
using System.Data.SqlClient;
using System.Linq;
using Galen.CI.Azure.Sql.Sharding.App.Arguments;
using PowerArgs;
using Serilog;

namespace Galen.CI.Azure.Sql.Sharding.App
{
    public class App
    {
        [ArgActionMethod]
        [ArgDescription("Deploys the shard map manager schema if it does not already exist and optionally creates a login and/or user.")]
        public void Deploy(DeployArgs args)
        {
            args.Validate();

            var connectionStringBuilder = new SqlConnectionStringBuilder(args.ConnectionString);
            var targetDatabase = connectionStringBuilder.InitialCatalog;
            var targetServer = connectionStringBuilder.DataSource;

            connectionStringBuilder.InitialCatalog = string.Empty;
            var serverConnectionString = connectionStringBuilder.ConnectionString;

            Log.Information(LoggingMessageTemplates.CreateDatabase, targetDatabase, targetServer);
            SqlDatabaseUtilities.CreateDatabaseIfNotExists(targetDatabase, serverConnectionString);

            Log.Information(LoggingMessageTemplates.Deploy, targetDatabase, targetServer);

            var shardManagementService = new ShardMapManagementService(args.ConnectionString);
            shardManagementService.Deploy();

            if (args.HasLoginName && args.UseWindowsLogin)
            {
                Log.Information(LoggingMessageTemplates.CreateSqlLoginFromWindowsAccount, args.LoginName, targetServer);
                SqlDatabaseUtilities.CreateWindowsLoginIfNotExists(args.LoginName, serverConnectionString);
            }
            else if (args.HasLoginName)
            {
                Log.Information(LoggingMessageTemplates.CreateSqlLogin, args.LoginName, targetServer);
                SqlDatabaseUtilities.CreateSqlLoginIfNotExists(
                    args.LoginName,
                    args.LoginPassword,
                    serverConnectionString);
            }

            if (args.HasDatabaseUserName)
            {
                Log.Information(
                    LoggingMessageTemplates.CreateDatabaseUser, 
                    args.DatabaseUserName, 
                    targetDatabase,
                    args.LoginName, 
                    targetServer);

                SqlDatabaseUtilities.CreateDatabaseUserIfNotExists(
                    args.LoginName, 
                    args.DatabaseUserName,
                    args.ConnectionString);

                Log.Information(
                    LoggingMessageTemplates.GrantUserReadWritePermissions, 
                    args.DatabaseUserName,
                    targetDatabase, targetServer);

                SqlDatabaseUtilities.GrantUserDatabaseRequiredPermissions(args.DatabaseUserName, args.ConnectionString);
            }
        }

        [ArgActionMethod]
        [ArgDescription("Creates a list shard map if it does not already exist.")]
        [ArgShortcut("Create-ListShardMap")]
        public void CreateListShardMap(CreateShardMapArgs args)
        {
            args.Validate();

            var connectionString = new SqlConnectionStringBuilder(args.ConnectionString);
            Log.Information(
                LoggingMessageTemplates.CreateListShardMap,
                args.MapName,
                args.ShardKeyType.Name,
                connectionString.InitialCatalog,
                connectionString.DataSource);

            CreateShardMap(args, "CreateListShardMap");
        }

        [ArgActionMethod]
        [ArgDescription("Creates a range shard map if it does not already exist.")]
        [ArgShortcut("Create-RangeShardMap")]
        public void CreateRangeShardMap(CreateShardMapArgs args)
        {
            args.Validate();

            var connectionString = new SqlConnectionStringBuilder(args.ConnectionString);
            Log.Information(
                LoggingMessageTemplates.CreateRangeShardMap,
                args.MapName,
                args.ShardKeyType.Name,
                connectionString.InitialCatalog,
                connectionString.DataSource);

            CreateShardMap(args, "CreateRangeShardMap");
        }

        [ArgActionMethod]
        [ArgDescription("Adds a shard to a list shard map.")]
        [ArgShortcut("Add-ListMapShard")]
        public void AddListMapShard(AddListMapShardArgs args)
        {
            args.Validate();

            var connectionString = new SqlConnectionStringBuilder(args.ConnectionString);
            Log.Information(
                LoggingMessageTemplates.AddListMapShard,
                args.ShardDatabaseName,
                args.ShardServerName,
                args.ShardKey.KeyType.Name,
                args.ShardKey.KeyValue,
                args.MapName,
                connectionString.InitialCatalog,
                connectionString.DataSource);

            var shardManagementServiceType = typeof(ShardMapManagementService);
            var addListMapShardGeneric = shardManagementServiceType.GetMethod("AddListMapShard");
            var addListMapShard = addListMapShardGeneric.MakeGenericMethod(args.ShardKey.KeyType);

            var shardManagementService = new ShardMapManagementService(args.ConnectionString);
            var parameters = new[] {args.MapName, args.ShardKey.KeyValue, args.ShardServerName, args.ShardDatabaseName};
            addListMapShard.Invoke(shardManagementService, parameters);
        }

        [ArgActionMethod]
        [ArgDescription("Adds a shard to a range shard map.")]
        [ArgShortcut("Add-RangeMapShard")]
        public void AddRangeMapShard(AddRangeMapShardArgs args)
        {
            args.Validate();

            var connectionString = new SqlConnectionStringBuilder(args.ConnectionString);
            Log.Information(
                LoggingMessageTemplates.AddRangeMapShard,
                args.ShardDatabaseName,
                args.ShardServerName,
                args.ShardKeyRange.KeyType.Name,
                args.ShardKeyRange.LowValue,
                args.ShardKeyRange.HighValue,
                args.MapName,
                connectionString.InitialCatalog,
                connectionString.DataSource);

            var shardManagementServiceType = typeof(ShardMapManagementService);
            var addRangeMapShardGeneric = shardManagementServiceType.GetMethod("AddRangeMapShard");
            var addRangeMapShard = addRangeMapShardGeneric.MakeGenericMethod(args.ShardKeyRange.KeyType);

            var shardManagementService = new ShardMapManagementService(args.ConnectionString);
            var parameters = new[]
            {
                args.MapName,
                args.ShardKeyRange.LowValue,
                args.ShardKeyRange.HighValue,
                args.ShardServerName,
                args.ShardDatabaseName
            };
            addRangeMapShard.Invoke(shardManagementService, parameters);
        }

        [ArgActionMethod]
        [ArgDescription("Adds shards to a range map by evenly distributing the entire Int32 range across them.")]
        [ArgShortcut("Add-Int32RangeMapShards")]
        public void AddInt32RangeMapShards(AddInt32RangeMapShardsArgs args)
        {
            args.Validate();

            var shardRanges = Int32RangeGenerator.GetRanges(args.ShardLocations.Locations.Length);

            // sanity checks
            if (shardRanges.Count != args.ShardLocations.Locations.Length)
            {
                throw new Exception($"The number of locations ({args.ShardLocations.Locations.Length}) does not match the number of generated shard ranges ({shardRanges.Count}).");
            }

            if (shardRanges[1].LowValue != int.MinValue)
            {
                throw new Exception($"Expected the first shard range low value to be int.MinValue, but it is {shardRanges[1].LowValue}.");
            }

            if (shardRanges[shardRanges.Count].HighValue != int.MaxValue)
            {
                throw new Exception($"Expected the last shard range high value to be int.MaxValue, but it is {shardRanges[shardRanges.Count].HighValue}.");
            }

            // now that we're sure everything is as expected, let's proceed...
            var shardManagementService = new ShardMapManagementService(args.ConnectionString);

            var keyType = typeof(int).Name;
            var connectionString = new SqlConnectionStringBuilder(args.ConnectionString);

            for (int i = 0; i < args.ShardLocations.Locations.Length - 1; i++)
            {
                var shardNumber = (i + 1);
                var location = args.ShardLocations.Locations[i];
                var range = shardRanges[shardNumber];

                Log.Information(
                    LoggingMessageTemplates.AddRangeMapShard,
                    shardNumber,
                    location.DatabaseName,
                    location.ServerName,
                    keyType,
                    range.LowValue,
                    range.HighValue,
                    args.MapName,
                    connectionString.InitialCatalog,
                    connectionString.DataSource);

                shardManagementService.AddRangeMapShard(
                    args.MapName,
                    range.LowValue,
                    range.HighValue,
                    location.ServerName,
                    location.DatabaseName);
            }

            var lastLocation = args.ShardLocations.Locations.Last();
            var lastRange = shardRanges.Last();

            Log.Information(
                LoggingMessageTemplates.AddRangeMapShard,
                lastRange.Key,
                lastLocation.DatabaseName,
                lastLocation.ServerName,
                keyType,
                lastRange.Value.LowValue,
                "+infinity",
                args.MapName,
                connectionString.InitialCatalog,
                connectionString.DataSource);

            shardManagementService.AddRangeMapShard(
                args.MapName, 
                lastRange.Value.LowValue, 
                lastLocation.ServerName,
                lastLocation.DatabaseName);
        }

        private static void CreateShardMap(CreateShardMapArgs args, string createMethodName)
        {
            var shardManagementServiceType = typeof(ShardMapManagementService);
            var createShardMapGeneric = shardManagementServiceType.GetMethod(createMethodName);
            var createShardMap = createShardMapGeneric.MakeGenericMethod(args.ShardKeyType);

            var shardManagementService = new ShardMapManagementService(args.ConnectionString);
            var parameters = new object[] { args.MapName };
            createShardMap.Invoke(shardManagementService, parameters);
        }
    }
}
