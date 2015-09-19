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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace Galen.CI.Azure.Sql.Sharding.Tests
{
    [TestClass]
    public class ShardMapServiceTests
    {
        [TestMethod]
        public void CorrectlyDeploysShardMapManager()
        {
            const string serverName = @"(localdb)\mssqllocaldb";
            var databaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                ApplicationName = "Galen.CI.Azure.Sql.Sharding.Tests"
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                TestUtilities.CreateDatabase(serverName, databaseName);

                var sut = new ShardMapManagementService(connectionString);
                sut.Deploy();

                var isDeployed = TestUtilities.ShardMapManagerExists(connectionString);
                Assert.IsTrue(isDeployed);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, databaseName);
            }
        }

        [TestMethod]
        public void DoesNotThrowExceptionWhenShardMapManagerAlreadyDeployed()
        {
            const string serverName = @"(localdb)\mssqllocaldb";
            var databaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                ApplicationName = "Galen.CI.Azure.Sql.Sharding.Tests"
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                TestUtilities.CreateDatabase(serverName, databaseName);
                TestUtilities.InitializeShardMapManager(connectionString);

                var sut = new ShardMapManagementService(connectionString);
                sut.Deploy();   // should not throw an exception
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, databaseName);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesListShardMapForGuid()
        {
            const string serverName = @"(localdb)\mssqllocaldb";
            var databaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                ApplicationName = "Galen.CI.Azure.Sql.Sharding.Tests"
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                TestUtilities.CreateDatabase(serverName, databaseName);
                TestUtilities.InitializeShardMapManager(connectionString);

                var listMapName = $"TestListShardMap_{Guid.NewGuid():N}";
                var sut = new ShardMapManagementService(connectionString);
                sut.CreateListShardMap<Guid>(listMapName);

                var isListShardMapExists = TestUtilities.ListShardMapExists<Guid>(connectionString, listMapName);
                Assert.IsTrue(isListShardMapExists);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, databaseName);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesRangeShardMapForInt()
        {
            const string serverName = @"(localdb)\mssqllocaldb";
            var databaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                ApplicationName = "Galen.CI.Azure.Sql.Sharding.Tests"
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                TestUtilities.CreateDatabase(serverName, databaseName);
                TestUtilities.InitializeShardMapManager(connectionString);

                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";
                var sut = new ShardMapManagementService(connectionString);
                sut.CreateRangeShardMap<int>(rangeMapName);

                var isRangeShardMapExists = TestUtilities.RangeShardMapExists<int>(connectionString, rangeMapName);
                Assert.IsTrue(isRangeShardMapExists);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, databaseName);
            }
        }

        [TestMethod]
        public void DoesNotThrowExceptionWhenListShardMapForGuidAlreadyExists()
        {
            const string serverName = @"(localdb)\mssqllocaldb";
            var databaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                ApplicationName = "Galen.CI.Azure.Sql.Sharding.Tests"
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var listMapName = $"TestListShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, databaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateListShardMap<Guid>(connectionString, listMapName);

                var sut = new ShardMapManagementService(connectionString);
                sut.CreateListShardMap<Guid>(listMapName);      // should not throw exception
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, databaseName);
            }
        }

        [TestMethod]
        public void DoesNotThrowExceptionWhenRangeShardMapForIntAlreadyExists()
        {
            const string serverName = @"(localdb)\mssqllocaldb";
            var databaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                ApplicationName = "Galen.CI.Azure.Sql.Sharding.Tests"
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, databaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                var sut = new ShardMapManagementService(connectionString);
                sut.CreateRangeShardMap<int>(rangeMapName);      // should not throw exception
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, databaseName);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesShardAndMappingWhenAddingListShardMap()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var listMapName = $"TestListShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateListShardMap<Guid>(connectionString, listMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                var shardKey = Guid.NewGuid();

                var sut = new ShardMapManagementService(connectionString);
                sut.AddListMapShard(listMapName, shardKey, serverName, shardDatabaseName);

                var actualShardLocation = TestUtilities.GetShardLocationFromListMap(
                    connectionString, 
                    listMapName,
                    shardKey);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingListMapShardConnection(
                    connectionString,
                    listMapName,
                    shardKey,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void DoesNotThrowExceptionWhenAddingSameShardForExistingListKey()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard01_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var listMapName = $"TestListShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateListShardMap<Guid>(connectionString, listMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                var shardKey = Guid.NewGuid();

                var sut = new ShardMapManagementService(connectionString);
                sut.AddListMapShard(listMapName, shardKey, serverName, shardDatabaseName);

                // do it again; this should essentially no-op and not throw an exception
                sut.AddListMapShard(listMapName, shardKey, serverName, shardDatabaseName);

                var actualShardLocation = TestUtilities.GetShardLocationFromListMap(
                    connectionString,
                    listMapName,
                    shardKey);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingListMapShardConnection(
                    connectionString,
                    listMapName,
                    shardKey,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void DoesNotThrowExceptionWhenAddingSameShardForExistingBoundedRange()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard01_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                const int shardRangeLow = 0;
                const int shardRangeHigh = 1000;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, shardRangeHigh, serverName, shardDatabaseName);

                // do it again; this should essentially no-op and not throw an exception
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, shardRangeHigh, serverName, shardDatabaseName);

                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow,
                    shardRangeHigh);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                var randomKeyInRange = new Random().Next(shardRangeLow, shardRangeHigh);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void DoesNotThrowExceptionWhenAddingSameShardForExistingInfiniteRange()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard01_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                const int shardRangeLow = 0;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, serverName, shardDatabaseName);

                // do it again; this should essentially no-op and not throw an exception
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, serverName, shardDatabaseName);

                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                // make sure the range is "infinite" (well, inclusive of int.MaxValue anyways...)
                var actualDummyRowCountMax = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    int.MaxValue,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountMax.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCountMax.Value);

                // also verify the range with a random key
                var randomKeyInRange = new Random().Next(shardRangeLow, int.MaxValue);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void DoesNotChangeMappingWhenAddingDifferentShardForExistingListKey()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName01 = $"GalenTest_Shard01_{Guid.NewGuid():N}";
            var shardDatabaseName02 = $"GalenTest_Shard02_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var listMapName = $"TestListShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateListShardMap<Guid>(connectionString, listMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName01);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName02);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName01, expectedDummyRowCount);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName02, 9);

                var shardKey = Guid.NewGuid();

                var sut = new ShardMapManagementService(connectionString);
                sut.AddListMapShard(listMapName, shardKey, serverName, shardDatabaseName01);

                // try to map the same key to a different shard - this should throw an exception
                var isExceptionThrown = false;
                try
                {
                    sut.AddListMapShard(listMapName, shardKey, serverName, shardDatabaseName02);
                }
                catch (Exception ex)
                {
                    isExceptionThrown = ex.Message.Contains("already mapped");
                }
                Assert.IsTrue(isExceptionThrown);

                // didn't use ExpectedException attribute because want to verify that 
                // the existing mapping was not altered
                var actualShardLocation = TestUtilities.GetShardLocationFromListMap(
                    connectionString,
                    listMapName,
                    shardKey);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation.Database);

                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingListMapShardConnection(
                    connectionString,
                    listMapName,
                    shardKey,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName01);
                TestUtilities.DropDatabase(serverName, shardDatabaseName02);
            }
        }

        [TestMethod]
        public void DoesNotChangeMappingWhenAddingDifferentShardForExistingBoundedRange()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName01 = $"GalenTest_Shard01_{Guid.NewGuid():N}";
            var shardDatabaseName02 = $"GalenTest_Shard02_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName01);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName02);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName01, expectedDummyRowCount);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName02, 9);

                const int shardRangeLow = 0;
                const int shardRangeHigh = 1000;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, shardRangeHigh, serverName, shardDatabaseName01);

                var isExceptionThrown = false;
                try
                {
                    // try mapping the same range to a different database
                    sut.AddRangeMapShard(rangeMapName, shardRangeLow, shardRangeHigh, serverName, shardDatabaseName02);
                }
                catch (Exception ex)
                {
                    isExceptionThrown = ex.Message.Contains("already mapped");
                }
                Assert.IsTrue(isExceptionThrown);

                // didn't use ExpectedException attribute because want to verify that 
                // the existing mapping was not altered
                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow,
                    shardRangeHigh);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation.Database);

                var randomKeyInRange = new Random().Next(shardRangeLow, shardRangeHigh);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName01);
                TestUtilities.DropDatabase(serverName, shardDatabaseName02);
            }
        }

        [TestMethod]
        public void DoesNotChangeMappingWhenAddingDifferentShardForExistingInfiniteRange()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName01 = $"GalenTest_Shard01_{Guid.NewGuid():N}";
            var shardDatabaseName02 = $"GalenTest_Shard02_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName01);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName02);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName01, expectedDummyRowCount);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName02, 9);

                const int shardRangeLow = 0;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, serverName, shardDatabaseName01);

                var isExceptionThrown = false;
                try
                {
                    // try mapping the same infinite range to a different database
                    sut.AddRangeMapShard(rangeMapName, shardRangeLow, serverName, shardDatabaseName02);
                }
                catch (Exception ex)
                {
                    isExceptionThrown = ex.Message.Contains("already mapped");
                }
                Assert.IsTrue(isExceptionThrown);

                // didn't use ExpectedException attribute because want to verify that 
                // the existing mapping was not altered
                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation.Database);

                var randomKeyInRange = new Random().Next(shardRangeLow, int.MaxValue);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName01);
                TestUtilities.DropDatabase(serverName, shardDatabaseName02);
            }
        }

        [TestMethod]
        public void DoesNotChangeMappingWhenAddingDifferentShardThatOverlapsAnExistingBoundedRange()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName01 = $"GalenTest_Shard01_{Guid.NewGuid():N}";
            var shardDatabaseName02 = $"GalenTest_Shard02_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName01);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName02);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName01, expectedDummyRowCount);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName02, 9);

                const int shardRangeLow01 = 0;
                const int shardRangeHigh01 = 1000;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow01, shardRangeHigh01, serverName, shardDatabaseName01);

                // try to map an overlapping ("duplicate") range - this should throw an exception
                const int shardRangeLow02 = 500;
                const int shardRangeHigh02 = 1500;
                var isExceptionThrown = false;
                try
                {
                    sut.AddRangeMapShard(rangeMapName, shardRangeLow02, shardRangeHigh02, serverName, shardDatabaseName02);
                }
                catch (Exception ex)
                {
                    isExceptionThrown = ex.Message.Contains("already mapped");
                }
                Assert.IsTrue(isExceptionThrown);

                // didn't use ExpectedException attribute because want to verify that 
                // the existing mapping was not altered
                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow01,
                    shardRangeHigh01);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation.Database);

                // generate random key between the overlap range to verify everything is correct
                var randomKeyInRange = new Random().Next(shardRangeLow02, shardRangeHigh01);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName01);
                TestUtilities.DropDatabase(serverName, shardDatabaseName02);
            }
        }

        [TestMethod]
        public void DoesNotChangeMappingWhenAddingDifferentShardThatOverlapsAnExistingInfiniteRange()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName01 = $"GalenTest_Shard01_{Guid.NewGuid():N}";
            var shardDatabaseName02 = $"GalenTest_Shard02_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName01);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName02);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName01, expectedDummyRowCount);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName02, 9);

                const int shardRangeLow01 = 0;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow01, serverName, shardDatabaseName01);

                // try to map an overlapping ("duplicate") range - this should throw an exception
                const int shardRangeLow02 = 500;
                const int shardRangeHigh02 = 1500;
                var isExceptionThrown = false;
                try
                {
                    sut.AddRangeMapShard(rangeMapName, shardRangeLow02, shardRangeHigh02, serverName, shardDatabaseName02);
                }
                catch (Exception ex)
                {
                    isExceptionThrown = ex.Message.Contains("already mapped");
                }
                Assert.IsTrue(isExceptionThrown);

                // didn't use ExpectedException attribute because want to verify that 
                // the existing mapping was not altered
                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow01);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation.Database);

                // generate random key between the overlap range to verify everything is correct
                var randomKeyInRange = new Random().Next(shardRangeLow02, shardRangeHigh02);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName01);
                TestUtilities.DropDatabase(serverName, shardDatabaseName02);
            }
        }

        [TestMethod]
        public void DoesNotChangeMappingWhenAddingDifferentInfiniteShardThatOverlapsAnExistingBoundedRange()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName01 = $"GalenTest_Shard01_{Guid.NewGuid():N}";
            var shardDatabaseName02 = $"GalenTest_Shard02_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName01);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName02);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName01, expectedDummyRowCount);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName02, 9);

                const int shardRangeLow01 = 0;
                const int shardRangeHigh01 = 1000;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow01, shardRangeHigh01, serverName, shardDatabaseName01);

                // try to map an overlapping ("duplicate") infinite range - this should throw an exception
                const int shardRangeLow02 = 500;
                var isExceptionThrown = false;
                try
                {
                    sut.AddRangeMapShard(rangeMapName, shardRangeLow02, serverName, shardDatabaseName02);
                }
                catch (Exception ex)
                {
                    isExceptionThrown = ex.Message.Contains("already mapped");
                }
                Assert.IsTrue(isExceptionThrown);

                // didn't use ExpectedException attribute because want to verify that 
                // the existing mapping was not altered
                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow01,
                    shardRangeHigh01);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation.Database);

                // generate random key between the overlap range to verify everything is correct
                var randomKeyInRange = new Random().Next(shardRangeLow02, shardRangeHigh01);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName01);
                TestUtilities.DropDatabase(serverName, shardDatabaseName02);
            }
        }

        [TestMethod]
        public void DoesNotChangeMappingWhenAddingDifferentShardThatOverlapsMultipleExistingRanges()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName01 = $"GalenTest_Shard01_{Guid.NewGuid():N}";
            var shardDatabaseName02 = $"GalenTest_Shard02_{Guid.NewGuid():N}";
            var shardDatabaseName03 = $"GalenTest_Shard03_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                TestUtilities.CreateDatabase(serverName, shardDatabaseName01);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName02);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName03);

                const int expectedDummyRowCount01 = 2;
                const int expectedDummyRowCount02 = 9;
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName01, expectedDummyRowCount01);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName02, expectedDummyRowCount02);

                var sut = new ShardMapManagementService(connectionString);

                const int shardRangeLow01 = 0;
                const int shardRangeHigh01 = 500;

                sut.AddRangeMapShard(rangeMapName, shardRangeLow01, shardRangeHigh01, serverName, shardDatabaseName01);

                const int shardRangeLow02 = 500;
                const int shardRangeHigh02 = 1000;

                sut.AddRangeMapShard(rangeMapName, shardRangeLow02, shardRangeHigh02, serverName, shardDatabaseName02);

                var isExceptionThrown = false;
                try
                {
                    // try to map overtop of both of the other shards (a supershard, if you will) - this should throw an exception
                    sut.AddRangeMapShard(rangeMapName, shardRangeLow01, shardRangeHigh02, serverName, shardDatabaseName03);
                }
                catch (Exception ex)
                {
                    isExceptionThrown = ex.Message.Contains("already mapped");
                }
                Assert.IsTrue(isExceptionThrown);

                // didn't use ExpectedException attribute because want to verify that 
                // the existing mapping was not altered
                var actualShardLocation01 = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow01,
                    shardRangeHigh01);

                Assert.IsNotNull(actualShardLocation01);
                Assert.AreEqual(serverName, actualShardLocation01.Server);
                Assert.AreEqual(serverName, actualShardLocation01.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation01.Database);

                var randomKeyInRange01 = new Random().Next(shardRangeLow01, shardRangeHigh01);
                var actualDummyRowCount01 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange01,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount01.HasValue);
                Assert.AreEqual(expectedDummyRowCount01, actualDummyRowCount01.Value);

                var actualShardLocation02 = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow02,
                    shardRangeHigh02);

                Assert.IsNotNull(actualShardLocation02);
                Assert.AreEqual(serverName, actualShardLocation02.Server);
                Assert.AreEqual(serverName, actualShardLocation02.DataSource);
                Assert.AreEqual(shardDatabaseName02, actualShardLocation02.Database);

                var randomKeyInRange02 = new Random().Next(shardRangeLow02, shardRangeHigh02);
                var actualDummyRowCount02 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange02,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount02.HasValue);
                Assert.AreEqual(expectedDummyRowCount02, actualDummyRowCount02.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName01);
                TestUtilities.DropDatabase(serverName, shardDatabaseName02);
                TestUtilities.DropDatabase(serverName, shardDatabaseName03);
            }
        }

        [TestMethod]
        public void DoesNotChangeMappingWhenAddingDifferentInfiniteShardThatOverlapsMultipleExistingRanges()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName01 = $"GalenTest_Shard01_{Guid.NewGuid():N}";
            var shardDatabaseName02 = $"GalenTest_Shard02_{Guid.NewGuid():N}";
            var shardDatabaseName03 = $"GalenTest_Shard03_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                TestUtilities.CreateDatabase(serverName, shardDatabaseName01);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName02);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName03);

                const int expectedDummyRowCount01 = 2;
                const int expectedDummyRowCount02 = 9;
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName01, expectedDummyRowCount01);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName02, expectedDummyRowCount02);

                var sut = new ShardMapManagementService(connectionString);

                const int shardRangeLow01 = 0;
                const int shardRangeHigh01 = 500;

                sut.AddRangeMapShard(rangeMapName, shardRangeLow01, shardRangeHigh01, serverName, shardDatabaseName01);

                const int shardRangeLow02 = 500;
                const int shardRangeHigh02 = 1000;

                sut.AddRangeMapShard(rangeMapName, shardRangeLow02, shardRangeHigh02, serverName, shardDatabaseName02);

                var isExceptionThrown = false;
                try
                {
                    // try to map overtop of both of the other shards with an infinite range - this should throw an exception
                    sut.AddRangeMapShard(rangeMapName, -100, serverName, shardDatabaseName03);
                }
                catch (Exception ex)
                {
                    isExceptionThrown = ex.Message.Contains("already mapped");
                }
                Assert.IsTrue(isExceptionThrown);

                // didn't use ExpectedException attribute because want to verify that 
                // the existing mapping was not altered
                var actualShardLocation01 = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow01,
                    shardRangeHigh01);

                Assert.IsNotNull(actualShardLocation01);
                Assert.AreEqual(serverName, actualShardLocation01.Server);
                Assert.AreEqual(serverName, actualShardLocation01.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation01.Database);

                var randomKeyInRange01 = new Random().Next(shardRangeLow01, shardRangeHigh01);
                var actualDummyRowCount01 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange01,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount01.HasValue);
                Assert.AreEqual(expectedDummyRowCount01, actualDummyRowCount01.Value);

                var actualShardLocation02 = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow02,
                    shardRangeHigh02);

                Assert.IsNotNull(actualShardLocation02);
                Assert.AreEqual(serverName, actualShardLocation02.Server);
                Assert.AreEqual(serverName, actualShardLocation02.DataSource);
                Assert.AreEqual(shardDatabaseName02, actualShardLocation02.Database);

                var randomKeyInRange02 = new Random().Next(shardRangeLow02, shardRangeHigh02);
                var actualDummyRowCount02 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange02,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount02.HasValue);
                Assert.AreEqual(expectedDummyRowCount02, actualDummyRowCount02.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName01);
                TestUtilities.DropDatabase(serverName, shardDatabaseName02);
                TestUtilities.DropDatabase(serverName, shardDatabaseName03);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesShardAndMappingWhenAddingBoundedRangeShardMap()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                const int shardRangeLow = 0;
                const int shardRangeHigh = 1000;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, shardRangeHigh, serverName, shardDatabaseName);

                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow,
                    shardRangeHigh);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                var randomKeyInRange = new Random().Next(shardRangeLow, shardRangeHigh);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesSingleShardAndMappingWhenAddingRangeShardMapForEntireInt32Range()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, int.MinValue, serverName, shardDatabaseName);

                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    int.MinValue);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                // verify int.MinValue
                var actualDummyRowCountMin = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    int.MinValue,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountMin.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCountMin.Value);

                // verify int.MaxValue
                var actualDummyRowCountMax = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    int.MaxValue,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountMax.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCountMax.Value);

                // also verify the range with a random key
                var randomKeyInRange = new Random().Next(int.MinValue + 1, int.MaxValue);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesShardAndMappingWhenAddingInfiniteRangeShardMap()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                const int shardRangeLow = 0;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, serverName, shardDatabaseName);

                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                // make sure the range is "infinite" (ie: inclusive of int.MaxValue)
                var actualDummyRowCountMax = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    int.MaxValue,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountMax.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCountMax.Value);

                // also verify the range with a random key
                var randomKeyInRange = new Random().Next(shardRangeLow, int.MaxValue);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesMappingForExistingShardWhenAddingListShardMap()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                const int expectedDummyRowCount = 2;
                var listMapName = $"TestListShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateListShardMap<Guid>(connectionString, listMapName);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);
                TestUtilities.CreateShardInListMap<Guid>(
                    connectionString, 
                    listMapName, 
                    serverName,
                    shardDatabaseName);

                var shardKey = Guid.NewGuid();

                var sut = new ShardMapManagementService(connectionString);
                sut.AddListMapShard(listMapName, shardKey, serverName, shardDatabaseName);

                var actualShardLocation = TestUtilities.GetShardLocationFromListMap(
                    connectionString,
                    listMapName,
                    shardKey);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingListMapShardConnection(
                    connectionString,
                    listMapName,
                    shardKey,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesMappingForExistingShardWhenAddingRangeShardMap()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                const int expectedDummyRowCount = 2;
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);
                TestUtilities.CreateShardInRangeMap<int>(
                    connectionString,
                    rangeMapName,
                    serverName,
                    shardDatabaseName);

                const int shardRangeLow = 0;
                const int shardRangeHigh = 1000;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, shardRangeHigh, serverName, shardDatabaseName);

                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow,
                    shardRangeHigh);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                var randomKeyInRange = new Random().Next(shardRangeLow, shardRangeHigh);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesListShardMappingEndToEnd()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                const int expectedDummyRowCount = 2;

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                var listMapName = $"TestListShardMap_{Guid.NewGuid():N}";
                var shardKey = Guid.NewGuid();

                var sut = new ShardMapManagementService(connectionString);
                sut.Deploy();
                sut.CreateListShardMap<Guid>(listMapName);
                sut.AddListMapShard(listMapName, shardKey, serverName, shardDatabaseName);

                var actualShardLocation = TestUtilities.GetShardLocationFromListMap(
                    connectionString,
                    listMapName,
                    shardKey);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingListMapShardConnection(
                    connectionString,
                    listMapName, 
                    shardKey, 
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesBoundedRangeShardMappingEndToEnd()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                const int expectedDummyRowCount = 2;

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";
                const int shardRangeLow = 0;
                const int shardRangeHigh = 1000;

                var sut = new ShardMapManagementService(connectionString);
                sut.Deploy();
                sut.CreateRangeShardMap<int>(rangeMapName);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, shardRangeHigh, serverName, shardDatabaseName);

                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow,
                    shardRangeHigh);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                var randomKeyInRange = new Random().Next(shardRangeLow, shardRangeHigh);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesInfiniteRangeShardMappingEndToEnd()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                const int expectedDummyRowCount = 2;

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";
                const int shardRangeLow = 0;

                var sut = new ShardMapManagementService(connectionString);
                sut.Deploy();
                sut.CreateRangeShardMap<int>(rangeMapName);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, serverName, shardDatabaseName);

                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                // make sure the range is "infinite" (ie: inclusive of int.MaxValue)
                var actualDummyRowCountMax = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    int.MaxValue,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountMax.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCountMax.Value);

                // also verify the range with a random key
                var randomKeyInRange = new Random().Next(shardRangeLow, int.MaxValue);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesRangeShardMappingsEndToEndUsingInt32RangeGenerator()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName01 = $"GalenTest_Shard01_{Guid.NewGuid():N}";
            var shardDatabaseName02 = $"GalenTest_Shard02_{Guid.NewGuid():N}";
            var shardDatabaseName03 = $"GalenTest_Shard03_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                const int expectedDummyRowCount01 = 2;
                const int expectedDummyRowCount02 = 9;
                const int expectedDummyRowCount03 = 14;

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName01);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName02);
                TestUtilities.CreateDatabase(serverName, shardDatabaseName03);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName01, expectedDummyRowCount01);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName02, expectedDummyRowCount02);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName03, expectedDummyRowCount03);

                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";
                var ranges = Int32RangeGenerator.GetRanges(3);
                Assert.IsNotNull(ranges);
                Assert.AreEqual(3, ranges.Count);
                Assert.AreEqual(int.MinValue, ranges[1].LowValue);
                Assert.AreEqual(int.MaxValue, ranges[3].HighValue);     // this is important

                var sut = new ShardMapManagementService(connectionString);
                sut.Deploy();
                sut.CreateRangeShardMap<int>(rangeMapName);
                sut.AddRangeMapShard(rangeMapName, ranges[1].LowValue, ranges[1].HighValue, serverName, shardDatabaseName01);
                sut.AddRangeMapShard(rangeMapName, ranges[2].LowValue, ranges[2].HighValue, serverName, shardDatabaseName02);

                // add the last range as LowValue to +infinity so that we get int.MaxValue in the range as well
                // (we already verified above that ranges[3].HighValue == int.MaxValue)
                sut.AddRangeMapShard(rangeMapName, ranges[3].LowValue, serverName, shardDatabaseName03);

                var actualShardLocation01 = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    ranges[1].LowValue,
                    ranges[1].HighValue);

                Assert.IsNotNull(actualShardLocation01);
                Assert.AreEqual(serverName, actualShardLocation01.Server);
                Assert.AreEqual(serverName, actualShardLocation01.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation01.Database);

                var actualShardLocation02 = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    ranges[2].LowValue,
                    ranges[2].HighValue);

                Assert.IsNotNull(actualShardLocation02);
                Assert.AreEqual(serverName, actualShardLocation02.Server);
                Assert.AreEqual(serverName, actualShardLocation02.DataSource);
                Assert.AreEqual(shardDatabaseName02, actualShardLocation02.Database);

                var actualShardLocation03 = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    ranges[3].LowValue,
                    ranges[3].HighValue);

                Assert.IsNotNull(actualShardLocation03);
                Assert.AreEqual(serverName, actualShardLocation03.Server);
                Assert.AreEqual(serverName, actualShardLocation03.DataSource);
                Assert.AreEqual(shardDatabaseName03, actualShardLocation03.Database);

                // make sure the boundary keys all map correctly
                var actualDummyRowCountForMin01 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    ranges[1].LowValue,     // ie int.MinValue
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountForMin01.HasValue);
                Assert.AreEqual(expectedDummyRowCount01, actualDummyRowCountForMin01.Value);

                var actualDummyRowCountForMax01 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    ranges[1].HighValue - 1,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountForMax01.HasValue);
                Assert.AreEqual(expectedDummyRowCount01, actualDummyRowCountForMax01.Value);

                Assert.AreEqual(ranges[1].HighValue, ranges[2].LowValue);

                var actualDummyRowCountForMin02 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    ranges[2].LowValue,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountForMin02.HasValue);
                Assert.AreEqual(expectedDummyRowCount02, actualDummyRowCountForMin02.Value);

                var actualDummyRowCountForMax02 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    ranges[2].HighValue - 1,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountForMax02.HasValue);
                Assert.AreEqual(expectedDummyRowCount02, actualDummyRowCountForMax02.Value);

                Assert.AreEqual(ranges[2].HighValue, ranges[3].LowValue);

                var actualDummyRowCountForMin03 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    ranges[3].LowValue,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountForMin03.HasValue);
                Assert.AreEqual(expectedDummyRowCount03, actualDummyRowCountForMin03.Value);

                var actualDummyRowCountForMax03 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    int.MaxValue,   // ie ranges[3].HighValue; should be included in range because we used the +infinity range above
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountForMax03.HasValue);
                Assert.AreEqual(expectedDummyRowCount03, actualDummyRowCountForMax03.Value);

                // check some random keys in each range
                var randomKeyInRange01 = new Random().Next(ranges[1].LowValue, ranges[1].HighValue);
                var actualDummyRowCountForRandom01 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange01,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountForRandom01.HasValue);
                Assert.AreEqual(expectedDummyRowCount01, actualDummyRowCountForRandom01.Value);

                var randomKeyInRange02 = new Random().Next(ranges[2].LowValue, ranges[2].HighValue);
                var actualDummyRowCountForRandom02 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange02,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountForRandom02.HasValue);
                Assert.AreEqual(expectedDummyRowCount02, actualDummyRowCountForRandom02.Value);

                var randomKeyInRange03 = new Random().Next(ranges[3].LowValue, ranges[3].HighValue);
                var actualDummyRowCountForRandom03 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange03,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCountForRandom03.HasValue);
                Assert.AreEqual(expectedDummyRowCount03, actualDummyRowCountForRandom03.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName01);
                TestUtilities.DropDatabase(serverName, shardDatabaseName02);
                TestUtilities.DropDatabase(serverName, shardDatabaseName03);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesTwoShardsInListShardMap()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName01 = $"GalenTest_Shard01_{Guid.NewGuid():N}";
            var shardDatabaseName02 = $"GalenTest_Shard02_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var listMapName = $"TestListShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateListShardMap<Guid>(connectionString, listMapName);

                const int expectedDummyRowCountShard01 = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName01);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName01, expectedDummyRowCountShard01);

                const int expectedDummyRowCountShard02 = 9;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName02);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName02, expectedDummyRowCountShard02);

                var shardKey01 = Guid.NewGuid();
                var shardKey02 = Guid.NewGuid();

                var sut = new ShardMapManagementService(connectionString);
                sut.AddListMapShard(listMapName, shardKey01, serverName, shardDatabaseName01);
                sut.AddListMapShard(listMapName, shardKey02, serverName, shardDatabaseName02);

                var actualShardLocation01 = TestUtilities.GetShardLocationFromListMap(
                    connectionString,
                    listMapName,
                    shardKey01);

                Assert.IsNotNull(actualShardLocation01);
                Assert.AreEqual(serverName, actualShardLocation01.Server);
                Assert.AreEqual(serverName, actualShardLocation01.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation01.Database);

                var actualShardLocation02 = TestUtilities.GetShardLocationFromListMap(
                    connectionString,
                    listMapName,
                    shardKey02);

                Assert.IsNotNull(actualShardLocation02);
                Assert.AreEqual(serverName, actualShardLocation02.Server);
                Assert.AreEqual(serverName, actualShardLocation02.DataSource);
                Assert.AreEqual(shardDatabaseName02, actualShardLocation02.Database);

                var actualDummyRowCount01 = TestUtilities.GetDummyRowCountUsingListMapShardConnection(
                    connectionString,
                    listMapName,
                    shardKey01,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount01.HasValue);
                Assert.AreEqual(expectedDummyRowCountShard01, actualDummyRowCount01.Value);

                var actualDummyRowCount02 = TestUtilities.GetDummyRowCountUsingListMapShardConnection(
                    connectionString,
                    listMapName,
                    shardKey02,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount02.HasValue);
                Assert.AreEqual(expectedDummyRowCountShard02, actualDummyRowCount02.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName01);
                TestUtilities.DropDatabase(serverName, shardDatabaseName02);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesTwoShardsInRangeShardMap()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName01 = $"GalenTest_Shard01_{Guid.NewGuid():N}";
            var shardDatabaseName02 = $"GalenTest_Shard02_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCountShard01 = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName01);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName01, expectedDummyRowCountShard01);

                const int expectedDummyRowCountShard02 = 9;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName02);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName02, expectedDummyRowCountShard02);


                const int shardRangeLow01 = 0;
                const int shardRangeHigh01 = 1000;

                const int shardRangeLow02 = shardRangeHigh01;
                const int shardRangeHigh02 = 2000;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow01, shardRangeHigh01, serverName, shardDatabaseName01);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow02, shardRangeHigh02, serverName, shardDatabaseName02);

                var actualShardLocation01 = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow01,
                    shardRangeHigh01);

                Assert.IsNotNull(actualShardLocation01);
                Assert.AreEqual(serverName, actualShardLocation01.Server);
                Assert.AreEqual(serverName, actualShardLocation01.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation01.Database);

                var actualShardLocation02 = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow02,
                    shardRangeHigh02);

                Assert.IsNotNull(actualShardLocation02);
                Assert.AreEqual(serverName, actualShardLocation02.Server);
                Assert.AreEqual(serverName, actualShardLocation02.DataSource);
                Assert.AreEqual(shardDatabaseName02, actualShardLocation02.Database);

                var randomKeyInRange01 = new Random().Next(shardRangeLow01, shardRangeHigh01);
                var actualDummyRowCount01 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange01,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount01.HasValue);
                Assert.AreEqual(expectedDummyRowCountShard01, actualDummyRowCount01.Value);

                var randomKeyInRange02 = new Random().Next(shardRangeLow02, shardRangeHigh02);
                var actualDummyRowCount02 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange02,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount02.HasValue);
                Assert.AreEqual(expectedDummyRowCountShard02, actualDummyRowCount02.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName01);
                TestUtilities.DropDatabase(serverName, shardDatabaseName02);
            }
        }

        [TestMethod]
        public void CorrectlyCreatesMapsTwoNonOverlappingRangesToSameShardInRangeShardMap()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName01 = $"GalenTest_Shard01_{Guid.NewGuid():N}";
            var shardDatabaseName02 = $"GalenTest_Shard02_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCountShard01 = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName01);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName01, expectedDummyRowCountShard01);

                const int expectedDummyRowCountShard02 = 9;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName02);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName02, expectedDummyRowCountShard02);


                const int shardRangeLow01 = 0;
                const int shardRangeHigh01 = 1000;

                const int shardRangeLow02 = shardRangeHigh01;
                const int shardRangeHigh02 = 2000;

                const int shardRangeLow03 = shardRangeHigh02;
                const int shardRangeHigh03 = 3000;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow01, shardRangeHigh01, serverName, shardDatabaseName01);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow02, shardRangeHigh02, serverName, shardDatabaseName02);

                // map the third shard range to the first server/database, so it gets range 01 and range 03
                sut.AddRangeMapShard(rangeMapName, shardRangeLow03, shardRangeHigh03, serverName, shardDatabaseName01);

                var actualShardLocation01 = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow01,
                    shardRangeHigh01);

                Assert.IsNotNull(actualShardLocation01);
                Assert.AreEqual(serverName, actualShardLocation01.Server);
                Assert.AreEqual(serverName, actualShardLocation01.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation01.Database);

                var actualShardLocation02 = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow02,
                    shardRangeHigh02);

                Assert.IsNotNull(actualShardLocation02);
                Assert.AreEqual(serverName, actualShardLocation02.Server);
                Assert.AreEqual(serverName, actualShardLocation02.DataSource);
                Assert.AreEqual(shardDatabaseName02, actualShardLocation02.Database);

                var actualShardLocation03 = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow03,
                    shardRangeHigh03);

                Assert.IsNotNull(actualShardLocation03);
                Assert.AreEqual(serverName, actualShardLocation03.Server);
                Assert.AreEqual(serverName, actualShardLocation03.DataSource);
                Assert.AreEqual(shardDatabaseName01, actualShardLocation03.Database);

                var randomKeyInRange01 = new Random().Next(shardRangeLow01, shardRangeHigh01);
                var actualDummyRowCount01 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange01,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount01.HasValue);
                Assert.AreEqual(expectedDummyRowCountShard01, actualDummyRowCount01.Value);

                var randomKeyInRange02 = new Random().Next(shardRangeLow02, shardRangeHigh02);
                var actualDummyRowCount02 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange02,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount02.HasValue);
                Assert.AreEqual(expectedDummyRowCountShard02, actualDummyRowCount02.Value);

                var randomKeyInRange03 = new Random().Next(shardRangeLow03, shardRangeHigh03);
                var actualDummyRowCount03 = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange03,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount03.HasValue);
                Assert.AreEqual(expectedDummyRowCountShard01, actualDummyRowCount03.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName01);
                TestUtilities.DropDatabase(serverName, shardDatabaseName02);
            }
        }

        [TestMethod]
        public void DoesNotChangeMappingWhenTryingToShrinkExistingShardRange()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard01_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                const int shardRangeLow = 0;
                const int shardRangeHigh = 1000;
                const int shardRangeShrinkHigh = 500;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, shardRangeHigh, serverName, shardDatabaseName);

                var isExceptionThrown = false;
                try
                {
                    // try to shrink the existing range (same shard and range is in the same mapping)
                    // but we don't support trying to change the range so this should throw an exception
                    sut.AddRangeMapShard(rangeMapName, shardRangeLow, shardRangeShrinkHigh, serverName, shardDatabaseName);
                }
                catch (Exception ex)
                {
                    isExceptionThrown = ex.Message.Contains("Changing an existing shard mapping range is not supported");
                }
                Assert.IsTrue(isExceptionThrown);

                // didn't use ExpectedException attribute because want to verify that 
                // the existing mapping was not altered
                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow,
                    shardRangeHigh);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                // generate random key in the range we tried to eliminate with the failed attempt at shrinking
                var randomKeyInRange = new Random().Next(shardRangeShrinkHigh, shardRangeHigh);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void DoesNotChangeMappingWhenTryingToExpandExistingShardRange()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard01_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                const int shardRangeLow = 0;
                const int shardRangeHigh = 1000;
                const int shardRangeExpandHigh = 2000;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, shardRangeLow, shardRangeHigh, serverName, shardDatabaseName);

                var isExceptionThrown = false;
                try
                {
                    // try to expand the existing range (same shard and range is in the same mapping)
                    // but we don't support trying to change the range so this should throw an exception
                    sut.AddRangeMapShard(rangeMapName, shardRangeLow, shardRangeExpandHigh, serverName, shardDatabaseName);
                }
                catch (Exception ex)
                {
                    isExceptionThrown = ex.Message.Contains("Changing an existing shard mapping range is not supported");
                }
                Assert.IsTrue(isExceptionThrown);

                // didn't use ExpectedException attribute because want to verify that 
                // the existing mapping was not altered
                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    shardRangeLow,
                    shardRangeHigh);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                var randomKeyInRange = new Random().Next(shardRangeLow, shardRangeHigh);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }

        [TestMethod]
        public void DoesNotChangeMappingWhenTryingToShiftExistingShardRange()
        {
            const string applicationName = "Galen.CI.Azure.Sql.Sharding.Tests";
            const string serverName = @"(localdb)\mssqllocaldb";
            var managerDatabaseName = $"GalenTest_Manager_{Guid.NewGuid():N}";
            var shardDatabaseName = $"GalenTest_Shard01_{Guid.NewGuid():N}";

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = managerDatabaseName,
                IntegratedSecurity = true,
                ApplicationName = applicationName
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            try
            {
                var rangeMapName = $"TestRangeShardMap_{Guid.NewGuid():N}";

                TestUtilities.CreateDatabase(serverName, managerDatabaseName);
                TestUtilities.InitializeShardMapManager(connectionString);
                TestUtilities.CreateRangeShardMap<int>(connectionString, rangeMapName);

                const int expectedDummyRowCount = 2;
                TestUtilities.CreateDatabase(serverName, shardDatabaseName);
                TestUtilities.CreateDummyTableWithNRows(serverName, shardDatabaseName, expectedDummyRowCount);

                const int originalShardRangeLow = 0;
                const int originalShardRangeHigh = 1000;
                const int shiftedShardRangeLow = 500;
                const int shiftedShardRangeHigh = 1500;

                var sut = new ShardMapManagementService(connectionString);
                sut.AddRangeMapShard(rangeMapName, originalShardRangeLow, originalShardRangeHigh, serverName, shardDatabaseName);

                var isExceptionThrown = false;
                try
                {
                    // try to shift the existing range (same shard and range is in the same mapping)
                    // but we don't support trying to change the range so this should throw an exception
                    sut.AddRangeMapShard(rangeMapName, shiftedShardRangeLow, shiftedShardRangeHigh, serverName, shardDatabaseName);
                }
                catch (Exception ex)
                {
                    isExceptionThrown = ex.Message.Contains("Changing an existing shard mapping range is not supported");
                }
                Assert.IsTrue(isExceptionThrown);

                // didn't use ExpectedException attribute because want to verify that 
                // the existing mapping was not altered
                var actualShardLocation = TestUtilities.GetShardLocationFromRangeMap(
                    connectionString,
                    rangeMapName,
                    originalShardRangeLow,
                    originalShardRangeHigh);

                Assert.IsNotNull(actualShardLocation);
                Assert.AreEqual(serverName, actualShardLocation.Server);
                Assert.AreEqual(serverName, actualShardLocation.DataSource);
                Assert.AreEqual(shardDatabaseName, actualShardLocation.Database);

                // generate random key in the range that falls within where the failed shift would have occurred
                var randomKeyInRange = new Random().Next(shiftedShardRangeLow, originalShardRangeHigh);
                var actualDummyRowCount = TestUtilities.GetDummyRowCountUsingRangeMapShardConnection(
                    connectionString,
                    rangeMapName,
                    randomKeyInRange,
                    $"Integrated Security=true; Application Name={applicationName}");
                Assert.IsTrue(actualDummyRowCount.HasValue);
                Assert.AreEqual(expectedDummyRowCount, actualDummyRowCount.Value);
            }
            finally
            {
                TestUtilities.DropDatabase(serverName, managerDatabaseName);
                TestUtilities.DropDatabase(serverName, shardDatabaseName);
            }
        }
    }
}